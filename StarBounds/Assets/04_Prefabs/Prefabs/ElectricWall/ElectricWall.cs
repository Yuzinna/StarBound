using UnityEngine;
using UnityEngine.SceneManagement;

public class ElectricWall : MonoBehaviour
{
	[Header("오브젝트 연결")]
	public GameObject electricBeamsGroup;
	public ParticleSystem electricParticle;

	[Header("전기빔 부품들 연결")]
	public Transform topBeam;
	public Transform middleBeamTemplate;
	public Transform bottomBeam;
	public Transform startPost;
	public Transform endPost;

	[Header("전기벽 세팅")]
	[Tooltip("총 칸 수 (Start와 End 사이의 간격)")]
	public int beamLength = 4;

	[Tooltip("스프라이트 1칸의 간격 (보통 1)")]
	public float tileSize = 1f;

	[Tooltip("생성될 콜라이더의 두께 (Y축)")]
	public float beamThickness = 0.5f;

	[Header("사운드")]
	public AudioClip zapSfx;

	private int _blockingCubeCount = 0;
	private bool _isElectricOn = true;
	
	[ContextMenu("전기벽 길이 자동 맞춤")]
	public void SetupWall()
	{
		if (electricBeamsGroup == null || topBeam == null || middleBeamTemplate == null || bottomBeam == null)
		{
			Debug.LogWarning("부품들을 모두 인스펙터에 연결해주세요!");
			return;
		}

		// 1. 기존 복사본 청소
		for (int i = electricBeamsGroup.transform.childCount - 1; i >= 0; i--)
		{
			Transform child = electricBeamsGroup.transform.GetChild(i);
			if (child != topBeam && child != middleBeamTemplate && child != bottomBeam)
			{
				DestroyImmediate(child.gameObject);
			}
		}

		int actualLength = Mathf.Max(3, beamLength); // 최소 3칸

		// 2. 기둥(Post) 배치 (Start는 0, End는 칸수 위치)
		if (startPost != null) startPost.localPosition = new Vector3(0f, 0f, 0f);
		if (endPost != null) endPost.localPosition = new Vector3(actualLength * tileSize, 0f, 0f);

		// 3. 전기빔 그룹 배치 (Start와 첫 번째 빔 사이의 중간인 0.5 위치로 고정!)
		electricBeamsGroup.transform.localPosition = new Vector3(0.5f * tileSize, 0f, 0f);

		// 4. 자식 빔들 정렬 (ElectricBeams 기준 로컬 좌표)
		bottomBeam.localPosition = new Vector3(0f, 0f, 0f); // 0부터 시작

		for (int i = 1; i < actualLength - 1; i++) // 1부터 배치 시작
		{
			Transform newMiddle = Instantiate(middleBeamTemplate, electricBeamsGroup.transform);
			newMiddle.localPosition = new Vector3(i * tileSize, 0f, 0f);
			newMiddle.name = "Middle_Clone_" + i;
		}

		topBeam.localPosition = new Vector3((actualLength - 1) * tileSize, 0f, 0f);

		// 5. 콜라이더 설정 (ElectricBeams에 자동 추가 및 세팅)
		BoxCollider2D childCol = electricBeamsGroup.GetComponent<BoxCollider2D>();
		if (childCol == null) childCol = electricBeamsGroup.AddComponent<BoxCollider2D>();
		childCol.isTrigger = true;

		
		float offsetX = (actualLength - 1) / 2f * tileSize;

		childCol.offset = new Vector2(offsetX, 0f);
		childCol.size = new Vector2(actualLength * tileSize, beamThickness);

		// 6. 충돌 전달자 스크립트 달아주기 (이전 단계에서 만든 자식용 스크립트)
		ElectricBeamTrigger triggerHelper = electricBeamsGroup.GetComponent<ElectricBeamTrigger>();
		if (triggerHelper == null) triggerHelper = electricBeamsGroup.AddComponent<ElectricBeamTrigger>();
		triggerHelper.parentWall = this;

		Debug.Log($"가로 방향 {actualLength}칸 자동 생성");
	}

	// =========================================================
	// 자식(ElectricBeamTrigger)이 호출해 줄 충돌 로직들
	// =========================================================
	public void OnBeamEnter(Collider2D collision)
	{
		if (collision.CompareTag("Cube"))
		{
			_blockingCubeCount++;
			UpdateElectricState();
		}
	}

	public void OnBeamStay(Collider2D collision)
	{
		if (_isElectricOn && collision.CompareTag("Player"))
		{
			KillAndRespawnPlayer(collision.gameObject);
		}
	}

	public void OnBeamExit(Collider2D collision)
	{
		if (collision.CompareTag("Cube"))
		{
			_blockingCubeCount--;
			if (_blockingCubeCount < 0) _blockingCubeCount = 0;
			UpdateElectricState();
		}
	}

	// =========================================================
	// 전력 제어 및 사망 로직
	// =========================================================
		private void UpdateElectricState()
		{
			_isElectricOn = (_blockingCubeCount == 0);

			// 🚨 오브젝트 전체를 끄지 않고(SetActive 금지), 그림(SpriteRenderer)만 끕니다!
			// 그래야 콜라이더가 살아있어서 큐브가 나가는 걸 인식할 수 있습니다.
			if (electricBeamsGroup != null)
			{
				SpriteRenderer[] renderers = electricBeamsGroup.GetComponentsInChildren<SpriteRenderer>();
				foreach (var sr in renderers)
				{
					sr.enabled = _isElectricOn;
				}
			}

			// 혹시 Top, Bottom, 원본 Middle이 그룹 바깥에 있을 경우를 대비해 확실하게 그림을 숨깁니다.
			if (topBeam != null) topBeam.GetComponent<SpriteRenderer>().enabled = _isElectricOn;
			if (bottomBeam != null) bottomBeam.GetComponent<SpriteRenderer>().enabled = _isElectricOn;
			if (middleBeamTemplate != null) middleBeamTemplate.GetComponent<SpriteRenderer>().enabled = _isElectricOn;

			// 파티클(불꽃) 제어
			if (electricParticle != null)
			{
				if (_isElectricOn && !electricParticle.isPlaying) electricParticle.Play();
				else if (!_isElectricOn && electricParticle.isPlaying)
				{
					electricParticle.Stop();
					electricParticle.Clear(); // 끄는 즉시 남아있는 불꽃 찌꺼기도 깔끔하게 지움!
				}
			}
		}
	

	private void KillAndRespawnPlayer(GameObject player)
	{
		if (zapSfx != null && SfxManager.Instance != null) SfxManager.Instance.PlaySfx(zapSfx);

		PlayerDeath playerDeath = player.GetComponent<PlayerDeath>();
		if (playerDeath != null) playerDeath.Die();
		else SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}
}