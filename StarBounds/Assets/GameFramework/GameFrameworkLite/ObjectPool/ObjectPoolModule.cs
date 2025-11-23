// Assets/GameFrameworkLite/ObjectPool/ObjectPoolModule.cs
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrameworkLite
{
	/// <summary>
	/// GameObject 풀과 일반 객체 풀을 관리하는 모듈.
	/// 현재는 GameObject 풀에 집중.
	/// </summary>
	public sealed class ObjectPoolModule : IGameFrameworkModule
	{
		// key(보통 프리팹 path) → GameObject 풀
		private readonly Dictionary<string, ObjectPool<GameObject>> _goPools =
			new Dictionary<string, ObjectPool<GameObject>>();

		/// <summary>
		/// key에 해당하는 풀에서 GameObject 하나 꺼내온다.
		/// 풀이 없으면 factory를 이용해 새로 풀을 만든다.
		/// </summary>
		public GameObject GetFromPool(string key, Func<GameObject> factory)
		{
			if (factory == null) return null;

			ObjectPool<GameObject> pool;
			if (!_goPools.TryGetValue(key, out pool))
			{
				pool = new ObjectPool<GameObject>(factory);
				_goPools.Add(key, pool);
			}

			var obj = pool.Get();
			if (obj != null)
				obj.SetActive(true);
			return obj;
		}

		/// <summary>
		/// 사용이 끝난 GameObject를 풀에 반환.
		/// 해당 key의 풀이 없으면 자동으로 생성해서 보관.
		/// </summary>
		public void ReleaseToPool(string key, GameObject instance)
		{
			if (instance == null) return;

			ObjectPool<GameObject> pool;
			if (!_goPools.TryGetValue(key, out pool))
			{
				// 풀이 없으면 이 인스턴스를 기준으로 새 풀 생성
				pool = new ObjectPool<GameObject>(() => instance);
				_goPools.Add(key, pool);
			}

			instance.SetActive(false);
			pool.Release(instance);
		}

		/// <summary>
		/// 모든 풀 정보 삭제.
		/// (실제 GameObject 파괴 여부는 사용 패턴에 따라 확장 가능)
		/// </summary>
		public void ClearAll()
		{
			_goPools.Clear();
		}

		public void Update(float deltaTime, float realDeltaTime) { }

		public void Shutdown()
		{
			ClearAll();
		}
	}
}
