using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Omniaudio.Core;
using System.Net;
using Lidgren.Network;
using Omniaudio.Properties;
using Omniaudio.Helpers;
using Omniaudio.Managers;
using Omniaudio.Pages;

namespace Omniaudio.Elements.Dialogs
{
    // allows user to connect directly to a server rather than those lsited  by the OSDL provided they give a valid  IP/PORT
    // and the server gives a postiive repsonse. 

    // Only to be used within the SESSION page
    class DirectConnectDialog : Dialog
    {
        #region Members 
        private List<InputField> fields;
        int fieldTab = 0;
        public Action destroyDialog;
        #endregion


        #region Methods

        // crapp init stuff
        public DirectConnectDialog(int xPos, int yPos, int width, int height, ref CHAR_INFO[,] rBuffer, bool isStatic = false) : base(xPos, yPos, width, height, ref rBuffer, isStatic)
        {
            this._x = xPos;
            this._y = yPos;
            this._w = width;
            this._h = height;

            this.drawBuffer = rBuffer;
            this._isStatic = _isStatic;

            fields = new List<InputField>();
            // initalizing elemnts

            fields.Add(new InputField(_x + 7, _y + 2, 15, 1, ref rBuffer, 15, false, " IP GOES HERE DAWG", CheckIP)); // ip is regular 15 digits at max so this is our charlimit, do not change
            fields.Add(new InputField(_x + 8, _y + 4, 5, 1, ref rBuffer, 5, false, "49168",CheckPort)); // deault omni audio port

            fields[0].isSelected = true; // on default ipField is selected
        }



        // handles the update logic for the following dialog
        virtual public void Update()
        {
            // allows user to easily switch between the two input fields
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

            if (fields[0].isSelected == false && fields[1].isSelected == false)
            {
                if (Global.cki.Key == ConsoleKey.C)
                {
                    Global.cki = new ConsoleKeyInfo();
                    destroyDialog();
                }

                if (Global.cki.Key == ConsoleKey.G)
                {
                    Global.cki = new ConsoleKeyInfo();

                    NetPeerConfiguration config = new NetPeerConfiguration("Omniaudio"); // nice app id
                    NetClient client = new NetClient(config);
                    client.Start();
                    NetOutgoingMessage hail = client.CreateMessage();
                    hail.Write(Settings.Default["username"].ToString());
                    client.Connect(fields[0].Message, int.Parse(fields[1].Message), hail);
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
                       //connection exception
                    }
                }
            }

            foreach (InputField ifield in fields)
            {
                ifield.Update();
            }
        }

        // handles draw *requests*

        // ***PLEASE NOTE THAT THE DIALOG DOES NOT HANDLE THE ACTUAL DRAWING LOGIC
        // that is the job of the pages themselves

        virtual public void Draw()
        {
            //Draw BackDrop (or background to make elemtns more visible
            ConsoleHelper.DrawRectangle(_x, _y, _w, _h, ref drawBuffer, 0x0001 | 0x0002 | 0x0004);

            //Cancel Button Drawing
            ConsoleHelper.DrawRectangle(_x, _y + 17, 10, 3, ref drawBuffer, 0x0C);
            ConsoleHelper.WriteLineInBuffer(new COORD((short)(_x + 1), (short)(_y + 18)), "(C)ancel", ref drawBuffer, 0x0A | 0x0C | 0x0040);
            // Go Button Drawing
            ConsoleHelper.DrawRectangle(_x + 40, _y + 17, 10, 3, ref drawBuffer, 0x0A | 0x0080);
            ConsoleHelper.WriteLineInBuffer(new COORD((short)(_x + 43), (short)(_y + 18)), "(G)o", ref drawBuffer, 0x0A | 0x0C | 0x0020);
            
            foreach (InputField ifield in fields)
            {
                ifield.Draw();
            }
            // boring label drawing stuff
            ConsoleHelper.WriteLineInBuffer(new COORD((short)(_x + 2),(short)(_y + 2)), "IP:", ref drawBuffer, 0x0010 | 0x0020 | 0x0040);
            ConsoleHelper.WriteLineInBuffer(new COORD((short)(_x + 2), (short)(_y + 4)), "Port:", ref drawBuffer, 0x0010 | 0x0020 | 0x0040);
                  
        }

        #endregion Methods

        #region Helpers
        
        private void CheckPort(string msg, ref bool validity)
        {
            if (!isDigitsOnly(msg))
            {
                validity = false;
            }
            else
            {
                validity = true;
            }

        }


        private void CheckIP(string ip, ref bool validity)
        {
            IPAddress address;
            if (IPAddress.TryParse(ip, out address))
            {
                validity = true;
            }
            else
            {
                validity = false;
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


        #endregion
    }
}
