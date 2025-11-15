using Microsoft.UI.Xaml.Data;
using QuickCode.Components.Data;
using System;
using Windows.ApplicationModel.Resources;

namespace QuickCode.Components.Converters
{
    public class EnumLocalizerConverter : IValueConverter
    {
        private readonly ResourceLoader _resourceLoader = new();
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is LinearGradientDirection direction) return GetText(direction);
            else return value;
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
        private string GetText(LinearGradientDirection direction)
        {
            switch (direction)
            {
                case LinearGradientDirection.LeftToRight: return _resourceLoader.GetString("LinearGradientDirection_LeftToRight");
                case LinearGradientDirection.TopToBottom: return _resourceLoader.GetString("LinearGradientDirection_TopToBottom");
                case LinearGradientDirection.TopLeftToBottomRight: return _resourceLoader.GetString("LinearGradientDirection_TopLeftToBottomRight");
                case LinearGradientDirection.BottomLeftToTopRight: return _resourceLoader.GetString("LinearGradientDirection_BottomLeftToTopRight");
                default: throw new NotImplementedException();
            }
        }
    }
}
