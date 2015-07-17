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

		[TestCase(5,6,5)]
		[TestCase(0,1,0)]
		[TestCase(8,9,8)]
		public void SimpleMatchTest(int a, int b, int expected)
		{

			var match = new Matcher<int, int>()
				.When((i, t) => i == a && t((j, _) => j == b), r => r)
				.Run(_sequence0To9);

			Assert.That(match, Is.EqualTo(expected));
		}

		[TestCase(9, 10, 0)]
		[TestCase(5, 5, 0)]
		[TestCase(5, 7, 0)]
		[TestCase(1, 0, 0)]
		[ExpectedException(typeof(MatchException))]
		public void MoMatchTest(int a, int b, int expected)
		{
			var match = new Matcher<int, int>()
				.When((i, t) => i == a && t((j, _) => j == b), r => r)
				.Run(_sequence0To9);

			Assert.That(match, Is.EqualTo(expected));
		}
	}
}
