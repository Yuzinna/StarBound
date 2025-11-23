// Assets/GameFrameworkLite/Sound/SoundModule.cs
using UnityEngine;

namespace GameFrameworkLite
{
	/// <summary>
	/// BGM / SFX 재생을 담당하는 간단한 사운드 모듈.
	/// </summary>
	public sealed class SoundModule : IGameFrameworkModule
	{
		private readonly ResourceModule _resourceModule;
		private AudioSource _musicSource;   // BGM 용
		private AudioSource _sfxSource;     // 효과음 용

		public SoundModule(ResourceModule resourceModule)
		{
			_resourceModule = resourceModule;

			var go = new GameObject("[SoundRoot]");
			Object.DontDestroyOnLoad(go);

			_musicSource = go.AddComponent<AudioSource>();
			_musicSource.loop = true;

			_sfxSource = go.AddComponent<AudioSource>();
		}

		/// <summary>
		/// BGM 재생.
		/// path 예: "Audio/Music/MainTheme"
		/// </summary>
		public void PlayMusic(string path, float volume = 1f)
		{
			var clip = _resourceModule.Load<AudioClip>(path);
			if (clip == null) return;
			_musicSource.clip = clip;
			_musicSource.volume = volume;
			_musicSource.Play();
		}

		/// <summary>
		/// BGM 정지.
		/// </summary>
		public void StopMusic()
		{
			_musicSource.Stop();
		}

		/// <summary>
		/// 효과음 한 번 재생.
		/// </summary>
		public void PlaySfx(string path, float volume = 1f)
		{
			var clip = _resourceModule.Load<AudioClip>(path);
			if (clip == null) return;
			_sfxSource.PlayOneShot(clip, volume);
		}

		public void Update(float deltaTime, float realDeltaTime) { }

		public void Shutdown()
		{
			if (_musicSource != null)
				Object.Destroy(_musicSource.gameObject);
		}
	}
}
