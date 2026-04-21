using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Tpv
{
    public partial class App : Application
    {
        public App()
        {
            System.Windows.Application.LoadComponent(this, new System.Uri("App.xaml", System.UriKind.Relative));
        }
    }
}
