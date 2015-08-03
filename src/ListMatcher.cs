using System;
using System.Collections.Generic;
using System.Linq;

namespace DeepMatch
{
	public delegate bool MatchFunc<in T> (T head, TailFunc<T> tail);
	public delegate bool TailFunc<out T> (Func<T, TailFunc<T>, bool> predicate);
	public delegate TR ResultFunc<in TI, out TR>(TI[] heads, IEnumerator<TI> tail);

	public class ListMatcher<TI, TR>
	{
		private readonly List<Tuple<MatchFunc<TI>, ResultFunc<TI, TR>>> _blocks;
		private Func<TR> _emptyMatchBlock;
 
		public ListMatcher()
		{
			_blocks = new List<Tuple<MatchFunc<TI>, ResultFunc<TI, TR>>>();
		}

		public ListMatcher<TI, TR> When(MatchFunc<TI> predicate, ResultFunc<TI, TR> makeResult)
		{
			if (predicate == null)
				throw new ArgumentNullException("predicate");

			_blocks.Add(new Tuple<MatchFunc<TI>, ResultFunc<TI, TR>>(predicate, makeResult));
			return this;
		}

		public ListMatcher<TI, TR> WhenEmpty(Func<TR> makeResult)
		{
			_emptyMatchBlock = makeResult;
			return this;
		}

		public TR Run(IEnumerator<TI> sourceEnumerator)
		{
			var enumerator = new CachingEnumerator<TI>(sourceEnumerator);
			var marker = Split(enumerator);

			if (marker.Item1)
			{
				var heads = new List<TI> {marker.Item2};
				var tailEnumerator = new[] {marker.Item3};

				var result = RunBlocks(marker.Item2, marker.Item3, heads, tailEnumerator);
				if (result != null) return result(heads.ToArray(), tailEnumerator[0].Clone());
			}
			else
			{
				if (_emptyMatchBlock != null)
					return _emptyMatchBlock();
			}

			throw new MatchException();
		}



		private ResultFunc<TI, TR> RunBlocks(TI first, CachingEnumerator<TI> tail, List<TI> heads, CachingEnumerator<TI>[] tailEnumerator)
		{
			return
				(from block in _blocks
					where block.Item1(first, MatchFunc(tail.Clone(), heads, tailEnumerator))
					select block.Item2)
					.FirstOrDefault();
		}

		private TailFunc<TI> MatchFunc(CachingEnumerator<TI> enumerator, List<TI> heads, CachingEnumerator<TI>[] tailEnumerator)
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
			if (enumerator == null || !enumerator.MoveNext())
				return new Tuple<bool, TI, CachingEnumerator<TI>>(false, default(TI), null);
			return new Tuple<bool, TI, CachingEnumerator<TI>>(true, enumerator.Current, enumerator.Shift(1));
		}
	}
}
