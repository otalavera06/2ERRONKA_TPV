using System.Windows;

namespace Tpv
{
    public partial class OdooDeskontuaLeihoa : Window
    {
        public string Kodea { get; private set; }

        public OdooDeskontuaLeihoa(string unekoKodea = "")
        {
            InitializeComponent();
            txtKodea.Text = unekoKodea ?? string.Empty;
            txtKodea.Focus();
            txtKodea.SelectAll();
        }

        private void Aplikatu_Klik(object sender, RoutedEventArgs e)
        {
            Kodea = txtKodea.Text.Trim();
            DialogResult = true;
        }

        private void Utzi_Klik(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
