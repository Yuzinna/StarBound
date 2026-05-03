//using UnityEngine;
//using Unity.Cinemachine;

//[RequireComponent(typeof(Rigidbody2D))]
//[RequireComponent(typeof(Collider2D))]
//public class GravityObjectLogic : MonoBehaviour
//{
//	private Rigidbody2D _rb;
//	private Collider2D _col;
//	private CinemachineImpulseSource _impulseSource;

//	[Header("특수 설정")]
//	[SerializeField] private bool ignoreInverseGravity = false;
//	[SerializeField] private LayerMask standableLayers;

//	// 상태 관리
//	private bool _isPushable; // 💡 이제 매니저의 저중력 신호를 "밀기 가능(Pushable)" 신호로 씁니다.
//	private eGravityDirection _currentDirection;

//	// 컨베이어 및 움직이는 발판 관련
//	private float _conveyorSpeed = 0f;
//	private bool _isTouchingMovingBlock = false;

//	[Header("SFX & VFX - Landing")]
//	[SerializeField] private AudioClip landThudClip;
//	[SerializeField, Range(0f, 1f)] private float landVolume = 0.8f;
//	[SerializeField] private float landMinImpactSpeed = 1.2f;
//	[SerializeField] private ParticleSystem landDustParticle;

//	private void Awake()
//	{
//		_rb = GetComponent<Rigidbody2D>();
//		_col = GetComponent<Collider2D>();
//		_impulseSource = GetComponent<CinemachineImpulseSource>();
//	}

//	private void Start()
//	{
//		if (GravityManager.Instance == null) return;
//		SyncGravityState();
//	}

//	private void FixedUpdate()
//	{
//		if (GravityManager.Instance == null) return;

//		SyncGravityState();
//		HandlePhysicsAndConstraints();
//	}

//	private void SyncGravityState()
//	{
//		eGravityDirection targetDir = GravityManager.Instance.CurrentDirection;
//		bool targetPushable = GravityManager.Instance.IsFloatingEnabled;

//		if (ignoreInverseGravity) targetDir = eGravityDirection.Normal;

//		if (targetDir != _currentDirection || targetPushable != _isPushable)
//		{
//			ApplyGravityState(targetDir, targetPushable);
//		}
//	}

//	// 🚨 매니저에서 호출하는 함수 이름 유지
//	public void ApplyGravityState(eGravityDirection direction, bool isLowGravity)
//	{
//		_currentDirection = direction;
//		_isPushable = isLowGravity; // 저중력 = 밀 수 있음

//		float gScale = GravityManager.Instance.normalGravityScale;
//		_rb.gravityScale = (direction == eGravityDirection.Normal) ? gScale : -gScale;

//		// 💡 [핵심] 질량(Mass)이나 마찰력을 바꾸는 코드가 싹 사라졌습니다! 팝콘 빠이빠이!
//	}

//	private void HandlePhysicsAndConstraints()
//	{
//		// 1. 회전은 항상 고정
//		RigidbodyConstraints2D constraints = RigidbodyConstraints2D.FreezeRotation;

//		if (!_isPushable)
//		{
//			// 🚨 [스위치 OFF] 밀 수 없는 상태: X축을 아예 잠가버림 (절대 못 밈)
//			// 단, 컨베이어 벨트나 움직이는 발판 위에 있을 때는 잠그면 발판을 못 따라가므로 예외 처리!
//			if (_conveyorSpeed == 0f && !_isTouchingMovingBlock)
//			{
//				constraints |= RigidbodyConstraints2D.FreezePositionX;
//			}
//		}
//		// [스위치 ON] 상태라면 X축이 잠기지 않으므로 자유롭게 밀 수 있음!

//		_rb.constraints = constraints;

//		// 컨베이어 벨트 이동 처리
//		if (_conveyorSpeed != 0f)
//		{
//			_rb.linearVelocity = new Vector2(_conveyorSpeed, _rb.linearVelocity.y);
//		}
//	}

//	private void OnCollisionEnter2D(Collision2D other)
//	{
//		UpdateConveyorSpeed(other);
//		if (other.gameObject.GetComponent<MovingBlock>() != null) _isTouchingMovingBlock = true;

//		CheckLandingEffect(other);
//	}

//	private void CheckLandingEffect(Collision2D other)
//	{
//		int mask = 1 << other.gameObject.layer;
//		if ((standableLayers.value & mask) != 0)
//		{
//			float impact = other.relativeVelocity.magnitude;
//			if (impact >= landMinImpactSpeed)
//			{
//				if (SfxManager.Instance != null && landThudClip != null)
//					SfxManager.Instance.PlayThudOnce(landThudClip, landVolume, 0.6f);

//				if (landDustParticle != null)
//				{
//					landDustParticle.transform.position = _col.bounds.center;
//					float rotZ = (_currentDirection == eGravityDirection.Inverse) ? 180f : 0f;
//					landDustParticle.transform.rotation = Quaternion.Euler(0, 0, rotZ);
//					landDustParticle.Play();
//				}

//				if (_impulseSource != null) _impulseSource.GenerateImpulse();
//			}
//		}
//	}

//	private void OnCollisionStay2D(Collision2D other)
//	{
//		UpdateConveyorSpeed(other);
//		if (other.gameObject.GetComponent<MovingBlock>() != null) _isTouchingMovingBlock = true;
//	}

//	private void OnCollisionExit2D(Collision2D other)
//	{
//		if (other.gameObject.GetComponent<SurfaceEffector2D>() != null) _conveyorSpeed = 0f;
//		if (other.gameObject.GetComponent<MovingBlock>() != null) _isTouchingMovingBlock = false;
//	}

//	private void UpdateConveyorSpeed(Collision2D collision)
//	{
//		SurfaceEffector2D effector = collision.gameObject.GetComponent<SurfaceEffector2D>();
//		if (effector != null) _conveyorSpeed = effector.speed;
//	}

//	public void TeleportTo(Vector2 newPosition)
//	{
//		_rb.position = newPosition;
//		_rb.linearVelocity = Vector2.zero;
//	}
//}
using UnityEngine;
using Unity.Cinemachine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class GravityObjectLogic : MonoBehaviour
{
	private Rigidbody2D _rb;
	private Collider2D _col;
	private CinemachineImpulseSource _impulseSource;

	[Header("특수 설정")]
	[SerializeField] private bool ignoreInverseGravity = false;
	[SerializeField] private LayerMask standableLayers;

	private bool _isPushable;
	private eGravityDirection _currentDirection;
	private float _conveyorSpeed = 0f;
	private bool _isTouchingMovingBlock = false;

	[Header("SFX & VFX - Landing")]
	[SerializeField] private AudioClip landThudClip;
	[SerializeField, Range(0f, 1f)] private float landVolume = 0.8f;
	[SerializeField] private float landMinImpactSpeed = 1.2f;
	[SerializeField] private ParticleSystem landDustParticle;

	private void Awake()
	{
		_rb = GetComponent<Rigidbody2D>();
		_col = GetComponent<Collider2D>();
		_impulseSource = GetComponent<CinemachineImpulseSource>();
	}

	private void Start()
	{
		if (GravityManager.Instance == null) return;
		SyncGravityState();
	}

	private void FixedUpdate()
	{
		if (GravityManager.Instance == null) return;

		SyncGravityState();
		HandlePhysicsAndConstraints();
	}

	private void SyncGravityState()
	{
		eGravityDirection targetDir = GravityManager.Instance.CurrentDirection;
		bool targetPushable = GravityManager.Instance.IsFloatingEnabled;

		if (ignoreInverseGravity) targetDir = eGravityDirection.Normal;

		if (targetDir != _currentDirection || targetPushable != _isPushable)
		{
			ApplyGravityState(targetDir, targetPushable);
		}
	}

	public void ApplyGravityState(eGravityDirection direction, bool isLowGravity)
	{
		_currentDirection = direction;
		_isPushable = isLowGravity;

		float gScale = GravityManager.Instance.normalGravityScale;
		_rb.gravityScale = (direction == eGravityDirection.Normal) ? gScale : -gScale;
	}

	// ==========================================
	// 🚨 [추가됨] 젠가 물리 법칙을 위한 감지 로직 🚨
	// ==========================================

	// 1. 내가 지금 '진짜 바닥(땅)'에 닿아있는지 확인 (다른 큐브 위면 false)
	private bool IsRestingOnFloor()
	{
		Vector2 origin = _col.bounds.center;
		Vector2 size = new Vector2(_col.bounds.size.x * 0.9f, 0.1f);
		float distance = (_col.bounds.size.y / 2f) + 0.05f;
		Vector2 dir = (_currentDirection == eGravityDirection.Normal) ? Vector2.down : Vector2.up;

		RaycastHit2D hit = Physics2D.BoxCast(origin, size, 0f, dir, distance, standableLayers);

		if (hit.collider != null)
		{
			// 내 아래에 있는 게 또 다른 큐브라면, 나는 탑 중간에 있는 거임 -> 바닥 아님!
			if (hit.collider.GetComponent<GravityObjectLogic>() != null) return false;

			return true; // 다른 큐브가 아니라면 진짜 바닥(Ground)임!
		}
		return false;
	}

	// 2. 플레이어가 나를 직접 터치(밀고) 있는지 확인
	private bool IsTouchedByPlayer()
	{
		Vector2 origin = _col.bounds.center;
		Vector2 size = new Vector2(_col.bounds.size.x + 0.1f, _col.bounds.size.y * 0.8f);

		Collider2D[] hits = Physics2D.OverlapBoxAll(origin, size, 0f);
		foreach (var h in hits)
		{
			if (h.CompareTag("Player")) return true;
		}
		return false;
	}

	private void HandlePhysicsAndConstraints()
	{
		RigidbodyConstraints2D constraints = RigidbodyConstraints2D.FreezeRotation;

		if (!_isPushable)
		{
			// [스위치 OFF] 
			if (_conveyorSpeed == 0f && !_isTouchingMovingBlock)
			{
				constraints |= RigidbodyConstraints2D.FreezePositionX;
			}
		}
		else
		{
			// [스위치 ON - 저중력 상태]
			bool isRestingOnFloor = IsRestingOnFloor();
			bool isTouchedByPlayer = IsTouchedByPlayer();

			// 🚨 핵심 규칙 🚨
			// 내가 맨 아래 깔려있는 큐브(바닥)인데, 플레이어가 날 직접 밀지 않는다면 -> 엑스칼리버처럼 바닥에 X축 영구 고정!
			if (isRestingOnFloor && !isTouchedByPlayer)
			{
				if (_conveyorSpeed == 0f && !_isTouchingMovingBlock)
				{
					constraints |= RigidbodyConstraints2D.FreezePositionX;
				}
			}
			// 그 외(공중, 탑 중간, 플레이어가 직접 미는 중)라면 X축 자유롭게 냅둠
		}

		_rb.constraints = constraints;

		if (_conveyorSpeed != 0f)
		{
			_rb.linearVelocity = new Vector2(_conveyorSpeed, _rb.linearVelocity.y);
		}
	}

	// ==========================================
	// 기존 충돌 처리 유지
	// ==========================================
	private void OnCollisionEnter2D(Collision2D other)
	{
		UpdateConveyorSpeed(other);
		if (other.gameObject.GetComponent<MovingBlock>() != null) _isTouchingMovingBlock = true;
		CheckLandingEffect(other);
	}

	private void CheckLandingEffect(Collision2D other)
	{
		int mask = 1 << other.gameObject.layer;
		if ((standableLayers.value & mask) != 0)
		{
			float impact = other.relativeVelocity.magnitude;
			if (impact >= landMinImpactSpeed)
			{
				// 🚨 [핵심 수정] 충돌 방향을 검사합니다.
				bool isVerticalLanding = false;

				foreach (ContactPoint2D contact in other.contacts)
				{
					// contact.normal.y 는 부딪힌 표면이 어느 방향을 바라보는지 나타냅니다.
					// 일반 중력일 때: 바닥이 나를 향해 위(Up, y가 양수)로 밀어냅니다.
					if (_currentDirection == eGravityDirection.Normal && contact.normal.y > 0.5f)
					{
						isVerticalLanding = true;
						break;
					}
					// 역중력일 때: 천장이 나를 향해 아래(Down, y가 음수)로 밀어냅니다.
					else if (_currentDirection == eGravityDirection.Inverse && contact.normal.y < -0.5f)
					{
						isVerticalLanding = true;
						break;
					}
				}

				// 수직으로(위/아래) 착지한 게 맞을 때만 이펙트와 진동을 발생시킵니다!
				if (isVerticalLanding)
				{
					if (SfxManager.Instance != null && landThudClip != null)
						SfxManager.Instance.PlayThudOnce(landThudClip, landVolume, 0.6f);

					if (landDustParticle != null)
					{
						landDustParticle.transform.position = _col.bounds.center;
						float rotZ = (_currentDirection == eGravityDirection.Inverse) ? 180f : 0f;
						landDustParticle.transform.rotation = Quaternion.Euler(0, 0, rotZ);
						landDustParticle.Play();
					}

					if (_impulseSource != null) _impulseSource.GenerateImpulse();
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
		if (other.gameObject.GetComponent<SurfaceEffector2D>() != null) _conveyorSpeed = 0f;
		if (other.gameObject.GetComponent<MovingBlock>() != null) _isTouchingMovingBlock = false;
	}

	private void UpdateConveyorSpeed(Collision2D collision)
	{
		SurfaceEffector2D effector = collision.gameObject.GetComponent<SurfaceEffector2D>();
		if (effector != null) _conveyorSpeed = effector.speed;
	}

	public void TeleportTo(Vector2 newPosition)
	{
		_rb.position = newPosition;
		_rb.linearVelocity = Vector2.zero;
	}
}