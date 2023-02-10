using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zeraniumu;

namespace AKBot.Scripts
{
    internal class CustomScript : Script
    {
        public CustomScript(IScript script) : base(script)
        {
        }

        public override void Stop()
        {
            base.Stop();
            (script as BotScript).StopEmulator();
        }
    }
}
