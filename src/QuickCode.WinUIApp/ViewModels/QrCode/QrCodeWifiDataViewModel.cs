using CommunityToolkit.Mvvm.ComponentModel;
using QuickCode.Components.Data;
using System;
using System.Text;
using Windows.ApplicationModel.Resources;

namespace QuickCode.ViewModels
{
    public class QrCodeWifiDataViewModel : ObservableObject, IQrCodeDataViewModel
    {
        #region Fields
        private string ssid = string.Empty;
        private WifiEncryptionType selectedEncryption;
        private string password = string.Empty;
        private bool isHidden;
        #endregion

        #region Constructors
        public QrCodeWifiDataViewModel()
        {
            var resourceLoader = new ResourceLoader();
            Header = resourceLoader.GetString("QrCodeWifi.Header");
            Description = resourceLoader.GetString("QrCodeWifi.Description");
            IconGlyph = "&#xE701;";
            Encryptions = Enum.GetValues<WifiEncryptionType>();
            SelectedEncryption = WifiEncryptionType.WPA2;
        }
        #endregion

        #region Events
        public event EventHandler<string?>? RawDataReceived;
        #endregion

        #region Properties
        public string Header { get; }
        public string Description { get; }
        public string IconGlyph { get; }
        public WifiEncryptionType[] Encryptions { get; }
        public string Ssid { get => ssid; set { ssid = value ?? string.Empty; OnPropertyChanged(); SendRawData(); } }
        public WifiEncryptionType SelectedEncryption { get => selectedEncryption; set { selectedEncryption = value; OnPropertyChanged(); SendRawData(); } }
        public string Password { get => password; set { password = value ?? string.Empty; OnPropertyChanged(); SendRawData(); } }
        public bool IsHidden { get => isHidden; set { isHidden = value; OnPropertyChanged(); SendRawData(); } }
        #endregion

        #region Helpers
        private void SendRawData()
        {
            if (string.IsNullOrWhiteSpace(Ssid))
            {
                RawDataReceived?.Invoke(this, null);
                return;
            }

            if (SelectedEncryption != WifiEncryptionType.None && string.IsNullOrEmpty(Password))
            {
                RawDataReceived?.Invoke(this, null);
                return;
            }

            var builder = new StringBuilder("WIFI:");
            builder.Append($"T:{GetEncryptionToken(SelectedEncryption)};");
            builder.Append($"S:{EscapeValue(Ssid)};");

            if (SelectedEncryption != WifiEncryptionType.None && !string.IsNullOrEmpty(Password))
            {
                builder.Append($"P:{EscapeValue(Password)};");
            }

            if (IsHidden)
            {
                builder.Append("H:true;");
            }

            builder.Append(';');

            RawDataReceived?.Invoke(this, builder.ToString());
        }

        private static string EscapeValue(string value)
        {
            return value
                .Replace("\\", "\\\\")
                .Replace(";", "\\;")
                .Replace(",", "\\,")
                .Replace(":", "\\:");
        }

        private static string GetEncryptionToken(WifiEncryptionType encryption)
        {
            return encryption switch
            {
                WifiEncryptionType.None => "nopass",
                WifiEncryptionType.WEP => "WEP",
                WifiEncryptionType.WPA => "WPA",
                WifiEncryptionType.WPA2 => "WPA2",
                _ => "nopass"
            };
        }
        #endregion
    }
}
