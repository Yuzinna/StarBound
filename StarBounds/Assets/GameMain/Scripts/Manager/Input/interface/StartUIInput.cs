using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
public class StartUIInput : BaseInput, Platformer.IUIActions
{
	public StartUI startUI;

	private bool _isCallbackRegistered = false;
	private void OnEnable()
	{
		// 중복 등록 방지 로직 추가
		if (!_isCallbackRegistered)
		{
			InputManager.Instance.Actions.UI.AddCallbacks(this);
			_isCallbackRegistered = true;
		}
		else
		{
			// 🚨 InputManager가 아직 null이면 등록을 시도하는 코루틴 시작
			// (코루틴은 MonoBehaviour가 활성화된 상태에서만 작동합니다.)
			StartCoroutine(WaitForInputManagerAndRegister());
		}
	}
	private void OnDisable()
	{
		// 이 스크립트가 비활성화되거나 파괴될 때만 콜백 제거
		if (InputManager.Instance != null && _isCallbackRegistered)
		{
			InputManager.Instance.Actions.UI.RemoveCallbacks(this);
			_isCallbackRegistered = false;
		}
	}
	// 📌 안전하게 InputManager 초기화를 기다리는 코루틴
	private IEnumerator WaitForInputManagerAndRegister()
	{
		// 매 프레임마다 InputManager가 초기화되었는지 확인합니다.
		while (InputManager.Instance == null)
		{
			yield return null; // 다음 프레임까지 대기
		}

		// 초기화가 완료되었을 때, 이 스크립트가 아직 활성화 상태라면 등록
		if (this.enabled && !_isCallbackRegistered)
		{
			InputManager.Instance.Actions.UI.AddCallbacks(this);
			_isCallbackRegistered = true;
		}
	}
	private void Awake()
	{
		startUI = FindAnyObjectByType<StartUI>();
	}
	public void OnSubmit(InputAction.CallbackContext context)
	{
		if (context.started && startUI != null) // 버튼 누르는 순간 실행
		{
			Debug.Log("submit excute");
			startUI.ExecuteSelectedButton();
		}
	}
	public void OnNavigate(InputAction.CallbackContext context)
	{
		if (context.performed&&startUI!=null)
		{
			
			Vector2 navInput = context.ReadValue<Vector2>();
			Debug.Log($"onnavigate excute{navInput.y}");
			// Y축(수직) 입력만 사용하여 버튼 인덱스 변경 함수 호출
			startUI.NavigateButtons(navInput.y);
		}
	}
	public void OnCancel(InputAction.CallbackContext context)
	{
		
	}
	public void OnClick(InputAction.CallbackContext context)
	{
		
	}
	public void OnEscape(InputAction.CallbackContext context)
	{
		
	}

	public void OnMiddleClick(InputAction.CallbackContext context)
	{
		
	}
	public void OnPoint(InputAction.CallbackContext context)
	{
		
	}
	public void OnRightClick(InputAction.CallbackContext context)
	{
		
	}
	public void OnScrollWheel(InputAction.CallbackContext context)
	{
		
	}
	public void OnTrackedDeviceOrientation(InputAction.CallbackContext context)
	{
		
	}

	public void OnTrackedDevicePosition(InputAction.CallbackContext context)
	{
		
	}
}
