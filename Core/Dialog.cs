using System;

namespace Omniaudio.Core
{
    /// <summary>
    /// base class for all Dialogs. Dialogs are specialized units, that perform their own uniuqe functions and abilities
    /// and can be initalised anywhere on a Page.
    /// </summary>
    class Dialog : IElement
    {
        #region Variables
        protected int _x, _y,_w,_h; // position, width, and height
        protected bool _isStatic = false; // if this is set to true the dialog will redraw its backdrop several times, per specified interval
        protected bool hasBeenDrawn; // a dirty flag
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
        /// <summary>
        /// Initalizes a Dialog
        /// </summary>
        /// <param name="x">The position of the dialog on the X axis of the draw buffer</param>
        /// <param name="y">The position of the dialog on the Y axis of the draw buffer</param>
        /// <param name="w">The width of the dialog along the X axis</param>
        /// <param name="h">The height of the dialog along the Y axis</param>
        /// <param name="rBuffer">The buffer to draw the dialog to</param>
        /// <param name="isStatic">A flag specifying if the dialog will constantly be redrawn</param>
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
        // default Draw function. If this function is not overridden in the overriding class, this will be executed
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
