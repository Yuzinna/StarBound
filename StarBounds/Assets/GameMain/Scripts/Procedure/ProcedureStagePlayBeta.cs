using GameFrameworkLite;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

// 주의: 이 코드는 GameFrameworkLite FSM에 SetData/GetData 기능이 없고,
//       씬 언로드는 Unity 기본 기능(SceneManager)을 사용한다고 가정하고 작성되었습니다.
public class ProcedureStagePlayBeta : ProcedureBase
{
	private EventModule _eventModule;
	private SceneModule _sceneModule; // 씬 로드에만 사용
	private EntityModule _entityModule;

	private Fsm<ProcedureModule> _fsm;

	// 로드할 씬의 이름을 저장하는 변수 (전역 상태에서 가져옴)
	private string _targetSceneName;

	//--------------------------------------------------
	// FSM 생명 주기
	//--------------------------------------------------

	public override void OnEnter(Fsm<ProcedureModule> fsm)
	{
		base.OnEnter(fsm);
		_fsm = fsm;

		_eventModule = GameFrameworkEntry.GetModule<EventModule>();
		_sceneModule = GameFrameworkEntry.GetModule<SceneModule>();
		_entityModule = GameFrameworkEntry.GetModule<EntityModule>();

		// 1. **전역 상태 변수에서 로드할 씬 이름 가져오기**
		// GameState 클래스에 BetaTestSceneName이라는 정적 변수가 있다고 가정합니다.
		// 이 변수는 이 Procedure로 넘어오기 직전에 외부에서 세팅해야 합니다.

		_targetSceneName = SceneManager.GetActiveScene().name;

		// 2. 씬 로드 완료 이벤트 구독 (Unity 기본 제공)
		SceneManager.sceneLoaded += OnSceneLoaded;

		// 3. 씬 로드 요청 (GameFrameworkLite SceneModule 기본 제공)
		_sceneModule.LoadScene(_targetSceneName);

		// 4. 스테이지 클리어 이벤트 구독 (기존 로직 유지)
		_eventModule.Subscribe((int)GameEventId.StageClear, OnStageClear);
	}

	public override void OnLeave(Fsm<ProcedureModule> fsm, bool isShutdown)
	{
		base.OnLeave(fsm, isShutdown);

		// 구독 해제 (Unity 기본 제공)
		SceneManager.sceneLoaded -= OnSceneLoaded;
		if (_eventModule != null)
		{
			_eventModule.Unsubscribe((int)GameEventId.StageClear, OnStageClear);
		}

		// 정리
		_targetSceneName = null;
	}

	//--------------------------------------------------
	// 이벤트 핸들러
	//--------------------------------------------------

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (scene.name != _targetSceneName)
			return;

		SceneManager.sceneLoaded -= OnSceneLoaded;
		SetupPlayer();
		SetupOtherThings();
	}

	private void OnStageClear(object userData)
	{
		Debug.Log("[ProcedureStagePlayBeta] StageClear 이벤트 수신! 베타 테스트 종료 및 정리 시작.");

		// 플레이어 엔티티 숨기기
		_entityModule.HideEntity(1);

		// **클리어된 현재 씬 언로드 (Unity 기본 함수 사용)**
		// _targetSceneName을 이용하여 비동기 언로드 요청
		SceneManager.UnloadSceneAsync(_targetSceneName);

		// 이벤트 중복 방지
		_eventModule.Unsubscribe((int)GameEventId.StageClear, OnStageClear);

		// 다음 프로시저로 이동 (원하시는 프로시저로 변경 가능)
		_fsm.Owner.ChangeProcedure<ProcedureStagePlay>();
	}

	//--------------------------------------------------
	// 초기화 로직 (기존과 동일)
	//--------------------------------------------------

	private void SetupPlayer()
	{
		// ... (기존과 동일한 로직)
		var startObj = GameObject.FindWithTag("PlayerStart");
		Vector3 spawnPos = startObj != null ? startObj.transform.position + Vector3.up : Vector3.zero;

		_entityModule.ShowEntity(1, "Prefabs/Player", "Player", null);

		//var logic = _entityModule.GetEntityLogic(1) as PlayerLogic;
		//if (logic != null)
		//{
		//	logic.transform.position = spawnPos;

		//	CinemachineCamera virtualCam = GameObject.FindAnyObjectByType<CinemachineCamera>();
		//	if (virtualCam != null)
		//	{
		//		virtualCam.Follow = logic.transform;
		//		virtualCam.LookAt = logic.transform;
		//	}
		//}
	}

	private void SetupOtherThings()
	{
		// 기타 초기화
	}
}