using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

namespace PM2E18322.Views;

public partial class Mapa : ContentPage
{
	public Mapa(string latitudeText, string longitudeText)
	{
		InitializeComponent();
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
}