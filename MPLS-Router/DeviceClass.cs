using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

/*
 * Klasa odpowiadająca za działanie całego urządzenia.
 * - instancja tej klasy jest tworzona w Program, nastepnie wszystko co się dzieje przechodzi tutaj
*/

namespace MPLS_Router
{
    public class DeviceClass
    {
        #region Private Variables
        private static string fileLogPath;
        private static int logID;
        private string fileConfigurationPath;
        ForwardingClass forward;
        ManagementAgent agent;
        ConfigurationClass config;
        PortsClass port;

        private static ReaderWriterLockSlim _writeLock = new ReaderWriterLockSlim();
        #endregion

        #region Public Accessors
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
        #endregion

        /*
         * Konstruktor
        */
        public DeviceClass()
        {
            ReadConfigFilePath();
            config = new ConfigurationClass(fileConfigurationPath);

            if (!config.IncorrectConfigFileFormat)
            {
                fileLogPath = config.LocalIPAdd + ".txt";
                InitializeLogLastIdNumber();

                agent = new ManagementAgent(config.LocalIPAdd, config.agentPortNumber, config.ManagmentIPAdd, config.ManagmentPortNumber, this);
                forward = new ForwardingClass(this);
                port = new PortsClass(config.LocalIPAdd, config.mplsPortNumber, config.CloudIPAdd, config.CloudPortNumber, this);

                StartWorking();
            }
            else
                StopWorking("Your configuration file is incorect.\nPlease close the application, repair configuration file and run program again.");
        }

        private void ReadConfigFilePath()
        {
            Console.WriteLine("\nEnter the path of the configuration file:");
            fileConfigurationPath = Console.ReadLine();
            if (fileConfigurationPath == "")
                fileConfigurationPath = "LSR_3.xml";
            Console.WriteLine();

            bool fileNotExist = !File.Exists(fileConfigurationPath);

            while (fileNotExist)
            {
                Console.WriteLine("Cannot find the file. Please enter the right path.");
                fileConfigurationPath = Console.ReadLine();
                fileNotExist = !File.Exists(fileConfigurationPath);
                Console.WriteLine();
            }
        }

        /*
         * Główna metoda programu
        */
        public void StartWorking()
        {
            MakeLog("INFO - Start working.");
            MakeConsoleLog("INFO - Start working.");
            Console.WriteLine();
            Console.WriteLine("Node is working. Write 'end' to close the program.");
            Console.WriteLine("<------------------------------------------------->");
            string end = null;
            do
            {
                end = Console.ReadLine();
            }
            while (end != "end");

            //LOG
            DeviceClass.MakeLog("INFO - Stop working.");
            DeviceClass.MakeConsoleLog("INFO - Stop working.");
        }

        public void StopWorking(string reason)
        {
            Console.WriteLine();
            Console.WriteLine(reason);
            Console.WriteLine("Click 'enter' to close the application...");
            Console.ReadLine();

            //LOG
            DeviceClass.MakeLog("INFO - Stop working.");
            DeviceClass.MakeConsoleLog("INFO - Stop working.");

            //wyłącz konsolę i zwolnij calą pamięć alokowaną
            Environment.Exit(0);
        }


        #region Logs
        public static void MakeLog(string logDescription)
        {
            string log;

            log = "#" + logID + " | " + DateTime.Now.ToString("hh:mm:ss") + " " + logDescription;
            WriteToFileThreadSafe(log, fileLogPath);
            logID++;

        }
        public static void MakeConsoleLog(string logDescription)
        {
            string log;
            log = "#" + logID + " | " + DateTime.Now.ToString("hh:mm:ss") + " " + logDescription;
            Console.WriteLine(log);
        }
        private void InitializeLogLastIdNumber()
        {
            if (File.Exists(fileLogPath))
            {
                string last = File.ReadLines(fileLogPath).Last();
                string[] tmp = last.Split('|');

                string tmp2 = tmp[0].Substring(1);
                
                logID = Int32.Parse(tmp2);
                logID++;
            }
            else
                logID = 1;
        }

        /*
       * Metoda odpowiedzialna za bezpieczne zapisywanie logów do pliku.
      */
        public static void WriteToFileThreadSafe(string text, string path)
        {
            // Set Status to Locked
            _writeLock.EnterWriteLock();
            try
            {
                // Append text to the file
                using (StreamWriter sw = File.AppendText(path))
                {
                    sw.WriteLine(text);
                    sw.Close();
                }
            }
            finally
            {
                // Release lock
                _writeLock.ExitWriteLock();
            }
        }
        #endregion
    }
}
