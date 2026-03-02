using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
	[Header("이펙트")]
	public ParticleSystem teleportParticle; // 인스펙터에서 연결할 파티클
	[Header("연결될 반대편 포탈")]
	public Portal linkedPortal;


	// [여기 추가!] 포탈 전용 효과음 변수
	[Header("사운드")]
	public AudioClip teleportSfx;
	[Range(0f, 1f)] public float sfxVolume = 1f; // 볼륨 조절용
	// 무한 텔레포트를 막기 위한 안전장치
	private bool canTeleport = true;
	private void Awake()
	{
		teleportParticle = GetComponentInChildren<ParticleSystem>();
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

				// 2. [여기 추가!] SfxManager를 이용해 텔레포트 효과음 1회 재생!
				if (teleportSfx != null && SfxManager.Instance != null)
				{
					SfxManager.Instance.PlaySfx(teleportSfx, sfxVolume,0.2f);
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
