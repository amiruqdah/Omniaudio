using System;
using Omniaudio.Core;
using System.Diagnostics;
using System.Timers;

namespace Omniaudio.Elements
{
    class Title : IElement
    {
        #region Variables
        private int _x, _y;
        private bool _isStatic;
        private bool hasBeenDrawn = false;
        private bool up = true;
        private float ElapsedTime;
        private CHAR_INFO[,] drawBuffer;
        Timer timer;
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
        #endregion

        #region Methods
        public Title(int x, int y, ref CHAR_INFO[,] renderBuffer,bool isStatic)
        {
            _x = x;
            _y = y;
            drawBuffer = renderBuffer;
            _isStatic = isStatic;
            timer = new Timer(Global.INTERVAL);
            timer.Elapsed += HandleTimerElapsed;
            timer.Start();
        }

        public void Update()
        {
            
            Animation_BobUpAndDown(200);

        }

        public void HandleTimerElapsed(object sender, ElapsedEventArgs e)
        {
            ElapsedTime += Global.INTERVAL;
        }
        public void Draw()
        {
            if (!hasBeenDrawn)
                SetTitle();

            if (_isStatic == true)
            {
                hasBeenDrawn = true;
            }
        }

        private void SetTitle()
        {
           

            int tempY = _y;



            string[] title = new string[5] {
            @"                             _                       _   _        ", 
       @"  ___    _ __ ___    _ __   (_)   __ _   _   _    __| | (_)   ___ ", 
      @" / _ \  | '_ ` _ \  | '_ \  | |  / _` | | | | |  / _` | | |  / _ \ ",
     @"| (_) | | | | | | | | | | | | | | (_| | | |_| | | (_| | | | | (_) |",
      @" \___/  |_| |_| |_| |_| |_| |_|  \__,_|  \__,_|  \__,_| |_|  \___/ "};

                for (int i = 0; i < 5; i++)
                {
                    COORD cp = new COORD((short)_x, (short)_y);
                    ConsoleHelper.WriteLineInBuffer(cp, title[i], ref drawBuffer, 0x0A);
                    _y += 1;
                }

                _y = tempY;
                               
        }

        private void Animation_BobUpAndDown(float perTick)
        {
            if (ElapsedTime > perTick)
            {
                ElapsedTime = 0;

                if (up)
                {
                    if (_y > 5)
                    {
                        ConsoleHelper.ClearRectInBuffer(_x, _y - 1, 75, 6,ref drawBuffer);
                        SetTitle();
                        //ConsoleHelper.MoveElement(_x, _y, 75, 6, 45, _y - 1);
                        _y-= 1;
                    }
                    else
                    {
                        up = false;
                    }
                }

                if (!up)
                {
                    //ConsoleHelper.MoveElement(_x, _y, 75, 6, 45, _y + 1);
                    ConsoleHelper.ClearRectInBuffer(_x, _y - 1, 75, 6, ref drawBuffer);
                    SetTitle();
                    _y += 1;

                    if (_y > 8)
                    {
                        up = true;
                    }
                }
            }
        }

        #endregion
    }
}
