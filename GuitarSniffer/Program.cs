using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using PcapDotNet.Base;
using PcapDotNet.Core;
using PcapDotNet.Packets;

namespace GuitarSniffer {
    internal class Program {
        private static readonly List<ValueTuple<Regex, Key>> converter = new List<ValueTuple<Regex, Key>>() {
            new ValueTuple<Regex, Key>(new Regex("..................00..00..01"), new Key(KeyEnum.GREEN, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A000000004000"), new Key(KeyEnum.GREEN, false)),
            new ValueTuple<Regex, Key>(new Regex("..................00..00..02"), new Key(KeyEnum.RED, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A000000004000"), new Key(KeyEnum.RED, false)),
            new ValueTuple<Regex, Key>(new Regex("..................00..00..04"), new Key(KeyEnum.YELLOW, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A000000004000"), new Key(KeyEnum.YELLOW, false)),
            new ValueTuple<Regex, Key>(new Regex("..................00..00..08"), new Key(KeyEnum.BLUE, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A000000004000"), new Key(KeyEnum.BLUE, false)),
            new ValueTuple<Regex, Key>(new Regex("..................10..00..10"), new Key(KeyEnum.ORANGE, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A000000004000"), new Key(KeyEnum.ORANGE, false)),
            new ValueTuple<Regex, Key>(new Regex("..................02..00..00"), new Key(KeyEnum.STRUM_DOWN, true)),
            new ValueTuple<Regex, Key>(new Regex("..................01..00..00"), new Key(KeyEnum.STRUM_UP, false)),
            new ValueTuple<Regex, Key>(new Regex("....00002000  0A000000004000"), new Key(KeyEnum.STRUM_RELEASE, true)),
            //new ValueTuple<Regex, Key>(new Regex("....00002000..0A000000FE4000"), new Key(KeyEnum.WHAMMY, false)),
            //new ValueTuple<Regex, Key>(new Regex("....00002000..0A000000FF4000"), new Key(KeyEnum.WHAMMY, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A100100004001"), new Key(KeyEnum.GREENSU, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A100200004001"), new Key(KeyEnum.GREENSD, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A200100004002"), new Key(KeyEnum.REDSU, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A800100004004"), new Key(KeyEnum.YELLOWSU, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A400100004008"), new Key(KeyEnum.BLUESU, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A001100004010"), new Key(KeyEnum.ORANGESU, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A200200004002"), new Key(KeyEnum.REDSD, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A800200004004"), new Key(KeyEnum.YELLOWSD, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A400200004008"), new Key(KeyEnum.BLUESD, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A001200004010"), new Key(KeyEnum.ORANGESD, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A300000004003"), new Key(KeyEnum.GREENRED, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A900000004005"), new Key(KeyEnum.GREENYELLOW, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A500000004009"), new Key(KeyEnum.GREENBLUE, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A101000004011"), new Key(KeyEnum.GREENORANGE, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0AA00000004006"), new Key(KeyEnum.REDYELLOW, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A60000000400A"), new Key(KeyEnum.REDBLUE, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A201000004012"), new Key(KeyEnum.REDORANGE, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0AC0000000400C"), new Key(KeyEnum.YELLOWBLUE, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A801000004014"), new Key(KeyEnum.YELLOWORANGE, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A401000004018"), new Key(KeyEnum.BLUEORANGE, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A300100004003"), new Key(KeyEnum.GREENREDSU, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A900100004005"), new Key(KeyEnum.GREENYELLOWSU, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A500100004009"), new Key(KeyEnum.GREENBLUESU, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A101100004011"), new Key(KeyEnum.GREENORANGESU, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0AA00100004006"), new Key(KeyEnum.REDYELLOWSU, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A60010000400A"), new Key(KeyEnum.REDBLUESU, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A201100004012"), new Key(KeyEnum.REDORANGESU, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0AC0010000400C"), new Key(KeyEnum.YELLOWBLUESU, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A801100004014"), new Key(KeyEnum.YELLOWORANGESU, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A401100004018"), new Key(KeyEnum.BLUEORANGESU, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A300200004003"), new Key(KeyEnum.GREENREDSD, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A900200004005"), new Key(KeyEnum.GREENYELLOWSD, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A500200004009"), new Key(KeyEnum.GREENBLUESD, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A101200004011"), new Key(KeyEnum.GREENORANGESD, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0AA00200004006"), new Key(KeyEnum.REDYELLOWSD, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A60020000400A"), new Key(KeyEnum.REDBLUESD, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A201200004012"), new Key(KeyEnum.REDORANGESD, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0AC0020000400C"), new Key(KeyEnum.YELLOWBLUESD, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A801200004014"), new Key(KeyEnum.YELLOWORANGESD, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A401200004018"), new Key(KeyEnum.BLUEORANGESD, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A040000004000"), new Key(KeyEnum.STARTBUTTON, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A080000004000"), new Key(KeyEnum.MENUBUTTON, true))
        };

        public static void Main(string[] args) {
            var devices = LivePacketDevice.AllLocalMachine; //get all the connected wifi devices
            LivePacketDevice xboneAdapter = null;
            try {
                xboneAdapter = devices.First(x =>
                    x.Description.ToLower().Contains("rpcap")); //get the wifi device with name 'rpcap' (usb adapter)
            } catch (FileNotFoundException ex) {
                Console.WriteLine("Please plug in the Xbox One Adapter!");
                Console.Read();
                System.Environment.Exit(-1);
            }
            using (PacketCommunicator communicator = xboneAdapter.Open(45, PacketDeviceOpenAttributes.Promiscuous, 50)
            ) {
                while (true) {
                    var result = communicator.ReceiveSomePackets(out int packetsSniffed, 5,
                        PacketHandler); //begin the packet handler
                    //Console.WriteLine($"Sniffed {packetsSniffed} packets");
                }
            }

            Console.Read();
        }

        static string DiffInString(string s, string multi) {
            var fixedS = s;
            fixedS = fixedS.Replace("88-11-A0-00-62-45-B4-F0-85-2C-7E-ED-8F-FF-", "");

            using (StringReader reader = new StringReader(multi)) {
                string line;
                while ((line = reader.ReadLine()?.Replace("88-11-A0-00-62-45-B4-F0-85-2C-7E-ED-8F-FF-", "")) != null) {
                    var aStringBuilder = new StringBuilder(fixedS);
                    for (int i = 0; i < fixedS.Length; i++) {
                        if (fixedS[i] == line[i]) {
                            aStringBuilder.Remove(i, 1);
                            aStringBuilder.Insert(i, " ");
                        }
                    }

                    fixedS = aStringBuilder.ToString();
                }
            }

            return fixedS;
        }

        //here we're trying to remember the last key pushed so that we know the key that is released next
        static KeyEnum oldKey = KeyEnum.NULL;

        private static void PacketHandler(Packet packet) {
            //Console.WriteLine($"PACKET HANDLED, SIZE: {packet.Count}");
            if (packet.Length != 40) return;

            var data = packet.Buffer;
            var data2 = BitConverter.ToString(data).Replace("-", " ");

            //var useableData = data.ReadBytes(31, 5);
            var useableData = data.ReadBytes(21, 15);

            var code = BitConverter.ToString(useableData).Replace("-", "");
            if (converter.All(o => !o.Item1.IsMatch(code))) {
                bool b = false;
                Console.WriteLine($"{(b ? "Key not handled, Data: " : "")}{BitConverter.ToString(data)}");
                return;
            }

            //Console.WriteLine("WE GOT TO HERE");
            ValueTuple<Regex, Key> key = new ValueTuple<Regex, Key>(new Regex(""), new Key());
            var count = converter.Count(o => o.Item1.IsMatch(code));
            if (count == 1) {
                key = converter.First(o => o.Item1.IsMatch(code));
                switch (key.Item2.code) {
                    case KeyEnum.GREEN:
                        InputManager.Down.Green();
                        break;
                    case KeyEnum.RED:
                        InputManager.Down.Red();
                        break;
                    case KeyEnum.YELLOW:
                        InputManager.Down.Yellow();
                        break;
                    case KeyEnum.BLUE:
                        InputManager.Down.Blue();
                        break;
                    case KeyEnum.ORANGE:
                        InputManager.Down.Orange();
                        break;
                    case KeyEnum.STRUM_UP:
                        InputManager.Up.Strum();
                        break;
                    case KeyEnum.STRUM_DOWN:
                        InputManager.Down.Strum();
                        break;
                    case KeyEnum.GREENSU:
                        InputManager.Down.Green();
                        InputManager.Up.Strum();
                        break;
                    case KeyEnum.GREENSD:
                        InputManager.Down.Green();
                        InputManager.Down.Strum();
                        break;
                    case KeyEnum.REDSU:
                        InputManager.Down.Red();
                        InputManager.Up.Strum();
                        break;
                    case KeyEnum.REDSD:
                        InputManager.Down.Red();
                        InputManager.Down.Strum();
                        break;
                    case KeyEnum.YELLOWSU:
                        InputManager.Down.Yellow();
                        InputManager.Up.Strum();
                        break;
                    case KeyEnum.YELLOWSD:
                        InputManager.Down.Yellow();
                        InputManager.Down.Strum();
                        break;
                    case KeyEnum.BLUESU:
                        InputManager.Down.Blue();
                        InputManager.Up.Strum();
                        break;
                    case KeyEnum.BLUESD:
                        InputManager.Down.Blue();
                        InputManager.Down.Strum();
                        break;
                    case KeyEnum.ORANGESU:
                        InputManager.Down.Orange();
                        InputManager.Up.Strum();
                        break;
                    case KeyEnum.ORANGESD:
                        InputManager.Down.Orange();
                        InputManager.Down.Strum();
                        break;
                    case KeyEnum.GREENRED:
                        InputManager.Down.Green();
                        InputManager.Down.Red();
                        break;
                    case KeyEnum.GREENYELLOW:
                        InputManager.Down.Green();
                        InputManager.Down.Yellow();
                        break;
                    case KeyEnum.GREENBLUE:
                        InputManager.Down.Green();
                        InputManager.Down.Blue();
                        break;
                    case KeyEnum.GREENORANGE:
                        InputManager.Down.Green();
                        InputManager.Down.Orange();
                        break;
                    case KeyEnum.REDYELLOW:
                        InputManager.Down.Red();
                        InputManager.Down.Yellow();
                        break;
                    case KeyEnum.REDBLUE:
                        InputManager.Down.Red();
                        InputManager.Down.Blue();
                        break;
                    case KeyEnum.REDORANGE:
                        InputManager.Down.Red();
                        InputManager.Down.Orange();
                        break;
                    case KeyEnum.YELLOWBLUE:
                        InputManager.Down.Yellow();
                        InputManager.Down.Blue();
                        break;
                    case KeyEnum.YELLOWORANGE:
                        InputManager.Down.Yellow();
                        InputManager.Down.Orange();
                        break;
                    case KeyEnum.BLUEORANGE:
                        InputManager.Down.Blue();
                        InputManager.Down.Orange();
                        break;
                    case KeyEnum.GREENREDSD:
                        InputManager.Down.Green();
                        InputManager.Down.Red();
                        InputManager.Down.Strum();
                        break;
                    case KeyEnum.GREENYELLOWSD:
                        InputManager.Down.Green();
                        InputManager.Down.Yellow();
                        InputManager.Down.Strum();
                        break;
                    case KeyEnum.GREENBLUESD:
                        InputManager.Down.Green();
                        InputManager.Down.Blue();
                        InputManager.Down.Strum();
                        break;
                    case KeyEnum.GREENORANGESD:
                        InputManager.Down.Green();
                        InputManager.Down.Orange();
                        InputManager.Down.Strum();
                        break;
                    case KeyEnum.REDYELLOWSD:
                        InputManager.Down.Red();
                        InputManager.Down.Yellow();
                        InputManager.Down.Strum();
                        break;
                    case KeyEnum.REDBLUESD:
                        InputManager.Down.Red();
                        InputManager.Down.Blue();
                        InputManager.Down.Strum();
                        break;
                    case KeyEnum.REDORANGESD:
                        InputManager.Down.Red();
                        InputManager.Down.Orange();
                        InputManager.Down.Strum();
                        break;
                    case KeyEnum.YELLOWBLUESD:
                        InputManager.Down.Yellow();
                        InputManager.Down.Blue();
                        InputManager.Down.Strum();
                        break;
                    case KeyEnum.YELLOWORANGESD:
                        InputManager.Down.Yellow();
                        InputManager.Down.Orange();
                        InputManager.Down.Strum();
                        break;
                    case KeyEnum.BLUEORANGESD:
                        InputManager.Down.Blue();
                        InputManager.Down.Orange();
                        InputManager.Down.Strum();
                        break;
                    case KeyEnum.GREENREDSU:
                        InputManager.Up.Green();
                        InputManager.Up.Red();
                        InputManager.Up.Strum();
                        break;
                    case KeyEnum.GREENYELLOWSU:
                        InputManager.Up.Green();
                        InputManager.Up.Yellow();
                        InputManager.Up.Strum();
                        break;
                    case KeyEnum.GREENBLUESU:
                        InputManager.Up.Green();
                        InputManager.Up.Blue();
                        InputManager.Up.Strum();
                        break;
                    case KeyEnum.GREENORANGESU:
                        InputManager.Up.Green();
                        InputManager.Up.Orange();
                        InputManager.Up.Strum();
                        break;
                    case KeyEnum.REDYELLOWSU:
                        InputManager.Up.Red();
                        InputManager.Up.Yellow();
                        InputManager.Up.Strum();
                        break;
                    case KeyEnum.REDBLUESU:
                        InputManager.Up.Red();
                        InputManager.Up.Blue();
                        InputManager.Up.Strum();
                        break;
                    case KeyEnum.REDORANGESU:
                        InputManager.Up.Red();
                        InputManager.Up.Orange();
                        InputManager.Up.Strum();
                        break;
                    case KeyEnum.YELLOWBLUESU:
                        InputManager.Up.Yellow();
                        InputManager.Up.Blue();
                        InputManager.Up.Strum();
                        break;
                    case KeyEnum.YELLOWORANGESU:
                        InputManager.Up.Yellow();
                        InputManager.Up.Orange();
                        InputManager.Up.Strum();
                        break;
                    case KeyEnum.BLUEORANGESU:
                        InputManager.Up.Blue();
                        InputManager.Up.Orange();
                        InputManager.Up.Strum();
                        break;
                    case KeyEnum.STARTBUTTON:
                        InputManager.Start();
                        break;
                    case KeyEnum.MENUBUTTON:
                        InputManager.Menu();
                        break;
                }

                oldKey = key.Item2.code;
            } else if (count > 1) {
                switch (oldKey) {
                    case KeyEnum.GREENSU:
                    case KeyEnum.GREENSD:
                    case KeyEnum.GREEN:
                        key = converter.First(o => o.Item1.IsMatch(code) && o.Item2.code == KeyEnum.GREEN);
                        InputManager.Up.Green();
                        break;
                    case KeyEnum.REDSU:
                    case KeyEnum.REDSD:
                    case KeyEnum.RED:
                        key = converter.First(o => o.Item1.IsMatch(code) && o.Item2.code == KeyEnum.RED);
                        InputManager.Up.Red();
                        break;
                    case KeyEnum.YELLOWSU:
                    case KeyEnum.YELLOWSD:
                    case KeyEnum.YELLOW:
                        key = converter.First(o => o.Item1.IsMatch(code) && o.Item2.code == KeyEnum.YELLOW);
                        InputManager.Up.Yellow();
                        break;
                    case KeyEnum.BLUESU:
                    case KeyEnum.BLUESD:
                    case KeyEnum.BLUE:
                        key = converter.First(o => o.Item1.IsMatch(code) && o.Item2.code == KeyEnum.BLUE);
                        InputManager.Up.Blue();
                        break;
                    case KeyEnum.ORANGESU:
                    case KeyEnum.ORANGESD:
                    case KeyEnum.ORANGE:
                        key = converter.First(o => o.Item1.IsMatch(code) && o.Item2.code == KeyEnum.ORANGE);
                        InputManager.Up.Orange();
                        break;
                    case KeyEnum.GREENREDSU:
                    case KeyEnum.GREENREDSD:
                    case KeyEnum.GREENRED:
                        key = converter.First(o => o.Item1.IsMatch(code) && o.Item2.code == KeyEnum.GREENRED);
                        InputManager.Up.Green();
                        InputManager.Up.Red();
                        break;
                    case KeyEnum.GREENYELLOWSU:
                    case KeyEnum.GREENYELLOWSD:
                    case KeyEnum.GREENYELLOW:
                        key = converter.First(o => o.Item1.IsMatch(code) && o.Item2.code == KeyEnum.GREENYELLOW);
                        InputManager.Up.Green();
                        InputManager.Up.Yellow();
                        break;
                    
                    case KeyEnum.GREENBLUESU:
                    case KeyEnum.GREENBLUESD:
                    case KeyEnum.GREENBLUE:
                        key = converter.First(o => o.Item1.IsMatch(code) && o.Item2.code == KeyEnum.GREENBLUE);
                        InputManager.Up.Green();
                        InputManager.Up.Blue();
                        break;
                    case KeyEnum.GREENORANGESU:
                    case KeyEnum.GREENORANGESD:
                    case KeyEnum.GREENORANGE:
                        key = converter.First(o => o.Item1.IsMatch(code) && o.Item2.code == KeyEnum.GREENORANGE);
                        InputManager.Up.Green();
                        InputManager.Up.Orange();
                        break;
                    case KeyEnum.REDYELLOWSU:
                    case KeyEnum.REDYELLOWSD:
                    case KeyEnum.REDYELLOW:
                        key = converter.First(o => o.Item1.IsMatch(code) && o.Item2.code == KeyEnum.REDYELLOW);
                        InputManager.Up.Red();
                        InputManager.Up.Yellow();
                        break;
                    case KeyEnum.REDBLUESU:
                    case KeyEnum.REDBLUESD:
                    case KeyEnum.REDBLUE:
                        key = converter.First(o => o.Item1.IsMatch(code) && o.Item2.code == KeyEnum.REDBLUE);
                        InputManager.Up.Red();
                        InputManager.Up.Blue();
                        break;
                    case KeyEnum.REDORANGESU:
                    case KeyEnum.REDORANGESD:
                    case KeyEnum.REDORANGE:
                        key = converter.First(o => o.Item1.IsMatch(code) && o.Item2.code == KeyEnum.REDORANGE);
                        InputManager.Up.Red();
                        InputManager.Up.Orange();
                        break;
                    case KeyEnum.YELLOWBLUESU:
                    case KeyEnum.YELLOWBLUESD:
                    case KeyEnum.YELLOWBLUE:
                        key = converter.First(o => o.Item1.IsMatch(code) && o.Item2.code == KeyEnum.YELLOWBLUE);
                        InputManager.Up.Yellow();
                        InputManager.Up.Blue();
                        break;
                    case KeyEnum.YELLOWORANGESU:
                    case KeyEnum.YELLOWORANGESD:
                    case KeyEnum.YELLOWORANGE:
                        key = converter.First(o => o.Item1.IsMatch(code) && o.Item2.code == KeyEnum.YELLOWORANGE);
                        InputManager.Up.Yellow();
                        InputManager.Up.Orange();
                        break;
                    case KeyEnum.BLUEORANGESU:
                    case KeyEnum.BLUEORANGESD:
                    case KeyEnum.BLUEORANGE:
                        key = converter.First(o => o.Item1.IsMatch(code) && o.Item2.code == KeyEnum.BLUEORANGE);
                        InputManager.Up.Blue();
                        InputManager.Up.Orange();
                        break;
                }

                oldKey = KeyEnum.NULL;
            }

            
            Console.WriteLine($"Fret {key.Item2.code} {(key.Item2.isDown ? "is down." : "is up.")}");
        }
    }

    struct Key {
        public KeyEnum code;
        public bool isDown;

        public Key(KeyEnum ke, bool b) {
            code = ke;
            isDown = b;
        }
    }

    enum KeyEnum {
        GREEN,
        RED,
        YELLOW,
        BLUE,
        ORANGE,
        STRUM_UP,
        STRUM_DOWN,
        STRUM_RELEASE,
        GREENRED,
        GREENYELLOW,
        GREENBLUE,
        GREENORANGE,
        REDYELLOW,
        REDBLUE,
        REDORANGE,
        YELLOWBLUE,
        YELLOWORANGE,
        BLUEORANGE,
        GREENSU,
        REDSU,
        YELLOWSU,
        BLUESU,
        ORANGESU,
        GREENSD,
        REDSD,
        YELLOWSD,
        BLUESD,
        ORANGESD,
        GREENREDSU,
        GREENYELLOWSU,
        GREENBLUESU,
        GREENORANGESU,
        REDYELLOWSU,
        REDBLUESU,
        REDORANGESU,
        YELLOWBLUESU,
        YELLOWORANGESU,
        BLUEORANGESU,
        GREENREDSD,
        GREENYELLOWSD,
        GREENBLUESD,
        GREENORANGESD,
        REDYELLOWSD,
        REDBLUESD,
        REDORANGESD,
        YELLOWBLUESD,
        YELLOWORANGESD,
        BLUEORANGESD,
        WHAMMY,
        NULL,
        STARTBUTTON,
        MENUBUTTON
    }
}