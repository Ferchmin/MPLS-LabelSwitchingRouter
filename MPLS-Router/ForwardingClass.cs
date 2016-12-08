using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPLS_Router
{
    public class ForwardingClass
    {
        PortsClass port;
        DeviceClass dev;
        MPLSPacket packet;
        byte[] fpacket;
        public ForwardingClass(DeviceClass dev)
        {
            this.dev = dev;
        }

        public byte[] ForwardingPacket (byte[] receivedPacket)
        {
            packet = new MPLSPacket(receivedPacket);
            packet.ReadCloudHeader();
            packet.ReadMplsHeader();
            ushort keyPart1 = packet.SourceInterface;
            ushort keyPart2 = packet.MplsLabel;
            string key = keyPart1.ToString() + "&" + keyPart2.ToString();
            string value = dev.Configuration.LFIBTable[key];
            if (value != null)
            {
                string[] valueParts = value.Split('&');
                string cloudHeaderOut = valueParts[0];
                string labelOut = valueParts[1];
                ushort s = 1;
                string operation = valueParts[2];
                switch (operation)
                {
                    case "pop":
                        packet.DeleteMplsHeader();
                        fpacket = packet.Packet;
                        dev.Forward.ForwardingPacket(fpacket);
                        break;
                    case "swap":
                        packet.ChangeMplsHeader(packet.BottomOfTheStack, ushort.Parse(cloudHeaderOut));
                        packet.ChangeCloudHeader(ushort.Parse(labelOut));
                        fpacket = packet.Packet;
                        break;
                    case "push":
                        packet.AddMplsHeader((ushort)(packet.BottomOfTheStack + s), ushort.Parse(cloudHeaderOut));
                        packet.ChangeCloudHeader(ushort.Parse(labelOut));
                        break;
                    default:

                        break;

                }
            }
            fpacket = packet.Packet;
            return fpacket;
        }
    }
}
