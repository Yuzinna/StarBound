using UnityEngine;

public class SinglePlatformDectect : MonoBehaviour
{
	public Collider2D _platformCollider;
	private Collider2D _detectCollider;

	private void Awake()
	{
		
		_detectCollider=GetComponent<BoxCollider2D>();
	}
	private void OnTriggerExit2D(Collider2D collision)
	{
		if(collision.gameObject.layer==3)
		{
			Physics2D.IgnoreCollision(collision, _platformCollider, false);
		}
	}
}
