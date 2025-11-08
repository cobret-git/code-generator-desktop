using CommunityToolkit.Mvvm.ComponentModel;
using System;
using Windows.ApplicationModel.Resources;

namespace QuickCode.ViewModels
{
    public class QrCodeCallDataViewModel : ObservableObject, IQrCodeDataViewModel
    {
        #region Fields
        private string phoneNumber = string.Empty;
        #endregion

        #region Constructors
        public QrCodeCallDataViewModel()
        {
            var resourceLoader = new ResourceLoader();
            Header = resourceLoader.GetString("CallQrCode.Header");
            Description = resourceLoader.GetString("CallQrCode.Description");
            IconGlyph = "&#xE717;";
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
        #endregion

        #region Handlers
        private void OnSelectedCodeChanged()
        {
            SendRawData();
        }
        private void OnPhoneNumberChanged()
        {
            SendRawData();
            OnPropertyChanged(nameof(PhoneNumber));
        }
        #endregion

        #region Helpers
        private void SendRawData()
        {
            if (string.IsNullOrWhiteSpace(PhoneNumber))
            {
                RawDataReceived?.Invoke(this, null);
                return;
            }
            RawDataReceived?.Invoke(this, $"tel:{phoneNumber}");
        }
        #endregion
    }
}
