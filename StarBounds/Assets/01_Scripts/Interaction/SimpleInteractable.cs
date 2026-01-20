using UnityEngine;

public class SimpleInteractable : MonoBehaviour, IInteractable
{
	[Tooltip("디버그용 메시지")]
	public string debugMessage = "플레이어가 이 오브젝트와 상호작용했습니다.";

	[Tooltip("상호작용 시 색을 바꿀 SpriteRenderer (선택 사항)")]
	public SpriteRenderer highlightRenderer;

	[Tooltip("상호작용 후 적용할 색 (선택 사항)")]
	public Color interactedColor = Color.yellow;

	private bool _hasInteracted = false;

	private void Reset()
	{
		// 상호작용 범위 체크용 Collider2D는 Trigger로 쓰는 것을 추천
		var col = GetComponent<Collider2D>();
		col.isTrigger = true;
	}

	public void Interact(PlayerLogic player)
	{
		if (_hasInteracted) return;

		_hasInteracted = true;
		Debug.Log($"[SimpleInteractable] {debugMessage}");

		if (highlightRenderer != null)
		{
			highlightRenderer.color = interactedColor;
		}

		// 여기서 문 열기, 스위치 토글, 퍼즐 상태 변경 등 원하는 로직을 넣으면 된다.
	}
}
