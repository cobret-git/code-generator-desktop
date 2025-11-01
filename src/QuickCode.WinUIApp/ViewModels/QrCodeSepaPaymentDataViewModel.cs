using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Globalization;
using System.Linq;

namespace QuickCode.ViewModels
{
    public class QrCodeSepaPaymentDataViewModel : ObservableObject, IQrCodeDataViewModel
    {
        #region Fields
        private string name = string.Empty;
        private string iban = string.Empty;
        private string bic = string.Empty;
        private string amount = string.Empty;
        private string purpose = string.Empty;
        private string paymentReference = string.Empty;
        private string remittanceInformation = string.Empty;
        private string note = string.Empty;
        #endregion

        #region Events
        public event EventHandler<string?>? RawDataReceived;
        #endregion

        #region Properties
        public string Name { get => name; set { name = value; OnPropertyChanged(); OnFieldChanged(); } }
        public string Iban { get => iban; set { iban = value; OnPropertyChanged(); OnFieldChanged(); } }
        public string Bic { get => bic; set { bic = value; OnPropertyChanged(); OnFieldChanged(); } }
        public string Amount { get => amount; set { amount = value; OnPropertyChanged(); OnFieldChanged(); } }
        public string Purpose { get => purpose; set { purpose = value; OnPropertyChanged(); OnFieldChanged(); } }
        public string PaymentReference { get => paymentReference; set { paymentReference = value; OnPropertyChanged(); OnFieldChanged(); } }
        public string RemittanceInformation { get => remittanceInformation; set { remittanceInformation = value; OnPropertyChanged(); OnFieldChanged(); } }
        public string Note { get => note; set { note = value; OnPropertyChanged(); OnFieldChanged(); } }
        #endregion

        #region Handlers
        private void OnFieldChanged()
        {
            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Iban))
            {
                RawDataReceived?.Invoke(this, null);
                return;
            }

            var sanitizedName = Name.Trim();
            var sanitizedIban = NormalizeIban(Iban);

            if (string.IsNullOrWhiteSpace(sanitizedName) || string.IsNullOrWhiteSpace(sanitizedIban))
            {
                RawDataReceived?.Invoke(this, null);
                return;
            }

            var sanitizedPurpose = Purpose?.Trim() ?? string.Empty;
            var sanitizedReference = PaymentReference?.Trim() ?? string.Empty;
            var sanitizedRemittance = RemittanceInformation?.Trim() ?? string.Empty;
            if (!string.IsNullOrEmpty(sanitizedReference))
            {
                sanitizedRemittance = string.Empty;
            }

            var sepaLines = new string[]
            {
                "BCD",
                "001",
                "1",
                "SCT",
                NormalizeBic(Bic),
                sanitizedName,
                sanitizedIban,
                BuildAmount(Amount),
                sanitizedPurpose,
                sanitizedReference,
                sanitizedRemittance,
                Note?.Trim() ?? string.Empty
            };

            RawDataReceived?.Invoke(this, string.Join('\n', sepaLines));
        }
        #endregion

        #region Helpers
        private static string NormalizeIban(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }
            var cleaned = new string(value.Where(c => !char.IsWhiteSpace(c)).ToArray());
            return cleaned.ToUpperInvariant();
        }

        private static string NormalizeBic(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }
            var cleaned = new string(value.Where(c => !char.IsWhiteSpace(c)).ToArray());
            return cleaned.ToUpperInvariant();
        }

        private static string BuildAmount(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var trimmedValue = value.Trim();
            var normalizedValue = trimmedValue.Replace(',', '.');

            if (decimal.TryParse(normalizedValue, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount) && amount > 0)
            {
                return $"EUR{amount:0.00}";
            }

            if (decimal.TryParse(trimmedValue, NumberStyles.Number, CultureInfo.CurrentCulture, out amount) && amount > 0)
            {
                return $"EUR{amount:0.00}";
            }

            return string.Empty;
        }
        #endregion
    }
}
