using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
	[Header("첫 스테이지 설정")]
	[Tooltip("게임 시작 버튼을 누르면 이동할 첫 번째 씬의 이름을 적어주세요.")]
	public string firstStageName = "Stage_01"; // 본인의 첫 스테이지 씬 이름으로 변경하세요!
	[Header("이어하기 설정")]
	[Tooltip("이어하기 버튼 오브젝트를 연결해주세요. (기록이 없으면 자동으로 숨겨집니다)")]
	public GameObject continueButton;

	private void Start()
	{
		// 1. 씬이 시작될 때, GameManager에 저장된 '마지막 씬' 기록이 있는지 확인합니다.
		if (GameManager.Instance != null && !string.IsNullOrEmpty(GameManager.Instance.lastPlayedScene))
		{
			// 기록이 있다면 '이어서 하기' 버튼을 화면에 띄웁니다!
			if (continueButton != null) continueButton.SetActive(true);
		}
		else
		{
			// 게임을 처음 켰거나 기록이 없다면 버튼을 숨깁니다.
			if (continueButton != null) continueButton.SetActive(false);
		}
	}
	// "게임 시작" 버튼을 눌렀을 때 실행될 함수
	public void Button_StartGame()
	{
		// 새 게임을 시작하면 이전 기록은 지워주는 게 깔끔합니다.
		if (GameManager.Instance != null)
		{
			// (GameManager의 lastPlayedScene을 직접 비워주는 로직이 필요하다면 GameManager에 메서드를 추가할 수도 있습니다. 지금은 덮어씌워지므로 패스!)
		}

		Time.timeScale = 1f;
		SceneManager.LoadScene(firstStageName);
	}
	// [새로 추가된 함수] "이어서 하기" 버튼을 눌렀을 때 실행!
	public void Button_ContinueGame()
	{
		if (GameManager.Instance != null && !string.IsNullOrEmpty(GameManager.Instance.lastPlayedScene))
		{
			Debug.Log($"🚀 하던 곳({GameManager.Instance.lastPlayedScene})으로 돌아갑니다!");
			Time.timeScale = 1f;
			SceneManager.LoadScene(GameManager.Instance.lastPlayedScene);
		}
	}
	// "게임 종료" 버튼을 눌렀을 때 실행될 함수
	public void Button_QuitGame()
	{
		Debug.Log("게임을 종료합니다.");

		// 참고: Application.Quit()은 유니티 에디터 안에서는 작동하지 않고,
		// 나중에 exe 파일로 빌드해서 실행했을 때만 실제로 창이 꺼집니다!
		Application.Quit();
	}
}
