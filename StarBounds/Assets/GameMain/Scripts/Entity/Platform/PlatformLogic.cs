using System.Collections;
using UnityEngine;

public class PlatformLogic : MonoBehaviour
{
	private Collider2D _platformCollider;
	private Coroutine _reEnableCoroutine;
	private void Awake()
	{
		// 플랫폼 오브젝트에 부착된 콜라이더를 가져옵니다.
		// (BoxCollider2D 또는 EdgeCollider2D 등이 될 수 있습니다.)
		_platformCollider = GetComponent<CompositeCollider2D>();

		if (_platformCollider == null)
		{
			Debug.LogError("PlatformLogic에는 Collider2D 컴포넌트가 필요합니다.", gameObject);
			enabled = false;
		}	

	}
	/// <summary>
	/// 플레이어와 이 플랫폼 간의 충돌을 일시적으로 비활성화합니다.
	/// </summary>
	/// <param name="playerCollider">플레이어의 Collider2D 컴포넌트.</param>
	/// <param name="duration">충돌을 비활성화할 시간(초).</param>
	public void DisableCollisionForDrop(Collider2D playerCollider)
	{
		if (playerCollider == null || _platformCollider == null) return;

		// 중복 코루틴 실행 방지
		if (_reEnableCoroutine != null)
		{
			StopCoroutine(_reEnableCoroutine);
		}

		// 1. 드롭을 시작한 순간의 플레이어 하단 Y 좌표를 저장합니다.
		float startDropY = playerCollider.bounds.min.y;

		// 2. 플레이어와 '이 특정 플랫폼'의 충돌만을 무시합니다. (수직 겹침 문제 해결)
		Physics2D.IgnoreCollision(playerCollider, _platformCollider, true);

		// 3. Y 좌표 기반 복구 로직 시작 (낮은 플랫폼 튕김 문제 해결)
		_reEnableCoroutine = StartCoroutine(ReEnableCollisionCoroutine(playerCollider, startDropY));
	}
	// 코루틴: 충돌 무시를 해제하여 플레이어가 다시 밟을 수 있게 합니다.
	private IEnumerator ReEnableCollisionCoroutine(Collider2D playerCollider, float startDropY)
	{
		// 플레이어의 하단 Y 좌표가 출발선(startDropY)보다 크거나 같으면 계속 대기합니다.
		while (playerCollider != null && playerCollider.bounds.max.y >= startDropY)
		{
			yield return null;
		}

		// 안전 마진: 출발선 아래로 내려왔다면, 0.1초 동안 추가로 낙하할 시간을 줍니다.
		yield return new WaitForSeconds(0.1f);

		// 4. 충돌 복원
		if (playerCollider != null && _platformCollider != null)
		{
			Physics2D.IgnoreCollision(playerCollider, _platformCollider, false);
			Debug.Log($"[PlatformLogic] 충돌 복구 완료: Y 좌표({startDropY}) 통과");
		}

		_reEnableCoroutine = null;
	}
}
