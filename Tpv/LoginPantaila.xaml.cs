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
using Tpv.Modeloak.Tpv.Modeloak;

namespace Tpv
{
    public partial class LoginPantaila : Window
    {
        public LoginPantaila()
        {
            InitializeComponent();
        }

        private void Titulua_EzkerrekoBotoiaKlik(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void GarbituEremuak()
        {
            txtErabiltzailea.Text = string.Empty;
            txtPasahitza.Clear();
            txtErabiltzailea.Focus();
        }

        private void Minimizatu_Klik(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Itxi_Klik(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private async void BtnLogin_Klik(object sender, RoutedEventArgs e)
        {

            string erabiltzailea = txtErabiltzailea.Text.Trim();
            string pasahitza = txtPasahitza.Password.Trim();

            if (string.IsNullOrEmpty(erabiltzailea) || string.IsNullOrEmpty(pasahitza))
            {
                MessageBox.Show("Sartu erabiltzailea eta pasahitza.");
                GarbituEremuak();
                return;
            }

            var loginRequest = new
            {
                erabiltzailea,
                pasahitza,
            };

            try
            {
                using (var client = new HttpClient())
                {
                    var url = "http://localhost:5005/api/langileak/login";
                    var json = JsonConvert.SerializeObject(loginRequest);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var erantzuna = await client.PostAsync(url, content);

                    if (erantzuna.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        MessageBox.Show("Ez da existitzen erabiltzaile hori edo pasahitza okerra da.");
                        GarbituEremuak();
                        return;
                    }
                    if (!erantzuna.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Errorea login egitean.");
                        GarbituEremuak();
                        return;
                    }

                    var jsonResponse = await erantzuna.Content.ReadAsStringAsync();
                    var erabiltzaileObj = JsonConvert.DeserializeObject<LangileakDto>(jsonResponse);

                    if (erabiltzaileObj == null || !erabiltzaileObj.Baimena)
                    {
                        MessageBox.Show("Erabiltzaileak ez du baimenik sistema honetara sartzeko.");
                        GarbituEremuak();
                        return;
                    }

                    MessageBox.Show("Ongi etorri, " + erabiltzaileObj.Erabiltzailea + "!");
                    this.Hide();
                    var menu = new Menu();
                    menu.Show();

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Errorea APIarekin konektatzean: " +
                    ex.Message);
                GarbituEremuak();
            }
        }

    }
}
