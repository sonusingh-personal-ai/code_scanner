using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity
{
    public class enSettingInfo
    {
        public int Id { get; set; }
        public int SettingId { get; set; }
        public string Parameters { get; set; }
        public bool Status { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
