using UnityEngine;
[RequireComponent(typeof(Rigidbody2D))]

public class PlayerLogic : MonoBehaviour

{
	[Header("Movement")]
	[Tooltip("좌우 이동 속도")]
	public float moveSpeed = 5f;
	[Tooltip("점프 힘 (Impulse)")]
	public float jumpForce = 12f;

	[Tooltip("부유 상태일 때 점프 배수 (예: 1.3이면 30% 증가)")]
	public float floatingJumpMultiplier = 1.3f;

	[Header("Jump Boost By Gravity")]
	[Tooltip("기준이 될 기본 점프 힘")]
	[SerializeField] private float _baseJumpForce = 12f;


	[Header("Gravity State")]
	[Tooltip("1: 일반 중력 (아래), -1: 반중력 (위)")]
	//_gravityDirection을 직접 설정하지 않고 GravityManager로부터 받도록 수정
	[SerializeField] public float _gravityDirection = 1f;

	[Header("Interaction")]
	[Tooltip("상호작용 가능한 오브젝트가 있는 레이어")]
	public LayerMask interactLayer;


	[Header("Push Check")]
	[Tooltip("밀 수 있는 오브젝트 레이어 (박스 등)")]
	public LayerMask pushableLayers;

	private Rigidbody2D _rb;
	[SerializeField] private bool _isGrounded;
	[Header("Standable (Feet can stand here)")]
	[Tooltip("플레이어가 '밟고 설 수 있는' 모든 레이어들 (바닥, 박스, 발판 등)")]
	public LayerMask standableLayers;

	// ❗ [Ground Check Settings]
	[Header("Ground Check")]
	[Tooltip("발 아래 Ray를 쏠 시작점 (Collider 중앙 아래)")]
	public Vector3 groundCheckOffset;
	[Tooltip("Raycast의 길이")]
	public float groundCheckDistance = 1f;

	// ❗ [추가] 점프 입력 감지 플래그
	private bool _jumpInputReceived = false;

	private int _facingDir = 1; // 1 = 오른쪽, -1 = 왼쪽

	[SerializeField] private bool _isPushing;
	public float _moveX; // OnUpdate에서 입력 값을 받아 FixedUpdate에서 사용
	private bool _isCollidingWithPushable; // 미는 오브젝트와 충돌 중인가?

	[Header("Visual")]
	[SerializeField] private Animator _animator;
	[SerializeField] private SpriteRenderer _spriteRenderer;
	// 상호작용 관련

	
	private IInteractable _currentInteractable; // 굳이 안 써도 되지만, 상호작용 대상이 여러 개일 때를 위해 유지
	[SerializeField] private bool isInteract=false;
	
	private SwitchLogic _currentSwitch = null; // ❗ 현재 충돌 중인 SwitchLogic을 저장
											   // 입력 소스
	[Header("Push Settings")]
	[Tooltip("큐브를 밀 때 가하는 힘의 크기")]
	public float pushForceMagnitude = 5f; // 필요한 힘 크기 설정 (유니티 인스펙터에서 조정)
	[Tooltip("큐브의 레이어 (GroundCheck Layer와는 별개)")]
	public LayerMask cubeLayer; // GravityObjectLogic을 가진 오브젝트 레이어

	// 기존 AudioSource는 SFX용으로 사용
	public AudioSource audioSourceSFX;
	// ❗ [추가] 걷기 사운드 전용 AudioSource
	public AudioSource audioSourceLoop;
	// 유니티 인스펙터에서 할당할 사운드 클립들
	[Header("사운드 이펙트")]
	public AudioClip walkSound;
	public AudioClip normalJumpSound;//노말점프
	public AudioClip floatingJumpSound;//플로팅점프
	


	// ================== Unity Lifecycle / Physics ==================
	private void Awake()
	{
		
		if (_rb == null)	
			_rb = GetComponent<Rigidbody2D>();
		
		if (_animator == null)
			_animator = GetComponent<Animator>();

		if (_spriteRenderer == null)
			_spriteRenderer = GetComponent<SpriteRenderer>();

		audioSourceSFX.volume = 0.6f;
		audioSourceLoop.volume = 1.0f;

	}

	private void Start()
	{
		
		//초기 상태를 반영
		if (GravityManager.Instance != null)
		{
			ApplyGravityAndFloatingState(
				GravityManager.Instance.CurrentDirection,
				GravityManager.Instance.IsFloatingEnabled
			);
		}
	}
	private void FixedUpdate()
	{
		if (_rb == null) return;
		// --- 1. 푸시 상태 결정 (물리 계산 전) ---
		// X축 움직임이 있을 때만 푸시 상태를 허용해야 애니메이션 고착 문제를 해결합니다.
		// 현재 코드에서는 _moveX 체크가 빠져있으므로, 안전을 위해 0.1f 체크를 다시 추가합니다.
		bool currentlyGrounded = GroundCheck();

		
		_isGrounded = currentlyGrounded;

		// ▼ 입력값이 있을 때만 푸시 상태 허용
		bool hasMoveInput = Mathf.Abs(_moveX) > 0.01f;
		_isPushing = _isCollidingWithPushable && _isGrounded && hasMoveInput;


		// --- 2. 이동 처리 (FixedUpdate로 이동) ---
		HandleMovementPhysics();
		// --- 3. 점프 처리 (FixedUpdate로 이동) ---
		HandleJumpPhysics();

		
	}
	

	// ================== EntityLogic ==================

	
	
	public void ApplyGravityAndFloatingState(eGravityDirection direction, bool isFloating)
	{
		if(_rb == null)
		{
			Debug.Log("rigid null");
		}
		// 1. 중력 방향 설정 (플레이어 Rigidbody의 gravityScale과 내부 _gravityDirection 변수 업데이트)
		if (direction == eGravityDirection.Normal)
		{
			_gravityDirection = 1f;
			_rb.gravityScale = GravityManager.Instance.normalGravityScale;
			// 스프라이트의 상하 반전 (예: 반중력 시 플레이어 뒤집기)
			if (_spriteRenderer != null) _spriteRenderer.transform.localScale = Vector3.one;
		}
		else // Inverse
		{
			_gravityDirection = -1f;
			_rb.gravityScale = -GravityManager.Instance.normalGravityScale;
			// 스프라이트 상하 반전
			if (_spriteRenderer != null) _spriteRenderer.transform.localScale = new Vector3(1f, -1f, 1f);
		}
		// 2. 부유 특성에 따른 점프력 및 드래그 설정
		if (isFloating)
		{
			// 부유 상태에서는 조금 더 높이 점프
			jumpForce = _baseJumpForce * floatingJumpMultiplier;
			// 부유 드래그 적용 (선택 사항: 플레이어에도 적용하고 싶다면)
			// _rb.drag = GravityManager.Instance.floatingDrag; 
		}
		else
		{
			// 원래 점프력으로 복구
			jumpForce = _baseJumpForce;
			// 일반 드래그 적용
			// _rb.drag = GravityManager.Instance.normalDrag;
		}

		Debug.Log($"[PlayerLogic] Dir={direction}, Floating={isFloating}, gravityScale={_rb.gravityScale}, jumpForce={jumpForce}");
	}

	public void OnInputDropThrough()
	{
		// 드롭 스루는 '바닥' 위에 있을 때만 작동해야 합니다.
		if (!_isGrounded) return;

		// 1. GroundCheck 로직을 사용하여 현재 밟고 있는 플랫폼의 콜라이더와 PlatformLogic을 가져옵니다.
		// (이전에 정의된 GroundCheck() 로직을 활용합니다.)

		// GroundCheck를 다시 수행하여 현재 닿고 있는 콜라이더 정보를 얻습니다.
		Vector2 rayStart = transform.TransformPoint(groundCheckOffset);

		// ❗ 중요: 레이캐스트의 방향을 현재 '중력 방향'과 동일하게 설정하여, 
		// 일반 중력(아래)이든 반중력(위)이든 '밟고 있는' 표면을 검사해야 합니다.
		RaycastHit2D hit = Physics2D.Raycast(rayStart, Vector2.down * _gravityDirection, groundCheckDistance, standableLayers);
		Debug.Log($"{hit.collider.gameObject}");
		if (hit.collider != null)
		{
			SinglePlatformLogic platform = hit.collider.GetComponent<SinglePlatformLogic>();

			if (platform != null)
			{
				// 2. 플랫폼의 드롭 함수 호출
				// 플레이어 자신의 콜라이더를 PlatformLogic에 넘겨줍니다.
				platform.DisableCollisionForDrop(GetComponent<Collider2D>());

				// 드롭 스루 시 즉시 중력 방향으로 약간의 힘을 가하여 콜라이더 겹침을 방지할 수 있습니다.
				//_rb.AddForce(Vector2.down * _gravityDirection * 5f, ForceMode2D.Impulse);
			}
		}
	}
	public void OnInputInteract()
	{
		// 입력 이벤트가 발생했을 때, 플레이어가 스위치와 충돌 중인지 확인합니다.
		if (_currentInteractable != null)
		{
			// 충돌 중이라면, 상호작용을 실행합니다. (PlayerLogic 인스턴스인 this를 전달)
			_currentInteractable.Interact(this);
		}
	}
	public void OnInputJump()
	{
			// 점프 키가 눌릴 때마다 플래그를 true로 설정
			_jumpInputReceived = true;
		
	}
	private void Update()
	{
		// 3. 시각적 업데이트는 OnUpdate에서 처리
		UpdateDirectionVisuals();
		UpdateAnimation();
	}
	
	private bool GroundCheck()
	{
		// GroundCheckOffset을 사용하여 발 근처에서 레이를 쏩니다.
		Vector2 rayStart = transform.TransformPoint(groundCheckOffset);

		Vector2 size = new Vector2(0.7f, 0.1f);
		// 레이캐스트의 방향을 현재 '중력 방향'(_gravityDirection)에 따라 설정
		
		RaycastHit2D hit = Physics2D.BoxCast(rayStart, size, 0f, Vector2.down * _gravityDirection, groundCheckDistance, standableLayers);
		// 디버깅을 위해 Ray를 그려줍니다.
		Debug.DrawRay(rayStart, Vector2.down * _gravityDirection * groundCheckDistance, hit.collider != null ? Color.green : Color.black);
		return hit.collider != null;
	}
	// ================== Movement / Jump Physics (FixedUpdate 호출) ==================
	/// 키보드 입력 처리 (좌우 이동) - Rigidbody 사용
	private void HandleMovementPhysics()
	{
		
		// Rigidbody2D의 x 속도만 변경 (y 속도는 점프/중력 담당)
		Vector2 v = _rb.linearVelocity;
		v.x = _moveX * moveSpeed;
		_rb.linearVelocity = v;
		// 1. 플레이어가 현재 움직이는 중인지 확인 (FixedUpdate이므로 _rb.linearVelocity.x 사용)
		bool isMoving = Mathf.Abs(_rb.linearVelocity.x) > 0.01f;

		// 2. 땅에 닿아 있고, 움직이는 중이라면
		if (_isGrounded && isMoving)
		{
			// AudioSource가 재생 중이 아니라면 (처음 걷기 시작할 때)
			if (audioSourceLoop != null && !audioSourceLoop.isPlaying)
			{
				// WalkSound를 AudioSource의 기본 클립으로 설정하고 반복 재생 시작
				audioSourceLoop.clip = walkSound;
				audioSourceLoop.loop = true; // 반복 재생
				audioSourceLoop.time = 2f;
				audioSourceLoop.Play();
			}
		}
		else // 멈췄거나 공중에 있다면
		{
			if (audioSourceLoop != null && audioSourceLoop.isPlaying)
			{
				audioSourceLoop.Stop();
			}
		}
	}
	/// 점프 처리 (바닥 위에서만 점프) - Rigidbody 사용
	private void HandleJumpPhysics()
	{
		// ❗ 점프 실행 조건: 땅에 닿아 있고, 점프 입력이 들어왔을 때
		bool canJump = _isGrounded && _jumpInputReceived;

		if (canJump)
		{
			// 점프 전 y 속도 초기화 (더블 점프 방지)
			Vector2 v = _rb.linearVelocity;
			v.y = 0f;
			_rb.linearVelocity = v;

			float jumpDirection = _gravityDirection;
			_rb.AddForce(Vector2.up * jumpDirection * jumpForce, ForceMode2D.Impulse);


			// 1. 현재 부유 상태 확인
			bool isFloating = GravityManager.Instance.IsFloatingEnabled;

			// 2. 재생할 사운드 클립 결정
			AudioClip clipToPlay = isFloating ? floatingJumpSound : normalJumpSound;
			float jumpOffset = isFloating ? 1.0f : 0.7f;
			//// ❗ [추가] 점프 사운드 재생							
			// 3. 사운드 재생
			if (audioSourceSFX != null && clipToPlay != null)
			{
				audioSourceSFX.Stop();   // ❗ 이전 재생 잔여 제거
				audioSourceSFX.clip = clipToPlay;
				audioSourceSFX.time = jumpOffset;
				audioSourceSFX.Play();
			}
			
		}
		_jumpInputReceived = false;
	}

	// ================== Visuals / Animation / Interaction (OnUpdate 호출) ==================
	/// 바라보는 방향 및 스프라이트 좌우 반전 업데이트
	private void UpdateDirectionVisuals()
	{
		// 바라보는 방향 업데이트
		if (_moveX > 0.01f)
			_facingDir = 1;
		else if (_moveX < -0.01f)
			_facingDir = -1;
		// 스프라이트 좌우 반전
		if (_facingDir != 0 && _spriteRenderer != null)
		{
			_spriteRenderer.flipX = (_facingDir == -1);
		}
	}
	private void UpdateAnimation()
	{
		if (_animator == null || _rb == null) return;
		float speedX = Mathf.Abs(_rb.linearVelocity.x);
		_animator.SetFloat("Speed", speedX);
		_animator.SetBool("IsGrounded", _isGrounded);
		_animator.SetBool("IsPushing", _isPushing);
	}
	// ================== Collision Check (바닥/푸시) ==================
	private void OnCollisionStay2D(Collision2D other)
	{
		int layerMask = 1 << other.gameObject.layer;

		if ((layerMask & pushableLayers) != 0)
		{
			bool sideContact = false;

			foreach (var contact in other.contacts)
			{
				// normal은 "상대 → 플레이어" 방향
				// - 위에서 밟고 있을 때: normal ≈ (0, 1)
				// - 왼쪽에서 박스를 밀 때: normal ≈ (1, 0)
				// - 오른쪽에서 박스를 밀 때: normal ≈ (-1, 0)

				bool isSide =
					Mathf.Abs(contact.normal.x) > 0.5f &&   // 옆면에 가깝고
					Mathf.Abs(contact.normal.y) < 0.5f;     // 위/아래는 아님

				if (isSide)
				{
					sideContact = true;
					break;
				}
			}

			// 옆면 접촉일 때만 "푸시 가능한 상태"로 취급
			_isCollidingWithPushable = sideContact;
		}
	}


	private void OnCollisionExit2D(Collision2D other)
	{
		int layerMask = 1 << other.gameObject.layer;

		// Ground 체크는 이제 Raycast(GroundCheck)가 담당하므로 여기선 안 건드림.

		// 2. Pushable 체크 리셋
		if ((layerMask & pushableLayers) != 0)
		{
			_isCollidingWithPushable = false;
		}
	}
	private void OnTriggerEnter2D(Collider2D collision)
	{
		
		int layerMask = 1 << collision.gameObject.layer;

		// ❗ 3. Interact 체크 (충돌 진입 시 SwitchLogic 저장)

		if ((layerMask & interactLayer) != 0)

		{

			// 충돌한 오브젝트에서 SwitchLogic을 찾고 저장

			_currentInteractable = collision.gameObject.GetComponent<IInteractable>();
			isInteract = true;

		}


	}
	private void OnTriggerStay2D(Collider2D collision)
	{
		
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
   		int layerMask = 1 << collision.gameObject.layer;
		// ❗ 3. Interact 체크 리셋 (충돌 종료 시 SwitchLogic 해제)
		if ((layerMask & interactLayer) != 0)
		{
			var interactable = collision.GetComponent<IInteractable>();
			// 충돌이 끝난 오브젝트가 현재 _currentSwitch였을 경우에만 null 처리
			if (interactable == _currentInteractable)
			{
				_currentInteractable = null;
				isInteract = false;
			}
		}
	}
}