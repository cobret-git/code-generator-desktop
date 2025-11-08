using CommunityToolkit.Mvvm.ComponentModel;
using System;
using Windows.ApplicationModel.Resources;

namespace QuickCode.ViewModels
{
    public class QrCodeSmsDataViewModel : ObservableObject, IQrCodeDataViewModel
    {
        #region Fields
        private string phoneNumber = string.Empty;
        private string smsText = string.Empty;
        #endregion

        #region Constructors
        public QrCodeSmsDataViewModel()
        {
            var resourceLoader = new ResourceLoader();
            Header = resourceLoader.GetString("QrCodeSms.Header");
            Description = resourceLoader.GetString("QrCodeSms.Description");
            IconGlyph = "&#xE8BD;";
        }
        #endregion

        #region Events
        public event EventHandler<string?>? RawDataReceived;
        #endregion

        #region Properties
        public string Header { get; }
        public string Description { get; }
        public string IconGlyph { get; }
        public string PhoneNumber { get => phoneNumber; set { phoneNumber = value; OnPhoneNumberChanged(); } }
        public string SmsText { get => smsText; set { smsText = value; OnSmsTextChanged(); } }
        #endregion

        #region Handlers
        private void OnPhoneNumberChanged()
        {
            SendRawData();
            OnPropertyChanged(nameof(PhoneNumber));
        }
        private void OnSmsTextChanged()
        {
            SendRawData();
            OnPropertyChanged(nameof(SmsText));
        }
        #endregion

        #region Helpers
        private void SendRawData()
        {
            if (string.IsNullOrWhiteSpace(PhoneNumber) || string.IsNullOrWhiteSpace(SmsText))
            {
                RawDataReceived?.Invoke(this, null);
                return;
            }
            RawDataReceived?.Invoke(this, $"smsto:{phoneNumber}:{SmsText}");
        }
        #endregion
    }
}
