using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.Util
{
    public class enSettingResponse
    {
        public int sno { get; set; }
        public string header { get; set; }
        public string footer { get; set; }
        public string displayPv { get; set; }
        public string controlPv { get; set; }
        public string sysRating { get; set; }
        public string model { get; set; }
        public List<IntegerType> interType { get; set; }
        public int status { get; set; }
        public string message { get; set; }
        public bool isOk { get; set; } = false;
        public List<List<string>> totalString { get; set; }
        public List<enSettingInfo> SettingInfoList { get; set; }
    }

    public class IntegerType
    {
        public string parameter { get; set; }
        public string dispaly { get; set; }
        public string actual { get; set; }
        public string status { get; set; }
        public bool resp { get; set; }
    }

}
