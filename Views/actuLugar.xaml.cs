using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Collections.ObjectModel;
using PM2E18322.Views;
namespace PM2E18322.Views;

public partial class actuLugar : ContentPage
{
    private const string GoogleMapsApiKey = "AIzaSyCUM-myzK7lScxEnEDRG2NlbpwXg1A0h0k";
    Controllers.LugarControllers controller;
    FileResult photo; //Para tomar foto
    List<Models.Lugar> lugares;
    private int autorId;

    public actuLugar(int authorID)
    {
        InitializeComponent();
        this.autorId = authorID;
        //BuscarAutor(authorID);
        controller = new Controllers.LugarControllers();
    }
    public actuLugar(Controllers.LugarControllers dbPath)
    {
        InitializeComponent();
        controller = dbPath;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        lugares = await controller.getListLugar();

    }
    private async void BuscarAutor(int authorId)
    {
        lugares = await controller.getListLugar();

        var results = lugares
            .Where(lu => lu.Id == authorId)
            .ToList();


        if (results.Any())
        {
            var lu = results.First();

            imgFoto.Source = lu.Foto;
            PlaceEntry.Text = lu.Descripcion;
            LongitudEntry.Text = lu.Longitud;
            LatitudEntry.Text = lu.Latitud;

        }
        else
        {
            await DisplayAlert("Error", "Id no encontrado", "OK");
        }
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
        //Navigation.PushAsync(new nuevoAutor());
    }

    private async void btnAgregar_Clicked(object sender, EventArgs e)
    {
        photo = await MediaPicker.CapturePhotoAsync();

        if (photo != null)
        {
            string photoPath = Path.Combine(FileSystem.CacheDirectory, photo.FileName);
            using Stream sourcephoto = await photo.OpenReadAsync();
            using FileStream streamlocal = File.OpenWrite(photoPath);

            imgFoto.Source = ImageSource.FromStream(() => photo.OpenReadAsync().Result); //Para verla dentro de archivo

            await sourcephoto.CopyToAsync(streamlocal); //Para Guardarla local
        }
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
            Id = autorId,
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
                    await DisplayAlert("Aviso", "Registro actualizado con exito!", "OK");
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