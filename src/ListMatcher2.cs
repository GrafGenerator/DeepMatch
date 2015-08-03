using System;
using System.Collections.Generic;
using System.Linq;

namespace DeepMatch
{
	public delegate bool ChainFunc<in T>(T head, IEnumerator<T> tail);
	public delegate bool TailFunc2<out T> (Func<T, TailFunc2<T>, bool> predicate);
	public delegate TR ResultFunc2<in TI, out TR>(TI[] heads, IEnumerator<TI> tail);

	public struct MResult<T>
	{
		public readonly bool Success;
		public readonly ChainFunc<T> Next;

		public MResult(bool success, ChainFunc<T> next)
		{
			Success = success;
			Next = next;
		}
	}

	public class ListMatcher2<TI, TR>
	{
		private readonly List<Tuple<ChainFunc<TI>, ResultFunc2<TI, TR>>> _blocks;
		private Func<TR> _emptyMatchBlock;
 
		public ListMatcher2()
		{
			_blocks = new List<Tuple<ChainFunc<TI>, ResultFunc2<TI, TR>>>();
		}

		public ListMatcher2<TI, TR> When(ChainFunc<TI> predicate, ResultFunc2<TI, TR> makeResult)
		{
			if (predicate == null)
				throw new ArgumentNullException("predicate");

			_blocks.Add(new Tuple<ChainFunc<TI>, ResultFunc2<TI, TR>>(predicate, makeResult));
			return this;
		}

		public ListMatcher2<TI, TR> WhenEmpty(Func<TR> makeResult)
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
				var tailEnumerator = marker.Item3;

				var result = (from block in _blocks
					where block.Item1(marker.Item2, tailEnumerator)
					select block.Item2)
					.FirstOrDefault();

				if (result != null) return result(heads.ToArray(), tailEnumerator.Clone());
			}
			else
			{
				if (_emptyMatchBlock != null)
					return _emptyMatchBlock();
			}

			throw new MatchException();
		}


		private TailFunc2<TI> MatchFunc2(CachingEnumerator<TI> enumerator, List<TI> heads, CachingEnumerator<TI>[] tailEnumerator)
		{
			var marker = Split(enumerator);
			if (!marker.Item1) return _ => false;

			return tailFn =>
			{
				// add head and new tail enumerator only in case tail func invoked
				heads.Add(marker.Item2);
				tailEnumerator[0] = marker.Item3;

				return tailFn(marker.Item2, MatchFunc2(marker.Item3, heads, tailEnumerator));
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
