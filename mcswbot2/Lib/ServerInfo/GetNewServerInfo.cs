using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq.Expressions;
using System.Net.Sockets;
using System.Text;
using mcswbot2.Lib.Payload;
using Newtonsoft.Json;

namespace mcswbot2.Lib.ServerInfo
{
    internal static class GetNewServerInfo
    {
        // your "client" protocol version to tell the server 
        // doesn't really matter, server will return its own version independently
        private const int Proto = 51;

        /// <summary>
        ///     Connect to the Server and print information, then return Protocol version
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <returns><see cref="ServerInfoBase" />result</returns>
        public static ServerInfoBase Get(string ip, int port = 25565)
        {
            var now = DateTime.Now;
            var sw = new Stopwatch();
            sw.Start();

            try
            {
                using (var tcp = new TcpClient(ip, port))
                {
                    tcp.ReceiveBufferSize = 2048 * 2048;

                    var packetId = GetVarInt(0);
                    var protocolVersion = GetVarInt(Proto);
                    var serverAddressVal = Encoding.UTF8.GetBytes(ip);
                    var serverAddressLen = GetVarInt(serverAddressVal.Length);
                    var serverPort = BitConverter.GetBytes((ushort) port);
                    Array.Reverse(serverPort);
                    var nextState = GetVarInt(1);
                    var packet = ConcatBytes(packetId, protocolVersion, serverAddressLen, serverAddressVal, serverPort,
                        nextState);
                    var toSend = ConcatBytes(GetVarInt(packet.Length), packet);
                    tcp.Client.Send(toSend, SocketFlags.None);

                    var statusRequest = GetVarInt(0);
                    var requestPacket = ConcatBytes(GetVarInt(statusRequest.Length), statusRequest);
                    tcp.Client.Send(requestPacket, SocketFlags.None);

                    var offset = 0;
                    var buffer = new byte[32769];

                    using (var stream = tcp.GetStream())
                    {
                        stream.Read(buffer, 0, buffer.Length);

                        // first read the packet length and check it to be > 0
                        // then read the packet it and check it to be 0 == ping response
                        if (ReadVarInt(buffer, ref offset) <= 0 || ReadVarInt(buffer, ref offset) != 0x00)
                            throw new Exception("Server sent an invalid response.");

                        var jsonLength = ReadVarInt(buffer, ref offset);

                        //Console.WriteLine("Json length: " + jsonLength);

                        var json = ReadString(buffer, ref offset, jsonLength);

                        // description is an object? use alternate payload
                        // @TODO find better approach than this

                        //Console.WriteLine("\r\n\r\n" + json + "\r\n\r\n");

                        if (json.Contains("\"description\":{\""))
                        {
                            try
                            {
                                var ping = JsonConvert.DeserializeObject<PingPayLoad2>(json);

                                sw.Stop();
                                return new ServerInfoBase(now, sw.Elapsed, ping.Description.ToSimpleString(),
                                    ping.Players.Max, ping.Players.Online, ping.Version.Name, ping.Players.Sample);
                            }
                            catch (JsonException jex)
                            {
                                Console.WriteLine("Error json parsing: " + jex.ToString());
                            }
                        }

                        var ping2 = JsonConvert.DeserializeObject<PingPayLoad>(json);
                        sw.Stop();
                        return new ServerInfoBase(now, sw.Elapsed, ping2.Motd, ping2.Players.Max,
                            ping2.Players.Online, ping2.Version.Name, ping2.Players.Sample);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("another Error: " + ex.ToString());
                return new ServerInfoBase(ex);
            }
        }

        #region request building methods

        private static byte[] ConcatBytes(params byte[][] bytes)
        {
            var result = new List<byte>();
            foreach (var array in bytes)
                result.AddRange(array);
            return result.ToArray();
        }

        private static byte[] GetVarInt(int paramInt)
        {
            var bytes = new List<byte>();
            while ((paramInt & -128) != 0)
            {
                bytes.Add((byte) ((paramInt & 127) | 128));
                paramInt = (int) ((uint) paramInt >> 7);
            }
            bytes.Add((byte) paramInt);
            return bytes.ToArray();
        }

        #endregion


        #region buffer read methods

        private static byte ReadByte(byte[] buffer, ref int off)
        {
            var b = buffer[off];
            off += 1;
            return b;
        }

        private static byte[] Read(byte[] buffer, ref int off, int length)
        {
            var data = new byte[length];
            Array.Copy(buffer, off, data, 0, length);
            off += length;
            return data;
        }

        private static int ReadVarInt(byte[] buffer, ref int off)
        {
            var value = 0;
            var size = 0;
            int b;
            while (((b = ReadByte(buffer, ref off)) & 0x80) == 0x80)
            {
                value |= (b & 0x7F) << (size++ * 7);
                if (size > 5) throw new IOException("This VarInt is an imposter!");
            }

            return value | ((b & 0x7F) << (size * 7));
        }

        private static string ReadString(byte[] buffer, ref int off, int length)
        {
            var data = Read(buffer, ref off, length);
            return Encoding.UTF8.GetString(data);
        }

        #endregion
    }
}