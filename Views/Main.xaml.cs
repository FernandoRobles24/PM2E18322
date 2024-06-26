using Microsoft.Maui.Controls;
//using Microsoft.Maui.Essentials;
using System;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Collections.ObjectModel;
using PM2E18322.Views;
using Microsoft.Maui.Maps.Handlers;
using Microsoft.Maui.Controls.PlatformConfiguration;
namespace PM2E18322.Views;

public partial class Main : ContentPage
{
    private const string GoogleMapsApiKey = "AIzaSyCUM-myzK7lScxEnEDRG2NlbpwXg1A0h0k";
    Controllers.LugarControllers controller;
    FileResult photo; //Para tomar foto

    public Main()
	{
		InitializeComponent();
        controller = new Controllers.LugarControllers();
        UpdateGuardarButtonState();

    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            var location = await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Medium));

            if (location != null)
            {
                LatitudEntry.Text = location.Latitude.ToString();
                LongitudEntry.Text = location.Longitude.ToString();

                var placemarks = await Geocoding.GetPlacemarksAsync(location.Latitude, location.Longitude);
                var placemark = placemarks?.FirstOrDefault();

                if (placemark != null)
                {
                    PlaceEntry.Text = placemark.Thoroughfare + ", " + placemark.Locality; // Ejemplo de construcci�n de la descripci�n del lugar
                }
                else
                {
                    PlaceEntry.Text = "No location description available";
                }
            }
            else
            {
                await DisplayAlert("Error", "Unable to get location", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error: {ex.Message}", "OK");
        }
        await CheckGpsStatusAsync();
        UpdateGuardarButtonState();
    }

    private async System.Threading.Tasks.Task CheckGpsStatusAsync()
    {
        var locationStatus = await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10)));

        if (locationStatus == null)
        {
            await DisplayAlert("GPS Not Enabled", "Please enable GPS to use this app.", "OK");
        }
    }

    private bool AreAllFieldsValid()
    {
        if (string.IsNullOrEmpty(PlaceEntry.Text))
        {
            return false;
        }

        if (string.IsNullOrEmpty(LatitudEntry.Text))
        {
            return false;
        }

        if (string.IsNullOrEmpty(LongitudEntry.Text))
        {
            return false;
        }

        // Verificar si la imagen est� seleccionada
        if (photo == null)
        {
            return false;
        }

        return true;
    }
    private void UpdateGuardarButtonState()
    {
        btnGuardar.IsEnabled = AreAllFieldsValid();
    }

    private async void btnBuscar_Clicked(object sender, EventArgs e)
    {
        string place = PlaceEntry.Text;
        if (string.IsNullOrEmpty(place))
        {
            await DisplayAlert("Error", "Please enter a place name", "OK");
            return;
        }

        string requestUri = $"https://maps.googleapis.com/maps/api/geocode/json?address={place}&key={GoogleMapsApiKey}";

        using (HttpClient client = new HttpClient())
        {
            string response = await client.GetStringAsync(requestUri);
            var json = JObject.Parse(response);

            if (json["status"].ToString() == "OK")
            {
                var location = json["results"][0]["geometry"]["location"];
                double latitude = (double)location["lat"];
                double longitude = (double)location["lng"];

                LatitudEntry.Text = latitude.ToString();
                LongitudEntry.Text = longitude.ToString();
            }
            else
            {
                await DisplayAlert("Error", "Unable to find location", "OK");
            }
        }
    }

    public string? GetImg64()
    {
        if (photo != null)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                Stream stream = photo.OpenReadAsync().Result;
                stream.CopyTo(ms);
                byte[] data = ms.ToArray();

                String Base64 = Convert.ToBase64String(data);

                return Base64;
            }
        }
        return null;
    }

    public byte[]? GetImageArray()
    {
        if (photo != null)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                Stream stream = photo.OpenReadAsync().Result;
                stream.CopyTo(ms);
                byte[] data = ms.ToArray();

                return data;
            }
        }
        return null;
    }

    private void btnSitios_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new ListaLugar());
    }
    private void btnSalir_Clicked(object sender, EventArgs e)
    {
        ExitApp();
    }
    private void ExitApp()
    {
#if ANDROID
            Platform.CurrentActivity.FinishAffinity();
#elif IOS
            System.Diagnostics.Process.GetCurrentProcess().CloseMainWindow();
#elif WINDOWS
            Application.Current.Quit();
#endif
    }

    private async void btnAgregar_Clicked(object sender, EventArgs e)
    {
        photo = await MediaPicker.CapturePhotoAsync();

        if (photo != null)
        {
            string photoPath = Path.Combine(FileSystem.CacheDirectory, photo.FileName);
            using Stream sourcephoto = await photo.OpenReadAsync();
            using FileStream streamlocal = File.OpenWrite(photoPath);

            imgFoto.Source = ImageSource.FromStream(() => photo.OpenReadAsync().Result); // Para verla dentro de archivo

            await sourcephoto.CopyToAsync(streamlocal); // Para guardarla local
        }

        UpdateGuardarButtonState();
    }

    private async void btnGuardar_Clicked(object sender, EventArgs e)
    {
        string Descripcion = PlaceEntry.Text;

        if (string.IsNullOrEmpty(Descripcion))
        {
            await DisplayAlert("Error", "Porfavor ingrese el nombre del autor", "OK");
            return;
        }

        var autor = new Models.Lugar
        {
            Longitud = LongitudEntry.Text,
            Latitud = LatitudEntry.Text,
            Descripcion = PlaceEntry.Text,
            Foto = GetImg64()
        };

        try
        {
            if (controller != null)
            {
                if (await controller.storeAutor(autor) > 0)
                {
                    await DisplayAlert("Aviso", "Registro Ingresado con Exito!", "OK");
                    await Navigation.PopAsync();
                }
                else
                {
                    await DisplayAlert("Error", "Ocurrio un Error", "OK");
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Ocurrio un Error: {ex.Message}", "OK");
        } 
    }

    private void PlaceEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateGuardarButtonState(); // Actualiza el estado del bot�n Guardar cuando cambia el texto
    }

    private void LatitudEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateGuardarButtonState(); // Actualiza el estado del bot�n Guardar cuando cambia el texto
    }

    private void LongitudEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateGuardarButtonState(); // Actualiza el estado del bot�n Guardar cuando cambia el texto
    }


    private void OnLinkTapped(object sender, EventArgs e)
    {
        if (sender is Label label && label.GestureRecognizers[0] is TapGestureRecognizer tapGestureRecognizer)
        {
            string url = tapGestureRecognizer.CommandParameter as string;
            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    Uri uri = new Uri(url);
                    Browser.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
                }
                catch (Exception ex)

                {

                }
            }
        }
    }

}