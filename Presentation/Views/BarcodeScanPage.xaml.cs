using System.Linq;
using BarcodeScanning;

namespace VideoGameLibraryAndroid.Presentation.Views;

public partial class BarcodeScanPage : ContentPage
{
    private bool _handled;

    public BarcodeScanPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _handled = false;

        await Methods.AskForRequiredPermissionAsync();

        // Fijarlo aquí (no solo en el XAML) porque la cámara necesita que la página ya esté
        // realmente visible para poder engancharse a su ciclo de vida -- si se activa demasiado
        // pronto, CameraX se queda esperando ("Lifecycle is not set") sin arrancar nunca.
        Scanner.CameraEnabled = true;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        Scanner.CameraEnabled = false;
    }

    // Llega en el hilo de la cámara, no el de UI -- hay que despachar a MainThread antes de
    // tocar cualquier control o navegar. _handled evita procesar más de una detección si llegan
    // varios frames casi seguidos antes de que la navegación hacia atrás cierre la página.
    private void OnDetectionFinished(object sender, OnDetectionFinishedEventArg e)
    {
        if (_handled || e.BarcodeResults.Count == 0) return;
        _handled = true;

        var barcode = e.BarcodeResults.First().DisplayValue;

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await Shell.Current.GoToAsync($"..?barcode={barcode}");
        });
    }
}
