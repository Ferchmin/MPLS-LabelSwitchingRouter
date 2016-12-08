using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
 * Klasa odpowiadająca za działanie całego urządzenia.
 * - instancja tej klasy jest tworzona w Program, nastepnie wszystko co się dzieje przechodzi tutaj
*/

namespace MPLS_Router
{
    public class DeviceClass
    {
        private static string fileLogPath;
        private static int logID;
        private string fileConfigurationPath;
        ForwardingClass forward;
        ManagementAgent agent;
        ConfigurationClass config;
        PortsClass port;
        public ConfigurationClass Configuration
        {
            get { return config; }
            set { config = value; }
        }
        public ForwardingClass Forward
        {
            get { return forward; }
            set { forward = value; }
        }
        /*
         * Konstruktor
        */
        public DeviceClass()
        {
            ReadConfigFilePath();
            config = new MPLS_Router.ConfigurationClass(fileConfigurationPath);
            fileLogPath = config.LocalIPAdd + ".txt";
            InitializeLogLastIdNumber();
            agent = new MPLS_Router.ManagementAgent(config.LocalIPAdd, config.agentPortNumber, config.ManagmentIPAdd, config.ManagmentPortNumber, this);
            forward = new ForwardingClass(this);
            port = new PortsClass(config.LocalIPAdd, config.mplsPortNumber, config.CloudIPAdd, config.CloudPortNumber, this);
            StartWorking();
        }

        /*
         * Główna metoda programu
        */
        public void StartWorking()
        {
            MakeLog("INFO - Start working...");

            Console.WriteLine("Program działa - aby wyłączyć wpisz end.");
            Console.WriteLine("<-------------------------------------->");
            string end = null;
            do
            {
                end = Console.ReadLine();
            }
            while (end != "end");

            //LOG
            DeviceClass.MakeLog("INFO - Stop working...");
        }
        private void ReadConfigFilePath()
        {
            do
            {
                Console.WriteLine("Podaj ścieżkę pliku konfiguracyjnego");
                fileConfigurationPath = Console.ReadLine();

            } while (!File.Exists(fileConfigurationPath));
        }
        #region Logs
        public static void MakeLog(string logDescription)
        {
            string log;

            using (StreamWriter file = new StreamWriter(fileLogPath, true))
            {
                log = "#" + logID + " | " + DateTime.Now.ToString("hh:mm:ss") + " " + logDescription;
                file.WriteLine(log);
                logID++;
            }

            Console.WriteLine(log);
        }
        /*
        public static void MakeMplsLog(string logDescription)
        {
            string log;

            using (StreamWriter file = new StreamWriter(fileLogPath, true))
            {
                log = logID + " | " + DateTime.Now.ToString("hh:mm:ss") + " " + logDescription;
                file.WriteLine(log);
                logID++;
            }

            Console.WriteLine(log);
        }
        public static void MakeUdpLog(string logDescription)
        {
            string log;

            using (StreamWriter file = new StreamWriter(fileLogPath, true))
            {
                log = "//" + logID + " | " + DateTime.Now.ToString("hh:mm:ss") + " " + logDescription;
                file.WriteLine(log);
                logID++;
            }

            Console.WriteLine(log);
        }*/
        private void InitializeLogLastIdNumber()
        {
            if (File.Exists(fileLogPath))
            {
                string last = File.ReadLines(fileLogPath).Last();
                string[] tmp = last.Split('|');
                logID = Int32.Parse(tmp[0]);
                logID++;
            }
            else
                logID = 1;
        }

        #endregion
    }
}
