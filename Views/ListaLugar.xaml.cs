using Microsoft.Maui.Controls;
using PM2E18322.Controllers;
using PM2E18322.Models;
using System.Collections.ObjectModel;
namespace PM2E18322.Views;

public partial class ListaLugar : ContentPage
{
    private Controllers.LugarControllers LugarController;
    private List<Models.Lugar> autores;
    Models.Lugar selectedAuthor;
    private LugarControllers controller;
    public ObservableCollection<Lugar> Autores { get; set; }
    public Command<Lugar> UpdateCommand { get; }
    public Command<Lugar> DeleteCommand { get; }

    public ListaLugar()
    {
        InitializeComponent();
        LugarController = new Controllers.LugarControllers();
        controller = new LugarControllers();
        Autores = new ObservableCollection<Lugar>();
        BindingContext = this;
    }

    //Metodo que permite mostrar la lista mientras la pagina se esta mostrando o cargando
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Obtiene la lista de personas de la base de datos
        autores = await LugarController.getListLugar();

        // Coloca la lista en el collection view
        collectionView.ItemsSource = autores;
    }

    private void searchBar_SearchButtonPressed(object sender, EventArgs e)
    {
        BuscarAutores(searchBar.Text);
    }

    //Funcion para realizar una busqueda en la lista o base de datos
    private void BuscarAutores(string query)
    {

        //Usa LINQ (Language-Integrated Query) (es una característica en el framework .NET que proporciona una sintaxis estandarizada
        //y declarativa para consultar y manipular datos de diferentes tipos de fuentes, como colecciones,
        //bases de datos, XML, entre otros.) en una expresion tipo lambda para filtrar la informacion de la base 
        //de datos y mostrar los resultados basados en la busqueda.

        var results = autores
            .Where(author => author.Descripcion?.ToLowerInvariant().Contains(query.ToLowerInvariant()) == true)
        .ToList();

        collectionView.ItemsSource = new List<Models.Lugar>(results);
    }

    private void searchBar_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(e.NewTextValue))
        {
            collectionView.ItemsSource = autores;
        }
    }

    private void btnRegresar_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new Main());
    }

    private void collectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        selectedAuthor = e.CurrentSelection.FirstOrDefault() as Models.Lugar;
    }

    private async void ActualizarAutor_Clicked(object sender, EventArgs e)
    {
        if (selectedAuthor != null)
        {
            await Navigation.PushAsync(new actuLugar(selectedAuthor.Id));
        }
        else
        {
            await DisplayAlert("Error", "Seleccione un lugar primero", "OK");
        }
    }

    private async void EliminarAutor_Clicked(object sender, EventArgs e)
    {
        var result = await DisplayAlert("Confirmar", "¿Está seguro que desea eliminar este lugar?", "Sí", "No");

        if (selectedAuthor != null)
        {
            if (result)
            {
                await controller.deleteLugar(selectedAuthor.Id);
                Autores.Remove(selectedAuthor);

                Navigation.PopAsync();
            }
            else
            {
                return;
            }
        }
        else
        {
            await DisplayAlert("Error", "Seleccione un autor primero", "OK");
        }
    }

    private async void Mapa_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Mapa(selectedAuthor.Latitud, selectedAuthor.Longitud, selectedAuthor.Foto));
    }
}