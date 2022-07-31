using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace McswBot2.Static
{
    internal static class Utils
    {
        /// <summary>
        ///     Finds all classes in RT Assemnly which inherit from Base Type 'bt'
        /// </summary>
        /// <param name="bt">Base Type</param>
        /// <param name="excludeGenerics">whether to exclude Classes from result that take any Generic-Arguments</param>
        /// <param name="excludeAbstract">whether to exclude abstract Classes from result</param>
        /// <returns></returns>
        public static IEnumerable<Type> FindDerivedAssemblyTypes(
            this Type bt,
            bool excludeGenerics = true,
            bool excludeAbstract = true)
        {
            return bt.Assembly.GetTypes()
                .Where(t =>
                    t != bt &&
                    bt.IsAssignableFrom(t) &&
                    (!t.ContainsGenericParameters || !excludeGenerics) &&
                    (!t.IsAbstract || !excludeAbstract));
        }


        /// <summary>
        ///     Removes previously applied Telegram Html style tags
        /// </summary>
        /// <returns></returns>
        internal static string NoHtml(string input)
        {
            return input.Replace("<code>", "").Replace("</code>", "");
        }

        /// <summary>
        ///     Will verify a given server label string
        /// </summary>
        /// <param name="txt"></param>
        internal static void VerifyLabel(string txt)
        {
            if (txt.Contains('.'))
            {
                throw new Exception("Label should not contain Dots! ('.')");
            }

            if (txt.Length > 12)
            {
                throw new Exception("Label should be 12 characters at max!");
            }
        }

        /// <summary>
        ///     Will verify a given server address and port
        ///     by basic port checking, Uri-checking,
        ///     name resolving and regex-checking for private ip ranges.
        /// </summary>
        /// <param name="addr">server address ip or domain</param>
        /// <param name="port">mc server port</param>
        /// <returns></returns>
        internal static void VerifyAddress(string addr, int port)
        {
            // length check
            if (addr.Length > 256)
            {
                throw new Exception("The address length should not exceed 256 characters!");
            }

            // port check
            if (port < 80 || port > 65534)
            {
                throw new Exception("Invalid Port! Choose one above 79 and below 65535.");
            }

            // check if ip address was entered
            var ipRegex = @"(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)(\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)){3}";
            var resolved = "";
            if (Regex.IsMatch(addr, ipRegex))
            {
                resolved = addr;
            }
            else
            {
                // some hostname checks
                if (string.Equals(Dns.GetHostName(), addr, StringComparison.CurrentCultureIgnoreCase) ||
                    Uri.CheckHostName(addr) == UriHostNameType.Unknown ||
                    !addr.Contains('.'))
                {
                    throw new Exception("Invalid hostname!");
                }

                // resolve
                var host = Dns.GetHostEntry(addr);
                if (host == null || host.AddressList == null || host.AddressList.Length == 0)
                {
                    throw new Exception("No hostname address entries!");
                }

                // try to get ipv4 entry
                try
                {
                    resolved = host.AddressList.First(h => h.AddressFamily == AddressFamily.InterNetwork).ToString();
                }
                catch
                {
                    try
                    {
                        resolved = host.AddressList.First(h => h.AddressFamily == AddressFamily.InterNetworkV6)
                            .ToString();
                    }
                    catch
                    {
                    }
                }

                if (string.IsNullOrEmpty(resolved))
                {
                    throw new Exception("No valid hostname resolved.");
                }
            }

            /* Block following ip-ranges
                127. 0.0.0 – 127.255.255.255     127.0.0.0 /8
                10.  0.0.0 –  10.255.255.255      10.0.0.0 /8
                172. 16.0.0 – 172. 31.255.255    172.16.0.0 /12
                192.168.0.0 – 192.168.255.255   192.168.0.0 /16
            */
            // assumes that ipv4 format sanity checking has already been done 
            var blockStr =
                @"(192\.168(\.[0-9]{1,3}){2})|(172\.(1[6-9]|2[0-9]|3[0-1])(\.[0-9]{1,3}){2})|([10|27]+(\.[0-9]{1,3}){3})";
            // private check
            if (Regex.IsMatch(resolved, blockStr))
            {
                throw new Exception("Invalid IP-Address Range!");
            }
            // all ok
        }

        /// <summary>
        ///     Wrap Link Html Tag
        /// </summary>
        /// <param name="l"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        internal static string WrapLink(string l, string t)
        {
            return $"<a href='{l}'>{t}</a>";
        }
    }
}