using System.Collections.Generic;
using UnityEngine;

public class LaserSwitch : MonoBehaviour, IInteractable
{
	[Header("스위치 on/off")]
	public bool isOn = false;

	[Header("스프라이트")]
	public SpriteRenderer spriteRenderer;
	public Sprite switchOffSprite;
	public Sprite switchOnSprite;

	

	[Header("스위치를 누를 수 있는 레이어")]
	[Tooltip("플레이어와 큐브의 레이어를 모두 체크해주세요.")]
	public LayerMask pressableLayers;

	private HashSet<Collider2D> _pressingObjects = new HashSet<Collider2D>();
	// 켜고 끌 레이저 오브젝트를 연결할 변수
	public Laser firstLaser;


	private void Start()
	{
		UpdateVisual();
		
	}
	public void Interact(PlayerLogic player)
	{
		isOn = !isOn;
		UpdateVisual();

		
	}
	private void UpdateVisual()
	{
		if(spriteRenderer != null)
		{
			spriteRenderer.sprite = isOn ? switchOnSprite : switchOffSprite;
		}
	}
	// 누군가 스위치 영역에 들어왔을 때
	private void OnTriggerEnter2D(Collider2D collision)
	{
		if(((1 << collision.gameObject.layer)& pressableLayers )!=0)
		{
			_pressingObjects.Add(collision); // 목록에 추가
			CheckSwitchState();
			firstLaser.gameObject.SetActive(true); // 레이저 켜기
		}
	}
	// 누군가 스위치 영역에서 나갔을 때
	private void OnTriggerExit2D(Collider2D other)
	{
		// 나간 물체가 목록에 있다면 제거
		if (_pressingObjects.Contains(other))
		{
			_pressingObjects.Remove(other);
			CheckSwitchState();
			firstLaser.TurnOffSequence(); // 레이저 끄기
		}
	}
	//안전장치: 스위치 위에서 큐브가 파괴되거나 비활성화되는 경우를 대비
	private void Update()
	{
		if (_pressingObjects.Count > 0)
		{
			// 목록에 있는 콜라이더 중 파괴(null)되었거나 비활성화된 것이 있다면 목록에서 제거
			if (_pressingObjects.RemoveWhere(col => col == null || !col.gameObject.activeInHierarchy) > 0)
			{
				CheckSwitchState();
			}
		}
	}
	// 스위치를 켤지 끌지 결정하는 핵심 로직
	private void CheckSwitchState()
	{
		// 누르고 있는 물체가 1개 이상이면 스위치 ON
		bool shouldBeOn = _pressingObjects.Count > 0;

		// 상태가 변했을 때만 실행 (불필요한 연산 방지)
		if (isOn != shouldBeOn)
		{
			isOn = shouldBeOn;
			UpdateVisual();
		}
	}
}
