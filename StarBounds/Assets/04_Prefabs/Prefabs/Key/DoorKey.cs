using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DoorKey : MonoBehaviour // 이름을 DoorKey로 변경!
{
	[Header("사운드 (선택)")]
	public AudioClip getSfx;
	[Range(0f, 1f)] public float volume = 1f;

	private void OnTriggerEnter2D(Collider2D col)
	{
		if (col.CompareTag("Player"))
		{
 			if (getSfx != null && SfxManager.Instance != null)
			{
				SfxManager.Instance.PlaySfx(getSfx, volume);
			}

			ClearDoor door = FindAnyObjectByType<ClearDoor>();
			if (door != null)
			{
				door.AddKey();
			}

			Destroy(gameObject);
		}
	}
}