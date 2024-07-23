using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace API.Models
{
	public class Gebruiker : IdentityUser
	{
		public string Voornaam { get; set; } = default!;
		public string Achternaam { get; set; } = default!;

		[JsonIgnore]
		public List<Bestelling>? Bestellingen { get; set; } = default!;
        [JsonIgnore]
        public List<GebruikerRechten>? GebruikerRechten { get; set; } = default!;
        [JsonIgnore]
        public List<EvenementenRegistratie>? EvenementenRegistraties { get; set; } = default!;
	}
}