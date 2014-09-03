using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Omniaudio.Networking
{
   public enum MessageType : byte
   {
       /*CHAT DIALOG MESSAGE TYPES*/
        ChatDialog_Add,   
        ChatDialog_Broadcast,
        /*CLIENT DIALOG MESSAGE TYPES*/
        ClientDialog_Add, // indicates a client has been added to the dialog
        ClientDialog_Init, // indicates that a client has connected and has been given the current dialog dataset
        ClientDialog_Remove, // indicates that a client has been removed from the dialog
        Audio_AlbumInformation,
        Audio_ByteData,
        Audio_OnRecieved,
       /*SERVER SIDE COMMANDS*/
       Command_Blacklist, // blacklists a client, and makes sure that they aren't coming back(blacklist.txt)
       Command_Censor // censors a client from communicating in chat
       //Command_Promote -- *UNIMPLEMENTED* should promote a user to [ADMIN]
       
    }
     
}
