using System.Xml;
using System.Xml.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
 * Klasa odpowiedzialna za wczytywanie pliku konfiguracyjnego
 * pliki są przechowywane w postaci xml
 */

namespace MPLS_Router
{
    /*
     * 
     * 
     */
    public class ConfigurationClass
    {
        public string ManagmentIPAdd { get; private set; }
        public int ManagmentPortNumber { get; private set; }
        public string CloudIPAdd { get; set; }
        public int CloudPortNumber { get; private set; }
        public string LocalIPAdd { get; set; }
        public int mplsPortNumber { get; private set; }
        public int agentPortNumber { get; private set; }
        public Dictionary<string, string> LFIBTable;
        public ConfigurationClass(string filepath)
        {
            LFIBTable = new Dictionary<string, string>();
            var config = LoadingData.Deserialization(filepath);
            ManagmentIPAdd = config.ManagmentIPAdd;
            ManagmentPortNumber = config.ManagmentPortNumber;
            CloudIPAdd = config.CloudIPAdd;
            CloudPortNumber = config.CloudPortNumber;
            LocalIPAdd = config.LocalIPAdd;
            agentPortNumber = config.AgentPortNumber;
            mplsPortNumber = config.MplsPortNumber;
            foreach (var lfibpack in config.Lfiby)
            {
                LFIBTable.Add(lfibpack.LabelIn + "&" + lfibpack.PortIn, lfibpack.LabelOut + "&" + lfibpack.PortOut + "&" + lfibpack.operation);
            }
        }
    }
}
