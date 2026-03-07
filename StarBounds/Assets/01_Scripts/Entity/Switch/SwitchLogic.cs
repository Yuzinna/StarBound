using UnityEngine;

using System; // Action 타입을 사용하려면 필요 (GravityManager 이벤트 정의 시 사용됨)

public enum eSwitch
{
	Floating,
	Inverse
}
public enum eSwitchOrientation
{
	// 똑바로 서 있는 상태 (중력 방향: Normal)
	Upright,
	// 거꾸로 매달린 상태 (중력 방향: Inverse)
	Inverted
}

public class SwitchLogic : MonoBehaviour, IInteractable
{
	// ... (기존 변수 선언은 유지) ...

	[Header("스위치 종류")]
	[SerializeField] public eSwitch switchType; // ⬅️ 이 스위치의 기능 설정

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
	[SerializeField, Range(0f, 1f)] private float sfxVolume = 1f;

	// =========================================================================
	// Unity Lifecycle / Initialization
	// =========================================================================


	private void Awake()
	{
		Top=transform.Find("Top");
	}
	private void Start()
	{
		Initialize();
		SubscribeToGravityEvents(); //이벤트 구독 호출
	}

	private void OnDestroy()
	{
		UnsubscribeFromGravityEvents(); //오브젝트 파괴 시 구독 해제
	}

	public void Initialize()
	{
		

		//현재 GravityManager 상태에 맞춰 _isOn 초기화 (기존 로직 유지)
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
	private void SetIsOn(bool newState)
	{
		if (_isOn == newState) return; //

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

		// 💡 스위치 타입에 맞는 이벤트만 구독합니다.
		if (switchType == eSwitch.Floating)
		{
			// Floating 상태 변화 이벤트 구독
			GravityManager.Instance.OnFloatingStateChanged += OnFloatingStateChange;
		}
		else if (switchType == eSwitch.Inverse)
		{
			// 중력 방향 변화 이벤트 구독
			GravityManager.Instance.OnGravityDirectionChanged += OnGravityDirectionChange;
		}
	}

	private void UnsubscribeFromGravityEvents()
	{
		if (GravityManager.Instance == null) return;

		if (switchType == eSwitch.Floating)
		{
			GravityManager.Instance.OnFloatingStateChanged -= OnFloatingStateChange;
		}
		else if (switchType == eSwitch.Inverse)
		{
			GravityManager.Instance.OnGravityDirectionChanged -= OnGravityDirectionChange;
		}
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

	// 💡 초기화 및 이벤트 발생 시 호출되는 상태 업데이트 로직 통합
	private void UpdateStateFromGravityManager()
	{
		if (GravityManager.Instance == null) return;

		if (switchType == eSwitch.Floating)
		{
			_isOn = GravityManager.Instance.IsFloatingEnabled;
		}
		else if (switchType == eSwitch.Inverse)
		{
			_isOn = GravityManager.Instance.CurrentDirection == eGravityDirection.Inverse;
		}
	}

	// =========================================================================
	// Interaction and Effect (기존 로직 유지)
	// =========================================================================

	public void Interact(PlayerLogic player)
	{
		if (GravityManager.Instance == null) return;

		// 핵심 로직: 현재 스위치 위치와 중력 방향이 일치하는지 확인
		bool isInteractable = CheckInteractionCondition(GravityManager.Instance.CurrentDirection);

		if (!isInteractable)
		{
			// 조건이 일치하지 않으면 상호작용을 무시하고 종료
			Debug.LogWarning($"[SwitchLogic-{switchType}] Cannot interact. Gravity direction is not aligned with switch orientation.");
			return;
		}
		// 💡 상호작용 시에는 _isOn을 토글하고 효과 적용 (이벤트 발생)
		bool targetState = !_isOn;
		ApplySwitchEffect(targetState);

		// Note: UpdateVisual()은 이벤트 핸들러(On...Change)에서 호출되므로, 
		// 여기서는 제거하는 것이 깔끔하지만, 현재 GravityManager 코드를 모르므로 일단 유지합니다.
		// GravityManager에서 SetDirection이나 SetFloatingMode 호출 시 이벤트가 발생하고, 
		// 그 이벤트가 On...Change를 호출하여 UpdateVisual()을 실행하는 것이 가장 이상적입니다.
		// 현재 코드에서는 Interact에서 직접 UpdateVisual()을 호출하도록 유지하겠습니다.
		UpdateVisual();

		Debug.Log($" isOn = {_isOn}");
	}
	private bool CheckInteractionCondition(eGravityDirection currentDirection)
	{
		// 1. 스위치가 '똑바로 서 있는' 경우: 중력 방향이 Normal일 때만 상호작용 가능
		if (orientation == eSwitchOrientation.Upright)
		{
			return currentDirection == eGravityDirection.Normal;
		}
		// 2. 스위치가 '거꾸로 매달린' 경우: 중력 방향이 Inverse일 때만 상호작용 가능
		else if (orientation == eSwitchOrientation.Inverted)
		{
			return currentDirection == eGravityDirection.Inverse;
		}

		// 기본적으로 true 반환 (혹시 모를 예외 처리)
		return true;
	}
	private void ApplySwitchEffect(bool targetState)
	{
		if (GravityManager.Instance == null) return;

		switch (switchType)
		{
			case eSwitch.Floating:
				// GravityManager가 SetFloatingMode 호출 시 OnFloatingStateChanged 이벤트 발생 필요
				GravityManager.Instance.SetFloatingMode(targetState);
				break;

			case eSwitch.Inverse:
				eGravityDirection direction = targetState ? eGravityDirection.Inverse : eGravityDirection.Normal;
				// GravityManager가 SetDirection 호출 시 OnGravityDirectionChanged 이벤트 발생 필요
				GravityManager.Instance.SetDirection(direction);
				break;
			default:
				break;
		}
	}
	private void PlaySwitchSfx(bool isOn)
	{
		if (SfxManager.Instance == null) return;

		var clip = isOn ? switchOnClip : switchOffClip;
		var offset = isOn ? 0.5f : 0.5f;
		SfxManager.Instance.PlaySfx(clip, sfxVolume,offset);
	}
	private void UpdateVisual()
	{

		// _isOn 상태에 따라 스프라이트 교체
		if (_isOn)
		{
			Top.GetComponent<SpriteRenderer>().sprite = spriteOn[0];
		}
		else
		{
			Top.GetComponent<SpriteRenderer>().sprite = spriteOff[0];
		}
	}
	// =========================================================================
	// [새로 추가] 레이저 전력 공급용 함수
	// =========================================================================
	public void SetLaserPower(bool powerOn)
	{
		// 이미 원하는 상태(켜짐/꺼짐)와 똑같다면 무시
		if (_isOn == powerOn) return;

		// 전력이 들어오면 켜고, 끊기면 끄는 효과 적용!
		ApplySwitchEffect(powerOn);
	}
}