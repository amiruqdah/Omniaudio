using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Omniaudio.Networking;
using Lidgren.Network;
using Omniaudio.Helpers;
using System.Threading;
using System.Diagnostics;
using Omniaudio.Elements.Dialogs;
using System.Text.RegularExpressions;
using Omniaudio.Elements;
using Omniaudio.Properties;
using System.Net;
using Omniaudio.Managers;
using System.IO;
using NAudio;

namespace Omniaudio.Pages
{
    /*NOTES
     * 
     * Authorative Server Setup, Think: SERVER before CLIENT
     * 
     *
     * look into network seurity 
     * **/
    class Session : IPage
    {
        private Server serverInformation;
        private NetServer server;
        private NetIncomingMessage msg;
        private Thread reciever;
        private bool sessionEnded;
        private int y = Console.BufferHeight - 18;
        private int fieldTab = 0;
        private NotifyDialog nd;

        private COORD dwBufferSize = new COORD((short)Console.BufferWidth, (short)Console.BufferHeight);
        private COORD dwBufferCoord;
        private SMALL_RECT rcRegion = new SMALL_RECT(0, 0, (short)(Console.BufferWidth), (short)(Console.BufferHeight));
        private IntPtr oHandle = ConsoleHelper.GetStdOut();
        private CHAR_INFO[,] sBuffer;
        private CHAR_INFO[,] rBuffer;

        private ClientDialog cDialog;
        private ChatDialog chat;
        private PlaylistDialog pDialog;
        private List<string> log = new List<string>();
        private List<InputField> inputFields = new List<InputField>();

        //Threads
        private Thread dishMusic; // pretty self explanatory

        public Session(Server server)
        {
            this.serverInformation = server;
          
            this.rBuffer = new CHAR_INFO[Console.BufferHeight, Console.BufferWidth];
            sBuffer = new CHAR_INFO[Console.BufferHeight, Console.BufferWidth];
            ConsoleHelper.ClearBuffer(ref rBuffer); // clearing to be safe
            ConsoleHelper.ClearBuffer(ref sBuffer); // clearing to be safe
            dwBufferCoord.x = 0;
            dwBufferCoord.y = 0;
            sessionEnded = false;
        }
        // Init routine for sesion serverr
        public void Init()
        {
            
            log.Add("Initalizing Server...");
            Logger.Instance.Log("log", "Initalizing " + this.ToString());
            Logger.Instance.Flush();
            Logger.Instance.Log("server", "Initalizing Server Config");
            NetPeerConfiguration config = new NetPeerConfiguration("Omniaudio");
            config.SetMessageTypeEnabled(NetIncomingMessageType.ConnectionApproval, true);
            config.Port = serverInformation.Port;
            config.EnableUPnP = true;
            config.SendBufferSize = 30000; // so c00l
            config.MaximumConnections = serverInformation.MaxListeners;
            config.UseMessageRecycling = true;
           
            try
            {
                server = new NetServer(config);
                
                server.Start();
                
                //server.UPnP.ForwardPort(config.Port, "Attempting to forward port");

                reciever = new Thread(RecieveMessages);
                reciever.IsBackground = true;
                reciever.Start();
                
            }
            catch (Exception e)
            {
                throw e; // gotta create the thread or else things are gonna get hairy
            }


            // push data to log
            Logger.Instance.Log("server", "Created reciever thread for incoming messages");   
            Logger.Instance.Log("server", "Created " + serverInformation.Title + "on " + serverInformation.InternetProtocol + ":" + serverInformation.Port);
            Logger.Instance.Log("server", "waiting for client connections...");
            Logger.Instance.Flush();

            // initalize a few page elements
            chat = new ChatDialog((Console.BufferWidth - 140) / 2, 1, 130, 10, ref rBuffer);
            chat.sendMsgGlobal += SendChatMessage_ToClients;
            cDialog = new ClientDialog(Console.BufferWidth - 20, 1, 20, Console.BufferHeight - 1, ref rBuffer);
            pDialog = new PlaylistDialog(0, 13, 50, 25, ref rBuffer, false);
            pDialog.isSelected = false;
            pDialog.onSelect = PlaylistDialog_OnEnter;
            inputFields.Add(new InputField(0, Console.BufferHeight - 2, 49, 2, ref rBuffer, 25, true, "", HandleLogEvent));
            inputFields[0].isSelected = true;
            inputFields[0].ClearOnEnter = true;
            inputFields.Add(new InputField((Console.BufferWidth - 140) / 2, 11, 129, 1, ref rBuffer, 125, true, "enter text here", HandleChatEvent));
            inputFields[1].ClearOnEnter = true;
            log.Add("waiting for incoming connections...");
            cDialog.Admin = Settings.Default["username"].ToString();
            
        }
        public void Update()
        {
            ConsoleHelper.WriteConsoleOutput(oHandle, sBuffer, dwBufferSize, dwBufferCoord, ref rcRegion);
            ConsoleHelper.CopyBuffer(ref rBuffer, ref sBuffer);

            ConsoleHelper.WriteLineInBuffer(new COORD(0, 5), "Listeners: " + server.ConnectionsCount, ref rBuffer, 0x0C);


            chat.Update();
            chat.Draw();

            cDialog.Update();
            cDialog.Draw();

            
            pDialog.Update();
            pDialog.Draw();

            if (nd != null)
            {
                nd.Draw();
                nd.Update();
            }

            #region Input
            if (pDialog.isSelected == false)
            {
                if (Global.cki.Key == ConsoleKey.Tab)
                {
                    if (fieldTab + 1 < inputFields.Count)
                    {
                        inputFields[fieldTab].InvokeSubmitResponse();
                        inputFields[fieldTab].isSelected = false;
                        fieldTab += 1;

                        inputFields[fieldTab].isSelected = true;
                    }
                    else
                    {
                        if (fieldTab + 1 == inputFields.Count)
                        {
                            inputFields[inputFields.Count - 1].isSelected = false;
                            inputFields[0].isSelected = true;
                            fieldTab = 0;
                        }


                    }

                    Global.cki = new ConsoleKeyInfo();
                }
            }
            else
            {
                foreach (InputField ifi in inputFields)
                {
                    ifi.isSelected = false;
                }
            }

            if((Global.cki.Modifiers & ConsoleModifiers.Control) != 0 && Global.cki.Key == ConsoleKey.P)
            {
                this.pDialog.isSelected = !pDialog.isSelected;
                Global.cki = new ConsoleKeyInfo();
            }
            #endregion

            #region ServerDiagnostics

            #if DEBUG
            ConsoleHelper.ClearRectInBuffer(0, 0, 10, 1, ref rBuffer);
            ConsoleHelper.WriteLineInBuffer(new COORD(), server.Statistics.SentMessages.ToString(), ref rBuffer);
            #endif
            
            #endregion

                #region LogMisc
                // WRAP THIS IN A DIALOG
            ConsoleHelper.DrawRectangle(0, Console.BufferHeight - 19, 50, 19, ref rBuffer, 0x0A | 0x0C | 0x0001);
   
    
            foreach (string msg in log)
            {
                
                    var words = msg.Split(' ');

                    for(int i = 0; i < words.Length; i++)
                    {
                        if (words[i] == "connected!")
                        {
                            ConsoleHelper.WriteLineInBufferWithExtent(new COORD(3, (short)y), msg, ref rBuffer, 47, 0x0A);
                            break;
                        }

                        if (words[i] == "disconnected!")
                        {
                            ConsoleHelper.WriteLineInBufferWithExtent(new COORD(3, (short)y), msg, ref rBuffer, 47, 0x0080 | 0x0040);
                            break;
                        }

                        ConsoleHelper.WriteLineInBufferWithExtent(new COORD(3, (short)y), msg, ref rBuffer, 47, 0x0080 | 0x0020);
                    }
                    y += 2;
            }
            y = Console.BufferHeight - 18;

            ClearLogInline();
            
            // WRAP THIS IN A DIALOG
            #endregion

            foreach (InputField field in inputFields)
            {
          
                field.Draw();
                field.Update();
            }

            
        }
        // 5 second rule for clean up (although you can add more time if you sleep)
        public void Cleanup()
        {
            // each client has a responsibility to remove their server from the directory
            Logger.Instance.Log("log", "attempting to delete server information from OSDL");

            // Attempt to post to OSDL
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://localhost:1812/home/Delete/" + serverInformation.Id);
                request.Method = "POST";
                request.Timeout = 3000; // 3 seconds time limit since we are already limited
                request.ContentType = "application/x-www-form-urlencoded";
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
               

                if (response.StatusCode == HttpStatusCode.OK)
                    Logger.Instance.Log("log", "Deleted server from directory ...");
                else
                    throw new Exception();
                
            }
            catch
            {
                Logger.Instance.Log("log", "Failed to delete server from directory!");
            }

            Logger.Instance.Flush();
            log.Add("server shutdown initiated...");
            sessionEnded = true;
            Logger.Instance.Log("server", "Reciever thread aborted");
            Logger.Instance.Flush();
            Logger.Instance.Log("log", "Cleaning up " + this.ToString());
            Logger.Instance.Flush();
            serverInformation = null;
            Logger.Instance.Log("server", "Server shutting down ...");
            server.Shutdown("Sever shutting down!");
            reciever.Abort();
            server.Connections.Clear(); // we no longer need to hold connection
            server = null;
            cDialog = null;

            // no longer allocate resources to run this thread
            Logger.Instance.Flush();
        }   
        private void RecieveMessages()
        {
            while (!sessionEnded)
            {
                server.MessageReceivedEvent.WaitOne(10); // 10ms wait since this is a busy loop. hackish optimization
                if ((msg = server.ReadMessage()) != null)
                {
                    switch (msg.MessageType)
                    {
                        case NetIncomingMessageType.StatusChanged:
                            // FIFO  The status change message is structured as <Byte><String>.
                            NetConnectionStatus newStatus = (NetConnectionStatus)msg.ReadByte();
                            if (newStatus == NetConnectionStatus.InitiatedConnect)
                            {
                                //ConsoleHelper.WriteLineInBuffer(new COORD(10, (short)y), newStatus.ToString(), ref rBuffer, 0x0C | 0x0B);
                            }

                            if(newStatus == NetConnectionStatus.Connected)
                            {
                                Logger.Instance.Log("server", msg.SenderConnection.RemoteEndPoint.ToString() + " connected!");
                                log.Add(msg.SenderConnection.RemoteEndPoint.ToString() + " has connected!");
                                Logger.Instance.Flush();
                                string username = msg.SenderConnection.RemoteHailMessage.ReadString();
                                Init_ClientDialog(msg.SenderConnection);
                                
                                cDialog.Add(msg.SenderConnection.RemoteEndPoint, username);
                                SendClientDialog_AddMessage(msg.SenderEndPoint, username);
                            }

                            if (newStatus == NetConnectionStatus.Disconnected)
                            {
                                Logger.Instance.Log("server", msg.SenderConnection.RemoteEndPoint.ToString() + " disconnected!");
                                log.Add(msg.SenderConnection.RemoteEndPoint.ToString() + " has disconnected!");
                                Logger.Instance.Flush();
                                cDialog.RemoveElement(msg.SenderConnection.RemoteEndPoint);
                                SendClientDialog_RemoveMessage(msg.SenderEndPoint);
                            }
                                break;

                        case NetIncomingMessageType.ConnectionApproval:
                            ConsoleHelper.WriteLineInBuffer(new COORD(20, 20), "Connection Approved", ref rBuffer, 0x0C);
                            msg.SenderConnection.Approve();
                            break;

                        case NetIncomingMessageType.Data:
                            MessageType messageType = (MessageType)msg.ReadByte();
                            switch (messageType)
                            {
                                case MessageType.ChatDialog_Broadcast:
                                    string usernameC = msg.ReadString();
                                    string chatmsgC = msg.ReadString();
                                    chat.ClearMsgEvent();
                                    chat.AddMsg(usernameC, chatmsgC, false);
                                    SendChatMessage_ExcludeSender(msg.SenderEndPoint, usernameC, chatmsgC);
                                    chat.sendMsgGlobal += SendChatMessage_ToClients;
                                   
                                   break;
                            }
                            break;

                        default:
                            ConsoleHelper.WriteLineInBuffer(new COORD(20, 20), "Unhandled type: " + msg.ReadString(), ref rBuffer, 0x0C);
                            break;
                    }

                    server.Recycle(msg);

                }

                }
            }
        private void HandleChatEvent(string message, ref bool validity)
        {
            
            validity = true;
            chat.AddMsg(cDialog.Admin, message);
        }
        private void HandleLogEvent(string message , ref bool validity)
        {
            string[] words = message.Split(' ');

            if (message.Contains("/"))
            {
                if (words.Length >= 2)
                {
                    if (words[0] == "/blacklist" && words[1] != " ")
                    {
                        validity = true;
                        Logger.Instance.Log("server", "Blacklisting" + words[1]);
                        Logger.Instance.Flush();
                        return;
                    }

                }

                if (words[0] == "/info" && words.Length == 1)
                {
                    validity = true;
                    Logger.Instance.Log("server", "Listing server information");
                    log.Add("Title: " + serverInformation.Title + "," + "Server Information: " + serverInformation.Description);
                    Logger.Instance.Flush();
                    return;
                }
                if (words[0] == "/clients" && words.Length == 1)
                {
                    validity = true;
                    Logger.Instance.Log("server", "Listing all clients");
                    Logger.Instance.Flush();
                    if (cDialog.DataSet.Count != 0)
                    {
                        foreach (var element in cDialog.DataSet)
                        {
                            log.Add(element.ToString());
                        }
                    }
                    else
                    {
                        log.Add("There are no clients to list");
                    }
                    return;
                }

                if (words[0] == "/shutdown" && words.Length == 1)
                {
                    ConsoleHelper.ClearBuffer(ref rBuffer);
                    PageManager.Instance.Pop();
                    PageManager.Instance.AddPage(new Menu(ref rBuffer));
                }

                if (words[0] == "/help" && words.Length == 1)
                {
                    validity = true;
                    log.Add("/blacklist <username>");
                    log.Add("/clients");
                    log.Add("/info -- server information");
                    ClearLogInline();
                    log.Add("/shutdown <reason>");
                    
                    return;
                }



                if (words[0] != "/help" ||  words[0] != "/blacklist" || words[0] != "/clients" || words[0] != "/shutdown")
                {
                    this.nd = new NotifyDialog(5, Console.BufferHeight - 10, 50, 5, ref sBuffer, false, "\n Looks like you're not entering\n the command correctly! \" /help \" if your still\n confused! That or it doesn't exist!");
                    this.nd.DialogDestroyed += new DialogDestroyed(Resume);
                    return;
                }

            }
            else
            {
                if (!string.IsNullOrWhiteSpace(message))
                {
                    this.nd = new NotifyDialog(5, Console.BufferHeight - 10, 50, 5, ref sBuffer, false, "\n Only console commands are allowed.\n Try entering a \\ followed by a valid command.\n Still confused? Try entering \"\\help\"");
                    this.nd.DialogDestroyed += new DialogDestroyed(Resume);
                }
            }
        }

        /* ALL EVENTS AND CALLBACKS ARE HANDlED BY THE SESSION PAGE AND NOT THEIR INDIVIDUAL COMPONENETS
         * */


        //Playlist Dialog Action (On Enter) - Begin To Stream
        private void PlaylistDialog_OnEnter(string musicLoc)
        {
            MusicPlayer plyr = new MusicPlayer(Environment.CurrentDirectory + "\\Playlists\\" + musicLoc);
            plyr.SendCompressedAudioChunk += new NetworkedEvent(MusicPLayer_SendAudioData);
            
            TagLib.File f = TagLib.File.Create(Environment.CurrentDirectory + "\\Playlists\\" + musicLoc);
            plyr.SendAudioChunks(1,f.Properties.AudioSampleRate); // 12 seconds of audio data
           
            string title = ExtensionMethods.ConvertStringArrayToString(f.Tag.Performers);
            uint year = f.Tag.Year;
            long length = f.Length;
           
           // plyr.Play();
            ConsoleHelper.WriteLineInBuffer(new COORD(70, 50),title, ref rBuffer);
            ConsoleHelper.WriteLineInBuffer(new COORD(70, 51), year.ToString(), ref rBuffer);
            ConsoleHelper.WriteLineInBuffer(new COORD(70, 52), length.ToString(), ref rBuffer);
        }

        private void MusicPLayer_SendAudioData(byte []chunkedAudioData, int bitDepth)
        {
            NetOutgoingMessage temp = server.CreateMessage();
            temp.Write((byte)MessageType.Audio_ByteData);
            temp.Write(bitDepth);
            temp.Write(chunkedAudioData.Count());
            temp.WritePadBits();
            temp.Write(chunkedAudioData);
            server.SendToAll(temp, NetDeliveryMethod.UnreliableSequenced);
        }
        //Authoratative Server Setup
        /*Client Events*/
        private void SendClientDialog_AddMessage(IPEndPoint ep, string message)
        {
            NetOutgoingMessage temp = server.CreateMessage();
            temp.Write((byte)MessageType.ClientDialog_Add);
            temp.Write(NetworkHelper.SerializeToBytes(ep).Length);
            temp.Write(NetworkHelper.SerializeToBytes(ep));
            temp.Write(message);
            server.SendToAll(temp, NetDeliveryMethod.UnreliableSequenced);
            Logger.Instance.Log("server", "Sent Client Update Event"); // all existing clients
            Logger.Instance.Flush();
        }
        private void Init_ClientDialog(NetConnection conn)
        {
            NetOutgoingMessage temp = server.CreateMessage();
            temp.Write((byte)MessageType.ClientDialog_Init);
            temp.Write(cDialog.Admin);
            temp.Write(cDialog.DataSet.Count());
            foreach (KeyValuePair<IPEndPoint, string> entry in cDialog.DataSet)
            {
                temp.Write(NetworkHelper.SerializeToBytes(entry.Key).Length);
                temp.Write(NetworkHelper.SerializeToBytes(entry.Key));
                temp.Write(entry.Value);
            }
            server.SendMessage(temp,conn, NetDeliveryMethod.UnreliableSequenced);
        }
        private void SendClientDialog_RemoveMessage(IPEndPoint ep)
        {
            NetOutgoingMessage temp = server.CreateMessage();
            temp.Write((byte)MessageType.ClientDialog_Remove);
            temp.Write(NetworkHelper.SerializeToBytes(ep).Length);
            temp.Write(NetworkHelper.SerializeToBytes(ep));
            server.SendToAll(temp, NetDeliveryMethod.UnreliableSequenced);
        }
        /*Chat Events*/
        // Anything that the admin types will be broadcasted
        private void SendChatMessage_ToClients(string user, string msg)
        {
            NetOutgoingMessage temp = server.CreateMessage();
            temp.Write((byte)MessageType.ChatDialog_Add);
            temp.Write(user);
            temp.Write(msg);

            server.SendToAll(temp, NetDeliveryMethod.UnreliableSequenced);
          
        } 
        // misc server function
        private void SendChatMessage_ExcludeSender(IPEndPoint excl, string username, string message)
        {
            foreach (NetConnection conn in server.Connections)
            {
                if (conn.RemoteEndPoint.ToString() != excl.ToString())
                {
                    Debug.WriteLine(conn.RemoteEndPoint.ToString());
                    NetOutgoingMessage temp = server.CreateMessage();
                    temp.Write((byte)MessageType.ChatDialog_Add);
                    temp.Write(username);
                    temp.Write(message);
                    server.SendMessage(temp, conn, NetDeliveryMethod.UnreliableSequenced);
                }
            }
        }
        private void Resume()
        {
            nd = null;
        }
        private void ClearLogInline()
        {

            if (log.Count * 2 > 17)
            {
                var temp = log.Last();
                log.Clear();
                log.Add(temp);

            }
        }
    }
    }

