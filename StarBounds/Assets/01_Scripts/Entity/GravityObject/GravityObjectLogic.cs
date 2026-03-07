
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

	// 상태 관리 (기존 변수들 아래에 추가)
	private bool _isTouchingMovingBlock = false; // 움직이는 블럭과 닿아있는가?
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

		SyncGravityState();

		// ⭐ 핵심: 물리 및 컨베이어 로직
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
	// =========================================================================
	// [추가] 물리 엔진 오류 없는 완벽한 순간이동 전용 함수!
	// =========================================================================
	public void TeleportTo(Vector2 newPosition)
	{
		// 1. Rigidbody와 Transform 위치를 완전히 이동시키고 속도 초기화
		_rb.position = newPosition;
		transform.position = newPosition;
		_rb.linearVelocity = Vector2.zero;

		if (_isFloating)
		{
			// 2. 물리 엔진에 "위치 옮겼으니 지금 당장 새로고침 해!" 라고 강제 명령 (매우 중요)
			Physics2D.SyncTransforms();

			// 3. 예전 바닥의 기억을 완전히 지우고 새 위치에서 다시 바닥 찾기
			_surfaceCollider = null;
			_floatCenter = ComputeFloatCenterAndCacheSurface(_currentDirection);

			// 4. 강제로 끌어내리던 Waving 상태를 멈추고, 천천히 하강(Ascending)하도록 리셋
			_floatingState = FloatingState.Ascending;
			_waveTime = 0f;
		}
	}
	// --------------------------------------------------
	// ⭐ 가장 완벽한 물리 고정 & 컨베이어 로직 (수정됨)
	// --------------------------------------------------
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
			// [여기 추가!] 움직이는 블럭에 닿아있으면 잠시 X축 고정을 풀어줍니다!
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
	// 중력 및 플로팅 전환
	// --------------------------------------------------
	private void SyncGravityState()
	{
		var newDir = GravityManager.Instance.CurrentDirection;
		var newFloating = GravityManager.Instance.IsFloatingEnabled;

		if (newDir != _currentDirection || newFloating != _isFloating)
		{
			ApplyGravityAndFloatingState(newDir, newFloating);
		}
	}

	public void ApplyGravityAndFloatingState(eGravityDirection direction, bool isFloating)
	{
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

	// --------------------------------------------------
	// 충돌 처리 (컨베이어 속도 수집 & 착지음)
	// --------------------------------------------------
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
			_conveyorSpeed = 0f; // 컨베이어에서 떨어지면 속도 초기화
		}
		// [추가] 움직이는 블럭에서 떨어졌어! (다시 X축 꽁꽁 얼리기)
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

	// --------------------------------------------------
	// 유틸리티
	// --------------------------------------------------
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