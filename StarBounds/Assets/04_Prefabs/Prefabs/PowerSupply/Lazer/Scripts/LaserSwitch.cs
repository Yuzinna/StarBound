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

	[Header("스위치 눌림 연출 설정")]
	[Tooltip("스위치가 눌렸을 때 bumpCollider가 내려갈 고도 (유니티 단위)")]
	public float pressedYOffset = -0.2f;
	private Vector3 _initialColliderPos;
	private void Start()
	{
		if (GravityManager.Instance != null)
		{
			_wasFloatingEnabled = GravityManager.Instance.IsFloatingEnabled;
			_lastGravityDirection = GravityManager.Instance.CurrentDirection;
		}

		// 💡 시작할 때 bumpCollider의 원래 로컬 위치를 기억해 둡니다.
		if (bumpCollider != null)
		{
			_initialColliderPos = bumpCollider.transform.localPosition;
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
			// 🚨 일반 벽으로 평생 고정합니다. 
			// 이렇게 해야 큐브가 옆에서 올 때 파묻히지 않고 폴리곤 경사를 타고 "위로" 올라섭니다.
			bumpCollider.enabled = true;
			bumpCollider.isTrigger = false;

			// 💡 [핵심 추가] 스위치 상태에 따라 콜라이더의 높이를 실시간으로 조절합니다.
			if (isOn)
			{
				// 스위치가 눌렸으면 원래 위치에서 pressedYOffset만큼 아래로 내립니다.
				bumpCollider.transform.localPosition = _initialColliderPos + new Vector3(0, pressedYOffset, 0);
			}
			else
			{
				// 스위치가 안 눌렸으면 원래 높이로 복구합니다.
				bumpCollider.transform.localPosition = _initialColliderPos;
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
		// 🚨 [핵심 추가] 중력이 플로팅 상태라면, 위에 큐브가 수백 개가 올라와 있어도 
		// 무조건 무시하고 즉시 false를 반환해서 스위치가 안 눌리게 만듭니다.
		if (GravityManager.Instance != null && GravityManager.Instance.IsFloatingEnabled)
		{
			return false;
		}

		bool hasRealPresser = false;

		foreach (var kvp in _pressingObjects)
		{
			if (kvp.Value > 0f) continue;

			Collider2D col = kvp.Key;
			GravityObjectLogic cube = col.GetComponentInParent<GravityObjectLogic>();

			// 이제 여기는 일반 중력 상태일 때만 실행됩니다.
			if (cube != null)
			{
				if (cube.GetComponent<Rigidbody2D>().gravityScale > 0)
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