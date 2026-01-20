using UnityEngine;

public enum GameEventId
{
	StageClear
}
public enum StartProcedureType
{
	None,       // 자동 시작 안 함
	map1_1,
	map1_2,
	map1_3,
	map1_4,
	map1_5,
	map1_6,
	map1_7,
	StageClear,
	// 필요하면 계속 추가
}
public static class GameState
{
	public static string BetaTestSceneName { get; set; }
	public static int CurrentStage = 1;   // 현재 스테이지 번호
	public const int MaxStage = 7;       // 지금은 7스테이지까지 있다고 가정
}
