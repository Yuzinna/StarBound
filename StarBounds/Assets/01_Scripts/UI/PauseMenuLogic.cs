using UnityEngine;
using UnityEngine.UI; // UI(버튼, 슬라이더)를 제어하기 위해 필수!
using UnityEngine.SceneManagement; // 씬 이동을 위해 필수!

public class PauseMenuLogic : MonoBehaviour
{
	[Header("UI 화면 연결")]
	[Tooltip("일시정지 화면 전체를 담고 있는 부모 패널을 넣으세요.")]
	public GameObject pauseMenuUI;

	[Header("사운드 설정")]
	public Slider volumeSlider; // 중앙에 배치할 볼륨 슬라이더

	private bool _isPaused = false;

	private void Start()
	{
		// 1. 시작할 때 슬라이더의 위치를 현재 게임의 실제 볼륨과 똑같이 맞춰줍니다.
		if (volumeSlider != null)
		{
			volumeSlider.value = AudioListener.volume;

			// 슬라이더를 마우스로 움직일 때마다 SetVolume 함수가 자동으로 실행되도록 연결!
			volumeSlider.onValueChanged.AddListener(SetVolume);
		}

		// 2. 게임 시작 시 일시정지 UI는 숨겨둡니다.
		if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
	}

	private void Update()
	{
		// ESC 키를 누르면 일시정지 창을 띄우거나 닫습니다. (원하는 키로 변경 가능)
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			if (_isPaused) ResumeGame();
			else PauseGame();
		}
	}

	// ==========================================
	// 🔘 버튼 및 기능 로직
	// ==========================================

	// 게임 일시정지 실행
	public void PauseGame()
	{
		if (pauseMenuUI != null) pauseMenuUI.SetActive(true); // UI 켜기
		Time.timeScale = 0f; // 유니티 세상의 시간 흐름을 완전히 멈춤!
		_isPaused = true;

		// 기획자님이 만들어두신 게임매니저 정지 상태 업데이트 (코기 조작 방지)
		if (GameManager.Instance != null) GameManager.Instance.IsPaused = true;
	}

	// [RESUME 버튼용] 게임 재개
	public void ResumeGame()
	{
		if (pauseMenuUI != null) pauseMenuUI.SetActive(false); // UI 끄기
		Time.timeScale = 1f; // 시간 흐름 정상화
		_isPaused = false;

		if (GameManager.Instance != null) GameManager.Instance.IsPaused = false;
	}

	// [MAINMENU 버튼용] 메인 화면으로 이동
	public void GoToMainMenu()
	{
		// 🚨 아주 중요: 씬을 이동하기 전에 반드시 멈췄던 시간을 다시 흐르게 해줘야 합니다!
		Time.timeScale = 1f;
		if (GameManager.Instance != null) GameManager.Instance.IsPaused = false;

		// "MainMenu" 라는 이름의 씬으로 이동합니다. (실제 메인 메뉴 씬 이름으로 바꿔주세요!)
		SceneManager.LoadScene("MainMenu");
	}

	// [볼륨 슬라이더용] 마스터 볼륨 조절
	private void SetVolume(float volume)
	{
		// AudioListener.volume은 유니티 게임 전체의 최종 볼륨을 조절합니다 (0.0 ~ 1.0)
		AudioListener.volume = volume;
	}
}