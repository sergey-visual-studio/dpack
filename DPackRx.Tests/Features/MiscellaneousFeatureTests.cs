using System;
using System.Collections.Generic;

using Moq;
using NUnit.Framework;

using DPackRx.CodeModel;
using DPackRx.Features;
using DPackRx.Features.Miscellaneous;
using DPackRx.Options;
using DPackRx.Package;
using DPackRx.Services;

namespace DPackRx.Tests.Features
{
	/// <summary>
	/// MiscellaneousFeature tests.
	/// </summary>
	[TestFixture]
	public class MiscellaneousFeatureTests
	{
		#region Fields

		private Mock<IServiceProvider> _serviceProviderMock;
		private Mock<ILog> _logMock;
		private Mock<IOptionsService> _optionsServiceMock;
		private Mock<IPackageService> _packageServiceMock;
		private Mock<IUtilsService> _utilsServiceMock;
		private Mock<IShellHelperService> _shellHelperServiceMock;
		private Mock<IShellStatusBarService> _shellStatusBarServiceMock;
		private Mock<IShellReferenceService> _shellReferenceServiceMock;
		private Mock<IShellSelectionService> _shellSelectionServiceMock;

		#endregion

		#region Tests Setup

		[SetUp]
		public void Setup()
		{
			_serviceProviderMock = new Mock<IServiceProvider>();

			_logMock = new Mock<ILog>();
			_logMock.Setup(l => l.LogMessage(It.IsAny<string>(), It.IsAny<string>())).Verifiable();

			_optionsServiceMock = new Mock<IOptionsService>();

			_packageServiceMock = new Mock<IPackageService>();
			_packageServiceMock.Setup(p => p.GetResourceString(It.IsAny<int>())).Returns(string.Empty).Verifiable();

			_utilsServiceMock = new Mock<IUtilsService>();
			_utilsServiceMock.Setup(u => u.SetClipboardData(It.IsNotNull<string>())).Verifiable();

			_shellHelperServiceMock = new Mock<IShellHelperService>();
			_shellHelperServiceMock.Setup(s => s.CollapseAllProjects()).Verifiable();

			_shellStatusBarServiceMock = new Mock<IShellStatusBarService>();
			_shellStatusBarServiceMock.Setup(s => s.SetStatusBarText(It.IsNotNull<string>())).Verifiable();

			_shellReferenceServiceMock = new Mock<IShellReferenceService>();
			_shellReferenceServiceMock
				.Setup(s => s.GetProjectReferences(It.IsAny<bool>()))
				.Returns(new List<ProjectReference>
				{
					new ProjectReference { Name = "System", Path = @"C:\System.dll" },
					new ProjectReference { Name = "System.Data", Path = @"C:\System.Data.dll" },
					new ProjectReference { Name = "Test", Path = @"C:\test.dll", ReferencingProjectName = "Test" }
				})
				.Verifiable();
			_shellReferenceServiceMock.Setup(s => s.AddAssemblyReference(It.IsNotNull<string>())).Returns(true).Verifiable();
			_shellReferenceServiceMock.Setup(s => s.AddProjectReference(It.IsNotNull<string>())).Returns(true).Verifiable();

			_shellSelectionServiceMock = new Mock<IShellSelectionService>();
			_shellSelectionServiceMock.Setup(s => s.IsContextActive(ContextType.SolutionHasProjects)).Returns(true).Verifiable();
		}

		[TearDown]
		public void TearDown()
		{
			_serviceProviderMock = null;
			_logMock = null;
			_optionsServiceMock = null;
			_packageServiceMock = null;
			_utilsServiceMock = null;
			_shellHelperServiceMock = null;
			_shellStatusBarServiceMock = null;
			_shellReferenceServiceMock = null;
			_shellSelectionServiceMock = null;
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Returns test feature instance.
		/// </summary>
		private IFeature GetFeature()
		{
			return new MiscellaneousFeature(_serviceProviderMock.Object, _logMock.Object, _optionsServiceMock.Object,
				_packageServiceMock.Object, _utilsServiceMock.Object, _shellHelperServiceMock.Object, _shellStatusBarServiceMock.Object,
				_shellReferenceServiceMock.Object, _shellSelectionServiceMock.Object);
		}

		#endregion

		#region Tests

		[Test]
		public void Initialize()
		{
			var feature = GetFeature();

			feature.Initialize();

			Assert.That(feature.Initialized, Is.True);
		}

		[Test]
		public void GetCommandIds()
		{
			var feature = GetFeature();

			var commands = feature.GetCommandIds();

			Assert.That(commands, Is.Not.Null);
			Assert.That(commands.Count, Is.EqualTo(7));
			Assert.That(commands, Contains.Item(CommandIDs.COLLAPSE_SOLUTION_CONTEXT));
			Assert.That(commands, Contains.Item(CommandIDs.COPY_REFERENCES_CONTEXT));
			Assert.That(commands, Contains.Item(CommandIDs.PASTE_REFERENCES_CONTEXT));
			Assert.That(commands, Contains.Item(CommandIDs.COPY_REFERENCE_CONTEXT));
			Assert.That(commands, Contains.Item(CommandIDs.LOCATE_IN_SOLUTION_EXPLORER_CONTEXT));
			Assert.That(commands, Contains.Item(CommandIDs.COMMAND_PROMPT));
			Assert.That(commands, Contains.Item(CommandIDs.COPY_FULL_PATH));
		}

		[TestCase(true)]
		[TestCase(false)]
		public void IsValidContext_CollapseSolution(bool contextActive)
		{
			var feature = GetFeature();

			_shellSelectionServiceMock.Setup(s => s.IsContextActive(ContextType.SolutionHasProjects)).Returns(contextActive).Verifiable();

			var result = feature.IsValidContext(CommandIDs.COLLAPSE_SOLUTION_CONTEXT);

			if (contextActive)
				Assert.That(result, Is.True);
			else
				Assert.That(result, Is.False);
			_shellSelectionServiceMock.Verify(s => s.IsContextActive(ContextType.SolutionHasProjects));
		}

		[TestCase(CommandIDs.COPY_REFERENCES_CONTEXT, true)]
		[TestCase(CommandIDs.PASTE_REFERENCES_CONTEXT, true)]
		[TestCase(CommandIDs.COPY_REFERENCE_CONTEXT, true)]
		[TestCase(CommandIDs.LOCATE_IN_SOLUTION_EXPLORER_CONTEXT, true)]
		[TestCase(CommandIDs.COMMAND_PROMPT, true)]
		[TestCase(CommandIDs.COPY_FULL_PATH, true)]
		[TestCase(0, false)]
		public void IsValidContext(int commandId, bool expectedResult)
		{
			var feature = GetFeature();

			var result = feature.IsValidContext(commandId);

			Assert.That(result, Is.EqualTo(expectedResult));
		}

		[Test]
		public void Execute_CollapseSolution()
		{
			var feature = GetFeature();

			var result = feature.Execute(CommandIDs.COLLAPSE_SOLUTION_CONTEXT);

			Assert.That(result, Is.True);
			_shellHelperServiceMock.Verify(s => s.CollapseAllProjects());
		}

		[TestCase(CommandIDs.COPY_REFERENCES_CONTEXT)]
		[TestCase(CommandIDs.COPY_REFERENCE_CONTEXT)]
		public void Execute_CopyReferences(int commandId)
		{
			var feature = GetFeature();

			var result = feature.Execute(commandId);

			Assert.That(result, Is.True);
			_shellReferenceServiceMock.Verify(s => s.GetProjectReferences(It.IsAny<bool>()));
			_utilsServiceMock.Verify(u => u.SetClipboardData(It.IsNotNull<string>()));
			_shellStatusBarServiceMock.Verify(s => s.SetStatusBarText(It.IsNotNull<string>()));
		}

		[Test]
		public void Execute_PasteReferences()
		{
			var feature = GetFeature();

			string data = @"c:\system.dll" + Environment.NewLine + @"c:\system.data.dll" + Environment.NewLine + @"Project=Test.csproj|c:\test.csproj";
			_utilsServiceMock.Setup(u => u.GetClipboardData(out data)).Returns(true).Verifiable();

			var result = feature.Execute(CommandIDs.PASTE_REFERENCES_CONTEXT);

			Assert.That(result, Is.True);
			_utilsServiceMock.Verify(u => u.GetClipboardData(out data));
			_shellReferenceServiceMock.Verify(s => s.AddAssemblyReference(It.IsNotNull<string>()), Times.Exactly(2));
			_shellReferenceServiceMock.Verify(s => s.AddProjectReference(It.IsNotNull<string>()), Times.Once);
			_shellStatusBarServiceMock.Verify(s => s.SetStatusBarText(It.IsNotNull<string>()));
		}

		[Test]
		public void Execute_PasteReferences_NoClipboardData()
		{
			var feature = GetFeature();

			string data = "test";
			_utilsServiceMock.Setup(u => u.GetClipboardData(out data)).Returns(false).Verifiable();

			var result = feature.Execute(CommandIDs.PASTE_REFERENCES_CONTEXT);

			Assert.That(result, Is.False);
			_utilsServiceMock.Verify(u => u.GetClipboardData(out data));
			_shellReferenceServiceMock.Verify(s => s.AddAssemblyReference(It.IsNotNull<string>()), Times.Never);
			_shellReferenceServiceMock.Verify(s => s.AddProjectReference(It.IsNotNull<string>()), Times.Never);
			_shellStatusBarServiceMock.Verify(s => s.SetStatusBarText(It.IsNotNull<string>()), Times.Never);
		}

		[Test]
		public void Execute_PasteReferences_ClipboardDataEmpty()
		{
			var feature = GetFeature();

			string data = string.Empty;
			_utilsServiceMock.Setup(u => u.GetClipboardData(out data)).Returns(true).Verifiable();

			var result = feature.Execute(CommandIDs.PASTE_REFERENCES_CONTEXT);

			Assert.That(result, Is.False);
			_utilsServiceMock.Verify(u => u.GetClipboardData(out data));
			_shellReferenceServiceMock.Verify(s => s.AddAssemblyReference(It.IsNotNull<string>()), Times.Never);
			_shellReferenceServiceMock.Verify(s => s.AddProjectReference(It.IsNotNull<string>()), Times.Never);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void Execute_LocateInSolutionExplorer(bool found)
		{
			var feature = GetFeature();

			var document = string.Empty;
			_shellHelperServiceMock.Setup(s => s.SelectSolutionExplorerDocument(out document)).Returns(found).Verifiable();

			var result = feature.Execute(CommandIDs.LOCATE_IN_SOLUTION_EXPLORER_CONTEXT);

			if (found)
			{
				Assert.That(result, Is.True);
				_shellStatusBarServiceMock.Verify(s => s.SetStatusBarText(It.IsNotNull<string>()), Times.Never);
			}
			else
			{
				Assert.That(result, Is.False);
				_shellStatusBarServiceMock.Verify(s => s.SetStatusBarText(It.IsNotNull<string>()));
			}
			_shellHelperServiceMock.Verify(s => s.SelectSolutionExplorerDocument(out document));
		}

		[Test(Description = "More abstraction need to be applied to the feature in order for command execution is not to fail here")]
		public void Execute_OpenCommandPrompt()
		{
			var feature = GetFeature();

			_shellHelperServiceMock.Setup(s => s.GetSelectedItemPath()).Returns(TestContext.CurrentContext.TestDirectory).Verifiable();
			_utilsServiceMock.Setup(u => u.ControlKeyDown()).Returns(false).Verifiable();

			var result = feature.Execute(CommandIDs.COMMAND_PROMPT);

			Assert.That(result, Is.False);
			_shellHelperServiceMock.Verify(s => s.GetSelectedItemPath());
			_utilsServiceMock.Verify(u => u.ControlKeyDown(), Times.Never);
		}

		[TestCase("")]
		[TestCase(null)]
		[TestCase("test")]
		public void Execute_CopyProjectFullPath(string path)
		{
			var feature = GetFeature();

			_shellHelperServiceMock.Setup(s => s.GetCurrentProjectPath()).Returns(path).Verifiable();
			_utilsServiceMock.Setup(u => u.SetClipboardData(It.IsNotNull<string>())).Verifiable();

			var result = feature.Execute(CommandIDs.COPY_FULL_PATH);

			if (!string.IsNullOrEmpty(path))
			{
				Assert.That(result, Is.True);
				_utilsServiceMock.Verify(u => u.SetClipboardData(path));
				_shellStatusBarServiceMock.Verify(s => s.SetStatusBarText(path));
			}
			else
			{
				Assert.That(result, Is.False);
				_utilsServiceMock.Verify(u => u.SetClipboardData(It.IsAny<string>()), Times.Never);
				_shellStatusBarServiceMock.Verify(s => s.SetStatusBarText(It.IsNotNull<string>()));
			}
			_shellHelperServiceMock.Verify(s => s.GetCurrentProjectPath());
		}

		#endregion
	}
}