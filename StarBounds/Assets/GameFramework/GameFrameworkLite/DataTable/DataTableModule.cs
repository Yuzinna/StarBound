// Assets/GameFrameworkLite/DataTable/DataTableModule.cs
using System.Collections.Generic;
using UnityEngine;

namespace GameFrameworkLite
{
	/// <summary>
	/// 간단한 TextAsset 기반 데이터 테이블 모듈.
	/// JSON, CSV 등은 TextAsset.text를 파싱해서 사용.
	/// </summary>
	public sealed class DataTableModule : IGameFrameworkModule
	{
		private readonly ResourceModule _resourceModule;

		// path → TextAsset
		private readonly Dictionary<string, TextAsset> _tables =
			new Dictionary<string, TextAsset>();

		public DataTableModule(ResourceModule resourceModule)
		{
			_resourceModule = resourceModule;
		}

		/// <summary>
		/// Raw TextAsset 로딩. 이후 text 파싱은 사용자가 구현.
		/// </summary>
		public TextAsset LoadRawTable(string path)
		{
			TextAsset asset;
			if (_tables.TryGetValue(path, out asset))
				return asset;

			asset = _resourceModule.Load<TextAsset>(path);
			if (asset != null)
				_tables.Add(path, asset);
			return asset;
		}

		public void Clear()
		{
			_tables.Clear();
		}

		public void Update(float deltaTime, float realDeltaTime) { }

		public void Shutdown()
		{
			Clear();
		}
	}
}
