using UnityEngine;

public class SfxManager : MonoBehaviour
{
	public static SfxManager Instance { get; private set; }


	[SerializeField] private AudioSource sfxSource;

	[Header("Debounce")]
	[SerializeField] private float thudCooldown = 0.1f; // 0.05~0.15 추천
	private float _lastThudTime = -999f;

	private void Awake()
	{
		if (Instance != null)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;
		DontDestroyOnLoad(gameObject);

		if (sfxSource == null)
			sfxSource = GetComponent<AudioSource>();
	}

	public void PlaySfx(AudioClip clip, float volume = 1f,float offset=0f)
	{
		if (clip == null || sfxSource == null) return;

		if (sfxSource.isPlaying)
			sfxSource.Stop();

		sfxSource.clip = clip;
		sfxSource.volume = volume;
		if (offset > 0f)
		{
			offset = Mathf.Clamp(offset, 0f, clip.length - 0.01f);
			sfxSource.time = offset;
		}
		else
		{
			sfxSource.time = 0f;
		}

		sfxSource.Play();
		//sfxSource.PlayOneShot(clip, volume);
	}
	public void PlayThudOnce(AudioClip clip, float volume = 1f, float offset = 0f)
	{
		if (clip == null || sfxSource == null) return;

		if (Time.time - _lastThudTime < thudCooldown)
			return;

		_lastThudTime = Time.time;

		// 오프셋 필요 없으면 아래 3줄을 PlayOneShot으로 바꿔도 됨
		if (sfxSource.isPlaying) sfxSource.Stop();
		sfxSource.clip = clip;
		sfxSource.volume = volume;
		sfxSource.time = Mathf.Clamp(offset, 0f, clip.length - 0.01f);
		sfxSource.Play();
	}
}
