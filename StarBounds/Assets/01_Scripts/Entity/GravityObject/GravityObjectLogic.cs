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

	//[추가됨] 연기가 나기 위한 최소 낙하 거리 (유니티 화면에서 조절 가능!)
	[SerializeField] private float minFallDistanceForEffect = 1.5f;

	//[추가됨] 공중에 있을 때 가장 높았던(또는 낮았던) Y 좌표를 기억할 변수
	private float _peakY;
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

		// 🚨 [추가됨] 낙하 높이 추적 로직
		// Y축 속도가 0.05 이상이라는 건 허공을 날거나 떨어지고 있다는 뜻!
		if (Mathf.Abs(_rb.linearVelocity.y) > 0.05f)
		{
			if (_currentDirection == eGravityDirection.Normal)
				_peakY = Mathf.Max(_peakY, transform.position.y); // 떨어지기 전 가장 높은 곳 기억
			else
				_peakY = Mathf.Min(_peakY, transform.position.y); // 역중력일 땐 가장 낮은 곳 기억
		}
		else
		{
			// 땅에 가만히 멈춰있을 때는 피크 높이를 현재 높이로 초기화
			_peakY = transform.position.y;
		}
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

			// 🚨 [추가됨] 내가 기억해둔 최고점(_peakY)과 지금 부딪힌 위치의 거리 차이를 계산
			float fallDistance = Mathf.Abs(transform.position.y - _peakY);

			// 🚨 [수정됨] 충돌 속도도 높고, 낙하 거리도 기준치(1.5f) 이상일 때만 실행!
			if (impact >= landMinImpactSpeed && fallDistance >= minFallDistanceForEffect)
			{
				bool isVerticalLanding = false;

				foreach (ContactPoint2D contact in other.contacts)
				{
					if (_currentDirection == eGravityDirection.Normal && contact.normal.y > 0.5f)
					{
						isVerticalLanding = true;
						break;
					}
					else if (_currentDirection == eGravityDirection.Inverse && contact.normal.y < -0.5f)
					{
						isVerticalLanding = true;
						break;
					}
				}

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

					// 🚨 [추가됨] 한 번 쿵! 찍었으면 바로 거리를 초기화해서 중복 발생 방지
					_peakY = transform.position.y;
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