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
    public partial class SaioaItxiDialog : Window
    {
        public bool BaiAukeratuta { get; private set; } = false;
        public SaioaItxiDialog()
        {
            InitializeComponent();
        }

        private void Bai_Klik(object sender, RoutedEventArgs e)
        {
            BaiAukeratuta = true;
            this.DialogResult = true;
        }

        private void Ez_Klik(object sender, RoutedEventArgs e)
        {
            BaiAukeratuta = false;
            this.DialogResult = false;
        }

    }
}
