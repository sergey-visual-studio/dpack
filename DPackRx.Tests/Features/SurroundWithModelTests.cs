using DPackRx.Features.SurroundWith;

using NUnit.Framework;

namespace DPackRx.Tests.Features
{
	/// <summary>
	/// SurroundWithModel tests.
	/// </summary>
	[TestFixture]
	public class SurroundWithModelTests
	{
		#region Tests

		[Test]
		public void SurroundWithModel()
		{
			var model = new SurroundWithModel();

			Assert.That(model.Models, Is.Not.Null);
			Assert.That(model.Models.Count, Is.Zero);
		}

		[Test]
		public void SurroundWithLanguageModel()
		{
			var model = new SurroundWithLanguageModel
			{
				Language = SurroundWithLanguage.CSharp,
				Type = SurroundWithType.ForEach,
				StartingCode = "test1",
				EndingCode = "test2",
				WordOffset = 123
			};

			Assert.That(model.Language, Is.EqualTo(SurroundWithLanguage.CSharp));
			Assert.That(model.Type, Is.EqualTo(SurroundWithType.ForEach));
			Assert.That(model.StartingCode, Is.EqualTo("test1"));
			Assert.That(model.EndingCode, Is.EqualTo("test2"));
			Assert.That(model.WordOffset, Is.EqualTo(123));
		}

		#endregion
	}
}