using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Operation
    {
        public int Id { get; set; }
        public string TypeOperation { get; set; }
        public decimal Montant { get; set; }
        public DateTime DateOperation { get; set; }
        [ForeignKey("Compte")]
        public int CompteID { get; set; }

        public Compte? Compte { get; set; }
    }

}
