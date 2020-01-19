using System;
using System.ComponentModel.Design;

using DPackRx.Services;

using Moq;

using NUnit.Framework;

namespace DPackRx.Tests.Services
{
	/// <summary>
	/// SharedServiceProvider tests.
	/// </summary>
	[TestFixture]
	public class SharedServiceProviderTests
	{
		#region Fields

		private Mock<IServiceContainer> _mefContainerMock;
		private Mock<LightInject.IServiceContainer> _diContainerMock;

		#endregion

		#region Test1 class

		private class Test1
		{
		}

		#endregion

		#region Test2 class

		private class Test2
		{
		}

		#endregion

		#region Test3 class

		private class Test3
		{
		}

		#endregion

		#region Tests Setup

		[SetUp]
		public void Setup()
		{
			_mefContainerMock = new Mock<IServiceContainer>();
			_mefContainerMock.Setup(c => c.GetService(typeof(Test1))).Returns(new Test1()).Verifiable();

			_diContainerMock = new Mock<LightInject.IServiceContainer>();
			_diContainerMock.Setup(c => c.TryGetInstance(typeof(Test2))).Returns(new Test2()).Verifiable();
		}

		[TearDown]
		public void TearDown()
		{
			_mefContainerMock = null;
			_diContainerMock = null;
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Returns test service instance.
		/// </summary>
		private ISharedServiceProvider GetProvider()
		{
			return new SharedServiceProvider();
		}

		#endregion

		#region Tests

		[Test]
		public void Initialize()
		{
			var service = GetProvider();
			var eventCalled = false;
			service.Initialized += (sender, e) => { eventCalled = true; };
			var initialized = service.IsInitialized;

			service.Initialize(_mefContainerMock.Object, _diContainerMock.Object);

			Assert.That(initialized, Is.False);
			Assert.That(service.IsInitialized, Is.True);
			Assert.That(eventCalled, Is.True);

			((IDisposable)service).Dispose();

			Assert.That(service.IsInitialized, Is.False);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void GetService(bool initialize)
		{
			var service = GetProvider();
			if (initialize)
				service.Initialize(_mefContainerMock.Object, _diContainerMock.Object);

			var instance1 = service.GetService(typeof(Test1));
			var instance2 = service.GetService(typeof(Test2));
			var instance3 = service.GetService(typeof(Test3));

			if (initialize)
			{
				Assert.That(instance1, Is.Not.Null);
				Assert.That(instance2, Is.Not.Null);
			}
			else
			{
				Assert.That(instance1, Is.Null);
				Assert.That(instance2, Is.Null);
			}
			Assert.That(instance3, Is.Null);
		}

		[Test]
		public void GetService_Uninitialized()
		{
			var service = GetProvider();

			var instance = service.GetService(typeof(Test1));

			Assert.That(instance, Is.Null);

			_diContainerMock.Verify(c => c.TryGetInstance(It.IsAny<Type>()), Times.Never);
			_mefContainerMock.Verify(c => c.GetService(It.IsAny<Type>()), Times.Never);
		}

		[TestCase(typeof(Test1), false, true)]
		[TestCase(typeof(Test2), true, true)]
		[TestCase(typeof(Test3), false, false)]
		public void GetService_Order(Type type, bool firstContainerOnly, bool expectedResult)
		{
			var service = GetProvider();
			service.Initialize(_mefContainerMock.Object, _diContainerMock.Object);

			var instance = service.GetService(type);

			if (expectedResult)
			{
				Assert.That(instance, Is.Not.Null);

				_diContainerMock.Verify(c => c.TryGetInstance(type));
				if (firstContainerOnly)
					_mefContainerMock.Verify(c => c.GetService(type), Times.Never);
				else
					_mefContainerMock.Verify(c => c.GetService(type));
			}
			else
			{
				Assert.That(instance, Is.Null);

				_diContainerMock.Verify(c => c.TryGetInstance(type));
				_mefContainerMock.Verify(c => c.GetService(type));
			}
		}

		#endregion
	}
}