using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class PauseUI : MonoBehaviour
{
	public GameObject pausePanel;
	private void Start()
	{
		if (pausePanel != null)
			pausePanel.SetActive(false);
		// GameManager의 일시정지 방송(Event)을 구독합니다!
		if (GameManager.Instance != null)
		{
			GameManager.Instance.OnPauseToggled += UpdateUIVisibility;
		}
	}
	private void OnDestroy()
	{
		// UI가 파괴될 때는 방송 구독을 꼭 취소해야 에러가 안 납니다.
		if (GameManager.Instance != null)
		{
			GameManager.Instance.OnPauseToggled -= UpdateUIVisibility;
		}
	}
	// 매니저가 TogglePause를 부를 때마다 이 함수가 자동으로 실행됩니다!
	private void UpdateUIVisibility(bool isPaused)
	{
		if (pausePanel != null)
		{
			pausePanel.SetActive(isPaused);
		}
	}
	// 1. "계속하기" 버튼용
	public void Button_ResumeGame()
	{
		if (GameManager.Instance != null)
		{
			GameManager.Instance.TogglePause(); // 일시정지를 다시 누른 것과 같은 효과!
		}
	}

	// 2. "메인 화면으로" 버튼용
	public void Button_GoToMainMenu()
	{
		if (GameManager.Instance != null)
		{
			GameManager.Instance.LoadMainMenu(); // 매니저에게 메인 화면으로 가라고 명령!
		}
	}
}
