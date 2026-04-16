using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Cinemachine; // 💡 시네머신 기능을 사용하기 위해 추가!

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
	public ParticleSystem smashDustParticle;

	private Rigidbody2D _rb;
	private BoxCollider2D _col;
	private Vector3 _startPos;
	private bool _isSmashing = false;
	private bool _isReturning = false;

	// 💡 [카메라 쉐이크 추가] 임펄스 소스를 담을 변수
	private CinemachineImpulseSource _impulseSource;

	private void Awake()
	{
		_rb = GetComponent<Rigidbody2D>();
		_col = GetComponent<BoxCollider2D>();

		// 💡 시작할 때 쿵쿵이에 달려있는 임펄스 소스 컴포넌트를 가져옵니다.
		_impulseSource = GetComponent<CinemachineImpulseSource>();

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

				if (hit.collider.CompareTag("Player"))
				{
					KillPlayer(hit.collider.gameObject);
				}
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

				if (smashDustParticle != null)
				{
					float rotZ = (GravityManager.Instance.CurrentDirection == eGravityDirection.Inverse) ? 180f : 0f;
					smashDustParticle.transform.rotation = Quaternion.Euler(0, 0, rotZ);
					smashDustParticle.Play();
				}

				// 💡 [핵심 추가] 바닥을 찍는 순간, 임펄스 충격파를 쾅! 하고 발사합니다.
				if (_impulseSource != null)
				{
					_impulseSource.GenerateImpulse();
				}
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