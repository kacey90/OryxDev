using System;
using System.Collections.Generic;
using System.Text;

namespace ByDesignServices.Core.DBModels.DTOs
{
    public class SiteDto
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string DisplayItem => $"{ID} - {Name}";
    }
}
