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

	// --- [일시정지 변수들] ---
	public bool IsPaused { get; set; } // 현재 멈춰있는가?
	public event Action<bool> OnPauseToggled;  // 멈추거나 풀릴 때 UI에게 알려줄 이벤트

	// 마지막으로 플레이했던 씬의 이름을 기억해 둘 변수
	public string lastPlayedScene { get; private set; } = "";

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);

			// 💡 [추가] 최초 게임 실행 시 세이브 데이터의 기본 판을 짭니다.
			// 만약 "LastPlayedStage" 키가 아예 없다면(게임 최초 실행) 0으로 초기화해 둡니다.
			if (!PlayerPrefs.HasKey("LastPlayedStage"))
			{
				PlayerPrefs.SetInt("LastPlayedStage", 0); // 0은 저장된 기록이 없다는 뜻
				PlayerPrefs.Save();
			}
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

		// 💡 [추가] 플레이어가 스폰되는 실제 게임 맵이므로, 현재 스테이지 번호를 자동 저장합니다!
		string sceneName = SceneManager.GetActiveScene().name;
		if (sceneName.StartsWith("Stage")) // 씬 이름이 "Stage1", "Stage2" 등인 경우
		{
			string stageNumStr = sceneName.Replace("Stage", ""); // "Stage" 글자를 지워 숫지만 남김
			if (int.TryParse(stageNumStr, out int currentStage))
			{
				// 현재 진입한 스테이지 번호를 저장하여 메인 메뉴의 '이어하기'와 연동합니다.
				PlayerPrefs.SetInt("LastPlayedStage", currentStage);
				PlayerPrefs.Save();
				Debug.Log($"[GameManager] 자동 저장 완료! 현재 스테이지: Stage {currentStage}");
			}
		}

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

	// R키 누를 때 매니저가 씬 재시작을 전담하게 만들면 좋습니다.
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

		// "MainMenu"라는 이름의 씬을 불러옵니다.
		SceneManager.LoadScene("MainMenu");
	}
}