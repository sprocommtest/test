using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Serialization;

namespace MES_Web_Services
{
    public class XmlDeserialize
    {
        public class ITEMINFOS
        {
            List<ITEMINFO> iTEMINFOs = new List<ITEMINFO>();

            [XmlElement(ElementName = "ITEMINFO")]
            public List<ITEMINFO> PerList
            {
                get { return iTEMINFOs; }
                set { iTEMINFOs = value; }
            }
        }
        public static ITEMINFOS XDeserialize(string str)
        {
            MemoryStream ms = new MemoryStream();
            StreamWriter writer = new StreamWriter(ms);
            writer.Write(str.Trim());
            writer.Flush();

            ms.Position = 0;
            XmlSerializer xmlSearializer = new XmlSerializer(typeof(ITEMINFOS));
            ITEMINFOS info = (ITEMINFOS)xmlSearializer.Deserialize(ms);
            ms.Close();
            writer.Close();
            writer.Dispose();
            return info;
        }
        /// <summary>  
        /// 将字符串（符合xml格式）转换为XmlDocument  
        /// </summary>  
        /// <param name="xmlString">XML格式字符串</param>  
        /// <returns></returns>  
        public static XmlDocument ConvertStringToXmlDocument(string xmlString)
        {
            XmlDocument document = new XmlDocument();
            document.LoadXml(xmlString);
            return document;
        }
    }
}