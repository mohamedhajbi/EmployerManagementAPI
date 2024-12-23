using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Models
{
    public class Compte
    {
        public int Id { get; set; }
        public string NumeroCompte { get; set; }
        public DateTime DateCreation { get; set; }
        public decimal Solde { get; set; }
        [ForeignKey("Client")]
        public int ClientID { get; set; }
        public string TypeCompte { get; set; }

        public Client? Client { get; set; }
        [JsonIgnore]
        public ICollection<Operation>? Operations { get; set; } = new List<Operation>();
    }

}
