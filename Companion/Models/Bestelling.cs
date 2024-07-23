using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Companion.Models
{
    public class Bestelling
    {
        [Key]
        public int Id { get; set; } = default!;

        public DateTime BestelTijd { get; set; } = DateTime.Now;
        public bool BetaalStatus { get; set; } = false;
        public string GebruikerId { get; set; } = default!;
        public int TafelNummer { get; set; }
        public float TotaalBedrag { get; set; }
        public List<BesteldProduct> BesteldeProducten { get; set; } = new List<BesteldProduct>();
    }
}