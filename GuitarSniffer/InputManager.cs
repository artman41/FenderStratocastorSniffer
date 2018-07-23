using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using DS4Windows;
using SharpDX.DirectInput;

//using XInputDotNetPure;

namespace GuitarSniffer {
    public static class InputManager {
        //private static GamePad GuitarGamePad { get; set; }
        public static DataValues Keys { get; set; } = new DataValues();
        private static Thread UpdateThread { get; set; }
        private static X360Device x360Bus { get; set; }
        
        static InputManager() {
            x360Bus = new X360Device();
            Console.WriteLine($"BUS OPEN: {x360Bus.Open()}");
            Console.WriteLine($"BUS START: {x360Bus.Start()}");
            if (x360Bus.Open() && x360Bus.Start())
            { 
                if (!x360Bus.Plugin(0)) {
                    Debug.WriteLine("Error 'plugging in' xinput controller.");
                }
            }
            UpdateThread = new Thread(UpdateController);
            UpdateThread.Start();
            //GuitarGamePad = new GamePad();
            //XInputDotNetPure.GamePad.GetState()
        }

        private static void UpdateController() {
            var Rumble = new byte[8];
            while (true) {
                x360Bus.Report(GetXinputData(), Rumble);
            }
        }

        static byte[] GetXinputData() {
            var xinputData = new byte[28];
            xinputData[0] = 0x1C;
            xinputData[4] = 1; //plugging in is zero indexed, but update frames are 1 indexed
            xinputData[9] = 0x14;

            for (int i = 10; i < xinputData.Length; i++)
            {
                xinputData[i] = 0;
            }
            
            xinputData[14] = 0; //LX
            xinputData[15] = 0; //LX
            xinputData[16] = 0; //LY
            xinputData[17] = 0; //LY
            
            xinputData[18] = Keys.Analogue.Whammy; //RX
            xinputData[19] = Keys.Analogue.Whammy; //RX
            //Console.WriteLine($"WHAMMY: {Keys.Analogue.Whammy}");
            
            xinputData[20] = Keys.Analogue.Acceleration; //RY
            xinputData[21] = Keys.Analogue.Acceleration; //RY
            //Console.WriteLine($"TILT: {Keys.Analogue.Acceleration}");
            
            /*if (left)
            {
                xinputData[10] |= (Byte)(1 << 2);
            }
            if (right)
            {
                xinputData[10] |= (Byte)(1 << 3);
            }*/
            if (Keys.States.Strum == States.StrumState.Up)
            {
                xinputData[10] |= (Byte)(1 << 0);
            }
            if (Keys.States.Strum == States.StrumState.Down)
            {
                xinputData[10] |= (Byte)(1 << 1);
            }
            
            if (Keys.Buttons.Green) xinputData[11] |= (Byte)(1 << 4); // A
            if (Keys.Buttons.Red) xinputData[11] |= (Byte)(1 << 5); // B
            if (Keys.Buttons.Blue) xinputData[11] |= (Byte)(1 << 6); // X
            if (Keys.Buttons.Yellow) xinputData[11] |= (Byte)(1 << 7); // Y
            
            if (Keys.Buttons.Orange) xinputData[11] |= (Byte)(1 << 0); // Left  Shoulder
            //if (r) xinputData[11] |= (Byte)(1 << 1); // Right Shoulder
            if (Keys.Buttons.Menu) xinputData[10] |= (Byte)(1 << 5); // Back
            if (Keys.Buttons.Start) xinputData[10] |= (Byte)(1 << 4); // Start
            
            xinputData[12] = 0; // Left Trigger
            xinputData[13] = 0; // Right Trigger
            
            return xinputData;
        }

        public static void Acceleration(byte accel) {
            //Console.WriteLine($"Guitar moving at speed {accel}");
            Keys.Analogue.Acceleration = accel;
        }

        public static void Whammy(byte pressure) {
            Console.WriteLine($"Whammy at pressure {pressure}");
            Keys.Analogue.Whammy = pressure;
        }

        public static void Slider(States.SliderState pos) {
            Console.WriteLine($"Slider at position {pos}");
            Keys.States.Slider = pos;
        }

        public static void Green(bool isDown) {
            Console.WriteLine($"Green is {isDown}");
            /*if (isDown) {
                keybd_event(VK_0,
                    0x45,
                    KEYEVENTF_EXTENDEDKEY | 0,
                    (UIntPtr) 0);
            } else {
                keybd_event(VK_0,
                    0x45,
                    KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP,
                    (UIntPtr) 0);
            }*/
            Keys.Buttons.Green = isDown;
        }

        public static void Red(bool isDown) {
            Console.WriteLine($"Red is {isDown}");
            /*if (isDown) {
                keybd_event(VK_1,
                    0x45,
                    KEYEVENTF_EXTENDEDKEY | 0,
                    (UIntPtr) 0);
            } else {
                keybd_event(VK_1,
                    0x45,
                    KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP,
                    (UIntPtr) 0);
            }*/
            Keys.Buttons.Red = isDown;
        }

        public static void Yellow(bool isDown) {
            Console.WriteLine($"Yellow is {isDown}");
            /*if (isDown) {
                keybd_event(VK_2,
                    0x45,
                    KEYEVENTF_EXTENDEDKEY | 0,
                    (UIntPtr) 0);
            } else {
                keybd_event(VK_2,
                    0x45,
                    KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP,
                    (UIntPtr) 0);
            }*/
            Keys.Buttons.Yellow = isDown;
        }

        public static void Blue(bool isDown) {
            Console.WriteLine($"Blue is {isDown}");
            /*
            if (isDown) {
                keybd_event(VK_3,
                    0x45,
                    KEYEVENTF_EXTENDEDKEY | 0,
                    (UIntPtr) 0);
            } else {
                keybd_event(VK_3,
                    0x45,
                    KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP,
                    (UIntPtr) 0);
            }*/
            Keys.Buttons.Blue = isDown;
        }

        public static void Orange(bool isDown) {
            Console.WriteLine($"Orange is {isDown}");
            /*
            if (isDown) {
                keybd_event(VK_4,
                    0x45,
                    KEYEVENTF_EXTENDEDKEY | 0,
                    (UIntPtr) 0);
            } else {
                keybd_event(VK_4,
                    0x45,
                    KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP,
                    (UIntPtr) 0);
            }*/
            Keys.Buttons.Orange = isDown;
        }

       // private static readonly string[] str = {"Released", "Up", "Down"};
        public static void Strum(States.StrumState state) {
            Console.WriteLine($"Strum is {state}");
            /*switch(state) {
                case 0: //released
                    keybd_event(VK_5,
                        0x45,
                        KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP,
                        (UIntPtr) 0);
                    keybd_event(VK_6,
                        0x45,
                        KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP,
                        (UIntPtr) 0);
                    break;
                case 1: //up
                    keybd_event(VK_5,
                        0x45,
                        KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP,
                        (UIntPtr) 0);
                    keybd_event(VK_6,
                        0x45,
                        KEYEVENTF_EXTENDEDKEY | 0,
                        (UIntPtr) 0);
                    break;
                case 2: //down
                    keybd_event(VK_6,
                        0x45,
                        KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP,
                        (UIntPtr) 0);
                keybd_event(VK_5,
                    0x45,
                    KEYEVENTF_EXTENDEDKEY | 0,
                    (UIntPtr) 0);
                    break;
                
            }*/
            Keys.States.Strum = state;
        }

        public static void Start(bool isDown) {
            Console.WriteLine($"Start is {isDown}");
            /*if (isDown) {
                keybd_event(VK_RETURN,
                    0x45,
                    KEYEVENTF_EXTENDEDKEY | 0,
                    (UIntPtr) 0);
            } else {
                keybd_event(VK_RETURN,
                    0x45,
                    KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP,
                    (UIntPtr) 0);
            }*/
            Keys.Buttons.Start = isDown;
        }

        public static void Menu(bool isDown) {
            Console.WriteLine($"Menu is {isDown}");
            /*if (isDown) {
                keybd_event(VK_ESCAPE,
                    0x45,
                    KEYEVENTF_EXTENDEDKEY | 0,
                    (UIntPtr) 0);
            } else {
                keybd_event(VK_ESCAPE,
                    0x45,
                    KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP,
                    (UIntPtr) 0);
            }*/
            Keys.Buttons.Menu = isDown;
        }

        #region dll stuff

        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        const int KEYEVENTF_EXTENDEDKEY = 0x1;

        const int KEYEVENTF_KEYUP = 0x2;

        private const byte VK_0 = 0x30;

        private const byte VK_1 = 0x31;

        private const byte VK_2 = 0x32;

        private const byte VK_3 = 0x33;

        private const byte VK_4 = 0x34;

        private const byte VK_5 = 0x36;

        private const byte VK_6 = 0x35;
        
        private const byte VK_7 = 0x37;
        
        private const byte VK_8 = 0x38;

        private const byte VK_RETURN = 0x0D;

        private const byte VK_TAB = 0x09;

        private const byte VK_ESCAPE = 0x1B;

        #endregion
    }
}