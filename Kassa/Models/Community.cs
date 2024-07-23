using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kassa.Models
{
    public class Community
    {
        public int Id { get; set; } = default!;

        public string Naam { get; set; } = default!;
        public List<Evenement>? Evenementen { get; set; }
    }
}
