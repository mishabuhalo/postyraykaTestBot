using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramConsoleTestBot
{
    class WashMachineData
    {
        public Parameters param { get; set; }
        public Dictionary<int, WashMachine> wash_machine { get; set; } 
    }
}
