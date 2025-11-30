using GameFrameworkLite;
using UnityEngine;

public class PauseController : MonoBehaviour
{
	private UIModule _uiModule;
	[SerializeField] bool _isPaused = false;
	private const string PauseUIPath = "UI/PauseUI";

	private void Start()
	{
		_uiModule = GameFrameworkEntry.GetModule<UIModule>();
		InputManager.Instance._input.PauseAction += TogglePause;
		
	}
	private void OnDestroy()
	{
		if (InputManager.Instance != null)
		{
			InputManager.Instance._input.PauseAction -= TogglePause;
		}
	}
	private void TogglePause()
	{
		if (_isPaused)
			Resume();
		else
			Pause();
	}


	private void Pause()
	{
		//if (_isPaused)
		//	return;
		_uiModule.Open(PauseUIPath);
		_isPaused = true;
	}

	private void Resume()
	{
		//if (!_isPaused)
		//	return;
		_uiModule.Close(PauseUIPath);
		_isPaused = false;
	}
}
