using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDeath : MonoBehaviour
{
	[Header("데스 연출 설정")]
	public float deathJumpForce = 15f; // 위로 뿅! 튀어오르는 힘
	public float reloadDelay = 2.0f;   // 떨어지고 나서 재시작될 때까지 기다리는 시간

	private bool _isDead = false;

	public void Die()
	{
		if (_isDead)
			return;
		_isDead = true;

		StartCoroutine(DeathRoutine());
	}

	private IEnumerator DeathRoutine()
	{
		Debug.Log("플레이어 사망");

		// 죽는 순간 애니메이터의 Alive 파라미터를 false로 꺼줍니다!
		Animator anim = GetComponent<Animator>();
		if (anim != null) anim.SetBool("Alive", false);

		// 1. 조작 및 이동 스크립트 끄기 (플레이어가 움직이지 못하게 막음)
		var input = GetComponent<PlayerInput>();
		if (input != null) input.enabled = false;

		var logic = GetComponent<PlayerLogic>();
		if (logic != null) logic.enabled = false;

		// 2. 플레이어의 모든 콜라이더(충돌체) 끄기
		// (이것 때문에 천장을 뚫고 나갔던 겁니다!)
		Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
		foreach (var col in colliders)
		{
			col.enabled = false;
		}

		// 3. 물리 엔진 조작: 멈췄다가 튀어 오르기
		Rigidbody2D rb = GetComponent<Rigidbody2D>();
		if (rb != null)
		{
			// 기존 관성 지우기
			rb.linearVelocity = Vector2.zero;

			// 💡 [핵심 해결] 현재 중력 방향에 따라 튕겨 오르는 방향을 결정합니다!
			Vector2 jumpDirection = Vector2.up; // 기본은 바닥에서 위로 뿅!

			if (GravityManager.Instance != null && GravityManager.Instance.CurrentDirection == eGravityDirection.Inverse)
			{
				jumpDirection = Vector2.down; // 반중력일 땐 천장에서 아래로 뿅!
			}

			// 결정된 방향으로 강하게 튕겨냅니다.
			rb.AddForce(jumpDirection * deathJumpForce, ForceMode2D.Impulse);
		}

		// 4. 공중에 떴다가 화면 밖으로 떨어질 때까지 대기
		yield return new WaitForSeconds(reloadDelay);

		// 5. 대기가 끝나면 씬 재시작!
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}
}