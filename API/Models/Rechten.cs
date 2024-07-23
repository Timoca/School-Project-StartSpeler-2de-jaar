using System.ComponentModel.DataAnnotations;

namespace API.Models
{
	public class Rechten
	{
		public string Beschrijving { get; set; } = default!;

		[Key]
		public int Id { get; set; } = default!;

		public string RechtNaam { get; set; } = default!;
		public List<GebruikerRechten> GebruikerRechten { get; set; } = default!;
	}
}