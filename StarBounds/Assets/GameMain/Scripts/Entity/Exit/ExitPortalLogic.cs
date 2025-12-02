using UnityEngine;
using GameFrameworkLite;

[RequireComponent(typeof(Collider2D))]
public class ExitPortalLogic : MonoBehaviour ,IInteractable
{
	private EventModule _eventModule;

	public void Interact(PlayerLogic player)
	{
		//중력 상태가 노말일때만 실행
		if(GravityManager.Instance.CurrentDirection== eGravityDirection.Normal&& GravityManager.Instance.IsFloatingEnabled==false)
		{
			Debug.Log("[ExitPortalLogic] Player entered exit. Stage1Clear fired.");

			_eventModule.Fire((int)GameEventId.StageClear, null);
		}
	}
	private void Awake()
	{
		InitModulesAndCollider();
	}
	private void InitModulesAndCollider()
	{
		if (_eventModule == null)
		{
			_eventModule = GameFrameworkEntry.GetModule<EventModule>();
		}

		var col = GetComponent<Collider2D>();
		if (col != null)
		{
			col.isTrigger = true;
		}
	}
	

	private void OnTriggerEnter2D(Collider2D other)
	{
		
		
	}
}
