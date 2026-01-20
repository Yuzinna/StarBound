using System.Transactions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartUI : MonoBehaviour
{
	[SerializeField]
	private string nextSceneName = "SynopsisScene"; // 시놉시스 씬 이름을 여기에 입력합니다.

	public ButtonVisuals StartButton;
	public ButtonVisuals ContinueButton;
	public ButtonVisuals ExitButton;
	private ButtonVisuals[] buttons;

	private SceneTransitionManager transitionManager;
	private int selectedButtonIndex = 0;
	private void Awake()
	{
		
	}
	void Start()
	{
		buttons = new ButtonVisuals[] { StartButton, ContinueButton, ExitButton };
		// 씬 시작 시 초기 비주얼 업데이트 (첫 번째 버튼 선택)
		//UpdateSelection(selectedButtonIndex);
		UpdateButtonVisuals(buttons[selectedButtonIndex], true);
		UpdateButtonVisuals(buttons[1], false);
		UpdateButtonVisuals(buttons[2], false);
		
		// SceneTransitionManager가 DDOL로 설정되어 있다면 인스턴스를 가져옵니다.
		transitionManager = SceneTransitionManager.Instance;

		// ✨ (선택 사항) 최초 시작 시 Fade In 처리:
		// 타이틀 씬에 들어왔을 때 화면이 검은색이면 안 되므로, 
		// SceneTransitionManager에게 Fade In을 요청하여 화면을 밝게 만듭니다.
		if (transitionManager != null)
		{
			transitionManager.StartFadeIn();
		}
		//인풋을 플레이어와 게임맵 ui부분을 끄기
		InputManager.Instance.SwitchToUI();
	}
	public void NavigateButtons(float verticalInput)
	{
		int newIndex = selectedButtonIndex;

		// 위(양수) 또는 아래(음수) 입력 임계값 설정
		if (verticalInput < -0.5f) // 아래로 이동
		{
			// 배열 끝에서 처음으로 순환
			newIndex = (selectedButtonIndex + 1) % buttons.Length;
		}
		else if (verticalInput > 0.5f) // 위로 이동
		{
			// 배열 처음에서 끝으로 순환 (음수 순환을 위한 + buttons.Length)
			newIndex = (selectedButtonIndex - 1 + buttons.Length) % buttons.Length;
		}

		if (newIndex != selectedButtonIndex)
		{
			UpdateSelection(newIndex);
		}
	}
	/// <summary>
	/// OnSubmit에서 호출되어 현재 선택된 버튼에 연결된 로직을 실행합니다.
	/// </summary>
	public void ExecuteSelectedButton()
	{
		
        switch(selectedButtonIndex)
        {
            case 0: 
				StartNewGame();
				
				break;
            case 1: 
				LoadLastSave();

				break;
            case 2: 
				ExitGame();
				
				break;
        }
        
	}
	public void StartNewGame()
	{
		Debug.Log("새 게임 시작!");
		// 씬 전환 전 UI 입력 끄기 (선택 사항, 전환 로직에서 처리 권장)
		// InputManager.Instance?.SwitchToGameplay(); 
		if (SceneTransitionManager.Instance != null)
		{
			SceneTransitionManager.Instance.LoadNextScene(nextSceneName);
		}
	}
	public void LoadLastSave()
	{
		Debug.Log("이전에 플레이했던 맵부터 시작!");
		string lastSceneName = GameSceneSaver.LoadSavedSceneName();
		if (!string.IsNullOrEmpty(lastSceneName))
		{
			Debug.Log($"[StartUI] 이어서 하기: 마지막 씬 ({lastSceneName}) 로드.");

			// 씬 로드 전 UI 입력 끄기 (InputManager가 처리할 수도 있음)
			// InputManager.Instance?.SwitchToGameplay(); 
			SceneTransitionManager.Instance.LoadNextScene(lastSceneName);
			GameSceneSaver.ClearSessionSave();
		}
		else
		{
			Debug.LogWarning("[StartUI] 저장된 게임 데이터가 없습니다. 새 게임을 시작하세요.");
			// 저장 데이터가 없으면 새 게임 버튼 로직을 실행하거나 사용자에게 알림.
			StartNewGame();
		}
	}
	public void ExitGame()
	{
		Debug.Log("게임 종료!");
		Application.Quit();
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false; // 에디터에서 테스트 용
#endif
	}
	private void UpdateSelection(int newIndex)
	{
		// 1. 이전 버튼 비선택
		UpdateButtonVisuals(buttons[selectedButtonIndex], false);

		// 2. 인덱스 업데이트
		selectedButtonIndex = newIndex;

		// 3. 새 버튼 선택
		UpdateButtonVisuals(buttons[selectedButtonIndex], true);
	}
	private void UpdateButtonVisuals(ButtonVisuals buttonVisuals, bool isSelected)
	{
		// PauseUI와 동일하게 비주얼을 제어하는 로직
		if (buttonVisuals.targetButton != null && buttonVisuals.targetImage != null)
		{
			buttonVisuals.targetButton.gameObject.SetActive(isSelected);
			buttonVisuals.targetImage.gameObject.SetActive(!isSelected);
		}
	}
}
