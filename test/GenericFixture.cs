using System;
using System.Collections.Generic;
using System.Linq;
using DeepMatch;
using NUnit.Framework;

namespace Tests
{
	[TestFixture]
	public class GenericFixture
	{
		private readonly IEnumerable<int> _sequence1To50 = Enumerable.Range(1, 50);
		private readonly IEnumerable<int> _sequence1To10 = Enumerable.Range(1, 10);
		private readonly IEnumerable<int> _sequence1To2 = Enumerable.Range(1, 2);
		private readonly IEnumerable<int> _sequence1To1 = Enumerable.Range(1, 1);
		private readonly IEnumerable<int> _sequenceEmpty = Enumerable.Empty<int>();

		private ListMatcher<int, int> _sumRecursiveMatcher;
		private ListMatcher<int, int> _sumRecursiveMatcherNoEmpty;

		[SetUp]
		public void SetUp()
		{
			ListMatcher<int, int> matcher = null;
			matcher = new ListMatcher<int, int>()
				.When((_, __) => true, (head, tail) => head[0] + matcher.Run(tail))
				.WhenEmpty(() => 0);

			_sumRecursiveMatcher = matcher;


			ListMatcher<int, int> matcher2 = null;
			matcher2 = new ListMatcher<int, int>()
				.When((_, __) => true, (head, tail) => head[0] + matcher2.Run(tail));

			_sumRecursiveMatcherNoEmpty = matcher2;
		}


		#region Deep matching



		#endregion


		#region Usual cases

		[Test]
		public void SimpleMatchTest()
		{
			var result = new ListMatcher<int, int>()
				.When((i, _) => i == 1 , (_, __) => 42)
				.Run(_sequence1To10.GetEnumerator());

			Assert.That(result, Is.EqualTo(42));
		}

		[Test]
		public void EmptySequenceMatchTest()
		{
			var result = new ListMatcher<int, int>()
				.When((i, _) => i == 0, (_, __) => 42)
				.WhenEmpty(() => 11)
				.Run(_sequenceEmpty.GetEnumerator());

			Assert.That(result, Is.EqualTo(11));
		}

		#endregion


		#region No match tests

		[Test]
		[ExpectedException(typeof(MatchException))]
		public void MoMatchTest_Hardcore()
		{
			new ListMatcher<int, int>()
				.When((i, t) => false, (_, __) => 0)
				.Run(_sequence1To10.GetEnumerator());
		}

		[Test]
		[ExpectedException(typeof(MatchException))]
		public void MoMatchTest_NoEmptyStub_NonEmptySequence()
		{
			_sumRecursiveMatcherNoEmpty.Run(_sequence1To10.GetEnumerator());
		}

		[Test]
		[ExpectedException(typeof(MatchException))]
		public void MoMatchTest_NoEmptyStub_EmptySequence()
		{
			_sumRecursiveMatcherNoEmpty.Run(_sequenceEmpty.GetEnumerator());
		}

		#endregion



		#region Recursion tests

		[Test]
		[TestCaseSource("RecursionTestsSum")]
		public void RecursionTest_Sum(Tuple<IEnumerable<int>, int> input)
		{
			var result = _sumRecursiveMatcher.Run(input.Item1.GetEnumerator());

			Assert.That(result, Is.EqualTo(input.Item2));
		}

		private IEnumerable<Tuple<IEnumerable<int>, int>> RecursionTestsSum
		{
			get
			{
				yield return new Tuple<IEnumerable<int>, int>(_sequence1To50, _sequence1To50.Sum());
				yield return new Tuple<IEnumerable<int>, int>(_sequence1To10, _sequence1To10.Sum());
				yield return new Tuple<IEnumerable<int>, int>(_sequence1To2, _sequence1To2.Sum());
				yield return new Tuple<IEnumerable<int>, int>(_sequence1To1, _sequence1To1.Sum());
				yield return new Tuple<IEnumerable<int>, int>(_sequenceEmpty, _sequenceEmpty.Sum());
			}
		}

		#endregion

	}
}
