using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Windows.ApplicationModel.Resources;
using System;
using System.Globalization;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;

namespace QuickCode.ViewModels
{
    public partial class QrCodeLocationViewModel : ObservableObject, IQrCodeDataViewModel
    {
        #region Fields
        private readonly ResourceLoader resourceLoader = new();
        private double? latitude;
        private double? longitude;
        private bool isBusy;
        private string? statusMessage;
        #endregion

        #region Events
        public event EventHandler<string?>? RawDataReceived;
        #endregion

        #region Properties
        public double? Latitude
        {
            get => latitude;
            set
            {
                latitude = value;
                OnPropertyChanged();
                OnCoordinatesChanged();
            }
        }

        public double? Longitude
        {
            get => longitude;
            set
            {
                longitude = value;
                OnPropertyChanged();
                OnCoordinatesChanged();
            }
        }

        public bool IsBusy
        {
            get => isBusy;
            private set
            {
                if (isBusy != value)
                {
                    isBusy = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CanRequestLocation));
                }
            }
        }

        public bool CanRequestLocation => !IsBusy;

        public string? StatusMessage
        {
            get => statusMessage;
            private set
            {
                statusMessage = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Commands
        [RelayCommand]
        private async Task UseCurrentLocationAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                StatusMessage = null;

                var access = await Geolocator.RequestAccessAsync();
                if (access == GeolocationAccessStatus.Denied)
                {
                    StatusMessage = resourceLoader.GetString("QrCodeLocationStatusPermissionDenied.Text");
                    RawDataReceived?.Invoke(this, null);
                    return;
                }
                if (access == GeolocationAccessStatus.Unspecified)
                {
                    StatusMessage = resourceLoader.GetString("QrCodeLocationStatusGenericError.Text");
                    RawDataReceived?.Invoke(this, null);
                    return;
                }

                var geolocator = new Geolocator
                {
                    DesiredAccuracy = PositionAccuracy.High,
                    DesiredAccuracyInMeters = 30
                };

                var position = await geolocator.GetGeopositionAsync();
                Latitude = position.Coordinate.Point.Position.Latitude;
                Longitude = position.Coordinate.Point.Position.Longitude;
            }
            catch (UnauthorizedAccessException)
            {
                StatusMessage = resourceLoader.GetString("QrCodeLocationStatusPermissionDenied.Text");
                RawDataReceived?.Invoke(this, null);
            }
            catch (Exception)
            {
                StatusMessage = resourceLoader.GetString("QrCodeLocationStatusGenericError.Text");
                RawDataReceived?.Invoke(this, null);
            }
            finally
            {
                IsBusy = false;
            }
        }
        #endregion

        #region Methods
        public void SetCoordinates(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }
        #endregion

        #region Helpers
        private void OnCoordinatesChanged()
        {
            if (Latitude is double lat && Longitude is double lon)
            {
                if (lat < -90 || lat > 90 || lon < -180 || lon > 180)
                {
                    StatusMessage = resourceLoader.GetString("QrCodeLocationStatusOutOfRange.Text");
                    RawDataReceived?.Invoke(this, null);
                    return;
                }

                StatusMessage = null;
                var payload = string.Format(CultureInfo.InvariantCulture, "geo:{0},{1}", lat, lon);
                RawDataReceived?.Invoke(this, payload);
            }
            else
            {
                StatusMessage = null;
                RawDataReceived?.Invoke(this, null);
            }
        }
        #endregion
    }
}
