using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MirrorFloorButton : MonoBehaviour
{
	[Header("연결할 거울")]
	[Tooltip("이 발판을 밟았을 때 돌아갈 거울 오브젝트를 넣어주세요.")]
	public MirrorLogic targetMirror;

	[Header("작동 설정")]
	[Tooltip("이 발판을 밟을 수 있는 대상의 레이어 (예: Player, PushableCube 등)")]
	public LayerMask pressableLayers;

	[Header("비주얼 & 사운드")]
	public SpriteRenderer buttonRenderer;
	public Sprite unpressedSprite; // 안 눌렸을 때 이미지 (볼록)
	public Sprite pressedSprite;   // 눌렸을 때 이미지 (납작)
	public AudioClip pressSfx;     // 밟았을 때 딸깍! 소리

	// 현재 발판 위에 물체가 몇 개 올라와 있는지 (안전장치)
	private int _objectsOnButton = 0;

	private void OnTriggerEnter2D(Collider2D other)
	{
		// 들어온 오브젝트가 밟을 수 있는 레이어인지 확인
		if (((1 << other.gameObject.layer) & pressableLayers) != 0)
		{
			_objectsOnButton++;

			// 처음 밟히는 순간에만 작동!
			if (_objectsOnButton == 1)
			{
				PressButton();
			}
		}
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		if (((1 << other.gameObject.layer) & pressableLayers) != 0)
		{
			_objectsOnButton--;

			// 물체가 모두 내려가면 발판 원상복구
			if (_objectsOnButton <= 0)
			{
				_objectsOnButton = 0;
				ReleaseButton();
			}
		}
	}

	private void PressButton()
	{
		// 1. 비주얼 & 사운드 변경 (딸깍!)
		if (buttonRenderer != null && pressedSprite != null) buttonRenderer.sprite = pressedSprite;
		if (SfxManager.Instance != null && pressSfx != null) SfxManager.Instance.PlaySfx(pressSfx, 1f, 0f);

		// 2. ⭐️ 연결된 거울을 90도 회전시킵니다!
		if (targetMirror != null)
		{
			targetMirror.RotateMirror90Degrees();
		}
	}

	private void ReleaseButton()
	{
		// 발판이 다시 튀어오르는 비주얼 (거울은 다시 돌아가지 않고 그대로 둡니다!)
		if (buttonRenderer != null && unpressedSprite != null) buttonRenderer.sprite = unpressedSprite;
	}
}