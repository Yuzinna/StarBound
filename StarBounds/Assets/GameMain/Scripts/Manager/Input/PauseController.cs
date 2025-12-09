
using UnityEngine;

public class PauseController : MonoBehaviour
{

	[SerializeField] bool _isPaused = false;
	private const string PauseUIPath = "UI/PauseUI";

	private void Start()
	{
		
		InputManager.Instance._plInput.PauseAction += TogglePause;
		
	}
	private void OnDestroy()
	{
		if (InputManager.Instance != null)
		{
			InputManager.Instance._plInput.PauseAction -= TogglePause;
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
		
		_isPaused = true;
	}

	private void Resume()
	{
		//if (!_isPaused)
		//	return;
		
		_isPaused = false;
	}
}
