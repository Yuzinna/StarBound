// Assets/GameFrameworkLite/Entity/EntityLogic.cs
using UnityEngine;

namespace GameFrameworkLite
{
	/// <summary>
	/// 엔티티 프리팹에 붙이는 기본 로직 클래스.
	/// 플레이어, 적, 오브젝트 등은 이 클래스를 상속해서 사용.
	/// </summary>
	public abstract class EntityLogic : MonoBehaviour
	{
		// EntityModule에서 설정해주는 ID, 에셋 이름, 그룹 이름
		public int Id { get; internal set; }
		public string AssetName { get; internal set; }
		public string GroupName { get; internal set; }

		/// <summary>
		/// 엔티티가 처음 생성될 때 한 번 호출. (초기 설정용)
		/// userData로 추가 데이터 전달 가능.
		/// </summary>
		public virtual void OnInit(object userData) { }

		/// <summary>
		/// 엔티티가 "보여질" 때 호출. (UGF의 Show 개념)
		/// 풀에서 다시 꺼낼 때도 이 함수가 호출된다고 생각하면 됨.
		/// </summary>
		public virtual void OnShow(object userData) { }

		/// <summary>
		/// 엔티티가 "숨겨질" 때 호출. (UGF의 Hide 개념)
		/// 풀로 반환될 때 호출.
		/// </summary>
		public virtual void OnHide(object userData) { }

		/// <summary>
		/// 매 프레임 EntityModule.Update에서 호출.
		/// </summary>
		public virtual void OnUpdate(float deltaTime, float realDeltaTime) { }
	}
}
