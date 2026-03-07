using UnityEngine;
using UnityEngine.SceneManagement; // [추가] 씬 이동을 위해 반드시 필요합니다!

[RequireComponent(typeof(Collider2D))]
public class ClearDoor : MonoBehaviour
{
	[Header("클리어 연출")]
	[SerializeField] private ParticleSystem unlockParticle;

	[Header("사운드")]
	public AudioClip unlockSfx;
	public AudioClip clearSfx;

	// [여기 추가!] 다음으로 이동할 스테이지(씬)의 이름
	[Header("다음 스테이지 설정")]
	[SerializeField] private string nextSceneName;

	private int _totalKeys = 0;
	private int _collectedKeys = 0;
	private bool _isOpen = false;

	private void Start()
	{
		DoorKey[] keysInMap = FindObjectsByType<DoorKey>(FindObjectsSortMode.None);
		_totalKeys = keysInMap.Length;

		if (unlockParticle != null && unlockParticle.isPlaying)
		{
			unlockParticle.Stop();
		}

		if (_totalKeys == 0)
		{
			UnlockDoor();
		}
	}

	public void AddKey()
	{
		_collectedKeys++;

		if (_collectedKeys >= _totalKeys && !_isOpen)
		{
			UnlockDoor();
		}
	}

	private void UnlockDoor()
	{
		_isOpen = true;

		if (unlockParticle != null)
		{
			unlockParticle.Play();
		}

		if (unlockSfx != null && SfxManager.Instance != null)
		{
			SfxManager.Instance.PlaySfx(unlockSfx);
		}
	}

	private void OnTriggerEnter2D(Collider2D col)
	{
		if (_isOpen && col.CompareTag("Player"))
		{
			// 1. 클리어 사운드 재생
			if (clearSfx != null && SfxManager.Instance != null)
			{
				SfxManager.Instance.PlaySfx(clearSfx);
			}

			// 2. [수정됨] 다음 씬으로 실제로 이동!
			if (!string.IsNullOrEmpty(nextSceneName))
			{
				Debug.Log($"[{nextSceneName}] 씬으로 이동합니다!");
				SceneManager.LoadScene(nextSceneName);
			}
			else
			{
				Debug.LogWarning("[ClearDoor] 다음 씬 이름이 입력되지 않았습니다! 인스펙터를 확인하세요.");
			}
		}
	}
}