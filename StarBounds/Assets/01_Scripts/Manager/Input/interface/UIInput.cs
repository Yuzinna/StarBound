using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIInput : BaseInput, Platformer.IUIActions
{
	// ✨ 새로 추가된 변수
	[SerializeField]private PauseUI pauseUI;

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
	private void Start()
	{
		pauseUI = FindAnyObjectByType<PauseUI>();
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
	public void OnNavigate(InputAction.CallbackContext context)
	{
		Debug.Log($"OnNavigate 호출됨: {context.ReadValue<Vector2>().y}");
		// context.started는 W/S 키를 누르기 시작했을 때 한 번만 발생합니다.
		if (context.performed && pauseUI != null)
		{
			// Navigate는 Vector2 값을 반환하므로 y축을 사용합니다.
			float verticalInput = context.ReadValue<Vector2>().y;

			// PauseMenuManager에 내비게이션 처리를 위임합니다.
			pauseUI.NavigateButtons(verticalInput);
		}
	}
	public void OnSubmit(InputAction.CallbackContext context)
	{
		// context.performed는 스페이스바를 누르기 시작했을 때 발생합니다.
		if (context.performed && pauseUI != null)
		{
			// PauseMenuManager에 버튼 실행 처리를 위임합니다.
			pauseUI.ExecuteSelectedButton();
		}
	}
	public void OnEscape(InputAction.CallbackContext context)
	{
		if (context.performed && pauseUI != null)
		{
			pauseUI.ResumeGame();
		}
	}
	public void OnCancel(InputAction.CallbackContext context)
	{
		
	}

	public void OnClick(InputAction.CallbackContext context)
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
