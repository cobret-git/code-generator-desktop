using CommunityToolkit.Mvvm.ComponentModel;
using System;
using Windows.ApplicationModel.Resources;

namespace QuickCode.ViewModels
{
    public class QrCodeTextDataViewModel : ObservableObject, IQrCodeDataViewModel
    {
        #region Fields
        private string? text;
        #endregion

        #region Constructors
        public QrCodeTextDataViewModel()
        {
            var resourceLoader = new ResourceLoader();
            Header = resourceLoader.GetString("QrCodeText.Header");
            Description = resourceLoader.GetString("QrCodeText.Description");
            IconGlyph = "&#xE8D2;";
        }
        #endregion

        #region Events
        public event EventHandler<string?>? RawDataReceived;
        #endregion

        #region Properties
        public string Header { get; }
        public string Description { get; }
        public string IconGlyph { get; }
        public string? Text { get => text; set { text = value; OnTextChanged(value); } }
        #endregion

        #region Handlers
        private void OnTextChanged(string? value)
        {
            RawDataReceived?.Invoke(this, value);
            OnPropertyChanged(nameof(Text));
        }
        #endregion
    }
}
