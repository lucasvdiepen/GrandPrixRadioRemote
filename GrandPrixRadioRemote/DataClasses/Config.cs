using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrandPrixRadioRemote.DataClasses
{
    public class Config
    {
        public int Port { get; private set; }

        public Config(int port)
        {
            this.Port = port;
        }
    }
}
