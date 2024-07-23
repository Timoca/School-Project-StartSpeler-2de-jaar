using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
	public class Bestelling
	{
		public DateTime BestelTijd { get; set; } = default!;
		public int BetaalStatus { get; set; }

		[JsonIgnore]
		public Gebruiker? Gebruiker { get; set; } = default!;

		[ForeignKey("Gebruiker")]
		public string GebruikerId { get; set; } = default!;

		[Key]
		public int Id { get; set; } = default!;

		public int TafelNummer { get; set; } = default!;
		public float TotaalBedrag { get; set; } = default!;
		public List<BesteldProduct> BesteldeProducten { get; set; } = default!;
	}
}