using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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

        public MainWindow() {
            Application.Current.Exit += OnExit;
            var init = new Thread(() => Util.Init(this));
            InitializeComponent();
            ((INotifyCollectionChanged)ListViewPackets.Items).CollectionChanged += ListViewPackets_CollectionChanged;
            init.Start();
        }

        public void Instance_OnPacketDecoded(string outgoingData, string incomingData) {
            var time = DateTime.Now.ToLongTimeString();
            var listItem = new LItem(time, outgoingData, incomingData);
            Dispatcher.Invoke(() => ListViewPackets.Items.Add(listItem));
        }

        private void ListViewPackets_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            ListViewPackets.SelectedIndex = ListViewPackets.Items.Count - 1;
            ListViewPackets.ScrollIntoView(ListViewPackets.SelectedItem);
        }

        public void AddToTextboxCore(string s) {
            this.TextBoxCore.Text += $"{s}{Environment.NewLine}";
        }

        private void OnExit(object sender, ExitEventArgs e) {
            Util.OnExit();
        }
        
    }
}
