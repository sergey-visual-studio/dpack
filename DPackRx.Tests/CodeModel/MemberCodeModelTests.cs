using DPackRx.CodeModel;

using NUnit.Framework;

namespace DPackRx.Tests.CodeModel
{
	/// <summary>
	/// <see cref="MemberCodeModel"/> tests.
	/// </summary>
	[TestFixture]
	public class MemberCodeModelTests
	{
		#region Private Methods

		/// <summary>
		/// Returns test model instance.
		/// </summary>
		private MemberCodeModel GetModel()
		{
			return new MemberCodeModel();
		}

		#endregion

		#region Tests

		[TestCase("test", "test")]
		[TestCase("some.test", "test")]
		[TestCase(".", "")]
		public void ShortName(string name, string expectedName)
		{
			var model = GetModel();
			model.Name = name;

			Assert.That(model.ShortName, Is.EqualTo(expectedName));
		}

		[TestCase("test", "", 4)]
		[TestCase("test", "<", -1)]
		public void DataEndingIndex(string name, string generics, int expectedIndex)
		{
			var model = GetModel();
			model.Name = name;
			model.SupportsGenerics = !string.IsNullOrEmpty(generics);
			model.GenericsSuffix = generics;

			Assert.That(model.DataEndingIndex, Is.EqualTo(expectedIndex));
		}

		[TestCase("test", "")]
		[TestCase("Test", "T")]
		[TestCase("SomeTest", "ST")]
		[TestCase("SomeTest_OtherTest", "STOT")]
		public void PascalCasedData(string name, string expectedPascalCased)
		{
			var model = GetModel();
			model.Name = name;

			Assert.That(model.PascalCasedData, Is.EqualTo(expectedPascalCased));
		}

		[TestCase(0, 0, 1, 2, -1)]
		[TestCase(0, 0, 2, 1, 1)]
		[TestCase(1, 2, 0, 0, 1)]
		[TestCase(2, 1, 0, 0, -1)]
		public void CompareTo(int rank1, int rank2, int line1, int line2, int expectedResult)
		{
			var model = GetModel();
			model.Line = line1;
			model.Rank = rank1;
			model.ElementKind = Kind.Unknown;

			var anotherModel = GetModel();
			anotherModel.Line = line2;
			anotherModel.Rank = rank2;
			anotherModel.ElementKind = Kind.Unknown;

			int result = model.CompareTo(anotherModel);

			Assert.That(result, Is.EqualTo(expectedResult));
		}

		#endregion
	}
}