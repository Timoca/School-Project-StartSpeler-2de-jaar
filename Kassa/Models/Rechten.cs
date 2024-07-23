using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kassa.Models
{
    public class Rechten
    {
        public string Beschrijving { get; set; } = default!;

        public int Id { get; set; } = default!;

        public string RechtNaam { get; set; } = default!;
        public List<GebruikerRechten> GebruikerRechten { get; set; } = default!;
    }
}
