using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Collections.ObjectModel;
using PM2E18322.Views;
namespace PM2E18322.Views
{
    public partial class actuLugar : ContentPage
    {
        private const string GoogleMapsApiKey = "AIzaSyCUM-myzK7lScxEnEDRG2NlbpwXg1A0h0k";
        Controllers.LugarControllers controller;
        FileResult photo; // Para tomar foto
        List<Models.Lugar> lugares;
        private int autorId;
        private string? currentPhotoBase64; // Almacenar la imagen actual en Base64

        public actuLugar(int authorID)
        {
            InitializeComponent();
            this.autorId = authorID;
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
            await BuscarAutor(autorId); // Llamar a BuscarAutor aquí en lugar del constructor
        }

        private async Task BuscarAutor(int authorId)
        {
            var results = lugares
                .Where(lu => lu.Id == authorId)
                .ToList();

            if (results.Any())
            {
                var lu = results.First();

                imgFoto.Source = ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(lu.Foto)));
                currentPhotoBase64 = lu.Foto; // Almacenar la foto actual en Base64
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
            return currentPhotoBase64; // Devolver la foto actual si no se tomó una nueva
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
            return currentPhotoBase64 != null ? Convert.FromBase64String(currentPhotoBase64) : null;
        }

        private void btnSitios_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new ListaLugar());
        }

        private void btnSalir_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new ListaLugar());
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

                await sourcephoto.CopyToAsync(streamlocal); // Para guardarla localmente

                // Actualizar la foto actual en Base64
                using (MemoryStream ms = new MemoryStream())
                {
                    sourcephoto.Position = 0; // Reiniciar el stream
                    await sourcephoto.CopyToAsync(ms);
                    byte[] data = ms.ToArray();
                    currentPhotoBase64 = Convert.ToBase64String(data);
                }
            }
        }

        private async void btnGuardar_Clicked(object sender, EventArgs e)
        {
            string Descripcion = PlaceEntry.Text;

            if (string.IsNullOrEmpty(Descripcion))
            {
                await DisplayAlert("Error", "Por favor ingrese el nombre del autor", "OK");
                return;
            }

            var autor = new Models.Lugar
            {
                Id = autorId,
                Longitud = LongitudEntry.Text,
                Latitud = LatitudEntry.Text,
                Descripcion = PlaceEntry.Text,
                Foto = GetImg64() // Usar la foto actual o la nueva foto en Base64
            };

            try
            {
                if (controller != null)
                {
                    if (await controller.storeAutor(autor) > 0)
                    {
                        await DisplayAlert("Aviso", "Registro actualizado con éxito!", "OK");
                        await Navigation.PopAsync();
                    }
                    else
                    {
                        await DisplayAlert("Error", "Ocurrió un error", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Ocurrió un error: {ex.Message}", "OK");
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
}
