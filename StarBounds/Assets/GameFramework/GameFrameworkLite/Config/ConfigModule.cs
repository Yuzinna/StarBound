// Assets/GameFrameworkLite/Config/ConfigModule.cs
using System.Collections.Generic;
using UnityEngine;

namespace GameFrameworkLite
{
	/// <summary>
	/// 간단한 키-값 설정 모듈.
	/// 옵션, 난이도, 언어 설정 등 저장에 사용 가능.
	/// </summary>
	public sealed class ConfigModule : IGameFrameworkModule
	{
		private readonly ResourceModule _resourceModule;

		// key → string 값
		private readonly Dictionary<string, string> _values =
			new Dictionary<string, string>();

		public ConfigModule(ResourceModule resourceModule)
		{
			_resourceModule = resourceModule;
		}

		/// <summary>
		/// 설정 값 저장.
		/// </summary>
		public void Set(string key, string value)
		{
			_values[key] = value;
		}

		/// <summary>
		/// 설정 값 가져오기. 없으면 defaultValue 반환.
		/// </summary>
		public string Get(string key, string defaultValue = "")
		{
			string v;
			return _values.TryGetValue(key, out v) ? v : defaultValue;
		}

		public void Update(float deltaTime, float realDeltaTime) { }

		public void Shutdown()
		{
			_values.Clear();
		}
	}
}
