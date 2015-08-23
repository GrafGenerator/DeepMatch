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
		/// <summary>
		/// Tests for matching more than one sequential elements
		/// </summary>

		[Test]
		public void DeepMatch_ManyMatchBlocks_MatchingBlockFirst_Test()
		{
			var result = new ListMatcher<int, int>()
				.When((i, _) => i == 1 && _((i1, __) => i1 == 2), (_, __) => 1)
				.When((i, _) => i == 0 && _((i1, __) => i1 == 1), (_, __) => 0)
				.Run(_sequence1To10.GetEnumerator());

			Assert.That(result, Is.EqualTo(1));
		}

		[Test]
		public void DeepMatch_ManyMatchBlocks_MatchingBlockLast_Test()
		{
			var result = new ListMatcher<int, int>()
				.When((i, _) => i == 0 && _((i1, __) => i1 == 1), (_, __) => 0)
				.When((i, _) => i == 1 && _((i1, __) => i1 == 2), (_, __) => 1)
				.Run(_sequence1To10.GetEnumerator());

			Assert.That(result, Is.EqualTo(1));
		}

		[Test]
		[TestCaseSource("DeepMatchTestCases")]
		public void DeepMatchTest(Tuple<MatchFunc<int>, IEnumerable<int>, IEnumerable<int>> input)
		{
			var heads = Enumerable.Empty<int>();
			var tail = Enumerable.Empty<int>();

			var result = new ListMatcher<int, int>()
				.When(input.Item1, (h, t) =>
				{
					heads = h;
					tail = t.AsEnumerable();
					return 0;
				})
				.Run(_sequence1To50.GetEnumerator());

			Assert.That(heads, Is.EqualTo(input.Item2));
			Assert.That(tail, Is.EqualTo(input.Item3));
		}

		public IEnumerable<Tuple<MatchFunc<int>, IEnumerable<int>, IEnumerable<int>>> DeepMatchTestCases
		{
			get
			{
				yield return GenerateDeepMatchTestCase<int>(
					1, 
					(i, t) => i == 1
				);

				yield return GenerateDeepMatchTestCase<int>(
					5,
					(i1, t1) => 
						i1 == 1 && t1((i2, t2) => 
							i2 == 2 && t2((i3, t3) => 
								i3 == 3 && t3((i4, t4) => 
									i4 == 4 && t4((i5, _) => 
										i5 == 5))))
				);
			}
		}

		private Tuple<MatchFunc<T>, IEnumerable<int>, IEnumerable<int>> GenerateDeepMatchTestCase<T>(int count,
			MatchFunc<T> matchFunc)
		{
			return new Tuple<MatchFunc<T>, IEnumerable<int>, IEnumerable<int>>(
					matchFunc,
					Enumerable.Range(1, count),
					_sequence1To50.Skip(count)
				);
		}

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

		[Test]
		public void ManyMatchBlocks_MatchingBlockFirst_Test()
		{
			var result = new ListMatcher<int, int>()
				.When((i, _) => i == 1, (_, __) => 1)
				.When((i, _) => i == 0, (_, __) => 0)
				.When((i, _) => i == 2, (_, __) => 2)
				.When((i, _) => i == 3, (_, __) => 3)
				.Run(_sequence1To10.GetEnumerator());

			Assert.That(result, Is.EqualTo(1));
		}

		[Test]
		public void ManyMatchBlocks_MatchingBlockLast_Test()
		{
			var result = new ListMatcher<int, int>()
				.When((i, _) => i == 3, (_, __) => 3)
				.When((i, _) => i == 2, (_, __) => 2)
				.When((i, _) => i == 1, (_, __) => 1)
				.Run(_sequence1To10.GetEnumerator());

			Assert.That(result, Is.EqualTo(1));
		}

		#endregion


		#region Heads and tails tests

		[Test]
		public void TwoCloseMatchBlocks_HeadsShouldNotBePolluted_Test()
		{
			var result = new ListMatcher<int, int[]>()
				.When((i, t1) => i == 1 && t1((j, t2) => j == 2 && t2((k, _) => k == 4)), (heads, __) => heads)
				.When((i, t1) => i == 1 && t1((j, t2) => j == 2 && t2((k, _) => k == 3)), (heads, __) => heads)
				.Run(_sequence1To10.GetEnumerator());

			var expected = new[]{1, 2, 3};

			Assert.That(result, Is.EqualTo(expected));
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
