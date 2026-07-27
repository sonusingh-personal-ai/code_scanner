using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.Util
{
    public class enQrCodeResponse
    {
        public string BarCodeString { get; set; }
        public string PortNumber { get; set; }
        public int BaudRate { get; set; }
        public bool IsRepeat { get; set; }
        public string Visualby { get; set; }
        public string TestedBy { get; set; }
        public string ProductionLine { get; set; }
        public string LineInCharge { get; set; }
        public string TestingJig { get; set; }
        public string CurrentDate { get; set; }
        public string Time { get; set; }
        public bool IsRecurance { get; set; }
    }
}
