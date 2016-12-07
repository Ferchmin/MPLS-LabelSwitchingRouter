using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
 * Klasa odpowiedzialna za tworzenie i odczytywanie pakietów w domenie MPLS.
 * - tworzymy pakiet w określonej strukturze
 * - odczytujemy z niego odpowiednie nagłówki
 * - tworzymy tablice bajtów możliwą do wysłania przez sieć 
 * 
 * ----------------
 * MPLS Packet Structure
 * ----------------
 * HeaderName    -> |   CLOUD_HEADER   |  MPLS_HEADER  |         IP_HEADER          |  DATA_HEADER   |       DATA       |
 * Description   -> | Source_Interface | L_1 |...| L_N | IP_Source | IP_Destination | MESSAGE_LENGTH |     MESSAGE      |
 * Size in bytes -> |       1B        /      \         |    4B     |      4B        |      1B        | MESSAGE_LENGTH B |
 *                                   /        \
 *                                  /          \
 *                                | S  | MPLS_Label | 
 *                                | 1B |     1B     |
 *                                
 * CLOUD_HEADER: ma ustaloną długość 1 bajtów      - Source_Interface może mieć zakres 0-255! (wartość ushort)
 * MPLS_HEADER:  ma zmienną długość w zależności od liczby etykiet na stosie (dla jednej etykiety ma 2 bajty) (ushort)
 *                       - S (Bottom_of_the_stock) -> 1 bajt mówiący o tym, czy to jest wieżchołek stosu etykiet, czy nie (wartości 0 lub 1)
 *                       - MPLS_LABEL  -> 1 bajt mówiący o etykiecie (0-255!)
 * IP_HEADER:    ma ustaloną długość 8 bajtów      - przenosi adresy IP zapisane np 127001 - (127.0.0.1) każde pole ma swój bajt (ushort)
 * DATA_HEADER:  ma ustaloną długość 1 bajta       - określa, ile bajtów wiadomości przesyłamy w pakiecie (ushort)
 * DATA:         ma zmienną długość 0-255 bajtów   - przenosi wiadomość użytkową (string max 255 znaków)
 * 
 *                       
 *                       
 * OBIEKT TEJ KLASY TWORZONY JEST ZA KAŻDYM RAZEM, GDY CHCEMY UTWORZYĆ LUB ODCZYTAC (ZMODYFIKOWAC) DANE!
*/

namespace MPLS_Router
{
    class MPLSPacket : PacketStructure
    {
        #region Private Variables
        private ushort _source_interface;
        private ushort _s;
        private ushort _mpls_label;
        private string _ip_source;
        private string _ip_destination;
        private ushort _message_length;
        private string _data;
        #endregion


        #region Public Properties
        public ushort SourceInterface
        {
            get { return _source_interface; }
            set { _source_interface = value; }
        }
        public ushort BottomOfTheStack
        {
            get { return _s; }
            set { _s = value; }
        }
        public ushort MplsLabel
        {
            get { return _mpls_label; }
            set { _mpls_label = value; }
        }
        public string IpSource
        {
            get { return _ip_source; }
            set { _ip_source = value; }
        }
        public string IpDestination
        {
            get { return _ip_destination; }
            set { _ip_destination = value; }
        }
        public ushort MessageLength
        {
            get { return _message_length; }
            set { _message_length = value; }
        }
        public string Data
        {
            get { return _data; }
            set { _data = value; }
        }
        #endregion


        #region CreatingNewPacket
        /*
         * Konsruktor służący do stworzenia pakietu
         * - przypisanie wartości domyślnych (samych zer)
         * - maksymalny rozmiar pakietu ustawione na 275 BAJTY - ograniczenie do 5 zagnieżdzeń MPLS
         * (255 wiadomość + 1 długość_wiadomosci + 8 adresyIP + 5*2 MPLS_Header + 1 cloud_Header)
         */
        public MPLSPacket()
            : base(275)
        {
            _source_interface = 0;
            _s = 0;
            _mpls_label = 0;
            _ip_source = null;
            _ip_destination = null;
            _message_length = 0;
            _data = null;
        }

        /* TESTED
         * Metoda tworząca cały pakiet z tylko jedną wartością etykiety
         * - dla węzła klienckiego
        */
        public byte[] CreatePacket
            (ushort srcInt, ushort s, ushort mplsLabel, string ipScr, string ipDst, ushort messageLength, string message)
        {
            //zapisujemy nagłówek chmury oraz MPLS (zakładam tutaj jeden MPLS)
            WriteOneByte(srcInt, 0);
            WriteOneByte(s, 1);
            WriteOneByte(mplsLabel, 2);

            //dodaje ip źrodła
            string[] parsingTable;
            parsingTable = ipScr.Split('.');
            WriteOneByte(ushort.Parse(parsingTable[0]), 3);
            WriteOneByte(ushort.Parse(parsingTable[1]), 4);
            WriteOneByte(ushort.Parse(parsingTable[2]), 5);
            WriteOneByte(ushort.Parse(parsingTable[3]), 6);

            //dodaje ip docelowe
            parsingTable = ipDst.Split('.');
            WriteOneByte(ushort.Parse(parsingTable[0]), 7);
            WriteOneByte(ushort.Parse(parsingTable[1]), 8);
            WriteOneByte(ushort.Parse(parsingTable[2]), 9);
            WriteOneByte(ushort.Parse(parsingTable[3]), 10);

            //dodaję nagłówek danych oraz same dane
            WriteOneByte(messageLength, 11);
            WriteString(message, 12);

            if (Error)
            {
                Console.WriteLine("Podano błędne dane - dane poza zakresem!");
                return null;
            }
            else
                return EndMakingPacket();
        }
        #endregion


        #region ReadingFromPacket
        /*
         * Konsruktor służący do odczytania zawartości pakietu
         * - przypisanie referencji do pakietu do lokalnej zmiennej
         * - wyzerowanie wartości lokalnych
         */
        public MPLSPacket(byte[] receivedPacket)
            : base(receivedPacket)
        {
            _source_interface = 0;
            _s = 0;
            _mpls_label = 0;
            _ip_source = null;
            _ip_destination = null;
            _message_length = 0;
            _data = null;
        }

        /* TESTED
         * Metoda służaca do odczytania całego pakietu
         * - zakładam, że pakiet zawieta tylko jedną etykietę
         */
        public void ReadHolePacket()
        {
            ReadCloudHeader();
            ReadMplsHeader();
            ReadIpHeader();
            ReadDataHeader();
            ReadData();
        }
        public void ReadCloudHeader()
        {
            _source_interface = ReadOneByte(0);
        }
        public void ReadMplsHeader()
        {
            _s = ReadOneByte(1);
            _mpls_label = ReadOneByte(2);
        }
        public void ReadIpHeader()
        {
            //reading ip source
            ushort tmp_value;
            for (int i = 3; i < 7; i++)
            {
                tmp_value = ReadOneByte(i);
                _ip_source += tmp_value.ToString();
                if (i != 6) _ip_source += ".";
            }

            //reading ip destination
            for (int i = 7; i < 11; i++)
            {
                tmp_value = ReadOneByte(i);
                _ip_destination += tmp_value.ToString();
                if (i != 10) _ip_destination += ".";
            }
        }
        public void ReadDataHeader()
        {
            _message_length = ReadOneByte(11);
        }
        public void ReadData()
        {
            _data = ReadString(12, _message_length);
        }

        #endregion


        #region EditingExistingPacket

        /* TESTED
         * Metoda służaca zamiany nagłówka chmurowego.
         */
        public void ChangeCloudHeader(ushort newSourceInterface)
        {
            if (newSourceInterface > 255)
                Console.WriteLine("ChangeCloudHeader - Błędne dane do zamiany");
            else
            {
                EditOneByte((byte)newSourceInterface, 0);

                //aktualizuję zmienne lokalne
                ReadCloudHeader();
            }
        }

        /* TESTED
         * Metoda służaca do zamiany jednego nagłówka (tego pierwszego) MPLS
         */
        public void ChangeMplsHeader(ushort newS, ushort newMplsLabel)
        {
            byte[] newValues = new byte[2];
            newValues[0] = (byte)newS;
            newValues[1] = (byte)newMplsLabel;

            EditBytes(newValues, 1, 3);
            if (Error)
                Console.WriteLine("ChangeMplsHeader - Błędne dane do zamiany");
            else
                //aktualizuję zmienne lokalne
                ReadMplsHeader();
        }

        /* TESTED
         * Metoda służaca do dodawania na początku dodatkowego nagłówka MPLS.
         * - sytuacja wejścia do tunelu
         */
        public void AddMplsHeader(ushort newS, ushort newMplsLabel)
        {
            byte[] newValues = new byte[2];
            newValues[0] = (byte)newS;
            newValues[1] = (byte)newMplsLabel;

            AddBytes(newValues, 1);

            //aktualizuję zmienne label i s (te zmienne pokazują nagłówek będący na SZCZYCIE)
            ReadMplsHeader();
        }

        /* TESTED
         * Metoda służaca do usuwania jednego nagłówka MPLS z pakietu.
         * - sytuacja wyjścia z tunelu
         */
        public void DeleteMplsHeader()
        {
            DeleteBytes(1, 2);

            //aktualizuję zmienne label i s (te zmienne pokazują nagłówek będący na SZCZYCIE)
            ReadMplsHeader();
        }



        #endregion
    }
}
