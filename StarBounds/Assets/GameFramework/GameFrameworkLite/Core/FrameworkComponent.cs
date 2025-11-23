// Assets/GameFrameworkLite/Core/FrameworkComponent.cs
using UnityEngine;

namespace GameFrameworkLite
{
	/// <summary>
	/// 씬에 하나만 존재하는 프레임워크 진입점 컴포넌트.
	/// 여기서 모든 모듈을 생성하고 GameFrameworkEntry에 등록한다.
	/// </summary>
	public sealed class FrameworkComponent : MonoBehaviour
	{
		// ------------ 모듈 참조 (Inspector에서 확인용) ------------
		public EventModule EventModule { get; private set; }
		public ResourceModule ResourceModule { get; private set; }
		public ObjectPoolModule ObjectPoolModule { get; private set; }
		public EntityModule EntityModule { get; private set; }
		public FsmModule FsmModule { get; private set; }
		public ProcedureModule ProcedureModule { get; private set; }
		public UIModule UiModule { get; private set; }
		public SceneModule SceneModule { get; private set; }
		public SoundModule SoundModule { get; private set; }
		public DataTableModule DataTableModule { get; private set; }
		public ConfigModule ConfigModule { get; private set; }

		private void Awake()
		{
			// 씬이 바뀌어도 파괴되지 않도록 설정
			DontDestroyOnLoad(gameObject);

			// --- 모듈 생성 ---
			EventModule = new EventModule();
			ResourceModule = new ResourceModule();
			ObjectPoolModule = new ObjectPoolModule();
			EntityModule = new EntityModule(ResourceModule, ObjectPoolModule);
			FsmModule = new FsmModule();
			ProcedureModule = new ProcedureModule(FsmModule);
			UiModule = new UIModule(ResourceModule);
			SceneModule = new SceneModule();
			SoundModule = new SoundModule(ResourceModule);
			DataTableModule = new DataTableModule(ResourceModule);
			ConfigModule = new ConfigModule(ResourceModule);

			// GameFrameworkEntry에 자신(FrameworkComponent) 등록
			GameFrameworkEntry.RegisterFrameworkComponent(this);
		}

		private void Update()
		{
			// 매 프레임 모든 모듈의 Update 호출
			float dt = Time.deltaTime;
			float realDt = Time.unscaledDeltaTime;
			GameFrameworkEntry.UpdateAll(dt, realDt);
		}

		private void OnDestroy()
		{
			// 게임 종료 또는 오브젝트 파괴 시 모듈들 Shutdown
			GameFrameworkEntry.ShutdownAll();
		}
	}
}
