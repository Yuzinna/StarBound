using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI; // 🚨 [추가됨] Layout Element를 제어하기 위해 필요합니다!

[RequireComponent(typeof(Collider2D))]
public class NPCGuideLogic : MonoBehaviour, IInteractable
{
	[Header("대화 내용 설정")]
	[TextArea(2, 5)]
	public string[] dialogues;

	[Header("UI 연결")]
	public GameObject speechBubble;
	public TextMeshProUGUI dialogueText;

	// 💡 [추가됨] 말풍선 크기 제한을 위한 변수들
	[Header("말풍선 크기 제한")]
	[Tooltip("말풍선이 가로로 커질 수 있는 최대 길이 (픽셀)")]
	public float maxBubbleWidth = 300f;
	[Tooltip("대사 텍스트에 추가한 Layout Element를 넣어주세요.")]
	public LayoutElement textLayoutElement;

	[Header("연출 설정")]
	public float typingSpeed = 0.05f;

	private int _currentIndex = 0;
	private bool _isPlayerInRange = false;
	private bool _isTyping = false;
	private bool _isWaitingToTalk = true;

	private SpriteRenderer _spriteRenderer;
	private Transform _playerTransform;
	private Coroutine _typingCoroutine;

	private void Awake()
	{
		_spriteRenderer = GetComponentInChildren<SpriteRenderer>();
	}

	private void Start()
	{
		if (speechBubble != null) speechBubble.SetActive(false);
	}

	private void Update()
	{
		if (_isPlayerInRange && _playerTransform != null && _spriteRenderer != null)
		{
			_spriteRenderer.flipX = (_playerTransform.position.x < transform.position.x);
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.CompareTag("Player"))
		{
			_isPlayerInRange = true;
			_playerTransform = collision.transform;

			_currentIndex = 0;
			_isWaitingToTalk = true;
			ShowWaitBubble();
		}
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		if (collision.CompareTag("Player"))
		{
			_isPlayerInRange = false;
			_playerTransform = null;

			if (speechBubble != null) speechBubble.SetActive(false);
			if (_typingCoroutine != null) StopCoroutine(_typingCoroutine);
		}
	}

	public void Interact(PlayerLogic player)
	{
		if (!_isPlayerInRange) return;

		if (_isWaitingToTalk)
		{
			_isWaitingToTalk = false;
			ShowDialogue();
			return;
		}

		if (_isTyping)
		{
			if (_typingCoroutine != null) StopCoroutine(_typingCoroutine);
			dialogueText.maxVisibleCharacters = 99999;
			_isTyping = false;
			return;
		}

		_currentIndex++;

		if (_currentIndex < dialogues.Length)
		{
			ShowDialogue();
		}
		else
		{
			_currentIndex = 0;
			_isWaitingToTalk = true;
			ShowWaitBubble();
		}
	}

	// =========================================================
	// 💡 [핵심 로직] 글자 길이를 측정해서 말풍선 최대 길이를 제한합니다.
	// =========================================================
	private void AdjustBubbleSize(string textContent)
	{
		dialogueText.text = textContent;

		if (textLayoutElement != null)
		{
			// 1. 잠시 레이아웃 제한을 풀고 글자 고유의 길이를 잽니다.
			textLayoutElement.enabled = false;
			dialogueText.ForceMeshUpdate();

			// 2. 만약 글자가 내가 설정한 최대 길이(maxBubbleWidth)보다 길다면?
			if (dialogueText.preferredWidth > maxBubbleWidth)
			{
				textLayoutElement.enabled = true; // 제한 장치 ON
				textLayoutElement.preferredWidth = maxBubbleWidth; // 가로 길이를 최대치로 고정!
			}
		}
	}

	private void ShowWaitBubble()
	{
		if (speechBubble != null) speechBubble.SetActive(true);
		if (_typingCoroutine != null) StopCoroutine(_typingCoroutine);

		// 🚨 글자를 띄우기 전에 크기 제한부터 확인!
		AdjustBubbleSize("...");

		dialogueText.maxVisibleCharacters = 99999;
		_isTyping = false;
	}

	private void ShowDialogue()
	{
		if (dialogues == null || dialogues.Length == 0) return;

		if (speechBubble != null) speechBubble.SetActive(true);
		if (_typingCoroutine != null) StopCoroutine(_typingCoroutine);

		_typingCoroutine = StartCoroutine(TypewriterEffect(dialogues[_currentIndex]));
	}

	private IEnumerator TypewriterEffect(string sentence)
	{
		_isTyping = true;

		// 🚨 타이핑 시작 전에 전체 문장의 길이를 측정해서 말풍선 크기를 미리 잡아줍니다.
		AdjustBubbleSize(sentence);
		dialogueText.maxVisibleCharacters = 0;

		dialogueText.ForceMeshUpdate();
		int totalChars = dialogueText.textInfo.characterCount;

		for (int i = 0; i <= totalChars; i++)
		{
			dialogueText.maxVisibleCharacters = i;
			yield return new WaitForSeconds(typingSpeed);
		}

		_isTyping = false;
	}
}