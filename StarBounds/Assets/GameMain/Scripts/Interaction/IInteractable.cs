using UnityEngine;
using GameFrameworkLite;
/// <summary>
/// 플레이어와 상호작용할 수 있는 오브젝트가 구현하는 인터페이스.
/// </summary>
public interface IInteractable
{
	/// <summary>
	/// 플레이어가 상호작용할 때 호출되는 함수.
	/// </summary>
	void Interact(PlayerLogic player);
}
