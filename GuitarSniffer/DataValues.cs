namespace GuitarSniffer {
    public class Digital {
        
        public bool Green { get; set; }
        public bool Red { get; set; }
        public bool Yellow { get; set; }
        public bool Blue { get; set; }
        public bool Orange { get; set; }
        public bool Start { get; set; }
        public bool Menu { get; set; }
    }

    public class Analogue {
        public byte Whammy { get; set; }
        public byte Acceleration { get; set; }
    }

    public class States {
        public enum StrumState {
            Released, Up, Down
        }

        public enum SliderState {
            Pos1, Pos2, Pos3, Pos4, Pos5
        }
        
        public SliderState Slider { get; set; }
        public StrumState Strum { get; set; }
    }
    public class DataValues {
        public Digital Buttons { get; set; } = new Digital();
        public Analogue Analogue { get; set; } = new Analogue();
        public States States { get; set; } = new States();
    }
}