using UnityEngine;

public class ElectricWall : MonoBehaviour
{
	// 🚧 컴포넌트 참조
	[Header("충돌체")]
	// 부모 오브젝트가 충돌체를 담당하여 활성화/비활성화만 제어합니다.
	[SerializeField] private Collider2D wallCollider;

	// 💡 애니메이션 제어 설정
	[Header("자식 Animator 참조")]
	[SerializeField] private Animator topAnimator;
	[SerializeField] private Animator middleAnimator;
	[SerializeField] private Animator bottomAnimator;

	// Animator 컨트롤러에 정의할 Bool 파라미터 이름 (예: "IsActive")
	[SerializeField] private string activationParameterName = "IsActive";

	private bool _isWallActive = false;

	private void Awake()
	{
		topAnimator = transform.Find("Top").gameObject.GetComponent<Animator>();
		middleAnimator = transform.Find("Middle").gameObject.GetComponent<Animator>();
		bottomAnimator = transform.Find("Bottom").gameObject.GetComponent<Animator>();
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
	}
	//중력 방향이 바뀌었을때
	private void OnGravityStateChanged(eGravityDirection newDirection)
	{
		UpdateActivationState();
	}
	//중력 강도가 바뀌었을때
	private void OnFloatingStateChanged(bool isEnabled)
	{
		UpdateActivationState();
	}
	private bool CheckWallActivation(eGravityDirection direction, bool isFloating)
	{
		// Inverse 중력 또는 Floating 모드 활성화 시 -> 활성화
		if (direction == eGravityDirection.Inverse || isFloating)
		{
			return true;
		}
		// Normal 중력, Floating 비활성화인 안전 상태 -> 비활성화
		return false;
	}

	private void UpdateActivationState()
	{
		if (GravityManager.Instance == null) return;

		eGravityDirection currentDirection = GravityManager.Instance.CurrentDirection;
		bool isFloating = GravityManager.Instance.IsFloatingEnabled;

		// 벽 활성화 여부를 판단하고 상태 업데이트
		_isWallActive = CheckWallActivation(currentDirection, isFloating);

		UpdateWallState();
	}
	private void UpdateWallState()
	{
		//충돌체와 렌더러 상태 업데이트
		if (wallCollider != null)
		{
			wallCollider.enabled = _isWallActive;
		}
		// 💡 자식 Animator 제어 로직
		SetAnimatorState(_isWallActive);
	}
	private void SetAnimatorState(bool isActive)
	{
		// 세 자식 Animator 모두에게 동일한 bool 파라미터 값을 전달하여 애니메이션을 제어
		if (topAnimator != null)
		{
			topAnimator.SetBool(activationParameterName, isActive);
		}
		if (middleAnimator != null)
		{
			middleAnimator.SetBool(activationParameterName, isActive);
		}
		if (bottomAnimator != null)
		{
			bottomAnimator.SetBool(activationParameterName, isActive);
		}
	}
}	
