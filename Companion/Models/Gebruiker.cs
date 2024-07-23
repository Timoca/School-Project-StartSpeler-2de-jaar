using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Companion.Models
{
    public class Gebruiker
    {
        public Guid Id { get; set; } = default!;
        public string Voornaam { get; set; } = default!;
        public string Achternaam { get; set; } = default!;
        public List<EvenementenRegistratie> EvenementenRegistraties { get; set; } = default!;
    }
}
