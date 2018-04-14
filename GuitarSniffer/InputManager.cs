using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace GuitarSniffer {
    public static class InputManager {
        static int StrumDelay = 30;

        private static int AccelerometerMinSpeed = 113; //0x71
        private static int AccelerometerMaxSpeed = 255; //0xFF
        
        public static void Start() {
            var t = new Thread(o => {
                keybd_event(VK_RETURN, 0x45, KEYEVENTF_EXTENDEDKEY | 0, (UIntPtr) 0);
                Thread.Sleep(StrumDelay);
            });
            t.Start();
            t.Join();
        }

        public static void Menu() {
            var t = new Thread(o => {
                keybd_event(VK_TAB, 0x45, KEYEVENTF_EXTENDEDKEY | 0, (UIntPtr) 0);
                Thread.Sleep(StrumDelay);
            });
            t.Start();
            t.Join();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="speed">Takes as a Hex Value</param>
        public static void Accelerometer(string speed) {
            Console.WriteLine($"Current speed is {Convert.ToInt32(speed, 16) - AccelerometerMinSpeed}");
        }
        
        public struct Up {
            public static void Green() {
                var t = new Thread(o => {
                    keybd_event(VK_0, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, (UIntPtr) 0);
                    Thread.Sleep(StrumDelay);
                });
                t.Start();
                t.Join();
            }

            public static void Red() {
                var t = new Thread(o => {
                    keybd_event(VK_1, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, (UIntPtr) 0);
                    Thread.Sleep(StrumDelay);
                });
                t.Start();
                t.Join();
            }

            public static void Yellow() {
                var t = new Thread(o => {
                    keybd_event(VK_2, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, (UIntPtr) 0);
                    Thread.Sleep(StrumDelay);
                });
                t.Start();
                t.Join();
            }

            public static void Blue() {
                var t = new Thread(o => {
                    keybd_event(VK_3, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, (UIntPtr) 0);
                    Thread.Sleep(StrumDelay);
                });
                t.Start();
                t.Join();
            }

            public static void Orange() {
                var t = new Thread(o => {
                    keybd_event(VK_4, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, (UIntPtr) 0);
                    Thread.Sleep(StrumDelay);
                });
                t.Start();
                t.Join();
            }

            public static void Strum() {
                var t = new Thread(o => {
                    keybd_event(VK_5, 0x45, KEYEVENTF_EXTENDEDKEY | 0, (UIntPtr) 0);
                    Thread.Sleep(StrumDelay);
                    keybd_event(VK_5, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, (UIntPtr) 0);
                });
                t.Start();
                t.Join();
            }
        }

        public struct Down {
            public static void Green() {
                var t = new Thread(o => {
                    keybd_event(VK_0, 0x45, KEYEVENTF_EXTENDEDKEY | 0, (UIntPtr) 0);
                    Thread.Sleep(StrumDelay);
                });
                t.Start();
                t.Join();
            }

            public static void Red() {
                var t = new Thread(o => {
                    keybd_event(VK_1, 0x45, KEYEVENTF_EXTENDEDKEY | 0, (UIntPtr) 0);
                    Thread.Sleep(StrumDelay);
                });
                t.Start();
                t.Join();
            }

            public static void Yellow() {
                var t = new Thread(o => {
                    keybd_event(VK_2, 0x45, KEYEVENTF_EXTENDEDKEY | 0, (UIntPtr) 0);
                    Thread.Sleep(StrumDelay);
                });
                t.Start();
                t.Join();
            }

            public static void Blue() {
                var t = new Thread(o => {
                    keybd_event(VK_3, 0x45, KEYEVENTF_EXTENDEDKEY | 0, (UIntPtr) 0);
                    Thread.Sleep(StrumDelay);
                });
                t.Start();
                t.Join();
            }

            public static void Orange() {
                var t = new Thread(o => {
                    keybd_event(VK_4, 0x45, KEYEVENTF_EXTENDEDKEY | 0, (UIntPtr) 0);
                    Thread.Sleep(StrumDelay);
                });
                t.Start();
                t.Join();
            }

            public static void Strum() {
                var t = new Thread(o => {
                    keybd_event(VK_6, 0x45, KEYEVENTF_EXTENDEDKEY | 0, (UIntPtr) 0);
                    Thread.Sleep(StrumDelay);
                    keybd_event(VK_6, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, (UIntPtr) 0);
                });
                t.Start();
                t.Join();
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
        private const byte VK_5 = 0x35;
        private const byte VK_6 = 0x36;
        private const byte VK_RETURN = 0x0D;
        private const byte VK_TAB = 0x09;

        #endregion
    }
}