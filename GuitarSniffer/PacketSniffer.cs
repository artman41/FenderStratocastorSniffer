//#define SHOWPACKET

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text.RegularExpressions;
using Microsoft.SqlServer.Server;
using PcapDotNet.Core;
using PcapDotNet.Packets;

namespace GuitarSniffer {
    public class PacketSniffer {
        private const byte Fret_Green = 0x01;
        private const byte Fret_Red = 0x02;
        private const byte Fret_Yellow = 0x04;
        private const byte Fret_Blue = 0x08;
        private const byte Fret_Orange = 0x10;

        private const byte Strum_Up = 0x01;
        private const byte Strum_Down = 0x02;

        private const byte Slider_Pos1 = 0x00;
        private const byte Slider_Pos2 = 0x10;
        private const byte Slider_Pos3 = 0x20;
        private const byte Slider_Pos4 = 0x30;
        private const byte Slider_Pos5 = 0x40;

        private const byte Button_Start = 0x04;
        private const byte Button_Menu = 0x08;

        private const byte Dpad_Up = 0x02;
        private const byte Dpad_Down = 0x01;
        private const byte Dpad_Left = 0x04;
        private const byte Dpad_Right = 0x08;

        public void Start() {
            var devices = LivePacketDevice.AllLocalMachine; //get all the connected wifi devices
            LivePacketDevice xboneAdapter = null;
            try {
                xboneAdapter = devices.FirstOrDefault(x => x.Description.ToLower().Contains("rpcap")) ?? devices.FirstOrDefault(x =>
                                   x.Description.ToLower().Contains("MT7612US_RL".ToLower())); //get the wifi device with name 'rpcap' (usb adapter)  MT7612US_RL
                if(xboneAdapter != null)
                    Console.WriteLine("Xbox One Adapter found!");
            } catch (Exception ex) {
                    Console.WriteLine("Please plug in the Xbox One Adapter!");
                    Console.Read();
                    Environment.Exit(-1);
            }

            try {
                using (PacketCommunicator communicator =
                    xboneAdapter.Open(45, PacketDeviceOpenAttributes.Promiscuous, 50)
                ) {
                    while (true) {
                        var result = communicator.ReceiveSomePackets(out int packetsSniffed, 5,
                            PacketHandler);
                    }
                }
            }
            catch (NullReferenceException ex) {
                Console.WriteLine("Please plug in the Xbox One Adapter!");
                Console.WriteLine(ex.Message);
                Console.ReadLine();
                Console.WriteLine("Currently Connected Devices:");
                foreach (var livePacketDevice in devices) {
                    Console.WriteLine($"{livePacketDevice.Name} | {livePacketDevice.Description}");
                }

                Console.ReadLine();
                Environment.Exit(-1);
            }
        }

        private DataValue oldDataValue = new DataValue() {
            Slider = 0xFF,
            Strum = 0xFF
        };

        byte fretCounter;
        
        private void PacketHandler(Packet packet) {
            if (packet.Length != 40) return;

            var data = packet.Buffer;
            
            //Console.WriteLine($"TOTAL PACKET:{Environment.NewLine}{BitConverter.ToString(data)}");

            byte[] useableData = data.ReadBytes(30, 7);
            
            //Console.WriteLine($"TRIMMED PACKET:{Environment.NewLine}{BitConverter.ToString(useableData)}");

//            SECTIONS
//            0 : MENU
//            XY,
//            X -> Menu Button & Option Button
//            Y -> Green to Blue Frets
//            1 : Strum
//            XY,
//            X -> Strum & Dpad
//            y -> Orange Fret
//            2 : Acceleration
//            3 : Whammy
//            4 : Slider
//            5 : Top Fret
//            6 : Low Fret

            DataValue dv = new DataValue {
                Buttons = useableData[0],
                Strum = useableData[1],
                Accel = useableData[2],
                Whammy = useableData[3],
                Slider = useableData[4]
            };

            if (oldDataValue.Slider == 0xFF)
                oldDataValue.Slider = dv.Slider;

            #region Top Frets

            fretCounter = useableData[5];

            if ((fretCounter ^ Fret_Green) < fretCounter) {
                dv.Fret_TopGreen = true;
                fretCounter ^= Fret_Green;
            }

            if ((fretCounter ^ Fret_Red) < fretCounter) {
                dv.Fret_TopRed = true;
                fretCounter ^= Fret_Red;
            }

            if ((fretCounter ^ Fret_Yellow) < fretCounter) {
                dv.Fret_TopYellow = true;
                fretCounter ^= Fret_Yellow;
            }

            if ((fretCounter ^ Fret_Blue) < fretCounter) {
                dv.Fret_TopBlue = true;
                fretCounter ^= Fret_Blue;
            }

            if ((fretCounter ^ Fret_Orange) < fretCounter) {
                dv.Fret_TopOrange = true;
                fretCounter ^= Fret_Orange;
            }

            #endregion

            #region Bottom Frets

            fretCounter = useableData[6];

            if ((fretCounter ^ Fret_Green) < fretCounter) {
                dv.Fret_BottomGreen = true;
                fretCounter ^= Fret_Green;
            }

            if ((fretCounter ^ Fret_Red) < fretCounter) {
                dv.Fret_BottomRed = true;
                fretCounter ^= Fret_Red;
            }

            if ((fretCounter ^ Fret_Yellow) < fretCounter) {
                dv.Fret_BottomYellow = true;
                fretCounter ^= Fret_Yellow;
            }

            if ((fretCounter ^ Fret_Blue) < fretCounter) {
                dv.Fret_BottomBlue = true;
                fretCounter ^= Fret_Blue;
            }

            if ((fretCounter ^ Fret_Orange) < fretCounter) {
                dv.Fret_BottomOrange = true;
                fretCounter ^= Fret_Orange;
            }

            #endregion

            #region Buttons

            fretCounter = dv.Buttons;

            if ((fretCounter ^ Button_Start) < fretCounter) {
                InputManager.Start(true);
                dv.Start = true;
                fretCounter ^= Button_Start;
            } else if(oldDataValue.Start) {
                InputManager.Start(false);
            }

            if ((fretCounter ^ Button_Menu) < fretCounter) {
                InputManager.Menu(true);
                dv.Menu = true;
                fretCounter ^= Button_Menu;
            } else if(oldDataValue.Menu){
                InputManager.Menu(false);
            }

            #endregion

            #region StrumInput

            if (dv.Strum != oldDataValue.Strum) {
                if ((dv.Strum ^ Strum_Up) == 0) {
                    InputManager.Strum(States.StrumState.Up);
                } else if ((dv.Strum ^ Strum_Down) == 0) {
                    InputManager.Strum(States.StrumState.Down);
                } else {
                    fretCounter = (byte) (dv.Strum ^ Fret_Orange);
                    if ((fretCounter ^ Strum_Up) == 0) {
                        InputManager.Strum(States.StrumState.Up);
                    } else if ((fretCounter ^ Strum_Down) == 0) {
                        InputManager.Strum(States.StrumState.Down);
                    } else {
                        InputManager.Strum(States.StrumState.Released);
                    }
                }
            }

            #endregion

            if (dv.Accel != 0) {
                InputManager.Acceleration(dv.Accel);
            }

            if (dv.Whammy != 0) {
                InputManager.Whammy(dv.Whammy);
            }

            #region SliderInput

            if (dv.Slider != oldDataValue.Slider) {
                oldDataValue.Slider = dv.Slider;
                if ((dv.Slider ^ Slider_Pos1) == 0) {
                    InputManager.Slider(States.SliderState.Pos1);
                } else if ((dv.Slider ^ Slider_Pos2) == 0) {
                    InputManager.Slider(States.SliderState.Pos2);
                } else if ((dv.Slider ^ Slider_Pos3) == 0) {
                    InputManager.Slider(States.SliderState.Pos3);
                } else if ((dv.Slider ^ Slider_Pos4) == 0) {
                    InputManager.Slider(States.SliderState.Pos4);
                } else if ((dv.Slider ^ Slider_Pos5) == 0) {
                    InputManager.Slider(States.SliderState.Pos5);
                }
            }

            #endregion

            #region FretInput

            #region isDown

            if (dv.Fret_TopGreen || dv.Fret_BottomGreen) {
                InputManager.Green(true);
            }

            if (dv.Fret_TopRed || dv.Fret_BottomRed) {
                InputManager.Red(true);
            }

            if (dv.Fret_TopYellow || dv.Fret_BottomYellow) {
                InputManager.Yellow(true);
            }

            if (dv.Fret_TopBlue || dv.Fret_BottomBlue) {
                InputManager.Blue(true);
            }

            if (dv.Fret_TopOrange || dv.Fret_BottomOrange) {
                InputManager.Orange(true);
            }

            #endregion

            #region !isDown

            #region Top

            if (oldDataValue.Fret_TopGreen && !dv.Fret_TopGreen) {
                InputManager.Green(false);
            }

            if (oldDataValue.Fret_TopRed && !dv.Fret_TopRed) {
                InputManager.Red(false);
            }

            if (oldDataValue.Fret_TopYellow && !dv.Fret_TopYellow) {
                InputManager.Yellow(false);
            }

            if (oldDataValue.Fret_TopBlue && !dv.Fret_TopBlue) {
                InputManager.Blue(false);
            }

            if (oldDataValue.Fret_TopOrange && !dv.Fret_TopOrange) {
                InputManager.Orange(false);
            }

            #endregion

            #region Bottom

            if (oldDataValue.Fret_BottomGreen && !dv.Fret_BottomGreen) {
                InputManager.Green(false);
            }

            if (oldDataValue.Fret_BottomRed && !dv.Fret_BottomRed) {
                InputManager.Red(false);
            }

            if (oldDataValue.Fret_BottomYellow && !dv.Fret_BottomYellow) {
                InputManager.Yellow(false);
            }

            if (oldDataValue.Fret_BottomBlue && !dv.Fret_BottomBlue) {
                InputManager.Blue(false);
            }

            if (oldDataValue.Fret_BottomOrange && !dv.Fret_BottomOrange) {
                InputManager.Orange(false);
            }

            #endregion

            #endregion

            #endregion

            oldDataValue = dv;

            #region SHOWPACKET

#if SHOWPACKET
            string s = BitConverter.ToString(useableData);

            Console.WriteLine($"Packet sent was [{s}]");
            
            #endif

            #endregion
        }
    }

    struct DataValue {
        public byte
            Buttons,
            Accel,
            Whammy,
            Slider,
            Strum;

        public bool
            Fret_TopGreen,
            Fret_TopRed,
            Fret_TopYellow,
            Fret_TopBlue,
            Fret_TopOrange,
            Fret_BottomGreen,
            Fret_BottomRed,
            Fret_BottomYellow,
            Fret_BottomBlue,
            Fret_BottomOrange,
            Start,
            Menu;
    }
}