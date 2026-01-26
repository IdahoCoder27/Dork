using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dork.Engine.Game
{
    public interface ISaveService
    {
        void Write(SaveGame save);
        SaveGame Read();
        bool Exists();
    }
}
