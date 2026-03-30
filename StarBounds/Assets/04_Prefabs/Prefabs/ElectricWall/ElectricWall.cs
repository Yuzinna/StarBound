using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic; // List 사용을 위해 추가

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
	public int beamLength = 4;
	public float tileSize = 1f;
	public float beamThickness = 0.5f;

	[Header("사운드")]
	public AudioClip zapSfx;

	// 💡 [수정] 현재 전기장 안에 들어와 있는 모든 큐브를 추적합니다.
	private List<Collider2D> _cubesInRange = new List<Collider2D>();
	private bool _isElectricOn = true;
	private float _currentBlockingX = float.MaxValue;

	[ContextMenu("전기벽 길이 자동 맞춤")]
	public void SetupWall()
	{
		if (electricBeamsGroup == null || topBeam == null || middleBeamTemplate == null || bottomBeam == null)
		{
			Debug.LogWarning("부품들을 모두 인스펙터에 연결해주세요!");
			return;
		}

		for (int i = electricBeamsGroup.transform.childCount - 1; i >= 0; i--)
		{
			Transform child = electricBeamsGroup.transform.GetChild(i);
			if (child != topBeam && child != middleBeamTemplate && child != bottomBeam)
			{
				DestroyImmediate(child.gameObject);
			}
		}

		int actualLength = Mathf.Max(3, beamLength);

		if (startPost != null) startPost.localPosition = new Vector3(0f, 0f, 0f);
		if (endPost != null) endPost.localPosition = new Vector3(actualLength * tileSize, 0f, 0f);

		electricBeamsGroup.transform.localPosition = new Vector3(0.5f * tileSize, 0f, 0f);

		bottomBeam.localPosition = new Vector3(0f, 0f, 0f);
		for (int i = 1; i < actualLength - 1; i++)
		{
			Transform newMiddle = Instantiate(middleBeamTemplate, electricBeamsGroup.transform);
			newMiddle.localPosition = new Vector3(i * tileSize, 0f, 0f);
			newMiddle.name = "Middle_Clone_" + i;
		}
		topBeam.localPosition = new Vector3((actualLength - 1) * tileSize, 0f, 0f);

		BoxCollider2D childCol = electricBeamsGroup.GetComponent<BoxCollider2D>();
		if (childCol == null) childCol = electricBeamsGroup.AddComponent<BoxCollider2D>();
		childCol.isTrigger = true;

		float offsetX = (actualLength - 1) / 2f * tileSize;
		childCol.offset = new Vector2(offsetX, 0f);
		childCol.size = new Vector2(actualLength * tileSize, beamThickness);

		ElectricBeamTrigger triggerHelper = electricBeamsGroup.GetComponent<ElectricBeamTrigger>();
		if (triggerHelper == null) triggerHelper = electricBeamsGroup.AddComponent<ElectricBeamTrigger>();
		triggerHelper.parentWall = this;
	}

	private void Update()
	{
		// 매 프레임 큐브들의 위치를 체크하여 전기를 업데이트합니다.
		UpdateElectricState();
	}

	public void OnBeamEnter(Collider2D collision)
	{
		if (collision.CompareTag("Cube"))
		{
			if (!_cubesInRange.Contains(collision))
				_cubesInRange.Add(collision);
		}
	}

	public void OnBeamStay(Collider2D collision)
	{
		if (collision.CompareTag("Player"))
		{
			// 💡 [중요] 플레이어가 전기에 닿았을 때, 차단막(큐브)보다 앞에 있는지 확인합니다.
			float playerLocalX = transform.InverseTransformPoint(collision.transform.position).x;

			if (playerLocalX < _currentBlockingX)
			{
				KillAndRespawnPlayer(collision.gameObject);
			}
		}
	}

	public void OnBeamExit(Collider2D collision)
	{
		if (collision.CompareTag("Cube"))
		{
			if (_cubesInRange.Contains(collision))
				_cubesInRange.Remove(collision);
		}
	}

	private void UpdateElectricState()
	{
		// 1. 가장 가까운 큐브 찾기
		_currentBlockingX = float.MaxValue;

		foreach (var cube in _cubesInRange)
		{
			if (cube == null) continue;
			// 큐브의 위치를 전기벽 기준 로컬 좌표로 변환
			float cubeLocalX = transform.InverseTransformPoint(cube.transform.position).x;
			if (cubeLocalX < _currentBlockingX)
			{
				_currentBlockingX = cubeLocalX;
			}
		}

		// 2. 각 빔 조각들의 가시성 결정
		// electricBeamsGroup의 localPosition (0.5)을 고려해야 합니다.
		float groupOffsetX = electricBeamsGroup.transform.localPosition.x;

		SpriteRenderer[] renderers = electricBeamsGroup.GetComponentsInChildren<SpriteRenderer>(true);
		foreach (var sr in renderers)
		{
			// 각 조각의 부모(ElectricWall) 기준 실제 X 좌표 계산
			float partLocalX = sr.transform.localPosition.x + groupOffsetX;

			// 조각의 위치가 큐브보다 앞(작음)에 있으면 켭니다.
			sr.enabled = (partLocalX < _currentBlockingX);
		}

		// 3. 파티클 위치 조정 (큐브가 막고 있다면 큐브 위치에서 스파크 발생)
		if (electricParticle != null)
		{
			if (_cubesInRange.Count > 0)
			{
				if (!electricParticle.isPlaying) electricParticle.Play();
				// 파티클 위치를 차단 지점으로 이동
				electricParticle.transform.localPosition = new Vector3(_currentBlockingX, 0f, 0f);
			}
			else
			{
				// 막는 게 없으면 끝 지점에 배치하거나 끕니다 (기획에 따라 선택)
				electricParticle.transform.localPosition = new Vector3(beamLength * tileSize, 0f, 0f);
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