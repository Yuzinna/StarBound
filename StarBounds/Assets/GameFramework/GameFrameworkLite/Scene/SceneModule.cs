// Assets/GameFrameworkLite/Scene/SceneModule.cs
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameFrameworkLite
{
	/// <summary>
	/// 씬 로딩을 담당하는 간단한 모듈.
	/// </summary>
	public sealed class SceneModule : IGameFrameworkModule
	{
		private bool _isLoading; // 중복 로딩 방지 플래그

		/// <summary>
		/// 씬 이름으로 비동기 로딩 시작.
		/// </summary>
		public void LoadScene(string sceneName)
		{
			if (_isLoading)
			{
				Debug.LogWarning("[SceneModule] Already loading a scene.");
				return;
			}
			Debug.Log($"[SceneModule] LoadScene 호출: {sceneName}");

			var go = new GameObject("[SceneLoader]");
			Object.DontDestroyOnLoad(go);
			var loader = go.AddComponent<SceneLoaderHelper>();
			loader.StartCoroutine(LoadSceneCoroutine(sceneName, loader));
		}

		/// <summary>
		/// 실제 비동기 로딩 코루틴.
		/// </summary>
		private IEnumerator LoadSceneCoroutine(string sceneName, SceneLoaderHelper helper)
		{
			_isLoading = true;
			var op = SceneManager.LoadSceneAsync(sceneName);
			while (!op.isDone)
				yield return null;

			_isLoading = false;
			Object.Destroy(helper.gameObject);
		}

		public void Update(float deltaTime, float realDeltaTime) { }

		public void Shutdown() { }

		// 코루틴 실행용 임시 MonoBehaviour
		private sealed class SceneLoaderHelper : MonoBehaviour { }
	}
}
