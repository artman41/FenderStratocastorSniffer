using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuitarSnifferWrapper {

    #region Data Types
    public struct Frets {
        public bool Green { get; set; }
        public bool Red { get; set; }
        public bool Yellow { get; set; }
        public bool Blue { get; set; }
        public bool Orange { get; set; }

        public Frets(bool g, bool r, bool y, bool b, bool o) {
            Green = g;
            Red = r;
            Yellow = y;
            Blue = b;
            Orange = o;
        }
    }

    public struct Buttons {
        public bool Start { get; set; }
        public bool Menu { get; set; }

        public Buttons(bool s, bool m) {
            Start = s;
            Menu = m;
        }
    }

    public struct Motion {
        public byte Acceleration { get; set; }
        public byte Whammy { get; set; }

        public Motion(byte a, byte w) {
            Acceleration = a;
            Whammy = w;
        }
    }

    public enum Strum {
        NONE, UP, DOWN
    }

    public enum Slider {
        Position1,
        Position2,
        Position3,
        Position4,
        Position5,
    }

    #endregion

    public struct GuitarPacket {

        public Frets TopFrets { get; set; }
        public Frets BottomFrets { get; set; }
        public Strum Strum { get; set; }
        public Buttons Buttons { get; set; }
        public Motion Motion { get; set; }
        public Slider Slider { get; set; }

        public GuitarPacket(byte[] data) : this() {
            TopFrets = GetFrets(data.Take(5).ToArray());
            BottomFrets = GetFrets(data.Skip(5).Take(5).ToArray());
            Strum = GetStrum(data.Skip(5).Skip(5).Take(1).First());
            Buttons = GetButtons(data.Skip(5).Skip(5).Skip(1).Take(2).ToArray());
            Motion = GetMotion(data.Skip(5).Skip(5).Skip(1).Skip(2).Take(2).ToArray());
            Slider = GetSlider(data.Skip(5).Skip(5).Skip(1).Skip(2).Skip(2).Take(1).First());
        }

        Frets GetFrets(byte[] data) {
            var g = Convert.ToBoolean(data[0]);
            var r = Convert.ToBoolean(data[1]);
            var y = Convert.ToBoolean(data[2]);
            var b = Convert.ToBoolean(data[3]);
            var o = Convert.ToBoolean(data[4]);
            return new Frets(g, r, y, b, o);
        }

        Strum GetStrum(byte data) {
            switch (data) {
                case 1:
                    return Strum.DOWN;
                case 2:
                    return Strum.UP;
                default:
                    return Strum.NONE;
            }
        }

        Buttons GetButtons(byte[] data) {
            var s = Convert.ToBoolean(data[0]);
            var m = Convert.ToBoolean(data[1]);
            return new Buttons(s, m);
        }

        Motion GetMotion(byte[] data) {
            return new Motion(data[0], data[1]);
        }

        Slider GetSlider(byte data) {
            switch (data) {
                default:
                    return Slider.Position1;
                case 2:
                    return Slider.Position2;
                case 3:
                    return Slider.Position3;
                case 4:
                    return Slider.Position4;
                case 5:
                    return Slider.Position5;
            }
        }

    }
}
