// Assets/GameFrameworkLite/Core/GameFrameworkEntry.cs
using System;
using System.Collections.Generic;

namespace GameFrameworkLite
{
	/// <summary>
	/// 전역적으로 모듈을 관리하고 꺼내 쓰는 정적 클래스.
	/// UGF의 GameFrameworkEntry 느낌.
	/// </summary>
	public static class GameFrameworkEntry
	{
		// FrameworkComponent 인스턴스 (씬에 1개만 존재)
		private static FrameworkComponent s_FrameworkComponent;

		// 타입별 모듈 저장 딕셔너리
		private static readonly Dictionary<Type, IGameFrameworkModule> s_Modules =
			new Dictionary<Type, IGameFrameworkModule>();

		/// <summary>
		/// FrameworkComponent가 Awake에서 자기 자신을 등록할 때 호출.
		/// 이 안에서 모든 모듈을 등록한다.
		/// </summary>
		internal static void RegisterFrameworkComponent(FrameworkComponent component)
		{
			s_FrameworkComponent = component;
			s_Modules.Clear();

			// FrameworkComponent가 만든 모듈들을 전부 등록
			AddModule(component.EventModule);
			AddModule(component.ResourceModule);
			AddModule(component.ObjectPoolModule);
			AddModule(component.EntityModule);
			AddModule(component.FsmModule);
			AddModule(component.ProcedureModule);
			AddModule(component.UiModule);
			AddModule(component.SceneModule);
			AddModule(component.SoundModule);
			AddModule(component.DataTableModule);
			AddModule(component.ConfigModule);
		}

		/// <summary>
		/// 내부용 모듈 추가 함수. 타입을 키로 사용.
		/// </summary>
		private static void AddModule(IGameFrameworkModule module)
		{
			if (module == null) return;
			var type = module.GetType();
			if (!s_Modules.ContainsKey(type))
				s_Modules.Add(type, module);
		}

		/// <summary>
		/// 원하는 모듈 타입을 꺼내오기 위한 함수.
		/// 예: var entity = GameFrameworkEntry.GetModule&lt;EntityModule&gt;();
		/// </summary>
		public static T GetModule<T>() where T : class, IGameFrameworkModule
		{
			IGameFrameworkModule module;
			if (s_Modules.TryGetValue(typeof(T), out module))
			{
				return module as T;
			}

			return null;
		}

		/// <summary>
		/// 모든 모듈의 Update를 한 번에 호출.
		/// FrameworkComponent.Update에서 사용.
		/// </summary>
		internal static void UpdateAll(float deltaTime, float realDeltaTime)
		{
			foreach (var kv in s_Modules)
			{
				kv.Value.Update(deltaTime, realDeltaTime);
			}
		}

		/// <summary>
		/// 모든 모듈의 Shutdown을 한 번에 호출 후 딕셔너리 비우기.
		/// </summary>
		internal static void ShutdownAll()
		{
			foreach (var kv in s_Modules)
			{
				kv.Value.Shutdown();
			}
			s_Modules.Clear();
		}
	}
}
