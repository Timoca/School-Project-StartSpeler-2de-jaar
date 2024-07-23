using System.ComponentModel.DataAnnotations;

namespace API.Models
{
	public class Product
	{
		[Key]
		public int Id { get; set; } = default!;

		public string Naam { get; set; } = default!;
		public string Notities { get; set; } = default!;
		public float Prijs { get; set; } = default!;
		public int StockAantal { get; set; } = default!;
		public string Type { get; set; } = default!;
	}
}