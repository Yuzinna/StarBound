using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class GravityObjectLogic : MonoBehaviour
{
	private Rigidbody2D _rb;
	private Collider2D _col;

	[Header("기본 중력")]
	[Tooltip("노말 상태 중력 스케일은 GravityManager.normalGravityScale을 사용")]
	[SerializeField] private float normalDrag = 0f;
	[SerializeField] private PhysicsMaterial2D normalMaterial;

	[Header("Floating 설정")]
	[Tooltip("부유 상태로 올라갈 때 속도")]
	[SerializeField] private float ascendSpeed = 2f;
	[Tooltip("부유 상태에서 drag")]
	[SerializeField] private float floatingDrag = 2f;
	[SerializeField] private PhysicsMaterial2D floatingMaterial;

	[Header("Floating Gap/Surface")]
	[Tooltip("바닥/천장/다른 큐브와 띄울 거리")]
	[SerializeField] private float surfaceGap = 0.05f;
	[Tooltip("표면(바닥/천장/다른 큐브) 찾는 Ray 거리")]
	[SerializeField] private float surfaceRayDistance = 5f;
	[Tooltip("착륙 표면으로 인정할 레이어 (바닥/천장/다른 큐브 포함)")]
	[SerializeField] private LayerMask standableLayers;

	[Header("Floating Wave (선택)")]
	[SerializeField] private bool useWave = true;
	[SerializeField] private float waveAmplitude = 0.1f;
	[SerializeField] private float waveFrequency = 1f;

	// 상태
	private bool _isFloating = false;
	private eGravityDirection _currentDirection = eGravityDirection.Normal;

	private enum FloatingState { None, Ascending, Waving }
	private FloatingState _floatingState = FloatingState.None;

	// 부유 중심(표면에서 gap 띄운 목표 위치)
	private Vector2 _floatCenter;
	private float _waveTime;
	private float _waveOffset;

	// 플레이어가 옆에서 미는 중인지
	private bool _isBeingPushedSideways = false;

	// 디버그용 레이 시작 위치
	private Vector2 _rayOrigin;

	[Header("SFX - Landing")]
	[SerializeField] private AudioClip landThudClip;
	[SerializeField, Range(0f, 1f)] private float landVolume = 0.8f;
	[SerializeField] private float landMinImpactSpeed = 1.2f; // 너무 살살 닿을 땐 소리 X
	[SerializeField] private float landOffset = 0.6f; // 필요하면

	private bool _wasAirborne = false;

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

		// GravityManager 값 변화 감지
		var newDir = GravityManager.Instance.CurrentDirection;
		var newFloating = GravityManager.Instance.IsFloatingEnabled;

		if (newDir != _currentDirection || newFloating != _isFloating)
		{
			_currentDirection = newDir;
			_isFloating = newFloating;
			ApplyGravityAndFloatingState(_currentDirection, _isFloating);
		}

		// 디버그 레이
		Debug.DrawRay(_rayOrigin, GetGravityDir(_currentDirection) * surfaceRayDistance, Color.brown);

		if (!_isFloating) return;

		// Floating 단계 진행
		switch (_floatingState)
		{
			case FloatingState.Ascending:
				HandleAscending();
				break;
			case FloatingState.Waving:
				HandleWaving();
				break;
		}

		// Floating 중 X축 제약: 기본은 잠금, 옆에서 밀 때만 해제
		if (_isBeingPushedSideways)
		{
			_rb.constraints = RigidbodyConstraints2D.FreezeRotation; // X 풀림
		}
		else
		{
			_rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
		}
	}

	/// <summary>
	/// 중력 방향 + Floating 여부 적용
	/// </summary>
	public void ApplyGravityAndFloatingState(eGravityDirection direction, bool isFloating)
	{
		if (_rb == null || _col == null || GravityManager.Instance == null)
			return;

		_currentDirection = direction;
		_isFloating = isFloating;

		if (_isFloating)
		{
			EnterFloating(direction);
		}
		else
		{
			ExitFloating(direction);
		}
	}

	// ------------------------------------------------------
	// Floating ON
	// ------------------------------------------------------
	private void EnterFloating(eGravityDirection direction)
	{
		_floatingState = FloatingState.Ascending;

		// 중력 제거 + 속도 초기화
		_rb.gravityScale = 0f;
		_rb.linearVelocity = Vector2.zero;
		_rb.linearDamping = floatingDrag;

		// 부유 상태에서는 기본적으로 X 잠금(나중에 밀 때만 풀림)
		_rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;

		// 머티리얼 교체
		if (floatingMaterial != null)
			_col.sharedMaterial = floatingMaterial;

		// 표면 기준으로 gap 띄운 중심 위치 계산
		_floatCenter = ComputeFloatCenter(direction);

		_waveTime = 0f;
		_waveOffset = Random.Range(0f, 10f);
	}

	// ------------------------------------------------------
	// Floating OFF (노말 상태)
	// ------------------------------------------------------
	private void ExitFloating(eGravityDirection direction)
	{
		_floatingState = FloatingState.None;

		float g = GravityManager.Instance.normalGravityScale;

		// 중력 방향 반영
		_rb.gravityScale = (direction == eGravityDirection.Normal) ? g : -g;
		_rb.linearDamping = normalDrag;

		// 🔥 핵심: 노말 상태에서는 X축 항상 잠금 → 절대 못 미룸
		_rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;

		// 머티리얼 복구
		if (normalMaterial != null)
			_col.sharedMaterial = normalMaterial;
	}

	// ------------------------------------------------------
	// Ascending 단계: floatCenter까지 Y만 이동
	// ------------------------------------------------------
	private void HandleAscending()
	{
		Vector2 pos = _rb.position;
		float newY = Mathf.MoveTowards(pos.y, _floatCenter.y, ascendSpeed * Time.fixedDeltaTime);

		_rb.MovePosition(new Vector2(pos.x, newY));

		if (Mathf.Abs(newY - _floatCenter.y) < 0.01f)
		{
			_floatingState = FloatingState.Waving;
			_waveTime = 0f;
		}
	}

	// ------------------------------------------------------
	// Waving 단계: floatCenter 기준으로 사인 웨이브
	// ------------------------------------------------------
	private void HandleWaving()
	{
		if (!useWave) return;

		_waveTime += Time.fixedDeltaTime;

		Vector2 normal = (_currentDirection == eGravityDirection.Normal)
			? Vector2.up
			: Vector2.down;

		float wave = Mathf.Sin((_waveTime + _waveOffset) * waveFrequency * Mathf.PI * 2f) * waveAmplitude;

		Vector2 pos = _rb.position;
		float targetY = _floatCenter.y + normal.y * wave;

		_rb.MovePosition(new Vector2(pos.x, targetY));
	}

	// ------------------------------------------------------
	// 표면(바닥/천장/다른 큐브) 기준으로 gap 유지되는 floatCenter 계산
	// ------------------------------------------------------
	private Vector2 ComputeFloatCenter(eGravityDirection direction)
	{
		Vector2 gravityDir = GetGravityDir(direction);

		// 콜라이더 중심에서 Ray
		Vector2 origin = _col.bounds.center;
		_rayOrigin = origin;

		if (RaycastSurfaceExceptSelf(origin, gravityDir, surfaceRayDistance, out RaycastHit2D hit))
		{
			float halfHeight = _col.bounds.extents.y;

			// hit.point(표면)에서 중력 반대 방향으로 halfHeight + gap 만큼 떨어진 점이 콜라이더 중심
			Vector2 center = hit.point - gravityDir * (halfHeight + surfaceGap);
			return center;
		}

		// 표면 못 찾으면 현재 위치
		return _rb.position;
	}

	/// <summary>
	/// 같은 레이어의 다른 큐브/바닥/천장은 인정,
	/// '자기 자신 콜라이더'만 제외하는 Raycast
	/// </summary>
	private bool RaycastSurfaceExceptSelf(Vector2 origin, Vector2 dir, float distance, out RaycastHit2D result)
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
		return (direction == eGravityDirection.Normal) ? Vector2.down : Vector2.up;
	}

	// ------------------------------------------------------
	// 플레이어 옆면 충돌 시에만 Floating 상태에서 X 잠금 해제
	// ------------------------------------------------------
	private void OnCollisionEnter2D(Collision2D other)
	{
		CheckSideCollision(other, true);

		// Floating 중에는 착지 개념이 애매하니 스킵(원하면 바꿔도 됨)
		if (_isFloating) return;

		// standableLayers에 해당하는 것에 닿았는지 확인
		int mask = 1 << other.gameObject.layer;
		if ((standableLayers.value & mask) == 0) return;

		// 충격(상대 속도) 기준으로 너무 약하면 소리 안 냄
		float impact = other.relativeVelocity.magnitude;
		if (impact < landMinImpactSpeed) return;

		// 대표 소리 1번만 재생 요청
		if (SfxManager.Instance != null && landThudClip != null)
		{
			SfxManager.Instance.PlayThudOnce(landThudClip, landVolume, landOffset);
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

	private void CheckSideCollision(Collision2D other, bool enteringOrStaying)
	{
		// 플레이어만 체크
		if (other.gameObject.GetComponent<PlayerLogic>() == null)
			return;

		bool sideContact = false;
		foreach (var c in other.contacts)
		{
			if (Mathf.Abs(c.normal.x) > 0.5f && Mathf.Abs(c.normal.y) < 0.5f)
			{
				sideContact = true;
				break;
			}
		}

		_isBeingPushedSideways = enteringOrStaying && sideContact;

		// Floating 아닐 땐 무조건 못 미룸
		if (!_isFloating)
		{
			_isBeingPushedSideways = false;
		}
	}
}
