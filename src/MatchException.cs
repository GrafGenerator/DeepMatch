using System;

namespace DeepMatch
{
	public class MatchException: Exception
	{
		public MatchException()
			:base("No match found")
		{
		}
	}
}
