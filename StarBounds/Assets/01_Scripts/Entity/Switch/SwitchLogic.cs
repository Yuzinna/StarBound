using UnityEngine;
using System;

public enum eSwitch
{
	Floating,
	Inverse
}
public enum eSwitchOrientation
{
	Upright,
	Inverted
}

public class SwitchLogic : MonoBehaviour, IInteractable
{
	[Header("스위치 종류")]
	[SerializeField] public eSwitch switchType;

	[Header("스위치 위치")]
	[SerializeField] public eSwitchOrientation orientation;

	[Header("비주얼 옵션")]
	[SerializeField] private Sprite[] spriteOn;
	[SerializeField] private Sprite[] spriteOff;

	Transform Top;
	private bool _isOn;

	[Header("SFX")]
	[SerializeField] private AudioClip switchOnClip;
	[SerializeField] private AudioClip switchOffClip;
	[Tooltip("레이저 때문에 잠겨서 안 눌릴 때 나는 에러 소리 (삐빅!)")]
	[SerializeField] private AudioClip errorClip; // ⬅️ 에러 사운드 추가!
	[SerializeField, Range(0f, 1f)] private float sfxVolume = 1f;

	// =========================================================================
	// Unity Lifecycle / Initialization
	// =========================================================================
	private void Awake()
	{
		Top = transform.Find("Top");
	}

	private void Start()
	{
		Initialize();
		SubscribeToGravityEvents();
	}

	private void OnDestroy()
	{
		UnsubscribeFromGravityEvents();
	}

	public void Initialize()
	{
		if (GravityManager.Instance != null)
		{
			UpdateStateFromGravityManager();
		}
		else
		{
			_isOn = false;
		}
		UpdateVisual();
	}

	// 매니저에서 이벤트가 오면 상태, 비주얼, 사운드를 한 번에 업데이트!
	private void SetIsOn(bool newState)
	{
		if (_isOn == newState) return;

		_isOn = newState;
		UpdateVisual();
		PlaySwitchSfx(_isOn);
	}

	// =========================================================================
	// Gravity Manager Event Handling
	// =========================================================================
	private void SubscribeToGravityEvents()
	{
		if (GravityManager.Instance == null) return;

		if (switchType == eSwitch.Floating)
			GravityManager.Instance.OnFloatingStateChanged += OnFloatingStateChange;
		else if (switchType == eSwitch.Inverse)
			GravityManager.Instance.OnGravityDirectionChanged += OnGravityDirectionChange;
	}

	private void UnsubscribeFromGravityEvents()
	{
		if (GravityManager.Instance == null) return;

		if (switchType == eSwitch.Floating)
			GravityManager.Instance.OnFloatingStateChanged -= OnFloatingStateChange;
		else if (switchType == eSwitch.Inverse)
			GravityManager.Instance.OnGravityDirectionChanged -= OnGravityDirectionChange;
	}

	private void OnFloatingStateChange(bool isEnabled)
	{
		SetIsOn(isEnabled);
		Debug.Log($"[SwitchLogic-{switchType}] State updated via event: {_isOn}");
	}

	private void OnGravityDirectionChange(eGravityDirection newDirection)
	{
		SetIsOn(newDirection == eGravityDirection.Inverse);
		Debug.Log($"[SwitchLogic-{switchType}] State updated via event: {_isOn}");
	}

	private void UpdateStateFromGravityManager()
	{
		if (GravityManager.Instance == null) return;

		if (switchType == eSwitch.Floating)
			_isOn = GravityManager.Instance.IsFloatingEnabled;
		else if (switchType == eSwitch.Inverse)
			_isOn = GravityManager.Instance.CurrentDirection == eGravityDirection.Inverse;
	}

	// =========================================================================
	// 🔘 1. 수동 상호작용 (플레이어가 직접 누를 때)
	// ==========================================
	public void Interact(PlayerLogic player)
	{
		if (GravityManager.Instance == null) return;

		if (!CheckInteractionCondition(GravityManager.Instance.CurrentDirection))
		{
			Debug.LogWarning($"[SwitchLogic-{switchType}] 스위치 방향이 중력과 맞지 않아 누를 수 없습니다.");
			return;
		}

		bool targetState = !_isOn; // 켜져있으면 끄고, 꺼져있으면 켬

		// 💡 매니저에게 수동 조작을 "허락" 받습니다.
		bool success = TryApplyManualEffect(targetState);

		if (!success)
		{
			// ❌ 거절당함 (레이저 때문에 잠김) -> 에러 사운드 재생!
			if (SfxManager.Instance != null && errorClip != null)
			{
				SfxManager.Instance.PlaySfx(errorClip, sfxVolume, 0f);
			}
		}
		// 성공했다면(true)? -> GravityManager가 이벤트를 쏴서 SetIsOn이 자동으로 불리므로 여기서 비주얼 업데이트 안 해도 됨!
	}

	// =========================================================================
	// 🔫 2. 레이저 상호작용 (레이저가 닿거나 끊길 때)
	// ==========================================
	public void SetLaserPower(bool powerOn)
	{
		if (_isOn == powerOn) return;

		// 💡 레이저는 허락받지 않고 무조건 덮어쓰며 "잠가버립니다!"
		ApplyLaserEffect(powerOn);
	}

	// =========================================================================
	// 핵심 적용 로직 분리
	// =========================================================================

	// 수동 조작 요청
	private bool TryApplyManualEffect(bool targetState)
	{
		if (switchType == eSwitch.Floating)
		{
			return GravityManager.Instance.TryChangeFloatingManual(targetState);
		}
		else
		{
			eGravityDirection direction = targetState ? eGravityDirection.Inverse : eGravityDirection.Normal;
			return GravityManager.Instance.TryChangeDirectionManual(direction);
		}
	}

	// 레이저 강제 조작 (잠금 포함)
	private void ApplyLaserEffect(bool targetState)
	{
		if (switchType == eSwitch.Floating)
		{
			// 레이저가 켜지면(true) 잠금(true) / 꺼지면(false) 잠금해제(false)
			GravityManager.Instance.SetFloatingAndLock(targetState, targetState);
		}
		else
		{
			eGravityDirection direction = targetState ? eGravityDirection.Inverse : eGravityDirection.Normal;
			GravityManager.Instance.SetDirectionAndLock(direction, targetState);
		}
	}

	private bool CheckInteractionCondition(eGravityDirection currentDirection)
	{
		if (orientation == eSwitchOrientation.Upright) return currentDirection == eGravityDirection.Normal;
		else if (orientation == eSwitchOrientation.Inverted) return currentDirection == eGravityDirection.Inverse;
		return true;
	}

	private void PlaySwitchSfx(bool isOn)
	{
		if (SfxManager.Instance == null) return;

		var clip = isOn ? switchOnClip : switchOffClip;
		SfxManager.Instance.PlaySfx(clip, sfxVolume, 0.5f);
	}

	private void UpdateVisual()
	{
		if (_isOn) Top.GetComponent<SpriteRenderer>().sprite = spriteOn[0];
		else Top.GetComponent<SpriteRenderer>().sprite = spriteOff[0];
	}
}