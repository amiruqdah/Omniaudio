using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Omniaudio.Core;
using Lidgren.Network;
using Omniaudio.Properties;
using System.Net;

namespace Omniaudio.Elements.Dialogs
{

    class ClientDialog : Dialog
    {

        private Dictionary<IPEndPoint, string> Data = new Dictionary<IPEndPoint, string>();
        private string ADMIN;
        private short elementX, elementY;
        private string username = Settings.Default["username"].ToString();
        public ClientDialog(int xPos, int yPos, int width, int height, ref CHAR_INFO[,] rBuffer): base(xPos, yPos, width, height, ref rBuffer)
        {
            this._x = xPos;
            this._y = yPos;
            this._w = width;
            this._h = height;
            this.drawBuffer = rBuffer;
            this.elementX = (short)this._x;
            this.elementY = 0;

        }
        public Dictionary<IPEndPoint, string> DataSet
        {
            get { return Data; }
        }
        public string Admin
        {
            get { return ADMIN; }
            set
            {
                if (value.Count() > _w - 12)
                {
                    ADMIN = value.Substring(0, _w - 12);
                    ADMIN += "..";
                }
                else
                {
                    ADMIN = value;
                }
            }
        }
        public void Add(IPEndPoint sender, string element)
        {
            if (element.Length > _w - 5)
            {
                element = element.Substring(0, _w - 7);
                element += "..";
                Data.Add(sender, element);
            }
            else
                Data.Add(sender, element);
        }
        public void RemoveElement(IPEndPoint sender)
        {
            Data.Remove(sender);
        }
        public sealed override void Update()
        {
            base.Update();
        }
        public sealed override void Draw()
        {
            ConsoleHelper.DrawRectangle(_x, _y, _w, _h, ref drawBuffer, 0x0001 | 0x0002 | 0x004);
            ConsoleHelper.WriteLineInBuffer(new COORD((short)(_x + 2), (short)(_y + 2)), "[admin]" + ADMIN, ref drawBuffer,  0x1000 | 0x0008 | 0x0001 | 0x0002);
            
            if (Data.Count > 0)
            {
                elementY = (short)(_y + 4);

               
                foreach (var element in Data)
                {
                    if(element.Value != username)
                        ConsoleHelper.WriteLineInBuffer(new COORD((short)(_x + 2), (short)elementY), element.Value, ref drawBuffer,  0X0080 | 0x1000 | 0x0001 | 0x0002 | 0x0004 | 0x0008);
                    else
                        ConsoleHelper.WriteLineInBuffer(new COORD((short)(_x + 2), (short)elementY), element.Value, ref drawBuffer, 0X0080 | 0x1000 );
                    elementY += 2;
                }

            }

            else
            {
                ConsoleHelper.WriteLineInBuffer(new COORD((short)(_x + 5), (short)((_y + _h) / 2)), "No Clients", ref drawBuffer,  0X0080 | 0x1000 | 0xA | 0x0B | 0x0004 | 0x0008);
            }
        }

    }
}
