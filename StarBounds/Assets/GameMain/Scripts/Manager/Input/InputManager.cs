// Assets/GameMain/Scripts/Input/InputManager.cs
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;



public enum InputContext
{
	None,
	Player,
	UI
}

/// 이 프로젝트 전용 입력 매니저.
/// - 새 Input System의 InputActionAsset을 사용.
/// - Move / Jump / Interact 같은 "이 게임만의 개념"을 여기서 정의.

public class InputManager : MonoBehaviour
{
	public static InputManager Instance { get; private set; }

	//inputActionAsset
	public Platformer Actions { get; set ; }

	public PlayerInput _input;

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}

		Instance = this;
		DontDestroyOnLoad(gameObject);

		Actions = new Platformer();
		_input = GetComponent<PlayerInput>();
	}

	private void OnEnable()
	{
		Actions.Enable();
		// 초기엔 예를 들어 Gameplay만 활성화하고 UI는 비활성화해도 됨
		Actions.UI.Disable();
	}
	private void OnDisable()
	{
		Actions.Disable();
	}
	
}
