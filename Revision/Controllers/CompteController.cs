using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models;
using Revision.Repository;

namespace Revision.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompteController : ControllerBase
    {
        private readonly ICompteRepository _compteRepository;

        // Injection de la dépendance au service ICompteRepository
        public CompteController(ICompteRepository compteRepository)
        {
            _compteRepository = compteRepository;
        }

        // Récupérer tous les comptes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Compte>>> GetComptes()
        {
            var comptes = await _compteRepository.GetAll();
            return Ok(comptes);
        }

        // Récupérer un compte par ID
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Compte>> GetCompte(int id)
        {
            var compte = await _compteRepository.GetById(id);
            if (compte == null)
            {
                return NotFound($"Compte avec l'ID {id} non trouvé.");
            }
            return Ok(compte);
        }

        // Créer un nouveau compte
        [HttpPost]
        public async Task<ActionResult<Compte>> CreateCompte(Compte c)
        {
            // Vérifier que ClientID existe
            var clientExists = await _compteRepository.ClientExists(c.ClientID);
            if (!clientExists)
            {
                return BadRequest($"Client avec l'ID {c.ClientID} n'existe pas.");
            }

            var createdCompte = await _compteRepository.Add(c);
            return CreatedAtAction(nameof(GetCompte), new { id = createdCompte.Id }, createdCompte);
        }

        // Supprimer un compte
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> DeleteCompte(int id)
        {
            var compteToDelete = await _compteRepository.GetById(id);
            if (compteToDelete == null)
            {
                return NotFound($"Compte avec l'ID {id} non trouvé.");
            }

            await _compteRepository.Delete(id);
            return Ok($"Compte avec l'ID {id} supprimé avec succès.");
        }

        // Rechercher les comptes par ClientID
        [HttpGet("search/{ClientId:int}")]
        public async Task<ActionResult<IEnumerable<Compte>>> Search(int ClientId)
        {
            var comptes = await _compteRepository.Search(ClientId);
            if (!comptes.Any())
            {
                return NotFound($"Aucun compte trouvé pour le client avec l'ID {ClientId}.");
            }
            return Ok(comptes);
        }

        // Ajouter une opération (Retrait/Versement) à un compte
        [HttpPost("operation/{CompteId:int}")]
        public async Task<ActionResult<Operation>> CreateOperation(int CompteId, Operation p)
        {
            try
            {
                var compte = await _compteRepository.GetById(CompteId);
                if (compte == null)
                {
                    return NotFound($"Compte avec l'ID {CompteId} non trouvé.");
                }

                if (string.Equals(p.TypeOperation, "Retrait", StringComparison.OrdinalIgnoreCase) && compte.Solde < p.Montant)
                {
                    return BadRequest("Solde insuffisant pour effectuer le retrait.");
                }

                var operation = await _compteRepository.Add(CompteId, p);
                await _compteRepository.Maj_solde(compte, p);

                return CreatedAtAction(nameof(GetCompte), new { id = CompteId }, operation);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Erreur: {ex.Message}");
            }
        }

    }

}
