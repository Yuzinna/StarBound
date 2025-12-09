using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[System.Serializable]
public class ButtonVisuals
{
	public Button targetButton;
	public Image targetImage;
}
public class PauseUI : MonoBehaviour
{
	[Header("UI 연결")]
	[Tooltip("PausePanel 오브젝트를 연결하세요.")]
	[SerializeField] private GameObject pausePanel;

	
	[Header("버튼 비주얼 설정")]
	[Tooltip("재개 및 메인 메뉴 버튼의 이미지를 각각 설정하세요.")]
	[SerializeField] private ButtonVisuals resumeButtonVisuals;
	[SerializeField] private ButtonVisuals mainMenuButtonVisuals;

	private bool isPaused = false;
	private const string START_SCENE_NAME = "StartScene"; // 시작 씬 이름

	// 내비게이션 관리
	private ButtonVisuals[] buttons;
	private int selectedButtonIndex = 0;


	private void Awake()
	{
		// 관리할 버튼 리스트를 초기화합니다.
		buttons = new ButtonVisuals[] { resumeButtonVisuals, mainMenuButtonVisuals };
	}
	void Start()
	{
		// 씬 시작 시 Pause UI를 비활성화 상태로 시작합니다.
		if (pausePanel != null)
		{
			pausePanel.SetActive(false);
		}
		// 게임이 시작될 때 timeScale이 1로 설정되어 있는지 확인합니다.
		Time.timeScale = 1f;
		InputManager.Instance._plInput.PauseAction+= PauseGame;
	}
	
	/// <summary>
	/// UIInput의 OnSubmit(스페이스바) 이벤트에서 호출되어 현재 선택된 버튼을 실행합니다.
	/// </summary>
	public void ExecuteSelectedButton()
	{
		if (!isPaused) return;

		// 현재 선택된 인덱스의 버튼 객체를 가져옵니다.
		Button selectedButton = buttons[selectedButtonIndex].targetButton;

		// Unity UI Button 컴포넌트의 OnClick() 이벤트를 수동으로 발동시킵니다.
		selectedButton.onClick.Invoke();
	}
	/// <summary>
	/// UIInput의 OnNavigate(W/S) 이벤트에서 호출되어 버튼 선택 인덱스를 변경하고 이미지를 업데이트합니다.
	/// </summary>
	/// <param name="verticalInput">W(Up)이면 양수, S(Down)이면 음수 값</param>
	public void NavigateButtons(float verticalInput)
	{
		if (!isPaused) return;

		int newIndex = selectedButtonIndex;

		if (verticalInput < -0.5f) // 아래로 이동 (S)
		{
			newIndex = (selectedButtonIndex + 1) % buttons.Length;
		}
		else if (verticalInput > 0.5f) // 위로 이동 (W)
		{
			newIndex = (selectedButtonIndex - 1 + buttons.Length) % buttons.Length;
		}

		if (newIndex != selectedButtonIndex)
		{
			UpdateSelection(newIndex);
		}
	}
	public void PauseGame()
	{
		if (pausePanel != null)
		{
			pausePanel.SetActive(true);
		}
		Time.timeScale = 0f; // 게임 시간 정지
		isPaused = true;

		UpdateButtonVisuals(resumeButtonVisuals, true);
		UpdateButtonVisuals(mainMenuButtonVisuals, false);
		if (InputManager.Instance != null)
		{
			InputManager.Instance.SwitchToUI();
		}
	}
	public void ResumeGame()
	{
		if (pausePanel != null)
		{
			pausePanel.SetActive(false);
		}
		Time.timeScale = 1f; // 게임 시간 재개
		isPaused = false;
		if (InputManager.Instance != null)
		{
			InputManager.Instance.SwitchToGameplay();
		}
	}
	/// <summary>
	/// 일시정지를 해제하고 메인 메뉴 씬으로 돌아갑니다. (MainMenuButton에 연결)
	/// </summary>
	public void GoToMainMenu()
	{
		// 씬 전환 전 게임 시간을 다시 흐르게 합니다. (필수)
		Time.timeScale = 1f;

		// SceneTransitionManager를 통해 StartScene으로 전환 요청
		if (SceneTransitionManager.Instance != null)
		{
			SceneTransitionManager.Instance.LoadNextScene(START_SCENE_NAME);
		}
		else
		{
			// 비상시 바로 로드
			SceneManager.LoadScene(START_SCENE_NAME);
		}
	}
	private void UpdateSelection(int newIndex)
	{
		// 1. 이전 버튼의 비주얼을 비선택 상태로 되돌립니다.
		UpdateButtonVisuals(buttons[selectedButtonIndex], false);

		// 2. 인덱스 업데이트
		selectedButtonIndex = newIndex;

		// 3. 새 버튼의 비주얼을 선택 상태로 업데이트합니다.
		UpdateButtonVisuals(buttons[selectedButtonIndex], true);
	}
	/// <summary>
	/// ButtonVisuals 구조체 정보를 사용하여 버튼의 스프라이트를 업데이트합니다.
	/// </summary>
	private void UpdateButtonVisuals(ButtonVisuals buttonVisuals, bool isSelected)
	{
		if (buttonVisuals.targetButton != null && buttonVisuals.targetImage != null)
		{
			if(isSelected)
			{
				buttonVisuals.targetButton.gameObject.SetActive(true);
				buttonVisuals.targetImage.gameObject.SetActive(false);
			}
			else
			{
				buttonVisuals.targetButton.gameObject.SetActive(false);
				buttonVisuals.targetImage.gameObject.SetActive(true);
			}
			// ButtonVisuals에 정의된 고유 이미지를 사용
			//buttonVisuals.targetButton.image.sprite = isSelected ? buttonVisuals.selectedSprite : buttonVisuals.unselectedSprite;
		}
	}
}
