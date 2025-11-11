using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Globalization;
using Windows.ApplicationModel.Resources;

namespace QuickCode.ViewModels
{
    public partial class QrCodeLocationViewModel : ObservableObject, IQrCodeDataViewModel
    {
        #region Fields
        private readonly ResourceLoader resourceLoader = new();
        private double? latitude;
        private double? longitude;
        #endregion

        #region Constructors
        public QrCodeLocationViewModel()
        {
            var resourceLoader = new ResourceLoader();
            Header = resourceLoader.GetString("QrCodeLocation_Header");
            Description = resourceLoader.GetString("QrCodeLocation_Description");
            IconGlyph = "\uE707";
        }
        #endregion

        #region Events
        public event EventHandler<string?>? RawDataReceived;
        #endregion

        #region Properties
        public string Header { get; }
        public string Description { get; }
        public string IconGlyph { get; }
        public double? Latitude { get => latitude; set { latitude = value; OnPropertyChanged(); OnCoordinatesChanged(); } }
        public double? Longitude { get => longitude; set { longitude = value; OnPropertyChanged(); OnCoordinatesChanged(); } }
        #endregion

        #region Methods
        public void SetCoordinates(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }
        #endregion

        #region Handlers
        private void OnCoordinatesChanged()
        {
            if (Latitude is double lat && Longitude is double lon)
            {
                if (lat < -90 || lat > 90 || lon < -180 || lon > 180)
                {
                    RawDataReceived?.Invoke(this, null);
                    return;
                }

                var payload = string.Format(CultureInfo.InvariantCulture, "geo:{0},{1}", lat, lon);
                RawDataReceived?.Invoke(this, payload);
            }
            else
            {
                RawDataReceived?.Invoke(this, null);
            }
        }
        #endregion
    }
}
