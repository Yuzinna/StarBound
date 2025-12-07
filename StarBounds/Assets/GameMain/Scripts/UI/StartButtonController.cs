using System.Transactions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartButtonController : MonoBehaviour
{
	[SerializeField]
	private string nextSceneName = "SynopsisScene"; // 시놉시스 씬 이름을 여기에 입력합니다.

	private SceneTransitionManager transitionManager;
	void Start()
	{
		// 버튼 컴포넌트를 가져옵니다.
		Button button = GetComponent<Button>();
		if (button != null)
		{
			// 버튼 클릭 이벤트에 씬 전환 함수를 연결합니다.
			button.onClick.AddListener(OnStartButtonClicked);
		}

		// SceneTransitionManager가 DDOL로 설정되어 있다면 인스턴스를 가져옵니다.
		transitionManager = SceneTransitionManager.Instance;

		// ✨ (선택 사항) 최초 시작 시 Fade In 처리:
		// 타이틀 씬에 들어왔을 때 화면이 검은색이면 안 되므로, 
		// SceneTransitionManager에게 Fade In을 요청하여 화면을 밝게 만듭니다.
		if (transitionManager != null)
		{
			transitionManager.StartFadeIn();
		}
	}
	private void OnStartButtonClicked()
	{
		Debug.Log("[StartButtonController] Start 버튼 클릭! 시놉시스 씬으로 전환합니다.");

		if (string.IsNullOrEmpty(nextSceneName))
		{
			Debug.LogError("다음 씬 이름이 설정되지 않았습니다!");
			return;
		}

		// SceneTransitionManager가 있다면 Fade Out 후 씬 전환을 요청합니다.
		if (transitionManager != null)
		{
			// ✨ Fade Out 애니메이션 시작 후 다음 씬 로드
			transitionManager.LoadNextScene(nextSceneName);
		}
		else
		{
			// SceneTransitionManager가 없으면 바로 씬 로드 (비상용)
			SceneManager.LoadScene(nextSceneName);
		}
	}
}
