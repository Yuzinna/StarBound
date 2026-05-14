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
		// 닿은 오브젝트가 플레이어이거나 큐브이고, 포탈이 켜져(canTeleport) 있다면?
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

				// ==========================================
				// 🚨 [여기가 추가된 핵심 로직!] 안전한 도착 지점(destPos) 찾기
				// ==========================================
				Vector2 destPos = linkedPortal.transform.position;
				Vector2 checkSize = new Vector2(0.8f, 0.8f); // 겹침을 검사할 박스 크기

				// 중력 방향에 따라 위로 쌓을지 아래로 쌓을지 결정
				float stackOffset = 1.0f; // 기본은 위로(+1) 
				if (GravityManager.Instance != null && GravityManager.Instance.CurrentDirection == eGravityDirection.Inverse)
				{
					stackOffset = -1.0f; // 역중력이면 아래로(-1)
				}

				int safetyCount = 0;
				// 빈 공간을 찾을 때까지 최대 10번 반복해서 위로 올리며 검사합니다. (큐브가 여러 개 쌓여있을 때를 대비)
				while (safetyCount < 10)
				{
					bool isOccupied = false;
					Collider2D[] overlaps = Physics2D.OverlapBoxAll(destPos, checkSize, 0f);

					foreach (Collider2D over in overlaps)
					{
						// 나 자신은 제외, 포탈 같은 트리거 제외하고, 다른 '플레이어'나 '큐브'가 있다면?
						if (over.gameObject != col.gameObject && !over.isTrigger)
						{
							if (over.CompareTag("Cube") || over.CompareTag("Player"))
							{
								isOccupied = true;
								break;
							}
						}
					}

					if (isOccupied)
					{
						destPos.y += stackOffset; // 자리가 꽉 찼네? 한 칸 위(혹은 아래)로 올려서 다시 검사!
						safetyCount++;
					}
					else
					{
						break; // 자리 비었음! 루프 탈출!
					}
				}
				// ==========================================

				// 들어온 게 큐브인지 확인
				GravityObjectLogic gravityObj = col.GetComponent<GravityObjectLogic>();
				if (gravityObj != null)
				{
					// 🚨 [수정됨] 무작정 포탈 위치가 아니라, 방금 계산한 '안전한 위치(destPos)'로 이동!
					gravityObj.TeleportTo(destPos);
				}
				else
				{
					// 🚨 [수정됨] 플레이어도 안전한 위치(destPos)로 이동!
					col.transform.position = destPos;
				}

				// 텔레포트 직후 속도 제한 (놀이기구 효과)
				Rigidbody2D rb = col.GetComponent<Rigidbody2D>();
				if (rb != null)
				{
					if (Mathf.Abs(rb.linearVelocity.y) > maxFallSpeed)
					{
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