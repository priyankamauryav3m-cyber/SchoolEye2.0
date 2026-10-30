using Microsoft.JSInterop;
using System.Timers;

public class SweetAlertService

{

    private readonly IJSRuntime _js;

    public SweetAlertService(IJSRuntime js)
    {

        _js = js;

    }

    public async Task ShowError(string message)
    {

        try
        {

            await _js.InvokeVoidAsync("Swal.fire", new
            {

                icon = "error",

                text = message,

                timer = 10000,

                timerProgressBar = true,

            });

        }

        catch (TaskCanceledException)
        {

            // ignore (timer close case)

        }

    }

    // every success alert (save / update / active / deactive) is the same animated top-end toast
    // (window.showSuccessToast in sweetalert.js), shown above any open popup
    private async Task ShowSuccessToast(string? message)
    {
        try
        {
            await _js.InvokeVoidAsync("showSuccessToast", message, false);
        }
        catch (TaskCanceledException)
        {
            // ignore (page closed while showing)
        }
    }

    public Task ShowSuccess(string? message) => ShowSuccessToast(message);

    public Task ShowSuccessVerification(string? message) => ShowSuccessToast(message);

    public class SweetAlertResult
    {

        public bool isConfirmed { get; set; }

        public bool isDismissed { get; set; }

    }

    public async Task<bool> ShowConfirm(string message)
    {

        var result = await _js.InvokeAsync<SweetAlertResult>(

            "Swal.fire",
            new
            {
                text = message,
                icon = "question",
                showCancelButton = true,
                timer = 10000,
                timerProgressBar = true,
                confirmButtonText = "Yes",
                cancelButtonText = "No"
            });

        return result.isConfirmed;

    }

    public async Task<bool> ShowRegistrationSuccess(string message)
    {

        return await _js.InvokeAsync<bool>("showRegistrationSuccess", message);

    }

    public Task ShowDeactive(string message) => ShowSuccessToast(message);

    public Task ShowActive(string message) => ShowSuccessToast(message);

    public Task ShowUpdate(string message) => ShowSuccessToast(message);

    public async Task ShowCustomAlert(string message)
    {

        await _js.InvokeVoidAsync("Swal.fire", new
        {

            icon = "info",

            title = message,

            timer = 10000,

            timerProgressBar = true,

        });

    }

    public async Task ShowWarning(string message)
    {

        await _js.InvokeVoidAsync("Swal.fire", new
        {

            icon = "warning",
            timer = 10000,
            timerProgressBar = true,
            text = message,

        });

    }
    public async Task ShowMessage(string message)
    {

        await _js.InvokeVoidAsync("Swal.fire", new
        {

            icon = "success",
            timer = 10000,
            timerProgressBar = true,
            html = message,

        });

    }

}

