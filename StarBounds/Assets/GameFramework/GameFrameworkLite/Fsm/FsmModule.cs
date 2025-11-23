// Assets/GameFrameworkLite/Fsm/FsmModule.cs
using System;
using System.Collections.Generic;

namespace GameFrameworkLite
{
	/// <summary>
	/// 여러 FSM을 이름으로 관리하는 모듈.
	/// 예: "Procedure", "PlayerFSM" 등.
	/// </summary>
	public sealed class FsmModule : IGameFrameworkModule
	{
		// 이름 → FSM 인스턴스 (타입이 제각각이라 object로 보관)
		private readonly Dictionary<string, object> _fsms =
			new Dictionary<string, object>();

		/// <summary>
		/// 새 FSM 생성.
		/// </summary>
		public Fsm<TOwner> CreateFsm<TOwner>(string name, TOwner owner, params FsmState<TOwner>[] states)
			where TOwner : class
		{
			var fsm = new Fsm<TOwner>(owner, states);
			_fsms[name] = fsm;
			return fsm;
		}

		/// <summary>
		/// 이름으로 FSM 가져오기.
		/// </summary>
		public Fsm<TOwner> GetFsm<TOwner>(string name) where TOwner : class
		{
			object fsmObj;
			if (_fsms.TryGetValue(name, out fsmObj))
				return fsmObj as Fsm<TOwner>;
			return null;
		}

		/// <summary>
		/// FSM 제거 및 Shutdown 호출.
		/// </summary>
		public void DestroyFsm(string name)
		{
			object fsmObj;
			if (_fsms.TryGetValue(name, out fsmObj))
			{
				var shutdownMethod = fsmObj.GetType().GetMethod("Shutdown");
				shutdownMethod?.Invoke(fsmObj, null);
				_fsms.Remove(name);
			}
		}

		/// <summary>
		/// 등록된 모든 FSM의 Update 호출.
		/// </summary>
		public void Update(float deltaTime, float realDeltaTime)
		{
			foreach (var kv in _fsms)
			{
				var fsmObj = kv.Value;
				var updateMethod = fsmObj.GetType().GetMethod("Update");
				updateMethod?.Invoke(fsmObj, new object[] { deltaTime, realDeltaTime });
			}
		}

		public void Shutdown()
		{
			foreach (var kv in _fsms)
			{
				var fsmObj = kv.Value;
				var shutdownMethod = fsmObj.GetType().GetMethod("Shutdown");
				shutdownMethod?.Invoke(fsmObj, null);
			}
			_fsms.Clear();
		}
	}
}
