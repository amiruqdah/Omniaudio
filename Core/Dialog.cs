using System;

namespace Omniaudio.Core
{
    class Dialog : IElement
    {
        #region Variables
        protected int _x, _y,_w,_h;
        protected bool _isStatic = false;
        protected bool hasBeenDrawn;
        protected int selectorX, selectorY;
        protected CHAR_INFO[,] drawBuffer;
        #endregion

        #region Fields
        public int x {
            get { return _x; }
            set { _x = value; }
        }
        public int y{
            get { return _y; }
            set { _y = value; }
        }
        public bool isStatic{
            get { return _isStatic; }
        }
        #endregion

        #region Methods
        public Dialog(int x, int y, int w, int h, ref CHAR_INFO[,] rBuffer, bool isStatic = false)
        {
            _x = x;
            _y = y;
            _w = w;
            _h = h;
            drawBuffer = rBuffer;
            _isStatic = isStatic;
        }
        virtual public void Update()
        {
            // depend on the inheriting class to do whatever the hell it wants here
        }
        virtual public void Draw()
        {
            if (!hasBeenDrawn)
            {
                ConsoleHelper.DrawRectangle(_x, _y, _w, _h, ref drawBuffer);
                ConsoleHelper.WriteLineInBuffer(new COORD((short)x , (short)y), "<dialog>", ref drawBuffer, 0x0001 | 0x0002 | 0x0004 | 0x0008 | 0x0080);
            }
            if (_isStatic)
            {
                hasBeenDrawn = true;
            }
        }
        #endregion

    }
}
