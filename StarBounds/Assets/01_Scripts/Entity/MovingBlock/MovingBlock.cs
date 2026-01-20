using UnityEngine;

public class MovingBlock : MonoBehaviour
{
	[SerializeField]  Transform endPoint;
	[SerializeField] Transform startPoint;
	public float speed = 1f;
	Transform destinationPoint;
	private void Awake()
	{
		
	}
	private void Start()
	{
		destinationPoint = endPoint;
		transform.position = startPoint.position;
		
	}
	private void Update()
	{
		float t = Mathf.PingPong(Time.time * speed, 1f);
		transform.position = Vector3.Lerp(startPoint.position, endPoint.position, t);
	}
}
