using System;
using System.Collections.Generic;

using Moq;
using NUnit.Framework;

using DPackRx.Features;
using DPackRx.Options;
using DPackRx.Services;

namespace DPackRx.Tests.Options
{
	/// <summary>
	/// OptionsService tests.
	/// </summary>
	[TestFixture]
	public class OptionsServiceTests
	{
		#region Fields

		private Mock<ILog> _logMock;
		private Mock<IOptionsPersistenceService> _optionsPersistenceServiceMock;

		#endregion

		#region Tests Setup

		[SetUp]
		public void Setup()
		{
			_logMock = new Mock<ILog>();
			_logMock.Setup(l => l.LogMessage(It.IsAny<string>(), It.IsAny<string>())).Verifiable();

			var options = new Dictionary<string, object> { { "test", 123 }, { "test_int", 456 }, { "test_bool", Convert.ToInt32(true) }, { "test_str", "test" } };
			var defaultOptions = new Dictionary<string, object> { { "test", 789 } };

			_optionsPersistenceServiceMock = new Mock<IOptionsPersistenceService>();
			_optionsPersistenceServiceMock.Setup(o => o.LoadOptions(KnownFeature.SupportOptions)).Returns(options).Verifiable();
			_optionsPersistenceServiceMock.Setup(o => o.LoadOptions(KnownFeature.Miscellaneous)).Returns(new Dictionary<string, object>()).Verifiable();
			_optionsPersistenceServiceMock.Setup(o => o.LoadDefaultOptions(KnownFeature.SupportOptions)).Returns(defaultOptions).Verifiable();
			_optionsPersistenceServiceMock.Setup(o => o.LoadDefaultOptions(KnownFeature.Miscellaneous)).Returns((Dictionary<string, object>)null).Verifiable();
			_optionsPersistenceServiceMock.Setup(o => o.SaveOptions(KnownFeature.SupportOptions, It.IsAny<IDictionary<string, object>>())).Verifiable();
			_optionsPersistenceServiceMock.Setup(o => o.SaveOptions(KnownFeature.Miscellaneous, It.IsAny<IDictionary<string, object>>())).Verifiable();
			_optionsPersistenceServiceMock.Setup(o => o.DeleteOptions(KnownFeature.SupportOptions)).Verifiable();
			_optionsPersistenceServiceMock.Setup(o => o.DeleteOptions(KnownFeature.Miscellaneous)).Verifiable();
		}

		[TearDown]
		public void TearDown()
		{
			_logMock = null;
			_optionsPersistenceServiceMock = null;
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Returns test service instance.
		/// </summary>
		private IOptionsService GetService()
		{
			return new OptionsService(_logMock.Object, _optionsPersistenceServiceMock.Object);
		}

		#endregion

		#region Tests

		[TestCase(KnownFeature.SupportOptions, 4)]
		[TestCase(KnownFeature.Miscellaneous, 0)]
		public void GetOptionCount(KnownFeature feature, int expectedCount)
		{
			var service = GetService();

			var countBeforeLoad = service.GetOptionCount(feature);
			service.Reload(feature);
			var countAfterLoad = service.GetOptionCount(feature);

			Assert.That(countAfterLoad, Is.EqualTo(expectedCount));
			_optionsPersistenceServiceMock.Verify(o => o.LoadOptions(feature), Times.Once);
			if (expectedCount == 0)
				_optionsPersistenceServiceMock.Verify(o => o.LoadDefaultOptions(feature), Times.Once);
			else
				_optionsPersistenceServiceMock.Verify(o => o.LoadDefaultOptions(feature), Times.Never);
		}

		[TestCase(KnownFeature.SupportOptions, "test", true, false)]
		[TestCase(KnownFeature.SupportOptions, "bad", false, false)]
		[TestCase(KnownFeature.Miscellaneous, "test", false, true)]
		public void OptionExists(KnownFeature feature, string name, bool expectedResult, bool expectedLoadDefaults)
		{
			var service = GetService();

			var result = service.OptionExists(feature, name);

			Assert.That(result, Is.EqualTo(expectedResult));
			_optionsPersistenceServiceMock.Verify(o => o.LoadOptions(feature));
			if (expectedLoadDefaults)
				_optionsPersistenceServiceMock.Verify(o => o.LoadDefaultOptions(feature));
			else
				_optionsPersistenceServiceMock.Verify(o => o.LoadDefaultOptions(feature), Times.Never);
		}

		[Test]
		public void OptionExists_NoOption()
		{
			var service = GetService();

			var result = service.OptionExists(KnownFeature.Miscellaneous, "test");

			Assert.That(result, Is.False);
		}

		[TestCase(null)]
		[TestCase("")]
		public void OptionExists_ErrorHandling(string name)
		{
			var service = GetService();

			Assert.Throws<ArgumentNullException>(() => service.OptionExists(KnownFeature.SupportOptions, name));
		}

		[TestCase("test", true)]
		[TestCase("bad", false)]
		public void DeleteOption(string name, bool expectedDelete)
		{
			var service = GetService();

			service.Reload(KnownFeature.SupportOptions);
			var countBeforeDelete = service.GetOptionCount(KnownFeature.SupportOptions);
			service.DeleteOption(KnownFeature.SupportOptions, name);
			var countAfterDelete = service.GetOptionCount(KnownFeature.SupportOptions);

			if (expectedDelete)
				Assert.That(countAfterDelete, Is.EqualTo(countBeforeDelete - 1));
			else
				Assert.That(countAfterDelete, Is.EqualTo(countBeforeDelete));
		}

		[TestCase(null)]
		[TestCase("")]
		public void DeleteOption_ErrorHandling(string name)
		{
			var service = GetService();

			Assert.Throws<ArgumentNullException>(() => service.DeleteOption(KnownFeature.SupportOptions, name));
		}

		[TestCase("test", 123, Description = "Feature default should not be applied here")]
		[TestCase("test_int", 456)]
		[TestCase("test_bool", true)]
		[TestCase("test_str", "test")]
		public void GetOption(string name, object expectedValue)
		{
			var service = GetService();

			service.Reload(KnownFeature.SupportOptions);

			var value = service.GetOption(KnownFeature.SupportOptions, name);

			if (expectedValue is bool)
				Assert.That(value, Is.EqualTo(Convert.ToInt32(expectedValue)));
			else
				Assert.That(value, Is.EqualTo(expectedValue));
		}

		[Test]
		public void GetOption_NoOption()
		{
			var service = GetService();

			var value = service.GetOption(KnownFeature.Miscellaneous, "test");

			Assert.That(value, Is.Null);
		}

		[TestCase(null)]
		[TestCase("")]
		public void GetOption_ErrorHandling(string name)
		{
			var service = GetService();

			Assert.Throws<ArgumentNullException>(() => service.GetOption(KnownFeature.SupportOptions, name));
		}

		[TestCase("test", 1234)]
		[TestCase("test_integer", 5678)]
		[TestCase("test_boolean", false)]
		[TestCase("test_string", "test")]
		public void SetOption(string name, object value)
		{
			var service = GetService();

			service.Reload(KnownFeature.SupportOptions);

			service.SetOption(KnownFeature.SupportOptions, name, value);
			var result = service.GetOption(KnownFeature.SupportOptions, name);

			if (value is bool)
				Assert.That(result, Is.EqualTo(Convert.ToInt32(value)));
			else
				Assert.That(result, Is.EqualTo(value));
		}

		[TestCase(null)]
		[TestCase("")]
		public void SetOption_ErrorHandling(string name)
		{
			var service = GetService();

			Assert.Throws<ArgumentNullException>(() => service.SetOption(KnownFeature.SupportOptions, name, "test"));
		}

		[TestCase("test_str", "test")]
		[TestCase("bad", "")]
		public void GetStringOption(string name, string expectedValue)
		{
			var service = GetService();

			service.Reload(KnownFeature.SupportOptions);

			var value = service.GetStringOption(KnownFeature.SupportOptions, name);

			Assert.That(value, Is.EqualTo(expectedValue));
		}

		[TestCase(null)]
		[TestCase("")]
		public void GetStringOption_ErrorHandling(string name)
		{
			var service = GetService();

			Assert.Throws<ArgumentNullException>(() => service.GetStringOption(KnownFeature.SupportOptions, name, "test"));
		}

		[Test]
		public void SetStringOption()
		{
			var service = GetService();

			service.Reload(KnownFeature.SupportOptions);

			service.SetStringOption(KnownFeature.SupportOptions, "test_string", "test");
			var result = service.GetStringOption(KnownFeature.SupportOptions, "test_string");

			Assert.That(result, Is.EqualTo("test"));
		}

		[TestCase(null)]
		[TestCase("")]
		public void SetStringOption_ErrorHandling(string name)
		{
			var service = GetService();

			Assert.Throws<ArgumentNullException>(() => service.SetStringOption(KnownFeature.SupportOptions, name, "test"));
		}

		[TestCase("test_int", 456)]
		[TestCase("bad", 0)]
		public void GetIntOption(string name, int expectedValue)
		{
			var service = GetService();

			service.Reload(KnownFeature.SupportOptions);

			var value = service.GetIntOption(KnownFeature.SupportOptions, name);

			Assert.That(value, Is.EqualTo(expectedValue));
		}

		[TestCase(null)]
		[TestCase("")]
		public void GetIntOption_ErrorHandling(string name)
		{
			var service = GetService();

			Assert.Throws<ArgumentNullException>(() => service.GetIntOption(KnownFeature.SupportOptions, name, 123));
		}

		[Test]
		public void SetIntOption()
		{
			var service = GetService();

			service.Reload(KnownFeature.SupportOptions);

			service.SetIntOption(KnownFeature.SupportOptions, "test_integer", 1234);
			var result = service.GetIntOption(KnownFeature.SupportOptions, "test_integer");

			Assert.That(result, Is.EqualTo(1234));
		}

		[TestCase(null)]
		[TestCase("")]
		public void SetIntOption_ErrorHandling(string name)
		{
			var service = GetService();

			Assert.Throws<ArgumentNullException>(() => service.SetIntOption(KnownFeature.SupportOptions, name, 123));
		}

		[TestCase("test_bool", true)]
		[TestCase("bad", false)]
		public void GetBoolOption(string name, bool expectedValue)
		{
			var service = GetService();

			service.Reload(KnownFeature.SupportOptions);

			var value = service.GetBoolOption(KnownFeature.SupportOptions, name);

			Assert.That(value, Is.EqualTo(expectedValue));
		}

		[TestCase(null)]
		[TestCase("")]
		public void GetBoolOption_ErrorHandling(string name)
		{
			var service = GetService();

			Assert.Throws<ArgumentNullException>(() => service.GetBoolOption(KnownFeature.SupportOptions, name, false));
		}

		[Test]
		public void SetBoolOption()
		{
			var service = GetService();

			service.Reload(KnownFeature.SupportOptions);

			service.SetBoolOption(KnownFeature.SupportOptions, "test_boolean", true);
			var result = service.GetBoolOption(KnownFeature.SupportOptions, "test_boolean");

			Assert.That(result, Is.EqualTo(true));
		}

		[TestCase(null)]
		[TestCase("")]
		public void SetBoolOption_ErrorHandling(string name)
		{
			var service = GetService();

			Assert.Throws<ArgumentNullException>(() => service.SetBoolOption(KnownFeature.SupportOptions, name, true));
		}

		[Test]
		public void Flush()
		{
			var service = GetService();
			var changed = false;
			service.Changed += (sender, args) => { changed = true; };

			service.Reload(KnownFeature.SupportOptions);

			service.Flush(KnownFeature.SupportOptions);

			Assert.That(changed, Is.True);
			_optionsPersistenceServiceMock.Verify(o => o.SaveOptions(KnownFeature.SupportOptions, It.IsAny<IDictionary<string, object>>()));
			_logMock.Verify(l => l.LogMessage(It.IsNotNull<string>(), It.IsNotNull<string>()));
		}

		[Test]
		public void Flush_NotSaved()
		{
			var service = GetService();
			var changed = false;
			service.Changed += (sender, args) => { changed = true; };

			service.Flush(KnownFeature.Miscellaneous);

			Assert.That(changed, Is.False);
			_optionsPersistenceServiceMock.Verify(o => o.SaveOptions(KnownFeature.SupportOptions, It.IsAny<IDictionary<string, object>>()), Times.Never);
		}

		[Test]
		public void Reload()
		{
			var service = GetService();
			var changed = false;
			service.Changed += (sender, args) => { changed = true; };

			var countBeforeLoad = service.GetOptionCount(KnownFeature.SupportOptions);
			service.Reload(KnownFeature.SupportOptions);
			var countAfterLoad = service.GetOptionCount(KnownFeature.SupportOptions);

			Assert.That(countBeforeLoad, Is.Zero);
			Assert.That(countAfterLoad, Is.GreaterThan(countBeforeLoad));
			_optionsPersistenceServiceMock.Verify(o => o.LoadOptions(KnownFeature.SupportOptions), Times.Once);
			_optionsPersistenceServiceMock.Verify(o => o.LoadDefaultOptions(KnownFeature.SupportOptions), Times.Never);
			Assert.That(changed, Is.True);
		}

		[Test]
		public void Reload_LoadDefaults()
		{
			var service = GetService();

			service.Reload(KnownFeature.Miscellaneous);

			_optionsPersistenceServiceMock.Verify(o => o.LoadOptions(KnownFeature.Miscellaneous), Times.Once);
			_optionsPersistenceServiceMock.Verify(o => o.LoadDefaultOptions(KnownFeature.Miscellaneous), Times.Once);
		}

		[Test]
		public void ResetAll()
		{
			var service = GetService();

			service.Reload(KnownFeature.SupportOptions);

			_optionsPersistenceServiceMock.Reset();
			var changed = false;
			var reset = false;
			service.Changed += (sender, args) => { changed = true; };
			service.Reset += (sender, args) => { reset = true; };
			_optionsPersistenceServiceMock.Setup(o => o.LoadDefaultOptions(KnownFeature.SupportOptions)).Returns(new Dictionary<string, object> { { "test", 789 } }).Verifiable();

			service.ResetAll();

			_optionsPersistenceServiceMock.Verify(o => o.LoadOptions(KnownFeature.SupportOptions), Times.Never);
			_optionsPersistenceServiceMock.Verify(o => o.LoadDefaultOptions(KnownFeature.SupportOptions), Times.Once);
			_optionsPersistenceServiceMock.Verify(o => o.DeleteOptions(KnownFeature.SupportOptions), Times.Once);
			_logMock.Verify(l => l.LogMessage(It.IsNotNull<string>(), It.IsNotNull<string>()));
			Assert.That(changed, Is.True);
			Assert.That(reset, Is.True);
			Assert.That(service.GetOption(KnownFeature.SupportOptions, "test"), Is.EqualTo(789));
			Assert.That(service.GetOption(KnownFeature.SupportOptions, "test_str"), Is.Null);
		}

		#endregion
	}
}