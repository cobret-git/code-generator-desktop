using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Text;
using Windows.ApplicationModel.Resources;

namespace QuickCode.ViewModels
{
    public class QrCodeVcardDataViewModel : ObservableObject, IQrCodeDataViewModel
    {
        #region Fields
        private string firstName = string.Empty;
        private string lastName = string.Empty;
        private string phoneNumber = string.Empty;
        private string mobile = string.Empty;
        private string email = string.Empty;
        private string websiteUrl = string.Empty;
        private string companyName = string.Empty;
        private string jobTitle = string.Empty;
        private string fax = string.Empty;
        private string address = string.Empty;
        private string city = string.Empty;
        private string postCode = string.Empty;
        private string county = string.Empty;
        #endregion

        #region Constructors
        public QrCodeVcardDataViewModel()
        {
            var resourceLoader = new ResourceLoader();
            Header = resourceLoader.GetString("QrCodeVcard.Header");
            Description = resourceLoader.GetString("QrCodeVcard.Description");
            IconGlyph = "&#xEA4A;";
        }
        #endregion

        #region Events
        public event EventHandler<string?>? RawDataReceived;
        #endregion

        #region Properties
        public string Header { get; }
        public string Description { get; }
        public string IconGlyph { get; }
        public string FirstName { get => firstName; set { firstName = value ?? string.Empty; OnPropertyChanged(); OnFieldChanged(); } }
        public string LastName { get => lastName; set { lastName = value ?? string.Empty; OnPropertyChanged(); OnFieldChanged(); } }
        public string PhoneNumber { get => phoneNumber; set { phoneNumber = value ?? string.Empty; OnPropertyChanged(); OnFieldChanged(); } }
        public string Mobile { get => mobile; set { mobile = value ?? string.Empty; OnPropertyChanged(); OnFieldChanged(); } }
        public string Email { get => email; set { email = value ?? string.Empty; OnPropertyChanged(); OnFieldChanged(); } }
        public string WebsiteUrl { get => websiteUrl; set { websiteUrl = value ?? string.Empty; OnPropertyChanged(); OnFieldChanged(); } }
        public string CompanyName { get => companyName; set { companyName = value ?? string.Empty; OnPropertyChanged(); OnFieldChanged(); } }
        public string JobTitle { get => jobTitle; set { jobTitle = value ?? string.Empty; OnPropertyChanged(); OnFieldChanged(); } }
        public string Fax { get => fax; set { fax = value ?? string.Empty; OnPropertyChanged(); OnFieldChanged(); } }
        public string Address { get => address; set { address = value ?? string.Empty; OnPropertyChanged(); OnFieldChanged(); } }
        public string City { get => city; set { city = value ?? string.Empty; OnPropertyChanged(); OnFieldChanged(); } }
        public string PostCode { get => postCode; set { postCode = value ?? string.Empty; OnPropertyChanged(); OnFieldChanged(); } }
        public string County { get => county; set { county = value ?? string.Empty; OnPropertyChanged(); OnFieldChanged(); } }
        #endregion

        #region Helpers
        private void OnFieldChanged()
        {
            if (AreAllFieldsEmpty())
            {
                RawDataReceived?.Invoke(this, null);
                return;
            }

            var builder = new StringBuilder();
            builder.AppendLine("BEGIN:VCARD");
            builder.AppendLine("VERSION:3.0");

            AppendName(builder);
            AppendCompanyInformation(builder);
            AppendContactInformation(builder);
            AppendLocation(builder);

            builder.Append("END:VCARD");

            RawDataReceived?.Invoke(this, builder.ToString());
        }

        private void AppendName(StringBuilder builder)
        {
            if (string.IsNullOrWhiteSpace(FirstName) && string.IsNullOrWhiteSpace(LastName))
            {
                return;
            }

            builder.AppendLine($"N:{Escape(LastName)};{Escape(FirstName)};;;");

            var formattedName = $"{FirstName} {LastName}".Trim();
            if (!string.IsNullOrWhiteSpace(formattedName))
            {
                builder.AppendLine($"FN:{Escape(formattedName)}");
            }
        }

        private void AppendCompanyInformation(StringBuilder builder)
        {
            if (!string.IsNullOrWhiteSpace(CompanyName))
            {
                builder.AppendLine($"ORG:{Escape(CompanyName)}");
            }

            if (!string.IsNullOrWhiteSpace(JobTitle))
            {
                builder.AppendLine($"TITLE:{Escape(JobTitle)}");
            }
        }

        private void AppendContactInformation(StringBuilder builder)
        {
            AppendTelephoneLine(builder, PhoneNumber, "work,voice");
            AppendTelephoneLine(builder, Mobile, "cell");
            AppendTelephoneLine(builder, Fax, "fax");

            if (!string.IsNullOrWhiteSpace(Email))
            {
                builder.AppendLine($"EMAIL;TYPE=internet:{Escape(Email)}");
            }

            if (!string.IsNullOrWhiteSpace(WebsiteUrl))
            {
                builder.AppendLine($"URL:{Escape(WebsiteUrl)}");
            }
        }

        private void AppendTelephoneLine(StringBuilder builder, string value, string type)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            builder.AppendLine($"TEL;TYPE={type}:{Escape(value)}");
        }

        private void AppendLocation(StringBuilder builder)
        {
            if (string.IsNullOrWhiteSpace(Address)
                && string.IsNullOrWhiteSpace(City)
                && string.IsNullOrWhiteSpace(PostCode)
                && string.IsNullOrWhiteSpace(County))
            {
                return;
            }

            builder.AppendLine($"ADR;TYPE=work:;;{Escape(Address)};{Escape(City)};{Escape(County)};{Escape(PostCode)};");
        }

        private static string Escape(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return value
                .Replace("\\", "\\\\")
                .Replace(";", "\\;")
                .Replace(",", "\\,")
                .Replace("\n", "\\n")
                .Replace("\r", string.Empty)
                .Trim();
        }

        private bool AreAllFieldsEmpty()
        {
            return string.IsNullOrWhiteSpace(FirstName)
                && string.IsNullOrWhiteSpace(LastName)
                && string.IsNullOrWhiteSpace(PhoneNumber)
                && string.IsNullOrWhiteSpace(Mobile)
                && string.IsNullOrWhiteSpace(Email)
                && string.IsNullOrWhiteSpace(WebsiteUrl)
                && string.IsNullOrWhiteSpace(CompanyName)
                && string.IsNullOrWhiteSpace(JobTitle)
                && string.IsNullOrWhiteSpace(Fax)
                && string.IsNullOrWhiteSpace(Address)
                && string.IsNullOrWhiteSpace(City)
                && string.IsNullOrWhiteSpace(PostCode)
                && string.IsNullOrWhiteSpace(County);
        }
        #endregion
    }
}
