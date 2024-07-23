using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kassa.Models
{
    public class GebruikerRechten
    {
        public Gebruiker Gebruiker { get; set; } = default!;

        public string GebruikerId { get; set; } = default!;

        public int Id { get; set; } = default!;

        public Rechten Rechten { get; set; } = default!;

        public int RechtenId { get; set; }
    }
}
