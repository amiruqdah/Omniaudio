using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Omniaudio.Helpers;
using Omniaudio.Elements;
using Omniaudio.Managers;
using Omniaudio.Properties;

namespace Omniaudio.Pages
{
    class UserSettings : IPage
    {
        private InputField userNameField;

        private CHAR_INFO[,] rBuffer;
        private COORD dwBufferSize = new COORD((short)Console.BufferWidth, (short)Console.BufferHeight);
        private COORD dwBufferCoord;
        private SMALL_RECT rcRegion = new SMALL_RECT(0, 0, (short)(Console.BufferWidth), (short)(Console.BufferHeight));
        private IntPtr oHandle = ConsoleHelper.GetStdOut();
        private CHAR_INFO[,] uBuffer;

        private int recX, recY;

        public UserSettings(ref CHAR_INFO[,] buffer)
        {
            this.rBuffer = buffer;

            uBuffer = new CHAR_INFO[Console.BufferHeight, Console.BufferWidth];
            rBuffer = new CHAR_INFO[Console.BufferHeight, Console.BufferWidth];
            ConsoleHelper.ClearBuffer(ref uBuffer); // clearing to be safe
            ConsoleHelper.ClearBuffer(ref rBuffer); // clearing to be safe
            dwBufferCoord.x = 0;
            dwBufferCoord.y = 0;
        }

        public void Init()
        {
            Logger.Instance.Log("log", "Initializing " + this.ToString());

            userNameField = new InputField((Console.BufferWidth - 50) / 2 + 17,20, 20, 18, ref rBuffer, 15, true, "stranger", CheckEmptyAndStringValid);
            userNameField.Message = Settings.Default["username"].ToString();
            userNameField.isSelected = true;

            recX = (Console.BufferWidth - 50) / 2;
            recY = (Console.BufferHeight - 20) / 2;

            Logger.Instance.Flush();
        }

        public void Update()
        {
            ConsoleHelper.WriteConsoleOutput(oHandle, uBuffer, dwBufferSize, dwBufferCoord, ref rcRegion);
            ConsoleHelper.CopyBuffer(ref rBuffer, ref uBuffer);
            ConsoleHelper.DrawRectangle(recX, recY, 50, 20, ref rBuffer, 0x0001 | 0x0002 | 0x0004);
            ConsoleHelper.WriteLineInBuffer(new COORD((short)(recX + 6), (short)(recY+ 2)), " Username:", ref rBuffer, 0x0001 | 0x0002 | 0x0004);
           
            ConsoleHelper.DrawRectangle(recX + 40, recY + 17, 10, 3, ref rBuffer, 0x0A | 0x0080); // Save
            ConsoleHelper.DrawRectangle(recX, recY + 17, 10, 3, ref rBuffer, 0x0C);  // Cancel

            ConsoleHelper.WriteLineInBuffer(new COORD((short)(recX + 1), (short)(recY + 18)), "(C)ancel", ref rBuffer, 0x0A | 0x0C | 0x0040);
            ConsoleHelper.WriteLineInBuffer(new COORD((short)(recX + 42), (short)(recY + 18)), "(S)ave", ref rBuffer, 0x0A | 0x0C | 0x0020);
           
            userNameField.Update();
            userNameField.Draw();

            if (Global.cki.Key == ConsoleKey.C && userNameField.isSelected == false)
            {
                Global.cki = new ConsoleKeyInfo();
                PageManager.Instance.Pop();
                PageManager.Instance.AddPage(new Menu(ref rBuffer));
            }

            if (Global.cki.Key == ConsoleKey.S && 
                userNameField.isSelected == false &&
                    userNameField.isValid)
            {
                Settings.Default["username"] = userNameField.Message;
                Settings.Default.Save();

                Global.cki = new ConsoleKeyInfo();

                PageManager.Instance.Pop();
                PageManager.Instance.AddPage(new Menu(ref rBuffer));
            }

            if (Global.cki.Key == ConsoleKey.Tab)
            {
                userNameField.isSelected = true;
                Global.cki = new ConsoleKeyInfo();
            }
        }

        public void Cleanup()
        {
            Logger.Instance.Log("log", "Cleaning up " + this.ToString());
            Logger.Instance.Flush();
            userNameField = null;
        }

        // Error Handling stuff
        private void CheckEmptyAndStringValid(string msg, ref bool validity)
        {
            if (string.IsNullOrWhiteSpace(msg) || msg.ToUpper().Contains("ADMIN"))
            {
                validity = false;
            }
            else
            {
                validity = true;
            }
        }

    }
}
