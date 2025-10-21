using Microsoft.UI.Xaml.Data;
using Microsoft.Windows.ApplicationModel.Resources;
using QuickCode.Components.Data;
using System;
using Windows.ApplicationModel.Resources;

namespace QuickCode.Components.Converters
{
    public class EnumToLocalizedTextConverter : IValueConverter
    {
        #region Fields
        private readonly Microsoft.Windows.ApplicationModel.Resources.ResourceLoader resourceLoader = new();
        #endregion

        #region Methods
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is CodeGenerationType codeGenerationType) return GetLocalizedText(codeGenerationType);
            else return value;
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        private string GetLocalizedText(CodeGenerationType value)
        {
            switch (value)
            {
                case CodeGenerationType.Link: return resourceLoader.GetString("CodeGenerationType_Link");
                case CodeGenerationType.Text: return resourceLoader.GetString("CodeGenerationType_Text"); 
                case CodeGenerationType.Email: return resourceLoader.GetString("CodeGenerationType_Email"); 
                case CodeGenerationType.Call: return resourceLoader.GetString("CodeGenerationType_Call"); 
                case CodeGenerationType.Sms: return resourceLoader.GetString("CodeGenerationType_Sms"); 
                case CodeGenerationType.VCard: return resourceLoader.GetString("CodeGenerationType_VCard"); 
                case CodeGenerationType.WhatsApp: return resourceLoader.GetString("CodeGenerationType_WhatsApp"); 
                case CodeGenerationType.Wifi: return resourceLoader.GetString("CodeGenerationType_Wifi"); 
                case CodeGenerationType.Pdf: return resourceLoader.GetString("CodeGenerationType_Pdf"); 
                case CodeGenerationType.App: return resourceLoader.GetString("CodeGenerationType_App"); 
                case CodeGenerationType.Image: return resourceLoader.GetString("CodeGenerationType_Image"); 
                case CodeGenerationType.Video: return resourceLoader.GetString("CodeGenerationType_Video"); 
                case CodeGenerationType.SocialMedia: return resourceLoader.GetString("CodeGenerationType_SocialMedia"); 
                case CodeGenerationType.Event: return resourceLoader.GetString("CodeGenerationType_Event");
                default: throw new NotImplementedException($"No implementation for such value: {value}");
            }
        }
        #endregion
    }
}
