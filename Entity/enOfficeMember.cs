using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity
{
    public class enOfficeMember
    {
        public int ID { get; set; }
        public int Type { get; set; }
        public string Name { get; set; }
        public DateTime InsertedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }
}
