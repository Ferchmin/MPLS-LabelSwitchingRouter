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
        [XmlElement("ipManagement")]
        public string ManagmentIPAdd { get; set; }
        [XmlElement("portManagement")]
        public int ManagmentPortNumber { get; set; }
        [XmlElement("ipCloudSending")]
        public string CloudIPAdd { get; set; }
        [XmlElement("portCloudSending")]
        public int CloudPortNumber { get; set; }
        [XmlElement("ipMyReceiving")]
        public string LocalIPAdd { get; set; }
        [XmlElement("agentReceiving")]
        public int AgentPortNumber { get; set; }
        [XmlElement("mplsReceiving")]
        public int MplsPortNumber { get; set; }
        public struct LFIB
        {
            [XmlElement("LabelIn")]
            public int LabelIn { get; set; }
            [XmlElement("SrcInterface")]
            public int SrcInterface { get; set; }
            [XmlElement("LabelOut")]
            public int LabelOut { get; set; }
            [XmlElement("DstInterface")]
            public int DstInterface { get; set; }
            [XmlElement("Operation")]
            public string operation { get; set; }
        }
        [XmlElement("LFIB")]
        public LFIB[] Lfiby { get; set; }
        public EntryData(){
        }
    }
}
