using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity
{
    public class enResponseSummary
    {
        public int Id { get; set; }
        public int ResponseId { get; set; }
        public string Parameters { get; set; }
        public string Dispaly { get; set; }
        public string Actual { get; set; }
        public string Status { get; set; }
        public bool IsFinal { get; set; }
    }
}
