using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class GravityObjectLogic : MonoBehaviour
{
	private Rigidbody2D _rb;
	private Collider2D _col;

	[Header("Floating 설정")]
	[SerializeField] private float ascendSpeed = 2f;
	[SerializeField] private float floatingDrag = 2f;
	[SerializeField] private PhysicsMaterial2D floatingMaterial;

	[Header("Floating Gap / Surface")]
	[SerializeField] private float surfaceGap = 0.05f;
	[SerializeField] private float surfaceRayDistance = 5f;
	[SerializeField] private LayerMask standableLayers;

	[Header("Floating Wave")]
	[SerializeField] private bool useWave = true;
	[SerializeField] private float waveAmplitude = 0.1f;
	[SerializeField] private float waveFrequency = 1f;

	// 상태
	private bool _isFloating;
	private eGravityDirection _currentDirection;

	private enum FloatingState { None, Ascending, Waving }
	private FloatingState _floatingState = FloatingState.None;

	// Floating 기준
	private Vector2 _floatCenter;
	private Collider2D _surfaceCollider;          // ⭐ 기준 표면
	private Vector2 _surfaceLocalOffset;           // ⭐ 표면 기준 로컬 오프셋

	private float _waveTime;
	private float _waveOffset;

	// 플레이어 밀기
	private bool _isBeingPushedSideways;

	// 디버그
	private Vector2 _rayOrigin;

	[Header("SFX - Landing")]
	[SerializeField] private AudioClip landThudClip;
	[SerializeField, Range(0f, 1f)] private float landVolume = 0.8f;
	[SerializeField] private float landMinImpactSpeed = 1.2f;
	[SerializeField] private float landOffset = 0.6f;

	private void Awake()
	{
		_rb = GetComponent<Rigidbody2D>();
		_col = GetComponent<Collider2D>();
	}

	private void Start()
	{
		if (GravityManager.Instance == null) return;

		_currentDirection = GravityManager.Instance.CurrentDirection;
		_isFloating = GravityManager.Instance.IsFloatingEnabled;

		ApplyGravityAndFloatingState(_currentDirection, _isFloating);
	}

	private void FixedUpdate()
	{
		if (GravityManager.Instance == null) return;

		// Gravity 상태 변경 감지
		var newDir = GravityManager.Instance.CurrentDirection;
		var newFloating = GravityManager.Instance.IsFloatingEnabled;

		if (newDir != _currentDirection || newFloating != _isFloating)
		{
			ApplyGravityAndFloatingState(newDir, newFloating);
		}

		// Floating 중이면 기준 표면 기준으로 중심 갱신 ⭐
		if (_isFloating && _surfaceCollider != null)
		{
			_floatCenter =
				(Vector2)_surfaceCollider.transform.position + _surfaceLocalOffset;
		}

		Debug.DrawRay(_rayOrigin, GetGravityDir(_currentDirection) * surfaceRayDistance, Color.cyan);

		if (!_isFloating) return;

		switch (_floatingState)
		{
			case FloatingState.Ascending:
				HandleAscending();
				break;

			case FloatingState.Waving:
				HandleWaving();
				break;
		}

		// X축 제약
		if (_isBeingPushedSideways)
			_rb.constraints = RigidbodyConstraints2D.FreezeRotation;
		else
			_rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
	}

	// --------------------------------------------------
	// Gravity / Floating 전환
	// --------------------------------------------------
	public void ApplyGravityAndFloatingState(eGravityDirection direction, bool isFloating)
	{
		_currentDirection = direction;
		_isFloating = isFloating;

		if (_isFloating)
			EnterFloating(direction);
		else
			ExitFloating(direction);
	}

	// --------------------------------------------------
	// Floating ON
	// --------------------------------------------------
	private void EnterFloating(eGravityDirection direction)
	{
		_floatingState = FloatingState.Ascending;

		_rb.gravityScale = 0f;
		_rb.linearVelocity = Vector2.zero;
		_rb.linearDamping = floatingDrag;

		_rb.constraints =
			RigidbodyConstraints2D.FreezePositionX |
			RigidbodyConstraints2D.FreezeRotation;

		_floatCenter = ComputeFloatCenterAndCacheSurface(direction);

		_waveTime = 0f;
		_waveOffset = Random.Range(0f, 10f);
	}

	// --------------------------------------------------
	// Floating OFF
	// --------------------------------------------------
	private void ExitFloating(eGravityDirection direction)
	{
		_floatingState = FloatingState.None;
		_surfaceCollider = null;

		float g = GravityManager.Instance.normalGravityScale;
		_rb.gravityScale = (direction == eGravityDirection.Normal) ? g : -g;

		_rb.constraints =
			RigidbodyConstraints2D.FreezePositionX |
			RigidbodyConstraints2D.FreezeRotation;
	}

	// --------------------------------------------------
	// Ascending
	// --------------------------------------------------
	private void HandleAscending()
	{
		Vector2 pos = _rb.position;
		float newY = Mathf.MoveTowards(
			pos.y,
			_floatCenter.y,
			ascendSpeed * Time.fixedDeltaTime
		);

		_rb.MovePosition(new Vector2(pos.x, newY));

		if (Mathf.Abs(newY - _floatCenter.y) < 0.01f)
		{
			_floatingState = FloatingState.Waving;
			_waveTime = 0f;
		}
	}

	// --------------------------------------------------
	// Waving
	// --------------------------------------------------
	private void HandleWaving()
	{
		if (!useWave) return;

		_waveTime += Time.fixedDeltaTime;

		Vector2 normal =
			(_currentDirection == eGravityDirection.Normal)
			? Vector2.up
			: Vector2.down;

		float wave =
			Mathf.Sin((_waveTime + _waveOffset) * waveFrequency * Mathf.PI * 2f)
			* waveAmplitude;

		Vector2 pos = _rb.position;
		float targetY = _floatCenter.y + normal.y * wave;

		_rb.MovePosition(new Vector2(pos.x, targetY));
	}

	// --------------------------------------------------
	// 기준 표면 + 로컬 오프셋 계산 ⭐
	// --------------------------------------------------
	private Vector2 ComputeFloatCenterAndCacheSurface(eGravityDirection direction)
	{
		Vector2 gravityDir = GetGravityDir(direction);

		Vector2 origin = _col.bounds.center;
		_rayOrigin = origin;

		if (RaycastSurfaceExceptSelf(origin, gravityDir, surfaceRayDistance, out RaycastHit2D hit))
		{
			_surfaceCollider = hit.collider;

			float halfHeight = _col.bounds.extents.y;

			Vector2 center =
				hit.point - gravityDir * (halfHeight + surfaceGap);

			_surfaceLocalOffset =
				center - (Vector2)_surfaceCollider.transform.position;

			return center;
		}

		_surfaceCollider = null;
		return _rb.position;
	}

	private bool RaycastSurfaceExceptSelf(
		Vector2 origin,
		Vector2 dir,
		float distance,
		out RaycastHit2D result)
	{
		var hits = Physics2D.RaycastAll(origin, dir, distance, standableLayers);

		foreach (var h in hits)
		{
			if (h.collider != null && h.collider != _col)
			{
				result = h;
				return true;
			}
		}

		result = default;
		return false;
	}

	private Vector2 GetGravityDir(eGravityDirection direction)
	{
		return (direction == eGravityDirection.Normal)
			? Vector2.down
			: Vector2.up;
	}

	// --------------------------------------------------
	// 플레이어 밀기 판정
	// --------------------------------------------------
	private void OnCollisionEnter2D(Collision2D other)
	{
		CheckSideCollision(other, true);

		if (_isFloating) return;

		int mask = 1 << other.gameObject.layer;
		if ((standableLayers.value & mask) == 0) return;

		float impact = other.relativeVelocity.magnitude;
		if (impact < landMinImpactSpeed) return;

		if (SfxManager.Instance != null && landThudClip != null)
		{
			SfxManager.Instance.PlayThudOnce(
				landThudClip,
				landVolume,
				landOffset
			);
		}
	}

	private void OnCollisionStay2D(Collision2D other)
	{
		CheckSideCollision(other, true);
	}

	private void OnCollisionExit2D(Collision2D other)
	{
		CheckSideCollision(other, false);
	}

	private void CheckSideCollision(Collision2D other, bool active)
	{
		if (other.gameObject.GetComponent<PlayerLogic>() == null)
			return;

		bool sideContact = false;
		foreach (var c in other.contacts)
		{
			if (Mathf.Abs(c.normal.x) > 0.5f &&
				Mathf.Abs(c.normal.y) < 0.5f)
			{
				sideContact = true;
				break;
			}
		}

		_isBeingPushedSideways = active && sideContact;

		if (!_isFloating)
			_isBeingPushedSideways = false;
	}
}