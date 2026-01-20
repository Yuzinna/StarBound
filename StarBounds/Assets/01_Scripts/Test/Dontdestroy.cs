using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class Dontdestroy : MonoBehaviour
{
	private static Dontdestroy Instance;
	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;
		DontDestroyOnLoad(gameObject);
		
	}
	
}
