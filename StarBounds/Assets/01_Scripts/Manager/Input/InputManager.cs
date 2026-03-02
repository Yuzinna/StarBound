// Assets/GameMain/Scripts/Input/InputManager.cs
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using static UnityEngine.Timeline.DirectorControlPlayable;



/// 이 프로젝트 전용 입력 매니저.
/// - 새 Input System의 InputActionAsset을 사용.
/// - Move / Jump / Interact 같은 "이 게임만의 개념"을 여기서 정의.

public class InputManager : MonoBehaviour
{
	public static InputManager Instance { get; private set; }

	//inputActionAsset
	public Platformer Actions { get; set ; }

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
		
		InitInputCallbacks();
	}
	private void InitInputCallbacks()
	{
		
		Actions.Enable(); // 모든 ActionMap을 고,
		Actions.UI.Disable(); // UI ActionMap은 명시적으로 끕니다.
	}

	/// <summary>
	/// 게임 플레이 모드로 전환 (UI를 끄고 Player ActionMap을 켭니다.)
	/// </summary>
	public void SwitchToGameplay()
	{
		Actions.UI.Disable();
		Actions.Player.Enable();
	}

	/// <summary>
	/// UI 메뉴 모드로 전환 (Player를 끄고 UI ActionMap을 켭니다.)
	/// </summary>
	public void SwitchToUI()
	{
		Actions.Player.Disable();
		Actions.UI.Enable();
	}
	private void OnEnable()
	{
		// InputManager 컴포넌트가 활성화될 때 Action 객체 전체를 활성화
		Actions?.Enable();
	}

	private void OnDisable()
	{
		// InputManager 컴포넌트가 비활성화될 때 Action 객체 전체를 비활성화 (메모리 정리 목적)
		Actions?.Disable();
	}

}
