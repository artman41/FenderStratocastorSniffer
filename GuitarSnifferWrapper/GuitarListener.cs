using PcapDotNet.Core;
using PcapDotNet.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GuitarSnifferWrapper {
    public class GuitarListener {
        static GuitarListener _Instance;
        public static GuitarListener Instance { get {
                if (_Instance == null)
                    _Instance = new GuitarListener();
                return _Instance;
            }
        }

        private GuitarListener() { }

        internal void Start() {
            var devices = LivePacketDevice.AllLocalMachine; //get all the connected wifi devices
            LivePacketDevice xboneAdapter = null;

            try {
                xboneAdapter = devices.FirstOrDefault(x => x.Description.ToLower().Contains("rpcap")) ?? devices.FirstOrDefault(x =>
                                   x.Description.ToLower().Contains("MT7612US_RL".ToLower())); //get the wifi device with name 'rpcap' (usb adapter)  MT7612US_RL
                if (xboneAdapter != null)
                    Console.WriteLine("Xbox One Adapter found!");
            } catch (Exception) {
                if (MessageBox.Show("Please plug in the Xbox One Adapter!") == MessageBoxResult.OK)
                    Environment.Exit(-1);
            }

            try {
                using (PacketCommunicator communicator =
                    xboneAdapter.Open(45, PacketDeviceOpenAttributes.Promiscuous, 50)
                ) {
                    while (true) {
                        try {
                            communicator.ReceiveSomePackets(out int packetsSniffed, 5, PacketServer.Instance.AddPacket);
                        } catch (InvalidOperationException) {
                            // looks like this can throw if you plug a controller in via usb
                        }
                     }
                }
            } catch (NullReferenceException ex) {
                if (MessageBox.Show(ex.Message, "Please plug in the Xbox One Adapter!") == MessageBoxResult.OK)
                    Environment.Exit(-1);
            }
        }
    }
}
