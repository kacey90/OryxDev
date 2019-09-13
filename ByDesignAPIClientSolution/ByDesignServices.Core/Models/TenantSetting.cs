using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ByDesignServices.Core.Models
{
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public class TenantSetting
    {
        [Required]
        public string User { get; set; }

        /// <remarks/>
        [Required]
        public string Password { get; set; }

        [Required]
        public string BaseUrl { get; set; }
    }
}
