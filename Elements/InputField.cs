using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Omniaudio.Core;

namespace Omniaudio.Elements
{
    delegate void SubmitionEvent(string msg, ref bool validity);
    class InputField : IElement
    {
        #region Events
        public event SubmitionEvent SubmitResponse;
        #endregion
        #region Variables
        private LinkedList<char> inputData;
        private int _x, _y, _w, _h;
        private CHAR_INFO[,] drawBuffer;
        private bool _selected;
        private int charLimit;
        private bool _isStatic;
        private string dpString;
        private bool stretch;
        private bool _isValid;
        private int cIndex;
        private bool clearOnEnter;
        private bool hasEntered;
        #endregion
        #region Fields
        public int x
        {
            get
            {
                return _x;
            }
            set
            {
                _x = value;
            }
        }
        public int y
        {
            get { return _y; }
            set { _y = value; }
        }
        public bool isStatic
        {
            get
            {
                return _isStatic;
            }
        }
        public bool isSelected
        {
          get { return _selected; }
          set { _selected = value; }
        }
        public bool isValid
        {
            get { return _isValid;   }
            set { _isValid = value; }
        }
        public bool ClearOnEnter
        {
            set { clearOnEnter = value; }
        }
        #endregion
        #region Methods
        public InputField(int x, int y, int w, int h, ref CHAR_INFO[,] renderBuffer, int charLimit, bool stretch = false, string defaultInput = "", SubmitionEvent ev = null)
        {
            _x = x;
            _y = y;

            if (stretch)
                _w = w;
            else
                _w = charLimit;

            _h = h;
            drawBuffer = renderBuffer;
            isSelected = false;
            inputData = new LinkedList<char>();

            this.charLimit = charLimit;
            
            cIndex = 1;
            this.stretch = stretch;
            if (stretch)
                charLimit = _w * _h; // area forumla

            if (defaultInput != null)
            {
                for (int i = 0; i < defaultInput.Count(); i++)
                {
                    inputData.AddLast(defaultInput[i]);
                }
            }

            dpString = new string(inputData.ToArray());
            SubmitResponse = ev;
            _isValid = true;
            clearOnEnter = false;
            hasEntered = false;
            
        }

      

        public void InvokeSubmitResponse()
        {
            if(SubmitResponse != null)
            SubmitResponse(dpString,ref _isValid);
        }

        public void Update()
        {

            if (_selected && hasEntered)
            {
                if (clearOnEnter)
                {
                    inputData.Clear();
                    dpString = new string(inputData.ToArray());
                }
                    hasEntered = false;
            }

            if (_selected)
            {
                dpString = new string(inputData.ToArray());


                if (Global.cki.Key == ConsoleKey.Enter)
                {
                    if(!clearOnEnter)
                        _selected = false;

                    hasEntered = true;
                    if(SubmitResponse != null)
                        SubmitResponse(dpString, ref _isValid);
                    
                    Global.cki = new ConsoleKeyInfo();

                }

                if (dpString.Count() <= charLimit)
                {
                    if (Global.cki.KeyChar != '\0' && Global.cki.KeyChar != '\b')
                    {
                        inputData.AddLast(Global.cki.KeyChar);
                        Global.cki = new ConsoleKeyInfo();
                    }
                }
                if (stretch)
                {
                    for (int i = 0; i < inputData.Count(); )
                    {
                        int step = Math.Min(_w, inputData.Count() - i);
                        dpString = new string(inputData.ToArray()).Substring(i, step);
                        i += step;
                    }
                }

                if (Global.cki.KeyChar == '\b')
                {
                    if (inputData.Count != 0)
                    {
                        inputData.RemoveLast();
                    }
                    Global.cki = new ConsoleKeyInfo();
                }
            }

        }
        public string Message
        {
            get { return new string(inputData.ToArray()); }
            set
            {
                inputData.Clear();
                foreach (char c in value)
                {
                    inputData.AddLast(c);
                }
            }
        }
        public void Draw()
        {

            ConsoleHelper.ClearRectInBuffer(_x, _y, _w, 0, ref drawBuffer);


            if (_selected)
            {

                    ConsoleHelper.DrawRectangle(_x, _y, _w + 1, 1, ref drawBuffer, 0x0001 | 0x0008 | 0x0002);
                    ConsoleHelper.WriteLineInBuffer(new COORD((short)_x, (short)_y), dpString, ref drawBuffer, 0x0010 | 0x0080 | 0x0020 | 0x00001 | 0x00010 | 0x00080);             
            }
            else
            {
                if (_isValid)
                {
                    ConsoleHelper.DrawRectangle(_x, _y, _w + 1, 1, ref drawBuffer, 0x00001 | 0x00010 | 0x00080);
                    ConsoleHelper.WriteLineInBuffer(new COORD((short)_x, (short)_y), dpString, ref drawBuffer, 0x0001 | 0x0002 | 0x0004 | 0x0008 | 0x00001 | 0x00010 | 0x00080);
                }
                else
                {
                    ConsoleHelper.DrawRectangle(_x, _y, _w + 1, 1, ref drawBuffer, 0x0C);
                    ConsoleHelper.WriteLineInBuffer(new COORD((short)_x, (short)_y), dpString, ref drawBuffer, 0x0A | 0x0C | 0x0040);
                }
                    
            }



           
        }
        #endregion

    }
}
