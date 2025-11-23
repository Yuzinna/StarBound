// Assets/GameFrameworkLite/ObjectPool/ObjectPool.cs
using System;
using System.Collections.Generic;

namespace GameFrameworkLite
{
	/// <summary>
	/// 일반 C# 객체용 제네릭 풀.
	/// new 비용이 큰 객체를 재사용할 때 사용.
	/// </summary>
	public sealed class ObjectPool<T> where T : class
	{
		// 풀 내부 스택
		private readonly Stack<T> _stack = new Stack<T>();

		// 인스턴스 생성 함수 (필수)
		private readonly Func<T> _factory;

		public ObjectPool(Func<T> factory, int initialCapacity = 0)
		{
			_factory = factory ?? throw new ArgumentNullException(nameof(factory));

			// 초기 용량만큼 미리 생성해서 넣어두기
			for (int i = 0; i < initialCapacity; i++)
				_stack.Push(_factory());
		}

		/// <summary>
		/// 풀에서 객체 하나 꺼내오기.
		/// 비어있으면 새로 생성.
		/// </summary>
		public T Get()
		{
			return _stack.Count > 0 ? _stack.Pop() : _factory();
		}

		/// <summary>
		/// 사용이 끝난 객체를 다시 풀에 반환.
		/// </summary>
		public void Release(T obj)
		{
			if (obj == null) return;
			_stack.Push(obj);
		}

		/// <summary>
		/// 풀 내부 모두 비우기.
		/// </summary>
		public void Clear()
		{
			_stack.Clear();
		}
	}
}
