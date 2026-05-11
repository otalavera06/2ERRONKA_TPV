using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Xml;

namespace Tpv
{
    public partial class EguraldiaLeihoa : Window
    {
        private const string AemetIconBaseUrl = "https://www.aemet.es/imagenes/png/estado_cielo/";

        private static readonly Dictionary<string, string> EgoeraDeskribapenak = new Dictionary<string, string>
        {
            { "Despejado", "Oskarbi" },
            { "Poco nuboso", "Hodei gutxi" },
            { "Intervalos nubosos", "Hodei-tarteak" },
            { "Nuboso", "Hodeitsu" },
            { "Muy nuboso", "Oso hodeitsu" },
            { "Cubierto", "Estalia" },
            { "Nubes altas", "Hodei altuak" },
            { "Intervalos nubosos con lluvia escasa", "Hodei-tarteak eta euri txikia" },
            { "Nuboso con lluvia escasa", "Hodeitsu, euri txikiarekin" },
            { "Muy nuboso con lluvia escasa", "Oso hodeitsu, euri txikiarekin" },
            { "Cubierto con lluvia escasa", "Estalia, euri txikiarekin" },
            { "Intervalos nubosos con lluvia", "Hodei-tarteak eta euria" },
            { "Nuboso con lluvia", "Hodeitsu, euriarekin" },
            { "Muy nuboso con lluvia", "Oso hodeitsu, euriarekin" },
            { "Cubierto con lluvia", "Estalia, euriarekin" },
            { "Intervalos nubosos con nieve escasa", "Hodei-tarteak eta elur txikia" },
            { "Nuboso con nieve escasa", "Hodeitsu, elur txikiarekin" },
            { "Muy nuboso con nieve escasa", "Oso hodeitsu, elur txikiarekin" },
            { "Cubierto con nieve escasa", "Estalia, elur txikiarekin" },
            { "Intervalos nubosos con nieve", "Hodei-tarteak eta elurra" },
            { "Nuboso con nieve", "Hodeitsu, elurrarekin" },
            { "Muy nuboso con nieve", "Oso hodeitsu, elurrarekin" },
            { "Cubierto con nieve", "Estalia, elurrarekin" },
            { "Intervalos nubosos con tormenta", "Hodei-tarteak eta ekaitza" },
            { "Nuboso con tormenta", "Hodeitsu, ekaitzarekin" },
            { "Muy nuboso con tormenta", "Oso hodeitsu, ekaitzarekin" },
            { "Cubierto con tormenta", "Estalia, ekaitzarekin" },
            { "Intervalos nubosos con tormenta y lluvia escasa", "Hodei-tarteak, ekaitza eta euri txikia" },
            { "Nuboso con tormenta y lluvia escasa", "Hodeitsu, ekaitza eta euri txikiarekin" },
            { "Muy nuboso con tormenta y lluvia escasa", "Oso hodeitsu, ekaitza eta euri txikiarekin" },
            { "Cubierto con tormenta y lluvia escasa", "Estalia, ekaitza eta euri txikiarekin" },
            { "Niebla", "Lainoa" },
            { "Bruma", "Behelainoa" },
            { "Calima", "Kalima" }
        };

        public class ForecastDay
        {
            public string Data { get; set; }
            public string Egoera { get; set; }
            public string Ikonoa { get; set; }
            public string Max { get; set; }
            public string Min { get; set; }
        }

        private class ZeruEgoera
        {
            public string Deskribapena { get; set; }
            public string Ikonoa { get; set; }
        }

        public EguraldiaLeihoa()
        {
            InitializeComponent();
            DatuakKargatu();
        }

        private async void DatuakKargatu()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string url = "https://www.aemet.es/xml/municipios/localidad_20019.xml";
                    string xmlString = await client.GetStringAsync(url);
                    
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(xmlString);

                    // XPath bidez datu orokorrak lortu
                    lblHerria.Text = doc.SelectSingleNode("/root/nombre")?.InnerText ?? "Ezezaguna";
                    lblProbintzia.Text = doc.SelectSingleNode("/root/provincia")?.InnerText ?? "Ezezaguna";

                    // Gaurko datuak
                    XmlNode gaurNode = doc.SelectSingleNode("/root/prediccion/dia[1]");
                    if (gaurNode != null)
                    {
                        lblGaurData.Text = gaurNode.Attributes["fecha"]?.Value ?? "-";
                        
                        ZeruEgoera gaurEgoera = ZeruEgoeraLortu(gaurNode);
                        lblGaurEgoera.Text = gaurEgoera.Deskribapena;
                        imgGaurIkonoa.Source = new BitmapImage(new Uri(gaurEgoera.Ikonoa));

                        string max = gaurNode.SelectSingleNode("temperatura/maxima")?.InnerText ?? "--";
                        string min = gaurNode.SelectSingleNode("temperatura/minima")?.InnerText ?? "--";
                        
                        lblGaurTenperatura.Text = $"{max}°C";
                        lblGaurMinMax.Text = $"Min: {min}°C | Max: {max}°C";
                    }

                    // Hurrengo egunetako iragarpena (2. egunetik aurrera)
                    List<ForecastDay> forecast = new List<ForecastDay>();
                    XmlNodeList egunak = doc.SelectNodes("/root/prediccion/dia");
                    
                    for (int i = 1; i < egunak.Count; i++) // 1-etik hasita gaurkoa saltatzeko
                    {
                        XmlNode egunNode = egunak[i];
                        ZeruEgoera egoera = ZeruEgoeraLortu(egunNode);
                        forecast.Add(new ForecastDay
                        {
                            Data = egunNode.Attributes["fecha"]?.Value ?? "-",
                            Egoera = egoera.Deskribapena,
                            Ikonoa = egoera.Ikonoa,
                            Max = egunNode.SelectSingleNode("temperatura/maxima")?.InnerText ?? "--",
                            Min = egunNode.SelectSingleNode("temperatura/minima")?.InnerText ?? "--"
                        });
                    }

                    lstForecast.ItemsSource = forecast;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Errorea eguraldiaren datuak kargatzean: " + ex.Message, "Errorea", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
        }

        private ZeruEgoera ZeruEgoeraLortu(XmlNode egunNode)
        {
            XmlNode egoeraNode = EgoeraNodeLortu(egunNode);
            string kodea = KodeaLortu(egoeraNode);

            return new ZeruEgoera
            {
                Deskribapena = DeskribapenaLortu(kodea, egoeraNode),
                Ikonoa = IkonoaLortu(kodea)
            };
        }

        private XmlNode EgoeraNodeLortu(XmlNode egunNode)
        {
            XmlNodeList egoerak = egunNode.SelectNodes("estado_cielo");
            foreach (XmlNode egoera in egoerak)
            {
                string kodea = KodeaLortu(egoera);
                if (!string.IsNullOrWhiteSpace(kodea))
                {
                    return egoera;
                }
            }

            return egunNode.SelectSingleNode("estado_cielo[1]");
        }

        private string KodeaLortu(XmlNode egoeraNode)
        {
            string atributuKodea = egoeraNode?.Attributes["value"]?.Value;
            if (!string.IsNullOrWhiteSpace(atributuKodea))
            {
                return atributuKodea.Trim();
            }

            string testuKodea = egoeraNode?.InnerText;
            return string.IsNullOrWhiteSpace(testuKodea) ? null : testuKodea.Trim();
        }

        private string DeskribapenaLortu(string kodea, XmlNode egoeraNode)
        {
            string deskribapena = egoeraNode?.Attributes["descripcion"]?.Value;
            if (!string.IsNullOrWhiteSpace(deskribapena))
            {
                string gakoa = deskribapena.Replace(" noche", string.Empty).Trim();
                if (EgoeraDeskribapenak.ContainsKey(gakoa))
                {
                    return EgoeraDeskribapenak[gakoa];
                }

                return deskribapena;
            }

            return string.IsNullOrWhiteSpace(kodea) ? "Ezezaguna" : kodea;
        }

        private string IkonoaLortu(string kodea)
        {
            string kodeFinala = string.IsNullOrWhiteSpace(kodea) ? "11" : kodea;
            return AemetIconBaseUrl + kodeFinala + ".png";
        }

        private void Itxi_Klik(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
