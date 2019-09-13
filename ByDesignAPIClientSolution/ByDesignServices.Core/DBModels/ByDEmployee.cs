using System;
using System.Collections.Generic;
using System.Text;

namespace ByDesignServices.Core.DBModels
{
    public class ByDEmployee
    {
        public int ID { get; set; }
        public string SAPId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => $"{SAPId} - {FirstName} {LastName}";
    }
}
