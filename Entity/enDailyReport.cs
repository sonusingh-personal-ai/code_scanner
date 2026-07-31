using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity
{
    public class enDailyReport
    {
        public int Id { get; set; }
        public int Date { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public string FilePath { get; set; }
        public DateTime CreatedOn { get; set; }

    }
}
