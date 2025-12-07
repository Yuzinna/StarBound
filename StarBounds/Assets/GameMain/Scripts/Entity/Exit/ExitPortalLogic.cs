using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class ExitPortalLogic : MonoBehaviour ,IInteractable
{
	[SerializeField] string nextSceneName = "Level_02";	
	
	public void Interact(PlayerLogic player)
	{
		//중력 상태가 노말일때만 실행
		if(GravityManager.Instance.CurrentDirection== eGravityDirection.Normal&& GravityManager.Instance.IsFloatingEnabled==false)
		{
			Debug.Log("[ExitPortalLogic] Player entered exit. Stage1Clear fired.");

			// ✨ SceneTransitionManager를 통해 씬 전환 요청
			if (SceneTransitionManager.Instance != null)
			{
				SceneTransitionManager.Instance.LoadNextScene(nextSceneName);
			}
			else
			{
				// 트랜지션 매니저가 없을 경우 바로 로드 (비상 시)
				SceneManager.LoadScene(nextSceneName);
			}
		}
	}
	
	private void Awake()
	{
		InitModulesAndCollider();
	}
	private void InitModulesAndCollider()
	{
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
