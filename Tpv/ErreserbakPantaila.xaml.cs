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

namespace Tpv
{
    public partial class ErreserbakPantaila : Window
    {
        private bool mota = true;
        public ErreserbakPantaila()
        {
            InitializeComponent();
            egutegia.SelectedDate = DateTime.Today;
            PantailaEguneratu();
        }

        private void Minimizatu_Klik(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        private void Titulua_EzkerrekoBotoiaKlik(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void Itxi_Klik(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
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

        private void Egutegia_aldatu(object sender, SelectionChangedEventArgs e)
        {
            PantailaEguneratu();
        }

        private void Bazkaria_Klik(object sender, RoutedEventArgs e)
        {
            mota = true;
            PantailaEguneratu();
        }

        private void Afaria_Klik(object sender, RoutedEventArgs e)
        {
            mota = false;
            PantailaEguneratu();
        }

        private async void PantailaEguneratu()
        {
            var eguna = egutegia.SelectedDate ?? DateTime.Today;

            using (var client = new HttpClient())
            {
                var url = $"{ApiConfig.ApiBaseUrl}/erreserbak?data={eguna:yyyy-MM-dd}&mota={mota.ToString().ToLower()}";

                var erantzuna = await client.GetAsync(url);
                var json = await erantzuna.Content.ReadAsStringAsync();

                var erreserbak = JsonConvert.DeserializeObject<List<ErreserbakDto>>(json);
                var okupatu = erreserbak.Select(r => r.MahaiakId).ToHashSet();

                MahaiakMargotu(okupatu);
            }
        }

        private void Txat_Klik(object sender, RoutedEventArgs e)
        {
            TxatLeihoa txat = new TxatLeihoa();
            txat.Show();
        }

        private string ToolTipMezua(DateTime eguna, bool mota)
        {
            if (eguna.Date < DateTime.Today)
            {
                return "Ezin da iraganeko egunetan erreserbatu!";
            }

            if (eguna.Date == DateTime.Today)
            {
                var orain = DateTime.Now.TimeOfDay;

                if (mota && orain > new TimeSpan(13, 0, 0))
                    return "Bazkarirako erreserbak 13:00ak arte egin daitezke.";

                if (!mota && orain > new TimeSpan(20, 0, 0))
                    return "Afariarako erreserbak 20:00ak arte egin daitezke.";
            }
            return null;
        }


        private bool ErreserbaBaimenduta(DateTime eguna, bool mota)
        {
            if (eguna.Date < DateTime.Today)
                return false;
            if (eguna.Date == DateTime.Today)
            {
                var orain = DateTime.Now.TimeOfDay;

                if (mota && orain > new TimeSpan(13, 0, 0))
                    return false;
                if (!mota && orain > new TimeSpan(20, 0, 0))
                    return false;
            }
            return true;
        }

        private void MahaiakMargotu(HashSet<int> okupatu)
        {
            var eguna = egutegia.SelectedDate ?? DateTime.Today;

            foreach (var child in MahaiaGrid.Children)
            {
                if (child is Button btn)
                {
                    int mahaiaId = int.Parse(btn.Name.Replace("Mahaia", ""));
                    bool okupatuta = okupatu.Contains(mahaiaId);

                    bool baimenduta = ErreserbaBaimenduta(eguna, mota);
                    string mezua = ToolTipMezua(eguna, mota);

                    if (!baimenduta)
                    {
                        btn.Background = Brushes.LightGray;
                        btn.Foreground = Brushes.DarkGray;
                        btn.IsEnabled = false;
                        btn.Tag = "Blokeatuta";
                        btn.ToolTip = mezua;
                    }
                    else
                    {
                        btn.Background = okupatuta ? Brushes.DarkRed : Brushes.LightGreen;
                        btn.Foreground = Brushes.Black;
                        btn.IsEnabled = true;
                        btn.Tag = okupatuta ? "Okupatuta" : "Aske";
                        btn.ToolTip = okupatuta ? "Mahai hau erreserbatuta dao." : null;
                    }
                }
            }
        }

        private async void Mahaia_Klik(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                int mahaiaId = int.Parse(btn.Name.Replace("Mahaia", ""));
                var eguna = egutegia.SelectedDate ?? DateTime.Today;
                bool okupatuta = btn.Tag?.ToString() == "Okupatuta";

                if (eguna.Date < DateTime.Today)
                {
                    MessageBox.Show("Ezin da iraganeko egunetan erreserbatu.");
                    return;
                }

                if (eguna.Date ==  DateTime.Today)
                {
                    var orain = DateTime.Now.TimeOfDay;
                    if (mota && orain > new TimeSpan(13, 0, 0))
                    {
                        MessageBox.Show("Bazkarirako erreserbak 13:00etatik aurrera ezin dira egin!");
                        return;
                    }
                    if (!mota && orain > new TimeSpan(20, 0, 0))
                    {
                        MessageBox.Show("Afarirako erreserbak 20:00etatik aurrera ezin dira egin!");
                        return;
                    }
                }

                if (okupatuta) {
                    EguneraketaPopUp(mahaiaId, eguna);
                    return;
                }
                var dto = new ErreserbakSortuDto
                {
                    Data = eguna,
                    Mota = mota,
                    MahaiakId = mahaiaId,
                    ErabiltzaileakId = null
                };

                using (var client = new HttpClient())
                {
                    var json = JsonConvert.SerializeObject(dto);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var erantzuna = await client.PostAsync($"{ApiConfig.ApiBaseUrl}/erreserbak", content);


                    var result = await erantzuna.Content.ReadAsStringAsync();

                    if (erantzuna.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Erreserba modu egokian sortu da.");
                        PantailaEguneratu();
                    }
                    else
                    {
                        MessageBox.Show("Errorea erreserba sortzean:\n" + result);
                    }
                }
                
            }
        }

        private void EguneraketaPopUp(int mahaiaId, DateTime eguna)
        {
            var popup = new ErreserbaEguneratuPantaila(mahaiaId, eguna, mota);
            popup.Owner = this;

            if (popup.ShowDialog() == true)
                PantailaEguneratu();
        }

    }
}
