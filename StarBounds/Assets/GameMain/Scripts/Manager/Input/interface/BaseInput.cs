using System;
using UnityEngine;

public class BaseInput : MonoBehaviour
{
	/// <summary>좌우 이동 등 방향 입력</summary>
	public Vector2 MoveDir { get; protected set; }

	/// <summary>이 프레임에 점프 버튼이 눌렸는지 (WasPressedThisFrame 느낌)</summary>
	public bool JumpPressed { get; protected set; }

	public bool InteractPressd { get; protected set; }
	public event Action InteractAction;
	public event Action JumpAction;
	public event Action DropAction;
	public event Action PauseAction;
	/// <summary>
	/// 콜백에서 Jump/Interact를 켜고,
	/// 이 메서드에서 한 프레임짜리 플래그들을 초기화해준다.
	/// (LateUpdate에서 호출하는 용도)
	/// </summary>


	// ---- 아래 메서드들은 자식 클래스(플레이어, UI)가 콜백에서 호출하는 헬퍼 ----

	protected void SetMove(Vector2 dir)
	{
		MoveDir = dir;
	}

	protected void OnInteraction()
	{
		InteractAction?.Invoke();
	}
	protected void OnJumpaction()
	{
		JumpAction?.Invoke();
	}
	protected void OnDropaction()
	{
		DropAction?.Invoke();
	}
	protected void OnPauseAction()
	{
		PauseAction?.Invoke();
	}
}
