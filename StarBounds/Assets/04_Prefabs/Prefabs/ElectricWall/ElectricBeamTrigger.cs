using UnityEngine;

// 자식 오브젝트(ElectricBeams)에 붙어서 충돌을 부모에게 토스해주는 역할
[RequireComponent(typeof(BoxCollider2D))]
public class ElectricBeamTrigger : MonoBehaviour
{
	public ElectricWall parentWall;

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (parentWall != null) parentWall.OnBeamEnter(collision);
	}

	private void OnTriggerStay2D(Collider2D collision)
	{
		if (parentWall != null) parentWall.OnBeamStay(collision);
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		if (parentWall != null) parentWall.OnBeamExit(collision);
	}
}