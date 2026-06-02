using UnityEngine;
using UnityEngine.Rendering;

public class SfxManager : MonoBehaviour
{
	public static SfxManager Instance { get; private set; }

	// (이제 단일 sfxSource 변수는 안 써도 됩니다!)
	[SerializeField] private AudioSource defaultSource; // 💡 인스펙터에서 SfxManager에 고정으로 달아둘 오디오 소스

	[Header("Debounce")]

	[SerializeField] private float thudCooldown = 0.1f;
	private float _lastThudTime = -999f;
	[SerializeField] public float _volume = 1;
	private void Awake()
	{
		if (Instance != null)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;
		DontDestroyOnLoad(gameObject);

		// 만약 인스펙터에서 할당 안 했으면 자동으로 추가
		if (defaultSource == null)
			defaultSource = gameObject.AddComponent<AudioSource>();
	}

	public void PlaySfx(AudioClip clip, float volume = 1f, float offset = 0f)
	{
		if (clip == null) return;

		// 오프셋이 필요 없는 대부분의 경우: 오브젝트 생성 없이 정석대로 겹쳐서 재생!
		if (offset == 0f)
		{
			defaultSource.PlayOneShot(clip, volume * _volume);
		}
		else
		{
			// 오프셋이 정말 필요한 특별한 사운드만 임시 오브젝트 생성 (기존 로직 유지)
			GameObject sfxObj = new GameObject("TempSfx_Offset_" + clip.name);
			sfxObj.transform.SetParent(transform);
			AudioSource tempSource = sfxObj.AddComponent<AudioSource>();
			tempSource.clip = clip;
			tempSource.volume = volume * _volume;
			tempSource.time = Mathf.Clamp(offset, 0f, clip.length - 0.01f);
			tempSource.Play();
			Destroy(sfxObj, clip.length - offset);
		}
	}
	public void PlayThudOnce(AudioClip clip, float volume = 1f, float offset = 0f)
	{
		if (clip == null) return;

		// 쿨타임 체크
		if (Time.time - _lastThudTime < thudCooldown)
			return;

		_lastThudTime = Time.time;

		// 쿨타임 통과했으면, 위에서 만든 PlaySfx를 그대로 가져다 씁니다!
		PlaySfx(clip, volume*_volume, offset);
	}
}