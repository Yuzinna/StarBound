using UnityEngine;

// 이 스크립트를 넣으면 유니티가 알아서 Rigidbody2D를 달아줍니다!
[RequireComponent(typeof(Rigidbody2D))]
public class MovingBlock : MonoBehaviour
{
	[SerializeField] Transform startPoint;
	[SerializeField] Transform endPoint;
	public float speed = 1f;

	private Rigidbody2D _rb;

	private void Awake()
	{
		_rb = GetComponent<Rigidbody2D>();

		// [아주 중요!] 블럭은 중력에 안 떨어지고 코드로만 움직여야 하니 Kinematic으로 설정!
		_rb.bodyType = RigidbodyType2D.Kinematic;
	}

	// 물리적인 이동은 반드시 Update가 아니라 FixedUpdate에서 해야 합니다!
	private void FixedUpdate()
	{
		float t = Mathf.PingPong(Time.time * speed, 1f);
		Vector2 newPos = Vector2.Lerp(startPoint.position, endPoint.position, t);

		// transform.position 대신 물리 엔진의 힘으로 밀어붙이는 MovePosition 사용!
		_rb.MovePosition(newPos);
	}
}