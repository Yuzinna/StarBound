using System.Collections;
using UnityEngine;

public class SinglePlatformLogic : MonoBehaviour
{
	private Collider2D _platformCollider;
	private Transform _localTransform;
	public float duration = 0.5f;
	private void Awake()
	{
		// 플랫폼 오브젝트에 부착된 콜라이더를 가져옵니다.
		// (BoxCollider2D 또는 EdgeCollider2D 등이 될 수 있습니다.)
		_platformCollider = GetComponent<BoxCollider2D>();
		
		if (_platformCollider == null)
		{
			Debug.LogError("PlatformLogic에는 Collider2D 컴포넌트가 필요합니다.");
			enabled = false;
		}
	}
	private void Start()
	{
		//중력 방향이 바뀌면 호출할 함수
		GravityManager.Instance.OnGravityDirectionChanged += setInverseCollider;
	}
	public void setInverseCollider(eGravityDirection direction)
	{
		//중력 방향이 거꾸로 바뀜
		if (direction == eGravityDirection.Inverse)
		{
			transform.localPosition = new Vector3(0, 0, 0);
			transform.localScale = new Vector3(1, -1, 1);
		}
		//중력 방향이 노말로 바뀜
		else if (direction == eGravityDirection.Normal)
		{
			transform.localPosition =  new Vector3(0, 0.133f, 0);
			transform.localScale = Vector3.one;
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

		// 1. 플레이어와 플랫폼 간의 충돌을 무시 (Drop-Through 시작)
		Physics2D.IgnoreCollision(playerCollider, _platformCollider, true);

		//// 2. 잠시 후 충돌을 다시 활성화하는 코루틴 시작
		//StartCoroutine(ReEnableCollisionCoroutine(playerCollider, duration));
	}
	
	
}
