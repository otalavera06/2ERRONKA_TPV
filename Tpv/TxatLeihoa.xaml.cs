using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Tpv
{
    public partial class TxatLeihoa : Window
    {
        private TcpClient socketa;
        private StreamReader irakurlea;
        private StreamWriter idazlea;
        public ObservableCollection<Contact> Contacts { get; set; }
        
        private Contact _selectedContact;

        public TxatLeihoa()
        {
            InitializeComponent();
            InitializeContacts();
            lstContacts.ItemsSource = Contacts;
            
            KonektatuZerbitzaria();
        }

        private void InitializeContacts()
        {
            Contacts = new ObservableCollection<Contact>
            {
                new Contact { Name = "Mahaia 1", Initials = "M1", Id = "mahaia1" },
                new Contact { Name = "Mahaia 2", Initials = "M2", Id = "mahaia2" },
                new Contact { Name = "Mahaia 3", Initials = "M3", Id = "mahaia3" },
                new Contact { Name = "Mahaia 4", Initials = "M4", Id = "mahaia4" },
                new Contact { Name = "Mahaia 5", Initials = "M5", Id = "mahaia5" }
            };
        }

        private void KonektatuZerbitzaria()
        {
            try
            {
                socketa = new TcpClient("192.168.1.104", 5555);
                var stream = socketa.GetStream();
                irakurlea = new StreamReader(stream);
                idazlea = new StreamWriter(stream) { AutoFlush = true };

                Thread haria = new Thread(Entzun);
                haria.IsBackground = true;
                haria.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Errorea konektatzean: " + ex.Message);
            }
        }

        private void Entzun()
        {
            try
            {
                while (true)
                {
                    string mezua = irakurlea.ReadLine();
                    if (mezua != null)
                    {
                        Dispatcher.Invoke(() => ProzesatuMezua(mezua));
                    }
                }
            }
            catch { }
        }

        private void ProzesatuMezua(string rawMezua)
        {
            // Ignorar mensajes enviados por el TPV si el servidor hace eco
            if (rawMezua.StartsWith("TPV:")) return;

            // Formato esperado: "MahaiaX: mensaje" o "User: mensaje"
            // También soporta formato antiguo Android: "ANDROID[MAHAI:X]: mensaje"
            string senderName = "Ezezaguna";
            string content = rawMezua;

            // Detectar formato Android antiguo/complejo
            if (rawMezua.StartsWith("ANDROID[MAHAI:"))
            {
                int endBracket = rawMezua.IndexOf(']');
                if (endBracket > 14) // "ANDROID[MAHAI:" is 14 chars
                {
                    string idPart = rawMezua.Substring(14, endBracket - 14); // Extract ID (e.g. "1")
                    senderName = "Mahaia " + idPart;
                    
                    int msgStart = rawMezua.IndexOf(':', endBracket);
                    if (msgStart > 0)
                    {
                        content = rawMezua.Substring(msgStart + 1).Trim();
                    }
                }
            }
            else
            {
                // Formato estándar "Sender: Message"
                int separatorIndex = rawMezua.IndexOf(':');
                if (separatorIndex > 0)
                {
                    senderName = rawMezua.Substring(0, separatorIndex).Trim();
                    content = rawMezua.Substring(separatorIndex + 1).Trim();
                }
            }

            // Buscar contacto (normalizando espacios y mayúsculas)
            // "Mahaia 1" vs "Mahaia1" vs "mahaia 1"
            var contact = Contacts.FirstOrDefault(c => 
                c.Name.Replace(" ", "").Equals(senderName.Replace(" ", ""), StringComparison.OrdinalIgnoreCase) ||
                c.Id.Equals(senderName, StringComparison.OrdinalIgnoreCase));

            if (contact != null)
            {
                contact.AddMessage(content, false);
                
                // Si no es el contacto seleccionado, aumentar contador
                if (_selectedContact != contact)
                {
                    contact.UnreadCount++;
                    // Notificación visual (opcional)
                    // System.Media.SystemSounds.Beep.Play(); 
                }
                else
                {
                    ScrollToBottom();
                }
            }
            else
            {
                // Mensaje de alguien desconocido, lo asignamos a un "General" o lo ignoramos
                // Por ahora, si no coincide, no hacemos nada o lo metemos en el primero si es de prueba
            }
        }

        private void LstContacts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstContacts.SelectedItem is Contact contact)
            {
                _selectedContact = contact;
                contact.UnreadCount = 0; // Resetear contador al leer
                UpdateTitle();
                lstMezuak.ItemsSource = contact.Messages;
                txtChatTitle.Text = contact.Name;
                ScrollToBottom();
            }
        }

        private void UpdateTitle()
        {
            int totalUnread = Contacts.Sum(c => c.UnreadCount);
            Title = totalUnread > 0 ? $"Txata ({totalUnread})" : "Txata";
        }

        private void BtnBidali_Click(object sender, RoutedEventArgs e)
        {
            BidaliMezua();
        }

        private void TxtMezua_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BidaliMezua();
            }
        }

        private void BidaliMezua()
        {
            if (_selectedContact == null)
            {
                MessageBox.Show("Aukeratu kontaktu bat lehenengo.");
                return;
            }

            if (!string.IsNullOrWhiteSpace(txtMezua.Text))
            {
                string text = txtMezua.Text;
                
                // Añadir mensaje localmente
                _selectedContact.AddMessage(text, true);
                ScrollToBottom();

                // Enviar al servidor
                // Protocolo: "TPV: @Mahaia1 Mensaje" o simplemente broadcast "TPV: Mensaje"
                // Asumimos broadcast o que el servidor gestiona
                try
                {
                    idazlea.WriteLine($"TPV: {text}"); 
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Errorea bidaltzean: " + ex.Message);
                }

                txtMezua.Clear();
            }
        }

        private void ScrollToBottom()
        {
            if (lstMezuak.Items.Count > 0)
            {
                lstMezuak.ScrollIntoView(lstMezuak.Items[lstMezuak.Items.Count - 1]);
            }
        }
    }

    public class Contact : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public string Initials { get; set; }
        public string Id { get; set; } // Para coincidir con la DB o protocolo (mahaia1, etc)

        private int _unreadCount;
        public int UnreadCount
        {
            get => _unreadCount;
            set
            {
                _unreadCount = value;
                OnPropertyChanged(nameof(UnreadCount));
                OnPropertyChanged(nameof(UnreadVisibility));
            }
        }

        public Visibility UnreadVisibility => UnreadCount > 0 ? Visibility.Visible : Visibility.Collapsed;

        public ObservableCollection<Mezua> Messages { get; set; } = new ObservableCollection<Mezua>();

        public void AddMessage(string text, bool isTpv)
        {
            Messages.Add(new Mezua
            {
                Testua = text,
                Tpvkoa = isTpv,
                SenderName = isTpv ? "TPV" : Name,
                TimeStamp = DateTime.Now.ToString("HH:mm")
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class Mezua
    {
        public string Testua { get; set; }
        public bool Tpvkoa { get; set; }
        public string SenderName { get; set; }
        public string TimeStamp { get; set; }
        
        public SolidColorBrush NameColor => Tpvkoa ? 
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7289DA")) : // TPV Color
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FADADD"));  // Client Color

        public Color AvatarColor => Tpvkoa ? 
            (Color)ColorConverter.ConvertFromString("#7289DA") : 
            (Color)ColorConverter.ConvertFromString("#5865F2");
            
        public string Initials => SenderName.Length >= 2 ? SenderName.Substring(0, 2).ToUpper() : SenderName.Substring(0, 1).ToUpper();
    }
}
