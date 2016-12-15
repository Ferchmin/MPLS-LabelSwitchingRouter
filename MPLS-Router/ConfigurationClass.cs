using System.Collections.Generic;

/*
 * Klasa odpowiedzialna za wczytywanie pliku konfiguracyjnego
 * pliki są przechowywane w postaci xml
 */

namespace MPLS_Router
{
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

        private bool incorrectConfigFileFormat;
        public bool IncorrectConfigFileFormat
        {
            get { return incorrectConfigFileFormat; }
        }



        public ConfigurationClass(string filepath)
        {
            LFIBTable = new Dictionary<string, string>();


            var config = LoadingData.Deserialization(filepath);
            if (config != null)
            {
                ManagmentIPAdd = config.ManagmentIPAdd;
                ManagmentPortNumber = config.ManagmentPortNumber;
                CloudIPAdd = config.CloudIPAdd;
                CloudPortNumber = config.CloudPortNumber;
                LocalIPAdd = config.LocalIPAdd;
                agentPortNumber = config.AgentPortNumber;
                mplsPortNumber = config.MplsPortNumber;
                foreach (var lfibpack in config.Lfiby)
                {
                    LFIBTable.Add(lfibpack.LabelIn + "&" + lfibpack.SrcInterface, lfibpack.LabelOut + "&" + lfibpack.DstInterface + "&" + lfibpack.operation);
                }
            }
            else
            {
                incorrectConfigFileFormat = true;
            }
        }
    }
}
