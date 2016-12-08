using System;
using System.Collections.Generic;
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
            config = new MPLS_Router.ConfigurationClass();
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
            string end = null;
            Console.WriteLine("Program działa - aby wyłączyć wpisz end.");
            do
            {
              
                end = Console.ReadLine();
            }
            while (end != "end");
        }
    }
}
