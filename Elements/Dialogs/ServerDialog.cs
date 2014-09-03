using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Runtime.Serialization.Json;
using Omniaudio.Core;
using Omniaudio.Networking;
using System.IO;
using System.Net.NetworkInformation;
using Omniaudio.Helpers;
using System.Diagnostics;
using Lidgren.Network;
using Omniaudio.Properties;
using System.Threading;
using Omniaudio.Managers;
using Omniaudio.Pages;
using System.Threading.Tasks;

namespace Omniaudio.Elements.Dialogs
{ 
    delegate void ServerDialogConnectionException();
    class ServerDialog : Dialog
    {
        public event ServerDialogConnectionException connectionException;
        // stores local and servers on OSDL
        private List<Server> serverDirectory;
        private List<Server> hiddenServers;
       
        private int activeServers;
        private int entryX, entryY;
        private int pickIndex = 0;
        private int scrollX, scrollY = 0;

        public ServerDialog(int xPos, int yPos, int width, int height, ref CHAR_INFO[,] rBuffer, bool isStatic): base(xPos, yPos, width, height, ref rBuffer,isStatic)
        {
            this._x = xPos;
            this._y = yPos;
            this._w = width;
            this._h = height;
            this.drawBuffer = rBuffer;
            this._isStatic = isStatic;
            activeServers = 0;
            pickIndex = 0;
            entryX = _x;
            entryY = _y + 2; // hardcoded padding of 2. Because powers of 2 are usually nice
            selectorX = _x;
            selectorY = _y + 2;
        }

        public void Init()
        {
            serverDirectory = new List<Server>();
            try
            {
                string jsonData;
                //request server data
                WebRequest wr = WebRequest.Create("http://localhost:1812/home/GetServers");
                wr.Timeout = 3000; // 3sec timeout
                wr.Method = "GET";
                wr.ContentType = "application/json; charset=utf-8";
                HttpWebResponse wo = (HttpWebResponse)wr.GetResponse();

                using (var reader = new StreamReader(wo.GetResponseStream()))
                {
                    jsonData = reader.ReadToEnd();
                    DataContractJsonSerializer sr = new DataContractJsonSerializer(typeof(Server[]));
                    var msnew = new MemoryStream(Encoding.UTF8.GetBytes(jsonData));
                    Server[] server = (Server[])sr.ReadObject(msnew);
                    serverDirectory = server.ToList();
                    reader.DiscardBufferedData();
                    reader.Dispose();
                    msnew.Dispose();
                }

                wo.Close();
                activeServers = serverDirectory.Count;
                hiddenServers = new List<Server>();
            }
            catch
            { 
                // do something here
            }

            if (serverDirectory != null)
            {           
                Parallel.ForEach(serverDirectory.Cast<Server>(),
                currentElement =>
                {
                    currentElement.Ping = (int)PingTimeAverage(currentElement.InternetProtocol,10);
                });         
            }
        }


        public sealed override void Update()
        {
            base.Update();
        }



        public sealed override void Draw()
        {
            ConsoleHelper.DrawRectangle(_x, _y, _w, _h, ref drawBuffer,  0x0001);
            ConsoleHelper.DrawRectangle(selectorX, selectorY, _w, 1, ref drawBuffer);


            //Draw Field Headers
            /*Title*/
            ConsoleHelper.WriteLineInBuffer(new COORD((short)_x, (short)_y), "Title", ref drawBuffer, 0x0A | 0x0010);
            /*Desc.*/
            ConsoleHelper.WriteLineInBuffer(new COORD((short)(_x + 28), (short)_y), "Desc.", ref drawBuffer, 0x0A | 0x0010);
            /*Internet Protocol*/
            ConsoleHelper.WriteLineInBuffer(new COORD((short)(_x + 50), (short)_y), "IP", ref drawBuffer, 0x0A | 0x0010);
            /*Ping*/
            ConsoleHelper.WriteLineInBuffer(new COORD((short)((_x + _w) - 4), (short)(_y)), "Ping", ref drawBuffer, 0x0A | 0x0010);

            int origX, origY;
            origX = entryX;
            origY = entryY;

            if (serverDirectory != null)
            {
                // SEE CHAR_INFO MSDN documentation for more information
                // red and green make yellow. 
                foreach (Server server in serverDirectory)
                {
                    ConsoleHelper.WriteLineInBuffer(new COORD((short)(entryX), (short)entryY), server.Title, ref drawBuffer, 0x0008 | 0x0010 | 0x004 | 0x002 | 0x0008 | 0x0002);
                    ConsoleHelper.WriteLineInBuffer(new COORD((short)(entryX + 28), (short)entryY), server.Description, ref drawBuffer, 0x0008 | 0x0010 | 0x004 | 0x002 | 0x0008 | 0x0002);
                    ConsoleHelper.WriteLineInBuffer(new COORD((short)(entryX + 50), (short)entryY), server.InternetProtocol, ref drawBuffer, 0x0008 | 0x0010 | 0x004 | 0x002 | 0x0008 | 0x0002);
                    ConsoleHelper.WriteLineInBuffer(new COORD((short)(entryX + 117), (short)entryY), server.Ping.ToString(), ref drawBuffer, 0x0008 | 0x0010 | 0x004 | 0x002 | 0x0008 | 0x0002);
                    entryY += 2;
                }
            }

            entryX = origX;
            entryY = origY;

            /*Interaction*/

            if (selectorY < (_y + (serverDirectory.Count * 2)))
            {
                if (Global.cki.Key == ConsoleKey.S)
                {
                    selectorY += 2;
                    pickIndex += 1;
                    Global.cki = new ConsoleKeyInfo();
                    Debug.Write(pickIndex);
                }
            }

            if (selectorY > _y + 2)
            {
                if (Global.cki.Key == ConsoleKey.W)
                {
                    selectorY -= 2;
                    pickIndex -= 1;
                    Global.cki = new ConsoleKeyInfo();
                    Debug.Write(pickIndex);
                }
            }

            if (serverDirectory.Count > 0)
            {
                if (Global.cki.Key == ConsoleKey.Enter)
                {
                    //Debug.Write(serverDirectory[pickIndex].Title);
                    try
                    {
                        NetPeerConfiguration config = new NetPeerConfiguration("Omniaudio"); // nice app id
                        NetClient client = new NetClient(config);
                        client.Start();
                        NetOutgoingMessage hail = client.CreateMessage();
                        hail.Write(Settings.Default["username"].ToString());
                        client.Connect("localhost", serverDirectory[pickIndex].Port, hail);
                        NetIncomingMessage im = null;
                        client.MessageReceivedEvent.WaitOne(2000); // 2 sec timeout
                        im = client.ReadMessage();
                        if (im != null)
                        {
                            switch (im.MessageType)
                            {
                                case NetIncomingMessageType.StatusChanged:

                                    switch ((NetConnectionStatus)im.ReadByte())
                                    {
                                        case NetConnectionStatus.Connected:
                                            Logger.Instance.Log("log", "established a valid connection to server");
                                            Logger.Instance.Flush();
                                            PageManager.Instance.Pop();
                                            PageManager.Instance.AddPage(new Client(client));
                                            break;
                                        case NetConnectionStatus.None:
                                            Logger.Instance.Log("log", " unknown exception occured" + im.ReadString());
                                            Logger.Instance.Flush();
                                            throw new Exception();
                                            

                                        default:
                                            throw new Exception();
                                    }
                                    break;
                            }

                        }
                        else
                        {
                            if (connectionException != null)
                            {
                                connectionException();
                            }
                        }
                    }
                    catch
                    {
                        if (connectionException != null)
                        {
                            connectionException();
                        }
                    }



                    Global.cki = new ConsoleKeyInfo();
                }

            }
            
        }

        private double PingTimeAverage(string host, int echoNum)
        {
            long totalTime = 0;
            int timeout = 100;
            Ping pingSender = new Ping();

            for (int i = 0; i < echoNum; i++)
            {
                PingReply reply = pingSender.Send(host,timeout);
                if (reply.Status == IPStatus.Success)
                {
                    totalTime += reply.RoundtripTime;
                }
            }
            return totalTime / echoNum;
        }

   



    }
}
