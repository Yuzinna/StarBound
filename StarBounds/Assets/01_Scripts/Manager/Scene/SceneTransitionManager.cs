using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
	public static SceneTransitionManager Instance { get; private set; }

	[Header("UI 설정")]
	[SerializeField] private GameObject fadeOverlayPrefab; // 💡 여기에 'FadeUI' 프리팹을 연결하세요
	[SerializeField] private float fadeDuration = 0.5f;

	private CanvasGroup fadeCanvasGroup;

	private void Awake()
	{
		if (Instance != null) { Destroy(gameObject); return; }
		Instance = this;
		DontDestroyOnLoad(gameObject);

		// UI 먼저 생성
		GameObject fadeObj = Instantiate(fadeOverlayPrefab);
		DontDestroyOnLoad(fadeObj);
		fadeCanvasGroup = fadeObj.GetComponentInChildren<CanvasGroup>();
		fadeCanvasGroup.alpha = 0f;
		fadeCanvasGroup.blocksRaycasts = false;

		// 구독 및 초기 실행
		SceneManager.sceneLoaded += OnSceneLoaded;
		OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
	}

	private void OnEnable()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
	}
	private void OnDisable()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	} 

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		// 씬이 로드되면 무조건 페이드 인(밝아짐)
		StartCoroutine(Fade(1f, 0f));
	}

	// 외부에서 호출할 함수들 (로직은 동일)
	public void FadeOutToLoadingScene(string targetSceneName)
	{
		StartCoroutine(FadeOutAndLoadWithManager(targetSceneName));
	}

	private IEnumerator FadeOutAndLoadWithManager(string targetSceneName)
	{
		yield return StartCoroutine(Fade(0f, 1f));
		SimpleLoadingManager.LoadScene(targetSceneName);
	}

	private IEnumerator Fade(float startAlpha, float endAlpha)
	{
		float elapsedTime = 0f;
		fadeCanvasGroup.alpha = startAlpha;
		fadeCanvasGroup.blocksRaycasts = true;

		while (elapsedTime < fadeDuration)
		{
			elapsedTime += Time.deltaTime;
			fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / fadeDuration);
			yield return null;
		}

		fadeCanvasGroup.alpha = endAlpha;
		if (endAlpha == 0f) fadeCanvasGroup.blocksRaycasts = false;
	}
}