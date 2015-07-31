using System;
using System.Collections;
using System.Collections.Generic;

namespace DeepMatch
{
	internal class CachingEnumerator<T>: IEnumerator<T>
	{
		private readonly IEnumerator<T> _source;
		private readonly int _originIndex;
		private int _currentIndex;
		private readonly IDictionary<int, T> _cache;

		private CachingEnumerator(IEnumerator<T> source, int originIndex, IDictionary<int, T> cache)
		{
			_source = source;
			_originIndex = originIndex;
			_currentIndex = _originIndex - 1;
			_cache = cache;
		}

		public CachingEnumerator(IEnumerator<T> source)
			: this(source, 0, new Dictionary<int, T>())
		{
		}

		public CachingEnumerator(CachingEnumerator<T> other, int offset)
			: this(other._source, other._originIndex + offset, other._cache)
		{
		}

		public void Dispose()
		{
			_source.Dispose();
		}

		public bool MoveNext()
		{
			var newCurrent = _currentIndex + 1;

			if (_cache.ContainsKey(newCurrent))
			{
				_currentIndex = newCurrent;
				return true;
			}

			if (_source.MoveNext())
			{
				_cache.Add(newCurrent, _source.Current);
				_currentIndex = newCurrent;
				return true;
			}

			return false;
		}

		public void Reset()
		{
			_currentIndex = _originIndex - 1;
		}

		public T Current
		{
			get
			{
				T value;
				if(!_cache.TryGetValue(_currentIndex, out value))
					throw new InvalidOperationException();

				return value;
			}
		}

		object IEnumerator.Current
		{
			get { return Current; }
		}
	}

	internal static class CachingEnumeratorExtensions
	{
		public static CachingEnumerator<T> Shift<T>(this CachingEnumerator<T> enumerator, int offset)
		{
			return new CachingEnumerator<T>(enumerator, offset);
		}

		public static CachingEnumerator<T> Fork<T>(this CachingEnumerator<T> enumerator)
		{
			return new CachingEnumerator<T>(enumerator, 0);
		}
	}
}
