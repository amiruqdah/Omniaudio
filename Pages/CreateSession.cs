using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using Omniaudio.Elements;
using Omniaudio.Elements.Dialogs;
using Omniaudio.Managers;
using Omniaudio.Networking;
using System.Runtime.Serialization.Json;
using Omniaudio.Helpers;

namespace Omniaudio.Pages
{
    class CreateSession : IPage
    {
        //network essentials 
        // we are going to need these attrs to post to the server directory
        
        private string _ip;
        private int _port;
        private string _desc;
        private string _title;
        private int _maxUsers;
        private int fieldTab;
        private bool canUpdate;
        private bool canNavigate;

        private CHAR_INFO[,] rBuffer;
        private COORD dwBufferSize = new COORD((short)Console.BufferWidth, (short)Console.BufferHeight);
        private COORD dwBufferCoord;
        private SMALL_RECT rcRegion = new SMALL_RECT(0, 0, (short)(Console.BufferWidth), (short)(Console.BufferHeight));
        private IntPtr oHandle = ConsoleHelper.GetStdOut();
        private CHAR_INFO[,] cBuffer;
        private NotifyDialog _nd;
        private int recX, recY;

        private List<InputField> fields;

        public CreateSession(ref CHAR_INFO[,] buffer)
        {
            this.rBuffer = buffer;
            rBuffer = new CHAR_INFO[Console.BufferHeight, Console.BufferWidth];
            cBuffer = new CHAR_INFO[Console.BufferHeight, Console.BufferWidth];
            ConsoleHelper.ClearBuffer(ref cBuffer); // clearing to be safe
            ConsoleHelper.ClearBuffer(ref rBuffer); // clearing to be safe
            dwBufferCoord.x = 0;
            dwBufferCoord.y = 0;

            fieldTab = 0;
            fields = new List<InputField>();
            recX = (Console.BufferWidth - 50) / 2;
            recY = (Console.BufferHeight - 20) / 2;
            canUpdate = true;
            _ip = null;
        }
        public void Init()
        {
            Logger.Instance.Log("log", "Initalizing " + this.ToString());
            try
            {
                if (_ip == null)
                {
                    // too bad I don't have .NET 4.5 available this machine :( no async
                    _ip = new System.Net.WebClient().DownloadString("http://bot.whatismyipaddress.com/");
                    _ip = System.Text.RegularExpressions.Regex.Replace(_ip, @"\t|\n|\r", "");
                }

                Logger.Instance.Log("log", "Grabbing IP from WebClient");
            }
            catch
            {
                if (_ip == null)
                {
                    try
                    {
                        _ip = new System.Net.WebClient().DownloadString("http://ipinfo.io/ip");
                        _ip = System.Text.RegularExpressions.Regex.Replace(_ip, @"\t|\n|\r", "");
                    }
                    catch { }
                }
                    Logger.Instance.Log("log", "Grabbing IP from WebClient");
            }
               


            _nd = new NotifyDialog(5, 10, 160, 5, ref cBuffer, false, "test");
            _nd.DialogCreated = Hold;
            _nd.DialogDestroyed += new DialogDestroyed(Resume);
            _nd.Message = "So, you're hosting a server eh?\n\nMake sure that no fields are left empty, and make sure that your IP and port are valid!\n\nFields that are \"incorrect\" are a reddish maroon. Close this, and host the damn server!";

            fields.Add(new InputField(recX + 16, recY + 3, 12, 1, ref rBuffer, 20,false,"",CheckEmpty));
            fields.Add(new InputField(recX + 16, recY + 5, 32, 1, ref rBuffer, 100, true,"",CheckEmpty));
            fields.Add(new InputField(recX + 16, recY + 7, 12, 1, ref rBuffer, 15, false, _ip, CheckIP));
            fields.Add(new InputField(recX + 16, recY + 9, 12, 1, ref rBuffer, 5, false, "49168",CheckPort));
            fields.Add(new InputField(recX + 16, recY + 11, 12, 1, ref rBuffer, 1, false, "10", CheckMaxUsers));

            
            fields[0].isSelected = true;
            fields[3].isValid = true;
            fields[4].isValid = true;

            fields[0].InvokeSubmitResponse();
            fields[1].InvokeSubmitResponse();
            fields[3].InvokeSubmitResponse();
            fields[4].InvokeSubmitResponse();

            Logger.Instance.Flush();
        }

        public void Update()
        {
            ConsoleHelper.WriteConsoleOutput(oHandle, cBuffer, dwBufferSize, dwBufferCoord, ref rcRegion);
            ConsoleHelper.CopyBuffer(ref rBuffer, ref cBuffer);

            if (canUpdate)
            {
                #region Misc Draw Code

                ConsoleHelper.DrawRectangle(recX, recY, 50, 20, ref rBuffer, 0x0001 | 0x0002 | 0x0004);
                ConsoleHelper.DrawRectangle(recX, recY + 17, 10, 3, ref rBuffer,0x0C);
                ConsoleHelper.WriteLineInBuffer(new COORD((short)(recX + 1), (short)(recY + 18)), "(C)ancel", ref rBuffer, 0x0A | 0x0C | 0x0040);
                ConsoleHelper.DrawRectangle(recX + 40, recY + 17, 10, 3, ref rBuffer, 0x0A | 0x0080);
                ConsoleHelper.WriteLineInBuffer(new COORD((short)(recX + 43), (short)(recY + 18)), "(G)o", ref rBuffer, 0x0A | 0x0C | 0x0020);

                ConsoleHelper.WriteLineInBuffer(new COORD((short)(recX + 3), (short)(recY + 3)), "Server Name:", ref rBuffer, 0x0010 | 0x0020 | 0x0040);
                ConsoleHelper.WriteLineInBuffer(new COORD((short)(recX + 3), (short)(recY + 5)), "Description:", ref rBuffer, 0x0010 | 0x0020 | 0x0040);
                ConsoleHelper.WriteLineInBuffer(new COORD((short)(recX + 12), (short)(recY + 7)), "I.P", ref rBuffer, 0x0010 | 0x0020 | 0x0040);
                ConsoleHelper.WriteLineInBuffer(new COORD((short)(recX + 11), (short)(recY + 9)), "Port", ref rBuffer, 0x0010 | 0x0020 | 0x0040);
                ConsoleHelper.WriteLineInBuffer(new COORD((short)(recX + 6), (short)(recY + 11)), "Max Users", ref rBuffer, 0x0010 | 0x0020 | 0x0040);
                ConsoleHelper.WriteLineInBuffer(new COORD((short)(recX + 19), (short)(recY + 11)), "(1-20)", ref rBuffer, 0x0010 | 0x0020 | 0x0040);
                #endregion

                if (Global.cki.Key == ConsoleKey.Tab)
                {
                    if (fieldTab + 1 < fields.Count)
                    {
                            fields[fieldTab].InvokeSubmitResponse();
                            fields[fieldTab].isSelected = false;
                            fieldTab += 1;
                        
                            fields[fieldTab].isSelected = true;
                    }
                    else
                    {
                        if (fieldTab + 1 == fields.Count)
                        {
                               
                                fields[fields.Count - 1].isSelected = false;
                                fields[0].isSelected = true;
                                fieldTab = 0;
                        }
                    }

                    Global.cki = new ConsoleKeyInfo();
                }
          

                if (_nd != null)
                {
                    _nd.Draw();
                    _nd.Update();
                }

                foreach (InputField ifield in fields)
                {
                    ifield.Update();
                    ifield.Draw();
                }

                foreach (InputField ifield in fields)
                {
                    if (ifield.isValid)
                        canNavigate = true;
                    else
                    {
                        canNavigate = false;
                        break;
                    }
                 }

                _title = fields[0].Message;
                _desc = fields[1].Message;
                _ip = fields[2].Message;
                int.TryParse(fields[3].Message,out _port);

                if(!string.IsNullOrWhiteSpace(fields[4].Message) && isDigitsOnly(fields[4].Message))
                    _maxUsers = Convert.ToInt32(fields[4].Message);

               
            }


                    if (Global.cki.Key == ConsoleKey.G && canNavigate == true)
                    {

                            Logger.Instance.Log("log", "Storing Server Information");
                           
                            canUpdate = false;
                            ConsoleHelper.ClearBuffer(ref rBuffer);
                            Global.cki = new ConsoleKeyInfo();

                            Server tS = new Server();
                            tS.Title = _title;

                            tS.Port = _port;
                            tS.Description = _desc;
                            tS.InternetProtocol = _ip;
                            tS.MaxListeners = 10;

                            Logger.Instance.Log("log", "attempting to add to OSDL..");
                        
                            // post server information
                            try
                            {
                                MemoryStream m1 = new MemoryStream();
                                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Server));
                                ser.WriteObject(m1, tS);
                                m1.Position = 0;
                                StreamReader sr = new StreamReader(m1);
                                byte[] postBytes = Encoding.UTF8.GetBytes(sr.ReadToEnd());
                                System.Net.WebRequest wr = WebRequest.Create("http://localhost:1812/home/CreateServer");
                                wr.Method = "POST";
                                wr.Timeout = 2000;
                                wr.ContentLength = postBytes.Length;
                                wr.ContentType = " application/json";
                                Stream dataStream = wr.GetRequestStream();
                                dataStream.Write(postBytes, 0, postBytes.Length);
                                dataStream.Close();
                                System.Net.HttpWebResponse wo = (HttpWebResponse)wr.GetResponse();
                                sr = new StreamReader(wo.GetResponseStream());
                                // ConsoleHelper.WriteLineInBuffer(new COORD(0, 0), wo.GetResponseStream().ToString(), ref rBuffer, 0x0A);
                                tS.Id = int.Parse(sr.ReadToEnd());
                                if (wo.StatusCode == HttpStatusCode.BadRequest || wo.StatusCode == HttpStatusCode.Forbidden || wo.StatusCode == HttpStatusCode.NotFound)
                                {
                                    throw new Exception();
                                }
                                if (wo.ContentLength == 0)
                                {
                                    throw new Exception();
                                }

                                wo.Close();
                                sr.Dispose();
                                Logger.Instance.Log("log", "server information successfully logged to OSDL!");
                            }
                            catch
                            {
                                Logger.Instance.Log("log", "server information failed to log to OSDL!");
                            }

                            Logger.Instance.Flush();
                            PageManager.Instance.Pop();
                            PageManager.Instance.AddPage(new Session(tS));
                            Logger.Instance.Flush();
                  
                }
                    if (Global.cki.Key == ConsoleKey.C)
                    {
                        Logger.Instance.Log("log", "Cancel Button Activated");
                        Logger.Instance.Flush();
                        Global.cki = new ConsoleKeyInfo();
                        ConsoleHelper.ClearBuffer(ref rBuffer);
                        PageManager.Instance.Pop();
                        PageManager.Instance.AddPage(new Menu(ref rBuffer));

                    }

            
        }

        public void Cleanup()
        {
            Logger.Instance.Log("log", "Cleaning " + this.ToString());
            Logger.Instance.Flush();
            fields.Clear();
            fields = null;
        }

        private  string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

        private void CheckIP(string ip,ref bool validity)
        {
            IPAddress address;
            if (IPAddress.TryParse(ip, out address))
            {
                validity = true;
            }
            else
            {
                /*_nd = new NotifyDialog(5, 10, 160, 5, ref cBuffer, false, "test");
                _nd.DialogCreated = Hold;
                _nd.DialogDestroyed += new DialogDestroyed(Resume);
                _nd.Message = "Woah, slow your roll before you lose your soul!\n\nLooks like your I.P is invalid... or my code is broken! I'm going to blame you though! \\_(. _ .)_/";
                
                 * */
                canNavigate = false;
                validity = false;
            }
        }

        private void CheckMaxUsers(string msg, ref bool validity)
        {

            if (!string.IsNullOrWhiteSpace(msg) && isDigitsOnly(msg))
            {
                int usr = Convert.ToInt32(msg);

                //elegant not efficent
                if (Enumerable.Range(1, 20).Contains(usr))
                    validity = true;
                else
                    validity = false;

            }
            else
            {
                validity = false;
            }


        }

        private void CheckEmpty(string msg, ref bool validity)
        {
            if (string.IsNullOrWhiteSpace(msg))
            {
                canNavigate = false;
                validity = false;
            }
            else
            {
                validity = true;
            }
        }
        private void CheckPort(string msg, ref bool validity)
        {
            if (!isDigitsOnly(msg))
            {
                /*
                _nd = new NotifyDialog(5, 10, 160, 5, ref cBuffer, false, "test");
                _nd.DialogCreated = Hold;
                _nd.DialogDestroyed += new DialogDestroyed(Resume);
                _nd.Message = "\nHey! Your port should only contain numbers, not letters! I forgive you. （ｖ＾＿＾;）ｖ";
                
                 * */canNavigate = false;
                     validity = false;
            }
            else
            {
                validity = true;
            }
            
        }


        private bool isDigitsOnly(string str)
        {
            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }

        private void Hold()
        {
            canUpdate = false;
        }

        private void Resume()
        {
            canUpdate = true;
            _nd = null;
        }
    }
}
