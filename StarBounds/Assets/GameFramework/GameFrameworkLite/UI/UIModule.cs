// Assets/GameFrameworkLite/UI/UIModule.cs
using System.Collections.Generic;
using UnityEngine;

namespace GameFrameworkLite
{
	/// <summary>
	/// 간단한 UI 관리 모듈.
	/// - UIRoot(Canvas) 자동 생성
	/// - 프리팹 경로로 Open / Close
	/// </summary>
	public sealed class UIModule : IGameFrameworkModule
	{
		private readonly ResourceModule _resourceModule;

		// 이미 열려있는 UI 목록 (path → UILogic)
		private readonly Dictionary<string, UILogic> _opened = new Dictionary<string, UILogic>();

		// 최상위 Canvas
		private Canvas _rootCanvas;

		public UIModule(ResourceModule resourceModule)
		{
			_resourceModule = resourceModule;

			// UIRoot(Canvas) 생성
			var canvasGo = new GameObject("[UIRoot]");
			Object.DontDestroyOnLoad(canvasGo);
			_rootCanvas = canvasGo.AddComponent<Canvas>();
			_rootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
			canvasGo.AddComponent<UnityEngine.UI.CanvasScaler>();
			canvasGo.AddComponent<UnityEngine.UI.GraphicRaycaster>();
		}

		/// <summary>
		/// UI 프리팹을 열기.
		/// path 예: "UI/MainMenu"
		/// </summary>
		public UILogic Open(string path, object userData = null)
		{
			if (_opened.ContainsKey(path))
				return _opened[path];

			GameObject go = _resourceModule.Instantiate(path, Vector3.zero, Quaternion.identity, _rootCanvas.transform);
			if (go == null) return null;

			var logic = go.GetComponent<UILogic>();
			if (logic == null)
			{
				Debug.LogError($"[UIModule] UI prefab '{path}' has no UILogic.");
				return null;
			}

			_opened.Add(path, logic);
			logic.OnOpen(userData);
			return logic;
		}

		/// <summary>
		/// 해당 경로로 열린 UI 닫기.
		/// </summary>
		public void Close(string path)
		{
			UILogic logic;
			if (_opened.TryGetValue(path, out logic))
			{
				logic.OnClose();
				Object.Destroy(logic.gameObject);
				_opened.Remove(path);
			}
		}

		/// <summary>
		/// 열린 모든 UI 닫기.
		/// </summary>
		public void CloseAll()
		{
			foreach (var kv in _opened)
			{
				kv.Value.OnClose();
				Object.Destroy(kv.Value.gameObject);
			}
			_opened.Clear();
		}

		public void Update(float deltaTime, float realDeltaTime) { }

		public void Shutdown()
		{
			CloseAll();
			if (_rootCanvas != null)
				Object.Destroy(_rootCanvas.gameObject);
		}
	}
}
