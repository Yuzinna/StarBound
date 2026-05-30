using UnityEngine;
using UnityEngine.Rendering;

public class SfxManager : MonoBehaviour
{
	public static SfxManager Instance { get; private set; }

	// (이제 단일 sfxSource 변수는 안 써도 됩니다!)

	[Header("Debounce")]
	[SerializeField] private float thudCooldown = 0.1f;
	private float _lastThudTime = -999f;
	[SerializeField] private float _volume = 1;
	private void Awake()
	{
		if (Instance != null)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;
		DontDestroyOnLoad(gameObject);
	}

	public void PlaySfx(AudioClip clip, float volume = 1f, float offset = 0f)
	{
		if (clip == null) return;

		// 1. 소리를 낼 '1회용 임시 스피커(빈 게임 오브젝트)' 생성
		GameObject sfxObj = new GameObject("TempSfx_" + clip.name);
		sfxObj.transform.SetParent(transform); // 매니저 자식으로 깔끔하게 정리

		// 2. 오디오 소스 컴포넌트 추가 및 설정
		AudioSource tempSource = sfxObj.AddComponent<AudioSource>();
		tempSource.clip = clip;
		tempSource.volume = volume*_volume;

		// 3. 시작 위치(offset) 조절
		if (offset > 0f)
		{
			tempSource.time = Mathf.Clamp(offset, 0f, clip.length - 0.01f);
		}

		// 4. 소리 재생!
		tempSource.Play();

		// 5. 소리 길이가 끝나면 임시 스피커를 자동으로 삭제 (메모리 낭비 방지!)
		Destroy(sfxObj, clip.length - offset);
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