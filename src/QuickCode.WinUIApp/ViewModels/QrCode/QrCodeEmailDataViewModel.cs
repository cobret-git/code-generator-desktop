using CommunityToolkit.Mvvm.ComponentModel;
using System;
using Windows.ApplicationModel.Resources;

namespace QuickCode.ViewModels
{
    public class QrCodeEmailDataViewModel : ObservableObject, IQrCodeDataViewModel
    {
        #region Fields
        private string address = string.Empty;
        private string subject = string.Empty;
        private string message = string.Empty;
        #endregion

        #region Constructors
        public QrCodeEmailDataViewModel()
        {
            var resourceLoader = new ResourceLoader();
            Header = resourceLoader.GetString("QrCodeEmail_Header");
            Description = resourceLoader.GetString("QrCodeEmail_Description");
            IconGlyph = "&#xE715;";
        }
        #endregion

        #region Events
        public event EventHandler<string?>? RawDataReceived;
        #endregion

        #region Properties
        public string Header { get; }
        public string Description { get; }
        public string IconGlyph { get; }
        public string Address { get => address; set { address = value; OnPropertyChanged(); OnFieldChanged(); } }
        public string Subject { get => subject; set { subject = value; OnPropertyChanged(); OnFieldChanged(); } }
        public string Message { get => message; set { message = value; OnPropertyChanged(); OnFieldChanged(); } }
        #endregion

        #region Handlers
        private void OnFieldChanged()
        {
            if (string.IsNullOrWhiteSpace(Address) 
                || string.IsNullOrWhiteSpace(Subject) 
                || string.IsNullOrWhiteSpace(Message))
            {
                RawDataReceived?.Invoke(this, null);
                return;
            }
            var rawText = $"MATMSG:TO:{Address};SUB:{Subject};BODY:{Message};;";
            RawDataReceived?.Invoke(this, rawText);
        }
        #endregion
    }
}
