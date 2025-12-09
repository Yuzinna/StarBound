using UnityEngine;
using UnityEngine.InputSystem;

public class UIInput : BaseInput, Platformer.IUIActions
{
	// ✨ 새로 추가된 변수
	private PauseUI pauseUI;
	

	private void OnEnable()
	{
		//TODO 언젠가 start의 내용을 이곳에 넣기
	}
	private void Start()
	{
		pauseUI = FindAnyObjectByType<PauseUI>();
	}
	private void OnDisable()
	{
		
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
