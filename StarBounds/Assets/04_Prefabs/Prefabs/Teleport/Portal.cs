using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
	[Header("이펙트")]
	public ParticleSystem teleportParticle; // 인스펙터에서 연결할 파티클
	[Header("연결될 반대편 포탈")]
	public Portal linkedPortal;

	// 포탈 전용 효과음 변수
	[Header("사운드")]
	public AudioClip teleportSfx;
	[Range(0f, 1f)] public float sfxVolume = 1f; // 볼륨 조절용

	// 💡 [여기 추가!] 무한 낙하 시 도달할 수 있는 최대 속도를 제한합니다.
	[Header("포탈 물리 설정")]
	[Tooltip("무한 텔레포트 시 허용할 최대 속도 (숫자를 낮추면 더 천천히 떨어집니다)")]
	public float maxFallSpeed = 15f;

	// 무한 텔레포트를 막기 위한 안전장치
	private bool canTeleport = true;

	public Space RotationSpace = Space.Self;
	//포탈이 회전하는 속도
	public Vector3 RotationSpeed = new Vector3(0f, 0f, 100f);
	private void Awake()
	{
		teleportParticle = GetComponentInChildren<ParticleSystem>();
	}

	private void Update()
	{
		transform.Rotate(RotationSpeed * Time.deltaTime, RotationSpace);
	}
	private void OnTriggerEnter2D(Collider2D col)
	{
		// 닿은 오브젝트가 플레이어이고, 포탈이 켜져(canTeleport) 있다면?
		if (canTeleport && (col.CompareTag("Player") || col.CompareTag("Cube")))
		{
			if (linkedPortal != null)
			{
				linkedPortal.StartCoroutine(linkedPortal.CooldownRoutine());

				//내 포탈(들어가는 곳)과 반대편 포탈(나오는 곳) 양쪽에서 파티클 터뜨리기
				if (teleportParticle != null) teleportParticle.Play();
				if (linkedPortal.teleportParticle != null) linkedPortal.teleportParticle.Play();

				// SfxManager를 이용해 텔레포트 효과음 1회 재생!
				if (teleportSfx != null && SfxManager.Instance != null)
				{
					SfxManager.Instance.PlaySfx(teleportSfx, sfxVolume, 0.2f);
				}

				// 들어온 게 큐브인지 확인
				GravityObjectLogic gravityObj = col.GetComponent<GravityObjectLogic>();
				if (gravityObj != null)
				{
					// 큐브라면 물리 엔진 전용 순간이동 실행!
					gravityObj.TeleportTo(linkedPortal.transform.position);
				}
				else
				{
					// 플레이어는 기존 방식대로 이동
					col.transform.position = linkedPortal.transform.position;
				}

				// ==========================================
				// 💡 [여기 추가됨!] 텔레포트 직후 속도 제한 (놀이기구 효과)
				// ==========================================
				Rigidbody2D rb = col.GetComponent<Rigidbody2D>();
				if (rb != null)
				{
					// 현재 떨어지는 Y축 속도의 '절댓값'이 maxFallSpeed를 넘었다면?
					if (Mathf.Abs(rb.linearVelocity.y) > maxFallSpeed)
					{
						// X축 속도는 그대로 두고, Y축 속도는 방향(+/-)만 살려서 maxFallSpeed로 고정!
						rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Sign(rb.linearVelocity.y) * maxFallSpeed);
					}
				}
			}
		}
	}

	// 포탈을 잠시 껐다가 켜는 타이머
	public IEnumerator CooldownRoutine()
	{
		canTeleport = false;                    // 포탈 작동 정지
		yield return new WaitForSeconds(0.5f);  // 0.5초 대기 (이 숫자로 쿨타임 조절 가능)
		canTeleport = true;                     // 0.5초 뒤 포탈 다시 작동!
	}
}