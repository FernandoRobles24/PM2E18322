using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PM2E18322.Models;

namespace PM2E18322.Controllers
{
    public class LugarControllers
    {
        SQLiteAsyncConnection _connection;

        //Constructor vacio
        public LugarControllers() { }

        //Conexion a la base de datos
        public async Task Init()
        {
            try
            {
                if (_connection is null)
                {
                    SQLite.SQLiteOpenFlags extensiones = SQLite.SQLiteOpenFlags.ReadWrite | SQLite.SQLiteOpenFlags.Create | SQLite.SQLiteOpenFlags.SharedCache;

                    if (string.IsNullOrEmpty(FileSystem.AppDataDirectory))
                    {
                        return;
                    }

                    _connection = new SQLiteAsyncConnection(Path.Combine(FileSystem.AppDataDirectory, "DBLugares"), extensiones);

                    var creacion = await _connection.CreateTableAsync<Models.Lugar>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Init(): {ex.Message}");
            }
        }

        //Crear metodos crud para la clase personas
        //Create
        public async Task<int> storeAutor(Lugar lugar)
        {
            await Init();
            if (lugar.Id == 0)
            {
                return await _connection.InsertAsync(lugar);
            }
            else
            {
                return await _connection.UpdateAsync(lugar);
            }
        }

        //Update
        public async Task<int> updateAutor(Lugar lugar)
        {
            await Init();
            return await _connection.UpdateAsync(lugar);
        }

        //Read
        public async Task<List<Models.Lugar>> getListLugar()
        {
            await Init();
            return await _connection.Table<Lugar>().ToListAsync();
        }

        //Read Element
        public async Task<Models.Lugar> getLugar(int pid)
        {
            await Init();
            return await _connection.Table<Lugar>().Where(i => i.Id == pid).FirstOrDefaultAsync();
        }

        //Delete
        public async Task<int> deleteLugar(int lugarID)
        {
            await Init();
            var lugarToDelete = await getLugar(lugarID);

            if (lugarToDelete != null)
            {
                return await _connection.DeleteAsync(lugarToDelete);
            }

            return 0;
        }
    }
}
