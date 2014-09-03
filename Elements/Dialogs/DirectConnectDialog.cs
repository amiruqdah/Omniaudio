using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Omniaudio.Core;

namespace Omniaudio.Elements.Dialogs
{
    // allows user to connect directly to a server rather than those lsited  by the OSDL provided they give a valid  IP/PORT
    // and the server gives a postiive repsonse. 

    // Only to be used within the SESSION page
    class DirectConnectDialog : Dialog
    {

        #region Members 
        // Check Dialog class for inherited members
        private InputField ipField; // stores the ip data
        private InputField portField; // stores the port data
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

            // initalizing elemnts

            ipField = new InputField(_x + 5, _y + 2, 15, 1, ref rBuffer, 15, false, " IP GOES HERE DAWG"); // ip is regular 15 digits at max so this is our charlimit, do not change
            portField = new InputField(_x + 5, _y + 4, 5, 1, ref rBuffer, 5, false, "49168"); // deault omni audio port

            ipField.isSelected = true; // on default ipField is selected
        }



        // handles the update logic for the following dialog
        virtual public void Update()
        {

            // allows user to easily switch between the two input fields
            if (Global.cki.Key == ConsoleKey.Tab)
            {
                ipField.isSelected = !ipField.isSelected;
                portField.isSelected = !portField.isSelected;
                Global.cki = new ConsoleKeyInfo(); // remember to reset the object so you don't end up having a persistent state bug.
            }

            ipField.Update();
            portField.Update();
        }

        // handles draw *requests*

        // ***PLEASE NOTE THAT THE DIALOG DOES NOT HANDLE THE ACTUAL DRAWING LOGIC
        // that is the job of the pages themselves

        virtual public void Draw()
        {
            //Draw BackDrop (or background to make elemtns more visible
            ConsoleHelper.DrawRectangle(_x, _y, _w, _h, ref drawBuffer);


            ipField.Draw();
            portField.Draw();
            

            // boring label drawing stuff
            ConsoleHelper.WriteLineInBuffer(new COORD((short)(_x + 15),(short)(_y + 2)), "IP", ref drawBuffer);
        
        }

        #endregion Methods
    }
}
