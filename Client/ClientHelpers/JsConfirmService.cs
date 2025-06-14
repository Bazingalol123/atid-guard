using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace AuthWithAdmin.Client.ClientHelpers
{
    /// <summary>
    /// A service that provides JavaScript-based confirmation dialogs
    /// as a fallback when the PopupService is not available
    /// </summary>
    public class JsConfirmService
    {
        private readonly IJSRuntime _jsRuntime;
        
        public JsConfirmService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }
        
        /// <summary>
        /// Shows a confirmation dialog using JavaScript
        /// </summary>
        /// <param name="title">The title of the dialog (not used in standard JS confirm)</param>
        /// <param name="message">The message to display</param>
        /// <param name="confirmText">The text for the confirm button (not used in standard JS confirm)</param>
        /// <param name="cancelText">The text for the cancel button (not used in standard JS confirm)</param>
        /// <param name="isDanger">Whether this is a dangerous action (not used in standard JS confirm)</param>
        /// <returns>True if confirmed, false otherwise</returns>
        public async Task<bool> ConfirmAsync(string title, string message, string confirmText = "אישור", string cancelText = "ביטול", bool isDanger = false)
        {
            try
            {
                // Try to use the custom showConfirmation function first
                return await _jsRuntime.InvokeAsync<bool>("showConfirmation", title, message, confirmText, cancelText, isDanger);
            }
            catch
            {
                // Fall back to standard confirm if the custom function doesn't exist
                return await _jsRuntime.InvokeAsync<bool>("confirm", message);
            }
        }
        
        /// <summary>
        /// Shows a danger confirmation dialog using JavaScript
        /// </summary>
        /// <param name="title">The title of the dialog (not used in standard JS confirm)</param>
        /// <param name="message">The message to display</param>
        /// <returns>True if confirmed, false otherwise</returns>
        public async Task<bool> ConfirmDangerAsync(string title, string message)
        {
            return await ConfirmAsync(title, message, "מחק", "ביטול", true);
        }
    }
} 