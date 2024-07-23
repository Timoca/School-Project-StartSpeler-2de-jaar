using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace Kassa.Models
{
    public class Bestelling
    {
        public DateTime BestelTijd { get; set; } = default!;

        public int BetaalStatus { get; set; }

        public string BetaalStatusString
        {
            get
            {
                // Controleer de betaalstatus en retourneer de juiste string
                return BetaalStatus == 0 ? "Afwachten" : "Betaald";
            }
        }

        [JsonIgnore]
        public Gebruiker? Gebruiker { get; set; } = default!;

        public string GebruikerId { get; set; } = default!;

        public int Id { get; set; } = default!;

        public int TafelNummer { get; set; } = default!;
        public float TotaalBedrag { get; set; } = default!;

        [JsonIgnore]
        public List<BesteldProduct> BesteldeProducten { get; set; } = new List<BesteldProduct>();

        public string VolledigeNaam { get; set; } = default!;
    }
}