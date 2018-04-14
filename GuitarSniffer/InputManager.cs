using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace GuitarSniffer {
    public static class InputManager {
        public static void Acceleration(byte accel) {
            //Console.WriteLine($"Guitar moving at speed {accel}");
        }

        public static void Whammy(byte pressure) {
            Console.WriteLine($"Whammy at pressure {pressure}");
        }

        public static void Slider(int pos) {
            Console.WriteLine($"Slider at position {pos}");
        }

        public static void Green(bool isDown) {
            Console.WriteLine($"Green is {isDown}");
            if (isDown) {
                keybd_event(VK_0,
                    0x45,
                    KEYEVENTF_EXTENDEDKEY | 0,
                    (UIntPtr) 0);
            } else {
                keybd_event(VK_0,
                    0x45,
                    KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP,
                    (UIntPtr) 0);
            }
        }

        public static void Red(bool isDown) {
            Console.WriteLine($"Red is {isDown}");
            if (isDown) {
                keybd_event(VK_1,
                    0x45,
                    KEYEVENTF_EXTENDEDKEY | 0,
                    (UIntPtr) 0);
            } else {
                keybd_event(VK_1,
                    0x45,
                    KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP,
                    (UIntPtr) 0);
            }
        }

        public static void Yellow(bool isDown) {
            Console.WriteLine($"Yellow is {isDown}");
            if (isDown) {
                keybd_event(VK_2,
                    0x45,
                    KEYEVENTF_EXTENDEDKEY | 0,
                    (UIntPtr) 0);
            } else {
                keybd_event(VK_2,
                    0x45,
                    KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP,
                    (UIntPtr) 0);
            }
        }

        public static void Blue(bool isDown) {
            Console.WriteLine($"Blue is {isDown}");
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
            }
        }

        public static void Orange(bool isDown) {
            Console.WriteLine($"Orange is {isDown}");
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
            }
        }

        private static readonly string[] str = {"Released", "Up", "Down"};
        public static void Strum(int state) {
            Console.WriteLine($"Strum is {str[state]}");
            switch(state) {
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
                
            }
        }

        public static void Start(bool isDown) {
            Console.WriteLine($"Start is {isDown}");
            if (isDown) {
                keybd_event(VK_RETURN,
                    0x45,
                    KEYEVENTF_EXTENDEDKEY | 0,
                    (UIntPtr) 0);
            } else {
                keybd_event(VK_RETURN,
                    0x45,
                    KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP,
                    (UIntPtr) 0);
            }
        }

        public static void Menu(bool isDown) {
            Console.WriteLine($"Menu is {isDown}");
            if (isDown) {
                keybd_event(VK_ESCAPE,
                    0x45,
                    KEYEVENTF_EXTENDEDKEY | 0,
                    (UIntPtr) 0);
            } else {
                keybd_event(VK_ESCAPE,
                    0x45,
                    KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP,
                    (UIntPtr) 0);
            }
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