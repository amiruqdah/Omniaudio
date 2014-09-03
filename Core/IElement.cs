using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Omniaudio.Core
{
    interface IElement
    {
        int x
        {
            get;
            set;
        }

        int y
        {
            get;
            set;
        }

        bool isStatic
        {
            get;
        }

        void Update();
        void Draw();

    }
}
