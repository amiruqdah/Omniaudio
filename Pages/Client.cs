using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using Omniaudio.Helpers;
using Omniaudio.Elements.Dialogs;
using System.Threading;
using Omniaudio.Networking;
using System.Net;
using Omniaudio.Elements;
using Omniaudio.Properties;
using NAudio.Wave;
using System.IO;
using System.Diagnostics;

namespace Omniaudio.Pages
{
    // in retrospect I shouldn't have been using interfaces... I should have been using a class
    // it would have cut down a lot of duplication
    

    
    class Client : IPage
    {
        private CHAR_INFO[,] rBuffer;
        private COORD dwBufferSize = new COORD((short)Console.BufferWidth, (short)Console.BufferHeight);
        private COORD dwBufferCoord;
        private SMALL_RECT rcRegion = new SMALL_RECT(0, 0, (short)(Console.BufferWidth), (short)(Console.BufferHeight));
        private IntPtr oHandle = ConsoleHelper.GetStdOut();
        private CHAR_INFO[,] cBuffer;
        private bool clientEnded;

        private bool hasBeenInit = false;
        private string mUsername;

        //req
        private Thread reciever; 
        private NetClient client;
        private NetIncomingMessage msg;
        // TUI elements
        private ClientDialog cDialog;
        private ChatDialog chat;
        private InputField chatField;
        private NotifyDialog nd;

        //music req 
        WaveOut outputDevice = new WaveOut();
        private IWavePlayer m_waveOut;
        public Client(NetClient client)
        {
            rBuffer = new CHAR_INFO[Console.BufferHeight, Console.BufferWidth];
            cBuffer = new CHAR_INFO[Console.BufferHeight, Console.BufferWidth];
            ConsoleHelper.ClearBuffer(ref cBuffer); // clearing to be safe
            ConsoleHelper.ClearBuffer(ref rBuffer); // clearing to be safe
            dwBufferCoord.x = 0;
            dwBufferCoord.y = 0;

            this.client = client;
            
            this.mUsername = Settings.Default.username.ToString();
            clientEnded = false;
        }
        // no idea why I was passing buffers
        
        public void Init()
        {

            outputDevice.DesiredLatency = 300;
            outputDevice.NumberOfBuffers = 3;
          
            
                                    
            reciever = new Thread(RecieveMessages);
            reciever.IsBackground = true;

            chatField = new InputField((Console.BufferWidth - 140) / 2, 11, 129, 1, ref rBuffer, 125, true, "enter text here", HandleChatEvent);
            chatField.isSelected = true;
            chatField.ClearOnEnter = true;
            Logger.Instance.Log("log", "Initalizing " + this.GetType().ToString());
            Logger.Instance.Log("log", "Client Connection on " + client.ServerConnection.RemoteEndPoint.Address.ToString() + ": " + client.ServerConnection.RemoteEndPoint.Port.ToString());
            cDialog = new ClientDialog(Console.BufferWidth - 20, 1, 20, Console.BufferHeight - 1, ref rBuffer);
            chat = new ChatDialog((Console.BufferWidth - 140) / 2, 1, 130, 10, ref rBuffer);
            chat.sendMsgGlobal += SendChatMessage_ToAll;
            
            Logger.Instance.Flush();
            reciever.Start();
        }

        public void Update()
        {
            ConsoleHelper.WriteConsoleOutput(oHandle, cBuffer, dwBufferSize, dwBufferCoord, ref rcRegion);
            ConsoleHelper.CopyBuffer(ref rBuffer, ref cBuffer);

            chat.Update();
            chat.Draw();
            chatField.Update();
            chatField.Draw();
            cDialog.Update();
            cDialog.Draw();

            //Notify Dialog
            if (nd != null)
            {
                nd.Update();
                nd.Draw();
            }
        }

        public void Cleanup()
        {
            client.Shutdown("Client Disconnected");
            Thread.Sleep(500);
            client = null;
            cDialog = null;
            clientEnded = true;
        }
        private void RecieveMessages()
        {
            while (!clientEnded)
            {
                client.MessageReceivedEvent.WaitOne(10);
                if ((msg = client.ReadMessage()) != null)
                {
                    switch (msg.MessageType)
                    {
                        case NetIncomingMessageType.StatusChanged:
                            NetConnectionStatus newStatus = (NetConnectionStatus)msg.ReadByte();
                            
                            break;

                        case NetIncomingMessageType.Data:
                           MessageType messageType = (MessageType)msg.ReadByte();
                            switch (messageType)
                            {
                                    /*CLIENT DIALOG HANDLES*/
                                case MessageType.ClientDialog_Init:
                                    cDialog.Admin = msg.ReadString();
                                    int c = msg.ReadInt32();
                                    for (int i = 0; i < c; i++)
                                    {
                                        int byteL = msg.ReadInt32();
                                        cDialog.Add((IPEndPoint)NetworkHelper.DeserializeFromBytes(msg.ReadBytes(byteL)), msg.ReadString());
                                    }
                                    break;

                                case MessageType.ClientDialog_Add:
                                         int byteL2 = msg.ReadInt32();
                                        cDialog.Add((IPEndPoint)NetworkHelper.DeserializeFromBytes(msg.ReadBytes(byteL2)), msg.ReadString());
                                    break;

                                case MessageType.ClientDialog_Remove:
                                    int byteL3 = msg.ReadInt32();
                                    cDialog.RemoveElement((IPEndPoint)NetworkHelper.DeserializeFromBytes(msg.ReadBytes(byteL3)));
                                    break;

                                    /*CHAT HANDLES*/
                                case MessageType.ChatDialog_Add:
                                    chat.ClearMsgEvent();
                                    chat.AddMsg(msg.ReadString(), msg.ReadString(), false);
                                    chat.sendMsgGlobal += SendChatMessage_ToAll;
                                    break;

                                    /*MUSIC HANDLES*/
                                case MessageType.Audio_ByteData:
                                    ConsoleHelper.WriteLineInBuffer(new COORD(50, 50), "ok!", ref rBuffer);
                                    int sampleRate = msg.ReadInt32();
                                    int count= msg.ReadInt32();
                                    msg.SkipPadBits();
                                    byte[] audioData = msg.ReadBytes(count);
                                    Debug.Write(audioData.Count());
                                    MemoryStream byteStream = new MemoryStream(audioData);
                                    BufferedWaveProvider m_bufferedWaveProvider = new BufferedWaveProvider(new WaveFormat(sampleRate, 16, 2));
                                    m_bufferedWaveProvider.AddSamples(audioData, 0, audioData.Length);
                                    
                                    byteStream.Close();
                                    byteStream.Dispose();

                                        outputDevice.Init(m_bufferedWaveProvider);
                                        outputDevice.Play();
                                    
                                    

                                    break;
                            }
                            break;

                        case NetIncomingMessageType.WarningMessage:
                            nd = new NotifyDialog(40, 30, 60, 5, ref rBuffer, false, msg.ReadString());
                            nd.DialogDestroyed += onNotifyDialogDestroy;
                            break;

                        default:
                            ConsoleHelper.WriteLineInBuffer(new COORD(20, 20), "Unhandled type: " + msg.MessageType.ToString(), ref rBuffer, 0x0C);
                            break;
                    }

                    client.Recycle(msg);
                }
            }


        }
        private void HandleChatEvent(string message, ref bool validity)
        {
            validity = true;
            chat.AddMsg(mUsername, message);
        }
        private void SendChatMessage_ToAll(string usr, string msg)
        {
            NetOutgoingMessage temp = client.CreateMessage();

            temp.Write((byte)MessageType.ChatDialog_Broadcast);
            temp.Write(usr);
            temp.Write(msg);
            client.SendMessage(temp, NetDeliveryMethod.UnreliableSequenced);
        }
        private void onNotifyDialogDestroy()
        {
            nd.DialogDestroyed -= onNotifyDialogDestroy;
            nd = null;
        }
        private byte[] ConvertNonSeekableStreamToByteArray(Stream NonSeekableStream)
        {
            MemoryStream ms = new MemoryStream();
            byte[] buffer = new byte[1024];
            int bytes;
            while ((bytes = NonSeekableStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                ms.Write(buffer, 0, bytes);
            }
            byte[] output = ms.ToArray();
            return output;
        }
    }
}
