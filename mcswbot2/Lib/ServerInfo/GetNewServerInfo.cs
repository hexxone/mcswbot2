using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using mcswbot2.Lib.Payload;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

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
                    tcp.ReceiveBufferSize = 512 * 1024;
                    tcp.ReceiveTimeout = 5000;

                    var packetId = GetVarInt(0);
                    var protocolVersion = GetVarInt(Proto);
                    var serverAddressVal = Encoding.UTF8.GetBytes(ip);
                    var serverAddressLen = GetVarInt(serverAddressVal.Length);
                    var serverPort = BitConverter.GetBytes((ushort)port);
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
                    var buffer = new byte[tcp.ReceiveBufferSize];

                    using (var stream = tcp.GetStream())
                    {
                        stream.Read(buffer, 0, buffer.Length);

                        // first read the packet length and check it to be > 0
                        // then read the packet it and check it to be 0 == ping response
                        if (ReadVarInt(buffer, ref offset) <= 0 || ReadVarInt(buffer, ref offset) != 0x00)
                            throw new Exception("Server sent an invalid response.");

                        var jsonLength = ReadVarInt(buffer, ref offset);

                        var json = ReadString(buffer, ref offset, jsonLength);

                        //Console.WriteLine("\r\n\r\nJSON: " + json + "\r\n\r\n");

                        // get our dynamic object
                        dynamic pingDat = JsonConvert.DeserializeObject(json);
                        object pingData = pingDat;

                        sw.Stop();

                        bool HasValue(object basis, string field) => basis.GetType().GetProperties().Any(p => p.Name == field);
                        object GetValue(object basis, string field) => basis != null && HasValue(basis, field) ? basis.GetType().GetProperties().First(p => p.Name == field).GetValue(basis, null) : null;

                        // convert our given types
                        var pingDataPlayers = GetValue(pingData, "players");
                        var pingDataVersion = GetValue(pingData, "version");
                        var pingDataDesc = GetValue(pingData, "description");

                        if (pingDataPlayers == null || pingDataVersion == null || pingDataDesc == null)
                            throw new Exception("Could not parse important information.");

                        var pingDataSample = GetValue(pingDataPlayers, "sample");
                        var max = (int)GetValue(pingDataPlayers, "max");
                        var onl = (int)GetValue(pingDataPlayers, "online");
                        var ver = (string)GetValue(pingDataVersion, "name");

                        var desc = "";
                        var plrs = new List<PlayerPayLoad>();
                        // parse description object instead of string
                        if (pingDataDesc != null)
                        {
                            try
                            {
                                var txt = GetValue(pingDataDesc, "text");
                                var ext = GetValue(pingDataDesc, "extra");

                                if (txt != null) desc = (string)txt;
                                if (ext != null)
                                {
                                    foreach (object key in (Array)ext)
                                    {
                                        var txt2 = GetValue(key, "text");
                                        if (txt2 != null) desc += (string)txt2;
                                    }
                                }
                            }
                            catch (Exception jex)
                            {
                                Program.WriteLine("\r\n\addr: " + ip + ":" + port + "\r\nError: " + jex.ToString() + "\r\n\r\njson: " + json + "\r\n");
                                try
                                {
                                    desc = (string)pingDataDesc;
                                }
                                catch { }
                            }
                            finally
                            {
                                if (string.IsNullOrEmpty(desc)) throw new Exception("Description is null.");
                            }
                        }

                        // get players from dynamic object
                        if (pingDataSample != null)
                        {
                            try
                            {
                                foreach (object key in (Array)pingDataSample)
                                {
                                    var kid = GetValue(key, "id");
                                    var kna = GetValue(key, "name");
                                    if (kid != null && kna != null)
                                    {
                                        var plr = new PlayerPayLoad() { Id = (string)kid, Name = (string)kna };
                                        Console.WriteLine("Add Player: " + plr.Name);
                                        plrs.Add(plr);
                                    }
                                }
                            }
                            catch (Exception sex)
                            {
                                Console.WriteLine("Error iterating samples... " + sex.ToString());
                            }
                        }

                        // we done boi
                        return new ServerInfoBase(now, sw.Elapsed, desc, max, onl, ver, plrs);
                    }
                }
            }
            catch (Exception ex)
            {
                Program.WriteLine("\r\n\addr: " + ip + ":" + port + "\r\nanother Error: " + ex.ToString() + "\r\n");
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
                bytes.Add((byte)((paramInt & 127) | 128));
                paramInt = (int)((uint)paramInt >> 7);
            }
            bytes.Add((byte)paramInt);
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