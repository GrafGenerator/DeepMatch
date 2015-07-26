using System.Collections.Generic;

namespace DeepMatch
{
	public static class EnumeratorExtensions
	{
		public static IEnumerable<T> AsEnumerable<T>(this IEnumerator<T> enumerator)
		{
			if (enumerator == null) yield break;
			while (enumerator.MoveNext()) yield return enumerator.Current;
		}
	}
}
