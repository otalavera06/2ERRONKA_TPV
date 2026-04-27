using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Linq;

namespace Tpv
{
    public partial class EguraldiaLeihoa : Window
    {
        public class ForecastDay
        {
            public string Data { get; set; }
            public string Egoera { get; set; }
            public string Max { get; set; }
            public string Min { get; set; }
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
                        
                        // Zeruaren egoera (deskribapena hartu)
                        // AEMET XML-an estado_cielo hainbat ordukoak izan daitezke, lehenengoa hartuko dugu
                        XmlNode egoeraNode = gaurNode.SelectSingleNode("estado_cielo[1]");
                        lblGaurEgoera.Text = egoeraNode?.Attributes["descripcion"]?.Value ?? "Ezezaguna";

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
                        forecast.Add(new ForecastDay
                        {
                            Data = egunNode.Attributes["fecha"]?.Value ?? "-",
                            Egoera = egunNode.SelectSingleNode("estado_cielo[1]")?.Attributes["descripcion"]?.Value ?? "Ezezaguna",
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

        private void Itxi_Klik(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
