using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace MPLS_Router
{
    class ManagementAgent
    {
        DeviceClass dev;
        Socket agentSocket;
        IPEndPoint myIPEndPoint;
        IPEndPoint managementIPEndPoint;
        IPEndPoint receivedIPEndPoint;
        EndPoint managementEndPoint;
        byte[] buffer;
        byte[] packet;
        public string myIPAddress { get; private set; }
        public int agentPort { get; private set; }
        public string managementIPAddress { get; private set; }
        public int managementPort { get; private set; }

        public ManagementAgent(string myIPAddress, int agentPort, string managementIPAddress, int managementPort, DeviceClass dev)
        {
            InitializeData(myIPAddress, agentPort, managementIPAddress, managementPort, dev);
            InitializeSocket();
        }

        /*
		* Metoda odpowiedzialna za przypisanie danych do lokalnych zmiennych.
		*/
        private void InitializeData(string myIPAddress, int agentPort, string managementIPAddress, int managementPort, DeviceClass dev)
        {
            this.dev = dev;
            this.myIPAddress = myIPAddress;
            this.agentPort = agentPort;
            this.managementIPAddress = managementIPAddress;
            this.managementPort = managementPort;
        }
        private void InitializeSocket()
        {
            //tworzymy gniazdo i przypisujemy mu numer portu i IP zgodne z plikiem konfig
            agentSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            myIPEndPoint = new IPEndPoint((IPAddress.Parse(myIPAddress)), agentPort);
            agentSocket.Bind(managementIPEndPoint);

            //tworzymy punkt końcowy centrum zarzadzania
            managementIPEndPoint = new IPEndPoint((IPAddress.Parse(managementIPAddress)), managementPort);
            managementEndPoint = (EndPoint)managementIPEndPoint;
            SendIsUp();
            System.Timers.Timer aTimer = new System.Timers.Timer();
            aTimer.Elapsed += new ElapsedEventHandler(SendKeepAlive);
            aTimer.Interval = 5000;
            aTimer.Enabled = true;
            //tworzymy bufor nasłuchujący
            buffer = new byte[1024];

            //nasłuchujemy
            agentSocket.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref managementEndPoint, new AsyncCallback(ReceivedPacket), null);
        }

        private void SendIsUp()
        {
            ManagementPacket pack = new ManagementPacket();
            packet = pack.CreatePacket(0, myIPAddress, managementIPAddress);
            //inicjuje start wysyłania przetworzonego pakietu do nadawcy
            agentSocket.BeginSendTo(packet, 0, packet.Length, SocketFlags.None, managementEndPoint, new AsyncCallback(SendPacket), null);

            //tworzmy log zdarzenia
            Console.WriteLine("Wysłaliśmy pakiet do: " + receivedIPEndPoint.Address + " port " + receivedIPEndPoint.Port);
            Console.WriteLine("Pakieto to: " + Encoding.UTF8.GetString(packet));
        }
        private void SendKeepAlive(object source, ElapsedEventArgs e)
        {
            ManagementPacket pack = new ManagementPacket();
            packet = pack.CreatePacket(1, myIPAddress, managementIPAddress);
            //inicjuje start wysyłania przetworzonego pakietu do nadawcy
            agentSocket.BeginSendTo(packet, 0, packet.Length, SocketFlags.None, managementEndPoint, new AsyncCallback(SendPacket), null);

            //tworzmy log zdarzenia
            Console.WriteLine("Wysłaliśmy pakiet do: " + receivedIPEndPoint.Address + " port " + receivedIPEndPoint.Port);
            Console.WriteLine("Pakieto to: " + Encoding.UTF8.GetString(packet));
        }
        public void SendPacket(IAsyncResult res)
        {
            //kończymy wysyłanie pakietu - funkcja zwraca rozmiar wysłanego pakietu
            int size = agentSocket.EndSendTo(res);

            //tworzmy log zdarzenia
            Console.WriteLine("Wysłaliśmy pakiet do: " + receivedIPEndPoint.Address + " port " + receivedIPEndPoint.Port);
            Console.WriteLine("Pakieto to: " + Encoding.UTF8.GetString(packet));
        }

        public void ReceivedPacket(IAsyncResult res)
        {
            //kończymy odbieranie pakietu - metoda zwraca rozmiar faktycznie otrzymanych danych
            int size = agentSocket.EndReceiveFrom(res, ref managementEndPoint);

            //tworzę tablicę bajtów składającą się jedynie z danych otrzymanych (otrzymany pakiet)
            byte[] receivedPacket = new byte[size];
            Array.Copy(buffer, receivedPacket, receivedPacket.Length);

            //tworzę tymczasoyw punkt końcowy zawierający informacje o nadawcy (jego ip oraz nr portu)
            //tutaj niby zawsze będzie to z chmury kablowej więc cloudIPEndPoint powinien być tym samym co receivedIPEndPoint
            //tutaj można będzie zrobić sprawdzenie bo cloud to teoria a received to praktyka skąd przyszły dane
            receivedIPEndPoint = (IPEndPoint)managementEndPoint;

            //generujemy logi
            Console.WriteLine("Otrzymaliśmy pakiet od: " + receivedIPEndPoint.Address + " port " + receivedIPEndPoint.Port);
            Console.WriteLine("Pakieto to: " + Encoding.UTF8.GetString(receivedPacket));

            //przesyłam pakiet do metody przetwarzającej
            bool response = ProcessReceivedPacket(receivedPacket);

            //jeżeli komenda centrum zarzadzania zostala poprawnie wykonana to odsylamy wiadomosc Accepted, jezeli nie to Denied
            if (response)
            {
                ManagementPacket pack = new ManagementPacket();
                byte[] packetToSend = pack.CreatePacket(3, myIPAddress, managementIPAddress, (ushort)"Accepted".Length, "Accepted" );
                //inicjuje start wysyłania przetworzonego pakietu do nadawcy
                agentSocket.BeginSendTo(packetToSend, 0, packetToSend.Length, SocketFlags.None, managementEndPoint, new AsyncCallback(SendPacket), null);
            }
            else
            {
                ManagementPacket pack = new ManagementPacket();
                byte[] packetToSend = pack.CreatePacket(3, myIPAddress, managementIPAddress, (ushort)"Denied".Length, "Denied");
                //inicjuje start wysyłania przetworzonego pakietu do nadawcy
                agentSocket.BeginSendTo(packetToSend, 0, packetToSend.Length, SocketFlags.None, managementEndPoint, new AsyncCallback(SendPacket), null);
            }

            //zeruje bufor odbierający
            buffer = new byte[1024];

            //uruchamiam ponowne nasłuchiwanie
            agentSocket.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref managementEndPoint, new AsyncCallback(ReceivedPacket), null);
        }
        #region Obsluga komendy centrum zarzadzania
        public bool ProcessReceivedPacket(byte[] pack)
        {
            ManagementPacket packet = new ManagementPacket(pack);
            packet.ReadManagementHeader();
            if (packet.DataIdentifier == 2)
            {
                packet.ReadData();
                bool flag = ProcessCommand(packet.Data);
                return flag;
            }
            else
                return false;
        }

        public bool ProcessCommand (string command)
        {
            bool flag = false;
            string[] parts = command.Split();
            if (String.Equals(parts[0], "Add"))
                return flag = AddNewKey(parts);
            else if (String.Equals(parts[0], "Remove"))
                return flag = RemoveKey(parts);
            else
                return flag;
        }
        public bool AddNewKey(string[] part)
        {
            string key = part[1] + "&" + part[2];
            string value = part[3] + "&" + part[4];
            try
            {
                dev.Configuration.LFIBTable.Add(key, value);
                return true;
            }
            catch(ArgumentException)
            {
                return false;
            }
        }
        public bool RemoveKey(string[] part)
        {
            string key = part[1] + "&" + part[2];
            bool flag = dev.Configuration.LFIBTable.Remove(key);
            return flag;
        }
        #endregion
    }
}
