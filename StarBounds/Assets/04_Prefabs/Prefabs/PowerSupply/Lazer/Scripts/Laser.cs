	using System.Collections;
	using UnityEngine;

	public class Laser : MonoBehaviour
	{
		[Header("이펙트")]
		public ParticleSystem hitSpark; // 인스펙터에서 넣을 파티클

		[Header("사운드")]
		public AudioClip bounceSfx;      // 인스펙터에서 넣을 반사 소리
		private bool isBouncing = false; // 소리가 여러 번 나는 걸 막는 변수

		//레이저 프리팹
		public Laser prefab;

		// [변경점 2] 코드가 스스로 만들어낸 다음 레이저를 기억해 둘 변수 (Inspector에 안 보여도 됨)
		private Laser nextLaser;

		public int bounce = 0;     // 현재 튕긴 횟수
		public int maxBounce = 5;  // 최대 튕길 횟수 (게임 멈춤 방지!)

		private SpriteRenderer sr;
		private BoxCollider2D bc;

		// 최적화 및 유지보수를 위한 변수 캐싱
		private LayerMask hitLayers;
		private float maxDist = 100f;

		// [추가] 꺼지는 중인지 확인할 변수
		private bool isTurningOff = false;
		// [추가] 현재 전력을 공급 중인 스위치를 기억할 변수
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
			// [수정] "Switch" 레이어도 맞출 수 있게 추가!
			hitLayers = LayerMask.GetMask("Ground", "Mirror", "Interactable","Portal");
			// [마법의 1줄 추가!] 내 몸(자식 오브젝트)에 달려있는 파티클 시스템을 스스로 찾아서 연결!
			hitSpark = GetComponentInChildren<ParticleSystem>();

		}
		private void FixedUpdate()
		{
			// [추가] 꺼지는 중이면 새로 뻗어나가는 계산 중지!
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

			SwitchLogic hitSwitch = null; // 이번 프레임에 맞은 스위치

			// 무언가에 맞았을 때
			if (ray.collider != null)
			{
				// [파티클 추가] 부딪힌 위치로 파티클을 옮기고 켜기!
				if (hitSpark != null)
				{
					hitSpark.transform.position = ray.point; // 부딪힌 곳으로 이동
					if (!hitSpark.isPlaying) hitSpark.Play(); // 꺼져있으면 재생
					Debug.Log("파티클 재생 명령 들어감!");
				}
				// 1. 거울에 맞았을 때
				if (ray.collider.CompareTag("Mirror"))
				{
					// [사운드 재생]
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
					isBouncing = false; // 거울이 아니므로 반사음은 끕니다

					// 맞은 포탈의 스크립트 가져오기
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

							// 1. 위치: 반대편 포탈 위치에서, '원래 날아가던 방향(transform.right)'으로 살짝 밀어서 생성
							nextLaser.transform.position = inPortal.linkedPortal.transform.position + (transform.right * 0.6f);

							// 2. 방향: 오렌지 포탈의 방향은 무시하고, '원래 날아가던 방향'을 그대로 물려줌!
							nextLaser.transform.right = transform.right;
						}
					}
				}
				// 2. 스위치에 맞았을 때
				else if (ray.collider.CompareTag("Switch"))
				{
					isBouncing = false;
					if (nextLaser != null) nextLaser.gameObject.SetActive(false); // 꼬리 자르기

					hitSwitch = ray.collider.GetComponent<SwitchLogic>();
				}
				// 3. 땅이나 다른 벽에 맞았을 때
				else
				{
					isBouncing = false;
					if (nextLaser != null) nextLaser.gameObject.SetActive(false); // 꼬리 자르기
				}
			}
			// 허공에 쏠 때
			else
			{
				isBouncing = false;
				if (nextLaser != null) nextLaser.gameObject.SetActive(false); // 꼬리 자르기
				// [파티클 끄기]
				if (hitSpark != null) hitSpark.Stop();
			}

			// --- 전력 공급 켜고 끄기 로직 ---
			if (hitSwitch != currentSwitch)
			{
				if (currentSwitch != null) currentSwitch.SetLaserPower(false);
				currentSwitch = hitSwitch;
				if (currentSwitch != null) currentSwitch.SetLaserPower(true);
			}
		}
		public void TurnOffSequence()
		{
			if (gameObject.activeInHierarchy)
				StartCoroutine(TurnOffRoutine());
		}

		private IEnumerator TurnOffRoutine()
		{
			isTurningOff = true; // 1. 레이저 뻗어나가는 계산 멈춤
			sr.enabled = false;   // 2. 내 이미지 숨김
			bc.enabled = false;   // 3. 내 충돌체 숨김

			// [추가] 내가 꺼질 때, 때리고 있던 스위치가 있으면 전력 끊기!
			if (currentSwitch != null)
			{
				currentSwitch.SetLaserPower(false);
				currentSwitch = null;
			}
			// [파티클 끄기] 레이저 꺼질 때 불꽃도 같이 끔
			if (hitSpark != null) hitSpark.Stop();

			yield return new WaitForSeconds(0.05f); // 4. 아주 잠깐 대기 (숫자가 클수록 느리게 꺼짐)

			// 5. 다음 레이저가 있으면 걔한테도 꺼지라고 명령 (도미노!)
			if (nextLaser != null)
				nextLaser.TurnOffSequence();

			gameObject.SetActive(false); // 6. 완전 비활성화
		}

		// [새로 추가] 다시 발판을 밟아서 켜질 때 원래 상태로 복구해 주는 함수
		private void OnEnable()
		{

			isTurningOff = false;
			isBouncing = false; // [추가] 다시 켜질 때 "거울에 닿은 적 없음"으로 기억 리셋!

			if (sr != null) sr.enabled = true;
			if (bc != null) bc.enabled = true;
		}
	// 오브젝트가 SetActive(false)로 꺼지거나 파괴될 때 유니티가 '무조건' 마지막으로 실행해 주는 함수
	private void OnDisable()
	{
		// 1. 내가 죽기 전에 혹시 켜둔 스위치가 있다면? 무조건 전력을 끊고 죽는다!
		if (currentSwitch != null)
		{
			currentSwitch.SetLaserPower(false);
			currentSwitch = null;
		}

		// 2. 파티클 끄기 (혹시 불꽃이 허공에 남는 버그 방지)
		if (hitSpark != null) hitSpark.Stop();

		// 3. 내 꼬리(반사된 다음 레이저)가 허공에 남아있다면 걔네도 강제로 다 꺼버림!
		if (nextLaser != null)
		{
			nextLaser.gameObject.SetActive(false);
		}

		isTurningOff = false;
		isBouncing = false;
	}
}

