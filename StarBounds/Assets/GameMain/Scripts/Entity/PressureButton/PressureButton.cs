using UnityEngine;

public class PressureButton : MonoBehaviour
{
	// ... (기존 스프라이트, 오프셋, 태그 설정 변수들은 동일)
	public Sprite defaultSprite;
	public Sprite pressedSprite;
	
	// **[추가] 감지 역할을 할 Trigger Collider를 Inspector에서 연결해야 합니다.**
	[Header("Collider Setup")]
	[Tooltip("감지 역할을 할 (Is Trigger이 체크된) 콜라이더를 연결하세요.")]
	public Collider2D triggerSensor;

	private SpriteRenderer spriteRenderer;
	private int objectCount = 0;
	private bool isPressed = false;
	[Tooltip("밟는 오브젝트(FootTrigger)의 레이어 번호를 설정하세요. (예: Foot 레이어)")]
	public LayerMask requiredLayer;

	public System.Action<bool> OnButtonStateChanged;

	void Awake()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();

		if (spriteRenderer == null || triggerSensor == null)
		{
			Debug.LogError("[PressureButton] SpriteRenderer와 Trigger Sensor Collider가 모두 연결되어야 합니다!");
			return;
		}

		// Solid Collider는 건드리지 않습니다. Trigger Sensor만 사용할 것입니다.
		if (!triggerSensor.isTrigger)
		{
			Debug.LogError("[PressureButton] 연결된 Trigger Sensor는 반드시 Is Trigger가 체크되어야 합니다.");
		}

		spriteRenderer.sprite = defaultSprite;
	}

	// (UpdateButtonState 및 IsAllowedObject 함수는 이전과 동일)
	// ...

	// OnTriggerEnter2D는 **Trigger Sensor**에서 발생한 이벤트만 처리합니다.
	private void OnTriggerEnter2D(Collider2D other)
	{
		// 1. **레이어로 충돌 필터링 (가장 안정적인 방법)**
		if (!IsAllowedLayer(other)) return;

		objectCount++;		
		UpdateButtonState();
 	}

	private void OnTriggerExit2D(Collider2D other)
	{
		if (!IsAllowedLayer(other)) return;

		if (objectCount > 0)
		{
			objectCount--;
			UpdateButtonState();
		}
	}

	// 이 오브젝트가 허용된 태그를 가졌는지 확인하는 헬퍼 함수 (이전과 동일)
	private bool IsAllowedLayer(Collider2D other)
	{
		
		return (requiredLayer.value & (1 << other.gameObject.layer)) != 0;
	}

	private void UpdateButtonState()
	{
		bool shouldBePressed = objectCount > 0;

		if (shouldBePressed != isPressed)
		{
			isPressed = shouldBePressed;
			spriteRenderer.sprite = isPressed ? pressedSprite : defaultSprite;
			OnButtonStateChanged?.Invoke(isPressed);
		}
	}
}