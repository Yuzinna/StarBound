using GameFrameworkLite;
using UnityEngine;


/// <summary>
/// 이 클래스는 프레임워크를 사용하는 "게임 진입점" 예시.
/// 플레이어 엔티티 하나를 생성해서 위치만 잡아주는 샘플.
/// </summary>
public class GameEntryLite : MonoBehaviour
{

	[Header("에디터 전용: 이 씬에서 바로 시작할 프로시저")]
	[SerializeField] private StartProcedureType startProcedure = StartProcedureType.Stage1_1;
	private void Start()
	{

		Debug.Log("[GameEntryLite] Start – Procedure 초기화 시작");
		// 1. 프로시저 모듈 가져오기
		var procedure = GameFrameworkEntry.GetModule<ProcedureModule>();

		//2.사용할 프로시저들 등록
		procedure.Initialize(
			new ProcedureStage1(),
			new ProcedureStageClear(),
			new ProcedureStage2()
		);

		// 3. 처음 시작할 프로시저 지정
		procedure.StartProcedure<ProcedureStage1>();
	}
}

