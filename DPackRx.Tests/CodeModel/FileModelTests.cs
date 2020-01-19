using DPackRx.CodeModel;

using NUnit.Framework;

namespace DPackRx.Tests.CodeModel
{
	/// <summary>
	/// FileModel tests.
	/// </summary>
	[TestFixture]
	public class FileModelTests
	{
		#region Private Methods

		/// <summary>
		/// Returns test model instance.
		/// </summary>
		private FileModel GetModel()
		{
			return new FileModel();
		}

		#endregion

		#region Tests

		[TestCase("test", -1)]
		[TestCase("test.txt", 4)]
		public void DataEndingIndex(string fileName, int expectedIndex)
		{
			var model = GetModel();
			model.FileName = fileName;

			Assert.That(model.DataEndingIndex, Is.EqualTo(expectedIndex));
		}

		[TestCase("test", "")]
		[TestCase("Test", "T")]
		[TestCase("SomeTest", "ST")]
		[TestCase("SomeTest_OtherTest", "STOT")]
		public void PascalCasedData(string fileName, string expectedPascalCased)
		{
			var model = GetModel();
			model.FileName = fileName;

			Assert.That(model.PascalCasedData, Is.EqualTo(expectedPascalCased));
		}

		[TestCase(0, 0, "proj", "proj", "file1", "file2", -1)]
		[TestCase(0, 0, "proj", "proj", "file", "file", 0)]
		[TestCase(0, 0, "proj2", "proj1", "file1", "file2", -1)]
		[TestCase(1, 2, "proj1", "proj2", "file1", "file2", 1)]
		[TestCase(2, 1, "proj1", "proj2", "file1", "file2", -1)]
		public void CompareTo(int rank1, int rank2, string project1, string project2, string file1, string file2, int expectedResult)
		{
			var model = GetModel();
			model.FileName = file1;
			model.ProjectItem = project1;
			model.Rank = rank1;

			var anotherModel = GetModel();
			anotherModel.FileName = file2;
			anotherModel.ProjectItem = project2;
			anotherModel.Rank = rank2;

			int result = model.CompareTo(anotherModel);

			Assert.That(result, Is.EqualTo(expectedResult));
		}

		#endregion
	}
}