using System.Collections;
using UnityEngine;

public class PlatformLogic : MonoBehaviour
{
    private Collider2D _platformCollider;

	private void Awake()
	{
		// 플랫폼 오브젝트에 부착된 콜라이더를 가져옵니다.
		// (BoxCollider2D 또는 EdgeCollider2D 등이 될 수 있습니다.)
		_platformCollider = GetComponent<CompositeCollider2D>();

		if (_platformCollider == null)
		{
			Debug.LogError("PlatformLogic에는 Collider2D 컴포넌트가 필요합니다.");
			enabled = false;
		}
	}
	/// <summary>
	/// 플레이어와 이 플랫폼 간의 충돌을 일시적으로 비활성화합니다.
	/// </summary>
	/// <param name="playerCollider">플레이어의 Collider2D 컴포넌트.</param>
	/// <param name="duration">충돌을 비활성화할 시간(초).</param>
	public void DisableCollisionForDrop(Collider2D playerCollider, float duration = 0.3f)
	{
		if (playerCollider == null || _platformCollider == null) return;

		// 1. 플레이어와 플랫폼 간의 충돌을 무시 (Drop-Through 시작)
		Physics2D.IgnoreCollision(playerCollider, _platformCollider, true);

		// 2. 잠시 후 충돌을 다시 활성화하는 코루틴 시작
		StartCoroutine(ReEnableCollisionCoroutine(playerCollider, duration));
	}
	// 코루틴: 충돌 무시를 해제하여 플레이어가 다시 밟을 수 있게 합니다.
	private IEnumerator ReEnableCollisionCoroutine(Collider2D playerCollider, float duration)
	{
		yield return new WaitForSeconds(duration);

		// 안전하게 플레이어 콜라이더가 아직 유효한 경우에만 복원
		if (playerCollider != null && _platformCollider != null)
		{
			Physics2D.IgnoreCollision(playerCollider, _platformCollider, false);
		}
	}
}
