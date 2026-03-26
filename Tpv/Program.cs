using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tpv
{
    internal static class Program
    {
       
        [STAThread]
        static void Main()
        {
            var app = new App();
            var login = new LoginPantaila();
            app.Run(login);
        }
    }
}
