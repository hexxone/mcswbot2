using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using mcswbot2.Lib.Event;
using Newtonsoft.Json;

namespace mcswbot2.Lib.ServerInfo
{
    internal static class GetNewServerInfo
    {
        // your "client" protocol version to tell the server 
        // doesn't really matter, server will return its own version independently
        private const int Proto = 51;
        private const int BufferSize = Int16.MaxValue;
        private const int TimeoutAfter = 500;

        /// <summary>
        ///     Connect to the Server and print information, then return Protocol version
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <returns><see cref="ServerInfoBase" />result</returns>
        public static ServerInfoBase Get(string ip, int port = 25565)
        {
            var sw = new Stopwatch();
            sw.Start();
            var now = DateTime.Now;
            try
            {
                var json = "";

                using (var client = new TcpClient())
                {
                    var task = client.ConnectAsync(ip, port);
                    while (!task.IsCompleted)
                    {
#if DEBUG
                        Debug.WriteLine("Connecting..");
#endif
                        Thread.Sleep(100);
                    }

                    if (!client.Connected)
                        return new ServerInfoBase(new Exception("Unable to connect..."));

                    var _offset = 0;
                    using (var stream = client.GetStream())
                    {
                        var writeBuffer = new List<byte>();

                        WriteVarInt(writeBuffer, 47);
                        WriteString(writeBuffer, ip);
                        WriteShort(writeBuffer, Convert.ToInt16(port));
                        WriteVarInt(writeBuffer, 1);
                        Flush(writeBuffer, stream, 0);

                        Flush(writeBuffer, stream, 0);

                        /*
                        byte[] readBuffer;
                        byte[] data = new byte[1024];
                        using (MemoryStream ms = new MemoryStream())
                        {
                            int numBytesRead;
                            while ((numBytesRead = stream.Read(data, 0, data.Length)) > 0)
                            {
                                ms.Write(data, 0, numBytesRead);
                            }
                            readBuffer = ms.ToArray();
                            ms.Close();
                        }
                        stream.Close();
                        client.Close();
                        */

                        var readBuffer = new byte[Int16.MaxValue];
                        stream.Read(readBuffer, 0, readBuffer.Length);

                        try
                        {
                            var length = ReadVarInt(ref _offset, readBuffer);
                            var packet = ReadVarInt(ref _offset, readBuffer);
                            var jsonLength = ReadVarInt(ref _offset, readBuffer);

                            json = ReadString(ref _offset, readBuffer, jsonLength);
                        }
                        catch (IOException ex)
                        {
                            Console.WriteLine("Unable to read packet length from server,");
                            Console.WriteLine("are you sure it's a Minecraft server?");
#if DEBUG
                            Console.WriteLine("Here are the details:");
                            Console.WriteLine(ex.ToString());
#endif
                            return new ServerInfoBase(ex);
                        }
                    }

                }

                sw.Stop();

                dynamic ping = JsonConvert.DeserializeObject(json.Replace("\n",""));

                var sample = new List<PlayerPayLoad>();
                if (json.Contains("\"sample\":["))
                {
                    try
                    {
                        foreach (dynamic key in ping.players.sample)
                        {
                            if (key.id != null && key.name != null)
                            {
                                var plr = new PlayerPayLoad() { Id = key.id, Name = key.name };
                                Console.WriteLine("Add Player: " + plr.Name);
                                sample.Add(plr);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error when sample processing " + e.ToString());
                    }

                }

                var desc = "";
                if (json.Contains("\"description\":{\""))
                {
                    try
                    {
                        desc = (string)ping.description.text;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error description: " + e.ToString());
                    }
                }

                if (string.IsNullOrEmpty(desc))
                    desc = (string)ping.description;

                return new ServerInfoBase(now, sw.ElapsedMilliseconds, desc, (int)ping.players.max, (int)ping.players.online, (string)ping.version.name, sample);

            }
            catch (Exception e)
            {
                Console.WriteLine("another Error: " + e.ToString());
                return new ServerInfoBase(e);
            }
        }


        #region request building methods

        internal static byte ReadByte(ref int _offset, byte[] buffer)
        {
            var b = buffer[_offset];
            _offset += 1;
            return b;
        }

        internal static byte[] Read(ref int _offset, byte[] buffer, int length)
        {
            var data = new byte[length];
            Array.Copy(buffer, _offset, data, 0, length);
            _offset += length;
            return data;
        }

        internal static int ReadVarInt(ref int _offset, byte[] buffer)
        {
            var value = 0;
            var size = 0;
            int b;
            while (((b = ReadByte(ref _offset, buffer)) & 0x80) == 0x80)
            {
                value |= (b & 0x7F) << (size++ * 7);
                if (size > 5)
                {
                    throw new IOException("This VarInt is an imposter!");
                }
            }
            return value | ((b & 0x7F) << (size * 7));
        }

        internal static string ReadString(ref int _offset, byte[] buffer, int length)
        {
            var data = Read(ref _offset, buffer, length);
            return Encoding.UTF8.GetString(data);
        }

        internal static void WriteVarInt(List<byte> buffer, int value)
        {
            while ((value & 128) != 0)
            {
                buffer.Add((byte)(value & 127 | 128));
                value = (int)((uint)value) >> 7;
            }
            buffer.Add((byte)value);
        }

        internal static void WriteShort(List<byte> buffer, short value)
        {
            buffer.AddRange(BitConverter.GetBytes(value));
        }

        internal static void WriteString(List<byte> buffer, string data)
        {
            var buff = Encoding.UTF8.GetBytes(data);
            WriteVarInt(buffer, buff.Length);
            buffer.AddRange(buff);
        }

        internal static void Write(NetworkStream stream, byte b)
        {
            stream.WriteByte(b);
        }

        internal static void Flush(List<byte> buffer, NetworkStream stream, int id = -1)
        {
            var buff = buffer.ToArray();
            buffer.Clear();

            var add = 0;
            var packetData = new[] { (byte)0x00 };
            if (id >= 0)
            {
                WriteVarInt(buffer, id);
                packetData = buffer.ToArray();
                add = packetData.Length;
                buffer.Clear();
            }

            WriteVarInt(buffer, buff.Length + add);
            var bufferLength = buffer.ToArray();
            buffer.Clear();

            stream.Write(bufferLength, 0, bufferLength.Length);
            stream.Write(packetData, 0, packetData.Length);
            stream.Write(buff, 0, buff.Length);
        }

        #endregion
    }
}