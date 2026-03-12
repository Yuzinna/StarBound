using System;
using UnityEngine;

public enum eGravityDirection
{
	Normal,
	Inverse,
}

public class GravityManager : MonoBehaviour
{
	// 싱글톤
	public static GravityManager Instance { get; private set; }

	public eGravityDirection CurrentDirection { get; private set; } = eGravityDirection.Normal;
	public bool IsFloatingEnabled { get; private set; } = false;
	public float normalGravityScale { get; set; } = 3.0f;
	public float floatingGravityScale { get; set; } = 0.1f;

	// 이벤트
	public event Action<eGravityDirection> OnGravityDirectionChanged;
	public event Action<bool> OnFloatingStateChanged;

	// ==========================================
	// 🔒 [수정됨] 자물쇠를 2개로 분리했습니다!
	// ==========================================
	private int _directionLockCount = 0; // 방향(Inverse/Normal) 전용 자물쇠
	public bool IsDirectionLocked => _directionLockCount > 0;

	private int _floatingLockCount = 0;  // 부유(Floating) 전용 자물쇠
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
	}

	// ==========================================
	// 1. 방향 중력 잠금 함수 수정
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

				// 🚨 [제가 빼먹었던 바로 그 핵심 코드!!] 
				// 잠금이 풀렸으니 원래 중력(Normal)으로 강제로 되돌리고 알림을 쏴야 스위치가 꺼집니다!
				CurrentDirection = eGravityDirection.Normal;
				ApplyToAllGravityObjects();
				OnGravityDirectionChanged?.Invoke(CurrentDirection);
			}
		}
	}

	// ==========================================
	// 2. 부유(Floating) 잠금 함수 수정
	// ==========================================
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

				
				// 잠금이 풀렸으니 부유 상태를 끄고(false) 알림을 쏴야 스위치가 꺼집니다!
				IsFloatingEnabled = false;
				ApplyToAllGravityObjects();
				OnFloatingStateChanged?.Invoke(IsFloatingEnabled);
			}
		}
	}

	// ==========================================
	// 2. 수동 스위치 전용 함수 
	// ==========================================
	public bool TryChangeDirectionManual(eGravityDirection newDir)
	{
		//  부유 스위치가 잠겨있든 말든, "방향 자물쇠"만 안 잠겨있으면 통과!
		if (IsDirectionLocked)
		{
			Debug.Log("❌ [거부됨] 방향 중력 레이저가 켜져 있어 수동으로 바꿀 수 없습니다!");
			return false;
		}

		CurrentDirection = newDir;
		ApplyToAllGravityObjects();
		OnGravityDirectionChanged?.Invoke(CurrentDirection);
		return true;
	}

	public bool TryChangeFloatingManual(bool isFloating)
	{
		// 💡 방향 스위치가 잠겨있든 말든, "부유 자물쇠"만 안 잠겨있으면 통과!
		if (IsFloatingLocked)
		{
			Debug.Log("❌ [거부됨] 부유 중력 레이저가 켜져 있어 수동으로 바꿀 수 없습니다!");
			return false;
		}

		IsFloatingEnabled = isFloating;
		ApplyToAllGravityObjects();
		OnFloatingStateChanged?.Invoke(IsFloatingEnabled);
		return true;
	}

	// ==========================================
	// 기존 함수들 (유지)
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
			obj.ApplyGravityAndFloatingState(CurrentDirection, IsFloatingEnabled);
		}

		var players = FindObjectsByType<PlayerLogic>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
		foreach (var player in players)
		{
			player.ApplyGravityAndFloatingState(CurrentDirection, IsFloatingEnabled);
		}
	}
}