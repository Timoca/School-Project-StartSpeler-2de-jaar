using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    public class GebruikerRechten
    {
        public Gebruiker Gebruiker { get; set; } = default!;

        [ForeignKey("Gebruiker")]
        public string GebruikerId { get; set; } = default!;

        [Key]
        public int Id { get; set; } = default!;

        public Rechten Rechten { get; set; } = default!;

        [ForeignKey("Rechten")]
        public int RechtenId { get; set; }
    }
}