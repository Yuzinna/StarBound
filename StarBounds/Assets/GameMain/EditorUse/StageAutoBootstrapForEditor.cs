using UnityEngine;
using UnityEngine.SceneManagement;
using GameFrameworkLite;

public class StageAutoBootstrapForEditor : MonoBehaviour
{
	[SerializeField]
	private string entrySceneName = "Entry";

	private void Awake()
	{
#if UNITY_EDITOR
		if (!Application.isPlaying)
			return;

		var framework = FindAnyObjectByType<FrameworkComponent>();
		if (framework != null)
			return;

		Debug.Log("[StageAutoBootstrapForEditor] Framework 없음 → Entry 씬으로 이동해서 부트스트랩 시작");
		SceneManager.LoadScene(entrySceneName);
#endif
	}
}
