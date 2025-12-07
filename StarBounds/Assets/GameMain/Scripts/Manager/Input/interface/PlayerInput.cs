
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerInput : BaseInput,Platformer.IPlayerActions
{
	Platformer input;

	private void OnEnable()
	{
		//TODO 언젠가 start의 내용을 이곳에 넣기
	}
	private void Start()
	{
		if (input == null)
		{
			input = InputManager.Instance.Actions;
		}
		if (input != null)
		{
			input.Player.SetCallbacks(this);
			input.Player.Enable();
		}
	}
	private void OnDisable()
	{
		input.Player.Disable();
		input.Player.SetCallbacks(null);
	}
	
	public void OnMove(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			MoveDir = new Vector2(context.ReadValue<float>(),0);
		}
		if(context.canceled)
		{
			MoveDir = new Vector2(0, 0);
		}
		// Move를 1D Axis로 만들었으면 float로 읽고, 2D면 Vector2로 읽으면 된다.
		// 여기서는 1D 가정 (← -1, 0, 1 →)
		

		// BaseInput에 저장
		
	}

	public void OnJump(InputAction.CallbackContext context)
	{
		if (context.started)
		{
			OnJumpaction();
		}
	}

	//클릭을 하면
	public void OnInteract(InputAction.CallbackContext context)
	{
		if (context.started)
		{
			OnInteraction();
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
			OnDropaction();
		}
	}

	public void OnPause(InputAction.CallbackContext context)
	{
		if(context.started)
		{
			OnPauseAction();
		}
	}
}
