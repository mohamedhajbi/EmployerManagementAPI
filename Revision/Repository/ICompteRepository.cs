using Models;

namespace Revision.Repository
{
    public interface ICompteRepository
    {
        Task<IEnumerable<Compte>> GetAll();
        Task<Compte> GetById(int CompteId);
        Task<Compte> Add(Compte c);
        Task<Compte> Update(Compte c);
        Task<Compte> Delete(int CompteId);
        Task<IEnumerable<Compte>> Search(int ClientId);
        Task<Operation> Add(int CompteId, Operation o);
        Task<Compte> Maj_solde(Compte c, Operation o);
        Task<IEnumerable<Operation>> Extrait_Compte(int numcompte, DateTime d);
        Task<double> Solde_Total(int ClientId);
        Task MAJVirement(string Compte1Id, string Compte2Id, double m);
        Task<bool> ClientExists(int clientId);
    }
}
