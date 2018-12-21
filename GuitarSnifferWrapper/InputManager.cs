using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuitarSnifferWrapper {
    public class InputManager {

        public delegate void GuitarPacketHandler(GuitarPacket packet);
        public delegate void FretHandler(Frets frets);
        public delegate void StrumHandler(Strum strum);
        public delegate void ButtonsHandler(Buttons buttons);
        public delegate void MotionHandler(Motion motion);
        public delegate void SliderHandler(Slider slider);

        public event GuitarPacketHandler DoHandleGuitarPacket;
        public event FretHandler         UpdateTopFrets;
        public event FretHandler         UpdateBottomFrets;
        public event StrumHandler        UpdateStrum;
        public event ButtonsHandler      UpdateButtons;
        public event MotionHandler       UpdateMotion;
        public event SliderHandler       UpdateSlider;

        static InputManager _Instance;
        public static InputManager Instance { get {
                if (_Instance == null)
                    _Instance = new InputManager();
                return _Instance;
            }
        }

        public InputManager() {
            PacketServer.Instance.OnPacketDecoded += OnPacketDecoded;
            DoHandleGuitarPacket += HandlePacket;
        }

        public void OnPacketDecoded(byte[] outgoingData, byte[] incomingData) {
            if(incomingData.Length == 16)
                DoHandleGuitarPacket?.Invoke(new GuitarPacket(incomingData));
        }

        void HandlePacket(GuitarPacket packet) {
            UpdateTopFrets?.Invoke(packet.TopFrets);
            UpdateBottomFrets?.Invoke(packet.BottomFrets);
            UpdateStrum?.Invoke(packet.Strum);
            UpdateButtons?.Invoke(packet.Buttons);
            UpdateMotion?.Invoke(packet.Motion);
            UpdateSlider?.Invoke(packet.Slider);
        }
    }
}
