﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json.Linq;

namespace GuitarSnifferWrapper {
    public class Util {

        public static Thread CoreWatcher { get; set; }
        public static Thread PacketServerThread { get; set; }
        public static Thread GuitarListenerThread { get; set; }
        static Process CoreProcess { get; set; } = new Process();

        static DirectoryInfo SnifferLocation { get; set; } = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)).CreateSubdirectory("GuitarSniffer");

        static DirectoryInfo CoreLocation { get; set; } = SnifferLocation.CreateSubdirectory("Core");

        const string githubLatest = "https://api.github.com/repos/artman41/GuitarSnifferCore/releases/latest";

        public static MainWindow MainWindow { get; set; }

        public static void Init(MainWindow mw) {
            MainWindow = mw;
            DownloadRelease();
            StartThreads();
        }

        static void StartThreads() {
            CoreWatcher = GetCoreWatcherThread();
            CoreWatcher.IsBackground = true;
            CoreWatcher.Start();
            PacketServerThread = new Thread(() => PacketServer.Instance.Start()) {
                IsBackground = true
            };
            PacketServerThread.Start();
            GuitarListenerThread = new Thread(() => GuitarListener.Instance.Start()) {
                IsBackground = true
            };
            GuitarListenerThread.Start();
        }

        static void DownloadRelease() {
            if(CoreLocation.GetDirectories().Length <= 0) {
                MainWindow.Dispatcher.Invoke(() => {
                    MainWindow.GridDownload.Visibility = Visibility.Visible;
                });
                using (var client = new WebClient()) {
                    client.Headers.Add("user-agent", "GuitarSnifferWrapper");
                    var jsonS = client.DownloadString(githubLatest);
                    var jObj = JObject.Parse(jsonS);
                    client.DownloadFile(jObj.GetValue("assets")[0].Value<string>("browser_download_url"), "GuitarSnifferCore.zip");
                }
                CoreLocation.Delete(true);
                CoreLocation.Create();
                ZipFile.ExtractToDirectory("GuitarSnifferCore.zip", CoreLocation.FullName);
                File.Delete("GuitarSnifferCore.zip");
                MainWindow.Dispatcher.Invoke(() => {
                    MainWindow.GridDownload.Visibility = Visibility.Hidden;
                });
            }
        }

        public static void OnExit() {
            try {
                Process.GetProcessesByName("erl").ToList().ForEach(o => o.Kill());
                Process.GetProcessesByName("epmd").ToList().ForEach(o => o.Kill());
            } catch(Exception ex) {
                Debug.WriteLine(ex.Message);
            }
            CoreWatcher.Abort();
        }

        static Thread GetCoreWatcherThread() {
            return new Thread(() => {
                var StartInfo = new ProcessStartInfo {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    FileName = $"{CoreLocation.FullName}/bin/guitar_sniffer_core.bat",
                    Arguments = "foreground",

                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                CoreProcess = Process.Start(StartInfo);
                CoreProcess.OutputDataReceived += (object sender, DataReceivedEventArgs e) => {
                    // Trace.WriteLine(e.Data);
                    try {
                        MainWindow.Dispatcher.Invoke(() => MainWindow.AddToTextboxCore($"[stdout] {e.Data ?? string.Empty}"));
                    } catch(Exception ex) {
                        Debug.WriteLine(ex);
                    }
                };

                CoreProcess.ErrorDataReceived += (object sender, DataReceivedEventArgs e) => {
                    // Trace.WriteLine(e.Data);
                    try {
                        MainWindow.Dispatcher.Invoke(() => MainWindow.AddToTextboxCore($"[stderr] {e.Data ?? string.Empty}"));
                    } catch (Exception ex) {
                        Debug.WriteLine(ex);
                    }
            };
                CoreProcess.BeginOutputReadLine();
                CoreProcess.BeginErrorReadLine();
            });
        }
    }
}
