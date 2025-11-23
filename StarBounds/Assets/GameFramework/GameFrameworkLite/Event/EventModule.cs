// Assets/GameFrameworkLite/Event/EventModule.cs
using System;
using System.Collections.Generic;

namespace GameFrameworkLite
{
	/// <summary>
	/// 간단한 이벤트 버스.
	/// int eventId → 여러 리스너(Action<object>) 목록.
	/// </summary>
	public sealed class EventModule : IGameFrameworkModule
	{
		// eventId별로 핸들러 리스트를 관리하는 딕셔너리
		private readonly Dictionary<int, List<Action<object>>> _eventHandlers =
			new Dictionary<int, List<Action<object>>>();

		/// <summary>
		/// 이벤트 구독.
		/// eventId : 이벤트 번호
		/// handler : 콜백 (object userData 인자 받음)
		/// </summary>
		public void Subscribe(int eventId, Action<object> handler)
		{
			if (handler == null) return;
			List<Action<object>> list;
			if (!_eventHandlers.TryGetValue(eventId, out list))
			{
				list = new List<Action<object>>();
				_eventHandlers.Add(eventId, list);
			}
			if (!list.Contains(handler))
				list.Add(handler);
		}

		/// <summary>
		/// 이벤트 구독 해제.
		/// </summary>
		public void Unsubscribe(int eventId, Action<object> handler)
		{
			List<Action<object>> list;
			if (_eventHandlers.TryGetValue(eventId, out list))
			{
				list.Remove(handler);
				if (list.Count == 0)
					_eventHandlers.Remove(eventId);
			}
		}

		/// <summary>
		/// 이벤트 발행.
		/// 이 eventId를 구독한 모든 핸들러에게 userData 전달.
		/// </summary>
		public void Fire(int eventId, object userData = null)
		{
			List<Action<object>> list;
			if (_eventHandlers.TryGetValue(eventId, out list))
			{
				// 순회 중 리스트 변경 방지용 복사
				var temp = list.ToArray();
				foreach (var handler in temp)
				{
					handler?.Invoke(userData);
				}
			}
		}

		public void Update(float deltaTime, float realDeltaTime) { }

		public void Shutdown()
		{
			_eventHandlers.Clear();
		}
	}
}
