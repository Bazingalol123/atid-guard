using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace AuthWithAdmin.Client.ClientHelpers
{
    public class PopupService
    {
        private readonly ILogger<PopupService> _logger;
        private readonly IJSRuntime _jsRuntime;
        private bool _isInitialized = false;

        public PopupService(IJSRuntime jsRuntime, ILogger<PopupService> logger = null)
        {
            _jsRuntime = jsRuntime;
            _logger = logger;
        }

        /// <summary>
        /// Event triggered when a confirmation dialog should be shown
        /// </summary>
        public event Func<PopupOptions, Task<bool>> OnShowConfirmation;

        /// <summary>
        /// Event triggered when an information dialog should be shown
        /// </summary>
        public event Func<PopupOptions, Task> OnShowInfo;

        /// <summary>
        /// Called by the PopupHost component to register itself
        /// </summary>
        public void Initialize()
        {
            _isInitialized = true;
            _logger?.LogInformation("PopupService initialized");
        }

        /// <summary>
        /// Shows a confirmation dialog with the provided options
        /// </summary>
        /// <param name="options">Configuration options for the dialog</param>
        /// <returns>True if confirmed, false otherwise</returns>
        public async Task<bool> ShowConfirmationAsync(PopupOptions options)
        {
            if (!_isInitialized || OnShowConfirmation == null)
            {
                _logger?.LogWarning("Using JavaScript fallback for confirmation dialog");

                // Fallback to JavaScript confirm
                try
                {
                    // Try to use the custom showConfirmation function first
                    return await _jsRuntime.InvokeAsync<bool>(
                        "showConfirmation",
                        options.Title,
                        options.Message,
                        options.ConfirmText,
                        options.CancelText,
                        options.ConfirmButtonClass == "btn-danger",
                        options.FooterClass,
                        options.ContainerClass);
                }
                catch
                {
                    // Fall back to standard confirm
                    return await _jsRuntime.InvokeAsync<bool>("confirm", options.Message);
                }
            }

            try
            {
                return await OnShowConfirmation.Invoke(options);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error showing confirmation dialog: {ex.Message}");
                _logger?.LogError(ex, "Error showing popup, falling back to JavaScript confirm");

                // Fallback to JavaScript confirm
                return await _jsRuntime.InvokeAsync<bool>("confirm", options.Message);
            }
        }

        /// <summary>
        /// Shows an informational dialog with the provided options
        /// </summary>
        /// <param name="options">Configuration options for the dialog</param>
        public async Task ShowInfoAsync(PopupOptions options)
        {
            if (!_isInitialized || OnShowInfo == null)
            {
                _logger?.LogWarning("Using JavaScript fallback for info dialog");

                // Fallback to JavaScript alert
                try
                {
                    await _jsRuntime.InvokeVoidAsync("alert", $"{options.Title}\n\n{options.Message}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error showing alert dialog: {ex.Message}");
                }
                return;
            }

            try
            {
                await OnShowInfo.Invoke(options);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error showing info dialog: {ex.Message}");
                _logger?.LogError(ex, "Error showing info popup, falling back to JavaScript alert");

                // Fallback to JavaScript alert
                await _jsRuntime.InvokeVoidAsync("alert", $"{options.Title}\n\n{options.Message}");
            }
        }

        /// <summary>
        /// Shows a simple about dialog with default styling
        /// </summary>
        /// <param name="content">Content for about dialog</param>
        public async Task ShowAboutAsync(string content)
        {
            await ShowInfoAsync(new PopupOptions
            {
                Title = "אודות",
                HtmlContent = content,
                ConfirmText = "סגור",
                ConfirmButtonClass = "btn-main",
                ShowCancelButton = false,
                FooterText = "© HIT כל הזכויות שמורות למכון טכנולוגי חולון " +
                "<br/>" +
                "<a href='https://www.flaticon.com/free-icons/google' title='google icons'>Google icons created by Freepik - Flaticon</a>\r\n",
                FooterClass = "about-footer"
            });
        }

        /// <summary>
        /// Shows a simple confirmation dialog with default styling
        /// </summary>
        /// <param name="title">Dialog title</param>
        /// <param name="message">Dialog message</param>
        /// <returns>True if confirmed, false otherwise</returns>
        public async Task<bool> ConfirmAsync(string title, string message)
        {
            return await ShowConfirmationAsync(new PopupOptions
            {
                Title = title,
                Message = message,
                ConfirmText = "אישור",
                CancelText = "ביטול",
                ConfirmButtonClass = "btn-main",
                FooterClass = "confirmation-footer",
                ContainerClass = "confirmation-container",
                CurrentDirection = ""

            });
        }

        /// <summary>
        /// Shows a danger confirmation dialog with red styling for destructive actions
        /// </summary>
        /// <param name="title">Dialog title</param>
        /// <param name="message">Dialog message</param>
        /// <returns>True if confirmed, false otherwise</returns>
        public async Task<bool> ConfirmDangerAsync(string title, string message)
        {
            return await ShowConfirmationAsync(new PopupOptions
            {
                Title = title,
                Message = message,
                ConfirmText = "מחיקה",
                CancelText = "ביטול",
                ConfirmButtonClass = "btn-danger"

            });
        }
    }

    /// <summary>
    /// Configuration options for the popup dialog
    /// </summary>
    public class PopupOptions
    {
        /// <summary>
        /// Dialog title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Dialog message (plain text)
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// HTML content for the dialog body (overrides Message if provided)
        /// </summary>
        public string HtmlContent { get; set; }

        /// <summary>
        /// Text for the confirm button
        /// </summary>
        public string ConfirmText { get; set; } = "אישור";

        /// <summary>
        /// Text for the cancel button
        /// </summary>
        public string CancelText { get; set; } = "ביטול";

        /// <summary>
        /// CSS class for the confirm button
        /// </summary>
        public string ConfirmButtonClass { get; set; } = "btn-main";

        /// <summary>
        /// Whether to show the cancel button
        /// </summary>
        public bool ShowCancelButton { get; set; } = true;

        /// <summary>
        /// Whether to close the popup when clicking outside of it
        /// </summary>
        public bool CloseOnBackdropClick { get; set; } = true;

        /// <summary>
        /// CSS class for the popup footer
        /// </summary>
        public string? FooterClass { get; set; } = "";  

        public string FooterText { get; set; } = "";
        public string ContainerClass { get; set; } = "";
        public string CurrentDirection { get; set; } = "rtl";

    }
}