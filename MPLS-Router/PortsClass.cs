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
        DeviceClass dev;
        Socket mySocket;
        IPEndPoint myIpEndPoint;

        IPEndPoint cloudIPEndPoint;
        IPEndPoint receivedIPEndPoint;
        EndPoint cloudEndPoint;

        byte[] buffer;
        byte[] packet;

        string myIpAddress;
        int myPort;

        string cloudIpAddress;
        int cloudPort;

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
        }

        /*
		* Metoda odpowiedzialna za inicjalizację nasłuchiwania na przychodzące wiadomośći.
		*/
        private void InitializeSocket()
        {
            //tworzymy gniazdo i przypisujemy mu numer portu i IP zgodne z plikiem konfig
            mySocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            myIpEndPoint = new IPEndPoint((IPAddress.Parse(myIpAddress)), myPort);
            mySocket.Bind(myIpEndPoint);
            DeviceClass.MakeLog("INFO - Router Socket: IP:" + myIpAddress + " Port:" + myPort);
            //tworzymy punkt końcowy chmury kablowej
            cloudIPEndPoint = new IPEndPoint((IPAddress.Parse(cloudIpAddress)), cloudPort);
            cloudEndPoint = (EndPoint)cloudIPEndPoint;
            
            //tworzymy bufor nasłuchujący
            buffer = new byte[1024];

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
            //kończymy odbieranie pakietu - metoda zwraca rozmiar faktycznie otrzymanych danych
            int size = mySocket.EndReceiveFrom(res, ref cloudEndPoint);

            //tworzę tablicę bajtów składającą się jedynie z danych otrzymanych (otrzymany pakiet)
            byte[] receivedPacket = new byte[size];
            Array.Copy(buffer, receivedPacket, receivedPacket.Length);

            //tworzę tymczasoyw punkt końcowy zawierający informacje o nadawcy (jego ip oraz nr portu)
            //tutaj niby zawsze będzie to z chmury kablowej więc cloudIPEndPoint powinien być tym samym co receivedIPEndPoint
            //tutaj można będzie zrobić sprawdzenie bo cloud to teoria a received to praktyka skąd przyszły dane
            receivedIPEndPoint = (IPEndPoint)cloudEndPoint;

            //generujemy logi
            DeviceClass.MakeLog("INFO - Received packet from: IP:" + receivedIPEndPoint.Address + " Port: " + receivedIPEndPoint.Port);


            //przesyłam pakiet do metody przetwarzającej
            packet = dev.Forward.ForwardingPacket(receivedPacket);

            //jeżeli komunikacja omija agenta to od razu wysyłaj
            if(true)
                //inicjuje start wysyłania przetworzonego pakietu do nadawcy
                mySocket.BeginSendTo(packet, 0, packet.Length, SocketFlags.None, cloudEndPoint, new AsyncCallback(SendPacket), null);


            //zeruje bufor odbierający
            buffer = new byte[1024];

            //uruchamiam ponowne nasłuchiwanie
            mySocket.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref cloudEndPoint, new AsyncCallback(ReceivedPacket), null);
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
            DeviceClass.MakeLog("INFO - Packet send to: IP:" + endPoint.Address + " Port: " + endPoint.Port);

        }

        /*
		* Metoda odpowiedzialna za przetwarzanie odebranego pakietu.
		*/
     /*   private void ProcessReceivedPacket(byte[] receivedPacket)
        {
            //w celach testowych przypisuje ten sam pakiet co przyszedł do wysłania
            packet = receivedPacket;
        }
        */
        /*
		* Metoda odpowiedzialna za wysyłanie wiadomości na żądania
		*/
        public void SendMyPacket(byte[] myPacket)
        {
            packet = myPacket;

            //inicjuje start wysyłania przetworzonego pakietu do nadawcy
            mySocket.BeginSendTo(packet, 0, packet.Length, SocketFlags.None, cloudEndPoint, new AsyncCallback(SendPacket), null);

            //tworzmy log zdarzenia
            //Console.WriteLine("Wysłaliśmy pakiet do: " + receivedIPEndPoint.Address + " port " + receivedIPEndPoint.Port);
            
        }
    }
}
