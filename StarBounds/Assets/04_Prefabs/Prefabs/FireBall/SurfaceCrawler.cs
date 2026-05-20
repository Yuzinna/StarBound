using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class SurfaceCrawler : MonoBehaviour
{
	[Header("이동 설정")]
	[Tooltip("불꽃이 이동할 속도")]
	public float speed = 5f;
	[Tooltip("시계 방향(True) 또는 반시계 방향(False)")]
	public bool moveClockwise = true;

	[Header("레이캐스트 (더듬이) 설정")]
	[Tooltip("바닥/벽/큐브로 인식할 레이어 (Ground, Pushable 등 체크)")]
	public LayerMask surfaceLayer;
	[Tooltip("앞쪽 벽을 감지하는 더듬이 길이")]
	public float forwardRayLength = 0.5f;
	[Tooltip("아래쪽 바닥을 감지하는 더듬이 길이")]
	public float downwardRayLength = 1.0f;
	[Tooltip("표면에서 얼마나 떨어져서 이동할지 (불꽃 크기에 맞춰 조절)")]
	public float distanceToSurface = 0.5f;

	[Header("시각 연출")]
	[Tooltip("회전시킬 실제 그래픽 오브젝트 (자식 오브젝트여야 함)")]
	public Transform modelTransform;
	public float rotationSpeed = 15f;

	// --- 내부 계산용 변수 ---
	private Transform _rotationRef; // 보이지 않는 영혼(실제 물리적 회전 기준점)
	private Vector2 _moveDirection; // 현재 나아갈 방향
	private Vector2 _rayOrigin;
	private Quaternion _targetRotation;

	// 💡 쿨다운 대신 "현재 불꽃이 밟고 있는 스위치들"을 기억하는 플래그 리스트!
	private List<SwitchLogic> _activeSwitches = new List<SwitchLogic>();

	public AudioClip killSfx;
	private void Awake()
	{
		// Collider는 트리거로 작동해야 플레이어와 겹쳤을 때 죽일 수 있음
		GetComponent<Collider2D>().isTrigger = true;

		// 회전 기준점(영혼)을 투명하게 만듭니다.
		_rotationRef = new GameObject("CrawlerRotationRef").transform;
		_rotationRef.SetParent(this.transform);
		_rotationRef.localPosition = Vector3.zero;
		_rotationRef.rotation = transform.rotation;

		_targetRotation = _rotationRef.rotation;
	}

	private void FixedUpdate()
	{
		HandleCrawlPhysics();
		HandleModelRotation();
	}

	/// <summary>
	/// 더듬이로 벽과 바닥을 감지하고 이동하는 핵심 로직
	/// </summary>
	private void HandleCrawlPhysics()
	{
		_rayOrigin = transform.position;
		Vector2 forwardDir = moveClockwise ? _rotationRef.right : -_rotationRef.right;

		RaycastHit2D forwardHit = Physics2D.Raycast(_rayOrigin, forwardDir, forwardRayLength, surfaceLayer);
		Debug.DrawRay(_rayOrigin, forwardDir * forwardRayLength, Color.red);

		if (forwardHit)
		{
			// 🚨 수정됨: forwardHit.point (부딪힌 정확한 좌표)를 넘겨줍니다.
			AlignToSurface(forwardHit.normal, forwardHit.point);
		}
		else
		{
			Vector2 downwardDir = -_rotationRef.up;
			RaycastHit2D downwardHit = Physics2D.Raycast(_rayOrigin, downwardDir, downwardRayLength, surfaceLayer);
			Debug.DrawRay(_rayOrigin, downwardDir * downwardRayLength, Color.blue);

			if (downwardHit)
			{
				// 🚨 수정됨: downwardHit.point를 넘겨줍니다.
				AlignToSurface(downwardHit.normal, downwardHit.point);
			}
			else
			{
				bool foundNewGround = false;
				Quaternion originalRot = _rotationRef.rotation;

				for (int i = 0; i < 9; i++)
				{
					float angle = moveClockwise ? -10f : 10f;
					_rotationRef.Rotate(0, 0, angle);

					downwardHit = Physics2D.Raycast(_rayOrigin, -_rotationRef.up, downwardRayLength, surfaceLayer);

					if (downwardHit)
					{
						// 🚨 수정됨: downwardHit.point를 넘겨줍니다.
						AlignToSurface(downwardHit.normal, downwardHit.point);
						foundNewGround = true;
						break;
					}
				}

				if (!foundNewGround)
				{
					_rotationRef.rotation = originalRot;
				}
			}
		}

		transform.Translate(_moveDirection * speed * Time.fixedDeltaTime, Space.World);
	}

	/// <summary>
	/// 닿은 벽의 각도에 맞춰서 몸을 꺾고 밀착시키는 함수
	/// </summary>
	// 🚨 수정됨: Vector2 hitPoint 매개변수가 추가되었습니다!
	private void AlignToSurface(Vector2 surfaceNormal, Vector2 hitPoint)
	{
		_moveDirection = moveClockwise ? new Vector2(surfaceNormal.y, -surfaceNormal.x) : new Vector2(-surfaceNormal.y, surfaceNormal.x);

		float angle = Mathf.Atan2(_moveDirection.y, _moveDirection.x) * Mathf.Rad2Deg;
		_targetRotation = Quaternion.Euler(0, 0, angle);
		_rotationRef.rotation = _targetRotation;

		// 🚨 핵심 해결책: 현재 위치가 아니라 '실제로 닿은 표면(hitPoint)'을 기준으로 거리를 띄웁니다!
		transform.position = hitPoint + (surfaceNormal * distanceToSurface);
	}
	/// <summary>
	/// 닿은 벽의 각도에 맞춰서 몸을 꺾고 밀착시키는 함수
	/// </summary>
	private void AlignToSurface(Vector2 surfaceNormal)
	{
		// 벽의 수직 방향(Normal)을 기준으로 시계/반시계로 90도 꺾어서 나아갈 방향 설정
		_moveDirection = moveClockwise ? new Vector2(surfaceNormal.y, -surfaceNormal.x) : new Vector2(-surfaceNormal.y, surfaceNormal.x);

		// 회전 기준점 각도 갱신
		float angle = Mathf.Atan2(_moveDirection.y, _moveDirection.x) * Mathf.Rad2Deg;
		_targetRotation = Quaternion.Euler(0, 0, angle);
		_rotationRef.rotation = _targetRotation;

		// 벽 표면에 딱 붙게 위치 보정 (안으로 파고드는 버그 방지)
		transform.position = (Vector2)transform.position + (surfaceNormal * distanceToSurface);
	}

	/// <summary>
	/// 눈에 보이는 그래픽 모델을 스무스하게 회전시킵니다.
	/// </summary>
	private void HandleModelRotation()
	{
		if (modelTransform == null) return;
		// 1프레임만에 홱! 꺾이지 않고 부드럽게 돌아가도록 Slerp 사용
		modelTransform.rotation = Quaternion.Slerp(modelTransform.rotation, _targetRotation, Time.fixedDeltaTime * rotationSpeed);
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		// 1. 코기(플레이어)에 닿으면 즉사
		if (collision.CompareTag("Player"))
		{
			Debug.Log("🔥 불꽃에 닿아서 코기가 죽었습니다!");
			KillAndRespawnPlayer(collision.gameObject);
			return;
		}

		// 2. 스위치에 처음 진입(Enter)했을 때!
		SwitchLogic switchLogic = collision.GetComponent<SwitchLogic>();
		if (switchLogic != null)
		{
			// 플래그 확인: 아직 밟고 있는 리스트에 없는 스위치라면?
			if (!_activeSwitches.Contains(switchLogic))
			{
				_activeSwitches.Add(switchLogic); // 플래그 ON (리스트에 추가)

				switchLogic.Interact(null); // 스위치 작동!
				Debug.Log($"🔥 불꽃이 스위치({switchLogic.switchType})를 작동시켰습니다!");
			}
		}
	}

	// ========================================================
	// 🔥 스위치 영역 벗어남 (Exit)
	// ========================================================
	private void OnTriggerExit2D(Collider2D collision)
	{
		// 불꽃이 스위치에서 완전히 빠져나갔을 때!
		SwitchLogic switchLogic = collision.GetComponent<SwitchLogic>();
		if (switchLogic != null)
		{
			// 플래그 확인: 밟고 있던 스위치였다면?
			if (_activeSwitches.Contains(switchLogic))
			{
				_activeSwitches.Remove(switchLogic); // 플래그 OFF (리스트에서 제거)

				// (참고: 불꽃이 지나가면서 "눌렀다가 떼는(압력 발판)" 방식이 아니라
				// 기획자님의 Interact처럼 "토글(스위치형)" 방식이므로 여기서 Interact를 다시 부르지는 않습니다!)
			}
		}
	}
	private void KillAndRespawnPlayer(GameObject player)
	{
		if (killSfx != null && SfxManager.Instance != null) SfxManager.Instance.PlaySfx(killSfx);

		PlayerDeath playerDeath = player.GetComponent<PlayerDeath>();
		if (playerDeath != null) playerDeath.Die();
		else SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}
}