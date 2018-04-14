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
        private static readonly List<ValueTuple<Regex, Key>> Converter = new List<ValueTuple<Regex, Key>>() {
            new ValueTuple<Regex, Key>(new Regex("..................00..00..01"), new Key(KeyEnum.Green, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A000000004000"), new Key(KeyEnum.Green, false)),
            new ValueTuple<Regex, Key>(new Regex("..................00..00..02"), new Key(KeyEnum.Red, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A000000004000"), new Key(KeyEnum.Red, false)),
            new ValueTuple<Regex, Key>(new Regex("..................00..00..04"), new Key(KeyEnum.Yellow, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A000000004000"), new Key(KeyEnum.Yellow, false)),
            new ValueTuple<Regex, Key>(new Regex("..................00..00..08"), new Key(KeyEnum.Blue, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A000000004000"), new Key(KeyEnum.Blue, false)),
            new ValueTuple<Regex, Key>(new Regex("..................10..00..10"), new Key(KeyEnum.Orange, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A000000004000"), new Key(KeyEnum.Orange, false)),
            new ValueTuple<Regex, Key>(new Regex("..................02..00..00"), new Key(KeyEnum.StrumDown, true)),
            new ValueTuple<Regex, Key>(new Regex("..................01..00..00"), new Key(KeyEnum.StrumUp, false)),
            new ValueTuple<Regex, Key>(new Regex("....00002000  0A000000004000"), new Key(KeyEnum.StrumRelease, true)),
            //new ValueTuple<Regex, Key>(new Regex("....00002000..0A000000FE4000"), new Key(KeyEnum.WHAMMY, false)),
            //new ValueTuple<Regex, Key>(new Regex("....00002000..0A000000FF4000"), new Key(KeyEnum.WHAMMY, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A100100004001"), new Key(KeyEnum.Greensu, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A100200004001"), new Key(KeyEnum.Greensd, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A200100004002"), new Key(KeyEnum.Redsu, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A800100004004"), new Key(KeyEnum.Yellowsu, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A400100004008"), new Key(KeyEnum.Bluesu, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A001100004010"), new Key(KeyEnum.Orangesu, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A200200004002"), new Key(KeyEnum.Redsd, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A800200004004"), new Key(KeyEnum.Yellowsd, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A400200004008"), new Key(KeyEnum.Bluesd, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A001200004010"), new Key(KeyEnum.Orangesd, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A300000004003"), new Key(KeyEnum.Greenred, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A900000004005"), new Key(KeyEnum.Greenyellow, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A500000004009"), new Key(KeyEnum.Greenblue, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A101000004011"), new Key(KeyEnum.Greenorange, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0AA00000004006"), new Key(KeyEnum.Redyellow, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A60000000400A"), new Key(KeyEnum.Redblue, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A201000004012"), new Key(KeyEnum.Redorange, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0AC0000000400C"), new Key(KeyEnum.Yellowblue, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A801000004014"), new Key(KeyEnum.Yelloworange, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A401000004018"), new Key(KeyEnum.Blueorange, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A300100004003"), new Key(KeyEnum.Greenredsu, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A900100004005"), new Key(KeyEnum.Greenyellowsu, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A500100004009"), new Key(KeyEnum.Greenbluesu, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A101100004011"), new Key(KeyEnum.Greenorangesu, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0AA00100004006"), new Key(KeyEnum.Redyellowsu, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A60010000400A"), new Key(KeyEnum.Redbluesu, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A201100004012"), new Key(KeyEnum.Redorangesu, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0AC0010000400C"), new Key(KeyEnum.Yellowbluesu, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A801100004014"), new Key(KeyEnum.Yelloworangesu, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A401100004018"), new Key(KeyEnum.Blueorangesu, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A300200004003"), new Key(KeyEnum.Greenredsd, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A900200004005"), new Key(KeyEnum.Greenyellowsd, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A500200004009"), new Key(KeyEnum.Greenbluesd, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A101200004011"), new Key(KeyEnum.Greenorangesd, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0AA00200004006"), new Key(KeyEnum.Redyellowsd, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A60020000400A"), new Key(KeyEnum.Redbluesd, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A201200004012"), new Key(KeyEnum.Redorangesd, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0AC0020000400C"), new Key(KeyEnum.Yellowbluesd, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A801200004014"), new Key(KeyEnum.Yelloworangesd, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A401200004018"), new Key(KeyEnum.Blueorangesd, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A040000004000"), new Key(KeyEnum.Startbutton, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A080000004000"), new Key(KeyEnum.Menubutton, true)),
            new ValueTuple<Regex, Key>(new Regex("....00002000..0A0000..004000"), new Key(KeyEnum.Accelerometer, true))
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
                 Environment.Exit(-1);
             }
             using (PacketCommunicator communicator = xboneAdapter.Open(45, PacketDeviceOpenAttributes.Promiscuous, 50)
             ) {
                 while (true) {
                     var result = communicator.ReceiveSomePackets(out int packetsSniffed, 5,
                         PacketHandler); //begin the packet handler
                     //Console.WriteLine($"Sniffed {packetsSniffed} packets");
                 }
             }
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
        static KeyEnum _oldKey = KeyEnum.Null;

        private static void PacketHandler(Packet packet) {
            //Console.WriteLine($"PACKET HANDLED, SIZE: {packet.Count}");
            if (packet.Length != 40) return;

            var data = packet.Buffer;
            var data2 = BitConverter.ToString(data).Replace("-", " ");

            //var useableData = data.ReadBytes(31, 5);
            var useableData = data.ReadBytes(21, 15);

            var code = BitConverter.ToString(useableData).Replace("-", "");
            if (Converter.All(o => !o.Item1.IsMatch(code))) {
                bool b = false;
                Console.WriteLine($"{(b ? "Key not handled, Data: " : "")}{BitConverter.ToString(data)}");
                return;
            }

            //Console.WriteLine("WE GOT TO HERE");
            ValueTuple<Regex, Key> key = new ValueTuple<Regex, Key>(new Regex(""), new Key());
            var count = Converter.Count(o => o.Item1.IsMatch(code));
            if (count == 1) {
                key = Converter.First(o => o.Item1.IsMatch(code));
                if (key.Item2.Code == KeyEnum.Accelerometer) {
                    var speedAmount = code.Substring(22, 2);
                    //Console.WriteLine(speedAmount);
                    InputManager.Accelerometer(speedAmount);
                } else {
                    switch (key.Item2.Code) {
                        case KeyEnum.Green:
                            InputManager.Down.Green();
                            break;
                        case KeyEnum.Red:
                            InputManager.Down.Red();
                            break;
                        case KeyEnum.Yellow:
                            InputManager.Down.Yellow();
                            break;
                        case KeyEnum.Blue:
                            InputManager.Down.Blue();
                            break;
                        case KeyEnum.Orange:
                            InputManager.Down.Orange();
                            break;
                        case KeyEnum.StrumUp:
                            InputManager.Up.Strum();
                            break;
                        case KeyEnum.StrumDown:
                            InputManager.Down.Strum();
                            break;
                        case KeyEnum.Greensu:
                            InputManager.Down.Green();
                            InputManager.Up.Strum();
                            break;
                        case KeyEnum.Greensd:
                            InputManager.Down.Green();
                            InputManager.Down.Strum();
                            break;
                        case KeyEnum.Redsu:
                            InputManager.Down.Red();
                            InputManager.Up.Strum();
                            break;
                        case KeyEnum.Redsd:
                            InputManager.Down.Red();
                            InputManager.Down.Strum();
                            break;
                        case KeyEnum.Yellowsu:
                            InputManager.Down.Yellow();
                            InputManager.Up.Strum();
                            break;
                        case KeyEnum.Yellowsd:
                            InputManager.Down.Yellow();
                            InputManager.Down.Strum();
                            break;
                        case KeyEnum.Bluesu:
                            InputManager.Down.Blue();
                            InputManager.Up.Strum();
                            break;
                        case KeyEnum.Bluesd:
                            InputManager.Down.Blue();
                            InputManager.Down.Strum();
                            break;
                        case KeyEnum.Orangesu:
                            InputManager.Down.Orange();
                            InputManager.Up.Strum();
                            break;
                        case KeyEnum.Orangesd:
                            InputManager.Down.Orange();
                            InputManager.Down.Strum();
                            break;
                        case KeyEnum.Greenred:
                            InputManager.Down.Green();
                            InputManager.Down.Red();
                            break;
                        case KeyEnum.Greenyellow:
                            InputManager.Down.Green();
                            InputManager.Down.Yellow();
                            break;
                        case KeyEnum.Greenblue:
                            InputManager.Down.Green();
                            InputManager.Down.Blue();
                            break;
                        case KeyEnum.Greenorange:
                            InputManager.Down.Green();
                            InputManager.Down.Orange();
                            break;
                        case KeyEnum.Redyellow:
                            InputManager.Down.Red();
                            InputManager.Down.Yellow();
                            break;
                        case KeyEnum.Redblue:
                            InputManager.Down.Red();
                            InputManager.Down.Blue();
                            break;
                        case KeyEnum.Redorange:
                            InputManager.Down.Red();
                            InputManager.Down.Orange();
                            break;
                        case KeyEnum.Yellowblue:
                            InputManager.Down.Yellow();
                            InputManager.Down.Blue();
                            break;
                        case KeyEnum.Yelloworange:
                            InputManager.Down.Yellow();
                            InputManager.Down.Orange();
                            break;
                        case KeyEnum.Blueorange:
                            InputManager.Down.Blue();
                            InputManager.Down.Orange();
                            break;
                        case KeyEnum.Greenredsd:
                            InputManager.Down.Green();
                            InputManager.Down.Red();
                            InputManager.Down.Strum();
                            break;
                        case KeyEnum.Greenyellowsd:
                            InputManager.Down.Green();
                            InputManager.Down.Yellow();
                            InputManager.Down.Strum();
                            break;
                        case KeyEnum.Greenbluesd:
                            InputManager.Down.Green();
                            InputManager.Down.Blue();
                            InputManager.Down.Strum();
                            break;
                        case KeyEnum.Greenorangesd:
                            InputManager.Down.Green();
                            InputManager.Down.Orange();
                            InputManager.Down.Strum();
                            break;
                        case KeyEnum.Redyellowsd:
                            InputManager.Down.Red();
                            InputManager.Down.Yellow();
                            InputManager.Down.Strum();
                            break;
                        case KeyEnum.Redbluesd:
                            InputManager.Down.Red();
                            InputManager.Down.Blue();
                            InputManager.Down.Strum();
                            break;
                        case KeyEnum.Redorangesd:
                            InputManager.Down.Red();
                            InputManager.Down.Orange();
                            InputManager.Down.Strum();
                            break;
                        case KeyEnum.Yellowbluesd:
                            InputManager.Down.Yellow();
                            InputManager.Down.Blue();
                            InputManager.Down.Strum();
                            break;
                        case KeyEnum.Yelloworangesd:
                            InputManager.Down.Yellow();
                            InputManager.Down.Orange();
                            InputManager.Down.Strum();
                            break;
                        case KeyEnum.Blueorangesd:
                            InputManager.Down.Blue();
                            InputManager.Down.Orange();
                            InputManager.Down.Strum();
                            break;
                        case KeyEnum.Greenredsu:
                            InputManager.Up.Green();
                            InputManager.Up.Red();
                            InputManager.Up.Strum();
                            break;
                        case KeyEnum.Greenyellowsu:
                            InputManager.Up.Green();
                            InputManager.Up.Yellow();
                            InputManager.Up.Strum();
                            break;
                        case KeyEnum.Greenbluesu:
                            InputManager.Up.Green();
                            InputManager.Up.Blue();
                            InputManager.Up.Strum();
                            break;
                        case KeyEnum.Greenorangesu:
                            InputManager.Up.Green();
                            InputManager.Up.Orange();
                            InputManager.Up.Strum();
                            break;
                        case KeyEnum.Redyellowsu:
                            InputManager.Up.Red();
                            InputManager.Up.Yellow();
                            InputManager.Up.Strum();
                            break;
                        case KeyEnum.Redbluesu:
                            InputManager.Up.Red();
                            InputManager.Up.Blue();
                            InputManager.Up.Strum();
                            break;
                        case KeyEnum.Redorangesu:
                            InputManager.Up.Red();
                            InputManager.Up.Orange();
                            InputManager.Up.Strum();
                            break;
                        case KeyEnum.Yellowbluesu:
                            InputManager.Up.Yellow();
                            InputManager.Up.Blue();
                            InputManager.Up.Strum();
                            break;
                        case KeyEnum.Yelloworangesu:
                            InputManager.Up.Yellow();
                            InputManager.Up.Orange();
                            InputManager.Up.Strum();
                            break;
                        case KeyEnum.Blueorangesu:
                            InputManager.Up.Blue();
                            InputManager.Up.Orange();
                            InputManager.Up.Strum();
                            break;
                        case KeyEnum.Startbutton:
                            InputManager.Start();
                            break;
                        case KeyEnum.Menubutton:
                            InputManager.Menu();
                            break;
                    }

                    _oldKey = key.Item2.Code;
                }
            } else if (count > 1) {
                switch (_oldKey) {
                    case KeyEnum.Greensu:
                    case KeyEnum.Greensd:
                    case KeyEnum.Green:
                        key = Converter.First(o => o.Item1.IsMatch(code) && o.Item2.Code == KeyEnum.Green);
                        InputManager.Up.Green();
                        break;
                    case KeyEnum.Redsu:
                    case KeyEnum.Redsd:
                    case KeyEnum.Red:
                        key = Converter.First(o => o.Item1.IsMatch(code) && o.Item2.Code == KeyEnum.Red);
                        InputManager.Up.Red();
                        break;
                    case KeyEnum.Yellowsu:
                    case KeyEnum.Yellowsd:
                    case KeyEnum.Yellow:
                        key = Converter.First(o => o.Item1.IsMatch(code) && o.Item2.Code == KeyEnum.Yellow);
                        InputManager.Up.Yellow();
                        break;
                    case KeyEnum.Bluesu:
                    case KeyEnum.Bluesd:
                    case KeyEnum.Blue:
                        key = Converter.First(o => o.Item1.IsMatch(code) && o.Item2.Code == KeyEnum.Blue);
                        InputManager.Up.Blue();
                        break;
                    case KeyEnum.Orangesu:
                    case KeyEnum.Orangesd:
                    case KeyEnum.Orange:
                        key = Converter.First(o => o.Item1.IsMatch(code) && o.Item2.Code == KeyEnum.Orange);
                        InputManager.Up.Orange();
                        break;
                    case KeyEnum.Greenredsu:
                    case KeyEnum.Greenredsd:
                    case KeyEnum.Greenred:
                        key = Converter.First(o => o.Item1.IsMatch(code) && o.Item2.Code == KeyEnum.Greenred);
                        InputManager.Up.Green();
                        InputManager.Up.Red();
                        break;
                    case KeyEnum.Greenyellowsu:
                    case KeyEnum.Greenyellowsd:
                    case KeyEnum.Greenyellow:
                        key = Converter.First(o => o.Item1.IsMatch(code) && o.Item2.Code == KeyEnum.Greenyellow);
                        InputManager.Up.Green();
                        InputManager.Up.Yellow();
                        break;
                    
                    case KeyEnum.Greenbluesu:
                    case KeyEnum.Greenbluesd:
                    case KeyEnum.Greenblue:
                        key = Converter.First(o => o.Item1.IsMatch(code) && o.Item2.Code == KeyEnum.Greenblue);
                        InputManager.Up.Green();
                        InputManager.Up.Blue();
                        break;
                    case KeyEnum.Greenorangesu:
                    case KeyEnum.Greenorangesd:
                    case KeyEnum.Greenorange:
                        key = Converter.First(o => o.Item1.IsMatch(code) && o.Item2.Code == KeyEnum.Greenorange);
                        InputManager.Up.Green();
                        InputManager.Up.Orange();
                        break;
                    case KeyEnum.Redyellowsu:
                    case KeyEnum.Redyellowsd:
                    case KeyEnum.Redyellow:
                        key = Converter.First(o => o.Item1.IsMatch(code) && o.Item2.Code == KeyEnum.Redyellow);
                        InputManager.Up.Red();
                        InputManager.Up.Yellow();
                        break;
                    case KeyEnum.Redbluesu:
                    case KeyEnum.Redbluesd:
                    case KeyEnum.Redblue:
                        key = Converter.First(o => o.Item1.IsMatch(code) && o.Item2.Code == KeyEnum.Redblue);
                        InputManager.Up.Red();
                        InputManager.Up.Blue();
                        break;
                    case KeyEnum.Redorangesu:
                    case KeyEnum.Redorangesd:
                    case KeyEnum.Redorange:
                        key = Converter.First(o => o.Item1.IsMatch(code) && o.Item2.Code == KeyEnum.Redorange);
                        InputManager.Up.Red();
                        InputManager.Up.Orange();
                        break;
                    case KeyEnum.Yellowbluesu:
                    case KeyEnum.Yellowbluesd:
                    case KeyEnum.Yellowblue:
                        key = Converter.First(o => o.Item1.IsMatch(code) && o.Item2.Code == KeyEnum.Yellowblue);
                        InputManager.Up.Yellow();
                        InputManager.Up.Blue();
                        break;
                    case KeyEnum.Yelloworangesu:
                    case KeyEnum.Yelloworangesd:
                    case KeyEnum.Yelloworange:
                        key = Converter.First(o => o.Item1.IsMatch(code) && o.Item2.Code == KeyEnum.Yelloworange);
                        InputManager.Up.Yellow();
                        InputManager.Up.Orange();
                        break;
                    case KeyEnum.Blueorangesu:
                    case KeyEnum.Blueorangesd:
                    case KeyEnum.Blueorange:
                        key = Converter.First(o => o.Item1.IsMatch(code) && o.Item2.Code == KeyEnum.Blueorange);
                        InputManager.Up.Blue();
                        InputManager.Up.Orange();
                        break;
                }

                _oldKey = KeyEnum.Null;
            }

            
            Console.WriteLine(count == 1 ? $"Fret {key.Item2.Code} {(key.Item2.IsDown ? "is down." : "is up.")}" : $"Accelerometer moving at Speed {0}");
        }
    }

    struct Key {
        public KeyEnum Code;
        public bool IsDown;

        public Key(KeyEnum ke, bool b) {
            Code = ke;
            IsDown = b;
        }
    }

    enum KeyEnum {
        Accelerometer,
        Green,
        Red,
        Yellow,
        Blue,
        Orange,
        StrumUp,
        StrumDown,
        StrumRelease,
        Greenred,
        Greenyellow,
        Greenblue,
        Greenorange,
        Redyellow,
        Redblue,
        Redorange,
        Yellowblue,
        Yelloworange,
        Blueorange,
        Greensu,
        Redsu,
        Yellowsu,
        Bluesu,
        Orangesu,
        Greensd,
        Redsd,
        Yellowsd,
        Bluesd,
        Orangesd,
        Greenredsu,
        Greenyellowsu,
        Greenbluesu,
        Greenorangesu,
        Redyellowsu,
        Redbluesu,
        Redorangesu,
        Yellowbluesu,
        Yelloworangesu,
        Blueorangesu,
        Greenredsd,
        Greenyellowsd,
        Greenbluesd,
        Greenorangesd,
        Redyellowsd,
        Redbluesd,
        Redorangesd,
        Yellowbluesd,
        Yelloworangesd,
        Blueorangesd,
        Whammy,
        Null,
        Startbutton,
        Menubutton
    }
}