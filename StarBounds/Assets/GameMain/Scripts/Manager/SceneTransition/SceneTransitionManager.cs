using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    [SerializeField] private Animator transitionAnimator;

    private const string FADE_OUT_TRIGGER = "StartFade";
	private const string FADE_IN_TRIGGER = "StratFadeIn";

	public static SceneTransitionManager Instance { get; private set; }

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			// 씬이 바뀌어도 파괴되지 않게 하여 어디서든 접근 가능하게 합니다.
			DontDestroyOnLoad(gameObject);
			SceneManager.sceneLoaded += OnSceneLoadCompleted;
		}
		else
		{
			Destroy(gameObject);
		}
	}
	public void LoadNextScene(string sceneName)
	{
		StartCoroutine(TransitionToScene(sceneName));
	}
	// ✨ 코루틴을 이용한 로딩 제어
	private IEnumerator TransitionToScene(string sceneName)
	{
		// 1. 페이드 아웃 애니메이션 시작
		if (transitionAnimator != null)
		{
			transitionAnimator.SetTrigger(FADE_OUT_TRIGGER);

			// 애니메이션이 끝날 때까지 기다립니다.
			// 애니메이션 클립 길이가 0.5초라면, 여기에 yield return new WaitForSeconds(0.5f); 를 넣거나
			// 아래와 같이 애니메이터 상태가 될 때까지 기다립니다. (더 정교한 방법)
			yield return new WaitForSeconds(1f); // 임시로 충분한 시간을 기다립니다.
		}

		
		Debug.Log($"[TransitionManager] 씬 로드 시작: {sceneName} (동기)");
		SceneManager.LoadScene(sceneName);
		
	}
	// ==========================================================
	// ✨ 2. Fade In Public Method (새로운 기능)
	// ==========================================================
	/// <summary>
	/// 새로운 씬 로드 후, 화면을 밝게 만드는 Fade In 애니메이션을 시작합니다.
	/// </summary>
	public void StartFadeIn()
	{
		if (transitionAnimator != null)
		{
			// ✨ Fade In 트리거 발동
			transitionAnimator.SetTrigger(FADE_IN_TRIGGER);
		}
	}
	private void OnSceneLoadCompleted(Scene scene, LoadSceneMode mode)
	{
		// DDOL 오브젝트인 GameInit이 살아있으므로, 직접 호출합니다.
		if (GameInit.Instance != null)
		{
			Debug.Log($"[TransitionManager] 새 씬 ({scene.name}) 로드 완료. GameInit 초기화 실행.");

			// 1. 초기화 (플레이어 스폰/위치 설정 등)
			GameInit.Instance.InitGame();

			// 2. Fade In 시작
			GameInit.Instance.StartFadeIn();
		}
	}
}

