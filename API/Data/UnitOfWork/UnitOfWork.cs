using API.Data.Repository;
using API.Models;
using System.Numerics;

namespace API.Data.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly StartspelerContext _context;

        public UnitOfWork(StartspelerContext context)
        {
            _context = context;

            EvenementRepository = new GenericRepository<Evenement>(_context);
        }

        public IGenericRepository<Evenement> EvenementRepository { get; }



        public void SaveChanges()
        {
            _context.SaveChanges();
        }
    }
}
