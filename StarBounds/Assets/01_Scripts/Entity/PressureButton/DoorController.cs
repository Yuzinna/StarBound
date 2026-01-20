using UnityEngine;

/// <summary>
/// PressureButton의 상태 변화에 따라 문 오브젝트를 활성화/비활성화(사라지게/나타나게) 합니다.
/// </summary>
public class DoorController : MonoBehaviour
{
	// [연결] 씬에 있는 압력 버튼 오브젝트를 여기에 드래그하여 연결합니다.
	[Tooltip("씬에 있는 PressureButton 오브젝트를 여기에 드래그")]
	public PressureButton pressureButton;

	// 문이 사라질 때/나타날 때의 상태를 정의합니다.
	[Tooltip("체크 시: 버튼을 누르면 문이 사라짐 (일반적인 문 작동)")]
	public bool openWhenPressed = true;

	void Start()
	{
		if (pressureButton == null)
		{
			Debug.LogError("[DoorController] PressureButton이 연결되지 않았습니다! 문 작동 불가.");
			return;
		}

		// 버튼 이벤트 구독
		pressureButton.OnButtonStateChanged += OnButtonPress;

		// 초기 상태 설정: 문은 기본적으로 활성화(보이는) 상태로 시작합니다.
		// gameObject.SetActive(true); // 보통 유니티 초기 상태 그대로 사용
	}

	private void OnButtonPress(bool isPressed)
	{
		bool targetState;

		if (openWhenPressed)
		{
			// '버튼을 누르면 문이 사라지는' 일반적인 경우:
			// isPressed가 true -> 문은 false(사라짐)
			// isPressed가 false -> 문은 true(나타남)
			targetState = !isPressed;
		}
		else
		{
			// '버튼을 누르면 문이 나타나는' 반전된 경우:
			// isPressed가 true -> 문은 true(나타남)
			targetState = isPressed;
		}

		// **문 오브젝트의 활성화 상태를 설정합니다. (사라지게/나타나게)**
		gameObject.SetActive(targetState);

		Debug.Log($"[DoorController] 버튼 상태: {isPressed}. 문 활성화 상태: {targetState}.");
	}

	// 이 방식은 Update나 코루틴이 필요 없으므로 MoveDoor 함수는 삭제합니다.
}