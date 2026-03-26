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
    public partial class Mezuak : Window
    {
        public bool Onartuta { get; private set; } = false;
        public Mezuak()
        {
            InitializeComponent();
        }
        private void Onartu_Klik(object sender, RoutedEventArgs e)
        {
            Onartuta = true;
            this.DialogResult = true;
        }
    }
}
