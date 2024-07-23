using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kassa.Models
{
    public class Gebruiker
    {
        public string Id { get; set; } = default!;
        public string UserName { get; set; } = default!;
        public string NormalizedUserName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string NormalizedEmail { get; set; } = default!;
        public string Achternaam { get; set; } = default!;
        public string Voornaam { get; set; } = default!;
        public string Telefoonnummer { get; set; } = default!;

        public List<Bestelling> Bestellingen { get; set; } = default!;
        public List<GebruikerRechten> GebruikerRechten { get; set; } = default!;
        public List<EvenementenRegistratie> EvenementenRegistraties { get; set; } = default!;
        public List<string> CurrentRoles { get; set; } = new List<string>();
        public string RolesAsString => string.Join(", ", CurrentRoles);
    }
}