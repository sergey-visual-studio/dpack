using System;
using System.Threading;
using System.Windows;

using Moq;
using NUnit.Framework;

using DPackRx.Services;
using DPackRx.UI;

namespace DPackRx.Tests.Services
{
	/// <summary>
	/// ModalDialogService tests.
	/// </summary>
	[TestFixture]
	[RequiresThread(ApartmentState.STA)]
	public class ModalDialogServiceTests
	{
		#region Fields

		private TestViewModel _testViewModel;
		private Mock<IServiceProvider> _serviceProviderMock;
		private Mock<IAsyncTaskService> _asyncTaskServiceMock;

		#endregion

		#region TestViewModel class

		private class TestViewModel : ViewModelBase
		{
			public TestViewModel(IServiceProvider serviceProvider) : base(serviceProvider)
			{
			}

			public bool Closed { get; private set; }

			public override void OnInitialize(object argument)
			{
			}

			public override void OnClose(bool apply)
			{
				this.Closed = true;
			}
		}

		#endregion

		#region TestWindow class

		private class TestWindow : Window
		{
			public TestWindow()
			{
				// Make the test window as invisible as possible
				this.WindowStyle = WindowStyle.None;
				this.ShowInTaskbar = false;
				this.Visibility = Visibility.Hidden;
				this.Width = 1;
				this.Height = 1;
			}

			protected override void OnActivated(EventArgs e)
			{
				base.OnActivated(e);

				// Close the test window as soon as it's activated
				this.DialogResult = true;
				this.Close();
			}
		}

		#endregion

		#region Tests Setup

		[SetUp]
		public void Setup()
		{
			_serviceProviderMock = new Mock<IServiceProvider>();
			_testViewModel = new TestViewModel(_serviceProviderMock.Object);

			_serviceProviderMock.Setup(p => p.GetService(typeof(TestViewModel))).Returns(_testViewModel).Verifiable();
			_serviceProviderMock.Setup(p => p.GetService(typeof(TestWindow))).Returns(new TestWindow()).Verifiable();

			_asyncTaskServiceMock = new Mock<IAsyncTaskService>();
			_asyncTaskServiceMock.Setup(a => a.RunOnMainThread(It.IsAny<Action<object>>(), It.IsAny<object>(), It.IsAny<string>())).Returns(true).Verifiable();
		}

		[TearDown]
		public void TearDown()
		{
			_testViewModel = null;
			_serviceProviderMock = null;
			_asyncTaskServiceMock = null;
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Returns test service instance.
		/// </summary>
		private IModalDialogService GetService()
		{
			return new ModalDialogService(_serviceProviderMock.Object, _asyncTaskServiceMock.Object);
		}

		#endregion

		#region Tests

		[Test]
		public void ShowDialog()
		{
			var service = GetService();

			var result = service.ShowDialog<TestWindow, TestViewModel>("test", null);

			Assert.That(result, Is.True);
			Assert.That(_testViewModel.Closed, Is.True);
			_serviceProviderMock.Verify(p => p.GetService(typeof(TestViewModel)));
			_asyncTaskServiceMock.Verify(a => a.RunOnMainThread(It.IsAny<Action<object>>(), It.IsAny<object>(), It.IsAny<string>()));
		}

		[Test]
		public void ShowDialog_Cancelled()
		{
			_asyncTaskServiceMock.Setup(a => a.RunOnMainThread(It.IsAny<Action<object>>(), It.IsAny<object>(), It.IsAny<string>())).Returns(false).Verifiable();
			var service = GetService();

			var result = service.ShowDialog<TestWindow, TestViewModel>("test", null);

			Assert.That(result, Is.False);
			Assert.That(_testViewModel.Closed, Is.False);
			_serviceProviderMock.Verify(p => p.GetService(typeof(TestViewModel)));
			_asyncTaskServiceMock.Verify(a => a.RunOnMainThread(It.IsAny<Action<object>>(), It.IsAny<object>(), It.IsAny<string>()));
		}

		#endregion
	}
}