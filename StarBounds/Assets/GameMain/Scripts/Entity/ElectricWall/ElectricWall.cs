using UnityEngine;

public class ElectricWall : MonoBehaviour
{
	// 🚧 컴포넌트 참조
	[Header("충돌체 및 렌더러")]
	[SerializeField] private Collider2D wallCollider;
	[SerializeField] private Renderer wallRenderer;

	// 💡 애니메이션 설정
	[Header("애니메이션 설정")]
	[SerializeField] private float frameDuration = 0.05f; // 프레임당 시간 간격
	[SerializeField] private Sprite[] animationSprites; // 모든 애니메이션 프레임 스프라이트

	// 💡 자식 SpriteRenderer 참조
	[Header("자식 SpriteRenderer 참조")]
	[SerializeField] private SpriteRenderer topRenderer;
	[SerializeField] private SpriteRenderer middleRenderer;
	[SerializeField] private SpriteRenderer bottomRenderer;

	private bool _isWallActive = false;
	private Coroutine _animationCoroutine;


	private void Awake()
	{
		topRenderer= transform.Find("Top").gameObject.GetComponent<SpriteRenderer>();
		middleRenderer=transform.Find("Middle").gameObject.GetComponent<SpriteRenderer>();
		bottomRenderer=transform.Find("Bottom").gameObject.GetComponent<SpriteRenderer>();
		wallCollider = GetComponent<BoxCollider2D>();
	}
	private void Start()
	{
		if (GravityManager.Instance != null)
		{
			// 초기 상태 확인 및 설정
			_isWallActive = CheckWallActivation(
				GravityManager.Instance.CurrentDirection,
				GravityManager.Instance.IsFloatingEnabled
			);
			UpdateWallState();

			// 두 가지 이벤트를 모두 구독하여 중력 상태 변화를 감지
			GravityManager.Instance.OnGravityDirectionChanged += OnGravityStateChanged;
			GravityManager.Instance.OnFloatingStateChanged += OnFloatingStateChanged;
		}
	}
	private void OnDestroy()
	{
		if (GravityManager.Instance != null)
		{
			// 구독 해제
			GravityManager.Instance.OnGravityDirectionChanged -= OnGravityStateChanged;
			GravityManager.Instance.OnFloatingStateChanged -= OnFloatingStateChanged;
		}
		// 오브젝트 파괴 시 코루틴도 확실히 중지
		if (_animationCoroutine != null)
		{
			StopCoroutine(_animationCoroutine);
		}
	}

}	
