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

	[Header("물리적 턱 (스위치 높이)")]
	[Tooltip("스위치가 튀어나왔을 때 큐브가 타고 올라갈 콜라이더")]
	public Collider2D bumpCollider;

	[Header("스위치를 누를 수 있는 레이어")]
	public LayerMask pressableLayers;

	[Header("딜레이 설정")]
	[Tooltip("발을 떼고 레이저가 꺼질 때까지의 대기 시간 (초)")]
	public float offDelay = 2.0f;

	// 💡 [핵심] 그냥 목록이 아니라, "콜라이더가 진짜 나갔는지" 검사하기 위한 딕셔너리로 변경!
	private Dictionary<Collider2D, float> _pressingObjects = new Dictionary<Collider2D, float>();

	public Laser firstLaser;
	private Coroutine _offTimer;

	private bool _wasFloatingEnabled;
	private eGravityDirection _lastGravityDirection;

	private void Start()
	{
		if (GravityManager.Instance != null)
		{
			_wasFloatingEnabled = GravityManager.Instance.IsFloatingEnabled;
			_lastGravityDirection = GravityManager.Instance.CurrentDirection;
		}
		UpdateVisual();
	}

	public void Interact(PlayerLogic player)
	{
		isOn = !isOn;
		UpdateVisual();
		if (isOn) TurnOnLaser();
		else TurnOffLaser();
	}

	private void UpdateVisual()
	{
		if (spriteRenderer != null)
			spriteRenderer.sprite = isOn ? switchOnSprite : switchOffSprite;

		if (bumpCollider != null)
			bumpCollider.enabled = !isOn;
	}

	// ==========================================
	// 🚨 1. 충돌 시작 (방어막 가동)
	// ==========================================
	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (((1 << collision.gameObject.layer) & pressableLayers) != 0)
		{
			// 방금 Exit로 나갔다고 찍혔던 놈이 0.1초 만에 다시 들어왔다면?
			// "아, 버그로 튕긴 거구나!" 하고 퇴출 취소!
			if (_pressingObjects.ContainsKey(collision))
			{
				_pressingObjects[collision] = 0f; // 나갈 준비 취소
			}
			else
			{
				_pressingObjects.Add(collision, 0f); // 새 멤버 등록
			}

			CheckSwitchState();
		}
	}

	// ==========================================
	// 🚨 2. 충돌 종료 (바로 안 빼고 대기표 발급!)
	// ==========================================
	private void OnTriggerExit2D(Collider2D other)
	{
		if (_pressingObjects.ContainsKey(other))
		{
			// 유니티 버그일 수 있으니 즉시 삭제하지 않고, "너 나갈 거면 0.1초 뒤에 나가라"며 시간을 기록함.
			_pressingObjects[other] = Time.time;
		}
	}

	// ==========================================
	// 🚨 3. 실시간 검사 및 찌꺼기 청소
	// ==========================================
	private void Update()
	{
		bool needCheck = false;
		List<Collider2D> toRemove = new List<Collider2D>();

		foreach (var kvp in _pressingObjects)
		{
			Collider2D col = kvp.Key;
			float exitTime = kvp.Value;

			// 1. 진짜 부서졌거나 꺼진 물체는 즉각 청소
			if (col == null || !col.gameObject.activeInHierarchy)
			{
				toRemove.Add(col);
				needCheck = true;
				continue;
			}

			// 2. 버그 방어막: Exit가 찍힌 지 0.15초가 지났다면? "아, 진짜로 밖으로 나간 게 맞구나!" 확정 삭제
			if (exitTime > 0f && (Time.time - exitTime > 0.15f))
			{
				toRemove.Add(col);
				needCheck = true;
			}
		}

		// 확정된 놈들만 리스트에서 진짜로 삭제!
		foreach (Collider2D col in toRemove)
		{
			_pressingObjects.Remove(col);
		}

		// 중력 변화 검사
		if (GravityManager.Instance != null)
		{
			if (_wasFloatingEnabled != GravityManager.Instance.IsFloatingEnabled ||
				_lastGravityDirection != GravityManager.Instance.CurrentDirection)
			{
				_wasFloatingEnabled = GravityManager.Instance.IsFloatingEnabled;
				_lastGravityDirection = GravityManager.Instance.CurrentDirection;
				needCheck = true;
			}
		}

		if (needCheck)
		{
			CheckSwitchState();
		}
	}

	private bool HasValidWeightPressing()
	{
		// "나갈 예정(ExitTime > 0)"인 애들 빼고 진짜로 밟고 있는 애들이 있는지 확인
		bool hasRealPresser = false;

		foreach (var kvp in _pressingObjects)
		{
			if (kvp.Value > 0f) continue; // 얘는 곧 나갈 애니까 무시

			Collider2D col = kvp.Key;
			GravityObjectLogic cube = col.GetComponentInParent<GravityObjectLogic>();

			if (cube != null)
			{
				if (!GravityManager.Instance.IsFloatingEnabled && cube.GetComponent<Rigidbody2D>().gravityScale > 0)
				{
					hasRealPresser = true;
					break;
				}
			}
			else
			{
				if (!GravityManager.Instance.IsFloatingEnabled && GravityManager.Instance.CurrentDirection == eGravityDirection.Normal)
				{
					hasRealPresser = true;
					break;
				}
			}
		}

		return hasRealPresser;
	}

	private void CheckSwitchState()
	{
		bool shouldBeOn = HasValidWeightPressing();

		if (shouldBeOn)
		{
			if (_offTimer != null)
			{
				StopCoroutine(_offTimer);
				_offTimer = null;
			}

			if (!isOn)
			{
				isOn = true;
				UpdateVisual();
				TurnOnLaser();
			}
		}
		else if (!shouldBeOn && isOn)
		{
			if (_offTimer == null)
			{
				_offTimer = StartCoroutine(DelayedOffRoutine());
			}
		}
	}

	private void TurnOnLaser()
	{
		if (firstLaser != null) firstLaser.gameObject.SetActive(true);
	}

	private void TurnOffLaser()
	{
		if (firstLaser != null) firstLaser.TurnOffSequence();
	}

	private IEnumerator DelayedOffRoutine()
	{
		yield return new WaitForSeconds(offDelay);

		// 오프 딜레이가 다 끝났는데도 누군가 밟고 있다면? 끄지 않음!
		if (HasValidWeightPressing())
		{
			_offTimer = null;
			yield break;
		}

		isOn = false;
		UpdateVisual();
		TurnOffLaser();
		_offTimer = null;
	}
}