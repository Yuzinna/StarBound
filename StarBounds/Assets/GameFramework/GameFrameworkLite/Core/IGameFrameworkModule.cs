// Assets/GameFrameworkLite/Core/IGameFrameworkModule.cs
namespace GameFrameworkLite
{
	/// <summary>
	/// 모든 프레임워크 모듈이 구현해야 하는 기본 인터페이스.
	/// - Update : 매 프레임 호출
	/// - Shutdown : 게임 종료 시 정리용
	/// </summary>
	public interface IGameFrameworkModule
	{
		/// <summary>
		/// 매 프레임 호출되는 함수.
		/// deltaTime      : Time.deltaTime
		/// realDeltaTime  : Time.unscaledDeltaTime
		/// </summary>
		void Update(float deltaTime, float realDeltaTime);

		/// <summary>
		/// 모듈 종료 시 리소스 해제, 정리용.
		/// </summary>
		void Shutdown();
	}
}
