using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPLS_Router
{
    static class LoadingData
    {
        public static EntryData data { get; set; }
        public static EntryData Deserialization()
        {
            object obj;
            XmlSerializer deserializer = new XmlSerializer(typeof(EntryData));
            using (TextReader reader = new StreamReader(@"EntryData.xml"))
            {
                obj = deserializer.Deserialize(reader);
            }
            return data = obj as EntryData;
        }
        public static void Serialization()
        {
            data = new EntryData();
            XmlSerializer serializer = new XmlSerializer(typeof(EntryData));
            using (TextWriter writer = new StreamWriter("EntryData"))
            {
                serializer.Serialize(writer, data);
            }
        }
    }
}
