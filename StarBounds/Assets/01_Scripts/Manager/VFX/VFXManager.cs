using UnityEngine;

public class VFXManager : MonoBehaviour
{
	// 싱글톤 패턴 (어디서든 부를 수 있게)
	public static VFXManager Instance { get; private set; }

	[Header("파티클 프리팹")]
	public GameObject jumpDustPrefab; // 점프 먼지
	public GameObject landDustPrefab; // 착지/충돌 먼지

	// (선택) 쿵쿵이처럼 엄청 무거운 애들을 위한 더 큰 먼지를 원한다면 추가!
	public GameObject heavyLandDustPrefab;

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;
		// 씬이 넘어가도 매니저가 파괴되지 않게 유지
		DontDestroyOnLoad(gameObject);
	}

	//  점프할 때 (위치는 발밑)
	public void PlayJumpDust(Vector3 position)
	{
		if (jumpDustPrefab != null)
		{
			// 발밑 위치에 파티클을 '생성'만 하고 버려둡니다! (알아서 터지고 알아서 삭제됨)
			Instantiate(jumpDustPrefab, position, Quaternion.identity);
		}
	}

	// 착지하거나 부딪힐 때 (위치는 바닥)
	// 회전값(rotation)을 추가로 받는 이유는, 쿵쿵이가 천장에 부딪힐 땐 먼지가 아래로 퍼져야 하기 때문입니다!
	public void PlayLandDust(Vector3 position, Quaternion rotation = default, bool isHeavy = false)
	{
		GameObject prefabToUse = (isHeavy && heavyLandDustPrefab != null) ? heavyLandDustPrefab : landDustPrefab;

		if (prefabToUse != null)
		{
			// 전달받은 회전값(혹은 기본값)으로 파티클 생성
			Instantiate(prefabToUse, position, (rotation == default ? Quaternion.identity : rotation));
		}
	}
}