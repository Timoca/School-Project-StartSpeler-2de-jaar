using System.ComponentModel.DataAnnotations;

namespace API.Models
{
	public class Community
	{
		[Key]
		public int Id { get; set; } = default!;

		public string Naam { get; set; } = default!;
		public List<Evenement>? Evenementen { get; set; }
	}
}