using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xaml;

using DPackRx.CodeModel;
using DPackRx.Extensions;
using DPackRx.Services;

namespace DPackRx.UI.Converters
{
	/// <summary>
	/// Converts <see cref="MemberCodeModel"/> to <see cref="ImageSource"/> type.
	/// </summary>
	[ValueConversion(typeof(MemberCodeModel), typeof(ImageSource))]
	public class FileCodeModelToImageConverter : MarkupExtension, IValueConverter
	{
		#region Fields

		private IServiceProvider _serviceProvider;
		private IImageService _imageService;

		#endregion

		#region MarkupExtension Overrides

		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
			return this;
		}

		#endregion

		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is MemberCodeModel))
				return null;

			var imageService = GetImageService();
			if (imageService == null)
				return null;

			var member = (MemberCodeModel)value;
			return imageService.GetMemberImage(member.ElementModifier, member.ElementKind, member.IsStatic);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Returns image service.
		/// </summary>
		private IImageService GetImageService()
		{
			if ((_serviceProvider != null) && (_imageService == null))
			{
				// HACK: not sure what's the best way to handle DI for converters
				_imageService = (((_serviceProvider as IRootObjectProvider)?
					.RootObject as UserControl)?
					.DataContext as ViewModelBase)?
					.ServiceProvider?
					.GetService<IImageService>(false);
			}

			return _imageService;
		}

		#endregion
	}
}