using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
	public static SceneTransitionManager Instance { get; private set; }

	[Header("UI 요소")]
	[SerializeField] private CanvasGroup fadeCanvasGroup;
	[SerializeField] private float fadeDuration = 0.5f;

	private void Awake()
	{
		if (Instance != null) { Destroy(gameObject); return; }
		Instance = this;
		DontDestroyOnLoad(gameObject);
	}

	// 💡 오브젝트가 활성화될 때 유니티의 '씬 로드 이벤트'를 구독합니다.
	private void OnEnable()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	// 💡 오브젝트가 비활성화될 때 구독을 해제합니다 (메모리 누수 방지).
	private void OnDisable()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	// ➡️ 유니티가 새 씬을 로드 완료하면 '무조건' 이 함수를 실행해줍니다!
	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		// 새 씬이 켜졌으니 화면을 서서히 밝게 만듭니다 (페이드 인)
		StartCoroutine(Fade(1f, 0f));
	}

	/// <summary>
	/// 외부(ClearDoor 등)에서 화면을 어둡게 만들고 싶을 때 호출하는 함수
	/// </summary>
	public void FadeOutToScene(string sceneName)
	{
		StartCoroutine(FadeOutAndLoad(sceneName));
	}

	private IEnumerator FadeOutAndLoad(string sceneName)
	{
		// 1. 화면을 서서히 어둡게 만듭니다.
		yield return StartCoroutine(Fade(0f, 1f));

		// 2. 로딩 씬(또는 다음 씬)을 불러옵니다.
		// 씬이 로드 완료되면 위의 OnSceneLoaded가 자동으로 발동하여 페이드 인을 해줍니다!
		SceneManager.LoadScene(sceneName);
	}

	/// <summary>
	/// 화면을 페이드 아웃한 뒤, SimpleLoadingManager를 통해 로딩 씬으로 안전하게 이동합니다.
	/// </summary>
	public void FadeOutToLoadingScene(string targetSceneName)
	{
		StartCoroutine(FadeOutAndLoadWithManager(targetSceneName));
	}

	private IEnumerator FadeOutAndLoadWithManager(string targetSceneName)
	{
		// 1. 화면이 완전히 암전될 때까지 기다립니다.
		yield return StartCoroutine(Fade(0f, 1f));

		// 2. 암전이 완료되면, 우리가 기존에 만든 로딩 매니저에게 다음 씬을 넘겨주며 실행합니다!
		SimpleLoadingManager.LoadScene(targetSceneName);
	}
	// 페이드 로직 (기존과 동일)
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