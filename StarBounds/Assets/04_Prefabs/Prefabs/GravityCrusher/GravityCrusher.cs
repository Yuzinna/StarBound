//using UnityEngine;
//using UnityEngine.SceneManagement;

//[RequireComponent(typeof(Rigidbody2D))]
//[RequireComponent(typeof(BoxCollider2D))]
//public class GravityCrusher : MonoBehaviour
//{
//	[Header("쿵쿵이 세팅")]
//	public float smashSpeed = 20f;
//	public float returnSpeed = 3f;
//	public float detectRange = 6f;
//	public LayerMask detectLayer; // 반드시 Player 레이어로 설정!

//	[Header("사운드")]
//	public AudioClip smashSfx;
//	public AudioClip killSfx;

//	private Rigidbody2D _rb;
//	private BoxCollider2D _col;
//	private Vector3 _startPos;
//	private bool _isSmashing = false;
//	private bool _isReturning = false;

//	private void Awake()
//	{
//		_rb = GetComponent<Rigidbody2D>();
//		_col = GetComponent<BoxCollider2D>();

//		// 물리 엔진의 '버그'에 의존하지 않기 위해 무조건 Kinematic 고정!
//		_rb.bodyType = RigidbodyType2D.Kinematic;
//		_startPos = transform.position;
//	}

//	private void Update()
//	{
//		Vector2 smashDirection = (GravityManager.Instance.CurrentDirection == eGravityDirection.Normal) ? Vector2.down : Vector2.up;

//		// 대기 중일 때만 플레이어 감지
//		if (!_isSmashing && !_isReturning)
//		{
//			Debug.DrawRay(transform.position, smashDirection * detectRange, Color.red);
//			RaycastHit2D hit = Physics2D.Raycast(transform.position, smashDirection, detectRange, detectLayer);

//			if (hit.collider != null && hit.collider.CompareTag("Player"))
//			{
//				_isSmashing = true;
//			}
//		}
//		else if (_isReturning)
//		{
//			// 제자리로 슬금슬금 돌아가기
//			transform.position = Vector3.MoveTowards(transform.position, _startPos, returnSpeed * Time.deltaTime);
//			if (Vector3.Distance(transform.position, _startPos) < 0.01f)
//			{
//				transform.position = _startPos;
//				_isReturning = false;
//			}
//		}
//	}

//	private void FixedUpdate()
//	{
//		if (_isSmashing)
//		{
//			Vector2 smashDirection = (GravityManager.Instance.CurrentDirection == eGravityDirection.Normal) ? Vector2.down : Vector2.up;
//			float moveDistance = smashSpeed * Time.fixedDeltaTime;

//			// 💡 [절대 관통 불가 로직] 이동하기 직전에 네모난 스캐너를 쏴서 앞길을 싹 다 검사합니다!
//			// 옆 벽에 긁히는 걸 방지하기 위해 박스 크기를 살짝(0.9f) 줄입니다.
//			Vector2 boxSize = _col.bounds.size * 0.9f;

//			// 특정 레이어만 보지 않고, 앞으로 갈 거리에 있는 "모든 물체"를 다 가져옵니다.
//			RaycastHit2D[] hits = Physics2D.BoxCastAll(_col.bounds.center, boxSize, 0f, smashDirection, moveDistance);

//			bool hitWall = false;
//			float allowedDistance = moveDistance; // 기본적으로는 원래 속도만큼 이동

//			foreach (RaycastHit2D hit in hits)
//			{
//				// 자기 자신은 무시
//				if (hit.collider.gameObject == this.gameObject) continue;

//				// 1. 앞으로 갈 길에 코기가 있다면 즉시 처형! (밀려나기 전에 컷)
//				if (hit.collider.CompareTag("Player"))
//				{
//					KillPlayer(hit.collider.gameObject);
//				}
//				// 2. 플레이어가 아닌 무언가 단단한 것(천장, 바닥, 큐브)을 발견했다면?
//				else if (!hit.collider.isTrigger)
//				{
//					hitWall = true;
//					// 천장을 뚫지 않도록, '천장 직전까지만' 이동 거리를 강제로 줄여버립니다.
//					if (hit.distance < allowedDistance)
//					{
//						allowedDistance = hit.distance;
//					}
//				}
//			}

//			// 💡 수학적으로 계산된 안전한 거리까지만 이동! 절대 천장을 뚫을 수 없습니다.
//			_rb.position = _rb.position + (smashDirection * allowedDistance);

//			// 천장이나 바닥에 닿았다면 다시 올라가기 시작
//			if (hitWall)
//			{
//				_isSmashing = false;
//				_isReturning = true;

//				if (smashSfx != null && SfxManager.Instance != null)
//				{
//					SfxManager.Instance.PlaySfx(smashSfx, 1f);
//				}
//			}
//		}
//	}

//	// 혹시라도 살짝 스쳐서 박스캐스트를 피했을 경우를 대비한 2중 안전장치
//	private void OnCollisionStay2D(Collision2D collision)
//	{
//		if (collision.gameObject.CompareTag("Player"))
//		{
//			KillPlayer(collision.gameObject);
//		}
//	}

//	private void KillPlayer(GameObject playerObj)
//	{
//		if (killSfx != null && SfxManager.Instance != null) SfxManager.Instance.PlaySfx(killSfx);

//		PlayerDeath playerDeath = playerObj.GetComponent<PlayerDeath>();
//		if (playerDeath != null) playerDeath.Die();
//		else SceneManager.LoadScene(SceneManager.GetActiveScene().name);
//	}
//}
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class GravityCrusher : MonoBehaviour
{
	[Header("쿵쿵이 세팅")]
	public float smashSpeed = 20f;
	public float returnSpeed = 3f;
	public float detectRange = 6f;
	public LayerMask detectLayer; // 반드시 Player 레이어로 설정! (감지용으로만 씁니다)

	[Header("사운드")]
	public AudioClip smashSfx;
	public AudioClip killSfx;

	private Rigidbody2D _rb;
	private BoxCollider2D _col;
	private Vector3 _startPos;
	private bool _isSmashing = false;
	private bool _isReturning = false;

	private void Awake()
	{
		_rb = GetComponent<Rigidbody2D>();
		_col = GetComponent<BoxCollider2D>();

		// 물리 엔진의 '버그'에 의존하지 않기 위해 무조건 Kinematic 고정!
		_rb.bodyType = RigidbodyType2D.Kinematic;
		_startPos = transform.position;
	}

	private void Update()
	{
		Vector2 smashDirection = (GravityManager.Instance.CurrentDirection == eGravityDirection.Normal) ? Vector2.down : Vector2.up;

		// 대기 중일 때만 플레이어 감지
		if (!_isSmashing && !_isReturning)
		{
			Debug.DrawRay(transform.position, smashDirection * detectRange, Color.red);
			RaycastHit2D hit = Physics2D.Raycast(transform.position, smashDirection, detectRange, detectLayer);

			if (hit.collider != null && hit.collider.CompareTag("Player"))
			{
				_isSmashing = true;
			}
		}
		else if (_isReturning)
		{
			// 제자리로 슬금슬금 돌아가기
			transform.position = Vector3.MoveTowards(transform.position, _startPos, returnSpeed * Time.deltaTime);
			if (Vector3.Distance(transform.position, _startPos) < 0.01f)
			{
				transform.position = _startPos;
				_isReturning = false;
			}
		}
	}

	private void FixedUpdate()
	{
		if (_isSmashing)
		{
			Vector2 smashDirection = (GravityManager.Instance.CurrentDirection == eGravityDirection.Normal) ? Vector2.down : Vector2.up;
			float moveDistance = smashSpeed * Time.fixedDeltaTime;

			// 💡 [수정된 부분: 정면 판정 압축]
			// 가로 길이는 85%로 줄여서 옆면 스침을 방지하고, 세로 두께는 0.05f로 아주 얇은 '판'을 만듭니다.
			Vector2 boxSize = new Vector2(_col.bounds.size.x * 0.85f, 0.05f);

			// 이 얇은 판(스캐너)의 시작 위치를 쿵쿵이 몸통 중심이 아닌, 진행 방향의 '맨 앞부분(끝단)'으로 옮깁니다.
			Vector2 origin = (Vector2)_col.bounds.center + (smashDirection * (_col.bounds.size.y * 0.5f));

			// 특정 레이어만 보지 않고, 앞길에 있는 "모든 물체(큐브, 바닥 포함)"를 다 가져옵니다. (터널링 완벽 방어)
			RaycastHit2D[] hits = Physics2D.BoxCastAll(origin, boxSize, 0f, smashDirection, moveDistance);

			bool hitWall = false;
			float allowedDistance = moveDistance; // 기본적으로는 원래 속도만큼 이동

			foreach (RaycastHit2D hit in hits)
			{
				// 자기 자신은 무시
				if (hit.collider.gameObject == this.gameObject) continue;

				// 1. 앞으로 갈 길의 '정면'에 코기가 있다면 즉시 처형! (옆구리는 안전합니다)
				if (hit.collider.CompareTag("Player"))
				{
					KillPlayer(hit.collider.gameObject);
				}
				// 2. 플레이어가 아닌 무언가 단단한 것(천장, 바닥, 큐브)을 발견했다면?
				else if (!hit.collider.isTrigger)
				{
					hitWall = true;
					// 물체를 뚫지 않도록, '직전까지만' 이동 거리를 강제로 줄여버립니다.
					if (hit.distance < allowedDistance)
					{
						allowedDistance = hit.distance;
					}
				}
			}

			// 💡 수학적으로 계산된 안전한 거리까지만 이동! 절대 큐브나 천장을 뚫을 수 없습니다.
			_rb.position = _rb.position + (smashDirection * allowedDistance);

			// 천장, 바닥, 큐브에 닿았다면 멈추고 다시 올라가기 시작
			if (hitWall)
			{
				_isSmashing = false;
				_isReturning = true;

				if (smashSfx != null && SfxManager.Instance != null)
				{
					SfxManager.Instance.PlaySfx(smashSfx, 1f);
				}
			}
		}
	}

	// 🚨 [수정된 부분] 
	// 맨 아래에 있던 OnCollisionStay2D 함수를 아예 통째로 삭제했습니다!
	// 이제 가만히 있거나 움직이는 쿵쿵이의 '옆면'에 코기가 몸을 비벼도 절대 죽지 않습니다.

	private void KillPlayer(GameObject playerObj)
	{
		if (killSfx != null && SfxManager.Instance != null) SfxManager.Instance.PlaySfx(killSfx);

		PlayerDeath playerDeath = playerObj.GetComponent<PlayerDeath>();
		if (playerDeath != null) playerDeath.Die();
		else SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}
}