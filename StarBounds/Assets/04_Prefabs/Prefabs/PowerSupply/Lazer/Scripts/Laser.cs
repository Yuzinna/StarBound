using System.Collections;
using UnityEngine;

public class Laser : MonoBehaviour
{
	[Header("이펙트")]
	public ParticleSystem hitSpark; // 부딪히는 곳에서 생길 파티클

	[Header("사운드")]
	public AudioClip bounceSfx;      // 부딪혔을 때 재생할 반사 소리
	private bool isBouncing = false; // 소리 중복 재생 방지를 위한 상태 변수

	//레이저 프리팹
	public Laser prefab;

	// [중요 2] 코드가 실제로 만들어낸 다음 레이저의 정보를 저장 (Inspector에 안 보이게 함)
	private Laser nextLaser;

	public int bounce = 0;     // 현재 튕긴 횟수
	public int maxBounce = 5;  // 최대 튕길 횟수 (반사 횟수 제한!)

	private SpriteRenderer sr;
	private BoxCollider2D bc;

	// 레이저가 부딪힐 레이어 설정
	private LayerMask hitLayers;
	private float maxDist = 100f;

	// [추가] 레이저 꺼짐 시퀀스 확인 변수
	private bool isTurningOff = false;
	// [추가] 현재 레이저가 닿고 있는 스위치 정보를 저장
	private SwitchLogic currentSwitch;

	private void Awake()
	{
		GetReferences();
	}

	private void GetReferences()
	{
		sr = GetComponent<SpriteRenderer>();
		bc = GetComponent<BoxCollider2D>();

		sr.drawMode = SpriteDrawMode.Tiled;
		// [수정] "Switch" 레이어도 감지할 수 있게 추가!
		hitLayers = LayerMask.GetMask("Ground", "Mirror", "Interactable", "Portal");
		// [수정 1순위 추가!] 내 몸(자식 오브젝트)에 매달려 있는 파티클 시스템을 자동으로 찾아서 할당!
		hitSpark = GetComponentInChildren<ParticleSystem>();
	}

	private void FixedUpdate()
	{
		// [추가] 레이저가 꺼지는 중이라면 작동 중단!
		if (isTurningOff) return;
		FormLaser();
	}

	private void FormLaser()
	{
		RaycastHit2D ray = Physics2D.Raycast(transform.position, transform.right, maxDist, hitLayers);
		float dist = ray.collider != null ? ray.distance : maxDist;

		sr.size = new Vector2(dist, sr.size.y);
		bc.size = new Vector2(dist, bc.size.y);
		bc.offset = new Vector2(dist / 2f, bc.offset.y);

		SwitchLogic hitSwitch = null; // 이번 프레임에 찾은 스위치

		// 무언가에 맞았을 때
		if (ray.collider != null)
		{
			// [파티클 추가] 부딪힌 위치로 파티클을 이동시킨다!
			if (hitSpark != null)
			{
				hitSpark.transform.position = ray.point; // 부딪힌 지점으로 이동
				if (!hitSpark.isPlaying) hitSpark.Play(); // 멈춰있다면 재생
				Debug.Log("파티클 재생 코드 들어감!");
			}

			// 1. 거울에 맞았을 때
			if (ray.collider.CompareTag("Mirror"))
			{
				// [사운드 처리]
				if (!isBouncing)
				{
					isBouncing = true;
					if (SfxManager.Instance != null && bounceSfx != null)
						SfxManager.Instance.PlaySfx(bounceSfx, 1f, 0.2f);
				}

				// [반사 계산]
				Vector2 inDir = transform.right;
				Vector2 refDir = Vector2.Reflect(inDir, ray.normal);

				if (Mathf.Abs(refDir.x) > Mathf.Abs(refDir.y))
					refDir = new Vector2(Mathf.Sign(refDir.x), 0f);
				else
					refDir = new Vector2(0f, Mathf.Sign(refDir.y));

				// [다음 레이저 생성]
				if (bounce < maxBounce)
				{
					if (nextLaser == null)
					{
						nextLaser = Instantiate(prefab);
						nextLaser.bounce = this.bounce + 1;
					}
					nextLaser.gameObject.SetActive(true);
					nextLaser.transform.position = ray.point + (refDir * 0.05f);
					nextLaser.transform.right = refDir;
				}
			}
			else if (ray.collider.CompareTag("Portal"))
			{
				isBouncing = false; // 거울이 아니므로 반사음은 끈다

				// 들어온 포탈의 스크립트 가져오기
				Portal inPortal = ray.collider.GetComponent<Portal>();

				// 연결된 반대편 포탈이 있다면?
				if (inPortal != null && inPortal.linkedPortal != null)
				{
					if (bounce < maxBounce)
					{
						if (nextLaser == null)
						{
							nextLaser = Instantiate(prefab);
							nextLaser.bounce = this.bounce + 1;
						}
						nextLaser.gameObject.SetActive(true);

						// 1. 위치: 반대편 포탈 위치에서, '원래 날아가던 방향'으로 살짝 띄워서 생성
						nextLaser.transform.position = inPortal.linkedPortal.transform.position + (transform.right * 0.6f);

						// 2. 방향: 들어온 포탈의 각도와 상관없이, '원래 날아가던 방향'을 그대로 유지!
						nextLaser.transform.right = transform.right;
					}
				}
			}
			// 2. 스위치에 맞았을 때
			else if (ray.collider.CompareTag("Switch"))
			{
				isBouncing = false;
				if (nextLaser != null) nextLaser.gameObject.SetActive(false); // 다음 레이저는 끈다

				hitSwitch = ray.collider.GetComponent<SwitchLogic>();
			}
			// 3. 벽이나 다른 물체에 맞았을 때
			else
			{
				isBouncing = false;
				if (nextLaser != null) nextLaser.gameObject.SetActive(false); // 다음 레이저는 끈다
			}
		}
		// 허공일 때
		else
		{
			isBouncing = false;
			if (nextLaser != null) nextLaser.gameObject.SetActive(false); // 다음 레이저는 끈다

			// [파티클 중지]
			if (hitSpark != null) hitSpark.Stop();
		}

		// --- 스위치 작동 제어 로직 ---
		//if (hitSwitch != currentSwitch)
		//{
		//	if (currentSwitch != null) currentSwitch.SetLaserPower(false);
		//	currentSwitch = hitSwitch;
		//	if (currentSwitch != null) currentSwitch.SetLaserPower(true);
		//}
		
		if (hitSwitch != currentSwitch)
		{
			// 1. 레이저가 스위치에서 떨어져서 꺼질 때!
			if (currentSwitch != null)
			{
				// 도대체 누가 레이저를 막았는지(또는 허공인지) 이름을 알아냅니다.
				string culprit = (ray.collider != null) ? ray.collider.gameObject.name : "허공(아무것도 없음)";
				Debug.LogWarning($"🚨 레이저 끊어짐! 스위치({currentSwitch.name}) OFF! 범인: {culprit}");

				currentSwitch.SetLaserPower(false);
			}

			currentSwitch = hitSwitch;

			// 2. 레이저가 스위치에 닿아서 켜질 때!
			if (currentSwitch != null)
			{
				Debug.Log($"✅ 레이저 연결됨! 스위치({currentSwitch.name}) ON!");
				currentSwitch.SetLaserPower(true);
			}
		}
	}

	public void TurnOffSequence()
	{
		if (gameObject.activeInHierarchy)
			StartCoroutine(TurnOffRoutine());
	}

	private IEnumerator TurnOffRoutine()
	{
		isTurningOff = true; // 1. 레이저 작동 로직 중단
		sr.enabled = false;   // 2. 렌더러 끄기
		bc.enabled = false;   // 3. 충돌체 끄기

		// [추가] 레이저 꺼질 때, 닿아있던 스위치도 꺼지도록 처리!
		if (currentSwitch != null)
		{
			currentSwitch.SetLaserPower(false);
			currentSwitch = null;
		}

		// [파티클 중지] 레이저 사라질 때 같이 사라짐
		if (hitSpark != null) hitSpark.Stop();

		yield return new WaitForSeconds(0.05f); // 4. 짧은 대기 (연쇄적으로 사라지는 효과)

		// 5. 다음 레이저가 있다면 재귀적으로 끈다
		if (nextLaser != null)
			nextLaser.TurnOffSequence();

		gameObject.SetActive(false); // 6. 오브젝트 비활성화
	}

	private void OnEnable()
	{
		isTurningOff = false;
		isBouncing = false; // [추가] 다시 켤 때 상태 초기화!

		if (sr != null) sr.enabled = true;
		if (bc != null) bc.enabled = true;
	}

	private void OnDisable()
	{
		// 1. 레이저 끄기 전 닿아있던 스위치가 있다면 끈다!
		if (currentSwitch != null)
		{
			currentSwitch.SetLaserPower(false);
			currentSwitch = null;
		}

		// 2. 파티클 중지
		if (hitSpark != null) hitSpark.Stop();

		// 3. 자식 레이저들도 모두 비활성화
		if (nextLaser != null)
		{
			nextLaser.gameObject.SetActive(false);
		}

		isTurningOff = false;
		isBouncing = false;
	}
}