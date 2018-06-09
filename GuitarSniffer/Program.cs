using System;
using System.IO;
using System.Text;
using PcapDotNet.Core;

namespace GuitarSniffer {
    internal class Program {
        private PacketSniffer ps;

        public static void Main(string[] args) => new Program().Start();

        void Start() {
            ps = new PacketSniffer();
            ps.Start();
        }

        /// <summary>
        /// use to find the difference in hex codes
        /// </summary>
        /// <param name="s"></param>
        /// <param name="multi"></param>
        /// <returns></returns>
        string DiffInString(string s, string multi) {
            var fixedS = s;
            fixedS = fixedS.Replace("88-11-A0-00-62-45-B4-F0-85-2C-7E-ED-8F-FF-", "");

            using (StringReader reader = new StringReader(multi)) {
                string line;
                while ((line = reader.ReadLine()?.Replace("88-11-A0-00-62-45-B4-F0-85-2C-7E-ED-8F-FF-", "")) != null) {
                    var aStringBuilder = new StringBuilder(fixedS);
                    for (int i = 0; i < fixedS.Length; i++) {
                        if (fixedS[i] == line[i]) {
                            aStringBuilder.Remove(i, 1);
                            aStringBuilder.Insert(i, " ");
                        }
                    }

                    fixedS = aStringBuilder.ToString();
                }
            }

            return fixedS;
        }
    }
}