using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MPLS_Router
{
    class PortsClass
    {
        #region Private Variables
        DeviceClass dev;
        Socket mySocket;
        IPEndPoint myIpEndPoint;

        IPEndPoint cloudIPEndPoint;
        EndPoint cloudEndPoint;

        byte[] buffer;
        byte[] packet;
        int bufferSize;

        string myIpAddress;
        int myPort;

        string cloudIpAddress;
        int cloudPort;

        #endregion


        /*
		* Konstruktor - wymaga podania zmiennych pobranych z pliku konfiguracyjnego
		*/
        public PortsClass(string myIpAddress, int myPort, string cloudIpAddress, int cloudPort, DeviceClass dev)
        {
            InitializeData(myIpAddress, myPort, cloudIpAddress, cloudPort, dev);
            InitializeSocket();
        }

        /*
		* Metoda odpowiedzialna za przypisanie danych do lokalnych zmiennych.
		*/
        private void InitializeData(string myIpAddress, int myPort, string cloudIpAddress, int cloudPort, DeviceClass dev)
        {
            this.dev = dev;
            this.myIpAddress = myIpAddress;
            this.myPort = myPort;
            this.cloudIpAddress = cloudIpAddress;
            this.cloudPort = cloudPort;
            this.bufferSize = 275;
        }

        /*
		* Metoda odpowiedzialna za inicjalizację nasłuchiwania na przychodzące wiadomośći.
		*/
        private void InitializeSocket()
        {
            try
            {
                //tworzymy gniazdo i przypisujemy mu numer portu i IP zgodne z plikiem konfig
                mySocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                myIpEndPoint = new IPEndPoint((IPAddress.Parse(myIpAddress)), myPort);
                mySocket.Bind(myIpEndPoint);
                DeviceClass.MakeLog("INFO - Router Socket: IP:" + myIpAddress + " Port:" + myPort);
            }
            catch
            {
                //LOG
                DeviceClass.MakeLog("ERROR - Incorrect IP address or port number or these values are already in use.");
                dev.StopWorking("Incorrect IP address or port number or these values are already in use.");
            }

            try
            {
                //tworzymy punkt końcowy chmury kablowej
                cloudIPEndPoint = new IPEndPoint((IPAddress.Parse(cloudIpAddress)), cloudPort);
                cloudEndPoint = (EndPoint)cloudIPEndPoint;
            }
            catch
            {
                //LOG
                DeviceClass.MakeLog("ERROR - Incorrect CLOUD IP address or port number.");
                dev.StopWorking("Incorrect CLOUD IP address or port number.");
            }

            //tworzymy bufor nasłuchujący
            buffer = new byte[bufferSize];

            //nasłuchujemy
            mySocket.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref cloudEndPoint, new AsyncCallback(ReceivedPacket), null);
            DeviceClass.MakeLog("INFO - Start Listening.");
        }

        /*
		* Metoda odpowiedzialna za ukończenie odbierania pakietu.
		* - tutaj generowany będzie log z wydarzenia;
		* - tutaj przesyłamy otryzmany pakiet do wewnętrznej metody odpowiedzialnej za przetwarzanie
		*/
        public void ReceivedPacket(IAsyncResult res)
        {
            int size;
            try
            {
                //kończymy odbieranie pakietu - metoda zwraca rozmiar faktycznie otrzymanych danych
                size = mySocket.EndReceiveFrom(res, ref cloudEndPoint);
            }
            catch
            {
                DeviceClass.MakeLog("ERROR - Cannot send packet. Cloud unreachable.");

                //uruchamiam ponowne nasłuchiwanie
                mySocket.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref cloudEndPoint, new AsyncCallback(ReceivedPacket), null);
                return;
            }

            //jeżeli pakiet przyszedł z chmury to analizuj go
            IPEndPoint receivedIPEndPoint = (IPEndPoint)cloudEndPoint;
            if ((receivedIPEndPoint.Address.ToString() == dev.Configuration.CloudIPAdd) && (receivedIPEndPoint.Port == dev.Configuration.CloudPortNumber))
            {
                //tworzę tablicę bajtów składającą się jedynie z danych otrzymanych (otrzymany pakiet)
                byte[] receivedPacket = new byte[size];
                Array.Copy(buffer, receivedPacket, receivedPacket.Length);

                //generujemy logi
                DeviceClass.MakeLog("INFO - Received packet from: IP: " + receivedIPEndPoint.Address + " Port: " + receivedIPEndPoint.Port);

                //zeruje bufor odbierający
                buffer = new byte[bufferSize];

                //uruchamiam ponowne nasłuchiwanie
                mySocket.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref cloudEndPoint, new AsyncCallback(ReceivedPacket), null);

                //przesyłam pakiet do metody przetwarzającej
                packet = dev.Forward.ForwardingPacket(receivedPacket);

                //jeżeli komunikacja omija agenta to od razu wysyłaj
                if (packet != null)
                    //inicjuje start wysyłania przetworzonego pakietu do nadawcy
                    mySocket.BeginSendTo(packet, 0, packet.Length, SocketFlags.None, cloudEndPoint, new AsyncCallback(SendPacket), receivedIPEndPoint);
            }
        }

        /*
		* Metoda odpowiedzialna za ukończenie wysyłania pakietu.
		* - tutaj generowany będzie log z wydarzenia;
		*/
        public void SendPacket(IAsyncResult res)
        {
            //kończymy wysyłanie pakietu - funkcja zwraca rozmiar wysłanego pakietu
            int size = mySocket.EndSendTo(res);
            var endPoint = res.AsyncState as IPEndPoint;
            
            //tworzmy log zdarzenia
            DeviceClass.MakeLog("INFO - Packet send to: IP: " + endPoint.Address + " Port: " + endPoint.Port);
        }

        /*
		* Metoda odpowiedzialna za wysyłanie wiadomości na żądania
		*/
        public void SendMyPacket(byte[] myPacket)
        {
            packet = myPacket;

            //inicjuje start wysyłania przetworzonego pakietu do nadawcy
            mySocket.BeginSendTo(packet, 0, packet.Length, SocketFlags.None, cloudEndPoint, new AsyncCallback(SendPacket), null);  
        }
    }
}
