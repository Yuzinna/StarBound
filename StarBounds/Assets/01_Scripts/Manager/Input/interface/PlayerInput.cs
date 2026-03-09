
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerInput : BaseInput,Platformer.IPlayerActions
{
	private PlayerLogic plLogic;
	private PauseUI pauseUi;

	private bool _isCallbackRegistered = false;
	private void OnEnable()
	{
		// 1. 이미 등록되어 있다면 아무것도 안 함
		if (_isCallbackRegistered) return;

		// 2. InputManager가 이미 존재한다면 즉시 등록!
		if (InputManager.Instance != null && InputManager.Instance.Actions != null)
		{
			InputManager.Instance.Actions.Player.AddCallbacks(this);
			_isCallbackRegistered = true;
		}
		// 3. 아직 InputManager가 안 만들어졌다면 코루틴으로 기다림!
		else
		{
			StartCoroutine(WaitForInputManagerAndRegister());
		}
	}
	//InputManager 초기화를 기다리는 코루틴
	private IEnumerator WaitForInputManagerAndRegister()
	{
		// InputManager와 Actions가 완벽히 준비될 때까지 안전하게 대기
		while (InputManager.Instance == null || InputManager.Instance.Actions == null)
		{
			yield return null;
		}

		if (this.enabled && !_isCallbackRegistered)
		{
			InputManager.Instance.Actions.Player.AddCallbacks(this);
			_isCallbackRegistered = true;
		}
	}
	private void Awake()
	{
		plLogic = GetComponent<PlayerLogic>();
		if (plLogic == null)
		{
			Debug.LogError("PlayerInput requires a PlayerController on the same GameObject.");
		}
		
	}
	private void Start()
	{
		pauseUi = FindAnyObjectByType<PauseUI>();
	}
	private void OnDisable()
	{
		// 이 스크립트가 비활성화되거나 파괴될 때만 콜백 제거
		if (InputManager.Instance != null && _isCallbackRegistered)
		{
			InputManager.Instance.Actions.Player.RemoveCallbacks(this);
			_isCallbackRegistered = false;
		}
	}
	
	public void OnMove(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			MoveDir = new Vector2(context.ReadValue<float>(),0);
			plLogic._moveX = MoveDir.x;
		}
		if(context.canceled)
		{
			MoveDir = new Vector2(0, 0);
			plLogic._moveX = MoveDir.x;
		}
		// Move를 1D Axis로 만들었으면 float로 읽고, 2D면 Vector2로 읽으면 된다.
		// 여기서는 1D 가정 (← -1, 0, 1 →)
		
		// BaseInput에 저장
		
	}

	public void OnJump(InputAction.CallbackContext context)
	{
		if (context.started)
		{
			plLogic.OnInputJump();
		}
	}

	//클릭을 하면
	public void OnInteract(InputAction.CallbackContext context)
	{
		if (context.started)
		{
			plLogic.OnInputInteract();
		}
	}
	public void OnDrop(InputAction.CallbackContext context)
	{
		if (context.started)
		{
			plLogic.OnInputDropThrough();
		}
	}

	public void OnRestart(InputAction.CallbackContext context)
	{
		if (context.started)
		{
			// 이제 매니저가 재시작을 깔끔하게 처리합니다!
			if (GameManager.Instance != null)
				GameManager.Instance.RestartCurrentStage();
		}
	}
	public void OnPause(InputAction.CallbackContext context)
	{
		if(context.started)
		{
			// 매니저에게 일시정지를 켜고 끄라고(Toggle) 명령합니다!
			if (GameManager.Instance != null)
				GameManager.Instance.TogglePause();
		}
	}
}
