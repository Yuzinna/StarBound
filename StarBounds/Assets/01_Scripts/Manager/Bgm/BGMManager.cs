using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BgmManager : MonoBehaviour
{
	public static BgmManager Instance { get; private set; }

	[Header("BGM Clips")]
	[SerializeField] private AudioClip menuBgm;   // Start + Synopsis
	[SerializeField] private AudioClip mapBgm;    // 모든 게임맵

	[Header("Volumes")]
	public float _volume = 0.5f;
	[Range(0f, 1f)][SerializeField] private float menuVolume = 0.8f;
	[Range(0f, 1f)][SerializeField] private float mapVolume = 0.8f;

	[Header("Scene Rule")]
	[Tooltip("이 접두사로 시작하는 씬은 전부 게임맵으로 취급")]
	[SerializeField] private string mapScenePrefix = "Stage"; // 예: Stage1, Stage2...

	[SerializeField] private AudioSource bgmSource;

	private void Awake()
	{
		if (Instance != null) { Destroy(gameObject); return; }
		Instance = this;
		DontDestroyOnLoad(gameObject);

		if (bgmSource == null) bgmSource = GetComponent<AudioSource>();
		bgmSource.loop = true;
		bgmSource.playOnAwake = false;
	}

	private void OnEnable()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnDisable()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	private void Start()
	{
		ApplyBgmForScene(SceneManager.GetActiveScene().name);
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		ApplyBgmForScene(scene.name);
	}

	private void ApplyBgmForScene(string sceneName)
	{
		bool isMap = sceneName.StartsWith(mapScenePrefix);

		AudioClip targetClip = isMap ? mapBgm : menuBgm;
		float targetVol = isMap ? mapVolume : menuVolume;

		PlayIfDifferent(targetClip, targetVol);
	}

	private void PlayIfDifferent(AudioClip clip, float volume)
	{
		if (clip == null || bgmSource == null) return;

		// ✅ 같은 곡이면 재시작하지 않고 볼륨만 반영
		if (bgmSource.clip == clip && bgmSource.isPlaying)
		{
			bgmSource.volume = volume * _volume;
			return;
		}

		bgmSource.clip = clip;
		bgmSource.volume = volume * _volume;
		bgmSource.Play();
	}

	// ==========================================
	// 💡 [추가] 슬라이더와 실시간 연동을 위한 함수
	// ==========================================
	public void SetMasterVolume(float sliderValue)
	{
		// 1. 슬라이더의 값(0~1)을 마스터 볼륨인 _volume 변수에 저장합니다.
		_volume = sliderValue;

		// 2. 현재 재생 중인 브금이 있다면, 바뀐 볼륨을 즉시 실시간으로 업데이트합니다.
		if (bgmSource != null && bgmSource.clip != null)
		{
			// 현재 어떤 씬이냐에 따라 원래 할당되어야 할 기본 볼륨(기본값)을 찾아 계산합니다.
			string currentSceneName = SceneManager.GetActiveScene().name;
			bool isMap = currentSceneName.StartsWith(mapScenePrefix);
			float baseVol = isMap ? mapVolume : menuVolume;

			// 최종 볼륨을 실시간 갱신!
			bgmSource.volume = baseVol * _volume;
		}
	}
}