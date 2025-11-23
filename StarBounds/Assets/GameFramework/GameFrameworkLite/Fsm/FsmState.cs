// Assets/GameFrameworkLite/Fsm/FsmState.cs
namespace GameFrameworkLite
{
	/// <summary>
	/// FSM 상태 기본 클래스.
	/// Owner 타입을 제네릭으로 지정.
	/// </summary>
	public abstract class FsmState<TOwner> where TOwner :class
	{
		/// <summary>
		/// 상태에 들어올 때 1회 호출.
		/// </summary>
		public virtual void OnEnter(Fsm<TOwner> fsm) { }

		/// <summary>
		/// 매 프레임 호출.
		/// </summary>
		public virtual void OnUpdate(Fsm<TOwner> fsm, float deltaTime, float realDeltaTime) { }

		/// <summary>
		/// 상태에서 나갈 때 호출.
		/// isShutdown : FSM 자체가 종료되는 경우 true
		/// </summary>
		public virtual void OnLeave(Fsm<TOwner> fsm, bool isShutdown) { }
	}
}
