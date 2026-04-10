using System.Collections;
using UnityEngine;

public class SinglePlatformLogic : MonoBehaviour
{
	private Collider2D _platformCollider;
	private Transform _localTransform;
	public float duration = 0.5f;
	private void Awake()
	{
		
		_platformCollider = GetComponent<BoxCollider2D>();
		
		if (_platformCollider == null)
		{
			Debug.LogError("PlatformLogic���� Collider2D ������Ʈ�� �ʿ��մϴ�.");
			enabled = false;
		}
	}
	private void Start()
	{
		
		GravityManager.Instance.OnGravityDirectionChanged += setInverseCollider;
	}
	public void setInverseCollider(eGravityDirection direction)
	{
		
		if (direction == eGravityDirection.Inverse)
		{
			transform.localPosition = new Vector3(0, 0, 0);
			transform.localScale = new Vector3(1, -1, 1);
		}
		//�߷� ������ �븻�� �ٲ�
		else if (direction == eGravityDirection.Normal)
		{
			transform.localPosition =  new Vector3(0, 0.133f, 0);
			transform.localScale = Vector3.one;
		}
	}
	
	public void DisableCollisionForDrop(Collider2D playerCollider)
	{
		if (playerCollider == null || _platformCollider == null) return;

		// 1. �÷��̾�� �÷��� ���� �浹�� ���� (Drop-Through ����)
		Physics2D.IgnoreCollision(playerCollider, _platformCollider, true);

		
		//StartCoroutine(ReEnableCollisionCoroutine(playerCollider, duration));
	}
	// =========================================================
	// 💡 [핵심] 오브젝트가 파괴될 때 불리는 유니티 내장 함수
	// =========================================================
	private void OnDestroy()
	{
		// 맵이 재시작되거나 파괴될 때, 불사신인 매니저에게 "나한테 더 이상 연락하지 마!" 라고 명부에서 지웁니다.
		if (GravityManager.Instance != null)
		{
			GravityManager.Instance.OnGravityDirectionChanged -= setInverseCollider;

			// (혹시 부유 이벤트도 구독 중이라면 아래 줄도 추가하세요!)
			// GravityManager.Instance.OnFloatingStateChanged -= 구독한함수이름;
		}
	}

}
