using GameFrameworkLite;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ProcedureStagePlay : ProcedureBase
{
    private EventModule _eventModule;//스테이지 시작 진행 클리어에 들어갈 이벤트를 관리
    private SceneModule _sceneModule;
    private EntityModule _entityModule;

	private Fsm<ProcedureModule> _fsm;   // 나중에 ChangeProcedure 할 때 쓰려고 저장
	private bool _sceneLoaded = false;

	
	public override void OnEnter(Fsm<ProcedureModule> fsm)
	{
		base.OnEnter(fsm);
		_fsm = fsm;
		Debug.Log("[ProcedureStagePlay] OnEnter - 현재 스테이지: " + GameState.CurrentStage);
		_eventModule = GameFrameworkEntry.GetModule<EventModule>();
		_sceneModule = GameFrameworkEntry.GetModule<SceneModule>();
		_entityModule = GameFrameworkEntry.GetModule<EntityModule>();

		//_sceneLoaded = false;

		// 1) 스테이지 클리어 이벤트 구독 (공용)
		_eventModule.Subscribe((int)GameEventId.StageClear, OnStageClear);

		// 2) 씬 로드 완료 이벤트 구독
		SceneManager.sceneLoaded += OnSceneLoaded;

		// 3) 현재 스테이지 번호에 따라 씬 이름 결정 후 로드
		string sceneName = GetSceneName(GameState.CurrentStage);
		Debug.Log("[ProcedureStagePlay] 씬 로드 요청: " + sceneName);
		_sceneModule.LoadScene(sceneName);
	}

	public override void OnLeave(Fsm<ProcedureModule> fsm, bool isShutdown)
	{
		base.OnLeave(fsm, isShutdown);

		SceneManager.sceneLoaded -= OnSceneLoaded;
		if (_eventModule != null)
		{
			_eventModule.Unsubscribe((int)GameEventId.StageClear, OnStageClear);
		}
	}

	public override void OnUpdate(Fsm<ProcedureModule> fsm, float deltaTime, float realDeltaTime)
	{
		base.OnUpdate(fsm, deltaTime, realDeltaTime);
	}

	private string GetSceneName(int stageIndex)
	{
		switch (stageIndex)
		{
			case 1: return "map1-1";
			case 2: return "map1-2";
			case 3: return "map1-3";
			case 4: return "map1-4";
			case 5: return "map1-5";
			case 6: return "map1-6";
			case 7: return "map1-7";
			default:
				Debug.LogWarning("[ProcedureStagePlay] 알 수 없는 스테이지 번호: " + stageIndex + " → Stage1로 fallback");
				return "Stage1";
		}
	}
	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		// 혹시 다른 씬 로드 이벤트가 오면 무시
		if (scene.name != GetSceneName(GameState.CurrentStage))
			return;

		// 더 이상 필요 없으니 한 번만 쓰고 구독 해제
		SceneManager.sceneLoaded -= OnSceneLoaded;

		// 여기서부터가 원래 네가 OnEnter에서 하고 싶었던 일들
		SetupPlayer();
		SetupOtherThings();

		
	}
	private void SetupPlayer()
	{
		// 1. PlayerStart 찾기 (모든 스테이지에서 태그만 맞춰두면 공용으로 사용 가능)
		var startObj = GameObject.FindWithTag("PlayerStart");
		if (startObj == null)
		{
			Debug.LogWarning("[ProcedureStagePlay] PlayerStart 태그를 못 찾음. (0,0)에 생성)");
		}

		Vector3 spawnPos = startObj != null ? startObj.transform.position + Vector3.up : Vector3.zero;

		// 2. 플레이어 엔티티 생성
		_entityModule.ShowEntity(1, "Prefabs/Player", "Player", null);

		//var logic = _entityModule.GetEntityLogic(1) as PlayerLogic;
		//if (logic != null)
		//{
		//	logic.transform.position = spawnPos;
		//	Debug.Log("[ProcedureStagePlay] PlayerLogic 위치 세팅 완료");

		//	// Cinemachine 카메라 연결
		//	CinemachineCamera virtualCam = GameObject.FindAnyObjectByType<CinemachineCamera>();
		//	if (virtualCam != null)
		//	{
		//		virtualCam.Follow = logic.transform;
		//		virtualCam.LookAt = logic.transform;
		//		Debug.Log("[ProcedureStagePlay] Cinemachine Camera를 플레이어에 연결 완료.");
		//	}
		//	else
		//	{
		//		Debug.LogWarning("[ProcedureStagePlay] CinemachineCamera 오브젝트를 찾을 수 없습니다!");
		//	}
		//}
		//else
		//{
		//	Debug.LogError("[ProcedureStagePlay] PlayerLogic 을 못 가져옴");
		//}
	}
	private void SetupOtherThings()
	{
		// 스테이지 공통 초기화 (예: UI 열기, 중력 초기화 등)
		// 나중에 필요하면 여기 채우면 됨.
	}
	private void OnStageClear(object userData)
	{
		Debug.Log("[ProcedureStagePlay] StageClear 이벤트 수신! 다음 스테이지로 이동 준비");

		// 플레이어 숨기기
		_entityModule.HideEntity(1);

		// 이벤트 중복 방지
		_eventModule.Unsubscribe((int)GameEventId.StageClear, OnStageClear);

		// 다음 스테이지 결정
		if (GameState.CurrentStage < GameState.MaxStage)
		{
			GameState.CurrentStage++;
			Debug.Log("[ProcedureStagePlay] 다음 스테이지로: " + GameState.CurrentStage);
			_fsm.Owner.ChangeProcedure<ProcedureStagePlay>();   // 다시 자기 자신으로, 다음 스테이지 시작
		}
		else
		{
			Debug.Log("[ProcedureStagePlay] 마지막 스테이지 클리어 → 결과/엔딩 프로시저로 이동");
			_fsm.Owner.ChangeProcedure<ProcedureStageClear>();  // 이런 프로시저 하나 만들어두면 됨
		}
	}
}
