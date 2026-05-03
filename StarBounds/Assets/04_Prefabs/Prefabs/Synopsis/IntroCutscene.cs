using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;

public class IntroCutscene : MonoBehaviour
{
	[Header("배우 및 연출 위치")]
	public Transform corgiActor;
	public Transform collapseGround;
	public Transform collapsePoint;

	[Header("연출 속도 & 카메라")]
	public float walkSpeed = 3f;

	public float gravityAcceleration = 9.8f;
	public float maxFallSpeed = 10f;
	public float fallRotationSpeed = 360f;

	public float zoomInSize = 2.5f;
	public float zoomSpeed = 1.5f;

	[Header("🚨 무너지는 땅 연출 설정")]
	public float groundShakeDuration = 0.5f;
	public float groundShakeAmount = 0.05f;
	public float groundDropSpeed = 25f;

	[Header("사운드 & 파티클 & UI")]
	public AudioClip collapseSfx;
	public ParticleSystem dustParticle;
	public Image fadeImage;
	public GameObject exclamationBubble; // 💡 [추가됨] 느낌표 말풍선 오브젝트!

	public float startFadeOutDelay = 1.5f;
	public float fadeDuration = 2.0f;

	[Tooltip("추락 시작 시 부여할 초기 속도 (0보다 크면 시작부터 빠르게 떨어짐)")]
	public float initialFallSpeed = 5f;

	[Header("다음 씬 이름")]
	public string nextSceneName = "Chapter1";

	private Animator _anim;
	private CinemachineImpulseSource _impulse;
	private CinemachineCamera _virtualCam;

	private void Start()
	{
		_anim = corgiActor.GetComponent<Animator>();
		_impulse = GetComponent<CinemachineImpulseSource>();

		// 🚨 [핵심 해결책] 가짜 배우에 Rigidbody2D가 달려있다면, 물리 엔진이 개입하지 못하게 중력을 강제로 꺼버립니다!
		Rigidbody2D rb = corgiActor.GetComponent<Rigidbody2D>();
		if (rb != null)
		{
			rb.gravityScale = 0f;       // 중력 무시
			rb.linearVelocity = Vector2.zero; // 혹시 모를 기존 이동 속도 제거
		}

		SetupCamera(corgiActor);

		StartCoroutine(PlayCutscene());
	}

	private IEnumerator PlayCutscene()
	{
		// 🎬 [SCENE 1] 걷기
		if (_anim != null) _anim.SetFloat("Speed", 1f);

		while (Mathf.Abs(corgiActor.position.x - collapsePoint.position.x) > 0.05f)
		{
			float newX = Mathf.MoveTowards(corgiActor.position.x, collapsePoint.position.x, walkSpeed * Time.deltaTime);
			corgiActor.position = new Vector3(newX, corgiActor.position.y, corgiActor.position.z);
			yield return null;
		}

		// 🎬 [SCENE 2] 도착 후 멈칫
		if (_anim != null) _anim.SetFloat("Speed", 0f);
		corgiActor.position = new Vector3(collapsePoint.position.x, corgiActor.position.y, corgiActor.position.z);
		yield return new WaitForSeconds(0.4f);

		// 🎬 [SCENE 3] 땅이 덜덜덜 떨리며 코기가 두리번거림! 👀
		if (collapseGround != null)
		{
			Vector3 originalGroundPos = collapseGround.position;
			Vector3 originalScale = corgiActor.localScale;
			float shakeTimer = 0f;
			float lookAroundTimer = 0f;
			float lookAroundInterval = 0.25f;

			while (shakeTimer < groundShakeDuration)
			{
				shakeTimer += Time.deltaTime;
				lookAroundTimer += Time.deltaTime;

				if (lookAroundTimer >= lookAroundInterval)
				{
					originalScale.x *= -1f;
					corgiActor.localScale = originalScale;
					lookAroundTimer = 0f;
				}

				Vector2 randomOffset = Random.insideUnitCircle * groundShakeAmount;
				collapseGround.position = originalGroundPos + new Vector3(randomOffset.x, randomOffset.y, 0f);
				yield return null;
			}

			collapseGround.position = originalGroundPos;
			originalScale.x = Mathf.Abs(originalScale.x);
			corgiActor.localScale = originalScale;
		}

		// 🎬 [SCENE 4] 콰광! 발밑 땅 무너져 내림
		if (collapseGround != null)
		{
			StartCoroutine(DropGroundRoutine(collapseGround));
		}

		if (collapseSfx != null && SfxManager.Instance != null) SfxManager.Instance.PlaySfx(collapseSfx);
		if (dustParticle != null)
		{
			dustParticle.transform.position = collapsePoint.position;
			dustParticle.Play();
		}
		if (_impulse != null) _impulse.GenerateImpulse(0.4f);

		// 💡 [추가된 연출] 허공에 남겨진 코기 머리 위에 느낌표 번쩍! ❗
		if (exclamationBubble != null) exclamationBubble.SetActive(true);

		// 느낌표를 띄운 채로 허공에서 0.5초 대기
		yield return new WaitForSeconds(0.5f);

		// 떨어지기 시작할 때 느낌표 다시 숨기기
		if (exclamationBubble != null) exclamationBubble.SetActive(false);


		// 🎬 [SCENE 5] 코기 꺄아아악 추락 시작!
		if (_anim != null) _anim.SetBool("Alive", false);

		if (_virtualCam != null)
		{
			_virtualCam.Follow = null;
			_virtualCam.LookAt = null;
		}

		float fallTimer = 0f;
		bool isFadingOut = false;
		float currentFallSpeed = initialFallSpeed;

		while (true)
		{
			fallTimer += Time.deltaTime;

			currentFallSpeed += gravityAcceleration * Time.deltaTime;
			currentFallSpeed = Mathf.Min(currentFallSpeed, maxFallSpeed);

			corgiActor.position += Vector3.down * currentFallSpeed * Time.deltaTime;
			corgiActor.Rotate(0, 0, fallRotationSpeed * Time.deltaTime);

			if (_virtualCam != null)
			{
				_virtualCam.transform.position = new Vector3(corgiActor.position.x, corgiActor.position.y, _virtualCam.transform.position.z);
				_virtualCam.Lens.OrthographicSize = Mathf.Lerp(_virtualCam.Lens.OrthographicSize, zoomInSize, zoomSpeed * Time.deltaTime);
			}

			if (fallTimer >= startFadeOutDelay && !isFadingOut)
			{
				isFadingOut = true;
				StartCoroutine(FadeOutRoutine());
			}

			if (isFadingOut && fallTimer >= (startFadeOutDelay + fadeDuration + 0.5f))
			{
				break;
			}

			yield return null;
		}

		// 🎬 [SCENE 6] 챕터 1 시작
		SceneManager.LoadScene(nextSceneName);
	}

	private IEnumerator DropGroundRoutine(Transform groundToDrop)
	{
		Collider2D[] colliders = groundToDrop.GetComponentsInChildren<Collider2D>();
		foreach (Collider2D col in colliders)
		{
			col.enabled = false;
		}

		float dropTimer = 0f;
		float currentGroundSpeed = groundDropSpeed;

		while (dropTimer < 2f)
		{
			dropTimer += Time.deltaTime;
			currentGroundSpeed += (gravityAcceleration * 5f) * Time.deltaTime;
			groundToDrop.position += Vector3.down * currentGroundSpeed * Time.deltaTime;
			yield return null;
		}

		groundToDrop.gameObject.SetActive(false);
	}

	private IEnumerator FadeOutRoutine()
	{
		if (fadeImage == null) yield break;

		float timer = 0f;
		Color color = fadeImage.color;

		while (timer < fadeDuration)
		{
			timer += Time.deltaTime;
			color.a = Mathf.Lerp(0f, 1f, timer / fadeDuration);
			fadeImage.color = color;
			yield return null;
		}

		color.a = 1f;
		fadeImage.color = color;
	}

	private void SetupCamera(Transform playerTransform)
	{
		_virtualCam = GameObject.FindAnyObjectByType<CinemachineCamera>();
		if (_virtualCam != null)
		{
			_virtualCam.Follow = playerTransform;
			_virtualCam.LookAt = playerTransform;
		}
	}
}