// Assets/GameFrameworkLite/UI/UILogic.cs
using UnityEngine;

namespace GameFrameworkLite
{
	/// <summary>
	/// UI 프리팹에 붙이는 기본 로직 클래스.
	/// </summary>
	public abstract class UILogic : MonoBehaviour
	{
		/// <summary>
		/// UI가 열릴 때 호출. (OpenUI 시점)
		/// </summary>
		public virtual void OnOpen(object userData) { }

		/// <summary>
		/// UI가 닫힐 때 호출.
		/// </summary>
		public virtual void OnClose() { }
	}
}
