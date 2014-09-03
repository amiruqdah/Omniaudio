using System;
using System.Collections.Generic;
using Omniaudio.Core;
using Omniaudio.Helpers;
using Omniaudio.Managers;
using Omniaudio.Pages;

namespace Omniaudio.Elements.Dialogs
{
    sealed class MenuDialog : Dialog
    {

        private List<string> options;
        private int _buttonY;
        private int _buttonX;
        private int _padding;
        private int max;

        #region Overriden Methods
        public MenuDialog(int xPos, int yPos, int width, int height, ref CHAR_INFO[,] rBuffer, bool isStatic): base(xPos, yPos, width, height, ref rBuffer,isStatic)
        {
            this._x = xPos;
            this._y = yPos;
            this._w = width;
            this._h = height;
            this.drawBuffer = rBuffer;
            this._isStatic = isStatic;
            this._buttonY = yPos;
            this.selectorY = this._buttonY;
            this.selectorX = _x;
            this.options = new List<string>();
        }

        public sealed override void Update()
        {
            if (Global.cki.Key == ConsoleKey.DownArrow)
            {
                selectorY += this._padding;

                Global.cki = new ConsoleKeyInfo();
            }

            if (Global.cki.Key == ConsoleKey.UpArrow)
            {
                selectorY -= this._padding;
                Global.cki = new ConsoleKeyInfo();
            }

            selectorY = ExtensionMethods.Clamp(selectorY, this.y, this.y + (_padding * options.Count) - _padding);
            if (Global.cki.Key == ConsoleKey.Enter)
            {
                // Playlists
                if (selectorY == this._y)
                {
                    ConsoleHelper.ClearBuffer(ref drawBuffer);
                    PageManager.Instance.Pop();
                    PageManager.Instance.AddPage(new Playlists(ref drawBuffer));
                }
                //Create Session
                if (selectorY == this._y + _padding)
                {
                    ConsoleHelper.ClearBuffer(ref drawBuffer);
                    PageManager.Instance.Pop();
                    PageManager.Instance.AddPage(new CreateSession(ref drawBuffer));
                }
                //Join Session
                if (selectorY == this._y + _padding * 2)
                {
                    ConsoleHelper.ClearBuffer(ref drawBuffer);
                    PageManager.Instance.Pop();
                    PageManager.Instance.AddPage(new JoinSession(ref drawBuffer));
                }

                // Settings
                if (selectorY == this.y + _padding * 3)
                {
                    ConsoleHelper.ClearBuffer(ref drawBuffer);
                    PageManager.Instance.Pop();
                    PageManager.Instance.AddPage(new UserSettings(ref drawBuffer));
                }
                //Quit
                if (selectorY == this.y + _padding * 4)
                {
                    ConsoleHelper.ClearBuffer(ref drawBuffer);
                    PageManager.Instance.Pop();
                    Program.hasQuit = true;
                }
                Global.cki = new ConsoleKeyInfo();
            }
        }
        public sealed override void Draw()
        {
            if (!hasBeenDrawn)
            {
                int originalY = _buttonY;
                foreach(string option in this.options)
                {
                     _buttonX = ((Console.BufferWidth - option.Length) / 2);
                     if (selectorY == _buttonY)
                         ConsoleHelper.WriteLineInBuffer(new COORD((short)_buttonX, (short)_buttonY), option, ref drawBuffer, 0x0001 | 0x0002 | 0x0004 | 0x0008);
                     else
                         ConsoleHelper.WriteLineInBuffer(new COORD((short)_buttonX, (short)_buttonY), option, ref drawBuffer, 0x0001 | 0x0002);
                     
                        _buttonY += _padding;
                }
                _buttonY = originalY;

            }

            if (_isStatic)
            {
                hasBeenDrawn = true;
            }
        }
        #endregion
        #region Properties
        public int Padding
        {
            set { _padding = value; }
        }
        #endregion
        #region Methods
        public void AddOptions(params string[] options)
        {
            max = this.selectorY + _padding * this.options.Count;
            foreach (string option in options)
            {
                this.options.Add(option);
            }
        }
        #endregion
    }
}
