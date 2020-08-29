using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

using Moq;
using NUnit.Framework;

using DPackRx.CodeModel;
using DPackRx.Extensions;
using DPackRx.Features.CodeBrowser;
using DPackRx.Options;
using DPackRx.Services;

namespace DPackRx.Tests.Features
{
	/// <summary>
	/// CodeBrowserViewModel tests.
	/// </summary>
	[TestFixture]
	public class CodeBrowserViewModelTests
	{
		#region Fields

		private Mock<IServiceProvider> _serviceProviderMock;
		private Mock<ILog> _logMock;
		private Mock<IMessageService> _messageServiceMock;
		private Mock<IOptionsService> _optionsServiceMock;
		private Mock<IFileProcessor> _fileProcessorMock;
		private Mock<ISearchMatchService> _searchMatchServiceMock;
		private Mock<IShellSelectionService> _shellSelectionServiceMock;
		private Mock<IShellImageService> _shellImageServiceMock;
		private List<MemberCodeModel> _members;

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

			_members = new List<MemberCodeModel>
			{
				new MemberCodeModel { Name = "Test", FullName = "class1.Test", ElementKind = Kind.Method, Rank = 0, Matched = false },
				new MemberCodeModel { Name = "Hello", FullName = "class1.Hello", ElementKind = Kind.Method, Rank = 0, Matched = false },
				new MemberCodeModel { Name = "TestToo", FullName = "class1.TestToo", ElementKind = Kind.Property, Rank = 0, Matched = false },
				new MemberCodeModel { Name = "_somethingElse", FullName = "class1._somethingElse", ElementKind = Kind.Variable, Rank = 0, Matched = false },
			};
			_fileProcessorMock = new Mock<IFileProcessor>();
			_fileProcessorMock
				.Setup(p => p.GetMembers(ProcessorFlags.IncludeFileCodeModel, It.IsAny<CodeModelFilterFlags>()))
				.Returns(new FileCodeModel { FileName = "test", Members = _members })
				.Verifiable();

			_searchMatchServiceMock = new Mock<ISearchMatchService>();
			_searchMatchServiceMock.Setup(s => s.MatchItems(It.IsAny<string>(), It.IsAny<IEnumerable<IMatchItem>>())).Verifiable();

			_shellSelectionServiceMock = new Mock<IShellSelectionService>();

			_shellImageServiceMock = new Mock<IShellImageService>();
		}

		[TearDown]
		public void TearDown()
		{
			_serviceProviderMock = null;
			_logMock = null;
			_messageServiceMock = null;
			_optionsServiceMock = null;
			_fileProcessorMock = null;
			_searchMatchServiceMock = null;
			_shellSelectionServiceMock = null;
			_shellImageServiceMock = null;
			_members = null;
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Returns test instance.
		/// </summary>
		private CodeBrowserViewModel GetViewModel()
		{
			return new CodeBrowserViewModel(_serviceProviderMock.Object, _logMock.Object, _messageServiceMock.Object,
				_optionsServiceMock.Object, _fileProcessorMock.Object, _searchMatchServiceMock.Object,
				_shellSelectionServiceMock.Object, _shellImageServiceMock.Object);
		}

		#endregion

		#region Tests

		[TestCase("test", CodeModelFilterFlags.All, 2)]
		[TestCase("test", CodeModelFilterFlags.Methods, 1)]
		[TestCase("test", CodeModelFilterFlags.Properties, 1)]
		[TestCase("test", CodeModelFilterFlags.ClassesInterfaces, 0)]
		[TestCase("", CodeModelFilterFlags.All, 4)]
		[TestCase("", CodeModelFilterFlags.Methods, 2)]
		[TestCase("", CodeModelFilterFlags.Properties, 2)]
		[TestCase("", CodeModelFilterFlags.ClassesInterfaces, 0)]
		public void OnInitialize(string search, CodeModelFilterFlags flags, int expectedCount)
		{
			var viewModel = GetViewModel();

			if (string.IsNullOrEmpty(search))
				_members.ForEach(f => f.Matched = true);
			else
				_members.Where(f => f.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0).ForEach(f => f.Matched = true);
			if (flags == CodeModelFilterFlags.Methods)
				_members.Where(f => f.Matched && f.ElementKind != Kind.Method).ForEach(f => f.Matched = false);
			else if (flags == CodeModelFilterFlags.Properties)
				_members.Where(f => f.Matched && !((f.ElementKind == Kind.Property) || (f.ElementKind == Kind.Variable))).ForEach(f => f.Matched = false);
			else if (flags == CodeModelFilterFlags.ClassesInterfaces)
				_members.Where(f => f.Matched && !((f.ElementKind == Kind.Class) || (f.ElementKind == Kind.Interface))).ForEach(f => f.Matched = false);

			_optionsServiceMock.Setup(o => o.GetStringOption(viewModel.Feature, "File", string.Empty)).Returns("test").Verifiable();
			_optionsServiceMock.Setup(o => o.GetStringOption(viewModel.Feature, "Search", string.Empty)).Returns(search).Verifiable();
			_optionsServiceMock.Setup(o => o.GetIntOption(viewModel.Feature, "Filter", (int)flags)).Returns((int)flags).Verifiable();
			_optionsServiceMock.Setup(o => o.GetBoolOption(viewModel.Feature, "XmlDoc", false)).Returns(false).Verifiable();

			viewModel.OnInitialize(flags);

			Assert.That(viewModel.Members, Is.Not.Null);
			Assert.That(viewModel.FilteredMembers, Is.Not.Null);
			Assert.That(viewModel.FilteredMembers.Count, Is.EqualTo(expectedCount));
			Assert.That(viewModel.Search, Is.EqualTo(search));
			Assert.That(viewModel.Filter, Is.EqualTo(flags));
			Assert.That(viewModel.SameType, Is.True);
			Assert.That(viewModel.FileName, Is.EqualTo("test"));
			Assert.That(viewModel.Title, Is.Not.Null.And.Not.Empty);
			Assert.That(viewModel.Title, Contains.Substring(" - test"));
			_fileProcessorMock.Verify(p => p.GetMembers(ProcessorFlags.IncludeFileCodeModel, It.IsAny<CodeModelFilterFlags>()));
			_optionsServiceMock.Verify(o => o.GetStringOption(viewModel.Feature, "File", string.Empty));
			_optionsServiceMock.Verify(o => o.GetStringOption(viewModel.Feature, "Search", string.Empty));
			_optionsServiceMock.Verify(o => o.GetIntOption(viewModel.Feature, "Filter", (int)flags));
			_optionsServiceMock.Verify(o => o.GetBoolOption(viewModel.Feature, "XmlDoc", false));
			_searchMatchServiceMock.Verify(s => s.MatchItems(search, It.IsAny<IEnumerable<IMatchItem>>()), Times.Once);
		}

		[Test]
		public void OnInitialize_NotSameType()
		{
			_members.Clear();
			_members.AddRange(new[]
			{
				new MemberCodeModel { Name = "Test1", FullName = "Test1", ElementKind = Kind.Class, Rank = 0, Matched = false },
				new MemberCodeModel { Name = "Test2", FullName = "Test2", ElementKind = Kind.Class, Rank = 0, Matched = false },
			});
			var viewModel = GetViewModel();

			viewModel.OnInitialize(CodeModelFilterFlags.All);

			Assert.That(viewModel.SameType, Is.False);
		}

		[Test]
		public void OnInitialize_ResetSearch()
		{
			var viewModel = GetViewModel();

			_optionsServiceMock.Setup(o => o.GetStringOption(viewModel.Feature, "File", string.Empty)).Returns("something else").Verifiable();
			_optionsServiceMock.Setup(o => o.GetStringOption(viewModel.Feature, "Search", string.Empty)).Returns("hello").Verifiable();

			viewModel.OnInitialize(CodeModelFilterFlags.All);

			Assert.That(viewModel.Search, Is.Null.Or.Empty, "Search should be reset on new file");
			Assert.That(viewModel.FileName, Is.EqualTo("test"));
		}

		[Test]
		public void OnInitialize_ErrorHandling()
		{
			var viewModel = GetViewModel();

			Assert.Throws<ArgumentException>(() => viewModel.OnInitialize(null));
		}

		[TestCase(true)]
		[TestCase(false)]
		public void OnClose(bool apply)
		{
			var viewModel = GetViewModel();

			_optionsServiceMock.Setup(o => o.SetStringOption(viewModel.Feature, "File", string.Empty)).Verifiable();
			_optionsServiceMock.Setup(o => o.SetStringOption(viewModel.Feature, "Search", string.Empty)).Verifiable();
			_optionsServiceMock.Setup(o => o.SetIntOption(viewModel.Feature, "Filter", 0)).Verifiable();

			_shellSelectionServiceMock.Setup(s => s.SetActiveFilePosition(It.IsAny<int>(), 1)).Verifiable();

			viewModel.Selection = _members[1];

			viewModel.OnClose(apply);

			_optionsServiceMock.Verify(o => o.SetStringOption(viewModel.Feature, "File", string.Empty));
			_optionsServiceMock.Verify(o => o.SetStringOption(viewModel.Feature, "Search", string.Empty));
			_optionsServiceMock.Verify(o => o.SetIntOption(viewModel.Feature, "Filter", 0));
			if (apply)
				_shellSelectionServiceMock.Verify(s => s.SetActiveFilePosition(It.IsAny<int>(), 1), Times.Once);
			else
				_shellSelectionServiceMock.Verify(s => s.SetActiveFilePosition(It.IsAny<int>(), 1), Times.Never);
		}

		[Test]
		public void OnClose_NoSelection()
		{
			var viewModel = GetViewModel();

			viewModel.OnClose(true);

			_shellSelectionServiceMock.Verify(s => s.SetActiveFilePosition(It.IsAny<int>(), 1), Times.Never);
		}

		[TestCase(CodeModelFilterFlags.All, false)]
		[TestCase(CodeModelFilterFlags.Methods, true)]
		[TestCase(CodeModelFilterFlags.Properties, true)]
		[TestCase(CodeModelFilterFlags.ClassesInterfaces, true)]
		public void ShowAllMembersCommand_CanExecute(CodeModelFilterFlags flags, bool expectedResult)
		{
			var viewModel = GetViewModel();
			viewModel.Filter = flags;

			var result = viewModel.ShowAllMembersCommand.CanExecute(null);

			Assert.That(result, Is.EqualTo(expectedResult));
		}

		[TestCase(CodeModelFilterFlags.All, Visibility.Collapsed)]
		[TestCase(CodeModelFilterFlags.Methods, Visibility.Visible)]
		[TestCase(CodeModelFilterFlags.Properties, Visibility.Visible)]
		[TestCase(CodeModelFilterFlags.ClassesInterfaces, Visibility.Visible)]
		public void ShowAllMembersCommand_Execute(CodeModelFilterFlags flags, Visibility expectedVisibility)
		{
			var viewModel = GetViewModel();
			var changed = false;
			viewModel.PropertyChanged += (sender, e) =>
			{
				if (e.PropertyName == nameof(viewModel.ShowAllMembers))
					changed = true;
			};

			var showAllMembers = viewModel.ShowAllMembers;
			viewModel.ShowAllMembersCommand.Execute(flags);

			Assert.That(viewModel.Filter, Is.EqualTo(flags));
			Assert.That(viewModel.ShowAllMembers, Is.EqualTo(expectedVisibility));
			Assert.That(changed, Is.True);
			_fileProcessorMock.Verify(p => p.GetMembers(ProcessorFlags.IncludeFileCodeModel, It.IsAny<CodeModelFilterFlags>()));
			_searchMatchServiceMock.Setup(s => s.MatchItems(It.IsAny<string>(), It.IsAny<IEnumerable<IMatchItem>>())).Verifiable();
		}

		[TestCase("test", true)]
		[TestCase(null, false)]
		public void SelectMemberCommand_CanExecute(object argument, bool expectedResult)
		{
			var viewModel = GetViewModel();

			var result = viewModel.SelectMemberCommand.CanExecute(argument);

			Assert.That(result, Is.EqualTo(expectedResult));
		}

		[Test]
		public void SelectMemberCommand_Execute()
		{
			var viewModel = GetViewModel();

			var selection = new MemberCodeModel();
			viewModel.SelectMemberCommand.Execute(selection);

			Assert.That(viewModel.Selection, Is.EqualTo(selection));
			Assert.That(viewModel.CloseWindow, Is.True);
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
		public void Members()
		{
			var viewModel = GetViewModel();

			Assert.That(viewModel.Members, Is.Not.Null);
		}

		[Test]
		public void FilteredMembers()
		{
			var viewModel = GetViewModel();

			_members.Where(f => f.Name.IndexOf("test", StringComparison.OrdinalIgnoreCase) >= 0).ForEach(f => f.Matched = true);
			viewModel.OnInitialize(CodeModelFilterFlags.All);
			viewModel.Search = "test";

			Assert.That(viewModel.FilteredMembers, Is.Not.Null);
			Assert.That(viewModel.FilteredMembers.Count, Is.EqualTo(2));
		}

		[TestCase(CodeModelFilterFlags.All, Visibility.Collapsed)]
		[TestCase(CodeModelFilterFlags.Methods, Visibility.Visible)]
		[TestCase(CodeModelFilterFlags.Properties, Visibility.Visible)]
		[TestCase(CodeModelFilterFlags.ClassesInterfaces, Visibility.Visible)]
		public void ShowAllMembers(CodeModelFilterFlags flags, Visibility expectedVisibility)
		{
			var viewModel = GetViewModel();

			viewModel.Filter = flags;

			var visibility = viewModel.ShowAllMembers;

			Assert.That(visibility, Is.EqualTo(expectedVisibility));
		}

		[Test]
		public void ImageShowAllMembers()
		{
			var viewModel = GetViewModel();
			var image = new BitmapImage();

			_shellImageServiceMock.Setup(s => s.GetWellKnownImage(WellKnownImage.AllCode)).Returns(image).Verifiable();

			var result = viewModel.ImageShowAllMembers;

			Assert.That(result, Is.EqualTo(image));
			_shellImageServiceMock.Verify(s => s.GetWellKnownImage(WellKnownImage.AllCode));
		}

		#endregion
	}
}