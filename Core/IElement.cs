using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Omniaudio.Core
{
    /// <summary>
    ///  A generic component that is to be used with a number of dialogs. They can be thought of as extremely reusable and specialized components
    ///  that are to be used among multiple dialogs.
    /// </summary>

    interface IElement
    {
        /// <summary>
        /// The elements draw position relative to the X axis in the draw buffer
        /// </summary>
        int x
        {
            get;
            set;
        }
        /// <summary>
        /// The elements draw positions relative to the Y axis in the draw buffer
        /// </summary>
        int y
        {
            get;
            set;
        }

        /// <summary>
        ///  a dirty flag, indicating wether or not, we will update every interval tick or if we will only update once
        /// </summary>

        bool isStatic
        {
            get;
        }


        /// each Element must have the ability to draw itself as well as update its component logic
        void Update();
        void Draw();

    }
}
