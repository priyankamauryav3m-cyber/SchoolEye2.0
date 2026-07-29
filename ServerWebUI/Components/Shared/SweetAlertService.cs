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

    public async Task ShowSuccess(string? message)
    {

        await _js.InvokeVoidAsync("Swal.fire", new
        {
            icon = "success",
            text = message,
            timer = 10000,
            timerProgressBar = true,


        });

    }
    public async Task ShowSuccessVerification(string? message)
    {

        await _js.InvokeVoidAsync("Swal.fire", new
        {
            position= "top-end",
            icon = "success",
            title = message,
            timer= 2000,
            showConfirmButton =false,


        });

    }

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

    public async Task ShowDeactive(string message)
    {

        await _js.InvokeVoidAsync("Swal.fire", new
        {

            icon = "success",

            title = message,

            timer = 10000,

            timerProgressBar = true,


        });

    }

    public async Task ShowActive(string message)
    {

        await _js.InvokeVoidAsync("Swal.fire", new
        {

            icon = "success",

            title = message,

            timer = 10000,

            timerProgressBar = true,

            IsVisible = true

        });

    }

    public async Task ShowUpdate(string message)
    {

        await _js.InvokeVoidAsync("Swal.fire", new
        {

            icon = "success",

            text = message,

            timer = 10000,

            timerProgressBar = true,

        });

    }

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

