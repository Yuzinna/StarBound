using GameFrameworkLite;
using UnityEngine;

public class ProcedureStageClear : ProcedureBase
{
	public override void OnEnter(Fsm<ProcedureModule> fsm)
	{
		base.OnEnter(fsm);
		Debug.Log("Stage 1 Clear!");
		// 여기서 클리어 UI 열기, 다음 스테이지로 넘어갈지 선택 등
		// 일단 예시로 바로 다음 스테이지로 넘긴다고 하면:
		// fsm.Owner.ChangeProcedure<ProcedureStage2>();
	}

}
