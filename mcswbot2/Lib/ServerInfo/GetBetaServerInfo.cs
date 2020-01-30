using mcswbot2.Lib.Event;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace mcswbot2.Lib.ServerInfo
{
    internal static class GetBetaServerInfo
    {
        /// <summary>
        ///     Gets the information from specified server
        ///     Upon failure, creates exception with descriptive information
        ///     and InnerException with details
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <returns>A <see cref="ServerInfoBase" /> instance with retrieved data</returns>
        public static ServerInfoBase Get(string ip, int port = 25565)
        {
            var now = DateTime.Now;
            var sw = new Stopwatch();
            sw.Start();
            var players = new List<PlayerPayLoad>();
            try
            {
                string[] packetData;
                using (var client = new TcpClient(ip, port))
                {
                    using (var ns = client.GetStream())
                    {
                        ns.Write(new byte[] { 0xFE }, 0, 1);
                        var buff = new byte[2048];
                        var br = ns.Read(buff, 0, buff.Length);
                        if (buff[0] != 0xFF)
                            return new ServerInfoBase(now, sw.ElapsedMilliseconds, new InvalidDataException("Received invalid packet"));
                        var packet = Encoding.BigEndianUnicode.GetString(buff, 3, br - 3);
                        if (!packet.StartsWith("§"))
                            return new ServerInfoBase(now, sw.ElapsedMilliseconds, new InvalidDataException("Received invalid data"));
                        packetData = packet.Split('\u0000');
                        ns.Close();
                    }

                    client.Close();
                }

                sw.Stop();
                return new ServerInfoBase(now, sw.ElapsedMilliseconds, packetData[3], int.Parse(packetData[5]),
                    int.Parse(packetData[4]), packetData[2], players);
            }
            catch (Exception ex)
            {
                return new ServerInfoBase(now, sw.ElapsedMilliseconds, ex);
            }
        }
    }
}