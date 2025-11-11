using CommunityToolkit.Mvvm.ComponentModel;
using QRCoder;
using System;
using System.Text;
using Windows.ApplicationModel.Resources;

namespace QuickCode.ViewModels
{
    public class QrCodeCalendarEventViewModel : ObservableObject, IQrCodeDataViewModel
    {
        #region Fields
        private string eventName = string.Empty;
        private DateTimeOffset startDate;
        private DateTimeOffset endDate;
        private TimeSpan? startTime;
        private TimeSpan? endTime;
        private bool isAllDay;
        private string location = string.Empty;
        #endregion

        #region Constructors
        public QrCodeCalendarEventViewModel()
        {
            var resourceLoader = new ResourceLoader();
            Header = resourceLoader.GetString("QrCodeCalendar_Header");
            Description = resourceLoader.GetString("QrCodeCalendar_Description");
            IconGlyph = "\uE787";

            var now = DateTimeOffset.Now;
            startDate = now;
            endDate = now.AddHours(1);
            startTime = now.TimeOfDay;
            endTime = endDate.TimeOfDay;
        }
        #endregion

        #region Events
        public event EventHandler<string?>? RawDataReceived;
        #endregion

        #region Properties
        public string Header { get; }
        public string Description { get; }
        public string IconGlyph { get; }
        public string EventName { get => eventName; set { eventName = value; OnPropertyChanged(); OnFieldChanged(); } }
        public DateTimeOffset StartDate { get => startDate; set { startDate = value; OnPropertyChanged(); OnFieldChanged(); } }
        public TimeSpan? StartTime { get => startTime; set { startTime = value; OnPropertyChanged(); OnFieldChanged(); } }
        public DateTimeOffset EndDate { get => endDate; set { endDate = value; OnPropertyChanged(); OnFieldChanged(); } }
        public TimeSpan? EndTime { get => endTime; set { endTime = value; OnPropertyChanged(); OnFieldChanged(); } }
        public bool IsAllDay { get => isAllDay; set { isAllDay = value; OnPropertyChanged(); OnFieldChanged(); } }
        public string Location { get => location; set { location = value; OnPropertyChanged(); OnFieldChanged(); } }
        #endregion

        #region Handlers
        private void OnFieldChanged()
        {
            if (string.IsNullOrWhiteSpace(EventName))
            {
                RawDataReceived?.Invoke(this, null);
                return;
            }

            var builder = new StringBuilder();
            builder.AppendLine("BEGIN:VCALENDAR");
            builder.AppendLine("VERSION:2.0");
            builder.AppendLine("BEGIN:VEVENT");
            builder.AppendLine($"SUMMARY:{EscapeIcsValue(EventName)}");

            if (!string.IsNullOrWhiteSpace(Location))
            {
                builder.AppendLine($"LOCATION:{EscapeIcsValue(Location)}");
            }

            if (IsAllDay)
            {
                var startDateOnly = StartDate.Date;
                var endDateOnly = EndDate.Date;

                if (endDateOnly < startDateOnly)
                {
                    RawDataReceived?.Invoke(this, null);
                    return;
                }

                builder.AppendLine($"DTSTART;VALUE=DATE:{startDateOnly:yyyyMMdd}");
                builder.AppendLine($"DTEND;VALUE=DATE:{endDateOnly.AddDays(1):yyyyMMdd}");
            }
            else
            {
                if (!StartTime.HasValue || !EndTime.HasValue)
                {
                    RawDataReceived?.Invoke(this, null);
                    return;
                }

                var startDateTime = CombineDateAndTime(StartDate, StartTime.Value);
                var endDateTime = CombineDateAndTime(EndDate, EndTime.Value);

                if (endDateTime <= startDateTime)
                {
                    RawDataReceived?.Invoke(this, null);
                    return;
                }

                builder.AppendLine($"DTSTART:{startDateTime.UtcDateTime:yyyyMMdd'T'HHmmss'Z'}");
                builder.AppendLine($"DTEND:{endDateTime.UtcDateTime:yyyyMMdd'T'HHmmss'Z'}");
            }

            builder.AppendLine("END:VEVENT");
            builder.Append("END:VCALENDAR");

            RawDataReceived?.Invoke(this, builder.ToString());
        }
        #endregion

        #region Helpers
        private static DateTimeOffset CombineDateAndTime(DateTimeOffset date, TimeSpan time)
        {
            var combined = date.Date + time;
            return new DateTimeOffset(combined, date.Offset);
        }

        private static string EscapeIcsValue(string value)
        {
            return value
                .Replace("\\", "\\\\")
                .Replace(";", "\\;")
                .Replace(",", "\\,")
                .Replace("\r\n", "\\n")
                .Replace("\n", "\\n");
        }
        #endregion
    }
}
