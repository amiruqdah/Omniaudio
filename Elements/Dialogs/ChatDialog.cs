using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Omniaudio.Core;
using Omniaudio.Helpers;
using Lidgren.Network;
using Omniaudio.Properties;
using System.Net;

namespace Omniaudio.Elements.Dialogs
{

    
    // add spam protection in 0.2
    class ChatDialog : Dialog
    {

        public Action<string,string> sendMsgGlobal;
        List<string> chat;
        List<string> scrolledChat;

        int scrollX, scrollY;
        int page;
        int chatX, chatY;

        public ChatDialog(int xPos, int yPos, int width, int height, ref CHAR_INFO[,] rBuffer): base(xPos, yPos, width, height, ref rBuffer)
        {
            this._x = xPos;
            this._y = yPos;
            this._w = width;
            this._h = height;
            this.drawBuffer = rBuffer;
            this.chat = new List<string>();
            this.scrolledChat = new List<string>();
            this.chatX = xPos + 2;
            this.chatY = yPos + 1;
            scrollX = _x + (_w - 2);
            scrollY = _y;
            page = 1;
        }
        public void AddMsg(string username, string msg, bool broadcast = true)
        {
            if (!string.IsNullOrWhiteSpace(msg))
            {
                chat.Add("<" + username + ">: " + msg);
                if (msg.Contains("@" + Settings.Default["username"].ToString()) && username != Settings.Default["username"].ToString())
                    Console.Beep(); // someone mentioned you, dood
            }
            else
            {
                return; // Don't do jack, because they want to spam 
            }

            if (chat.Count > _h / 2)
            {
                scrollY += 1; // snap it one char down

                scrolledChat.Add(chat.First<string>());
                chat.Remove(chat.First<string>());
            }


            if(broadcast)
                if(sendMsgGlobal != null)
                 sendMsgGlobal(username, msg);
         
            
        }
        public sealed override void Update()
        {
            if (chat.Count > _h / 2)
            {
                // handle some events for dialog control
                if (Global.cki.Key == ConsoleKey.DownArrow)
                {
                    scrollY += 1; // snap it one char down

                    scrolledChat.Add(chat.First<string>());
                    chat.Remove(chat.First<string>());
                   
                    Global.cki = new ConsoleKeyInfo();
                }
            }

            if(scrolledChat.Count != 0)
            {
                if (Global.cki.Key == ConsoleKey.UpArrow)
                {
                    scrollY -= 1;
                    chat.Insert(0, scrolledChat.Last<string>());
                    scrolledChat.Remove(scrolledChat.Last<string>());
                    Global.cki = new ConsoleKeyInfo();
                }
            }

            //limit scroll thru clamp
            scrollY = ExtensionMethods.Clamp(scrollY, _y, _y + (_h - 1));
            

            if (page > 10)
            {
                chat.Clear();
                //reset selector positions
            }
        }     
        public sealed override void Draw()
        {
            ConsoleHelper.DrawRectangle(_x, _y, _w, _h, ref drawBuffer);
            ConsoleHelper.DrawRectangle(scrollX, scrollY, 2, 1, ref drawBuffer, 0x0001 | 0x0002);
            
            int origChatY = chatY;
            int entry = 0;

            foreach (string msg in chat)
            {
                ConsoleHelper.WriteLineInBufferWithExtent(new COORD((short)chatX, (short)chatY), msg, ref drawBuffer, _w, 0x0001 | 0x0002 | 0x0004 | 0x0008 | 0x0080);
                chatY += 2;
                entry += 1;
                if (entry > (_h / 2) - 1)
                    break;
            }

            chatY = origChatY; // important to prevent buffer overflow
        }
        public  void ClearMsgEvent()
        {
            this.sendMsgGlobal = null;
        }
    }
}
