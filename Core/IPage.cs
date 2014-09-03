using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Omniaudio
{
    public interface IPage
    {
       void Init();
       void Update();
       void Cleanup();
    }
}
