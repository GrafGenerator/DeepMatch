using System;
using System.Collections.Generic;
using System.Linq;

namespace DeepMatch
{
	public delegate bool TailFunc<in T> (T head, TailFunc2<T> tail);
	public delegate bool TailFunc2<out T> (Func<T, TailFunc2<T>, bool> predicate);
	public delegate TR ActionFunc<in TI, out TR>(TI[] heads, IEnumerable<TI> tail);

	public class Matcher<TI, TR>
	{
		private readonly List<Tuple<TailFunc<TI>, ActionFunc<TI, TR>>> _blocks;

		public Matcher()
		{
			_blocks = new List<Tuple<TailFunc<TI>, ActionFunc<TI, TR>>>();
		}

		private Matcher(Matcher<TI, TR> other)
		{
			_blocks = other._blocks;
		}

		public Matcher<TI, TR> When(TailFunc<TI> predicate, ActionFunc<TI, TR> makeResult)
		{
			if (predicate == null)
				throw new ArgumentNullException("predicate");

			_blocks.Add(new Tuple<TailFunc<TI>, ActionFunc<TI, TR>>(predicate, makeResult));
			return new Matcher<TI, TR>(this);
		}

		public TR Run(IEnumerator<TI> sourceEnumerator, TR seed = default(TR))
		{
			var enumerator = new CachingEnumerator<TI>(sourceEnumerator);
			var marker = Split(enumerator);

			if (!marker.Item1) return seed;

			var heads = new List<TI> {marker.Item2};
			var tailEnumerator = new[] {marker.Item3};

			var result = RunBlocks(marker.Item2, marker.Item3, heads, tailEnumerator);
			if (result != null) return result(heads.ToArray(), EnumerateTail(tailEnumerator[0]));

			throw new MatchException();
		}



		private ActionFunc<TI, TR> RunBlocks(TI first, CachingEnumerator<TI> tail, List<TI> heads, CachingEnumerator<TI>[] tailEnumerator)
		{
			return (from block in _blocks where block.Item1(first, MatchFunc(tail, heads, tailEnumerator)) select block.Item2).FirstOrDefault();
		}

		private TailFunc2<TI> MatchFunc(CachingEnumerator<TI> enumerator, List<TI> heads, CachingEnumerator<TI>[] tailEnumerator)
		{
			var marker = Split(enumerator);
			if (!marker.Item1) return _ => false;

			return tailFn =>
			{
				// add head and new tail enumerator only in case tail func invoked
				heads.Add(marker.Item2);
				tailEnumerator[0] = marker.Item3;

				return tailFn(marker.Item2, MatchFunc(marker.Item3, heads, tailEnumerator));
			};
		}



		private Tuple<bool, TI, CachingEnumerator<TI>> Split(CachingEnumerator<TI> enumerator)
		{
			if (enumerator == null || !enumerator.MoveNext()) return new Tuple<bool, TI, CachingEnumerator<TI>>(false, default(TI), null);
			return new Tuple<bool, TI, CachingEnumerator<TI>>(true, enumerator.Current, new CachingEnumerator<TI>(enumerator, 1));
		}

		IEnumerable<TI> EnumerateTail(IEnumerator<TI> en)
		{
			if(en == null) yield break;
			while (en.MoveNext()) yield return en.Current;
		}
	}
}
