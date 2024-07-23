using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    public class EvenementenRegistratie
    {
        public int AantalPersonen { get; set; } = default!;

        public Evenement Evenement { get; set; } = default!;

        [ForeignKey("Evenement")]
        public int EvenementId { get; set; }

        public Gebruiker Gebruiker { get; set; } = default!;

        [ForeignKey("Gebruiker")]
        public string GebruikerId { get; set; } = default!;

        [Key]
        public int Id { get; set; } = default!;
    }
}