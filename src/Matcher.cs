using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepMatch
{
	public delegate bool TailFunc<T> (T item, TailFunc<T> tail);

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

		public TR Run(IEnumerable<TI> value)
		{
			var first = value.First();
			var tail = value.Skip(1);

			foreach (var block in _blocks)
			{
				if(block.Item1(first, 
			}
		}

		private TailFunc<TI> MatchFunc(TI i, TailFunc<TI> tail)
		{
			return (i, t) =>
			{

			};
		}
	}
}
