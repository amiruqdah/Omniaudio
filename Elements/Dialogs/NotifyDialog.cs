using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Omniaudio.Core;

namespace Omniaudio.Elements.Dialogs
{
    delegate void DialogDestroyed();

    sealed class NotifyDialog :Dialog
    {
        #region Actions
        public Action DialogCreated;
        public event DialogDestroyed DialogDestroyed; // fix this
        #endregion
        
        #region Overriden Methods
        public NotifyDialog(int xPos, int yPos, int width, int height, ref CHAR_INFO[,] rBuffer, bool isStatic, string message): base(xPos, yPos, width, height, ref rBuffer,isStatic)
        {
            this._x = xPos;
            this._y = yPos;
            this._w = width;
            this._h = height;
            this.drawBuffer = rBuffer;
            this._isStatic = isStatic;
            Message = message;

        }
        public sealed override void Update()
        {
            if (Global.cki.Key == ConsoleKey.Backspace || Global.cki.Key == ConsoleKey.X)
            {
                if (DialogDestroyed != null)
                {
                    DialogDestroyed();
                }
                Global.cki = new ConsoleKeyInfo();
            }
        }

        public string Message
        {
            get;
            set;
        }

        public sealed override void Draw()
        {

            ConsoleHelper.DrawRectangle(_x, _y, _w,Message.Count(f => f == '\n') + 2, ref drawBuffer,0x0C);
            if (Message != null)
            {
                ConsoleHelper.WriteLineInBuffer(new COORD((short)_x, (short)_y), Message, ref drawBuffer, 0x0C | 0x0A | 0x0B |0x0080| 0x0040);
            }

            ConsoleHelper.WriteLineInBuffer(new COORD((short)(_x + _w - 1), (short)_y), "X", ref drawBuffer, 0x0001 | 0x0008 | 0x0002);

            
        }


        #endregion

    }
}
