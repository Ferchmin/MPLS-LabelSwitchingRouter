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
            ushort keyPart2 = packet.SourceInterface;
            ushort keyPart1 = packet.MplsLabel;
            DeviceClass.MakeLog("INFO - Packet's label:" + keyPart1 + " Interface: " + keyPart2);
            string key = keyPart1.ToString() + "&" + keyPart2.ToString();

            string value = null;
            if (dev.Configuration.LFIBTable.ContainsKey(key))
                value = dev.Configuration.LFIBTable[key];

            if (value != null)
            {
                string[] valueParts = value.Split('&');
                string cloudHeaderOut = valueParts[0];
                string labelOut = valueParts[1];
                string operation = valueParts[2];
                switch (operation)
                {
                    case "pop":
                        ushort labelS = packet.BottomOfTheStack;

                        if (labelS == 0)
                        {
                            packet.DeleteMplsHeader();
                            fpacket = packet.Packet;
                            DeviceClass.MakeLog("INFO - Popped label.");

                            dev.Forward.ForwardingPacket(fpacket);
                        }
                        else
                        {
                            packet.ChangeMplsHeader(0, 0);
                            packet.ChangeCloudHeader(ushort.Parse(labelOut));
                            fpacket = packet.Packet;
                            DeviceClass.MakeLog("INFO - Popped final label.");
                        }
                        break;
                    case "swap":
                        packet.ChangeMplsHeader(packet.BottomOfTheStack, ushort.Parse(cloudHeaderOut));
                        packet.ChangeCloudHeader(ushort.Parse(labelOut));
                        fpacket = packet.Packet;
                        DeviceClass.MakeLog("INFO - Swapped label.");
                        dev.Forward.ForwardingPacket(fpacket);
                        break;
                    case "push":
                        packet.AddMplsHeader(0, ushort.Parse(cloudHeaderOut));
                        packet.ChangeCloudHeader(ushort.Parse(labelOut));
                        fpacket = packet.Packet;
                        DeviceClass.MakeLog("INFO - Pushed label.");
                        dev.Forward.ForwardingPacket(fpacket);
                        break;
                    default:

                        break;
                }

                fpacket = packet.Packet;
                return fpacket;
            }
            else
            {
                DeviceClass.MakeLog("ERROR - Cannot find the value in LFIBTable of key: " + key);
                return null;
            }
        }
    }
}
