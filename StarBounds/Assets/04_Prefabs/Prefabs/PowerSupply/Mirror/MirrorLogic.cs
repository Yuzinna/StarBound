using UnityEngine;

public class MirrorLogic : MonoBehaviour
{
	[Header("거울 회전 설정")]
	[Tooltip("거울이 돌아가는 속도 (초당 회전 각도). 예: 270이면 90도 도는 데 0.33초 걸림")]
	[SerializeField] private float rotationSpeed = 270f;

	[Tooltip("거울이 돌아갈 때 날 소리 (돌 긁히는 소리 등)")]
	[SerializeField] private AudioClip rotateSfx;
	[SerializeField, Range(0f, 1f)] private float sfxVolume = 1f;

	// 시각적으로 현재 돌아가고 있는 실제 각도
	private float _currentVisualAngle;

	// 거울이 최종적으로 멈춰야 할 '목표 각도'
	private float _targetAngle;

	private void Start()
	{
		// 1. 시작할 때 배치된 각도를 읽어와 90도 단위로 세팅
		_targetAngle = Mathf.RoundToInt(transform.eulerAngles.z / 90f) * 90;
		_currentVisualAngle = _targetAngle;

		//transform.rotation = Quaternion.Euler(0f, 0f, _currentVisualAngle);
	}

	private void Update()
	{
		// 3. 목표 각도와 현재 각도가 다르다면? -> 목표를 향해 부드럽게 회전!
		if (Mathf.Abs(_currentVisualAngle - _targetAngle) > 0.01f)
		{
			// MoveTowards는 설정한 속도(rotationSpeed)에 맞춰 목표값까지 부드럽게 숫자를 이동시킵니다.
			_currentVisualAngle = Mathf.MoveTowards(_currentVisualAngle, _targetAngle, rotationSpeed * Time.deltaTime);

			// 실제 거울 오브젝트 회전 적용
			transform.rotation = Quaternion.Euler(0f, 0f, _currentVisualAngle);
		}
	}

	// 발판이 밟힐 때마다 이 함수가 호출됩니다.
	public void RotateMirror90Degrees()
	{
		// 2. 당장 회전시키지 않고, "목표 각도를 90도 더 돌려라!" 라고 예약만 해둡니다.
		_targetAngle -= 90f; // 시계 방향 회전

		// 소리 재생 (회전 명령이 떨어질 때마다 딸깍! 또는 드르륵!)
		if (SfxManager.Instance != null && rotateSfx != null)
		{
			SfxManager.Instance.PlaySfx(rotateSfx, sfxVolume, 0f);
		}
	}
}