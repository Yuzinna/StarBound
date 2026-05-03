using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class Stage1Entry : MonoBehaviour
{
	[Header("대상 설정")]
	public Transform landPoint;

	[Header("추락 속도 설정")]
	public float initialFallSpeed = 10f;    // 시놉시스와 맞춘 시작 속도
	public float gravityAcceleration = 40f; // 강한 가속도
	public float maxFallSpeed = 50f;        // 최대 속도 제한
	public float fallRotationSpeed = 720f;  // 빙글빙글 회전 속도

	[Header("착지 설정")]
	public float lookAroundTime = 1.5f;

	private GameObject _player;
	private Animator _anim;
	private PlayerInput _playerMovement;
	private Rigidbody2D _rb;

	private IEnumerator Start()
	{
		// 1. 플레이어 스폰 대기
		while (_player == null)
		{
			_player = GameObject.FindWithTag("Player");
			yield return null;
		}

		_anim = _player.GetComponent<Animator>();
		_rb = _player.GetComponent<Rigidbody2D>();
		_playerMovement = _player.GetComponent<PlayerInput>(); // 플레이어 컨트롤러 스크립트

		StartCoroutine(PlayEntrySequence());
	}

	private IEnumerator PlayEntrySequence()
	{
		// [STEP 1] 초기화
		if (_playerMovement != null) _playerMovement.enabled = false;
		if (_rb != null)
		{
			_rb.simulated = false;
			_rb.linearVelocity = Vector2.zero;
		}

		// 추락 시작: X눈 애니메이션 강제 고정
		if (_anim != null)
		{
			_anim.enabled = true;
			_anim.Play("Corgi_Fall_X", 0, 0f);
			_anim.SetBool("Alive", false);
			_anim.Update(0f);
		}

		float currentSpeed = initialFallSpeed;

		// [STEP 2] 추락 (회전 포함)
		while (_player.transform.position.y > landPoint.position.y)
		{
			currentSpeed += gravityAcceleration * Time.deltaTime;
			currentSpeed = Mathf.Min(currentSpeed, maxFallSpeed);

			_player.transform.position += Vector3.down * currentSpeed * Time.deltaTime;
			_player.transform.Rotate(0, 0, fallRotationSpeed * Time.deltaTime);

			yield return null;
		}

		// [STEP 3] 착지 직후 (여전히 정신 못 차림)
		_player.transform.position = landPoint.position;
		_player.transform.rotation = Quaternion.identity; // 회전은 멈춤

		// 🚨 1초 동안 X눈 상태로 바닥에 누워 있음
		yield return new WaitForSeconds(1.0f);

		// [STEP 4] 이제 정신 차리고 두리번거리기 (Flip 이용)
		Vector3 originalScale = _player.transform.localScale;

		// 왼쪽 보기
		originalScale.x = -Mathf.Abs(originalScale.x);
		_player.transform.localScale = originalScale;
		yield return new WaitForSeconds(0.7f);

		// 오른쪽 보기
		originalScale.x = Mathf.Abs(originalScale.x);
		_player.transform.localScale = originalScale;
		yield return new WaitForSeconds(0.7f);

		// 다시 왼쪽 보기
		originalScale.x = -Mathf.Abs(originalScale.x);
		_player.transform.localScale = originalScale;
		yield return new WaitForSeconds(0.7f);

		// 정면(오른쪽) 보며 일어나기
		originalScale.x = Mathf.Abs(originalScale.x);
		_player.transform.localScale = originalScale;
		yield return new WaitForSeconds(0.7f);
		if (_anim != null)
		{
			_anim.Play("Idle", 0, 0f);
			_anim.SetBool("Alive", true); // 이제 진짜 살아남
			_anim.Update(0f);
		}

		// [STEP 5] 마지막 여운 후 조작권 반환
		yield return new WaitForSeconds(0.7f);

		if (_playerMovement != null) _playerMovement.enabled = true;
		if (_rb != null) _rb.simulated = true;

		this.enabled = false;
	}
}