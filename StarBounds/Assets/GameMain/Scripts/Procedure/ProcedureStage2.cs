using GameFrameworkLite;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ProcedureStage2 : ProcedureBase
{
	private EventModule _eventModule;
	private SceneModule _sceneModule;
	private EntityModule _entityModule;

	private bool _stageReady = false;
	private bool _stageCleared = false;

	public override void OnEnter(Fsm<ProcedureModule> fsm)
	{
		base.OnEnter(fsm);

		Debug.Log("[ProcedureStage2] OnEnter");

		_eventModule = GameFrameworkEntry.GetModule<EventModule>();
		_sceneModule = GameFrameworkEntry.GetModule<SceneModule>();
		_entityModule = GameFrameworkEntry.GetModule<EntityModule>();

		_stageCleared = false;

		// 1) 씬 로드 완료 이벤트 구독
		SceneManager.sceneLoaded += OnSceneLoaded;

		// 2) Stage2 씬 로드 요청  🔻🔻🔻
		_sceneModule.LoadScene("Stage2");

		// 3) 스테이지 클리어 이벤트 구독 (원하면 GameEventId.Stage2Clear 같은 것 사용)
		_eventModule.Subscribe(GameEventId.Stage2Clear, OnStage2Clear);
	}

	public override void OnUpdate(Fsm<ProcedureModule> fsm, float elapseSeconds, float realElapseSeconds)
	{
		base.OnUpdate(fsm, elapseSeconds, realElapseSeconds);

		if (_stageCleared)
		{
			_stageCleared = false;

			var procedure = fsm.Owner;

			Debug.Log("[ProcedureStage2] Stage2 클리어 → 다음 절차로 전환 (예: Stage3, Clear 화면 등)");

			// 예: Stage3로 간다거나, 클리어 화면으로 간다거나
			// procedure.ChangeProcedure<ProcedureStage3>();
		}
	}

	public override void OnLeave(Fsm<ProcedureModule> fsm, bool isShutdown)
	{
		base.OnLeave(fsm, isShutdown);

		SceneManager.sceneLoaded -= OnSceneLoaded;

		if (_eventModule != null)
		{
			_eventModule.Unsubscribe(GameEventId.Stage2Clear, OnStage2Clear);
		}
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (scene.name != "Stage2")
			return;

		Debug.Log("[ProcedureStage2] Stage2 씬 로드 완료");

		SceneManager.sceneLoaded -= OnSceneLoaded;

		SetupPlayer();
		SetupOtherThings();

		_stageReady = true;
	}

	private void SetupPlayer()
	{
		var startObj = GameObject.FindWithTag("PlayerStart");
		if (startObj == null)
		{
			Debug.LogWarning("[ProcedureStage2] PlayerStart 태그를 못 찾음. (0,0)에 생성)");
		}

		Vector3 spawnPos = startObj != null ? startObj.transform.position : Vector3.zero;

		// Stage2에서도 Player ID 1 그대로 써도 됨 (어차피 Stage1 떠날 때 HideEntity 했으니까)
		_entityModule.ShowEntity(1, "Entities/Player", "Player", null);

		var logic = _entityModule.GetEntityLogic(1) as PlayerLogic;
		if (logic != null)
		{
			logic.transform.position = spawnPos;
			Debug.Log("[ProcedureStage2] PlayerLogic 위치 세팅 완료");

			CinemachineCamera virtualCam = GameObject.FindAnyObjectByType<CinemachineCamera>();

			if (virtualCam != null)
			{
				virtualCam.Follow = logic.transform;
				virtualCam.LookAt = logic.transform;

				Debug.Log($"[ProcedureStage2] Cinemachine Camera를 플레이어 '{logic.name}'에 연결 완료.");
			}
			else
			{
				Debug.LogWarning("[ProcedureStage2] 씬에서 CinemachineCamera 오브젝트를 찾을 수 없습니다! 카메라 연결 실패.");
			}
		}
		else
		{
			Debug.LogError("[ProcedureStage2] PlayerLogic 을 못 가져옴");
		}
	}

	private void SetupOtherThings()
	{
		// Stage2 전용 초기화 로직
	}

	private void OnStage2Clear(object userData)
	{
		Debug.Log("[ProcedureStage2] Stage2Clear 이벤트 수신.");

		_entityModule.HideEntity(1);
		_eventModule.Unsubscribe(GameEventId.Stage2Clear, OnStage2Clear);

		_stageCleared = true;
	}
}
