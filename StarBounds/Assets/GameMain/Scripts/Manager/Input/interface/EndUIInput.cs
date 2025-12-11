using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class EndUIInput : BaseInput, Platformer.IUIActions
{
	public EndUI EndUI;

	private bool _isCallbackRegistered = false;

	private void OnEnable()
	{
		//콜백 함수가 등록이 안되었다면
		if(!_isCallbackRegistered)
		{
			InputManager.Instance.Actions.UI.AddCallbacks(this);
			_isCallbackRegistered = true;
		}
		else
		{
			StartCoroutine(WaitForInputManagerAndRegister());
		}
	}
	private void OnDisable()
	{
		if(InputManager.Instance!= null&& _isCallbackRegistered)
		{
			InputManager.Instance.Actions.UI.RemoveCallbacks(this);
			_isCallbackRegistered = false;
		}
	}
	private IEnumerator WaitForInputManagerAndRegister()
	{
		while(InputManager.Instance ==null)
		{
			yield return null;
		}
		if(this.enabled && !_isCallbackRegistered)
		{
			InputManager.Instance.Actions.UI.AddCallbacks(this);
			_isCallbackRegistered = true;
		}
	}
	private void Awake()
	{
		EndUI = GetComponent<EndUI>();
	}
	public void OnSubmit(InputAction.CallbackContext context)
	{
		if(context.performed)
		{
			EndUI.ExitGame();
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

	public void OnNavigate(InputAction.CallbackContext context)
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
