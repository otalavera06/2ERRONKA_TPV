using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Tpv.Modeloak;
using NHibernate;
using System.Linq;
using System.Windows.Media.Imaging;
using Microsoft.Win32.SafeHandles;
using System.Threading.Tasks;
using System.Net;
using Tpv.DTO;
using System.Linq.Expressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Windows.Data;
using System.Globalization;
using System.Configuration;

namespace Tpv
{
    public partial class EskaerakPantaila : Window
    {
        private decimal? _odooDeskontatutakoTotala;
        private string _odooDeskontuIzena = string.Empty;

        public EskaerakPantaila()
        {
            InitializeComponent();
            EguneratuEskaeraLaburpena();
            _ = EdariakKargatu();
            _ = AzkenekoEskaerakKargatu();
        }

        private readonly HttpClient _httpClient = new HttpClient()
        {
            BaseAddress = new Uri(ApiConfig.BaseUrl + "/")
        };
        private readonly string _odooBaseUrl = ConfigurationManager.AppSettings["OdooBaseUrl"] ?? "http://localhost:8069";

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

        private List<ProduktuaDto> _produktuak = new List<ProduktuaDto>();
        private async Task EdariakKargatu()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(ApiConfig.ApiBaseUrl + "/");
                    var response = await client.GetAsync("produktuak");
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        _produktuak = JsonConvert.DeserializeObject<List<ProduktuaDto>>(json);
                        
                        gridEdariak.Children.Clear();
                        KargatuProduktuakUI();
                    }
                    else
                    {
                        MessageBox.Show("Errorea produktuak kargatzean: " + response.StatusCode);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Errorea APIarekin konektatzean: " + ex.Message);
            }
        }
        private void KargatuProduktuakUI()
        {
            foreach (var p in _produktuak)
            {
                var btn = new Button
                {
                    Width = 120,
                    Height = 120,
                    Margin = new Thickness(5),
                    Tag = p
                };
                var panel = new StackPanel();
                if (!string.IsNullOrEmpty(p.IrudiaPath))
                {
                    var path = p.IrudiaPath.Trim();
                    if (path.StartsWith("wwwroot/", StringComparison.OrdinalIgnoreCase))
                    {
                        path = path.Substring("wwwroot/".Length);
                    }
                    path = path.TrimStart('/');
                    if (!path.StartsWith("irudiak/", StringComparison.OrdinalIgnoreCase))
                    {
                        path = "irudiak/" + path;
                    }
                    var fullUri = new Uri(_httpClient.BaseAddress, path);
                    var img = new Image
                    {
                        Source = new BitmapImage(fullUri),
                        Height = 60
                    };
                    panel.Children.Add(img);
                }
                panel.Children.Add(new TextBlock
                {
                    Text = p.Izena,
                    TextAlignment = TextAlignment.Center
                });
                btn.Content = panel;
                btn.Click += ProduktuaKlik;

                gridEdariak.Children.Add(btn);
            }
        }

        private void ProduktuaKlik(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is ProduktuaDto p)
            {
                lsEskaerak.Items.Add(p);
                EguneratuEskaeraLaburpena(true);
            }
        }
        private void LehioaIrekita(object sender, RoutedEventArgs e)
        {
            lsEskaerak.DisplayMemberPath = "Izena";
        }

        private async void BtnEskaeraJaso_Klik(object sender, RoutedEventArgs e)
        {
            if (lsEskaerak.Items.Count == 0)
            {
                MessageBox.Show("Gutxienez produktu bat gehitu behar duzu.");
                return;
            }

            var zerbitzua = new ZerbitzuaSortuDto
            {
                Data = DateTime.Now,
                MahaiakId = 6,
                PrezioTotala = 0,
                Eskaerak = new List<EskaerakSortuDto>()
            };

            foreach (ProduktuaDto p in lsEskaerak.Items)
            {
                zerbitzua.Eskaerak.Add(new EskaerakSortuDto
                {
                    ProduktuaId = p.Id,
                    Izena = p.Izena,
                    Prezioa = p.Prezioa,
                    Data = DateTime.Now,
                    Egoera = 0
                });
            }

            zerbitzua.PrezioTotala = _odooDeskontatutakoTotala ?? KalkulatuUnekoTotala();
           
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(ApiConfig.ApiBaseUrl + "/");
                var json = JsonConvert.SerializeObject(zerbitzua);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync("zerbitzua", content);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Eskaera ondo gorde da!");
                    lsEskaerak.Items.Clear();
                    EguneratuEskaeraLaburpena(true);
                    await AzkenekoEskaerakKargatu();
                }
                else
                {
                    MessageBox.Show("Errorea eskaera gordetzean: " + response.StatusCode);
                }
            }
        }
        private void BtnKendu_Klik(object sender, RoutedEventArgs e)
            {
                if (lsEskaerak.SelectedItem != null)
                {
                    lsEskaerak.Items.Remove(lsEskaerak.SelectedItem);
                    EguneratuEskaeraLaburpena(true);
                }
            }

        private async void BtnOdooDeskontua_Klik(object sender, RoutedEventArgs e)
        {
            if (lsEskaerak.Items.Count == 0)
            {
                MessageBox.Show("Lehenengo eskaeran produktuak gehitu behar dituzu.");
                return;
            }

            var uneKoTotala = KalkulatuUnekoTotala();

            try
            {
                var deskontuak = await LortuOdooDeskontuakAsync();
                var deskontuAktiboa = deskontuak.FirstOrDefault(d => d.Aktibo);

                if (deskontuAktiboa == null)
                {
                    MessageBox.Show("Odoo-n ez dago deskontu aktiborik.");
                    return;
                }

                var emaitza = await AplikatuOdooDeskontuaAsync(deskontuAktiboa.Id, uneKoTotala);
                _odooDeskontatutakoTotala = emaitza.DeskontatutakoPrezioa;
                _odooDeskontuIzena = emaitza.Deskontua ?? string.Empty;
                txtOdooDeskontua.Text = $"Odoo ({emaitza.Deskontua}): {emaitza.PrezioOriginala:0.00}€ -> {emaitza.DeskontatutakoPrezioa:0.00}€";
                EguneratuEskaeraLaburpena(false);

                MessageBox.Show(
                    $"Odoo deskontua aplikatuta.\n\n" +
                    $"Deskontua: {emaitza.Deskontua}\n" +
                    $"Jatorrizko prezioa: {emaitza.PrezioOriginala:0.00}€\n" +
                    $"Azken prezioa: {emaitza.DeskontatutakoPrezioa:0.00}€");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Errorea Odoo APIarekin konektatzean: " + ex.Message);
            }
        }

        private async void BtnOrdaindu_Klik(object sender, RoutedEventArgs e)
        {
            if (lsAzkenEskaerak.SelectedItem is ZerbitzuaDto vm)
            {
                var eskaera = vm.Eskaerak?.FirstOrDefault();
                if (eskaera?.Egoera == 1)
                {
                    MessageBox.Show("Zerbitzu honen eskaerak jada ordainduta daude!");
                    return;
                }
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(ApiConfig.ApiBaseUrl + "/");

                    try
                    {
                        var ordainErantzuna = await client.PostAsync($"zerbitzua/{vm.Id}/ordaindu", null);
                        var ordainEdukia = await ordainErantzuna.Content.ReadAsStringAsync();

                        if (!ordainErantzuna.IsSuccessStatusCode)
                        {
                            MessageBox.Show($"Errorea zerbitzua ordaintzean: {ordainErantzuna.StatusCode}\n{ordainEdukia}");
                            return;
                        }
                        else
                        {
                            MessageBox.Show("Zerbitzua ordainduta");
                            await AzkenekoEskaerakKargatu();
                        }   
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Errorea prozesuan: " + ex.Message);
                    }
                }
            }            
            else
            {
                MessageBox.Show("Ez dago zerbitzurik aukeratuta.");
            }
            
        }

        private async Task AzkenekoEskaerakKargatu()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(ApiConfig.ApiBaseUrl + "/");
                    var response = await client.GetAsync("zerbitzua/mahaia/6");

                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var zerrenda = JsonConvert.DeserializeObject<List<ZerbitzuaDto>>(json);
                        
                        lsAzkenEskaerak.ItemsSource = zerrenda;
                    }
                    else
                    {
                        MessageBox.Show("Errorea azken eskaerak kargatzean.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Errorea APIarekin konektatzean: " + ex.Message);
            }
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

        private void LsAzkenEskaerak_DoubleKlik(object sender, MouseButtonEventArgs e)
        {
            if (lsAzkenEskaerak.SelectedItem != null)
            {
                string aukeratutakoEskaera = lsAzkenEskaerak.SelectedItem.ToString();
                MessageBox.Show($"{aukeratutakoEskaera}");
            }
        }

        private decimal KalkulatuUnekoTotala()
        {
            return lsEskaerak.Items.OfType<ProduktuaDto>().Sum(p => p.Prezioa);
        }

        private void EguneratuEskaeraLaburpena(bool garbituOdooEmaitza = false)
        {
            var totala = KalkulatuUnekoTotala();
            var erakutsiTotala = _odooDeskontatutakoTotala ?? totala;
            txtUnekoTotala.Text = $"Guztira: {erakutsiTotala:0.00}€";

            if (garbituOdooEmaitza || totala == 0)
            {
                _odooDeskontatutakoTotala = null;
                _odooDeskontuIzena = string.Empty;
                txtOdooDeskontua.Text = "Odoo: ez dago deskonturik aplikatuta";
            }
            else if (_odooDeskontatutakoTotala.HasValue)
            {
                txtOdooDeskontua.Text = $"Odoo ({_odooDeskontuIzena}): {totala:0.00}€ -> {_odooDeskontatutakoTotala.Value:0.00}€";
            }
        }

        private HttpClient SortuOdooClient()
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri(_odooBaseUrl.TrimEnd('/') + "/")
            };
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }

        private async Task<List<OdooDeskontuaDto>> LortuOdooDeskontuakAsync()
        {
            using (var client = SortuOdooClient())
            {
                HttpResponseMessage response = await BidaliOdooJsonRpcAsync(client, "api/deskontuak", new { });
                var edukia = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException($"Odoo-k {response.StatusCode} erantzun du: {edukia}");
                }

                return DeserializatuOdooEdukia<List<OdooDeskontuaDto>>(edukia);
            }
        }

        private async Task<OdooDeskontuaEmaitzaDto> AplikatuOdooDeskontuaAsync(int deskontuaId, decimal prezioa)
        {
            using (var client = SortuOdooClient())
            {
                var response = await BidaliOdooJsonRpcAsync(client, "api/deskontuak/jarri", new
                {
                    deskontua_id = deskontuaId,
                    prezioa
                });
                var edukia = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException($"Odoo-k {response.StatusCode} erantzun du: {edukia}");
                }

                return DeserializatuOdooEdukia<OdooDeskontuaEmaitzaDto>(edukia);
            }
        }

        private async Task<HttpResponseMessage> BidaliOdooJsonRpcAsync(HttpClient client, string bidea, object parametroak)
        {
            var payload = new
            {
                jsonrpc = "2.0",
                method = "call",
                @params = parametroak,
                id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            return await client.PostAsync(bidea, content);
        }

        private T DeserializatuOdooEdukia<T>(string edukia)
        {
            if (string.IsNullOrWhiteSpace(edukia))
            {
                throw new InvalidOperationException("Odoo-k erantzun hutsa bidali du.");
            }

            var trimmed = edukia.TrimStart();
            if (trimmed.StartsWith("<!DOCTYPE html>", StringComparison.OrdinalIgnoreCase) ||
                trimmed.StartsWith("<html", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Odoo deskontuen API ez dago erabilgarri. Normalean honek esan nahi du Odoo martxan dagoela, baina " +
                    "'estatistikak' modulua ez dagoela kargatuta/eguneratuta edo Odoo berrabiarazi gabe dagoela.");
            }

            var token = JToken.Parse(edukia);

            if (token.Type == JTokenType.Object && token["error"] != null)
            {
                throw new InvalidOperationException(token["error"].ToString(Formatting.None));
            }

            if (token.Type == JTokenType.Object && token["result"] != null)
            {
                return token["result"].ToObject<T>();
            }

            return token.ToObject<T>();
        }

        private class OdooDeskontuaDto
        {
            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("name")]
            public string Izena { get; set; }

            [JsonProperty("aktibo")]
            public bool Aktibo { get; set; }
        }

        private class OdooDeskontuaEmaitzaDto
        {
            [JsonProperty("prezio_originala")]
            public decimal PrezioOriginala { get; set; }

            [JsonProperty("deskontatutako_prezioa")]
            public decimal DeskontatutakoPrezioa { get; set; }

            [JsonProperty("deskontua")]
            public string Deskontua { get; set; }
        }
    }
}
