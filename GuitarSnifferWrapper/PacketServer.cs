using PcapDotNet.Packets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace GuitarSnifferWrapper {
    public class PacketServer {
        private static PacketServer _Instance;
        public static PacketServer Instance { get {
                if (_Instance == null)
                    _Instance = new PacketServer();
                return _Instance;
            }
        }
        ConcurrentQueue<Packet> Packets { get; set; }
        WebClient WebClient { get; set; }

        public delegate void PacketDecodedHandler(string outgoingData, byte[] incomingDataBytes, string incomingDataString);
        public event PacketDecodedHandler OnPacketDecoded;

        private PacketServer() {
            Packets = new ConcurrentQueue<Packet>();
            WebClient = new WebClient();
            WebClient.Headers.Add("user-agent", "GuitarSnifferWrapper");
        }

        internal void Start() {
            while (true) {
                if (Packets.Count == 0)
                    continue;
                if(Packets.TryDequeue(out var packet))
                    HandlePacket(packet);
            }
        }

        public void AddPacket(Packet p) {
            Packets.Enqueue(p);
        }

        void HandlePacket(Packet packet) {
            if (packet == null)
                return;
            try {
                var outgoingData = BitConverter.ToString(packet.Buffer);
                var incomingDataBytes = Encoding.ASCII.GetBytes(WebClient.UploadString("http://localhost:8080/packet", "POST", outgoingData));
                var incomingDataString = string.Join("-", incomingDataBytes.Select(o => Convert.ToString(o, 16)));
                OnPacketDecoded?.Invoke(outgoingData, incomingDataBytes, incomingDataString);
            } catch(WebException) {
                Debug.WriteLine("Elixir Server down");
            }
        }
    }
}
