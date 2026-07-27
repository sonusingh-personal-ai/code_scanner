using System;
using System.Collections.Generic;

namespace Entity
{
    public class enResponse
    {
        public int Id { get; set; }
        public string Barcode { get; set; }

        public int QcStatus { get; set; }
        public int VisualBy { get; set; }
        public int TestedBy { get; set; }
        public int ProductionLine { get; set; }
        public int ProcessEngg { get; set; }
        
        public string SerialCardNo { get; set; }
        public string Model { get; set; }
        public string ConProgNo { get; set; }
        public string DisProgNo { get; set; }
        public string SystemRating { get; set; }
        public string CurrentDate { get; set; }
        public string CurrentTime { get; set; }
        public DateTime ResponseTime { get; set; }
        public DateTime CreatedOn { get; set; }
        public int RecordsCount { get; set; }
        public int RowNumber { get; set; }
        public List<enResponseSummary> listOfResponseSummary { get; set; }
        public enResponseSummary ResponseSummary { get; set; }

        #region Additional field
        public string Port { get; set; }
        public int BaudRate { get; set; }
        public bool IsRepeat { get; set; }
        public bool IsRecurrence { get; set; }
        #endregion
    }
}
