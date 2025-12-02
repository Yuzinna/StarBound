using GameFrameworkLite;
using Unity.Cinemachine;
using UnityEngine;

public class GameInit : MonoBehaviour
{
	// [필수] Inspector 창에서 플레이어 프리팹을 여기에 연결해야 합니다.
	[Tooltip("Hierarchy에 인스턴스화할 플레이어 GameObject 프리팹을 연결하세요.")]
	public GameObject playerPrefab;

	// 이 변수에 생성된 플레이어 인스턴스를 저장하여 카메라 연결에 사용합니다.
	private GameObject _playerInstance;
	void Start()
	{
		Init();
	}
	public void Init()
	{
		// 1. 플레이어 프리팹 유효성 검사
		if (playerPrefab == null)
		{
			Debug.LogError("[GameInit] 플레이어 프리팹(playerPrefab)이 Inspector에 연결되지 않았습니다! 초기화 중단.");
			return;
		}

		// 2. PlayerStart 오브젝트를 찾아 스폰 위치 결정
		var startObj = GameObject.FindWithTag("PlayerStart");
		Vector3 spawnPos;

		if (startObj == null)
		{
			Debug.LogWarning("[GameInit] 'PlayerStart' 태그를 가진 오브젝트를 찾을 수 없습니다. (0, 1, 0)에 생성합니다.");
			spawnPos = Vector3.up; // (0, 1, 0)
		}
		else
		{
			// 요구 사항: PlayerStart 위치 + Vector3.up (지면에서 약간 위)
			spawnPos = startObj.transform.position + Vector3.up;
		}

		// 3. **플레이어 오브젝트 인스턴스화 및 위치 설정**
		// 인스턴스화된 오브젝트를 _playerInstance 변수에 저장합니다.
		_playerInstance = Instantiate(playerPrefab, spawnPos, Quaternion.identity);

		// 이 매니저 오브젝트의 위치가 아닌, 플레이어 오브젝트의 위치를 설정합니다.
		// _playerInstance.transform.position = spawnPos; // Instantiate 시점에 위치가 설정되었으므로 생략 가능

		Debug.Log($"[GameInit] 플레이어 오브젝트를 {spawnPos} 위치에 인스턴스화 완료.");

		// 4. Cinemachine 카메라 연결 (인스턴스화된 플레이어의 Transform 전달)
		SetupCamera(_playerInstance.transform);

		// 참고: 기존 코드의 PlayerPrefabPath 변수는 더 이상 사용하지 않습니다.
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
