using System.Transactions;
using UnityEngine;

public class EndUI : MonoBehaviour
{
	private SceneTransitionManager transitionManager;
	private void Start()
	{
		transitionManager = SceneTransitionManager.Instance;
		if (transitionManager != null)
		{
			transitionManager.StartFadeIn();
		}
		InputManager.Instance.SwitchToUI();
	}
	public void ExitGame()
	{
		Debug.Log("게임 종료!");
		Application.Quit();
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false; // 에디터에서 테스트 용
#endif
	}
}
