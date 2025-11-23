// Assets/GameFrameworkLite/Procedure/ProcedureBase.cs
namespace GameFrameworkLite
{
	/// <summary>
	/// 게임 흐름(Procedure) 상태의 기본 클래스.
	/// 내부적으로는 FsmState&lt;ProcedureModule&gt;를 상속.
	/// </summary>
	public abstract class ProcedureBase : FsmState<ProcedureModule>
	{
	}
}
