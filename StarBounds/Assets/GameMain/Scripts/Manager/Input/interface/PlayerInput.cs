
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
		// 중복 등록 방지 로직 추가
		if (!_isCallbackRegistered)
		{
			InputManager.Instance.Actions.Player.AddCallbacks(this);
			_isCallbackRegistered = true;
		}
		else
		{
			// 🚨 InputManager가 아직 null이면 등록을 시도하는 코루틴 시작
			// (코루틴은 MonoBehaviour가 활성화된 상태에서만 작동합니다.)
			StartCoroutine(WaitForInputManagerAndRegister());
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

	public void OnRestart(InputAction.CallbackContext context)
	{
		if (context.started)
		{
			Debug.Log("[PlayerInput] Restart input received");

			// 1. **현재 활성화된 씬의 이름을 가져옵니다.**
			// 이것이 유니티가 현재 실행하려는 씬의 이름을 알려주는 함수입니다.
			string currentSceneName = SceneManager.GetActiveScene().name;

			// 2. **해당 씬을 다시 로드하여 스테이지를 재시작합니다.**
			// LoadScene은 동기 방식으로 씬을 로드합니다.
			SceneManager.LoadScene(currentSceneName);
		}
	}

	public void OnDrop(InputAction.CallbackContext context)
	{
		if (context.started)
		{
			plLogic.OnInputDropThrough();
		}
	}

	public void OnPause(InputAction.CallbackContext context)
	{
		if(context.started)
		{
			pauseUi.PauseGame();
		}
	}
}
