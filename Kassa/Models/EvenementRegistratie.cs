using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kassa.Models
{
    public class EvenementenRegistratie
    {
        public int AantalPersonen { get; set; } = default!;

        public Evenement Evenement { get; set; } = default!;

        public int EvenementId { get; set; }

        public Gebruiker Gebruiker { get; set; } = default!;

        public string GebruikerId { get; set; } = default!;

        [Key]
        public int Id { get; set; } = default!;
    }
}
