using System.Collections.Generic;
using DeepMatch;
using NUnit.Framework;

namespace Tests
{
	[TestFixture]
	public class GenericFixture
	{
		private IEnumerable<int> _sequence0To9;
		
		[SetUp]
		public void SetUp()
		{
			_sequence0To9 = new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
		}

		[TestCase(0,1,0)]
		[TestCase(1,2,1)]
		[TestCase(2,3,2)]
		[TestCase(3,4,3)]
		[TestCase(4,5,4)]
		[TestCase(5,6,5)]
		[TestCase(6,7,6)]
		[TestCase(7,8,7)]
		[TestCase(8,9,8)]
		public void SimpleMatchTest(int a, int b, int expected)
		{

			var match = new ListMatcher<int, int>()
				.When((i, t) => i == a && t((j, _) => j == b), (_, __) => 0)
				.Run(_sequence0To9.GetEnumerator());

			Assert.That(match, Is.EqualTo(expected));
		}

		[TestCase(9, 10, 0)]
		[TestCase(5, 5, 0)]
		[TestCase(5, 7, 0)]
		[TestCase(1, 0, 0)]
		[ExpectedException(typeof(MatchException))]
		public void MoMatchTest(int a, int b, int expected)
		{
			var match = new ListMatcher<int, int>()
				.When((i, t) => i == a && t((j, _) => j == b), (_, __) => 0)
				.Run(_sequence0To9.GetEnumerator());

			Assert.That(match, Is.EqualTo(expected));
		}

		[Test]
		public void RecursionTest()
		{
			ListMatcher<int, int> matcher = null;
			matcher = new ListMatcher<int, int>()
				.When((_, __) => true, (head, tail) => head[0] + matcher.Run(tail.GetEnumerator()));
				
			var result = matcher.Run(((IEnumerable<int>)new []{0,1}).GetEnumerator());

			Assert.That(result, Is.EqualTo(1));
		}
	}
}
