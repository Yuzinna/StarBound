//using System;
//using UnityEngine;

//public enum eGravityDirection
//{
//	Normal,
//	Inverse,
//}

//public class GravityManager : MonoBehaviour
//{
//	// 싱글톤
//	public static GravityManager Instance { get; private set; }

//	public eGravityDirection CurrentDirection { get; private set; } = eGravityDirection.Normal;
//	public bool IsFloatingEnabled { get; private set; } = false;
//	public float normalGravityScale { get; set; } = 3.0f;
//	public float floatingGravityScale { get; set; } = 0.1f;

//	// 이벤트
//	public event Action<eGravityDirection> OnGravityDirectionChanged;
//	public event Action<bool> OnFloatingStateChanged;

//	// ==========================================
//	// 🔒 [수정됨] 자물쇠를 2개로 분리했습니다!
//	// ==========================================
//	private int _directionLockCount = 0; // 방향(Inverse/Normal) 전용 자물쇠
//	public bool IsDirectionLocked => _directionLockCount > 0;

//	private int _floatingLockCount = 0;  // 부유(Floating) 전용 자물쇠
//	public bool IsFloatingLocked => _floatingLockCount > 0;

//	private void Awake()
//	{
//		if (Instance != null && Instance != this)
//		{
//			Destroy(gameObject);
//			return;
//		}
//		Instance = this;
//		DontDestroyOnLoad(gameObject);
//	}

//	// ==========================================
//	// 1. 방향 중력 잠금 함수 수정
//	// ==========================================
//	public void SetDirectionAndLock(eGravityDirection newDir, bool isLocking)
//	{
//		if (isLocking)
//		{
//			_directionLockCount++;
//			CurrentDirection = newDir;
//			ApplyToAllGravityObjects();
//			OnGravityDirectionChanged?.Invoke(CurrentDirection);
//		}
//		else
//		{
//			_directionLockCount--;
//			if (_directionLockCount <= 0)
//			{
//				_directionLockCount = 0;

//				// 🚨 [제가 빼먹었던 바로 그 핵심 코드!!] 
//				// 잠금이 풀렸으니 원래 중력(Normal)으로 강제로 되돌리고 알림을 쏴야 스위치가 꺼집니다!
//				CurrentDirection = eGravityDirection.Normal;
//				ApplyToAllGravityObjects();
//				OnGravityDirectionChanged?.Invoke(CurrentDirection);
//			}
//		}
//	}

//	// ==========================================
//	// 2. 부유(Floating) 잠금 함수 수정
//	// ==========================================
//	public void SetFloatingAndLock(bool isFloating, bool isLocking)
//	{
//		if (isLocking)
//		{
//			_floatingLockCount++;
//			IsFloatingEnabled = isFloating;
//			ApplyToAllGravityObjects();
//			OnFloatingStateChanged?.Invoke(IsFloatingEnabled);
//		}
//		else
//		{
//			_floatingLockCount--;
//			if (_floatingLockCount <= 0)
//			{
//				_floatingLockCount = 0;


//				// 잠금이 풀렸으니 부유 상태를 끄고(false) 알림을 쏴야 스위치가 꺼집니다!
//				IsFloatingEnabled = false;
//				ApplyToAllGravityObjects();
//				OnFloatingStateChanged?.Invoke(IsFloatingEnabled);
//			}
//		}
//	}

//	// ==========================================
//	// 2. 수동 스위치 전용 함수 
//	// ==========================================
//	public bool TryChangeDirectionManual(eGravityDirection newDir)
//	{
//		//  부유 스위치가 잠겨있든 말든, "방향 자물쇠"만 안 잠겨있으면 통과!
//		if (IsDirectionLocked)
//		{
//			Debug.Log("❌ [거부됨] 방향 중력 레이저가 켜져 있어 수동으로 바꿀 수 없습니다!");
//			return false;
//		}

//		CurrentDirection = newDir;
//		ApplyToAllGravityObjects();
//		OnGravityDirectionChanged?.Invoke(CurrentDirection);
//		return true;
//	}

//	public bool TryChangeFloatingManual(bool isFloating)
//	{
//		// 💡 방향 스위치가 잠겨있든 말든, "부유 자물쇠"만 안 잠겨있으면 통과!
//		if (IsFloatingLocked)
//		{
//			Debug.Log("❌ [거부됨] 부유 중력 레이저가 켜져 있어 수동으로 바꿀 수 없습니다!");
//			return false;
//		}

//		IsFloatingEnabled = isFloating;
//		ApplyToAllGravityObjects();
//		OnFloatingStateChanged?.Invoke(IsFloatingEnabled);
//		return true;
//	}

//	// ==========================================
//	// 기존 함수들 (유지)
//	// ==========================================
//	public void SetDirection(eGravityDirection direction)
//	{
//		CurrentDirection = direction;
//		ApplyToAllGravityObjects();
//		OnGravityDirectionChanged?.Invoke(CurrentDirection);
//	}

//	public void SetFloatingMode(bool isFloating)
//	{
//		IsFloatingEnabled = isFloating;
//		ApplyToAllGravityObjects();
//		OnFloatingStateChanged?.Invoke(IsFloatingEnabled);
//	}

//	private void ApplyToAllGravityObjects()
//	{
//		var objects = FindObjectsByType<GravityObjectLogic>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
//		foreach (var obj in objects)
//		{
//			obj.ApplyGravityAndFloatingState(CurrentDirection, IsFloatingEnabled);
//		}

//		var players = FindObjectsByType<PlayerLogic>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
//		foreach (var player in players)
//		{
//			player.ApplyGravityAndFloatingState(CurrentDirection, IsFloatingEnabled);
//		}
//	}
//}
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum eGravityDirection
{
	Normal,
	Inverse,
}

public class GravityManager : MonoBehaviour
{
	public static GravityManager Instance { get; private set; }

	public eGravityDirection CurrentDirection { get; private set; } = eGravityDirection.Normal;
	public bool IsFloatingEnabled { get; private set; } = false;
	public float normalGravityScale { get; set; } = 3.0f;
	public float floatingGravityScale { get; set; } = 0.1f;

	public event Action<eGravityDirection> OnGravityDirectionChanged;
	public event Action<bool> OnFloatingStateChanged;

	private int _directionLockCount = 0;
	public bool IsDirectionLocked => _directionLockCount > 0;

	private int _floatingLockCount = 0;
	public bool IsFloatingLocked => _floatingLockCount > 0;

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;
		DontDestroyOnLoad(gameObject);

		// 💡 [수정됨] OnEnable이 아니라, 진짜 싱글톤 본체만 여기서 딱 한 번 구독하게 만듭니다. (중복 구독 버그 방지)
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			SceneManager.sceneLoaded -= OnSceneLoaded;
		}
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		ResetGravityState();
	}

	private void ResetGravityState()
	{
		// 🚨 [핵심 해결책] 이전 씬의 유령 스위치들이 이벤트를 듣고 에러를 뿜지 못하도록, 
		// 씬이 재시작되면 매니저에 연결된 모든 이벤트(구독자)를 가차 없이 초기화(null) 해버립니다!
		OnGravityDirectionChanged = null;
		OnFloatingStateChanged = null;

		CurrentDirection = eGravityDirection.Normal;
		IsFloatingEnabled = false;

		_directionLockCount = 0;
		_floatingLockCount = 0;

		ApplyToAllGravityObjects();

		// (이벤트를 날릴 필요도 없습니다. 새 스위치들은 Start()에서 이 정상화된 상태를 스스로 읽어갑니다.)
		Debug.Log("🔄 맵 초기화 완료! 중력과 시스템 잠금이 완벽하게 포맷되었습니다.");
	}

	// ==========================================
	// 🔫 1. 레이저 전용 함수
	// ==========================================
	public void SetDirectionAndLock(eGravityDirection newDir, bool isLocking)
	{
		if (isLocking)
		{
			_directionLockCount++;
			CurrentDirection = newDir;
			ApplyToAllGravityObjects();
			OnGravityDirectionChanged?.Invoke(CurrentDirection);
		}
		else
		{
			_directionLockCount--;
			if (_directionLockCount <= 0)
			{
				_directionLockCount = 0;
				CurrentDirection = eGravityDirection.Normal;
				ApplyToAllGravityObjects();
				OnGravityDirectionChanged?.Invoke(CurrentDirection);
			}
		}
	}

	public void SetFloatingAndLock(bool isFloating, bool isLocking)
	{
		if (isLocking)
		{
			_floatingLockCount++;
			IsFloatingEnabled = isFloating;
			ApplyToAllGravityObjects();
			OnFloatingStateChanged?.Invoke(IsFloatingEnabled);
		}
		else
		{
			_floatingLockCount--;
			if (_floatingLockCount <= 0)
			{
				_floatingLockCount = 0;
				IsFloatingEnabled = false;
				ApplyToAllGravityObjects();
				OnFloatingStateChanged?.Invoke(IsFloatingEnabled);
			}
		}
	}

	// ==========================================
	// 🔘 2. 수동 스위치 전용 함수 
	// ==========================================
	public bool TryChangeDirectionManual(eGravityDirection newDir)
	{
		if (IsDirectionLocked) return false;

		CurrentDirection = newDir;
		ApplyToAllGravityObjects();
		OnGravityDirectionChanged?.Invoke(CurrentDirection);
		return true;
	}

	public bool TryChangeFloatingManual(bool isFloating)
	{
		if (IsFloatingLocked) return false;

		IsFloatingEnabled = isFloating;
		ApplyToAllGravityObjects();
		OnFloatingStateChanged?.Invoke(IsFloatingEnabled);
		return true;
	}

	// ==========================================
	// 기존 함수 및 일괄 적용 함수
	// ==========================================
	public void SetDirection(eGravityDirection direction)
	{
		CurrentDirection = direction;
		ApplyToAllGravityObjects();
		OnGravityDirectionChanged?.Invoke(CurrentDirection);
	}

	public void SetFloatingMode(bool isFloating)
	{
		IsFloatingEnabled = isFloating;
		ApplyToAllGravityObjects();
		OnFloatingStateChanged?.Invoke(IsFloatingEnabled);
	}

	private void ApplyToAllGravityObjects()
	{
		var objects = FindObjectsByType<GravityObjectLogic>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
		foreach (var obj in objects)
		{
			// 🚨 [수정됨] 큐브의 바뀐 함수 이름(ApplyGravityState)으로 호출합니다.
			obj.ApplyGravityState(CurrentDirection, IsFloatingEnabled);
		}

		var players = FindObjectsByType<PlayerLogic>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
		foreach (var player in players)
		{
			// 플레이어는 기존 함수 이름 그대로 둡니다.
			player.ApplyGravityAndFloatingState(CurrentDirection, IsFloatingEnabled);
		}
	}
}