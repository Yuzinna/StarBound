using UnityEngine;

// [수정] SceneManager는 이제 SimpleLoadingManager 안에서 쓰이므로 여기서는 지워도 됩니다.
// using UnityEngine.SceneManagement; 

[RequireComponent(typeof(Collider2D))]
public class ClearDoor : MonoBehaviour
{
	[Header("클리어 연출")]
	[SerializeField] private ParticleSystem unlockParticle;
	[SerializeField] private SpriteRenderer glowLeft;
	[SerializeField] private SpriteRenderer glowRight;

	[Header("사운드")]
	public AudioClip unlockSfx;
	public AudioClip clearSfx;

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
			glowLeft.gameObject.SetActive(true);
			glowRight.gameObject.SetActive(true);
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

			// 2. [수정됨] 로딩 씬을 거쳐서 다음 씬으로 이동!
			if (!string.IsNullOrEmpty(nextSceneName))
			{
				Debug.Log($"[ClearDoor] 페이드 아웃을 시작하며 [{nextSceneName}] 로딩 시퀀스에 진입합니다.");

				// 💡 싱글톤 인스턴스가 있는지 체크하고 페이드 연동 함수를 호출합니다.
				if (SceneTransitionManager.Instance != null)
				{
					SceneTransitionManager.Instance.FadeOutToLoadingScene(nextSceneName);
				}
				else
				{
					// 혹시 모를 예외 상황(페이드 매니저가 없을 때)을 대비해 기존 로직을 백업으로 둡니다.
					SimpleLoadingManager.LoadScene(nextSceneName);
				}

			}
			else
			{
				Debug.LogWarning("[ClearDoor] 다음 씬 이름이 입력되지 않았습니다! 인스펙터를 확인하세요.");
			}
		}
	}
}