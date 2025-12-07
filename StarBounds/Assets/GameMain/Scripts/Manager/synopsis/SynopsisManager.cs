using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
[System.Serializable]
public class SynopsisPage
{
	// ✨ 페이지에 표시할 스프라이트 (이미지)
	[Tooltip("표시할 배경 이미지 또는 일러스트")]
	public Sprite image;

	// ✨ 페이지에 표시할 텍스트 내용
	[Tooltip("페이지에 표시할 설명 텍스트")]
	[TextArea(3, 5)]
	public string text;
}
public class SynopsisManager : MonoBehaviour
{
	// ✨ 인스펙터에서 시놉시스 UI 패널을 연결합니다.
	[Header("UI 연결")]
	[Tooltip("시놉시스 내용을 담고 있는 화면 전체 패널을 연결하세요.")]
	[SerializeField] private GameObject synopsisPanel;

	// ✨ 페이지 이미지를 표시할 UI Image 컴포넌트
	[Tooltip("페이지 이미지를 표시할 UI Image 컴포넌트")]
	[SerializeField] private Image pageImage;

	// ✨ 페이지 텍스트를 표시할 UI Text 컴포넌트 (TextMeshPro를 사용하는 경우 해당 컴포넌트로 변경하세요)
	[Tooltip("페이지 텍스트를 표시할 UI Text 컴포넌트")]
	[SerializeField] private TextMeshProUGUI pageText;

	// ✨ 모든 시놉시스 페이지 데이터 (Inspector에서 설정)
	[Header("데이터 설정")]
	[Tooltip("시놉시스 페이지 순서대로 데이터를 입력하세요.")]
	[SerializeField] private SynopsisPage[] synopsisPages;

	// ✨ 시퀀스가 끝난 후 로드할 다음 맵 씬의 이름
	[SerializeField] private string nextLevelSceneName = "Level_01";

	// 현재 페이지 인덱스
	[SerializeField] private int currentPageIndex = 0;

	// GameInit에서 전달받은 콜백 함수(Fade In 시작)를 저장하는 변수
	private Action onSynopsisFinishedCallback;

	// ==========================================================
	// Lifecycle & Initialization
	// ==========================================================

	private void Awake()
	{
		

	}
	void Start()
	{
		// 1. SceneTransitionManager에게 Fade In을 요청하여 화면을 밝게 만듭니다.
		if (SceneTransitionManager.Instance != null)
		{
			SceneTransitionManager.Instance.StartFadeIn();
		}

		// 2. 시놉시스 패널을 활성화합니다.
		if (synopsisPanel != null)
		{
			synopsisPanel.SetActive(true);
		}
		AdvancePage();
		Debug.Log("[SynopsisManager] 시놉시스 재생 시작.");
	}
	void Update()
	{
		// 스페이스바 입력 감지
		if (Input.GetKeyDown(KeyCode.Space))
		{
			AdvancePage();
		}
	}
	/// <summary>
	/// 현재 인덱스의 이미지와 텍스트를 UI에 표시합니다.
	/// </summary>
	private void DisplayCurrentPage()
	{
		if (synopsisPages == null || synopsisPages.Length == 0)
		{
			Debug.LogError("[SynopsisManager] 시놉시스 페이지 데이터가 없습니다. 즉시 종료합니다.");
			OnSynopsisEnd();
			return;
		}

		if (currentPageIndex >= 0 && currentPageIndex < synopsisPages.Length)
		{
			SynopsisPage page = synopsisPages[currentPageIndex++];

			// 이미지 업데이트
			if (pageImage != null && page.image != null)
			{
				pageImage.sprite = page.image;
			}

			// 텍스트 업데이트
			if (pageText != null)
			{
				pageText.text = page.text;
			}
			Debug.Log($"[SynopsisManager] 페이지 {currentPageIndex + 1}/{synopsisPages.Length} 표시.");
		}
	}
	/// <summary>
	/// 스페이스바 입력 시 다음 페이지로 이동하거나 시퀀스를 종료합니다.
	/// </summary>
	private void AdvancePage()
	{
		//currentPageIndex++;

		// 1. 마지막 페이지인지 확인
		if (currentPageIndex < synopsisPages.Length)
		{
			// 다음 페이지 표시
			DisplayCurrentPage();
		}
		else
		{
			// 2. 시퀀스 종료 처리
			OnSynopsisEnd();
		}
	}
	/// <summary>
	/// 시퀀스가 완전히 끝났을 때 호출되며, 페이드 아웃 후 다음 씬으로 전환합니다.
	/// </summary>
	public void OnSynopsisEnd()
	{
		Debug.Log("[SynopsisManager] 시놉시스 종료. 다음 레벨로 전환 시작.");

		// 1. 시놉시스 UI 비활성화
		if (synopsisPanel != null)
		{
			//synopsisPanel.SetActive(false);
		}

		// 2. ✨ Fade Out 후 다음 씬 로드 요청 ✨
		if (SceneTransitionManager.Instance != null)
		{
			// SceneTransitionManager에게 Fade Out과 Level_01 로드를 동시에 요청합니다.
			SceneTransitionManager.Instance.LoadNextScene(nextLevelSceneName);
		}
		else
		{
			// 비상시 바로 씬 로드
			SceneManager.LoadScene(nextLevelSceneName);
		}

		// 시놉시스 매니저의 역할은 여기서 끝납니다.
	}
	
}