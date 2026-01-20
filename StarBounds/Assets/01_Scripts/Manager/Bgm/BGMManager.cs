using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BgmManager: MonoBehaviour
{
	public static BgmManager Instance { get; private set; }

	[Header("BGM Clips")]
	[SerializeField] private AudioClip menuBgm;   // Start + Synopsis
	[SerializeField] private AudioClip mapBgm;    // 모든 게임맵

	[Header("Volumes")]
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
			bgmSource.volume = volume;
			return;
		}

		bgmSource.clip = clip;
		bgmSource.volume = volume;
		bgmSource.Play();
	}
}
