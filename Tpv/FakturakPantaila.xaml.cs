using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Tpv.DTO;

namespace Tpv
{
    public partial class FakturakPantaila : Window
    {
        public FakturakPantaila()
        {
            InitializeComponent();
            Loaded += FakturakPantaila_Loaded;
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
            MessageBox.Show("Erreserben pantaila");
        }

        private void Profila_Klik(object sender, RoutedEventArgs e)
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

        private void Atzera_Klik(object sender, RoutedEventArgs e)
        {
            this.Hide();
            var menu = new Menu();
            menu.Show();
        }

        private void Txat_Klik(object sender, RoutedEventArgs e)
        {
            TxatLeihoa txat = new TxatLeihoa();
            txat.Show();
        }

        private async void FakturakPantaila_Loaded(object sender, RoutedEventArgs e)
        {
            await KargatuFakturak();
        }

        private async Task KargatuFakturak()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:5005/api/");
                var response = await client.GetAsync("fakturak");
                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Errorea fakturak kargatzean");
                    return;
                }

                var json = await response.Content.ReadAsStringAsync();
                var fakturak = JsonConvert.DeserializeObject<List<FakturaDto>>(json);
                dgFakturak.ItemsSource = fakturak;
            }
        }

        private async void BtnIkusiFaktura_Klik(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is FakturaDto faktura)
            {
                var url = $"http://localhost:5005/api/fakturak/{faktura.Id}/pdf";

                try
                {
                    using (var client = new HttpClient())
                    {
                        var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                        if (!response.IsSuccessStatusCode)
                        {
                            MessageBox.Show($"Errorea: {response.StatusCode}", "Errorea", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Errorea faktura irekitzean: {ex.Message}", "Errorea", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

    }
}
