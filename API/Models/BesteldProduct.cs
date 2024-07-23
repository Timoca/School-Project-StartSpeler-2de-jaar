using API.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class BesteldProduct
{
    public int Aantal { get; set; }

    public Bestelling? Bestelling { get; set; } = default!;

    [ForeignKey("Bestelling")]
    public int BestellingId { get; set; }

    [Key]
    public int Id { get; set; }

    public string Opmerking { get; set; } = default!;
    public Product? Product { get; set; } = default!;

    [ForeignKey("Product")]
    public int ProductId { get; set; }
}