using DPackRx.Services;

using NUnit.Framework;

namespace DPackRx.Tests.Services
{
	/// <summary>
	/// WildcardMatch tests.
	/// </summary>
	[TestFixture]
	public class WildcardMatchTests
	{
		#region Private Methods

		/// <summary>
		/// Returns test service instance.
		/// </summary>
		private IWildcardMatch GetService()
		{
			return new WildcardMatch();
		}

		#endregion

		#region Tests

		[TestCase("", false)]
		[TestCase("?", true)]
		[TestCase("Just a test", false)]
		[TestCase("Just ? test", true)]
		[TestCase("Just * test", true)]
		[TestCase("Just?", true)]
		[TestCase("Just*", true)]
		[TestCase("Just ? test?", true)]
		[TestCase("Just * test*", true)]
		[TestCase("Just ? test*", true)]
		public void WildcardPresent(string filter, bool expectedResult)
		{
			var service = GetService();
			service.Initialize(filter);

			Assert.That(expectedResult, Is.EqualTo(service.WildcardPresent));
		}

		[TestCase("", "Just a test", false)]
		[TestCase(null, "Just a test", false)]
		[TestCase("test", "", true)]
		[TestCase("test", null, false)]
		[TestCase("blah", "Just a test", false)]
		[TestCase("Test", "Just a test", true)]
		[TestCase("test", "Just a test", true)]
		public void Match(string filter, string data, bool expectedResult)
		{
			var service = GetService();
			service.Initialize(filter);

			var result = service.Match(data);

			Assert.That(expectedResult, Is.EqualTo(result));
		}

		#endregion
	}
}