using UnityEngine;
using UnityEngine.SceneManagement;

public class KillTilemap : MonoBehaviour
{
	[Header("사망 효과음 (선택)")]
	[Tooltip("가시에 찔리거나 용암에 빠질 때 날 소리를 넣어주세요")]
	public AudioClip killSfx;

	// 1. 트리거 모드일 때 (Is Trigger가 켜져 있어서 뚫고 들어갈 때 - 예: 용암)
	private void OnTriggerEnter2D(Collider2D collision)
	{
		CheckAndKillPlayer(collision.gameObject);
	}

	// 2. 물리 충돌 모드일 때 (Is Trigger가 꺼져 있어서 쿵 부딪힐 때 - 예: 단단한 가시)
	private void OnCollisionEnter2D(Collision2D collision)
	{
		CheckAndKillPlayer(collision.gameObject);
	}

	// 🔥 공통 사망 처리 로직
	private void CheckAndKillPlayer(GameObject target)
	{
		if (target.CompareTag("Player"))
		{
			Debug.Log($"💀 코기가 [{gameObject.name}] 함정에 닿아서 죽었습니다!");

			// 1. 사운드 재생
			if (killSfx != null && SfxManager.Instance != null)
			{
				SfxManager.Instance.PlaySfx(killSfx);
			}

			// 2. 플레이어 사망 처리 또는 씬 강제 재시작
			PlayerDeath playerDeath = target.GetComponent<PlayerDeath>();
			if (playerDeath != null)
			{
				playerDeath.Die();
			}
			else
			{
				SceneManager.LoadScene(SceneManager.GetActiveScene().name);
			}
		}
	}
}