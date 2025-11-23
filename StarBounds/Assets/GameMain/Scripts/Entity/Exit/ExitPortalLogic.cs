using UnityEngine;
using GameFrameworkLite;

[RequireComponent(typeof(Collider2D))]
public class ExitPortalLogic : EntityLogic ,IInteractable
{
	private EventModule _eventModule;

	public void Interact(PlayerLogic player)
	{
		//중력 상태가 노말일때만 실행
		if(GravityManager.Instance.CurrentDirection== eGravityDirection.Normal)
		{
			Debug.Log("[ExitPortalLogic] Player entered exit. Stage1Clear fired.");

			_eventModule.Fire(GameEventId.Stage1Clear, null);
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
	public override void OnInit(object userData)
	{
		base.OnInit(userData);
		InitModulesAndCollider();
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		
		
	}
}
