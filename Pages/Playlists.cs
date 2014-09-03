using System;
using System.Collections.Generic;
using System.IO;
using Omniaudio.Core;
using System.Runtime.InteropServices;
using Omniaudio.Elements.Dialogs;
using System.Net;
using Omniaudio.Networking;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Omniaudio.Pages
{
    class Playlists : IPage
    {
        // needed for double buffering
        private CHAR_INFO[,] rBuffer;
        private CHAR_INFO[,] pBuffer;

        private COORD dwBufferSize = new COORD((short)Console.BufferWidth, (short)Console.BufferHeight);
        private COORD dwBufferCoord = new COORD(0, 0);
        private SMALL_RECT rcRegion = new SMALL_RECT(0, 0, (short)(Console.BufferWidth), (short)(Console.BufferHeight));
        private IntPtr oHandle = ConsoleHelper.GetStdOut();
        private bool displayPlaylistInfo;

        private PlaylistDialog pDialog;
        private NotifyDialog nDialog;
        private bool canUpdate;

        public Playlists(ref CHAR_INFO [,]buffer)
        {
            
            rBuffer = new CHAR_INFO[Console.BufferHeight, Console.BufferWidth];
            ConsoleHelper.ClearBuffer(ref rBuffer);
            pBuffer = new CHAR_INFO[Console.BufferHeight, Console.BufferWidth];
            ConsoleHelper.ClearBuffer(ref pBuffer);
            ConsoleHelper.WriteConsoleOutput(oHandle, pBuffer, dwBufferSize, dwBufferCoord, ref rcRegion);

            displayPlaylistInfo = false;
        }
        public void Init()
        {

            if (Directory.Exists("Playlists"))
            {
                displayPlaylistInfo = true;
            }

            pDialog = new PlaylistDialog((Console.BufferWidth - 67 )/ 2, 20,67, 29, ref pBuffer, false);
            pDialog.PlaylistDialogException += NotifyAboutPlaylistDialog;
            canUpdate = true;
                
        }
        public void Update()
        {

            ConsoleHelper.WriteConsoleOutput(oHandle, pBuffer, dwBufferSize, dwBufferCoord, ref rcRegion);
            ConsoleHelper.CopyBuffer(ref rBuffer, ref pBuffer);

         
                if (displayPlaylistInfo)
                {
                    if (canUpdate)
                    {
                        pDialog.Update();
                    }
                        pDialog.Draw();

                                                           
                    ConsoleHelper.DrawRectangle(0, Console.BufferHeight - 6, 12, 3, ref pBuffer, 0x0001);
                    ConsoleHelper.WriteLineInBuffer(new COORD(1, (short)(Console.BufferHeight - 5)), "(-) Remove", ref pBuffer, 0x0A | 0x0C | 0x0010);
                    ConsoleHelper.DrawRectangle(Console.BufferWidth - 12, Console.BufferHeight - 6, 12, 3, ref pBuffer, 0x0001);
                    ConsoleHelper.WriteLineInBuffer(new COORD((short)(Console.BufferWidth - 9), (short)(Console.BufferHeight - 5)), "(+) Add", ref pBuffer, 0x0A | 0x0C | 0x0010); // yellow highlighted text
                    ConsoleHelper.WriteLineInBuffer(new COORD((short)((Console.BufferWidth - 23) / 2), (short)(Console.BufferHeight - 50)), "Playlist(s) Folder Found!", ref pBuffer, 0x0A);
                }
                else
                {
                    ConsoleHelper.WriteLineInBuffer(new COORD((short)((Console.BufferWidth - 23) / 2), (short)(Console.BufferHeight - 50)), "Playlist Folder Not Found!", ref pBuffer, 0x0C);
                }

                ConsoleHelper.WriteLineInBuffer(new COORD((short)((Console.BufferWidth - 18) / 2), (short)(Console.BufferHeight - 2)), "(C) Code Asssassin 2014  ", ref pBuffer, 0x0001 | 0x0002 | 0x0004 | 0x0008);
         

                if (Global.cki.Key == ConsoleKey.Backspace)
                {
                        ConsoleHelper.ClearBuffer(ref pBuffer);
                        Omniaudio.Managers.PageManager.Instance.Pop();
                        Omniaudio.Managers.PageManager.Instance.AddPage(new Menu(ref pBuffer));
                        Global.cki = new ConsoleKeyInfo();
                }
                if (displayPlaylistInfo)
                {
                    if (Global.cki.KeyChar == '+')
                    {
                        Global.OpenFileName op = new Global.OpenFileName();
                        op.lstructSize = Marshal.SizeOf(op);
                        op.lpstrFilter = "Music\0*.mp3";
                        op.lpstrFile = new string(new char[256]);
                        op.lMaxFile = op.lpstrFile.Length;
                        op.lpstrInitialDir = Environment.CurrentDirectory;
                        op.lpstrTitle = "Add Tunes";
                        op.lFlags = 0x00000200 | 0x00080000; // some cryptic flags that I'm too lazy to make readable

                        WindowHelper.GetOpenFileName(op);
                    

                        if (op != null)
                        {
                            string dest = op.lpstrFile.Substring(op.nFileOffset, op.lpstrFile.Length - op.nFileOffset).ToString();
                            try
                            {
                                File.Copy(op.lpstrFile.ToString(), @"Playlists/" + dest, true);
                                File.Delete(op.lpstrFile.ToString());
                            }
                            catch
                            {
                                // throw some error message     
                            }
                        }
                        Global.cki = new ConsoleKeyInfo();

                    }

                }
             


            if (nDialog != null)
            {
                nDialog.Draw();
                nDialog.Update();
            }
        }
        public void Cleanup()
        {
            pDialog = null;
        }
        private void Hold()
        {
            canUpdate = false;
        }
        private void Resume()
        {
            canUpdate = true;
            nDialog = null;
        }
        private void NotifyAboutPlaylistDialog()
        {

            nDialog = new NotifyDialog(5, 10, 160, 5, ref pBuffer, false, "test");
            nDialog.DialogCreated = Hold;
            nDialog.DialogCreated.Invoke();
            nDialog.DialogDestroyed += Resume;
            nDialog.Message = "\nOh no \\(・ ■ ・)/ !!!\n\nAn error occured while attempting to open your music.\nIf this problem persists, please try restarting Omniaudio.\n\n" + "Looks like: " + pDialog.Exception.Message;
        }
    }
}
