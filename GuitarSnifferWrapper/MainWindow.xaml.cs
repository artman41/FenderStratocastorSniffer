using SimWinInput;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace GuitarSnifferWrapper {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        struct LItem {
            public string Time { get; set; }
            public string Packet { get; set; }
            public string Return { get; set; }

            public LItem(string t, string p, string r) {
                Time = t;
                Packet = p;
                Return = r;
            }
        }

        public string CoreOutput { get; set; } = "";

        SimGamePad GamePad => SimGamePad.Instance;

        public MainWindow() {
            GamePad.Initialize();
            InitEventHandlers();
            var init = new Thread(() => Util.Init(this));
            InitializeComponent();
            GamePad.PlugIn();
            ((INotifyCollectionChanged)ListViewPackets.Items).CollectionChanged += ListViewPackets_CollectionChanged;
            init.Start();
        }

        public void InitEventHandlers() {
            Application.Current.Exit += OnExit;
            InputManager.Instance.UpdateTopFrets += UpdateTopFrets;
            InputManager.Instance.UpdateBottomFrets += UpdateBottomFrets;
            InputManager.Instance.UpdateButtons += UpdateButtons;
            InputManager.Instance.UpdateSlider += UpdateSlider;
            InputManager.Instance.UpdateStrum += UpdateStrum;
            InputManager.Instance.UpdateMotion += UpdateMotion;
            PacketServer.Instance.OnPacketDecoded += OnPacketDecoded;
        }

        private void UpdateMotion(Motion motion) {
            try {
                // The constant UI updates freeze the UI thread and cause a knock-on effect slowing everything else down
                LabelAcceleration.Dispatcher.Invoke(() => {
                    //LabelAcceleration.Content = Convert.ToInt32(motion.Acceleration);
                }, DispatcherPriority.Background);

            } catch (Exception ex) {
                Debug.WriteLine(ex.Message);
            }
            GamePad.State[0].LeftTrigger = motion.Acceleration;

            try {
                // The constant UI updates freeze the UI thread and cause a knock-on effect slowing everything else down
                LabelWhammy.Dispatcher.Invoke(() => {
                    //LabelWhammy.Content = Convert.ToInt32(motion.Whammy);
                }, DispatcherPriority.Background);
            } catch (Exception ex) {
                Debug.WriteLine(ex.Message);
            }
            GamePad.State[0].RightTrigger = motion.Whammy;

            GamePad.Update();
        }

        Strum Strum { get; set; } = Strum.NONE;

        private void UpdateStrum(Strum strum) {
            try {
                LabelStrumValue.Dispatcher.Invoke(() => {
                    LabelStrumValue.Content = strum.ToString();
                }, DispatcherPriority.Background);
            } catch (Exception ex) {
                Debug.WriteLine(ex.Message);
            }

            if (strum != Strum) {
                switch (Strum) {
                    case Strum.UP:
                        GamePad.State[0].Buttons ^= GamePadControl.DPadUp;
                        break;
                    case Strum.DOWN:
                        GamePad.State[0].Buttons ^= GamePadControl.DPadDown;
                        break;
                    default:
                        break;
                }
                switch (strum) {
                    case Strum.UP:
                        GamePad.State[0].Buttons |= GamePadControl.DPadUp;
                        break;
                    case Strum.DOWN:
                        GamePad.State[0].Buttons |= GamePadControl.DPadDown;
                        break;
                    default:
                        break;
                }
            }
            
            GamePad.Update();

           Strum = strum;
        }

        private void UpdateSlider(Slider slider) {
            try {
                LabelSliderValue.Dispatcher.Invoke(() => {
                    LabelSliderValue.Content = slider.ToString();
                }, DispatcherPriority.Background);
            } catch (Exception ex) {
                Debug.WriteLine(ex.Message);
            }
        }

        private void UpdateButtons(Buttons buttons) {
            try {
                LabelStart.Dispatcher.Invoke(() => {
                    LabelStart.Foreground = buttons.Start ? Brushes.Green : Brushes.Black;
                }, DispatcherPriority.Background);
            } catch (Exception ex) {
                Debug.WriteLine(ex.Message);
            }
            if (buttons.Start)
                GamePad.Use(GamePadControl.Start);
            try {
                LabelStart.Dispatcher.Invoke(() => {
                    LabelMenu.Foreground = buttons.Menu ? Brushes.Green : Brushes.Black;
                }, DispatcherPriority.Background);
            } catch (Exception ex) {
                Debug.WriteLine(ex.Message);
            }
            if (buttons.Menu)
                GamePad.Use(GamePadControl.Back);
        }

        private void UpdateTopFrets(Frets frets) {
            UpdateFrets(frets);
            try {
                Dispatcher.Invoke(() => {
                    LabelTopGreen.Foreground = frets.Green ? Brushes.Green : Brushes.Black;
                    LabelTopRed.Foreground = frets.Red ? Brushes.Green : Brushes.Black;
                    LabelTopYellow.Foreground = frets.Yellow ? Brushes.Green : Brushes.Black;
                    LabelTopBlue.Foreground = frets.Blue ? Brushes.Green : Brushes.Black;
                    LabelTopOrange.Foreground = frets.Orange ? Brushes.Green : Brushes.Black;
                }, DispatcherPriority.Background);
            } catch (Exception ex) {
                Debug.WriteLine(ex.Message);
            }
        }

        private void UpdateBottomFrets(Frets frets) {
            UpdateFrets(frets, false);
            try { 
                Dispatcher.Invoke(() => {
                    LabelBottomGreen.Foreground = frets.Green ? Brushes.Green : Brushes.Black;
                    LabelBottomRed.Foreground = frets.Red ? Brushes.Green : Brushes.Black;
                    LabelBottomYellow.Foreground = frets.Yellow ? Brushes.Green : Brushes.Black;
                    LabelBottomBlue.Foreground = frets.Blue ? Brushes.Green : Brushes.Black;
                    LabelBottomOrange.Foreground = frets.Orange ? Brushes.Green : Brushes.Black;
                }, DispatcherPriority.Background);
            } catch (Exception ex) {
                Debug.WriteLine(ex.Message);
            }
        }

        enum FretColor {
            Green, Green2,
            Red, Red2,
            Yellow, Yellow2,
            Blue, Blue2,
            Orange, Orange2
        }

        void UpdateFrets(Frets frets, bool top = true) {
            if (top) {
                UpdateFret(FretColor.Green, frets.Green);
                UpdateFret(FretColor.Red, frets.Red);
                UpdateFret(FretColor.Yellow, frets.Yellow);
                UpdateFret(FretColor.Blue, frets.Blue);
                UpdateFret(FretColor.Orange, frets.Orange);
            } else {
                UpdateFret(FretColor.Green2, frets.Green);
                UpdateFret(FretColor.Red2, frets.Red);
                UpdateFret(FretColor.Yellow2, frets.Yellow);
                UpdateFret(FretColor.Blue2, frets.Blue);
                UpdateFret(FretColor.Orange2, frets.Orange);
            }
        }
        void UpdateFret(FretColor c, bool b) {
            try {
                if (b) {
                    GamePad.SetControl(GetControl(c));
                } else {
                    GamePad.ReleaseControl(GetControl(c));
                }
            } catch(InvalidOperationException ex) {
                Debug.WriteLine(ex.Message);
            }
        }

        GamePadControl GetControl(FretColor fc) {
            switch (fc) {
                case FretColor.Green:
                    return GamePadControl.A;
                case FretColor.Green2:
                    return GamePadControl.RightStickUp;
                case FretColor.Red:
                    return GamePadControl.B;
                case FretColor.Red2:
                    return GamePadControl.RightStickDown;
                case FretColor.Yellow:
                    return GamePadControl.Y;
                case FretColor.Yellow2:
                    return GamePadControl.RightStickLeft;
                case FretColor.Blue:
                    return GamePadControl.X;
                case FretColor.Blue2:
                    return GamePadControl.RightStickRight;
                case FretColor.Orange:
                    return GamePadControl.LeftStickClick;
                case FretColor.Orange2:
                    return GamePadControl.RightStickClick;
            }
            return GamePadControl.None;
        }

        // The constant UI updates freeze the UI thread and cause a knock-on effect slowing everything else down
        private void OnPacketDecoded(byte[] outgoingData, byte[] incomingData) {
            //var incomingDataString = string.Join("-", incomingData.Select(o => Convert.ToString(o, 16)));
            //var time = DateTime.Now.ToLongTimeString();
            //var listItem = new LItem(time, BitConverter.ToString(outgoingData), incomingDataString);
            //try {
            //    ListViewPackets.Dispatcher.BeginInvoke((Action)(() => ListViewPackets.Items.Add(listItem)));
            //} catch (Exception ex) {
            //    Debug.WriteLine(ex.Message);
            //}
        }

        private void ListViewPackets_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            if (CheckBoxScrollPackets.IsChecked.Value) {
                ListViewPackets.SelectedIndex = ListViewPackets.Items.Count - 1;
                ListViewPackets.ScrollIntoView(ListViewPackets.SelectedItem);
            }
        }

        // The constant UI updates freeze the UI thread and cause a knock-on effect slowing everything else down
        public void AddToTextboxCore(string s) {
            //TextBoxCore.Dispatcher.Invoke(() => {
            //    TextBoxCore.Text += ($"{s}{Environment.NewLine}");
            //});
            //CoreOutput += s + Environment.NewLine;
        }

        private void OnExit(object sender, ExitEventArgs e) {
            GamePad.Unplug();
            GamePad.ShutDown();
            Util.OnExit();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e) {
            var items = ListViewPackets.SelectedItems;
            var text = string.Join(Environment.NewLine, items.Cast<LItem>().Select(item => $"Timestamp: {item.Time}{Environment.NewLine}Sent Packet: {item.Packet}{Environment.NewLine}Received Packet: {item.Return}"));
            Clipboard.SetText(text);
        }

        private void TextBoxCore_TextChanged(object sender, TextChangedEventArgs e) {
            if (CheckBoxScrollCore.IsChecked.Value) {
                TextBoxCore.ScrollToEnd();
            }
        }
    }
}
