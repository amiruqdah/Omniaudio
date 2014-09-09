using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Omniaudio
{
    /// <summary>
    /// A page encapsulates Dialogs, Elements, and other Misc Components. 
    /// It is the container in which all UI activity occurs
    /// </summary>
    public interface IPage
    {
        /// <summary>
        /// Handles initlization of buffer's, dialogs, creation of objects, etc
        /// </summary>
       void Init();
       /// <summary>
       /// Handles the drawing and update logic of initalized objects
       /// </summary>
       void Update();
        /// <summary>
        /// Cleans up objects once the page is popped from the stack
        /// </summary>
       void Cleanup();
    }
}
