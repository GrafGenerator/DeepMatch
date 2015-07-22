using System;
using System.Collections.Generic;
using System.Linq;

namespace DeepMatch
{
	public delegate bool TailFunc<in T> (T item, TailFunc2<T> tail);
	public delegate bool TailFunc2<out T> (Func<T, TailFunc2<T>, bool> predicate);

	public class Matcher<TI, TR>
	{
		private readonly List<Tuple<TailFunc<TI>, Func<TI, TR>>> _blocks;

		public Matcher()
		{
			_blocks = new List<Tuple<TailFunc<TI>, Func<TI, TR>>>();
		}

		private Matcher(Matcher<TI, TR> other)
		{
			_blocks = other._blocks;
		}

		public Matcher<TI, TR> When(TailFunc<TI> predicate, Func<TI, TR> makeResult)
		{
			if (predicate == null)
				throw new ArgumentNullException("predicate");

			_blocks.Add(new Tuple<TailFunc<TI>, Func<TI, TR>>(predicate, makeResult));
			return new Matcher<TI, TR>(this);
		}

		public TR Run(IEnumerable<TI> sequence)
		{
			var enumerator = sequence.GetEnumerator();
			var marker = Split(enumerator);

			while (marker.Item1)
			{
				var result = RunBlocks(marker.Item2, marker.Item3);
				if (result != null) return result(marker.Item2);

				marker = Split(marker.Item3);
			}

			throw new MatchException();
		}

		private Tuple<bool, TI, IEnumerator<TI>> Split(IEnumerator<TI> enumerator)
		{
			if(!enumerator.MoveNext()) return new Tuple<bool, TI, IEnumerator<TI>>(false, default(TI), null);
			return new Tuple<bool, TI, IEnumerator<TI>>(true, enumerator.Current, enumerator);
		}

		private Func<TI, TR> RunBlocks(TI first, IEnumerator<TI> tail)
		{
			return (from block in _blocks where block.Item1(first, MatchFunc(tail)) select block.Item2).FirstOrDefault();
		}

		private TailFunc2<TI> MatchFunc(IEnumerator<TI> tail)
		{
			var marker = Split(tail);
			if (!marker.Item1) return predicate => false;
			return predicate => predicate(marker.Item2, MatchFunc(marker.Item3));
		}

		IEnumerable<TI> EnumerateTail(IEnumerator<TI> en)
		{
			while (en.MoveNext()) yield return en.Current;
		}
	}
}
