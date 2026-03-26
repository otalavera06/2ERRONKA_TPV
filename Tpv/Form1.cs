using NHibernate.Linq;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Tpv.Modeloak;
using Tpv.Modeloak.Tpv.Modeloak;

namespace Tpv
{
    public partial class Form1 : Form
    {
        private Panel panelContainer;
        private Panel panelLogo;
        private Panel panelLogin;
        private PictureBox pictureBoxLogo;

        private readonly Color KoloreGorriHandia = ColorTranslator.FromHtml("#B92732"); 
        private readonly Color KoloreGorriArgia = ColorTranslator.FromHtml("#E9C4C7");
        private readonly Color KoloreMarkoIluna = ColorTranslator.FromHtml("#373F47");
        private readonly Color KoloreBerdea = ColorTranslator.FromHtml("#BBC7A4");
        private readonly Color KoloreGorriIluna = ColorTranslator.FromHtml("#B5424B");
        private readonly Color KoloreLogoZuria = ColorTranslator.FromHtml("#FCFEFC"); 

        public Form1()
        {
            InitializeComponent();

            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.AcceptButton = button1;   
            this.BackColor = KoloreMarkoIluna;
            this.Padding = new Padding(20);

            SortuPanelakEtaLogoa();
            EstilatuKontrolak();
            AntolatuLoginKontrolak();

   
            this.Resize += (s, e) => AntolatuLoginKontrolak();
        }

       
        private void SortuPanelakEtaLogoa()
        {
            panelContainer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };
            this.Controls.Add(panelContainer);

            panelLogo = new Panel
            {
                Name = "panelLogo",
                Dock = DockStyle.Left,
                BackColor = KoloreLogoZuria,
                Width = this.ClientSize.Width / 2
            };

            panelLogin = new Panel
            {
                Name = "panelLogin",
                Dock = DockStyle.Fill,
                BackColor = KoloreGorriHandia
            };

            panelContainer.Controls.Add(panelLogin);
            panelContainer.Controls.Add(panelLogo);

            pictureBoxLogo = new PictureBox
            {
                Name = "pictureBoxLogo",
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom
            };
            panelLogo.Controls.Add(pictureBoxLogo);

            KargatuLogoa();

            panelLogin.Controls.Add(label1);
            panelLogin.Controls.Add(label3);
            panelLogin.Controls.Add(textBox1);
            panelLogin.Controls.Add(textBox2);
            panelLogin.Controls.Add(button1);
        }

      
        private void KargatuLogoa()
        {
            try
            {
                string fitxategiIzena = "sushineli.png";  

                string ruta = Path.Combine(Application.StartupPath, fitxategiIzena);

                if (File.Exists(ruta))
                {
                    pictureBoxLogo.Image = Image.FromFile(ruta);
                }
                else
                {
                    MessageBox.Show("Ez da aurkitu logotipoaren irudia: " + ruta);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Errorea logoa kargatzean: " + ex.Message);
            }
        }

     
        private void EstilatuKontrolak()
        {
            label1.Text = "Erabiltzailea:";
            label3.Text = "Pasahitza:";
            label1.ForeColor = KoloreLogoZuria;
            label3.ForeColor = KoloreLogoZuria;
            label1.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            label3.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            label1.AutoSize = true;
            label3.AutoSize = true;

            textBox1.BackColor = KoloreLogoZuria;
            textBox2.BackColor = KoloreLogoZuria;
            textBox1.BorderStyle = BorderStyle.FixedSingle;
            textBox2.BorderStyle = BorderStyle.FixedSingle;
            textBox1.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            textBox2.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            textBox2.UseSystemPasswordChar = true; 

            button1.Text = "Saioa hasi";
            button1.BackColor = KoloreMarkoIluna;
            button1.ForeColor = KoloreLogoZuria;
            button1.FlatStyle = FlatStyle.Flat;
            button1.FlatAppearance.BorderSize = 0;
            button1.Font = new Font("Segoe UI", 10, FontStyle.Bold);

           
        }

   
        private void AntolatuLoginKontrolak()
        {
            if (panelLogin == null) return;

            int panelZabalera = panelLogin.ClientSize.Width;
            int panelAltuera = panelLogin.ClientSize.Height;

            int labelZabalera = 130;
            int textBoxZabalera = 220;
            int kontrolAltuera = 26;
            int tarteBertikala = 20;
            int botoiAltuera = 35;

            int taldeAltuera = kontrolAltuera * 2 + tarteBertikala + botoiAltuera + 20;
            int hasieraY = panelAltuera / 2 - taldeAltuera / 2;
            int taldeZabalera = labelZabalera + 10 + textBoxZabalera;
            int hasieraX = panelZabalera / 2 - taldeZabalera / 2;

            label1.Location = new Point(hasieraX, hasieraY);
            textBox1.Location = new Point(hasieraX + labelZabalera + 10, hasieraY - 3);
            textBox1.Size = new Size(textBoxZabalera, kontrolAltuera);

           
            int y2 = hasieraY + kontrolAltuera + tarteBertikala;
            label3.Location = new Point(hasieraX, y2);
            textBox2.Location = new Point(hasieraX + labelZabalera + 10, y2 - 3);
            textBox2.Size = new Size(textBoxZabalera, kontrolAltuera);

            button1.Size = new Size(140, botoiAltuera);
            button1.Location = new Point(
                hasieraX + labelZabalera + 10 + (textBoxZabalera - button1.Width),
                y2 + kontrolAltuera + 20
            );

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string erabiltzailea = textBox1.Text.Trim();
            string pasahitza = textBox2.Text.Trim();

            if (string.IsNullOrEmpty(erabiltzailea) || string.IsNullOrEmpty(pasahitza))
            {
                MessageBox.Show("Sartu erabiltzailea eta pasahitza.");
                GarbituEremuak();
                return;
            }

            try
            {
                using (var session = NHibernateHelper.OpenSession())
                {
                    var erabiltzaileObj = session.Query<Erabiltzailea>()
                        .FirstOrDefault(u => u.erabiltzailea == erabiltzailea &&
                                             u.pasahitza == pasahitza);

                    if (erabiltzaileObj != null)
                    {
                        MessageBox.Show("Ongi etorri, " + erabiltzaileObj.erabiltzailea + "!");
                    }
                    else
                    {
                        MessageBox.Show("Erabiltzailea edo pasahitza okerra da.");
                        GarbituEremuak();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Errorea datu-basearekin konektatzean: " +
                    ex.Message + Environment.NewLine +
                    ex.InnerException?.Message);
                GarbituEremuak();
            }
        }
        

        private void button2_Click(object sender, EventArgs e)
        {
            GarbituEremuak();
        }

        private void GarbituEremuak()
        {
            textBox1.Clear();
            textBox2.Clear();
            textBox1.Focus();
        }


        private void Form1_Load(object sender, EventArgs e) { }
        private void label1_Click(object sender, EventArgs e) { }
        private void label3_Click(object sender, EventArgs e) { }
    }
}
