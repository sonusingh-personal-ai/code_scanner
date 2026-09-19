using System;
using System.Collections.Generic;

namespace Entity
{
    public class enPrinterModel
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public List<enPrinterModelValue> ModelValues { get; set; } = new List<enPrinterModelValue>();
    }
}