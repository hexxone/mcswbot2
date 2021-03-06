﻿using System;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using static mcswbot2.Lib.Types;

namespace mcswbot2.Bot
{
    internal class Utils
    {
        /// <summary>
        ///     Will Plot and save Data to a file
        /// </summary>
        /// <param name="dat"></param>
        public static Bitmap PlotData(PlottableData[] dat, string xLab, string yLab)
        {
            var plt = new ScottPlot.Plot(355, 200);
            plt.XLabel(xLab);
            plt.YLabel(yLab);
            plt.Legend(true);
            foreach (var da in dat)
                if(da.DataX.Length > 0)
                    plt.PlotScatter(da.DataX, da.DataY, null, 1D, 5D, da.Label);
            return plt.GetBitmap();
        }

        /// <summary>
        ///     Will verify a given server label string
        /// </summary>
        /// <param name="txt"></param>
        public static void VerifyLabel(string txt)
        {
            if (txt.Contains('.')) throw new Exception("Label should not contain Dots! ('.')");
            if (txt.Length > 12) throw new Exception("Label should be 12 characters at max!");
        }

        /// <summary>
        ///     Will verify a given server address and port
        ///     by basic port checking, Uri-checking,
        ///     name resolving and regex-checking for private ip ranges.
        /// </summary>
        /// <param name="addr">server address ip or domain</param>
        /// <param name="port">mc server port</param>
        /// <returns></returns>
        public static void VerifyAddress(string addr, int port)
        {
            // dont 
            if (addr.Length > 256) throw new Exception("The address length should not exceed 256 characters!");
            // port check
            if (port < 80 || port > 65534) throw new Exception("Invalid Port! Choose one above 79 and below 65535.");
            // check if ip address was entered
            var ipRegex = @"(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)(\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)){3}";
            var resolved = "";
            if (Regex.IsMatch(addr, ipRegex)) resolved = addr;
            else
            {
                // some hostname checks
                if (Dns.GetHostName().ToLower() == addr.ToLower() ||
                    Uri.CheckHostName(addr) == UriHostNameType.Unknown ||
                    !addr.Contains('.'))
                        throw new Exception("Invalid hostname!");
                // resolve
                var host = Dns.GetHostEntry(addr);
                if (host == null || host.AddressList == null || host.AddressList.Length == 0) throw new Exception("No hostname address entries!");
                // try toget ipv4 entry
                try
                {
                    resolved = host.AddressList.First(h => h.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).ToString();
                }
                catch {
                    try
                    {
                        resolved = host.AddressList.First(h => h.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6).ToString();
                    }
                    catch { }
                }
                if (string.IsNullOrEmpty(resolved)) throw new Exception("No valid hostname resolved.");
            }

            /* Block following ip-ranges
                127. 0.0.0 – 127.255.255.255     127.0.0.0 /8
                10.  0.0.0 –  10.255.255.255      10.0.0.0 /8
                172. 16.0.0 – 172. 31.255.255    172.16.0.0 /12
                192.168.0.0 – 192.168.255.255   192.168.0.0 /16
            */
            // assumes that ipv4 format sanity checking has already been done 
            var blockStr = @"(192\.168(\.[0-9]{1,3}){2})|(172\.(1[6-9]|2[0-9]|3[0-1])(\.[0-9]{1,3}){2})|([10|27]+(\.[0-9]{1,3}){3})";
            // private check
            if (Regex.IsMatch(resolved, blockStr)) throw new Exception("Invalid IP-Address Range!");
            // all ok
        }

    }
}