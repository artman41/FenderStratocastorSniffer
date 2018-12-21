using PcapDotNet.Packets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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
        TcpClient TcpClient { get; set; }
        // WebClient WebClient { get; set; }

        public delegate void PacketDecodedHandler(byte[] outgoingData, byte[] incomingData);
        public event PacketDecodedHandler OnPacketDecoded;

        private PacketServer() {
            Packets = new ConcurrentQueue<Packet>();
            //WebClient = new WebClient();
            //WebClient.Headers.Add("user-agent", "GuitarSnifferWrapper");
        }

        internal void Start() {
            Thread.Sleep(3000);
            if(TcpClient == null)
                TcpClient = new TcpClient("localhost", 3000);
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

        byte[] IncomingDataBuffer = new byte[100];
        void HandlePacket(Packet packet) {
            if (packet == null)
                return;

            NetworkStream Stream;
            try {
                Stream = TcpClient.GetStream();
            } catch(Exception ex) {
                Debug.WriteLine(ex.Message);
                TcpClient.Close();
                TcpClient.Dispose();
                try {
                    TcpClient = new TcpClient("localhost", 3000);
                } catch(SocketException) {
                    return;
                }
                Stream = TcpClient.GetStream();
            }
            try {
                var str = BitConverter.ToString(packet.Buffer);
                var bytes = Encoding.UTF8.GetBytes(str);
                Stream.Write(bytes, 0, bytes.Length);
                // var incomingDataBytes = Encoding.ASCII.GetBytes(WebClient.UploadString("http://localhost:8080/packet", "POST", outgoingData));

                byte[] response = new byte[16];
                Stream.Read(response, 0, response.Length);

                // Debug.WriteLine(string.Join("-", response.Select(o => (int)o)));

                OnPacketDecoded?.Invoke(packet.Buffer, response);
            } catch(IOException) {
                Debug.WriteLine("Elixir Server down");
            }
        }
    }
}
