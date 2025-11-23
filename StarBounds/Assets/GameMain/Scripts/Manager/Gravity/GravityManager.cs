using System;
using UnityEditor.Search;
using UnityEngine;


public enum eGravityDirection
{
	Normal,
	Inverse,

}
public class GravityManager : MonoBehaviour
{
	//싱글톤
	static public GravityManager Instance { get;private set; }

	public eGravityDirection CurrentDirection { get; private set; } = eGravityDirection.Normal;
	public bool IsFloatingEnabled { get; private set; } = false; // ⬅️ 부유 특성 플래그 추가
	public float normalGravityScale { get; set; } =3.0f;
	public float floatingGravityScale { get; set; } = 0.1f;

	// 중력 방향이 바뀔 때 발생하는 이벤트
	public event Action<eGravityDirection> OnGravityDirectionChanged;
	// Floating 상태가 바뀔 때 발생하는 이벤트
	public event Action<bool> OnFloatingStateChanged;

	[Header("부유 상태 움직임 세팅")]
	public float floatingUpForce = 0.5f;   // 위로 살짝 밀어주는 힘
	public float floatingDrag = 2f;        // 부유 상태일 때 drag
	public float normalDrag = 0f;          // 일반 상태 drag


	private void Awake()
	{
		if(Instance != null&& Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;
		DontDestroyOnLoad(gameObject);
	}

	public void ChangeGravityDirection(eGravityDirection newDirection)
	{
		// ... 실제 중력 변경 로직 ...
		OnGravityDirectionChanged?.Invoke(newDirection); // 이벤트 발생
	}

	public void SetFloatingEnabled(bool isEnabled)
	{
		// ... 실제 Floating 상태 변경 로직 ...
		OnFloatingStateChanged?.Invoke(isEnabled); // 이벤트 발생
	}
	//public void SetState(GravityState state)
	//{
	//	CurrentState = state;
	//	ApplyToAllGravityObjects();
	//}
	// SetState 대신 SetDirection과 SetFloatingMode를 사용
	public void SetDirection(eGravityDirection direction)
	{
		CurrentDirection = direction;
		ApplyToAllGravityObjects(); // GravityObjectLogic에서 이 두 상태를 모두 받아 처리하도록 변경
		OnGravityDirectionChanged?.Invoke(CurrentDirection);
	}

	public void SetFloatingMode(bool isFloating)
	{
		IsFloatingEnabled = isFloating;
		ApplyToAllGravityObjects();
		OnFloatingStateChanged?.Invoke(IsFloatingEnabled);
	}
	//public void ToggleState()
	//{
	//	if (CurrentState == GravityState.Normal)
	//		SetState(GravityState.Floating);
	//	else
	//		SetState(GravityState.Normal);
	//}
	private void ApplyToAllGravityObjects()
	{
		//여긴 큐브같은
		var objects = FindObjectsByType<GravityObjectLogic>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
		foreach (var obj in objects)
		{
			// ⬅️ ApplyState 대신 두 개의 인자를 받는 새로운 메서드를 호출해야 합니다.
			obj.ApplyGravityAndFloatingState(CurrentDirection, IsFloatingEnabled);
		}

		// 2) 플레이어에도 중력 상태에 따른 점프력 적용
		var players = FindObjectsByType<PlayerLogic>(
			FindObjectsInactive.Exclude,
			FindObjectsSortMode.None
		);
		foreach (var player in players)
		{
			player.ApplyGravityAndFloatingState(CurrentDirection, IsFloatingEnabled);
		}
	}
	
}
