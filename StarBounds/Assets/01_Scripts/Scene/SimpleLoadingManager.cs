using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SimpleLoadingManager : MonoBehaviour
{
	[Header("UI Elements")]
	public Image progressBarImage;

	[Header("Settings")]
	// 최소 로딩 대기 시간 (초) - 인스펙터에서 수정할 수 있습니다.
	public float minLoadTime = 2.0f;

	private static string _sceneToLoad = "";

	public static void LoadScene(string sceneName)
	{
		_sceneToLoad = sceneName;
		SceneManager.LoadScene("LoadingScene");
	}

	private void Start()
	{
		if (progressBarImage != null) progressBarImage.fillAmount = 0f;

		if (!string.IsNullOrEmpty(_sceneToLoad))
		{
			StartCoroutine(LoadAsynchronously());
		}
	}

	private IEnumerator LoadAsynchronously()
	{
		AsyncOperation op = SceneManager.LoadSceneAsync(_sceneToLoad);
		op.allowSceneActivation = false;

		float timer = 0f; // 경과 시간을 측정할 타이머

		// 실제 로딩이 안 끝났거나 OR 최소 시간이 아직 안 지났다면 계속 반복
		while (op.progress < 0.9f || timer < minLoadTime)
		{
			timer += Time.deltaTime;

			// 1. 실제 로딩 진행률 (0.9를 1.0으로 정규화)
			float loadProgress = op.progress / 0.9f;

			// 2. 지정한 최소 시간 대비 경과 시간의 비율
			float timeProgress = timer / minLoadTime;

			// 3. 실제 로딩률과 시간 진행률 중 '더 작은 값'을 로딩 바에 적용
			// (씬이 0.1초 만에 로드되어도, timeProgress에 맞춰 서서히 바가 찹니다)
			if (progressBarImage != null)
			{
				progressBarImage.fillAmount = Mathf.Min(loadProgress, timeProgress);
			}

			yield return null;
		}

		// 반복문을 빠져나왔다면 로딩도 완료되고 최소 시간도 지났음
		if (progressBarImage != null)
		{
			progressBarImage.fillAmount = 1f;
		}

		// 시각적으로 100%가 된 모습을 아주 잠깐(0.2초) 보여주고 씬 전환
		yield return new WaitForSeconds(0.2f);

		op.allowSceneActivation = true;
	}
}