using System.Xml;
using System.Xml.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPLS_Router
{
    public class EntryData
    {
        [XmlElement("ipManagment")]
        public string ManagmentIPAdd { get; set; }
        [XmlElement("portManagment")]
        public int ManagmentPortNumber { get; set; }
        [XmlElement("ipCloudSending")]
        public string CloudIPAdd { get; set; }
        [XmlElement("portCloudSending")]
        public int CloudPortNumber { get; set; }
        [XmlElement("ipCloudReceiving")]
        public string LocalIPAdd { get; set; }
        [XmlElement("portCloudReceiving")]
        public int LocalPortNumber { get; set; }
        public struct LFIB
        {
            [XmlElement("LabelIn")]
            public int LabelIn { get; set; }
            [XmlElement("PortIn")]
            public int PortIn { get; set; }
            [XmlElement("LabelOut")]
            public int LabelOut { get; set; }
            [XmlElement("PortOut")]
            public int PortOut { get; set; }
            [XmlElement("Operation")]
            public string operation { get; set; }
        }
        [XmlElement("LFIB")]
        public LFIB[] Lfiby { get; set; }
        public EntryData(){
        }
    }
}
