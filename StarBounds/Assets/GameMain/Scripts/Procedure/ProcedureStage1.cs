using GameFrameworkLite;
using UnityEngine;
using UnityEngine.SceneManagement;

using Unity.Cinemachine; // ❗ Cinemachine 네임스페이스 추가

public class ProcedureStage1 : ProcedureBase
{
	private EventModule _eventModule;
	private SceneModule _sceneModule;
	private EntityModule _entityModule;

	private bool _stageReady = false;
	private bool _stageCleared = false;

	public override void OnEnter(Fsm<ProcedureModule> fsm)
	{
		base.OnEnter(fsm);

		Debug.Log("[ProcedureStage1] OnEnter");

		_eventModule = GameFrameworkEntry.GetModule<EventModule>();
		_sceneModule = GameFrameworkEntry.GetModule<SceneModule>();
		_entityModule = GameFrameworkEntry.GetModule<EntityModule>();

		_stageCleared = false;

		// 1) 씬 로드 완료 이벤트 구독
		SceneManager.sceneLoaded += OnSceneLoaded;

		// 2) Stage1 씬 로드 요청
		_sceneModule.LoadScene("Stage1");

		// 3. 스테이지 클리어 이벤트 구독
		_eventModule.Subscribe(GameEventId.Stage1Clear, OnStage1Clear);
	}
	
	public override void OnUpdate(Fsm<ProcedureModule> fsm, float elapseSeconds, float realElapseSeconds)
	{
		base.OnUpdate(fsm, elapseSeconds, realElapseSeconds);

		if (_stageCleared)
		{
			// 한 번만 처리하고, 다음 상태로 넘기기
			_stageCleared = false;

			var procedure = fsm.Owner;

			// 클리어 화면 절차로
			procedure.ChangeProcedure<ProcedureStage2>();
		}
	}

	public override void OnLeave(Fsm<ProcedureModule> fsm, bool isShutdown)
	{
		base.OnLeave(fsm, isShutdown);


		// 혹시라도 남아 있으면 깔끔히 해제
		SceneManager.sceneLoaded -= OnSceneLoaded;

		if (_eventModule != null)
		{
			_eventModule.Unsubscribe(GameEventId.Stage1Clear, OnStage1Clear);
		}
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (scene.name != "Stage1")
			return;

		Debug.Log("[ProcedureStage1] Stage1 씬 로드 완료");

		// 더 이상 필요 없으니 한 번만 쓰고 구독 해제
		SceneManager.sceneLoaded -= OnSceneLoaded;

		// 여기서부터가 원래 네가 OnEnter에서 하고 싶었던 일들
		SetupPlayer();
		SetupOtherThings();

		_stageReady = true;
	}

	private void SetupPlayer()
	{
		// 1. PlayerStart 찾기
		var startObj = GameObject.FindWithTag("PlayerStart");
		if (startObj == null)
		{
			Debug.LogWarning("[ProcedureStage1] PlayerStart 태그를 못 찾음. (0,0)에 생성)");
		}

		Vector3 spawnPos = startObj != null ? startObj.transform.position : Vector3.zero;

		// 2. 플레이어 엔티티 생성
		// 'Player' Entity ID: 1 사용
		_entityModule.ShowEntity(1, "Prefabs/Player", "Player", null);

		var logic = _entityModule.GetEntityLogic(1) as PlayerLogic;
		if (logic != null)
		{
			// 플레이어 위치 세팅
			logic.transform.position = spawnPos;
			Debug.Log("[ProcedureStage1] PlayerLogic 위치 세팅 완료");

			// 3. 🎥 Cinemachine 카메라 연결 (추가된 로직) 🎥
			CinemachineCamera virtualCam = GameObject.FindAnyObjectByType<CinemachineCamera>();

			if (virtualCam != null)
			{
				// 생성된 플레이어의 Transform을 Cinemachine 카메라의 추적 대상(Follow/LookAt)으로 설정
				virtualCam.Follow = logic.transform;
				virtualCam.LookAt = logic.transform;

				Debug.Log($"[ProcedureStage1] Cinemachine Camera를 플레이어 '{logic.name}'에 연결 완료.");
			}
			else
			{
				Debug.LogWarning("[ProcedureStage1] 씬에서 CinemachineCamera 오브젝트를 찾을 수 없습니다! 카메라 연결 실패.");
			}
		}
		else
		{
			Debug.LogError("[ProcedureStage1] PlayerLogic 을 못 가져옴");
		}
	}

	private void SetupOtherThings()
	{
		// 나중에 스테이지별 초기화 있으면 여기
	}

	private void OnStage1Clear(object userData)
	{
		Debug.Log("[ProcedureStage1] Stage1Clear 이벤트 수신. 다음 씬 로드!");
		// 스테이지를 떠날 때 플레이어 엔티티를 숨깁니다 (메모리 정리).
		_entityModule.HideEntity(1);

		// 2. 더 이상 이 이벤트는 필요 없으니 구독 해제 (중복 실행 방지)
		_eventModule.Unsubscribe(GameEventId.Stage1Clear, OnStage1Clear);

		// ✔ 여기서는 씬을 직접 로드하지 않고, 플래그만 켬
		_stageCleared = true;

		// (선택) 만약 나중에 ProcedureStage2 같은 걸 쓸 거면,
		// 여기서 fsm.Owner.ChangeProcedure<ProcedureStage2>(); 같은 걸 추가해도 됨.
	}
}