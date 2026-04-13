using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerLogic : MonoBehaviour
{
	#region [1. Variables / Settings]
	[Header("Movement & Jump")]
	public float moveSpeed = 5f;
	[Header("Jump Settings")]
	[SerializeField] private float _baseJumpForce = 12f;
	public float floatingJumpMultiplier = 1.3f;

	public float CurrentJumpForce
	{
		get
		{
			bool isFloating = GravityManager.Instance != null && GravityManager.Instance.IsFloatingEnabled;
			return isFloating ? _baseJumpForce * floatingJumpMultiplier : _baseJumpForce;
		}
	}

	[Header("Gravity State")]
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

	// 💡 [파티클 추가] 자식으로 달아둔 파티클을 여기에 연결!
	[Header("파티클 (자식 오브젝트)")]
	public ParticleSystem jumpDustParticle;
	public ParticleSystem landDustParticle;

	// --- State Variables ---
	private Rigidbody2D _rb;
	private IInteractable _currentInteractable;

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
		if (_animator != null) _animator.SetBool(AnimHashAlive, true);
		if (GravityManager.Instance != null)
		{
			ApplyGravityAndFloatingState(GravityManager.Instance.CurrentDirection, GravityManager.Instance.IsFloatingEnabled);
		}
	}

	private void Update()
	{
		if (GameManager.Instance != null && GameManager.Instance.IsPaused) return;

		UpdateDirectionVisuals();
		UpdateAnimation();
	}

	private void FixedUpdate()
	{
		if (GameManager.Instance != null && GameManager.Instance.IsPaused) return;
		if (_rb == null) return;

		// 💡 [착지 파티클] 이전 프레임 기억
		bool previousGrounded = _isGrounded;
		_isGrounded = GroundCheck();

		// 방금 땅에 닿았고, 속도가 좀 빠를 때만 파티클 재생
		if (!previousGrounded && _isGrounded && Mathf.Abs(_rb.linearVelocity.y) > 2f)
		{
			if (landDustParticle != null) landDustParticle.Play();
		}

		bool hasMoveInput = Mathf.Abs(_moveX) > 0.01f;
		_isPushing = _isCollidingWithPushable && _isGrounded && hasMoveInput;

		HandleMovementPhysics(hasMoveInput);
		HandleJumpPhysics();
	}
	#endregion

	#region [3. Input Methods]
	public void OnInputJump()
	{
		if (GameManager.Instance != null && GameManager.Instance.IsPaused) return;
		_jumpInputReceived = true;
	}

	public void OnInputInteract()
	{
		if (GameManager.Instance != null && GameManager.Instance.IsPaused) return;
		if (_currentInteractable != null) _currentInteractable.Interact(this);
	}

	public void OnInputDropThrough()
	{
		if (!_isGrounded) return;
		Vector2 rayStart = transform.TransformPoint(groundCheckOffset);
		RaycastHit2D hit = Physics2D.Raycast(rayStart, Vector2.down * _gravityDirection, groundCheckDistance, standableLayers);

		if (hit.collider != null)
		{
			SinglePlatformLogic platform = hit.collider.GetComponent<SinglePlatformLogic>();
			if (platform != null) platform.DisableCollisionForDrop(GetComponent<Collider2D>());
		}
	}
	#endregion

	#region [4. Physics Logic]
	private void HandleMovementPhysics(bool isMoving)
	{
		Vector2 v = _rb.linearVelocity;
		v.x = (_moveX * moveSpeed) + _conveyorSpeed;
		_rb.linearVelocity = v;

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
			else if (audioSourceLoop.isPlaying) audioSourceLoop.Stop();
		}
	}

	private void HandleJumpPhysics()
	{
		if (_isGrounded && _jumpInputReceived)
		{
			// 💡 [점프 파티클 재생]
			if (jumpDustParticle != null) jumpDustParticle.Play();

			_rb.linearVelocity = new Vector2(_rb.linearVelocity.x, 0f);
			_rb.AddForce(Vector2.up * _gravityDirection * CurrentJumpForce, ForceMode2D.Impulse);

			if (audioSourceSFX != null)
			{
				bool isFloating = GravityManager.Instance != null && GravityManager.Instance.IsFloatingEnabled;
				audioSourceSFX.Stop();
				audioSourceSFX.clip = isFloating ? floatingJumpSound : normalJumpSound;
				audioSourceSFX.Play();
			}
		}
		_jumpInputReceived = false;
	}

	private bool GroundCheck()
	{
		Vector2 rayStart = transform.TransformPoint(groundCheckOffset);
		Vector2 size = new Vector2(0.7f, 0.1f);
		RaycastHit2D hit = Physics2D.BoxCast(rayStart, size, 0f, Vector2.down * _gravityDirection, groundCheckDistance, standableLayers);
		return hit.collider != null;
	}
	#endregion

	#region [5. Visuals & Gravity & Collision (생략 없이 원본 유지)]
	public void ApplyGravityAndFloatingState(eGravityDirection direction, bool isFloating)
	{
		if (_rb == null) return;
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
		if (_spriteRenderer != null) _spriteRenderer.flipX = (_facingDir == -1);
	}

	private void UpdateAnimation()
	{
		if (_animator == null) return;
		float speedX = Mathf.Abs(_moveX * moveSpeed);
		float speedY = _rb.linearVelocity.y * _gravityDirection;

		_animator.SetFloat(AnimHashSpeed, speedX);
		_animator.SetBool(AnimHashIsGrounded, _isGrounded);
		_animator.SetBool(AnimHashIsPushing, _isPushing);
		_animator.SetFloat(AnimHashYSpeed, speedY);
	}

	private void OnCollisionStay2D(Collision2D other)
	{
		int layerMask = 1 << other.gameObject.layer;
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

	private void OnCollisionEnter2D(Collision2D other) { UpdateConveyorSpeed(other); }

	private void OnCollisionExit2D(Collision2D other)
	{
		int layerMask = 1 << other.gameObject.layer;
		if ((layerMask & pushableLayers) != 0) _isCollidingWithPushable = false;
		if (other.gameObject.GetComponent<SurfaceEffector2D>() != null) _conveyorSpeed = 0f;
	}

	private void UpdateConveyorSpeed(Collision2D collision)
	{
		SurfaceEffector2D effector = collision.gameObject.GetComponent<SurfaceEffector2D>();
		if (effector != null) _conveyorSpeed = effector.speed;
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (((1 << collision.gameObject.layer) & interactLayer) != 0)
			_currentInteractable = collision.gameObject.GetComponent<IInteractable>();
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		if (((1 << collision.gameObject.layer) & interactLayer) != 0)
		{
			var interactable = collision.GetComponent<IInteractable>();
			if (interactable == _currentInteractable) _currentInteractable = null;
		}
	}
	#endregion
}