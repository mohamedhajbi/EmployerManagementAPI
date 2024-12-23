using Microsoft.EntityFrameworkCore;
using Models;

namespace Revision.Repository
{
    public class CompteRepository : ICompteRepository
    {
        private readonly AppDbContext _context;

        public CompteRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Compte>> GetAll()
        {
            return await _context.Comptes.ToListAsync();
        }

        public async Task<Compte> GetById(int CompteId)
        {
            return await _context.Comptes
                .Include(c => c.Client)
                .FirstOrDefaultAsync(c => c.Id == CompteId);
        }

        public async Task<Compte> Add(Compte c)
        {
            await _context.Comptes.AddAsync(c);
            await _context.SaveChangesAsync();
            return c;
        }

        public async Task<Compte> Update(Compte c)
        {
            _context.Comptes.Update(c);
            await _context.SaveChangesAsync();
            return c;
        }

        public async Task<Compte> Delete(int CompteId)
        {
            var compte = await _context.Comptes.FindAsync(CompteId);
            if (compte != null)
            {
                _context.Comptes.Remove(compte);
                await _context.SaveChangesAsync();
            }
            return compte;
        }

        public async Task<IEnumerable<Compte>> Search(int ClientId)
        {
            return await _context.Comptes
                .Where(c => c.ClientID == ClientId)
                .Include(c => c.Client)
                .ToListAsync();
        }

        public async Task<Operation> Add(int CompteId, Operation o)
        {
            o.CompteID = CompteId;
            await _context.Operations.AddAsync(o);
            await _context.SaveChangesAsync();
            return o;
        }

        

        public async Task<IEnumerable<Operation>> Extrait_Compte(int numcompte, DateTime d)
        {
            return await _context.Operations
                .Where(o => o.CompteID == numcompte && o.DateOperation >= d)
                .OrderBy(o => o.DateOperation)
                .ToListAsync();
        }

        public async Task<double> Solde_Total(int ClientId)
        {
            return (double)await _context.Comptes
                .Where(c => c.ClientID == ClientId)
                .SumAsync(c => c.Solde);
        }

        public async Task MAJVirement(string Compte1Id, string Compte2Id, double m)
        {
            var compte1 = await _context.Comptes.FindAsync(int.Parse(Compte1Id));
            var compte2 = await _context.Comptes.FindAsync(int.Parse(Compte2Id));

            if (compte1 != null && compte2 != null && compte1.Solde >= (decimal)m)
            {
                compte1.Solde -= (decimal)m;
                compte2.Solde += (decimal)m;

                _context.Comptes.Update(compte1);
                _context.Comptes.Update(compte2);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<bool> ClientExists(int clientId)
        {
            return await _context.Clients.AnyAsync(c => c.Id == clientId);
        }

        public async Task<Compte> Maj_solde(Compte c, Operation o)
        {
            if (string.Equals(o.TypeOperation, "Retrait", StringComparison.OrdinalIgnoreCase))
            {
                c.Solde -= o.Montant;
            }
            else if (string.Equals(o.TypeOperation, "Versement", StringComparison.OrdinalIgnoreCase))
            {
                c.Solde += o.Montant;
            }

            _context.Comptes.Update(c);
            await _context.SaveChangesAsync();
            return c;
        }




    }
}
