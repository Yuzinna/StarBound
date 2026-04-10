using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class RollingBoulder : MonoBehaviour
{
	[Header("물리 설정")]
	[Tooltip("처음 출발할 때 서서히 속도가 붙는 정도 (낮을수록 무겁게 출발합니다!)")]
	public float acceleration = 5f;

	[Header("사운드 & 파티클")]
	public GameObject breakParticle;
	public AudioClip breakSfx;
	public AudioClip killSfx;

	private Rigidbody2D _rb;
	private Vector2 _rollDirection;
	private float _maxSpeed; // 이제 speed는 도착할 '최대 속도'가 됩니다.
	private bool _isInitialized = false;

	private void Awake()
	{
		_rb = GetComponent<Rigidbody2D>();
		_rb.bodyType = RigidbodyType2D.Dynamic;
		_rb.mass = 10f;
	}

	public void Initialize(Vector2 direction, float speed)
	{
		_rollDirection = direction.normalized;
		_maxSpeed = speed;
		_isInitialized = true;
	}

	private void FixedUpdate()
	{
		if (!_isInitialized) return;

		// 1. 우리가 최종적으로 도달해야 할 목표 속도
		float targetVelocityX = _rollDirection.x * _maxSpeed;

		// 2. 현재 내 속도
		float currentVelocityX = _rb.linearVelocity.x;

		// 💡 3. [핵심] 현재 속도에서 목표 속도까지 한 번에 안 가고, 'acceleration' 만큼 서서히 올립니다!
		float newVelocityX = Mathf.MoveTowards(currentVelocityX, targetVelocityX, acceleration * Time.fixedDeltaTime);

		// 4. 적용 (Y축 떨어지는 속도는 물리 엔진에 맡김)
		_rb.linearVelocity = new Vector2(newVelocityX, _rb.linearVelocity.y);
	}

	// ==========================================
	// 💡 변경된 충돌 처리 부분
	// ==========================================

	private void OnCollisionEnter2D(Collision2D collision)
	{
		HandleCollision(collision);
	}

	// 굴러가다가 스르륵 벽에 닿았을 때도 확실하게 깨지도록 Stay 추가!
	private void OnCollisionStay2D(Collision2D collision)
	{
		HandleCollision(collision);
	}

	private void HandleCollision(Collision2D collision)
	{
		// 1. 코기와 부딪혔을 때 (즉사)
		if (collision.gameObject.CompareTag("Player"))
		{
			if (killSfx != null && SfxManager.Instance != null) SfxManager.Instance.PlaySfx(killSfx);

			PlayerDeath playerDeath = collision.gameObject.GetComponent<PlayerDeath>();
			if (playerDeath != null) playerDeath.Die();
			else SceneManager.LoadScene(SceneManager.GetActiveScene().name);

			BreakBoulder();
			return;
		}

		// 2. 바닥이 아닌 '벽'에 부딪혔을 때 파괴
		// 💡 0번 지점만 보지 않고, 닿고 있는 '모든' 지점을 검사합니다!
		foreach (ContactPoint2D contact in collision.contacts)
		{
			// 수직(바닥)이 아니라 수평(옆면) 방향으로 부딪힌 지점이 단 하나라도 있다면?
			if (Mathf.Abs(contact.normal.x) > 0.5f)
			{
				BreakBoulder();
				return; // 부서졌으니 더 검사할 필요 없이 즉시 종료
			}
		}
	}

	private void BreakBoulder()
	{
		if (breakParticle != null) Instantiate(breakParticle, transform.position, Quaternion.identity);
		if (breakSfx != null && SfxManager.Instance != null) SfxManager.Instance.PlaySfx(breakSfx, 1f);

		Destroy(gameObject);
	}
}