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
        public static EntryData Deserialization(string filepath)
        {
            object obj;
            XmlSerializer deserializer = new XmlSerializer(typeof(EntryData));
            try
            {
                using (TextReader reader = new StreamReader(@filepath))
                {
                    obj = deserializer.Deserialize(reader);
                }
                return data = obj as EntryData;
            }
            catch (Exception e)
            {
                DeviceClass.MakeLog("ERROR - Deserialization cannot be complited.");
                DeviceClass.MakeConsoleLog("ERROR - Deserialization cannot be complited.");
                return null;
            }

        }
        public static void Serialization(string filepath)
        {
            data = new EntryData();
            XmlSerializer serializer = new XmlSerializer(typeof(EntryData));
            try
            {
                using (TextWriter writer = new StreamWriter(@filepath))
                {
                    serializer.Serialize(writer, data);
                }
            }
            catch (Exception e)
            {
                DeviceClass.MakeLog("ERROR - Serialization cannot be complited.");
                DeviceClass.MakeConsoleLog("ERROR - Serialization cannot be complited.");
            }
        }
    }
}
