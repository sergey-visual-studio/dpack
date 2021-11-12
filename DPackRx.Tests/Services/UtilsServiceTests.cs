using System.Threading;

using DPackRx.Services;

using NUnit.Framework;

namespace DPackRx.Tests.Services
{
	/// <summary>
	/// UtilsService tests.
	/// </summary>
	[TestFixture]
	[NonParallelizable]
	public class UtilsServiceTests
	{
		#region Private Methods

		/// <summary>
		/// Returns test service instance.
		/// </summary>
		private IUtilsService GetService()
		{
			return new UtilsService();
		}

		#endregion

		#region Tests

#if !DEBUG
		[Test]
		public void Beep()
		{
			var service = GetService();

			Assert.DoesNotThrow(() => service.Beep());
		}
#endif

		[Test]
		public void ControlKeyDown()
		{
			var service = GetService();

			Assert.That(service.ControlKeyDown(), Is.False);
		}

		[Test]
		[RequiresThread(ApartmentState.STA)]
		public void SetClipboardData()
		{
			var service = GetService();

			Assert.DoesNotThrow(() => service.SetClipboardData("test"));
		}

		[Test]
		[RequiresThread(ApartmentState.STA)]
		public void GetClipboardData()
		{
			var service = GetService();

			service.SetClipboardData("test");
			var result = service.GetClipboardData(out string data);

			Assert.That(result, Is.True);
			Assert.That(data, Is.EqualTo("test"));
		}

		#endregion
	}
}