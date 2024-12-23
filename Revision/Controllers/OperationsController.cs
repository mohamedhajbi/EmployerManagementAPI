using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using Revision;

namespace Revision.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OperationsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OperationsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Operations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Operation>>> GetOperations()
        {
            return await _context.Operations.ToListAsync();
        }

        // GET: api/Operations/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Operation>> GetOperation(int id)
        {
            var operation = await _context.Operations.FindAsync(id);

            if (operation == null)
            {
                return NotFound();
            }

            return operation;
        }

        // PUT: api/Operations/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOperation(int id, Operation operation)
        {
            if (id != operation.Id)
            {
                return BadRequest();
            }

            _context.Entry(operation).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OperationExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Operations
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Operation>> PostOperation(Operation operation)
        {
            // Valider le type d'opération
            if (operation.TypeOperation != "Retrait" && operation.TypeOperation != "Dépôt")
            {
                return BadRequest("TypeOperation doit être 'Retrait' ou 'Dépôt'.");
            }

            // Valider que le montant est positif
            if (operation.Montant <= 0)
            {
                return BadRequest("Le montant doit être supérieur à zéro.");
            }

            // Récupérer le compte associé
            var compte = await _context.Comptes.FindAsync(operation.CompteID);
            if (compte == null)
            {
                return BadRequest($"Compte avec ID {operation.CompteID} introuvable.");
            }

            // Début de la transaction
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Mettre à jour le solde en fonction du type d'opération
                    if (operation.TypeOperation == "Retrait")
                    {
                        if (compte.Solde < operation.Montant)
                        {
                            return BadRequest("Solde insuffisant pour effectuer le retrait.");
                        }
                        compte.Solde -= operation.Montant;
                    }
                    else if (operation.TypeOperation == "Dépôt")
                    {
                        compte.Solde += operation.Montant;
                    }

                    // Ajouter l'opération
                    _context.Operations.Add(operation);

                    // Enregistrer les modifications
                    await _context.SaveChangesAsync();

                    // Valider la transaction
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    // Annuler la transaction en cas d'erreur
                    await transaction.RollbackAsync();
                    return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
                }
            }

            // Charger le compte mis à jour pour la réponse
            operation = await _context.Operations
                .Include(o => o.Compte)
                .FirstOrDefaultAsync(o => o.Id == operation.Id);

            return CreatedAtAction("GetOperation", new { id = operation.Id }, operation);
        }


        // DELETE: api/Operations/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOperation(int id)
        {
            var operation = await _context.Operations.FindAsync(id);
            if (operation == null)
            {
                return NotFound();
            }

            _context.Operations.Remove(operation);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool OperationExists(int id)
        {
            return _context.Operations.Any(e => e.Id == id);
        }
    }
}
