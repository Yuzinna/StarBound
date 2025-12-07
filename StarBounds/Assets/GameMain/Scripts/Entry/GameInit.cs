
using System;
using Unity.Cinemachine;
using UnityEngine;

public class GameInit : MonoBehaviour
{
	// [필수] Inspector 창에서 플레이어 프리팹을 여기에 연결해야 합니다.
	[Tooltip("Hierarchy에 인스턴스화할 플레이어 GameObject 프리팹을 연결하세요.")]

	//1. 싱글톤 인스턴스: 씬이 바뀌어도 이 오브젝트의 로직을 사용할 수 있도록 합니다.
	public static GameInit Instance { get; private set; }



	public GameObject playerPrefab;
	// 이 변수에 생성된 플레이어 인스턴스를 저장하여 카메라 연결에 사용합니다.
	private GameObject _playerInstance;

	// 2. 초기화 완료 이벤트: 외부에서 초기화가 끝난 시점을 알 수 있도록 합니다.
	public event Action OnInitCompleted;

	private void Awake()
	{
		// 3. 싱글톤 및 DDOL 처리
		if (Instance == null)
		{
			Instance = this;
			// 씬이 바뀌어도 파괴되지 않게 하여 어디서든 접근 가능하게 합니다.
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			// 이미 존재하면 새 인스턴스를 파괴합니다.
			Destroy(gameObject);
			return; // 이후 코드가 실행되지 않도록 즉시 종료
		}
	}
	void Start()
	{
		Debug.Log("[GameInit] 최초 씬 시작 감지. InitAndFadeIn() 호출.");
		// ✨ 최초 시작 시, 초기화 및 Fade In을 즉시 실행
		InitGame();
		StartFadeIn();
	}
	// 씬 로드 후 초기화와 Fade In을 시작하는 진입점
	
	public void InitGame()
	{
		// 1. 플레이어 프리팹 유효성 검사
		if (playerPrefab == null)
		{
			Debug.LogError("[GameInit] 플레이어 프리팹(playerPrefab)이 Inspector에 연결되지 않았습니다! 초기화 중단.");
			return;
		}

		// 2. PlayerStart 오브젝트를 찾아 스폰 위치 결정
		var startObj = GameObject.FindWithTag("PlayerStart");
		if (startObj == null)
		{
			return; //스타트 지점이 없으면 실행 중지
		}
		Vector3 spawnPos = (startObj != null) ? startObj.transform.position + Vector3.up : Vector3.up;


		// 2. **플레이어 오브젝트 인스턴스화**
		// 씬 전환 시 플레이어가 DDOL이 아니라면, 새 씬마다 재스폰합니다.
		if (_playerInstance == null)
		{
			_playerInstance = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
		}
		else
		{
			_playerInstance.transform.position = spawnPos;
		}
		// 이 매니저 오브젝트의 위치가 아닌, 플레이어 오브젝트의 위치를 설정합니다.
		// _playerInstance.transform.position = spawnPos; // Instantiate 시점에 위치가 설정되었으므로 생략 가능

		Debug.Log($"[GameInit] 플레이어 오브젝트를 {spawnPos} 위치에 인스턴스화 완료.");

		// 4. Cinemachine 카메라 연결 (인스턴스화된 플레이어의 Transform 전달)
		SetupCamera(_playerInstance.transform);

		// 참고: 기존 코드의 PlayerPrefabPath 변수는 더 이상 사용하지 않습니다.


	}
	// ==========================================================
	// Fade In 함수 (StartFadeIn으로 간결하게 변경)
	// ==========================================================
	/// <summary>
	/// 초기화 완료 후 SceneTransitionManager를 통해 화면을 밝힙니다.
	/// </summary>
	public void StartFadeIn()
	{
		if (SceneTransitionManager.Instance != null)
		{
			SceneTransitionManager.Instance.StartFadeIn();
		}
		else
		{
			Debug.LogWarning("[GameInit] SceneTransitionManager를 찾을 수 없어 Fade In을 건너뜁니다.");
		}
	}
	private void SetupCamera(Transform playerTransform)
	{
		// 씬 내의 CinemachineCamera를 찾습니다.
		CinemachineCamera virtualCam = GameObject.FindAnyObjectByType<CinemachineCamera>();

		if (virtualCam != null)
		{
			virtualCam.Follow = playerTransform;
			virtualCam.LookAt = playerTransform;
			Debug.Log("[PlayerStarter] Cinemachine Camera를 플레이어에 연결 완료.");
		}
		else
		{
			Debug.LogWarning("[PlayerStarter] CinemachineCamera 오브젝트를 찾을 수 없습니다! 카메라 추적 기능이 비활성화됩니다.");
		}
	}
}
