using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class StageSelectController : MonoBehaviour
{
	[Header("슬라이드 UI 요소")]
	[SerializeField] private RectTransform chapterContainer; // 3개 챕터 통째로 묶은 부모
	[SerializeField] private Button leftArrowButton;
	[SerializeField] private Button rightArrowButton;
	[SerializeField] private Button backButton;            // 메인메뉴로 돌아가는 뒤로가기 버튼
	[SerializeField] private MainMenuController mainMenuController; // 패널을 다시 켜주기 위한 참조


	[Header("타이틀 설정")]
	[SerializeField] private TextMeshProUGUI titleText;

	[Header("슬라이드 옵션")]
	[SerializeField] private float slideDuration = 0.3f;
	private readonly float chapterWidth = 1920f;

	private int currentChapter = 0; // 0: 평원, 1: 동굴, 2: 화산
	private Coroutine slideCoroutine;

	private void OnEnable()
	{
		// 패널이 켜질 때마다 무조건 첫 번째 챕터(평원)로 초기화
		currentChapter = 0;
		if (chapterContainer != null) chapterContainer.anchoredPosition = Vector2.zero;
		UpdateArrowButtons();
		UpdateTitleColor();
	}

	private void Start()
	{
		if (leftArrowButton != null) leftArrowButton.onClick.AddListener(ClickLeftArrow);
		if (rightArrowButton != null) rightArrowButton.onClick.AddListener(ClickRightArrow);
		if (backButton != null) backButton.onClick.AddListener(ClickBack);
	}

	private void ClickRightArrow()
	{
		if (currentChapter >= 2) return;
		currentChapter++;
		StartSlide();
		UpdateTitleColor();
	}

	private void ClickLeftArrow()
	{
		if (currentChapter <= 0) return;
		currentChapter--;
		StartSlide();
		UpdateTitleColor();
	}

	private void ClickBack()
	{
		if (mainMenuController != null)
		{
			mainMenuController.ClickBackToMenu();
		}
	}

	private void StartSlide()
	{
		if (slideCoroutine != null) StopCoroutine(slideCoroutine);
		float targetX = -currentChapter * chapterWidth;
		slideCoroutine = StartCoroutine(SlideToPosition(targetX));
		UpdateArrowButtons();
	}
	//두 번째 배경(동굴)일 때만 텍스트 색상을 바꾸는 핵심 함수
	private void UpdateTitleColor()
	{
		if (titleText == null) return;

		// currentChapter == 1 이 바로 '동굴 배경'일 때입니다.
		if (currentChapter == 1)
		{
			titleText.color = Color.white; // 동굴에서는 하얀색으로 세팅
		}
		else
		{
			titleText.color = Color.black; // 평원(0)과 화산(2)에서는 원래대로 검은색
		}
	}
	private IEnumerator SlideToPosition(float targetX)
	{
		Vector2 startPos = chapterContainer.anchoredPosition;
		Vector2 targetPos = new Vector2(targetX, startPos.y);
		float elapsedTime = 0f;

		while (elapsedTime < slideDuration)
		{
			elapsedTime += Time.deltaTime;
			chapterContainer.anchoredPosition = Vector2.Lerp(startPos, targetPos, elapsedTime / slideDuration);
			yield return null;
		}
		chapterContainer.anchoredPosition = targetPos;
	}

	private void UpdateArrowButtons()
	{
		if (leftArrowButton != null) leftArrowButton.interactable = (currentChapter > 0);
		if (rightArrowButton != null) rightArrowButton.interactable = (currentChapter < 2);
	}

	// 개별 스테이지 버튼들이 클릭되었을 때 호출할 범용 함수
	public void ClickStageButton(string stageIdentifier)
	{
		Time.timeScale = 1f;
		// 만약 씬 이름이 'Stage1-1' 형태라면 아래처럼 작성합니다.
		string sceneName = "map" + stageIdentifier;
		if (SceneTransitionManager.Instance != null)
		{
			SceneTransitionManager.Instance.FadeOutToLoadingScene(sceneName);
		}
	}
}