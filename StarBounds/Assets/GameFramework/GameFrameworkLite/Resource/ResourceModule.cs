// Assets/GameFrameworkLite/Resource/ResourceModule.cs
using System.Collections.Generic;
using UnityEngine;

namespace GameFrameworkLite
{
	/// <summary>
	/// Resources.Load 기반 간단 리소스 매니저.
	/// - 캐싱 지원
	/// - 프리팹 Instantiate 헬퍼 제공
	/// </summary>
	public sealed class ResourceModule : IGameFrameworkModule
	{
		// path → 로드된 Object 캐시
		private readonly Dictionary<string, Object> _cache =
			new Dictionary<string, Object>();

		/// <summary>
		/// Resources.Load를 사용해서 T 타입 에셋을 로드.
		/// path 예: "Entities/Player"
		/// </summary>
		public T Load<T>(string path) where T : Object
		{
			if (string.IsNullOrEmpty(path))
			{
				Debug.LogError("[ResourceModule] Path is null or empty.");
				return null;
			}

			// 캐시에 있는 경우 바로 반환
			Object cached;
			if (_cache.TryGetValue(path, out cached))
				return cached as T;

			// 실제 Resources에서 로드
			var asset = Resources.Load<T>(path);
			if (asset == null)
			{
				Debug.LogError($"[ResourceModule] Failed to load asset at '{path}'.");
				return null;
			}

			_cache[path] = asset;
			return asset;
		}

		/// <summary>
		/// GameObject 프리팹을 로드 후 곧바로 Instantiate.
		/// parent를 지정하면 Hierarchy 정리에 유용.
		/// </summary>
		public GameObject Instantiate(string path, Vector3 position, Quaternion rotation, Transform parent = null)
		{
			var prefab = Load<GameObject>(path);
			if (prefab == null) return null;
			return Object.Instantiate(prefab, position, rotation, parent);
		}

		/// <summary>
		/// 캐시를 비우고 Unity에게 사용하지 않는 에셋 언로드 요청.
		/// </summary>
		public void UnloadUnused()
		{
			_cache.Clear();
			Resources.UnloadUnusedAssets();
		}

		public void Update(float deltaTime, float realDeltaTime) { }

		public void Shutdown()
		{
			UnloadUnused();
		}
	}
}
