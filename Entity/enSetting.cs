using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity
{
    public class enSetting
    {
        public int Id { get; set; }
        public string Header { get; set; }
        public string Footer { get; set; }
        public int Fields { get; set; }
        public string FileId { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public List<enSettingInfo> SettingInfo { get; set; }
        public enModel Model { get; set; }
    }
}
