using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class GravityObjectLogic : MonoBehaviour
{
	private Rigidbody2D _rb;
	private Collider2D _col;

	// ==========================================
	// 💡 [새로 추가된 특수 기믹 설정!]
	// ==========================================
	[Header("특수 큐브 설정")]
	[Tooltip("체크하면 중력 반전(Inverse)을 무시하고 항상 아래로만 떨어집니다. (부유 상태는 정상 작동)")]
	[SerializeField] private bool ignoreInverseGravity = false;

	[Header("Floating 설정")]
	[SerializeField] private float ascendSpeed = 2f;
	[SerializeField] private float floatingDrag = 2f;

	[Header("Floating Gap / Surface")]
	[SerializeField] private float surfaceGap = 0.05f;
	[SerializeField] private float surfaceRayDistance = 5f;
	[SerializeField] private LayerMask standableLayers;

	[Header("Floating Wave")]
	[SerializeField] private bool useWave = true;
	[SerializeField] private float waveAmplitude = 0.1f;
	[SerializeField] private float waveFrequency = 1f;

	// 상태 관리
	private bool _isFloating;
	private eGravityDirection _currentDirection;
	private enum FloatingState { None, Ascending, Waving }
	private FloatingState _floatingState = FloatingState.None;

	// Floating 기준 데이터
	private Vector2 _floatCenter;
	private Collider2D _surfaceCollider;
	private Vector2 _surfaceLocalOffset;
	private float _waveTime;
	private float _waveOffset;

	// 컨베이어 벨트 관련
	private float _conveyorSpeed = 0f;

	[Header("SFX - Landing")]
	[SerializeField] private AudioClip landThudClip;
	[SerializeField, Range(0f, 1f)] private float landVolume = 0.8f;
	[SerializeField] private float landMinImpactSpeed = 1.2f;
	[SerializeField] private float landOffset = 0.6f;

	private bool _isTouchingMovingBlock = false;

	private void Awake()
	{
		_rb = GetComponent<Rigidbody2D>();
		_col = GetComponent<Collider2D>();
	}

	private void Start()
	{
		if (GravityManager.Instance == null) return;

		// 💡 큐브가 태어날 때도 무시 옵션 체크!
		eGravityDirection targetDir = GravityManager.Instance.CurrentDirection;
		if (ignoreInverseGravity) targetDir = eGravityDirection.Normal;

		_currentDirection = targetDir;
		_isFloating = GravityManager.Instance.IsFloatingEnabled;
		ApplyGravityAndFloatingState(_currentDirection, _isFloating);
	}

	private void FixedUpdate()
	{
		if (GravityManager.Instance == null) return;

		SyncGravityState();

		HandlePhysicsAndConveyor();

		if (_isFloating)
		{
			UpdateFloatingPosition();
			switch (_floatingState)
			{
				case FloatingState.Ascending: HandleAscending(); break;
				case FloatingState.Waving: HandleWaving(); break;
			}
		}
	}

	public void TeleportTo(Vector2 newPosition)
	{
		_rb.position = newPosition;
		transform.position = newPosition;
		_rb.linearVelocity = Vector2.zero;

		if (_isFloating)
		{
			Physics2D.SyncTransforms();
			_surfaceCollider = null;
			_floatCenter = ComputeFloatCenterAndCacheSurface(_currentDirection);
			_floatingState = FloatingState.Ascending;
			_waveTime = 0f;
		}
	}

	private void HandlePhysicsAndConveyor()
	{
		if (_isFloating)
		{
			_rb.constraints = RigidbodyConstraints2D.FreezeRotation;
		}
		else
		{
			if (_conveyorSpeed != 0)
			{
				_rb.constraints = RigidbodyConstraints2D.FreezeRotation;
				_rb.linearVelocity = new Vector2(_conveyorSpeed, _rb.linearVelocity.y);
			}
			else if (_isTouchingMovingBlock)
			{
				_rb.constraints = RigidbodyConstraints2D.FreezeRotation;
			}
			else
			{
				_rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
			}
		}
	}

	// --------------------------------------------------
	// 💡 [핵심 수정] 매 프레임 상태를 동기화할 때, 무시 옵션이 켜져있으면 강제로 Normal로 둔갑시킵니다!
	// --------------------------------------------------
	private void SyncGravityState()
	{
		eGravityDirection targetDir = GravityManager.Instance.CurrentDirection;
		bool targetFloating = GravityManager.Instance.IsFloatingEnabled;

		// 반전 무시 큐브라면, 매니저가 Inverse를 외쳐도 귀를 막고 Normal로 취급합니다.
		if (ignoreInverseGravity)
		{
			targetDir = eGravityDirection.Normal;
		}

		if (targetDir != _currentDirection || targetFloating != _isFloating)
		{
			ApplyGravityAndFloatingState(targetDir, targetFloating);
		}
	}

	public void ApplyGravityAndFloatingState(eGravityDirection direction, bool isFloating)
	{
		// (안전장치) 외부에서 이 함수를 직접 부를 때도 무시 옵션을 적용
		if (ignoreInverseGravity) direction = eGravityDirection.Normal;

		_currentDirection = direction;
		_isFloating = isFloating;

		if (_isFloating)
		{
			_floatingState = FloatingState.Ascending;
			_rb.gravityScale = 0f;
			_rb.linearVelocity = Vector2.zero;
			_rb.linearDamping = floatingDrag;
			_floatCenter = ComputeFloatCenterAndCacheSurface(direction);
			_waveTime = 0f;
			_waveOffset = Random.Range(0f, 10f);
		}
		else
		{
			_floatingState = FloatingState.None;
			_surfaceCollider = null;
			float g = GravityManager.Instance.normalGravityScale;
			_rb.gravityScale = (direction == eGravityDirection.Normal) ? g : -g;
			_rb.linearDamping = 0f;
		}
	}

	private void UpdateFloatingPosition()
	{
		if (_surfaceCollider != null)
		{
			_floatCenter = (Vector2)_surfaceCollider.transform.position + _surfaceLocalOffset;
		}
	}

	private void HandleAscending()
	{
		Vector2 pos = _rb.position;
		float newY = Mathf.MoveTowards(pos.y, _floatCenter.y, ascendSpeed * Time.fixedDeltaTime);
		_rb.MovePosition(new Vector2(pos.x, newY));

		if (Mathf.Abs(newY - _floatCenter.y) < 0.01f)
		{
			_floatingState = FloatingState.Waving;
		}
	}

	private void HandleWaving()
	{
		if (!useWave) return;
		_waveTime += Time.fixedDeltaTime;
		Vector2 normal = (_currentDirection == eGravityDirection.Normal) ? Vector2.up : Vector2.down;
		float wave = Mathf.Sin((_waveTime + _waveOffset) * waveFrequency * Mathf.PI * 2f) * waveAmplitude;

		_rb.MovePosition(new Vector2(_rb.position.x, _floatCenter.y + normal.y * wave));
	}

	private void OnCollisionEnter2D(Collision2D other)
	{
		UpdateConveyorSpeed(other);

		if (other.gameObject.GetComponent<MovingBlock>() != null) _isTouchingMovingBlock = true;
		if (!_isFloating)
		{
			int mask = 1 << other.gameObject.layer;
			if ((standableLayers.value & mask) != 0)
			{
				float impact = other.relativeVelocity.magnitude;
				if (impact >= landMinImpactSpeed && SfxManager.Instance != null && landThudClip != null)
				{
					SfxManager.Instance.PlayThudOnce(landThudClip, landVolume, landOffset);
				}
			}
		}
	}

	private void OnCollisionStay2D(Collision2D other)
	{
		UpdateConveyorSpeed(other);
		if (other.gameObject.GetComponent<MovingBlock>() != null) _isTouchingMovingBlock = true;
	}

	private void OnCollisionExit2D(Collision2D other)
	{
		if (other.gameObject.GetComponent<SurfaceEffector2D>() != null)
		{
			_conveyorSpeed = 0f;
		}
		if (other.gameObject.GetComponent<MovingBlock>() != null) _isTouchingMovingBlock = false;
	}

	private void UpdateConveyorSpeed(Collision2D collision)
	{
		SurfaceEffector2D effector = collision.gameObject.GetComponent<SurfaceEffector2D>();
		if (effector != null)
		{
			_conveyorSpeed = effector.speed;
		}
	}

	private Vector2 ComputeFloatCenterAndCacheSurface(eGravityDirection direction)
	{
		Vector2 gravityDir = (direction == eGravityDirection.Normal) ? Vector2.down : Vector2.up;
		RaycastHit2D[] hits = Physics2D.RaycastAll(_col.bounds.center, gravityDir, surfaceRayDistance, standableLayers);

		foreach (var h in hits)
		{
			if (h.collider != null && h.collider != _col)
			{
				_surfaceCollider = h.collider;
				float halfHeight = _col.bounds.extents.y;
				Vector2 center = h.point - gravityDir * (halfHeight + surfaceGap);
				_surfaceLocalOffset = center - (Vector2)_surfaceCollider.transform.position;
				return center;
			}
		}
		return _rb.position;
	}
}