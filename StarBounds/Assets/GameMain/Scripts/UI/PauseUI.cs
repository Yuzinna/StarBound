using UnityEngine;
using GameFrameworkLite;
public class PauseUI : UILogic
{
	private const string Path = "UI/PauseUI";

	public override void OnClose()
	{
		base.OnClose();
		// 게임 다시 진행
		Time.timeScale = 1f;
		Debug.Log("[PauseUI] OnClose - 게임 재개");
	}

	public override void OnOpen(object userData)
	{
		base.OnOpen(userData);
		Time.timeScale = 0f;
		Debug.Log("게임 일시정지");
		
	}
	public void OnClickResume()
	{
		var ui = GameFrameworkEntry.GetModule<UIModule>();
		ui.Close(Path);   // 닫으면 OnClose()에서 timeScale 복구됨
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
