using System;

namespace QuickCode.ViewModels
{
    /// <summary>
    /// Represents a contract for ViewModels that manage the specific data 
    /// required to generate a particular type of QR code (e.g., Text, URL, Email).
    /// </summary>
    public interface IQrCodeDataViewModel
    {
        #region Properties

        /// <summary>
        /// A read-only string property, used for a icon for the data type.
        /// </summary>
        public string IconGlyph { get; }

        /// <summary>
        /// A read-only string property, likely used for a title or brief label for the data type.
        /// </summary>
        public string Header { get; }

        /// <summary>
        /// A read-only string property, likely providing a more detailed explanation of the data the ViewModel handles.
        /// </summary>
        public string Description { get; }
        #endregion

        #region Event

        /// <summary>
        /// Occurs when the raw, finalized data string for QR code generation is ready or updated.
        /// The event argument (string?) should contain the fully formatted data payload 
        /// (e.g., a 'tel:' URI, 'mailto:' URI, or plain text).
        /// </summary>
        public event EventHandler<string?>? RawDataReceived;
        #endregion
    }
}
