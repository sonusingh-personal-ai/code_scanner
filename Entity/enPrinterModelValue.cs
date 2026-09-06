using System;

namespace Entity
{
    public class enPrinterModelValue
    {
        public int? Id { get; set; }
        public int ModelId { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }
}