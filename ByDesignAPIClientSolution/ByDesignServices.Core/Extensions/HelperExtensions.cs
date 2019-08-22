using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace ByDesignServices.Core.Extensions
{
    public static class HelperExtensions
    {
        public static string SerializeObject<T>(IList<T> collection) where T : class
        {
            string res = string.Empty;

            foreach (var item in collection)
            {
                if (collection.Count > 1 && string.IsNullOrEmpty(res))
                    res = "[";
                if (!string.IsNullOrEmpty(res) && !res.Equals("["))
                    res += ",";
                var json = JsonConvert.SerializeObject(item);
                res += json;
            }

            if (collection.Count > 1 && !string.IsNullOrEmpty(res))
                res += "]";
            return res;
        }

        public static string SerializeToXml<T>(T dataToSerialize)
        {
            try
            {
                var stringwriter = new System.IO.StringWriter();
                var serializer = new XmlSerializer(typeof(T));
                serializer.Serialize(stringwriter, dataToSerialize);
                return stringwriter.ToString();
            }
            catch
            {
                throw;
            }
        }
    }
}
