using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class SynopsisUIInput : BaseInput, Platformer.IUIActions
{
	[SerializeField]SynopsisManager synopsisManager;

	private bool _isCallbackRegistered = false;
	private void Awake()
	{
		synopsisManager = GetComponent<SynopsisManager>();
	}
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
	public void OnSubmit(InputAction.CallbackContext context)
	{
		Debug.Log("submit call");
		if(context.performed)
		{
			synopsisManager.AdvancePage();
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
