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
            var e_pantaila = new ZerbitzuakPantaila();
            e_pantaila.Show();
            this.Hide();
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

        private async void Txat_Klik(object sender, RoutedEventArgs e)
        {
            if (SaioaInfo.UnekoErabiltzailea != null)
            {
                try
                {
                    using (var client = new System.Net.Http.HttpClient())
                    {
                        var response = await client.GetAsync($"{ApiConfig.ApiBaseUrl}/langileak/{SaioaInfo.UnekoErabiltzailea.Id}/txat-baimena");
                        if (response.IsSuccessStatusCode)
                        {
                            var jsonStr = await response.Content.ReadAsStringAsync();
                            dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonStr);
                            bool chatBaimena = jsonObj.chatBaimena;
                            if (!chatBaimena)
                            {
                                MessageBox.Show("Langile honek ez dauka txata erabiltzeko baimenik.", "Baimena ukatuta", MessageBoxButton.OK, MessageBoxImage.Warning);
                                return;
                            }
                        }
                        else if (!SaioaInfo.UnekoErabiltzailea.chatBaimena)
                        {
                            MessageBox.Show("Langile honek ez dauka txata erabiltzeko baimenik.", "Baimena ukatuta", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                    }
                }
                catch
                {
                    if (!SaioaInfo.UnekoErabiltzailea.chatBaimena)
                    {
                        MessageBox.Show("Langile honek ez dauka txata erabiltzeko baimenik.", "Baimena ukatuta", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }
            }

            TxatLeihoa txat = new TxatLeihoa();
            txat.Show();
        }

        private void Eguraldia_Klik(object sender, RoutedEventArgs e)
        {
            EguraldiaLeihoa eguraldia = new EguraldiaLeihoa();
            eguraldia.Show();
        }

    }
}




