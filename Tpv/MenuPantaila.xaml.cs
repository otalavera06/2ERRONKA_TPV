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
    public partial class Menu : Window
    {
        public Menu()
        {
            InitializeComponent();
        }

        private void Titulua_EzkerrekoBotoiaKlik(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void Minimizatu_Klik(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Itxi_Klik(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Eskaerak_Klik(object sender, RoutedEventArgs e)
        {
            this.Hide();
            var eskaera = new EskaerakPantaila();
            eskaera.Show();
        }
        private void Fakturak_Klik(object sender, RoutedEventArgs e)
        {
            this.Hide();
            var faktura = new FakturakPantaila();
            faktura.Show();
        }
        private void Erreserbak_Klik(object sender, RoutedEventArgs e)
        {
            this.Hide();
            var erreserba = new ErreserbakPantaila();
            erreserba.Show();
        }

        private void Profil_Klik(object sender, RoutedEventArgs e)
        {
            var aukera = new SaioaItxiDialog { Owner = this };
            bool? result = aukera.ShowDialog();

            if (result == true && aukera.BaiAukeratuta)
            {
                var onarpena = new Mezuak { Owner = this };
                onarpena.ShowDialog();
                this.Hide();
                var login = new LoginPantaila();
                login.Show();
            }
        }

        private void Txat_Klik(object sender, RoutedEventArgs e)
        {
            TxatLeihoa txat = new TxatLeihoa();
            txat.Show();
        }

    }
}




