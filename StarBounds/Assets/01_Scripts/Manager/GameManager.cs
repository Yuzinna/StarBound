using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
	MainMenu,   // 메인 메뉴 화면
	Playing,    // 게임 플레이 중
	Paused,     // 일시 정지됨
	StageClear, // 스테이지 클리어 연출 중
	GameOver    // 사망 연출 중
}
public class GameManager : MonoBehaviour
{
	public static GameManager Instance { get; private set; }

	[Header("플레이어 설정")]
	public GameObject playerPrefab;
	private GameObject _playerInstance;

	// --- [새로 추가된 일시정지 변수들] ---
	public bool IsPaused { get; private set; } // 현재 멈춰있는가?
	public event Action<bool> OnPauseToggled;  // 멈추거나 풀릴 때 UI에게 알려줄 이벤트

	// [추가] 마지막으로 플레이했던 씬의 이름을 기억해 둘 변수
	public string lastPlayedScene { get; private set; } = "";
	private void Awake()
	{
		if(Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);

		}
		else
		{
			Destroy(gameObject);
		}
	}
	private void OnEnable()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnDisable()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}
	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		// 씬이 켜질 때마다 플레이어를 스폰하고 카메라를 세팅합니다.
		SpawnPlayerAndSetupCamera();
	}
	private void SpawnPlayerAndSetupCamera()
	{
		if (playerPrefab == null) return;

		// 3. 스타트 지점 찾기
		GameObject startObj = GameObject.FindWithTag("PlayerStart");
		if (startObj == null) return; // 스타트 지점이 없는 씬(예: 메인화면)이면 무시

		Vector3 spawnPos = startObj.transform.position + Vector3.up;

		// 4. 플레이어 생성 또는 위치 이동
		if (_playerInstance == null)
		{
			_playerInstance = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
		}
		else
		{
			_playerInstance.transform.position = spawnPos;

			// 재시작 시 이전 관성 때문에 날아가는 것 방지
			Rigidbody2D rb = _playerInstance.GetComponent<Rigidbody2D>();
			if (rb != null) rb.linearVelocity = Vector2.zero;
		}
		SetupCamera(_playerInstance.transform);
	}
	//public void InitGame()
	//{
	//	if (playerPrefab == null)
	//	{
	//		Debug.LogError("[GameManager] 플레이어 프리팹이 없습니다!");
	//		return;
	//	}

	//	var startObj = GameObject.FindWithTag("PlayerStart");
	//	if (startObj == null)
	//	{
	//		Debug.LogWarning("[GameManager] PlayerStart 태그를 찾을 수 없어 플레이어를 소환하지 않습니다. (UI 씬일 수 있음)");
	//		return;
	//	}

	//	Vector3 spawnPos = startObj.transform.position + Vector3.up;

	//	if (_playerInstance == null)
	//	{
	//		_playerInstance = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
	//	}
	//	else
	//	{
	//		_playerInstance.transform.position = spawnPos;

	//		// 물리 관성 초기화 (재시작 시 날아가는 버그 방지)
	//		Rigidbody2D rb = _playerInstance.GetComponent<Rigidbody2D>();
	//		if (rb != null) rb.linearVelocity = Vector2.zero;
	//	}

	//	SetupCamera(_playerInstance.transform);
		
	//}
	
	private void SetupCamera(Transform playerTransform)
	{
		CinemachineCamera virtualCam = GameObject.FindAnyObjectByType<CinemachineCamera>();
		if (virtualCam != null)
		{
			virtualCam.Follow = playerTransform;
			virtualCam.LookAt = playerTransform;
		}
	}

	public void TogglePause()
	{
		IsPaused = !IsPaused; // 상태 뒤집기 (true -> false, false -> true)

		// 유니티의 시간을 멈추거나(0) 정상 속도로 돌립니다(1)
		Time.timeScale = IsPaused ? 0f : 1f;

		// UI 매니저 등에게 "일시정지 상태가 변했어!" 라고 방송을 냅니다.
		OnPauseToggled?.Invoke(IsPaused);

		Debug.Log(IsPaused ? "게임 일시 정지" : "게임 재개");
	}
	// (보너스) R키 누를 때 매니저가 씬 재시작을 전담하게 만들면 좋습니다.
	public void RestartCurrentStage()
	{
		// 씬을 다시 부를 때는 반드시 시간을 원상복구(1f) 해줘야 합니다!
		Time.timeScale = 1f;
		IsPaused = false;
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}
	public void LoadMainMenu()
	{
		//메인 메뉴로 넘어가기 직전에, 현재 씬의 이름을 매니저 수첩에 적어둡니다!
		lastPlayedScene = SceneManager.GetActiveScene().name;

		// 일시정지 중에 씬을 넘어가면 다음 씬에서도 시간이 멈춰있습니다
		// 그래서 넘어가기 전에 반드시 시간을 정상(1f)으로 돌려놓고, 일시정지를 풀어야 합니다.
		Time.timeScale = 1f;
		IsPaused = false;

		// "MainMenu"라는 이름의 씬을 불러옵니다. (나중에 씬 이름을 이렇게 지어주세요!)
		SceneManager.LoadScene("MainMenu");
	}
}
