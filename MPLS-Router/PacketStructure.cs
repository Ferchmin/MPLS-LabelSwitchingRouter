using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
 * Klasa abstrakcyjna, służąca do tworzenia w łatwy sposób dowolnego rodzaju pakietu.
 * - zawiera zbiór funkcji ułatwiających samodzielne tworzenie różnego rodzaju pakietów; 
 */

namespace MPLS_Router
{
    public abstract class PacketStructure
    {
        protected byte[] _packet;
        protected int _size_of_packet;
        protected bool Error { get; private set; }
        public byte[] Packet
        {
            get { return _packet; }
        }

        #region WriteMethodes
        /*
         * Konstruktor tworzący - alokacja pamięci na pakiet (max 1024 bajtow - 1MB)
        */
        protected PacketStructure(int maxPacketSize)
        {
            _packet = new byte[maxPacketSize];
            _size_of_packet = 0;
        }

        /*
         * Metoda dodająca do pakietu vartośc jednego bajtu
         * - offset oznacza miejsce w pakiecie, w którym należy dodać wartość
         * - 1 bajt może zajmowąc tylko wartośc z przedziału 0-255 (8 bitów więc 2^8)
         */
        protected void WriteOneByte(int value, int offset)
        {
            if (value > 255)
                Error = true;
            else
            {
                _packet[offset] = (byte)value;
                _size_of_packet++;
            }
        }

        /*
         * Metoda dodająca do pakietu vartośc typu ushort
         * - ushort zawsze zajmuje 2 bajty
         * - offset oznacza miejsce w pakiecie, w którym należy dodać wartość
         */
        protected void WriteUShort(ushort value, int offset)
        {
            byte[] buffer = new byte[2];
            buffer = BitConverter.GetBytes(value);
            Array.Copy(buffer, 0, _packet, offset, 2);
            _size_of_packet += 2;
        }

        /*
         * Metoda dodająca do pakietu vartośc typu ushort
         * - uint zawsze zajmuje 4 bajty
         * - offset oznacza miejsce w pakiecie, w którym należy dodać wartość
         */
        protected void WriteUInt(uint value, int offset)
        {
            byte[] buffer = new byte[4];
            buffer = BitConverter.GetBytes(value);
            Array.Copy(buffer, 0, _packet, offset, 4);
            _size_of_packet += 4;
        }

        /*
         * Metoda dodająca do pakietu vartośc typu string
         * - string zawsze zajmuje tyle bajtów ile wynosi jego długość
         * - offset oznacza miejsce w pakiecie, w którym należy dodać wartość
         */
        protected void WriteString(string value, int offset)
        {
            byte[] buffer = new byte[value.Length];
            buffer = Encoding.UTF8.GetBytes(value);
            Array.Copy(buffer, 0, _packet, offset, value.Length);
            _size_of_packet += value.Length;
        }

        /*
         * Metoda finalizująca operację tworzenia pakietu
         * - skraca pakiet do długośc rzeczywistej
         * - zwraca referencję do utworzonego pakietu
         */
        protected byte[] EndMakingPacket()
        {
            Array.Resize(ref _packet, _size_of_packet);
            return _packet;
        }

        #endregion

        #region ReadMethodes
        /*
         * Konstruktor odczytujący - alokacja pamięci na pakiet (max 1024 bajtow - 1MB)
        */
        protected PacketStructure(byte[] receivedPacket)
        {
            _packet = receivedPacket;
            _size_of_packet = receivedPacket.Length;
        }
        /*
         * Metoda odczytująca z pakietu vartośc jednego bajtu
         * - offset oznacza miejsce w pakiecie, w którym należy odczytać bajt
         */
        protected byte ReadOneByte(int offset)
        {
            return _packet[offset];
        }

        /*
         * Metoda odczytująca z pakietu vartośc typu ushort
         * - ushort zawsze zajmuje 2 bajty (od offsetu do offset+2 będzie czytał)
         * - offset oznacza miejsce w pakiecie, od którego należy zacząć czytać
         */
        protected ushort ReadUShort(int offset)
        {
            return BitConverter.ToUInt16(_packet, offset); ;
        }

        /*
         * Metoda odczytująca z pakietu vartośc typu uint
         * - uint zawsze zajmuje 4 bajty (od offsetu do offset+4 będzie czytał)
         * - offset oznacza miejsce w pakiecie, od którego należy zacząć czytać
         */
        protected uint ReadUInt(int offset)
        {
            return BitConverter.ToUInt32(_packet, offset);
        }

        /*
         * Metoda odczytująca z pakietu  vartośc typu string
         * - string zawsze zajmuje tyle bajtów ile wynosi jego długość
         * (od offsetu do offset+count będzie czytał)
         * - offset oznacza miejsce w pakiecie, w którym należy dodać wartość
         * - count oznacza, jak długa jest wiadomość;
         */
        protected string ReadString(int offset, int cout)
        {
            return Encoding.UTF8.GetString(_packet, offset, cout);
        }
        #endregion

        #region EditMethodes
        /*
         * Metoda podmieniająca wartośc jednego bajtu
        */
        protected void EditOneByte(byte newValue, ushort position)
        {
            _packet[position] = newValue;
        }

        /*
         * Metoda podmieniająca wartośc tablicy bajtów
         * - nowe wartości są zamieniane w miejscu pomiędzy start a end position
        */
        protected void EditBytes(byte[] newValues, int startPosition, int endPosition)
        {
            if ((newValues.Length != (endPosition - startPosition)))
                Error = true;
            else
            {
                List<byte> newPacket = new List<byte>();

                //dodaję to co było przed zamienianymi wartościami
                byte[] beforeRest = new byte[startPosition];
                Array.Copy(_packet, 0, beforeRest, 0, startPosition);
                newPacket.AddRange(beforeRest);

                //dodaję nowe wartości
                newPacket.AddRange(newValues);

                //dodaję to co było za zamienianymi wartościami
                byte[] afterRest = new byte[_size_of_packet - endPosition];
                Array.Copy(_packet, endPosition, afterRest, 0, _size_of_packet - endPosition);
                newPacket.AddRange(afterRest);

                //tworzę nowy pakiet
                _packet = newPacket.ToArray();
            }
        }

        /*
        * Metoda dodająca wartośc tablicy bajtów
        * - nowe wartości są wklejane w miejsce position
        */
        protected void AddBytes(byte[] newValues, int position)
        {
            List<byte> newPacket = new List<byte>();

            //dodaję to co było przed wyznaczoną pozycją
            byte[] beforeRest = new byte[position];
            Array.Copy(_packet, 0, beforeRest, 0, position);
            newPacket.AddRange(beforeRest);

            //dodaję nowe wartości
            newPacket.AddRange(newValues);

            //dodaję to co było za wyznaczoną pozycją (czyli od startPosition do _size)
            byte[] afterRest = new byte[_size_of_packet - position];
            Array.Copy(_packet, position, afterRest, 0, _size_of_packet - position);
            newPacket.AddRange(afterRest);

            //tworzę nowy pakiet
            _packet = newPacket.ToArray();

            //aktualizuję rozmiar pakietu
            _size_of_packet = _packet.Length;
        }

        /*
        * Metoda usuwająca dany zakres bajtów z pakietu
        * - od miejsca possition, do position + length
        */
        protected void DeleteBytes(int position, int length)
        {
            List<byte> newPacket = new List<byte>();

            //dodaję to co było przed zamienianymi wartościami
            byte[] beforeRest = new byte[position];
            Array.Copy(_packet, 0, beforeRest, 0, position);
            newPacket.AddRange(beforeRest);

            //dodaję to co było za ustawioną pozycją o length 
            byte[] afterRest = new byte[_size_of_packet - position - length];
            Array.Copy(_packet, position + length, afterRest, 0, _size_of_packet - position - length);
            newPacket.AddRange(afterRest);

            //tworzę nowy pakiet
            _packet = newPacket.ToArray();

            //aktualizuję rozmiar pakietu
            _size_of_packet = _packet.Length;
        }


        #endregion

    }
}
