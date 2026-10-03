using System;

namespace Entity
{
    public class enLine
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public int Sequence { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }
}
