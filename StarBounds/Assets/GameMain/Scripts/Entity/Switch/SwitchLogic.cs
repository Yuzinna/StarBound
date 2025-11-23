using UnityEngine;
using GameFrameworkLite;
using System; // Action 타입을 사용하려면 필요 (GravityManager 이벤트 정의 시 사용됨)

public enum eSwitch
{
	Floating,
	Inverse
}

public class SwitchLogic : EntityLogic, IInteractable
{
	// ... (기존 변수 선언은 유지) ...

	[Header("스위치 종류")]
	[SerializeField] public eSwitch switchType; // ⬅️ 이 스위치의 기능 설정

	[Header("비주얼 옵션")]
	[SerializeField] private SpriteRenderer spriteRenderer;
	[SerializeField] private Sprite spriteOn;
	[SerializeField] private Sprite spriteOff;


	private bool _isOn;

	// =========================================================================
	// Unity Lifecycle / Initialization
	// =========================================================================

	public override void OnInit(object userData)
	{
		// EntityLogic의 OnInit은 Start 전에 호출될 수 있으나,
		// 여기서는 Start에서 모든 초기화를 진행합니다.
	}

	private void Start()
	{
		Initialize();
		SubscribeToGravityEvents(); // 💡 이벤트 구독 호출
	}

	private void OnDestroy()
	{
		UnsubscribeFromGravityEvents(); // 💡 오브젝트 파괴 시 구독 해제
	}

	public void Initialize()
	{
		if (spriteRenderer == null)
			spriteRenderer = GetComponent<SpriteRenderer>();

		// 💡 현재 GravityManager 상태에 맞춰 _isOn 초기화 (기존 로직 유지)
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

	// 💡 Floating 이벤트 핸들러
	private void OnFloatingStateChange(bool isEnabled)
	{
		// _isOn을 이벤트 값으로 업데이트하고 비주얼 업데이트
		_isOn = isEnabled;
		UpdateVisual();
		Debug.Log($"[SwitchLogic-{switchType}] State updated via event: {_isOn}");
	}

	// 💡 Inverse 이벤트 핸들러
	private void OnGravityDirectionChange(eGravityDirection newDirection)
	{
		// Inverse 스위치는 방향이 Inverse일 때만 켜짐
		_isOn = (newDirection == eGravityDirection.Inverse);
		UpdateVisual();
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

		// 💡 상호작용 시에는 _isOn을 토글하고 효과 적용 (이벤트 발생)
		_isOn = !_isOn;
		ApplySwitchEffect(_isOn);

		// Note: UpdateVisual()은 이벤트 핸들러(On...Change)에서 호출되므로, 
		// 여기서는 제거하는 것이 깔끔하지만, 현재 GravityManager 코드를 모르므로 일단 유지합니다.
		// GravityManager에서 SetDirection이나 SetFloatingMode 호출 시 이벤트가 발생하고, 
		// 그 이벤트가 On...Change를 호출하여 UpdateVisual()을 실행하는 것이 가장 이상적입니다.
		// 현재 코드에서는 Interact에서 직접 UpdateVisual()을 호출하도록 유지하겠습니다.
		UpdateVisual();

		Debug.Log($" isOn = {_isOn}");
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

	private void UpdateVisual()
	{
		if (spriteRenderer == null) return;

		// _isOn 상태에 따라 스프라이트 교체
		if (_isOn)
		{
			spriteRenderer.sprite = spriteOn;
		}
		else
		{
			spriteRenderer.sprite = spriteOff;
		}
	}
}