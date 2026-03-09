using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerLogic : MonoBehaviour
{
	#region [1. Variables / Settings]
	[Header("Movement & Jump")]
	public float moveSpeed = 5f;
	[Header("Jump Settings (점프력 설정)")]
	[Tooltip("일반 상태일 때의 기본 점프력입니다. 평소 점프 높이를 바꾸려면 이 값을 조절하세요!")]
	[SerializeField] private float _baseJumpForce = 12f;

	[Tooltip("부유(Floating) 상태일 때 점프력이 몇 배로 뛸지 정합니다. (예: 1.3을 넣으면 평소보다 30% 높게 뜁니다)")]
	public float floatingJumpMultiplier = 1.3f;

	// 외부 스크립트에서 점프력이 궁금할 때 꺼내보는 '읽기 전용' 프로퍼티
	public float CurrentJumpForce
	{
		get
		{
			// 현재 중력 매니저가 부유 상태인지 실시간으로 확인하고, 맞으면 배수를 곱해서 돌려줍니다.
			bool isFloating = GravityManager.Instance != null && GravityManager.Instance.IsFloatingEnabled;
			return isFloating ? _baseJumpForce * floatingJumpMultiplier : _baseJumpForce;
		}
	}

	[Header("Gravity State")]
	[Tooltip("1: 일반 중력 (아래), -1: 반중력 (위)")]
	public float _gravityDirection = 1f;

	[Header("Layer Settings")]
	public LayerMask interactLayer;
	public LayerMask pushableLayers;
	public LayerMask standableLayers;

	[Header("Ground Check")]
	public Vector3 groundCheckOffset;
	public float groundCheckDistance = 1f;

	[Header("Visual & Audio")]
	[SerializeField] private Animator _animator;
	[SerializeField] private SpriteRenderer _spriteRenderer;
	public AudioSource audioSourceSFX;
	public AudioSource audioSourceLoop;
	public AudioClip walkSound;
	public AudioClip normalJumpSound;
	public AudioClip floatingJumpSound;

	// --- State Variables ---
	private Rigidbody2D _rb;
	private IInteractable _currentInteractable; // 상호작용 대상

	[SerializeField] private bool _isGrounded;
	[SerializeField] private bool _isPushing;
	private bool _isCollidingWithPushable;

	[HideInInspector] public float _moveX;
	private bool _jumpInputReceived = false;
	private int _facingDir = 1;
	private float _conveyorSpeed = 0f;

	private static readonly int AnimHashSpeed = Animator.StringToHash("Speed");
	private static readonly int AnimHashIsGrounded = Animator.StringToHash("IsGrounded");
	private static readonly int AnimHashIsPushing = Animator.StringToHash("IsPushing");
	private static readonly int AnimHashYSpeed = Animator.StringToHash("YSpeed");
	private static readonly int AnimHashAlive = Animator.StringToHash("Alive");
	#endregion

	#region [2. Unity Lifecycle]
	private void Awake()
	{
		_rb = GetComponent<Rigidbody2D>();
		if (_animator == null) _animator = GetComponent<Animator>();
		if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();

		if (audioSourceSFX != null) audioSourceSFX.volume = 0.6f;
		if (audioSourceLoop != null) audioSourceLoop.volume = 1.0f;
		
	}

	private void Start()
	{

		//태어날 때 무조건 Alive를 true로 설정
		if (_animator != null) _animator.SetBool(AnimHashAlive, true);
		// 중력 초기화
		if (GravityManager.Instance != null)
		{
			ApplyGravityAndFloatingState(GravityManager.Instance.CurrentDirection, GravityManager.Instance.IsFloatingEnabled);
		}
	}

	private void Update()
	{
		// 🚨 일시정지 중이거나 죽었을 때는 시각적 처리를 멈춥니다.
		if (GameManager.Instance != null && GameManager.Instance.IsPaused) return;

		UpdateDirectionVisuals();
		UpdateAnimation();
	}

	private void FixedUpdate()
	{
		// 🚨 일시정지 중에는 물리 연산 스킵
		if (GameManager.Instance != null && GameManager.Instance.IsPaused) return;
		if (_rb == null) return;

		_isGrounded = GroundCheck();
		bool hasMoveInput = Mathf.Abs(_moveX) > 0.01f;

		// 미는 상태 업데이트
		_isPushing = _isCollidingWithPushable && _isGrounded && hasMoveInput;

		HandleMovementPhysics(hasMoveInput);
		HandleJumpPhysics();
	}
	#endregion

	#region [3. Input Methods (Called by PlayerInput)]
	public void OnInputJump()
	{
		if (GameManager.Instance != null && GameManager.Instance.IsPaused) return;
		_jumpInputReceived = true;
	}

	public void OnInputInteract()
	{
		if (GameManager.Instance != null && GameManager.Instance.IsPaused) return;

		if (_currentInteractable != null)
		{
			_currentInteractable.Interact(this);
		}
	}

	public void OnInputDropThrough()
	{
		if (!_isGrounded) return;

		Vector2 rayStart = transform.TransformPoint(groundCheckOffset);
		RaycastHit2D hit = Physics2D.Raycast(rayStart, Vector2.down * _gravityDirection, groundCheckDistance, standableLayers);

		if (hit.collider != null)
		{
			SinglePlatformLogic platform = hit.collider.GetComponent<SinglePlatformLogic>();
			if (platform != null)
			{
				platform.DisableCollisionForDrop(GetComponent<Collider2D>());
			}
		}
	}
	#endregion

	#region [4. Physics Logic]
	private void HandleMovementPhysics(bool isMoving)
	{
		Vector2 v = _rb.linearVelocity;
		v.x = (_moveX * moveSpeed) + _conveyorSpeed;
		_rb.linearVelocity = v;

		// 사운드 처리 (최적화)
		if (audioSourceLoop != null)
		{
			if (_isGrounded && isMoving)
			{
				if (!audioSourceLoop.isPlaying)
				{
					audioSourceLoop.clip = walkSound;
					audioSourceLoop.loop = true;
					audioSourceLoop.Play();
				}
			}
			else if (audioSourceLoop.isPlaying)
			{
				audioSourceLoop.Stop();
			}
		}
	}

	private void HandleJumpPhysics()
	{
		if (_isGrounded && _jumpInputReceived)
		{
			// y축 속도 초기화 후 점프 (더블 점프/관성 방지)
			_rb.linearVelocity = new Vector2(_rb.linearVelocity.x, 0f);
			_rb.AddForce(Vector2.up * _gravityDirection * CurrentJumpForce, ForceMode2D.Impulse);

			// 사운드 재생
			if (audioSourceSFX != null)
			{
				bool isFloating = GravityManager.Instance != null && GravityManager.Instance.IsFloatingEnabled;
				audioSourceSFX.Stop();
				audioSourceSFX.clip = isFloating ? floatingJumpSound : normalJumpSound;
				audioSourceSFX.Play();
			}
		}
		_jumpInputReceived = false; // 입력 소비
	}

	private bool GroundCheck()
	{
		Vector2 rayStart = transform.TransformPoint(groundCheckOffset);
		Vector2 size = new Vector2(0.7f, 0.1f);
		RaycastHit2D hit = Physics2D.BoxCast(rayStart, size, 0f, Vector2.down * _gravityDirection, groundCheckDistance, standableLayers);

		Debug.DrawRay(rayStart, Vector2.down * _gravityDirection * groundCheckDistance, hit.collider != null ? Color.green : Color.red);
		return hit.collider != null;
	}
	#endregion

	#region [5. Visuals & Gravity]
	public void ApplyGravityAndFloatingState(eGravityDirection direction, bool isFloating)
	{
		if (_rb == null) return;

		// 중력 방향
		if (direction == eGravityDirection.Normal)
		{
			_gravityDirection = 1f;
			_rb.gravityScale = GravityManager.Instance.normalGravityScale;
			if (_spriteRenderer != null) _spriteRenderer.transform.localScale = Vector3.one;
		}
		else
		{
			_gravityDirection = -1f;
			_rb.gravityScale = -GravityManager.Instance.normalGravityScale;
			if (_spriteRenderer != null) _spriteRenderer.transform.localScale = new Vector3(1f, -1f, 1f);
		}

		
	}

	private void UpdateDirectionVisuals()
	{
		if (_moveX > 0.01f) _facingDir = 1;
		else if (_moveX < -0.01f) _facingDir = -1;

		if (_spriteRenderer != null)
		{
			_spriteRenderer.flipX = (_facingDir == -1);
		}
	}

	private void UpdateAnimation()
	{
		if (_animator == null) return;

		float speedX = Mathf.Abs(_moveX * moveSpeed);

		// 💡 [엄청 중요한 디테일!] 
		// 그냥 y 속도를 넣으면 안 되고, '중력 방향(_gravityDirection)'을 곱해줘야 합니다.
		// 그래야 반중력(위로 떨어짐) 상태일 때도 떨어지는 모션이 정상적으로 나옵니다!
		float speedY = _rb.linearVelocity.y * _gravityDirection;

		_animator.SetFloat(AnimHashSpeed, speedX);
		_animator.SetBool(AnimHashIsGrounded, _isGrounded);
		_animator.SetBool(AnimHashIsPushing, _isPushing);

		_animator.SetFloat(AnimHashYSpeed, speedY);
	}
	#endregion

	#region [6. Collision & Interactions]
	private void OnCollisionStay2D(Collision2D other)
	{
		int layerMask = 1 << other.gameObject.layer;

		// 미는 상태 체크 (옆면 접촉 확인)
		if ((layerMask & pushableLayers) != 0)
		{
			bool sideContact = false;
			foreach (var contact in other.contacts)
			{
				if (Mathf.Abs(contact.normal.x) > 0.5f && Mathf.Abs(contact.normal.y) < 0.5f)
				{
					sideContact = true;
					break;
				}
			}
			_isCollidingWithPushable = sideContact;
			UpdateConveyorSpeed(other);
		}
	}

	private void OnCollisionEnter2D(Collision2D other)
	{
		UpdateConveyorSpeed(other);
	}

	private void OnCollisionExit2D(Collision2D other)
	{
		int layerMask = 1 << other.gameObject.layer;

		if ((layerMask & pushableLayers) != 0)
		{
			_isCollidingWithPushable = false;
		}

		if (other.gameObject.GetComponent<SurfaceEffector2D>() != null)
		{
			_conveyorSpeed = 0f;
		}
	}

	private void UpdateConveyorSpeed(Collision2D collision)
	{
		SurfaceEffector2D effector = collision.gameObject.GetComponent<SurfaceEffector2D>();
		if (effector != null)
		{
			_conveyorSpeed = effector.speed;
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (((1 << collision.gameObject.layer) & interactLayer) != 0)
		{
			_currentInteractable = collision.gameObject.GetComponent<IInteractable>();
		}
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		if (((1 << collision.gameObject.layer) & interactLayer) != 0)
		{
			var interactable = collision.GetComponent<IInteractable>();
			if (interactable == _currentInteractable)
			{
				_currentInteractable = null;
			}
		}
	}
	#endregion
}