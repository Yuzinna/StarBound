// Assets/GameFrameworkLite/Procedure/ProcedureModule.cs
using System;

namespace GameFrameworkLite
{
	/// <summary>
	/// UGF 스타일의 Procedure 시스템.
	/// 내부적으로 FSM("Procedure")를 하나 생성해서 사용.
	/// </summary>
	public sealed class ProcedureModule : IGameFrameworkModule
	{
		private readonly FsmModule _fsmModule;
		private Fsm<ProcedureModule> _fsm;

		public ProcedureModule(FsmModule fsmModule)
		{
			_fsmModule = fsmModule;
		}

		/// <summary>
		/// 사용할 Procedure들을 등록하고 FSM 생성.
		/// 반드시 게임 시작 시 1번 호출.
		/// </summary>
		public void Initialize(params ProcedureBase[] procedures)
		{
			if (_fsm != null)
				throw new Exception("[ProcedureModule] Already initialized.");

			_fsm = _fsmModule.CreateFsm("Procedure", this, procedures);
		}

		/// <summary>
		/// 처음 시작할 Procedure 지정.
		/// </summary>
		public void StartProcedure<T>() where T : ProcedureBase
		{
			if (_fsm == null)
			{
				throw new Exception("[ProcedureModule] Not initialized. Call Initialize() first.");
			}

			_fsm.Start<T>();
		}

		/// <summary>
		/// 다른 Procedure로 전환.
		/// </summary>
		public void ChangeProcedure<T>() where T : ProcedureBase
		{
			_fsm.ChangeState<T>();
		}

		/// <summary>
		/// 특정 Procedure 인스턴스를 가져오고 싶다면, 추후 확장 가능.
		/// (현재는 사용하지 않음)
		/// </summary>
		public T GetProcedure<T>() where T : ProcedureBase
		{
			return null;
		}

		public void Update(float deltaTime, float realDeltaTime)
		{
			_fsm?.Update(deltaTime, realDeltaTime);
		}

		public void Shutdown()
		{
			if (_fsm != null)
			{
				_fsmModule.DestroyFsm("Procedure");
				_fsm = null;
			}
		}
	}
}
