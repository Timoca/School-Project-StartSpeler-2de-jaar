using API.Models;

namespace API.Data
{
    public class DbInitializer
    {
        public static void SeedRechtenData(StartspelerContext context)
        {
            if (!context.Rechten.Any())
            {
                context.Rechten.AddRange(
                    new Rechten { Id = 1, RechtNaam = "Beheerder", Beschrijving = "Beheerder rol beschrijving" },
                    new Rechten { Id = 2, RechtNaam = "Kelner", Beschrijving = "Kelner rol beschrijving" },
                    new Rechten { Id = 3, RechtNaam = "Event manager", Beschrijving = "Event manager rol beschrijving" }
                );
                context.SaveChanges();
            }
        }
    }
}