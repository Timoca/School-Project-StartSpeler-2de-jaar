using API.Data.Repository;
using API.Models;
using System.Numerics;

namespace API.Data.UnitOfWork
{
    public interface IUnitOfWork
    {
        IGenericRepository<Evenement> EvenementRepository { get; }
        public void SaveChanges();
    }
}
