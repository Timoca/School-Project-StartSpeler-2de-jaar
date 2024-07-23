using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
	public class Evenement
	{
        public string Beschrijving { get; set; } = default!;

        public Community? Community { get; set; } = default!;

        [ForeignKey("Community")]
        public int CommunityId { get; set; }

        public DateTime DatumEinde { get; set; } = default!;
        public DateTime DatumStart { get; set; } = default!;

        [Key]
        public int Id { get; set; } = default!;

        public float Kosten { get; set; } = default!;
        public int AantalDeelnemers { get; set; } = default!;
        public int MaxDeelnemers { get; set; } = default!;
        public string Naam { get; set; } = default!;
        public List<EvenementenRegistratie>? EvenementenRegistratie { get; set; } = default!;
    }
}
