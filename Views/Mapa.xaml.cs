using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

namespace PM2E18322.Views;

public partial class Mapa : ContentPage
{
    public string photo;
	public Mapa(string latitudeText, string longitudeText, string foto)
	{
		InitializeComponent();
        photo = foto;
        if (double.TryParse(latitudeText, out double latitude) && double.TryParse(longitudeText, out double longitude))
        {
            var position = new Location(latitude, longitude);
            MyMap.MoveToRegion(MapSpan.FromCenterAndRadius(position, Distance.FromMiles(0.5)));

            var pin = new Pin
            {
                Label = "Selected Location",
                Location = position,
                Type = PinType.Place
            };

            MyMap.Pins.Add(pin);
        }
        else
        {
            DisplayAlert("Error", "Invalid coordinates", "OK");
        }

    }
    private async void btnCompartir_Clicked(object sender, EventArgs e)
    {
        byte[] imageBytes = Convert.FromBase64String(photo);
        string tempFilePath = Path.Combine(FileSystem.CacheDirectory, "tempImage.png");
        File.WriteAllBytes(tempFilePath, imageBytes);

        await ShareFileAsync(tempFilePath);
    }
    private async Task ShareFileAsync(string filePath)
    {
        await Share.RequestAsync(new ShareFileRequest
        {
            Title = "Compartir Imagen",
            File = new ShareFile(filePath)
        });
    }
    private void btnSalir_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new ListaLugar());
    }
}