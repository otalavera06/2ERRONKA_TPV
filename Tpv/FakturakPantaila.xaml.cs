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
using Newtonsoft.Json.Linq;
using System.Linq;

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
            var eskaera = new ZerbitzuakPantaila();
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

        private async void FakturakPantaila_Loaded(object sender, RoutedEventArgs e)
        {
            await KargatuFakturak();
        }

        private async Task KargatuFakturak()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(ApiConfig.ApiBaseUrl + "/");
                var response = await client.GetAsync("fakturak");
                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Errorea fakturak kargatzean");
                    return;
                }

                var json = await response.Content.ReadAsStringAsync();
                var arr = JsonConvert.DeserializeObject<JArray>(json);
                var fakturak = new List<FakturaDto>();
                foreach (var it in arr ?? new JArray())
                {
                    decimal prezio = 0m;
                    var prezioToken = it["PrezioTotala"] ?? it["prezioTotala"] ?? it["prezio_totala"];
                    if (prezioToken != null)
                    {
                        if (prezioToken.Type == JTokenType.Float || prezioToken.Type == JTokenType.Integer)
                        {
                            prezio = prezioToken.Value<decimal>();
                        }
                        else
                        {
                            decimal.TryParse(prezioToken.ToString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out prezio);
                        }
                    }

                    var sortutaToken = it["Sortuta"] ?? it["sortuta"];
                    bool sortuta = false;
                    if (sortutaToken != null)
                    {
                        sortuta = sortutaToken.Type == JTokenType.Boolean
                            ? sortutaToken.Value<bool>()
                            : sortutaToken.Value<int>() != 0;
                    }

                    var faktura = new FakturaDto
                    {
                        Id = (it["Id"] ?? it["id"])?.Value<int>() ?? 0,
                        ZerbitzuaId = (it["ZerbitzuaId"] ?? it["zerbitzuaId"] ?? it["zerbitzua_id"])?.Value<int>() ?? 0,
                        PrezioTotala = prezio,
                        Sortuta = sortuta,
                        Path = (it["Path"] ?? it["path"])?.ToString() ?? string.Empty
                    };

                    
                    var zerbitzuaResponse = await client.GetAsync($"zerbitzua/{faktura.ZerbitzuaId}");
                    if (zerbitzuaResponse.IsSuccessStatusCode)
                    {
                        var zerbitzuaJson = await zerbitzuaResponse.Content.ReadAsStringAsync();
                        var zerbitzua = JsonConvert.DeserializeObject<DTO.ZerbitzuaDto>(zerbitzuaJson);
                        faktura.Data = zerbitzua.Data.ToString("yyyy-MM-dd HH:mm");
                        faktura.MahaiaIzena = $"Mahaia {zerbitzua.MahaiakId}"; 
                        faktura.EskaeraXehetasunak = string.Join(", ", zerbitzua.Eskaerak.Select(e => e.Izena));
                    }

                    fakturak.Add(faktura);
                }
                dgFakturak.ItemsSource = fakturak;
            }
        }

        private async void BtnIkusiFaktura_Klik(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is FakturaDto faktura)
            {
                var url = !string.IsNullOrWhiteSpace(faktura.Path)
                    ? $"{ApiConfig.BaseUrl}{faktura.Path}"
                    : $"{ApiConfig.ApiBaseUrl}/fakturak/{faktura.Id}/pdf";

                try
                {
                    await Task.Run(() => 
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = url,
                            UseShellExecute = true
                        });
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
