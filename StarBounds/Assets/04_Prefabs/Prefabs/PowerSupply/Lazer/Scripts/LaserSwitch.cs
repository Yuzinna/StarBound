using System.Collections;
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

	// 💡 여기에 딜레이 변수만 추가했습니다!
	[Header("딜레이 설정")]
	[Tooltip("발을 떼고 레이저가 꺼질 때까지의 대기 시간 (초)")]
	public float offDelay = 1.0f;

	private HashSet<Collider2D> _pressingObjects = new HashSet<Collider2D>();
	// 켜고 끌 레이저 오브젝트를 연결할 변수
	public Laser firstLaser;

	// 💡 코루틴(타이머)을 기억할 변수 추가
	private Coroutine _offTimer;

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
		if (spriteRenderer != null)
		{
			spriteRenderer.sprite = isOn ? switchOnSprite : switchOffSprite;
		}
	}

	// 누군가 스위치 영역에 들어왔을 때
	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (((1 << collision.gameObject.layer) & pressableLayers) != 0)
		{
			_pressingObjects.Add(collision); // 목록에 추가
			CheckSwitchState();
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
		}
	}

	// 안전장치: 스위치 위에서 큐브가 파괴되거나 비활성화되는 경우를 대비
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

	// 💡 레이저를 켜고 끄는 로직을 타이머와 연동되게 수정했습니다!
	private void CheckSwitchState()
	{
		// 누르고 있는 물체가 1개 이상이면 켜져야 함
		bool shouldBeOn = _pressingObjects.Count > 0;

		// 1. 스위치가 켜져야 할 때 (누군가 밟음)
		if (shouldBeOn)
		{
			// 꺼지려고 카운트다운 중이었다면 취소!
			if (_offTimer != null)
			{
				StopCoroutine(_offTimer);
				_offTimer = null;
			}

			if (!isOn)
			{
				isOn = true;
				UpdateVisual();
				if (firstLaser != null && !firstLaser.gameObject.activeSelf)
				{
					firstLaser.gameObject.SetActive(true); // 레이저 즉시 켜기
				}
			}
		}
		// 2. 스위치가 꺼져야 할 때 (모두 발을 뗌) -> 바로 끄지 않고 딜레이 시작!
		else if (!shouldBeOn && isOn)
		{
			if (_offTimer == null) // 타이머가 안 돌고 있을 때만 시작
			{
				_offTimer = StartCoroutine(DelayedOffRoutine());
			}
		}
	}

	// 💡 지정된 시간(offDelay)을 기다렸다가 레이저를 끄는 코루틴
	private IEnumerator DelayedOffRoutine()
	{
		yield return new WaitForSeconds(offDelay);

		// 시간이 다 지나면 비로소 스위치와 레이저 끄기
		isOn = false;
		UpdateVisual();

		if (firstLaser != null)
		{
			firstLaser.TurnOffSequence();
		}

		_offTimer = null; // 타이머 초기화
	}
}