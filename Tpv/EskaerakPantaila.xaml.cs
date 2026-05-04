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
        private int _mahaiId;
        private ZerbitzuaDto _editatzenDenZerbitzua;
        private readonly Dictionary<int, int> _editatzenHasierakoKantitateak = new Dictionary<int, int>();

        public EskaerakPantaila(int mahaiId)
        {
            InitializeComponent();
            _mahaiId = mahaiId;
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
                _produktuak.Clear();
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(ApiConfig.ApiBaseUrl + "/");
                    var response = await client.GetAsync("produktuak");
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var edariak = JsonConvert.DeserializeObject<List<ProduktuaDto>>(json);
                        _produktuak.AddRange(edariak.Where(p => p.ProduktuenMotakId != 8 && p.Stock > 0));
                    }
                    else
                    {
                        MessageBox.Show("Errorea produktuak kargatzean: " + response.StatusCode);
                    }

                    var responsePlaterak = await client.GetAsync("platerak");
                    if (responsePlaterak.IsSuccessStatusCode)
                    {
                        var json = await responsePlaterak.Content.ReadAsStringAsync();
                        var erantzuna = JsonConvert.DeserializeObject<ErantzunaDTO<List<PlateraDTO>>>(json);
                        if (erantzuna?.Datuak != null)
                        {
                            foreach (var pl in erantzuna.Datuak)
                            {
                                _produktuak.Add(new ProduktuaDto
                                {
                                    Id = pl.Id,
                                    Izena = pl.Izena,
                                    Prezioa = pl.Prezioa,
                                    IrudiaPath = pl.ArgazkiaUrl,
                                    Stock = 999, // Platerak always available unless OSAGAIAK is out, but TPV can handle it
                                    IsPlatera = true
                                });
                            }
                        }
                    }

                    gridProduktuak.Children.Clear();
                    gridPlaterak.Children.Clear();
                    KargatuProduktuakUI();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Errorea APIarekin konektatzean: " + ex.Message);
            }
        }
        private void KargatuProduktuakUI()
        {
            var produktuErakutsigarriak = _produktuak.OrderBy(p => p.Izena).ToList();

            foreach (var p in produktuErakutsigarriak)
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
                    var fullUri = SortuIrudiUri(p.IrudiaPath);
                    var img = new Image
                    {
                        Source = new BitmapImage(fullUri),
                        Height = 60,
                        Width = 96,
                        Stretch = System.Windows.Media.Stretch.UniformToFill
                    };
                    panel.Children.Add(img);
                }
                panel.Children.Add(new TextBlock
                {
                    Text = p.Izena,
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap
                });
                btn.Content = panel;
                btn.Click += ProduktuaKlik;

                if (p.IsPlatera)
                {
                    gridPlaterak.Children.Add(btn);
                }
                else
                {
                    gridProduktuak.Children.Add(btn);
                }
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

            var hautatutakoProduktuak = lsEskaerak.Items.OfType<ProduktuaDto>().ToList();
            var stockFalta = hautatutakoProduktuak
                .GroupBy(p => p.Id)
                .Select(g => new
                {
                    Produktua = g.First(),
                    EskatutakoKantitatea = g.Count(),
                    StockErabilgarria = g.First().Stock + LortuEditatzenHasierakoKantitatea(g.Key)
                })
                .FirstOrDefault(x => !x.Produktua.IsPlatera && x.EskatutakoKantitatea > x.StockErabilgarria);

            if (stockFalta != null)
            {
                MessageBox.Show(
                    $"Ez dago stock nahikorik produktu honentzat: {stockFalta.Produktua.Izena}. " +
                    $"Eskatuta: {stockFalta.EskatutakoKantitatea}, stock erabilgarria: {stockFalta.StockErabilgarria}");
                return;
            }

            var zerbitzua = EraikiZerbitzuaSortuDto();

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(ApiConfig.ApiBaseUrl + "/");
                var json = JsonConvert.SerializeObject(zerbitzua);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response;

                if (_editatzenDenZerbitzua != null)
                {
                    response = await client.PutAsync($"zerbitzua/{_editatzenDenZerbitzua.Id}", content);
                }
                else
                {
                    response = await client.PostAsync("zerbitzua", content);
                }

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show(_editatzenDenZerbitzua != null ? "Eskaera ondo eguneratu da!" : "Eskaera ondo gorde da!");
                    GarbituUnekoEskaera();
                    await AzkenekoEskaerakKargatu();
                    _produktuak.Clear();
                    await EdariakKargatu();
                }
                else
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    var erroreMezua = string.IsNullOrWhiteSpace(errorBody) ? response.StatusCode.ToString() : errorBody;
                    MessageBox.Show("Errorea eskaera gordetzean: " + erroreMezua);
                }
            }
        }

        private ZerbitzuaSortuDto EraikiZerbitzuaSortuDto()
        {
            var zerbitzua = new ZerbitzuaSortuDto
            {
                Data = DateTime.Now,
                MahaiakId = _mahaiId,
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
                    Egoera = 0,
                    IsPlatera = p.IsPlatera
                });
            }

            zerbitzua.PrezioTotala = _odooDeskontatutakoTotala ?? KalkulatuUnekoTotala();

            return zerbitzua;
        }

        private void GarbituUnekoEskaera()
        {
            _editatzenDenZerbitzua = null;
            _editatzenHasierakoKantitateak.Clear();
            lsEskaerak.Items.Clear();
            EguneratuEditatzenEgoera();
            EguneratuEskaeraLaburpena(true);
        }
        private void BtnKendu_Klik(object sender, RoutedEventArgs e)
            {
                if (lsEskaerak.SelectedItem != null)
                {
                    lsEskaerak.Items.Remove(lsEskaerak.SelectedItem);
                    EguneratuEskaeraLaburpena(true);
                }
            }

        private void BtnGehituAzkenetik_Klik(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is DTO.EskaeraDto eskaera)
            {
                var produktua = lsEskaerak.Items
                    .OfType<ProduktuaDto>()
                    .FirstOrDefault(p => p.Id == eskaera.ProduktuaId);

                if (produktua == null)
                {
                    MessageBox.Show("Produktu hori ez dago momentuko eskaeran.");
                    return;
                }

                lsEskaerak.Items.Remove(produktua);
                EguneratuEskaeraLaburpena(true);
            }
        }

        private void BtnKenduProduktua_Klik(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is ProduktuaDto produktua)
            {
                lsEskaerak.Items.Remove(produktua);
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

            var leihoa = new OdooDeskontuaLeihoa(txtDeskontuKodea.Text) { Owner = this };
            if (leihoa.ShowDialog() != true)
            {
                return;
            }

            var kodea = leihoa.Kodea;
            txtDeskontuKodea.Text = kodea;
            if (string.IsNullOrWhiteSpace(kodea))
            {
                MessageBox.Show("Deskontu kodea idatzi behar duzu.");
                return;
            }

            var uneKoTotala = KalkulatuUnekoTotala();

            try
            {
                var deskontua = await CheckOdooDeskontuaAsync(kodea);
                var deskontatutakoPrezioa = uneKoTotala * (1 - (deskontua.Ehunekoa / 100));

                _odooDeskontatutakoTotala = deskontatutakoPrezioa;
                _odooDeskontuIzena = $"{deskontua.Kodea} ({deskontua.Ehunekoa:0.##}%)";
                txtOdooDeskontua.Text = $"Odoo ({_odooDeskontuIzena}): {uneKoTotala:0.00}€ -> {deskontatutakoPrezioa:0.00}€";
                EguneratuEskaeraLaburpena(false);

                MessageBox.Show(
                    $"Odoo deskontua aplikatuta.\n\n" +
                    $"Kodea: {deskontua.Kodea}\n" +
                    $"Deskontua: {deskontua.Ehunekoa:0.##}%\n" +
                    $"Jatorrizko prezioa: {uneKoTotala:0.00}€\n" +
                    $"Azken prezioa: {deskontatutakoPrezioa:0.00}€");
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
                    var response = await client.GetAsync($"zerbitzua/mahaia/{_mahaiId}");

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

        private void LsAzkenEskaerak_DoubleKlik(object sender, MouseButtonEventArgs e)
        {
            if (lsAzkenEskaerak.SelectedItem is DTO.ZerbitzuaDto zerbitzua)
            {
                if (zerbitzua.Ordainduta || zerbitzua.Eskaerak.Any(eskaera => eskaera.Egoera == 1))
                {
                    MessageBox.Show("Zerbitzu hau jada ordainduta dago; ezin da editatu.");
                    return;
                }

                _editatzenDenZerbitzua = zerbitzua;
                _editatzenHasierakoKantitateak.Clear();
                foreach (var taldea in zerbitzua.Eskaerak.GroupBy(eskaera => eskaera.ProduktuaId))
                {
                    _editatzenHasierakoKantitateak[taldea.Key] = taldea.Count();
                }

                lsEskaerak.Items.Clear();

                foreach (var eskaera in zerbitzua.Eskaerak)
                {
                    var produktua = SortuProduktuaEskaeratik(eskaera);
                    lsEskaerak.Items.Add(produktua);
                }
                EguneratuEditatzenEgoera();
                EguneratuEskaeraLaburpena(true);
            }
        }

        private ProduktuaDto SortuProduktuaEskaeratik(EskaeraDto eskaera)
        {
            var katalogokoProduktua = _produktuak.FirstOrDefault(p => p.Id == eskaera.ProduktuaId);
            if (katalogokoProduktua != null)
            {
                return katalogokoProduktua;
            }

            return new ProduktuaDto
            {
                Id = eskaera.ProduktuaId,
                Izena = eskaera.Izena,
                Prezioa = eskaera.Prezioa,
                Stock = LortuEditatzenHasierakoKantitatea(eskaera.ProduktuaId)
            };
        }

        private int LortuEditatzenHasierakoKantitatea(int produktuaId)
        {
            int kantitatea;
            return _editatzenHasierakoKantitateak.TryGetValue(produktuaId, out kantitatea) ? kantitatea : 0;
        }

        private void EguneratuEditatzenEgoera()
        {
            var editatzen = _editatzenDenZerbitzua != null;
            editatzenBanner.Visibility = editatzen ? Visibility.Visible : Visibility.Collapsed;
            txtEditatzenInfo.Text = editatzen
                ? $"#{_editatzenDenZerbitzua.Id} zerbitzua"
                : string.Empty;
            txtEskaeraJasoBotoia.Text = editatzen ? "Eguneratu eskaera" : "Eskaera jaso";
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

        private Uri SortuIrudiUri(string irudiaPath)
        {
            var path = irudiaPath.Trim();
            if (Uri.TryCreate(path, UriKind.Absolute, out var absoluteUri))
            {
                return absoluteUri;
            }

            if (path.StartsWith("wwwroot/", StringComparison.OrdinalIgnoreCase))
            {
                path = path.Substring("wwwroot/".Length);
            }

            path = path.TrimStart('/');
            if (!path.StartsWith("irudiak/", StringComparison.OrdinalIgnoreCase))
            {
                path = "irudiak/" + path;
            }

            return new Uri(_httpClient.BaseAddress, path);
        }

        private async Task<OdooDeskontuaKodeaDto> CheckOdooDeskontuaAsync(string kodea)
        {
            using (var client = SortuOdooClient())
            {
                var parametroak = new
                {
                    code = kodea
                };

                var bideak = new[] { "api/check_discount", "api/get_discount" };
                string azkenErrorea = null;

                foreach (var bidea in bideak)
                {
                    var response = await BidaliOdooJsonRpcAsync(client, bidea, parametroak);
                    var edukia = await response.Content.ReadAsStringAsync();

                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        azkenErrorea = $"Odoo-k ez du {bidea} ruta aurkitu.";
                        continue;
                    }

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new InvalidOperationException($"Odoo-k {response.StatusCode} erantzun du: {edukia}");
                    }

                    var emaitza = DeserializatuOdooEdukia<OdooDeskontuaKodeaDto>(edukia);
                    if (!string.Equals(emaitza.Status, "success", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException(emaitza.Message ?? "Deskontu kodea ez da baliozkoa.");
                    }

                    return emaitza;
                }

                throw new InvalidOperationException(
                    (azkenErrorea ?? "Odoo deskontuen API ez dago erabilgarri.") +
                    " Egiaztatu 'estatistikak' modulua instalatuta/eguneratuta dagoela eta Odoo berrabiarazita dagoela.");
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

        private class OdooDeskontuaKodeaDto
        {
            [JsonProperty("status")]
            public string Status { get; set; }

            [JsonProperty("message")]
            public string Message { get; set; }

            [JsonProperty("code")]
            public string Kodea { get; set; }

            [JsonProperty("percentage")]
            public decimal Ehunekoa { get; set; }
        }
    }
}
