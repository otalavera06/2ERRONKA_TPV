using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
using Tpv.DTO;

using Tpv.Services;

namespace Tpv
{
    public partial class ErreserbaEguneratuPantaila : Window
    {
        private readonly int _mahaiaId;
        private readonly DateTime _eguna;
        private bool _mota;
        private readonly bool _motaOriginala;
        private readonly ErreserbaService _erreserbaService;

        public ErreserbaEguneratuPantaila(int mahaiaId, DateTime eguna, bool mota)
        {
            InitializeComponent();
            _mahaiaId = mahaiaId;
            _eguna = eguna;
            _mota = mota;
            _motaOriginala = mota;
            _erreserbaService = new ErreserbaService();

            dataPicker.SelectedDate = _eguna;
            EguneratuBotoiItxura();
        }

        private void EguneratuBotoiItxura()
        {
            if (_mota)
            {
                Bazkaria_Btn.Background = Brushes.LightGreen;
                Afaria_Btn.ClearValue(Button.BackgroundProperty);
            }
            else
            {
                Afaria_Btn.Background = Brushes.LightGreen;
                Bazkaria_Btn.ClearValue(Button.BackgroundProperty);
            }
        }

        private void Bazkaria_Klik(object sender, RoutedEventArgs e)
        {
            _mota = true;
            EguneratuBotoiItxura();
        }

        private void Afaria_Klik(object sender, RoutedEventArgs e)
        {
            _mota = false;
            EguneratuBotoiItxura();
        }

        private async void Aldatu_Klik(object sender, RoutedEventArgs e)
        {
            var baieztapena = MessageBox.Show("Ziur zaude erreserba eguneratu nahi duzula?", "Berrespena", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (baieztapena != MessageBoxResult.Yes)
            {
                return;
            }

            var dataBerria = dataPicker.SelectedDate ?? _eguna;

            var dto = new ErreserbakSortuDto
            {
                Data = dataBerria,
                Mota = _mota,
                MahaiakId = _mahaiaId,
                ErabiltzaileakId = 1
            };

            try
            {
                using (var client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) })
                {
                    var url = $"http://localhost:5005/api/erreserbak/mahaia/{_mahaiaId}?data={_eguna:yyyy-MM-dd}&mota={_motaOriginala.ToString().ToLower()}";
                    var json = JsonConvert.SerializeObject(dto);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var erantzuna = await client.PutAsync(url, content);
                    if (erantzuna.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Erreserba eguneratuta.");
                        DialogResult = true;
                    }
                    else
                    {
                        var edukia = await erantzuna.Content.ReadAsStringAsync();
                        MessageBox.Show($"Errorea erreserba eguneratzean: {erantzuna.StatusCode}\n" + edukia);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Errorea zerbitzariarekin konektatzean: " + ex.Message);
            }
        }

        private async void Ezabatu_Klik(object sender, RoutedEventArgs e)
        {
            var baieztapena = MessageBox.Show("Ziur zaude erreserba ezabatu nahi duzula?", "Berrespena", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (baieztapena != MessageBoxResult.Yes)
            {
                return;
            }

            var url = $"http://localhost:5005/api/erreserbak/mahaia/{_mahaiaId}?data={_eguna:yyyy-MM-dd}&mota={_motaOriginala.ToString().ToLower()}";
            using (var client = new HttpClient())
            {
                var erantzuna = await client.DeleteAsync(url);
                if (erantzuna.IsSuccessStatusCode)
                {
                    MessageBox.Show("Erreserba ezabatuta.");
                    DialogResult = true;
                }
                else
                {
                    var edukia = await erantzuna.Content.ReadAsStringAsync();
                    MessageBox.Show("Errorea erreserba ezabatzean.\n" + edukia);
                }
            }
        }

        private void Itxi_Klik(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
