using System.Collections;
using UnityEngine;

public class BoulderSpawner : MonoBehaviour
{
	[Header("생성 설정")]
	public GameObject boulderPrefab;
	public Transform spawnPoint;

	[Header("구르기 설정")]
	public float rollSpeed = 8f;
	public bool rollToRight = false;

	[Header("타이머 설정")]
	[Tooltip("한 번 가동되면 몇 초마다 바위를 생성할까요?")]
	public float spawnInterval = 3f;

	private bool _isStarted = false; // 가동 시작 여부 확인용

	private void Awake()
	{
		// 센서니까 Trigger 체크
		if (GetComponent<BoxCollider2D>() != null)
			GetComponent<BoxCollider2D>().isTrigger = true;
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		// 1. 플레이어가 들어왔고, 아직 가동 전이라면
		if (collision.CompareTag("Player") && !_isStarted)
		{
			_isStarted = true; // 가동 스위치 ON (이제 두 번 다시 이 if문은 안 들어옴)
			StartCoroutine(SpawnLoop()); // 무한 생성 루프 시작
		}
	}

	private IEnumerator SpawnLoop()
	{
		// 2. 가동되는 순간부터 게임이 끝날 때까지 무한 반복
		while (true)
		{
			SpawnBoulder();

			// 3. 설정한 시간만큼 정확하게 대기
			yield return new WaitForSeconds(spawnInterval);
		}
	}

	private void SpawnBoulder()
	{
		if (boulderPrefab != null && spawnPoint != null)
		{
			GameObject boulder = Instantiate(boulderPrefab, spawnPoint.position, Quaternion.identity);
			RollingBoulder rbScript = boulder.GetComponent<RollingBoulder>();
			if (rbScript != null)
			{
				Vector2 direction = rollToRight ? Vector2.right : Vector2.left;
				rbScript.Initialize(direction, rollSpeed);
			}
		}
	}
}