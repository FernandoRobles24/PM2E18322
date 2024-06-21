using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM2E18322.Models
{
    [SQLite.Table("Lugar")]
    public class Lugar
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [MaxLength(255), NotNull]
        public string? Longitud { get; set; }

        [MaxLength(255), NotNull]
        public string? Latitud { get; set; }

        [MaxLength(255), NotNull]
        public string? Descripcion { get; set; }
        public string? Foto { get; set; }
    }
}
