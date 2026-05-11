using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Newtonsoft.Json;
using Tpv.DTO;

namespace Tpv
{
    public partial class ZerbitzuakPantaila : Window
    {
        private const int DemoMahaiaId = 4;

        public ZerbitzuakPantaila()
        {
            InitializeComponent();
            _ = MahaiakKargatu();
        }

        private void Titulua_EzkerrekoBotoiaKlik(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void Atzera_Klik(object sender, RoutedEventArgs e)
        {
            var p = new Menu();
            p.Show();
            this.Close();
        }

        private void Minimizatu_Klik(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Itxi_Klik(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private async Task MahaiakKargatu()
        {
            gridMahaiak.Children.Clear();
            
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(ApiConfig.ApiBaseUrl + "/");
                    
                    HashSet<int> lunchReservations = new HashSet<int>();
                    HashSet<int> dinnerReservations = new HashSet<int>();
                    
                    var now = DateTime.Now;
                    string todayStr = now.ToString("yyyy-MM-dd");
                    
                    var resLunch = await client.GetAsync($"erreserbak?data={todayStr}&mota=true");
                    if (resLunch.IsSuccessStatusCode)
                    {
                        var jsonL = await resLunch.Content.ReadAsStringAsync();
                        var lunchList = JsonConvert.DeserializeObject<List<ErreserbakDto>>(jsonL);
                        if (lunchList != null) lunchReservations = new HashSet<int>(lunchList.Select(r => r.MahaiakId));
                    }
                    
                    var resDinner = await client.GetAsync($"erreserbak?data={todayStr}&mota=false");
                    if (resDinner.IsSuccessStatusCode)
                    {
                        var jsonD = await resDinner.Content.ReadAsStringAsync();
                        var dinnerList = JsonConvert.DeserializeObject<List<ErreserbakDto>>(jsonD);
                        if (dinnerList != null) dinnerReservations = new HashSet<int>(dinnerList.Select(r => r.MahaiakId));
                    }
                    
                    // Mahaiak 1-etik 6-ra bitartekoak dira
                    for (int i = 1; i <= 6; i++)
                    {
                        string mahaiaIzena = (i == 6) ? "Barra" : $"Mahaia {i}";
                        bool isOccupied = false;
                        
                        var response = await client.GetAsync($"zerbitzua/mahaia/{i}");
                        if (response.IsSuccessStatusCode)
                        {
                            var json = await response.Content.ReadAsStringAsync();
                            var zerrenda = JsonConvert.DeserializeObject<List<ZerbitzuaDto>>(json);
                            
                            
                            var azkenZerbitzua = zerrenda?.FirstOrDefault();
                            if (azkenZerbitzua != null && !azkenZerbitzua.Ordainduta)
                            {
                                bool forceClose = false;
                                if (i != 6 && i != DemoMahaiaId) // Barra eta demo mahaia ez dira ixten
                                {
                                    if (now.TimeOfDay >= new TimeSpan(16, 30, 0) && now.TimeOfDay < new TimeSpan(20, 0, 0))
                                    {
                                        forceClose = true;
                                    }
                                    else if (now.TimeOfDay >= new TimeSpan(23, 30, 0) || now.TimeOfDay < new TimeSpan(6, 0, 0))
                                    {
                                        forceClose = true;
                                    }
                                }

                                if (forceClose)
                                {
                                    // Behartutako itxiera API bidez
                                    var ordainduRes = await client.PostAsync($"zerbitzua/{azkenZerbitzua.Id}/ordaindu", null);
                                    if (!ordainduRes.IsSuccessStatusCode)
                                    {
                                        isOccupied = true; // Ezin izan bada itxi, mantendu irekita
                                    }
                                }
                                else
                                {
                                    isOccupied = true;
                                }
                            }
                        }

                        bool isReserved = false;
                        bool isClosed = false;
                        
                        if (!isOccupied && i != 6 && i != DemoMahaiaId) // Barra eta demo mahaia ez dira blokeatzen
                        {
                            bool hasLunch = lunchReservations.Contains(i);
                            bool hasDinner = dinnerReservations.Contains(i);

                            if (now.TimeOfDay < new TimeSpan(13, 0, 0))
                            {
                                if (hasLunch) isReserved = true;
                                else isClosed = true;
                            }
                            else if (now.TimeOfDay >= new TimeSpan(16, 0, 0) && now.TimeOfDay < new TimeSpan(20, 0, 0))
                            {
                                if (hasDinner) isReserved = true;
                                else isClosed = true;
                            }
                            else if (now.TimeOfDay >= new TimeSpan(23, 0, 0))
                            {
                                isClosed = true;
                            }
                        }
                        
                        Button btn = new Button
                        {
                            Style = (Style)FindResource("MahaiaBotoia"),
                            Margin = new Thickness(15),
                            Tag = i
                        };

                        StackPanel sp = new StackPanel
                        {
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center
                        };

                        sp.Children.Add(new TextBlock
                        {
                            Text = mahaiaIzena,
                            Foreground = (Brush)FindResource("BrandIvory"),
                            FontSize = 28,
                            FontWeight = FontWeights.Bold,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            Margin = new Thickness(0, 0, 0, 10)
                        });

                        string statusText = isOccupied ? "Zabalik / Aktibo" : 
                                            (isReserved ? "Erreserbatuta / Itxaron" : 
                                            (isClosed ? "Itxita / Ordua Itxaron" : "Libre / Berria Sortu"));
                        
                        Brush statusColor = isOccupied ? Brushes.LightGreen : 
                                            (isReserved ? Brushes.LightCoral : 
                                            (isClosed ? Brushes.LightGray : (Brush)FindResource("BrandGold")));

                        sp.Children.Add(new TextBlock
                        {
                            Text = statusText,
                            Foreground = statusColor,
                            FontSize = 20,
                            FontWeight = FontWeights.SemiBold,
                            HorizontalAlignment = HorizontalAlignment.Center
                        });

                        btn.Content = sp;
                        
                        if (isOccupied)
                        {
                            btn.Background = new SolidColorBrush(Color.FromRgb(40, 60, 40));
                            btn.Click += Mahaia_Klik;
                        }
                        else if (isReserved)
                        {
                            btn.Background = new SolidColorBrush(Color.FromRgb(80, 40, 40));
                            btn.Click += (s, e) => MessageBox.Show("Mahaia erreserbatuta dago. Ordua iritsi arte ezin da zerbitzua hasi.", "Erreserbatuta", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        else if (isClosed)
                        {
                            btn.Background = new SolidColorBrush(Color.FromRgb(50, 50, 50));
                            btn.Click += (s, e) => MessageBox.Show("Zerbitzua itxita dago ordu honetan. Itxaron ireki arte (13:00 edo 20:00).", "Itxita", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            btn.Click += Mahaia_Klik;
                        }

                        gridMahaiak.Children.Add(btn);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Errorea mahaiak kargatzean: " + ex.Message);
            }
        }

        private void Mahaia_Klik(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int mahaiId)
            {
                var eskaerak = new EskaerakPantaila(mahaiId);
                eskaerak.Show();
                this.Close();
            }
        }
    }
}
