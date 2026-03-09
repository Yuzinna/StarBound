using UnityEngine;
using UnityEngine.SceneManagement;

public class ElectricWall : MonoBehaviour
{
    public GameObject electricBeamsGroup;
    public ParticleSystem electricParticle;

	[Header("사운드")]
	public AudioClip zapSfx;

	private int _blockingCubeCount = 0;
	private bool _isElectricOn = true;

	private void Start()
	{
		
	}
	private void OnTriggerEnter2D(Collider2D collision)
	{
		if(collision.CompareTag("Cube"))
		{
			_blockingCubeCount++;
			UpdatElectricState();
			
		}
	}
	private void OnTriggerStay2D(Collider2D collision)
	{
		if(_isElectricOn && collision.CompareTag("Player"))
		{
			//TODO
			KillAndRespawnPlayer(collision.gameObject);
		}
	}
	private void OnTriggerExit2D(Collider2D collision)
	{
		if(collision.CompareTag("Cube"))
		{
			_blockingCubeCount--;
			if (_blockingCubeCount < 0)
				{ _blockingCubeCount = 0; }
			UpdatElectricState();
		}
	}
	private void UpdatElectricState()
	{
		_isElectricOn = (_blockingCubeCount == 0);

		if(electricBeamsGroup!= null)
		{
			electricBeamsGroup.SetActive(_isElectricOn);
		}
		if(electricParticle != null)
		{
			if (_isElectricOn && !electricParticle.isPlaying)
				electricParticle.Play();
			else if (!_isElectricOn && electricParticle.isPlaying)
				electricParticle.Stop();
		}
	}
	private void KillAndRespawnPlayer(GameObject player)
	{
		// 감전 사운드 재생
		if (zapSfx != null && SfxManager.Instance != null)
		{
			SfxManager.Instance.PlaySfx(zapSfx);
		}

		// 플레이어 몸에서 PlayerDeath 스크립트를 찾아서 Die() 실행!
		PlayerDeath playerDeath = player.GetComponent<PlayerDeath>();
		if (playerDeath != null)
		{
			playerDeath.Die(); // 마리오 데스 연출 시작!
		}
		else
		{
			// 혹시 스크립트가 없다면 예전처럼 즉시 재시작 (안전장치)
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		}
	}
}
