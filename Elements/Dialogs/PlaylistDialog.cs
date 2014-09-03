using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Omniaudio.Core;
using Omniaudio.Helpers;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;

namespace Omniaudio.Elements.Dialogs
{
    delegate void PlaylistDialogException();

    sealed class PlaylistDialog : Dialog
    {

        private List<string> pieces; // of awesome musics for sharing
        private List<string> nonDisplayPieces;
        private List<string> hidden;
        private int tY;
        private int tX;
        private int pickIndex;
        private bool _isSelected;
        private Exception we;
        
        private string directory;
        private string previousDirectory;


        private int hiX, hiY;
        #region Events/Actions
        public event PlaylistDialogException PlaylistDialogException;
        public Action<string> onSelect;
        #endregion
        #region Overriden Methods
        public PlaylistDialog(int xPos, int yPos, int width, int height, ref CHAR_INFO[,] rBuffer, bool isStatic): base(xPos, yPos, width, height, ref rBuffer,isStatic)
        {
            this._x = xPos;
            this._y = yPos;
            this._w = width;
            this._h = height;
            this.drawBuffer = rBuffer;
            this._isStatic = isStatic;
            this.selectorY = this._y;
            this.selectorX = _x;
            this.tY = this._y;
            this.tX = this._x;
            hiX = this._x;
            hiY = this._y;
            pickIndex = 0;
            this.pieces = new List<string>();
            this._isSelected = true;
            this.previousDirectory = Environment.CurrentDirectory + "\\Playlists\\";
            this.directory = Environment.CurrentDirectory + "\\Playlists\\";

                try
                {
                    
                    this.pieces = Directory.EnumerateFiles(Environment.CurrentDirectory + @"\Playlists", "*.mp3", SearchOption.TopDirectoryOnly).Select(Path.GetFileName).ToList<string>();
                    this.pieces.AddRange(Directory.GetDirectories(Environment.CurrentDirectory + @"\Playlists").ToList<string>());
                    nonDisplayPieces = pieces.ToList();

                    for (int i = 0; i < pieces.Count(); i++)
                    {
                        Debug.WriteLine(pieces[i]);
                        pieces[i] = LimitChar(pieces[i], 25);
                    }

                }
                catch(Exception e)
                {
                    //64
                    throw e;
                }

                this.hidden = new List<string>();

            }

        public sealed override void Update()
        {

            if (_isSelected)
            {
                if (pieces.Count > (_h / 2))
                {
                    if (Global.cki.Key == ConsoleKey.DownArrow)
                    {
                        selectorY += 1;
                        pickIndex += 1 + selectorY / (_h + 1);
                        Debug.WriteLine(pickIndex);
                        Global.cki = new ConsoleKeyInfo();
                        hidden.Add(pieces.First<string>());
                        pieces.Remove(pieces.First<string>());
                    }
                }

                if (hidden.Count != 0)
                {
                    if (Global.cki.Key == ConsoleKey.UpArrow)
                    {
                        Debug.WriteLine(pickIndex);
                        selectorY -= 1;
                        pickIndex -= 1 + selectorY / (_h + 1);
                        Global.cki = new ConsoleKeyInfo();
                        pieces.Insert(0, hidden.Last<string>());
                        hidden.Remove(hidden.Last<string>());
                    }
                }

                if (Global.cki.Key == ConsoleKey.LeftArrow)
                {
                    Refresh(previousDirectory);
                    Global.cki = new ConsoleKeyInfo(); // in retrospect maybe not initalising was a bad idea. /0 was robably a better idea. 
                }
             

                if (Global.cki.Key == ConsoleKey.W)
                {
                    if (hiY > _y)
                    {
                        Debug.WriteLine(pickIndex);
                        hiY -= 2;
                        pickIndex -= 1 - selectorY / (_h + 2);
                    }
                    Global.cki = new ConsoleKeyInfo();
                }

                if (Global.cki.Key == ConsoleKey.S)
                {
                    Debug.WriteLine(pickIndex);
                    if (hiY < (_y + (pieces.Count * 2)) - 2 && hiY + 2 < _y + _h - 2)
                    {
                        hiY += 2;
                        pickIndex += 1 - selectorY / (_h + 1);
                    }

                    Global.cki = new ConsoleKeyInfo();
                }

                if (Global.cki.Key == ConsoleKey.Enter)
                {
                    // only open up .mp3's because the dialog handles folders 
                    if (nonDisplayPieces[pickIndex].Contains(".mp3"))
                    {
                        try
                        {
                            if (onSelect == null)
                                Process.Start( directory + nonDisplayPieces[pickIndex]);
                            else
                                onSelect(nonDisplayPieces[pickIndex]);
                        }
                        catch (Exception e)
                        {
                            if (PlaylistDialogException != null)
                            {
                                we = e;
                                PlaylistDialogException();

                            }
                            else
                            {
                                throw new ArgumentNullException("There must be a method subscribed to tell the user wtf just happened");
                            }
                        }
                    }
                    else
                    {
                        Refresh(nonDisplayPieces[pickIndex]);
                    }
                    Global.cki = new ConsoleKeyInfo();
                }
            }
            }
        public Exception Exception
        {
            get{return we;}
        }
        public bool isSelected
        {
            get { return _isSelected; }
            set { _isSelected = value; }
        }

        public sealed override void Draw()
        {
            if (!hasBeenDrawn)
            {
                int entry = 0;

                if(_isSelected)
                ConsoleHelper.DrawRectangle(_x, _y, _w, _h, ref drawBuffer, 0x0001);
                else
                ConsoleHelper.DrawRectangle(_x, _y, _w, _h, ref drawBuffer, 0x0004);
                ConsoleHelper.DrawRectangle(hiX, hiY, _w, 1, ref drawBuffer, 0x0A | 0x0E);
                ConsoleHelper.DrawRectangle(selectorX + (_w - 2), selectorY, 2, 1, ref drawBuffer, 0x0001 | 0x0002);
                foreach (string musicPiece in pieces)
                {
                    if (!musicPiece.Contains(@".mp3"))
                        tX += _w - 30 - musicPiece.Length;

                    if(entry % 2 == 0)
                        ConsoleHelper.WriteLineInBuffer(new COORD((short)tX, (short)tY), musicPiece, ref drawBuffer, 0x0008 | 0x0001 | 0x0002);
                    else
                        ConsoleHelper.WriteLineInBuffer(new COORD((short)tX, (short)tY), musicPiece, ref drawBuffer, 0x0A | 0x0C);
                    tY += 2;

                    tX = _x;
                    entry += 1;
                    if (entry > (_h / 2) - 1)
                        break;
                }
                
                tY = _y;
                
            }
            if (_isStatic)
            {
                hasBeenDrawn = true;
            }
        }
        #endregion
        public void Refresh(string directory)
        {
            this.directory =  directory + "\\";
            this.nonDisplayPieces.Clear();
            this.pieces.Clear();
            this.hidden.Clear();
            hiY = this._y;
            selectorY = this._y;
            pickIndex = 0;
            // reset some GUI elements integral to index acess
            this.nonDisplayPieces = Directory.EnumerateFiles(directory, "*.mp3", SearchOption.TopDirectoryOnly).Select(Path.GetFileName).ToList<string>();
            this.nonDisplayPieces.AddRange(Directory.GetDirectories(directory).ToList<string>());

            // iterate through the enumeration and produce relative ilenames

            for (int i = 0; i < nonDisplayPieces.Count(); i++)
            {
                pieces.Add(LimitChar(nonDisplayPieces[i], 25));
            }
        }
        private string LimitChar(string str, int charLimit = 3)
        {
            if (str.Length > charLimit && !str.Contains("C:\\"))
            {
                str += "(...).mp3";
                str = str.Substring(0, charLimit);
                str += "(...).mp3";
            }
            else {

                if (str.LastIndexOf(@"\") != -1)
                {
                    str = str.Substring(str.LastIndexOf(@"\"));
                    if (str.Length > charLimit)
                    {
                        str = str.Remove(charLimit);
                        str += "...";   
                    }
                 }

             }
            // do nothing otherwise
            return str;
        }

       
    }
}
