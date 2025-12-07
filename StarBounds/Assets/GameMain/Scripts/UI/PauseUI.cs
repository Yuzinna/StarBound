using UnityEngine;

public class PauseUI : MonoBehaviour
{
	private const string Path = "UI/PauseUI";

	
	public void OnClickResume()
	{
		
	}

	public void OnClickRetry()
	{
		// TODO: 나중에 현재 스테이지 다시 시작 로직 넣기
		Debug.Log("[PauseUI] Retry 버튼 눌림 (나중에 구현)");
	}

	public void OnClickQuitToTitle()
	{
		// TODO: 타이틀 씬으로 가는 로직 (Procedure/SceneModule 연동)
		Debug.Log("[PauseUI] Quit 버튼 눌림 (나중에 구현)");
	}
}
