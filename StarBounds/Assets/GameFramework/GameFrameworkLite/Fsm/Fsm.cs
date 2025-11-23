// Assets/GameFrameworkLite/Fsm/Fsm.cs
using System;
using System.Collections.Generic;

namespace GameFrameworkLite
{
	/// <summary>
	/// 제네릭 FSM 구현.
	/// Owner 객체와 여러 FsmState를 가질 수 있다.
	/// </summary>
	public sealed class Fsm<TOwner> where TOwner : class
	{
		public TOwner Owner { get; private set; }              // FSM을 소유한 객체
		public FsmState<TOwner> CurrentState { get; private set; } // 현재 상태

		private readonly Dictionary<Type, FsmState<TOwner>> _states =
			new Dictionary<Type, FsmState<TOwner>>();

		public Fsm(TOwner owner, params FsmState<TOwner>[] states)
		{
			Owner = owner;
			foreach (var state in states)
			{
				_states.Add(state.GetType(), state);
			}
		}

		/// <summary>
		/// 초기 상태 시작.
		/// </summary>
		public void Start<TState>() where TState : FsmState<TOwner>
		{
			ChangeState<TState>();
		}

		/// <summary>
		/// 다른 상태로 전환.
		/// </summary>
		public void ChangeState<TState>() where TState : FsmState<TOwner>
		{
			var type = typeof(TState);
			FsmState<TOwner> state;
			if (!_states.TryGetValue(type, out state))
			{
				throw new Exception($"[Fsm] State '{type.Name}' not registered.");
			}

			// 기존 상태에서 나가기
			if (CurrentState != null)
				CurrentState.OnLeave(this, false);

			// 새 상태로 교체 후 OnEnter 호출
			CurrentState = state;
			CurrentState.OnEnter(this);
		}

		/// <summary>
		/// 매 프레임 상태 업데이트.
		/// </summary>
		public void Update(float deltaTime, float realDeltaTime)
		{
			CurrentState?.OnUpdate(this, deltaTime, realDeltaTime);
		}

		/// <summary>
		/// FSM 종료.
		/// </summary>
		public void Shutdown()
		{
			if (CurrentState != null)
			{
				CurrentState.OnLeave(this, true);
				CurrentState = null;
			}
			_states.Clear();
		}
	}
}
