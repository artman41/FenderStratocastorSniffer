using PcapDotNet.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace GuitarSnifferWrapper {
    public class PacketServer {
        public static PacketServer Instance { get; set; } = null;
        MainWindow MainWindow { get; set; }
        Queue<Packet> Packets { get; set; }
        WebClient WebClient { get; set; }

        public delegate void PacketDecodedHandler(string outgoingData, string incomingData);
        public event PacketDecodedHandler OnPacketDecoded;

        private PacketServer(MainWindow mw) {
            Instance = this;
            MainWindow = mw;
            Packets = new Queue<Packet>();
            WebClient = new WebClient();
            WebClient.Headers.Add("user-agent", "GuitarSnifferWrapper");
            OnPacketDecoded += mw.Instance_OnPacketDecoded;
        }

        public static PacketServer Create(MainWindow mw) {
            if (Instance != null)
                return Instance;
            return new PacketServer(mw);
        }

        internal void Start() {
            while (true) {
                if (Packets.Count == 0)
                    continue;
                HandlePacket(Packets.Dequeue());
            }
        }

        public void AddPacket(Packet p) {
            Packets.Enqueue(p);
        }

        void HandlePacket(Packet packet) {
            if (packet == null)
                return;
            var outgoingData = BitConverter.ToString(packet.Buffer).Replace("-", ":");
            var incomingData = WebClient.UploadString("http://localhost:8080/packet", "POST", outgoingData);
            OnPacketDecoded?.Invoke(outgoingData, incomingData);
        }
    }
}
