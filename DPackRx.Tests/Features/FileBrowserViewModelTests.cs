using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;

using Moq;
using NUnit.Framework;

using DPackRx.CodeModel;
using DPackRx.Extensions;
using DPackRx.Features;
using DPackRx.Features.FileBrowser;
using DPackRx.Options;
using DPackRx.Package;
using DPackRx.Services;

namespace DPackRx.Tests.Features
{
	/// <summary>
	/// FileBrowserViewModel tests.
	/// </summary>
	[TestFixture]
	public class FileBrowserViewModelTests
	{
		#region Fields

		private Mock<IServiceProvider> _serviceProviderMock;
		private Mock<ILog> _logMock;
		private Mock<IMessageService> _messageServiceMock;
		private Mock<IOptionsService> _optionsServiceMock;
		private Mock<ISolutionProcessor> _solutionProcessorMock;
		private Mock<IFileTypeResolver> _fileTypeResolverMock;
		private Mock<ISearchMatchService> _searchMatchServiceMock;
		private Mock<IShellHelperService> _shellHelperServiceMock;
		private Mock<IShellImageService> _shellImageServiceMock;
		private Mock<IUtilsService> _utilsServiceMock;
		private Mock<IFeature> _featureMock;
		private Mock<IFeatureFactory> _featureFactoryMock;
		private List<FileModel> _files;

		#endregion

		#region Tests Setup

		[SetUp]
		public void Setup()
		{
			_serviceProviderMock = new Mock<IServiceProvider>();

			_logMock = new Mock<ILog>();
			_logMock.Setup(l => l.LogMessage(It.IsAny<string>(), It.IsAny<string>())).Verifiable();

			_messageServiceMock = new Mock<IMessageService>();

			_optionsServiceMock = new Mock<IOptionsService>();

			_files = new List<FileModel>
			{
					new FileModel { FileName = "test.cs", FileNameWithPath = @"c:\test.cs", ItemSubType = FileSubType.Code, Rank = 0, Matched = false },
					new FileModel { FileName = "SomeTest.txt", FileNameWithPath = @"c:\SomeTest.txt", ItemSubType = FileSubType.None, Rank = 0, Matched = false },
					new FileModel { FileName = "SomethingElse.cs", FileNameWithPath = @"c:\SomethingElse.cs", ItemSubType = FileSubType.Code, Rank = 0, Matched = false },
					new FileModel { FileName = "AnotherOne.bmp", FileNameWithPath = @"c:\data\AnotherOne.bmp", ItemSubType = FileSubType.ImageFile, Rank = 0, Matched = false },
			};
			_solutionProcessorMock = new Mock<ISolutionProcessor>();
			_solutionProcessorMock
				.Setup(p => p.GetProjects(ProcessorFlags.IncludeFiles | ProcessorFlags.GroupLinkedFiles, CodeModelFilterFlags.All))
				.Returns(new SolutionModel { Files = _files })
				.Verifiable();

			_fileTypeResolverMock = new Mock<IFileTypeResolver>();

			_searchMatchServiceMock = new Mock<ISearchMatchService>();
			_searchMatchServiceMock.Setup(s => s.MatchItems(It.IsAny<string>(), It.IsAny<IEnumerable<IMatchItem>>())).Verifiable();

			_shellHelperServiceMock = new Mock<IShellHelperService>();

			_shellImageServiceMock = new Mock<IShellImageService>();

			_utilsServiceMock = new Mock<IUtilsService>();

			_featureMock = new Mock<IFeature>();
			_featureMock.SetupGet(f => f.KnownFeature).Returns(KnownFeature.CodeBrowser);
			_featureMock.Setup(f => f.Execute(It.IsAny<int>())).Returns(true).Verifiable();

			_featureFactoryMock = new Mock<IFeatureFactory>();
			_featureFactoryMock.Setup(f => f.GetFeature(KnownFeature.CodeBrowser)).Returns(_featureMock.Object).Verifiable();
		}

		[TearDown]
		public void TearDown()
		{
			_serviceProviderMock = null;
			_logMock = null;
			_messageServiceMock = null;
			_optionsServiceMock = null;
			_solutionProcessorMock = null;
			_fileTypeResolverMock = null;
			_searchMatchServiceMock = null;
			_shellHelperServiceMock = null;
			_shellImageServiceMock = null;
			_utilsServiceMock = null;
			_featureMock = null;
			_featureFactoryMock = null;
			_files = null;
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Returns test instance.
		/// </summary>
		private FileBrowserViewModel GetViewModel()
		{
			return new FileBrowserViewModel(_serviceProviderMock.Object, _logMock.Object, _messageServiceMock.Object,
				_optionsServiceMock.Object, _solutionProcessorMock.Object, _fileTypeResolverMock.Object,
				_searchMatchServiceMock.Object, _shellHelperServiceMock.Object, _shellImageServiceMock.Object,
				_utilsServiceMock.Object, _featureFactoryMock.Object);
		}

		#endregion

		#region Tests

		[TestCase("test", true, "", "", "", 1, 1)]
		[TestCase("test", true, ".txt,.txt", "", "", 1, 0)]
		[TestCase("test", false, "", "", "", 0, 1)]
		[TestCase("test", false, ".cs", "", "", 0, 0)]
		[TestCase("", true, "", "", "", 2, 2)]
		[TestCase("", true, "", "", @"C:\Data", 2, 1)]
		[TestCase("", false, "", "", "", 2, 0)]
		[TestCase("", false, "", ".bmp,.bmp", "", 2, 1)]
		[TestCase("", false, "", ".bmp,.bmp", @"C:\data", 2, 0)]
		public void OnInitialize(string search, bool allFiles, string ignoreFiles, string showFiles, string ignoreFolders,
			int expectedCodeFileCount, int expectedNoneCodeFileCount)
		{
			var viewModel = GetViewModel();

			if (string.IsNullOrEmpty(search))
				_files.ForEach(f => f.Matched = true);
			else
				_files.Where(f => f.FileName.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0).ForEach(f => f.Matched = true);

			_optionsServiceMock.Setup(o => o.GetStringOption(viewModel.Feature, "Search", string.Empty)).Returns(search).Verifiable();
			_optionsServiceMock.Setup(o => o.GetBoolOption(viewModel.Feature, "AllFiles", false)).Returns(allFiles).Verifiable();
			_optionsServiceMock.Setup(o => o.GetStringOption(viewModel.Feature, "IgnoreFiles", null)).Returns(ignoreFiles).Verifiable();
			_optionsServiceMock.Setup(o => o.GetStringOption(viewModel.Feature, "ShowFiles", null)).Returns(showFiles).Verifiable();
			_optionsServiceMock.Setup(o => o.GetStringOption(viewModel.Feature, "IgnoreFolders", null)).Returns(ignoreFolders).Verifiable();

			_fileTypeResolverMock.Setup(r => r.IsCodeSubType(FileSubType.Code, true)).Returns(true).Verifiable();
			_fileTypeResolverMock.Setup(r => r.IsCodeSubType(FileSubType.None, true)).Returns(false).Verifiable();
			_fileTypeResolverMock.Setup(r => r.IsCodeSubType(FileSubType.ImageFile, true)).Returns(false).Verifiable();

			viewModel.OnInitialize(null);

			Assert.That(viewModel.Files, Is.Not.Null);
			Assert.That(viewModel.FilteredFiles, Is.Not.Null);
			Assert.That(viewModel.FilteredFiles.Count, Is.EqualTo(expectedCodeFileCount + expectedNoneCodeFileCount));
			Assert.That(viewModel.Search, Is.EqualTo(search));
			Assert.That(viewModel.AllFiles, Is.EqualTo(allFiles));
			_solutionProcessorMock.Verify(p => p.GetProjects(ProcessorFlags.IncludeFiles | ProcessorFlags.GroupLinkedFiles, CodeModelFilterFlags.All));
			_optionsServiceMock.Verify(o => o.GetStringOption(viewModel.Feature, "Search", string.Empty), Times.Once);
			_optionsServiceMock.Verify(o => o.GetBoolOption(viewModel.Feature, "AllFiles", false), Times.Once);
			_optionsServiceMock.Verify(o => o.GetStringOption(viewModel.Feature, "IgnoreFiles", null), Times.Once);
			_optionsServiceMock.Verify(o => o.GetStringOption(viewModel.Feature, "ShowFiles", null), Times.Once);
			_optionsServiceMock.Verify(o => o.GetStringOption(viewModel.Feature, "IgnoreFolders", null), Times.Once);
			if (allFiles)
			{
				_fileTypeResolverMock.Verify(r => r.IsCodeSubType(FileSubType.Code, true), Times.Never);
				_fileTypeResolverMock.Verify(r => r.IsCodeSubType(FileSubType.None, true), Times.Never);
			}
			else
			{
				if (expectedCodeFileCount > 0)
					_fileTypeResolverMock.Verify(r => r.IsCodeSubType(FileSubType.Code, true), Times.AtLeast(expectedCodeFileCount));
				if (expectedNoneCodeFileCount > 0)
					_fileTypeResolverMock.Verify(r => r.IsCodeSubType(It.IsNotIn(FileSubType.Code), true), Times.AtLeast(expectedNoneCodeFileCount));
			}
			_searchMatchServiceMock.Verify(s => s.MatchItems(search, It.IsAny<IEnumerable<IMatchItem>>()), Times.Once);
		}

		[TestCase(true, true)]
		[TestCase(true, false)]
		[TestCase(false, true)]
		public void OnClose(bool apply, bool selectCode)
		{
			var viewModel = GetViewModel();

			_optionsServiceMock.Setup(o => o.SetStringOption(viewModel.Feature, "Search", string.Empty)).Verifiable();
			_optionsServiceMock.Setup(o => o.SetBoolOption(viewModel.Feature, "AllFiles", false)).Verifiable();

			_shellHelperServiceMock.Setup(s => s.OpenFiles(It.IsAny<IEnumerable<IExtensibilityItem>>())).Verifiable();
			_shellHelperServiceMock.Setup(s => s.OpenDesignerFiles(It.IsAny<IEnumerable<IExtensibilityItem>>())).Verifiable();

			viewModel.SelectionCode = selectCode;
			viewModel.Selection = _files;

			viewModel.OnClose(apply);

			_optionsServiceMock.Verify(o => o.SetStringOption(viewModel.Feature, "Search", string.Empty));
			_optionsServiceMock.Verify(o => o.SetBoolOption(viewModel.Feature, "AllFiles", false));
			if (apply)
			{
				if (selectCode)
				{
					_shellHelperServiceMock.Verify(s => s.OpenFiles(It.IsAny<IEnumerable<IExtensibilityItem>>()), Times.Once);
					_shellHelperServiceMock.Verify(s => s.OpenDesignerFiles(It.IsAny<IEnumerable<IExtensibilityItem>>()), Times.Never);
				}
				else
				{
					_shellHelperServiceMock.Verify(s => s.OpenFiles(It.IsAny<IEnumerable<IExtensibilityItem>>()), Times.Never);
					_shellHelperServiceMock.Verify(s => s.OpenDesignerFiles(It.IsAny<IEnumerable<IExtensibilityItem>>()), Times.Once);
				}
			}
			else
			{
				_shellHelperServiceMock.Verify(s => s.OpenFiles(It.IsAny<IEnumerable<IExtensibilityItem>>()), Times.Never);
				_shellHelperServiceMock.Verify(s => s.OpenDesignerFiles(It.IsAny<IEnumerable<IExtensibilityItem>>()), Times.Never);
			}
			_featureFactoryMock.Verify(f => f.GetFeature(KnownFeature.CodeBrowser), Times.Never);
			_featureMock.Verify(f => f.Execute(It.IsAny<int>()), Times.Never);
		}

		[TestCase(CommandIDs.CODE_BROWSER)]
		[TestCase(CommandIDs.CODE_BROWSER_CI)]
		[TestCase(CommandIDs.CODE_BROWSER_M)]
		[TestCase(CommandIDs.CODE_BROWSER_P)]
		public void OnClose_OpenCodeBrowser(int commandId)
		{
			var viewModel = GetViewModel();
			viewModel.SelectionCode = true;
			viewModel.Selection = _files;
			viewModel.CodeBrowserCommandId = commandId;

			viewModel.OnClose(true);

			_featureFactoryMock.Verify(f => f.GetFeature(KnownFeature.CodeBrowser), Times.Once);
			_featureMock.Verify(f => f.Execute(It.IsAny<int>()), Times.Once);
		}

		[Test]
		public void OnClose_NoSelection()
		{
			var viewModel = GetViewModel();

			_shellHelperServiceMock.Setup(s => s.OpenFiles(It.IsAny<IEnumerable<IExtensibilityItem>>())).Verifiable();
			_shellHelperServiceMock.Setup(s => s.OpenDesignerFiles(It.IsAny<IEnumerable<IExtensibilityItem>>())).Verifiable();

			viewModel.OnClose(true);

			_shellHelperServiceMock.Verify(s => s.OpenFiles(It.IsAny<IEnumerable<IExtensibilityItem>>()), Times.Never);
			_shellHelperServiceMock.Verify(s => s.OpenDesignerFiles(It.IsAny<IEnumerable<IExtensibilityItem>>()), Times.Never);
		}

		[Test]
		public void ShowFilesCommand_Execute()
		{
			var viewModel = GetViewModel();

			var allFilesBefore = viewModel.AllFiles;

			viewModel.ShowFilesCommand.Execute(true.ToString());

			var allFilesAfter = viewModel.AllFiles;

			Assert.That(allFilesBefore, Is.Not.EqualTo(allFilesAfter));
		}

		[Test]
		public void OpenCodeBrowserAllCommand_Execute()
		{
			var viewModel = GetViewModel();

			viewModel.OpenCodeBrowserAllCommand.Execute(new List<string> { "test" });

			Assert.That(viewModel.CodeBrowserCommandId, Is.EqualTo(CommandIDs.CODE_BROWSER));
			Assert.That(viewModel.CloseWindow, Is.True);
		}

		[Test]
		public void OpenCodeBrowserClassesCommand_Execute()
		{
			var viewModel = GetViewModel();

			viewModel.OpenCodeBrowserClassesCommand.Execute(new List<string> { "test" });

			Assert.That(viewModel.CodeBrowserCommandId, Is.EqualTo(CommandIDs.CODE_BROWSER_CI));
			Assert.That(viewModel.CloseWindow, Is.True);
		}

		[Test]
		public void OpenCodeBrowserMethodsCommand_Execute()
		{
			var viewModel = GetViewModel();

			viewModel.OpenCodeBrowserMethodsCommand.Execute(new List<string> { "test" });

			Assert.That(viewModel.CodeBrowserCommandId, Is.EqualTo(CommandIDs.CODE_BROWSER_M));
			Assert.That(viewModel.CloseWindow, Is.True);
		}

		[Test]
		public void OpenCodeBrowserPropertiesCommand_Execute()
		{
			var viewModel = GetViewModel();

			viewModel.OpenCodeBrowserPropertiesCommand.Execute(new List<string> { "test" });

			Assert.That(viewModel.CodeBrowserCommandId, Is.EqualTo(CommandIDs.CODE_BROWSER_P));
			Assert.That(viewModel.CloseWindow, Is.True);
		}

		[TestCase("test", true)]
		[TestCase(null, false)]
		public void OpenFilesCommand_CanExecute(string item, bool expectedResult)
		{
			var viewModel = GetViewModel();

			var selection = new List<string>();
			if (!string.IsNullOrEmpty(item))
				selection.Add(item);

			var result = viewModel.OpenFilesCommand.CanExecute(selection);

			Assert.That(result, Is.EqualTo(expectedResult));
		}

		[TestCase(false)]
		[TestCase(true)]
		public void OpenFilesCommand_Execute(bool controlKeyDown)
		{
			var viewModel = GetViewModel();

			_utilsServiceMock.Setup(u => u.ControlKeyDown()).Returns(controlKeyDown).Verifiable();

			var selection = new List<FileModel>();
			viewModel.OpenFilesCommand.Execute(selection);

			Assert.That(viewModel.SelectionCode, Is.EqualTo(controlKeyDown ? false : true));
			Assert.That(viewModel.Selection, Is.EqualTo(selection));
			Assert.That(viewModel.CloseWindow, Is.True);
			_utilsServiceMock.Verify(u => u.ControlKeyDown());
		}

		[Test]
		public void Search()
		{
			var viewModel = GetViewModel();
			var changed = false;
			viewModel.PropertyChanged += (sender, e) =>
			{
				if (e.PropertyName == nameof(viewModel.Search))
					changed = true;
			};

			viewModel.Search = "test";

			Assert.That(changed, Is.True);
			_searchMatchServiceMock.Verify(s => s.MatchItems(It.IsAny<string>(), It.IsAny<IEnumerable<IMatchItem>>()));
		}

		[Test]
		public void AllFiles()
		{
			var viewModel = GetViewModel();
			var changed = false;
			viewModel.PropertyChanged += (sender, e) =>
			{
				if (e.PropertyName == nameof(viewModel.AllFiles))
					changed = true;
			};

			viewModel.AllFiles = !viewModel.AllFiles;

			Assert.That(changed, Is.True);
			_searchMatchServiceMock.Verify(s => s.MatchItems(It.IsAny<string>(), It.IsAny<IEnumerable<IMatchItem>>()));
		}

		[Test]
		public void Files()
		{
			var viewModel = GetViewModel();

			Assert.That(viewModel.Files, Is.Not.Null);
		}

		[Test]
		public void FilteredFiles()
		{
			var viewModel = GetViewModel();

			_files.Where(f => f.FileName.IndexOf("test", StringComparison.OrdinalIgnoreCase) >= 0).ForEach(f => f.Matched = true);
			viewModel.OnInitialize(null);
			viewModel.AllFiles = true;
			viewModel.Search = "test";

			Assert.That(viewModel.FilteredFiles, Is.Not.Null);
			Assert.That(viewModel.FilteredFiles.Count, Is.EqualTo(2));
		}

		[Test]
		public void ImageShowAllFiles()
		{
			var viewModel = GetViewModel();
			var image = new BitmapImage();

			_shellImageServiceMock.Setup(s => s.GetWellKnownImage(WellKnownImage.AllFiles)).Returns(image).Verifiable();

			var result = viewModel.ImageShowAllFiles;

			Assert.That(result, Is.EqualTo(image));
			_shellImageServiceMock.Verify(s => s.GetWellKnownImage(WellKnownImage.AllFiles));
		}

		[Test]
		public void ImageShowCodeFiles()
		{
			var viewModel = GetViewModel();
			var image = new BitmapImage();

			_shellImageServiceMock.Setup(s => s.GetWellKnownImage(WellKnownImage.AllCode)).Returns(image).Verifiable();

			var result = viewModel.ImageShowCodeFiles;

			Assert.That(result, Is.EqualTo(image));
			_shellImageServiceMock.Verify(s => s.GetWellKnownImage(WellKnownImage.AllCode));
		}

		#endregion
	}
}