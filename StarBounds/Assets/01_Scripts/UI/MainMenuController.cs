using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
	[Header("버튼 연결")]
	[SerializeField] private Button newGameButton;
	[SerializeField] private Button continueButton;
	[SerializeField] private Button stageSelectButton;
	[SerializeField] private Button exitButton;



	[Header("연결할 패널 (기믹 구현용 잠금장치)")]
	[SerializeField] private GameObject mainMenuPanel;    // 💡 메인 메뉴 패널 추가
	[SerializeField] private GameObject stageSelectPanel; // 기존 스테이지 선택 패널
	private void Start()
	{
		// 버튼 이벤트 연결
		if (newGameButton != null) newGameButton.onClick.AddListener(ClickNewGame);
		if (continueButton != null) continueButton.onClick.AddListener(ClickContinue);
		if (stageSelectButton != null) stageSelectButton.onClick.AddListener(ClickStageSelect);
		if (exitButton != null) exitButton.onClick.AddListener(ClickExit);
		if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
		if (stageSelectPanel != null) stageSelectPanel.SetActive(false);

		// 이어하기 버튼 활성화 세팅
		if (continueButton != null)
		{
			int lastSavedStage = PlayerPrefs.GetInt("LastPlayedStage", 0);
			continueButton.interactable = (lastSavedStage > 0);
		}

		if (stageSelectPanel != null) stageSelectPanel.SetActive(false);
	}

	//[새로시작] 처음부터 시작
	private void ClickNewGame()
	{
		Time.timeScale = 1f;

		// 세이브 데이터 초기화 (1스테이지부터)
		PlayerPrefs.SetInt("LastPlayedStage", 1);
		PlayerPrefs.Save();

		// 💡 [유저님 스크립트 활용] 페이드 아웃 후 로딩씬을 거쳐 시놉시스로 이동
		if (SceneTransitionManager.Instance != null)
		{
			SceneTransitionManager.Instance.FadeOutToLoadingScene("Synopsis");
		}
	}

	//[이어하기] 마지막 스테이지부터 시작
	private void ClickContinue()
	{
		Time.timeScale = 1f;

		int lastSavedStage = PlayerPrefs.GetInt("LastPlayedStage", 1);

		// 💡 [유저님 스크립트 활용] 페이드 아웃 후 로딩씬을 거쳐 저장된 스테이지로 이동
		if (SceneTransitionManager.Instance != null)
		{
			SceneTransitionManager.Instance.FadeOutToLoadingScene("Stage" + lastSavedStage);
		}
	}

	//[스테이지 선택] 패널 열기
	private void ClickStageSelect()
	{
		if (stageSelectPanel != null)
		{
			if (mainMenuPanel != null) mainMenuPanel.SetActive(false); // 메인 메뉴 끄기
			if (stageSelectPanel != null) stageSelectPanel.SetActive(true); // 선택창 켜기
		}
	}
	
	public void ClickBackToMenu()
	{
		if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
		if (stageSelectPanel != null) stageSelectPanel.SetActive(false);
	}
	//[추후 활용] 나중에 만들 슬라이드식 스테이지 셀렉트 창에서 버튼을 누를 때 호출할 함수
	public void ClickSelectStageButton(int stageNumber)
	{
		Time.timeScale = 1f;

		if (SceneTransitionManager.Instance != null)
		{
			SceneTransitionManager.Instance.FadeOutToLoadingScene("Stage" + stageNumber);
		}
	}

	//[게임 종료]
	private void ClickExit()
	{
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
	}
}