using UnityEngine;
using UnityEngine.SceneManagement;
public static class GameSceneSaver
{
	// PlayerPrefs에 씬 이름을 저장할 때 사용할 키
	private static string _lastPlayedSceneName = "";

	private const string StartSceneName = "StartScene";

	/// <summary>
	/// 현재 씬의 이름을 임시 저장소에 저장합니다. (게임이 종료되면 초기화됨)
	/// </summary>
	public static void SaveCurrentSceneForSession()
	{
		string currentSceneName = SceneManager.GetActiveScene().name;

		// 시작 씬으로 돌아갈 때만 저장: 
		// 씬 전환이 발생하기 직전인 PauseUI.GoToMainMenu()에서 호출됩니다.
		if (currentSceneName != StartSceneName)
		{
			_lastPlayedSceneName = currentSceneName;
			Debug.Log($"[SessionSave] 현재 세션 임시 씬 저장 완료: {_lastPlayedSceneName}");
		}
	}


	/// <summary>
	/// 임시 저장된 씬 이름을 불러옵니다.
	/// </summary>
	public static string LoadSavedSceneName()
	{
		// PlayerPrefs 대신 정적 필드 값을 반환
		return _lastPlayedSceneName;
	}

	/// <summary>
	/// 현재 세션에 저장 데이터가 있는지 확인합니다.
	/// </summary>
	public static bool HasSavedData()
	{
		return !string.IsNullOrEmpty(_lastPlayedSceneName);
	}

	/// <summary>
	/// 임시 저장된 데이터를 명시적으로 지웁니다. (예: 새 게임 시작 시 호출)
	/// </summary>
	public static void ClearSessionSave()
	{
		_lastPlayedSceneName = "";
		Debug.Log("[SessionSave] 임시 저장 데이터가 초기화되었습니다.");
	}
}
