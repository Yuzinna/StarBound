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

	// ==========================================
	// 💡 비주얼 및 자식 콜라이더 상태 제어
	// ==========================================
	private void UpdateVisual()
	{
		if (spriteRenderer != null)
			spriteRenderer.sprite = isOn ? switchOnSprite : switchOffSprite;

		if (bumpCollider != null)
		{
			if (GravityManager.Instance != null)
			{
				// 🔄 [수정됨] 
				// 플로팅 상태면 콜라이더를 켜서(true) 큐브가 못 누르게 물리적으로 막음
				// 플로팅 상태가 아니면 콜라이더를 꺼서(false) 큐브가 아래로 밟고 내려가 누를 수 있게 함
				bumpCollider.enabled = GravityManager.Instance.IsFloatingEnabled;
			}
			else
			{
				bumpCollider.enabled = !isOn;
			}
		}
	}

	// ==========================================
	// 🚨 1. 충돌 시작 (방어막 가동)
	// ==========================================
	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (((1 << collision.gameObject.layer) & pressableLayers) != 0)
		{
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

			if (col == null || !col.gameObject.activeInHierarchy)
			{
				toRemove.Add(col);
				needCheck = true;
				continue;
			}

			if (exitTime > 0f && (Time.time - exitTime > 0.15f))
			{
				toRemove.Add(col);
				needCheck = true;
			}
		}

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

				// 💡 [추가] 중력/플로팅 상태가 바뀌는 순간 자식 콜라이더 켜고 끄기를 즉시 반영합니다.
				UpdateVisual();
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
		bool hasRealPresser = false;

		foreach (var kvp in _pressingObjects)
		{
			if (kvp.Value > 0f) continue;

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