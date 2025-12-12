using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class ExitPortalLogic : MonoBehaviour ,IInteractable
{
	[SerializeField] string nextSceneName = "Level_02";
	[SerializeField] AudioClip endSFX;

	[SerializeField] private SpriteRenderer endSignSprite;
	[SerializeField] private Sprite endSignOn;
	[SerializeField] private Sprite endSignOff;
	private void Awake()
	{
		InitModulesAndCollider();
	}
	private void Start()
	{
		UpdateEndSign();          // 시작 시 1회 반영
		SubscribeGravityEvents(); // 이후 변화 추적

		if (GravityManager.Instance.CurrentDirection == eGravityDirection.Normal &&
			GravityManager.Instance.IsFloatingEnabled == false)
		{
			endSignSprite.sprite = endSignOff;
		}
		else
		{
			endSignSprite.sprite = endSignOn;
		}
	}
	private void OnDestroy()
	{
		UnsubscribeGravityEvents();
	}
	private bool IsExitEnabled()
	{
		return GravityManager.Instance != null
			   && GravityManager.Instance.CurrentDirection == eGravityDirection.Normal
			   && GravityManager.Instance.IsFloatingEnabled == false;
	}

	private void UpdateEndSign()
	{
		if (endSignSprite == null) return;

		endSignSprite.sprite = IsExitEnabled() ? endSignOn : endSignOff;
	}
	private void SubscribeGravityEvents()
	{
		if (GravityManager.Instance == null) return;

		GravityManager.Instance.OnGravityDirectionChanged += HandleGravityChanged;
		GravityManager.Instance.OnFloatingStateChanged += HandleFloatingChanged;
	}
	private void UnsubscribeGravityEvents()
	{
		if (GravityManager.Instance == null) return;

		GravityManager.Instance.OnGravityDirectionChanged -= HandleGravityChanged;
		GravityManager.Instance.OnFloatingStateChanged -= HandleFloatingChanged;
	}
	private void HandleGravityChanged(eGravityDirection _)
	{
		UpdateEndSign();
	}

	private void HandleFloatingChanged(bool _)
	{
		UpdateEndSign();
	}
	public void Interact(PlayerLogic player)
	{
		if (!IsExitEnabled())
			return;

		Debug.Log("[ExitPortalLogic] Player entered exit. Stage1Clear fired.");

		if (SfxManager.Instance != null)
			SfxManager.Instance.PlaySfx(endSFX);

		if (SceneTransitionManager.Instance != null)
			SceneTransitionManager.Instance.LoadNextScene(nextSceneName);
		else
			SceneManager.LoadScene(nextSceneName);

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
