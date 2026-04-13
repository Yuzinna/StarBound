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
	public LayerMask detectLayer;

	[Header("사운드 & 파티클")]
	public AudioClip smashSfx;
	public AudioClip killSfx;
	// 💡 [파티클 추가] 쿵쿵이 자식으로 달아둔 파티클 연결!
	public ParticleSystem smashDustParticle;

	private Rigidbody2D _rb;
	private BoxCollider2D _col;
	private Vector3 _startPos;
	private bool _isSmashing = false;
	private bool _isReturning = false;

	private void Awake()
	{
		_rb = GetComponent<Rigidbody2D>();
		_col = GetComponent<BoxCollider2D>();
		_rb.bodyType = RigidbodyType2D.Kinematic;
		_startPos = transform.position;
	}

	private void Update()
	{
		Vector2 smashDirection = (GravityManager.Instance.CurrentDirection == eGravityDirection.Normal) ? Vector2.down : Vector2.up;

		if (!_isSmashing && !_isReturning)
		{
			Debug.DrawRay(transform.position, smashDirection * detectRange, Color.red);
			RaycastHit2D hit = Physics2D.Raycast(transform.position, smashDirection, detectRange, detectLayer);

			if (hit.collider != null && hit.collider.CompareTag("Player")) _isSmashing = true;
		}
		else if (_isReturning)
		{
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

			Vector2 boxSize = new Vector2(_col.bounds.size.x * 0.85f, 0.05f);
			Vector2 origin = (Vector2)_col.bounds.center + (smashDirection * (_col.bounds.size.y * 0.5f));

			RaycastHit2D[] hits = Physics2D.BoxCastAll(origin, boxSize, 0f, smashDirection, moveDistance);

			bool hitWall = false;
			float allowedDistance = moveDistance;

			foreach (RaycastHit2D hit in hits)
			{
				if (hit.collider.gameObject == this.gameObject) continue;

				if (hit.collider.CompareTag("Player")) KillPlayer(hit.collider.gameObject);
				else if (!hit.collider.isTrigger)
				{
					hitWall = true;
					if (hit.distance < allowedDistance) allowedDistance = hit.distance;
				}
			}

			_rb.position = _rb.position + (smashDirection * allowedDistance);

			if (hitWall)
			{
				_isSmashing = false;
				_isReturning = true;

				if (smashSfx != null && SfxManager.Instance != null) SfxManager.Instance.PlaySfx(smashSfx, 1f);

				// 💡 [파티클 재생] 부딪혔을 때 그냥 냅다 재생!
				if (smashDustParticle != null) smashDustParticle.Play();
			}
		}
	}

	private void KillPlayer(GameObject playerObj)
	{
		if (killSfx != null && SfxManager.Instance != null) SfxManager.Instance.PlaySfx(killSfx);
		PlayerDeath playerDeath = playerObj.GetComponent<PlayerDeath>();
		if (playerDeath != null) playerDeath.Die();
		else SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}
}