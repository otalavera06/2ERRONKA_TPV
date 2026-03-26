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
using System.Text;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Windows.Data;
using System.Globalization;

namespace Tpv
{
    public partial class EskaerakPantaila : Window
    {
        public EskaerakPantaila()
        {
            InitializeComponent();
            _ = EdariakKargatu();
            _ = AzkenekoEskaerakKargatu();
        }

        private readonly HttpClient _httpClient = new HttpClient()
        {
            BaseAddress = new Uri("http://localhost:5005/")
        };

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
                    client.BaseAddress = new Uri("http://localhost:5005/api/");
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
            }
        }
        private void LehioaIrekita(object sender, RoutedEventArgs e)
        {
            lsEskaerak.DisplayMemberPath = "Izena";
        }

        private async void BtnEskaeraJaso_Klik(object sender, RoutedEventArgs e)
        {
            
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
                zerbitzua.PrezioTotala += p.Prezioa;
            }
           
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:5005/api/");
                var json = JsonConvert.SerializeObject(zerbitzua);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync("zerbitzua", content);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Eskaera ondo gorde da!");
                    lsEskaerak.Items.Clear();
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
                    lsEskaerak.Items.Remove(lsEskaerak.SelectedItem);
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
                    client.BaseAddress = new Uri("http://localhost:5005/api/");

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
                    client.BaseAddress = new Uri("http://localhost:5005/api/");
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
    }
}
