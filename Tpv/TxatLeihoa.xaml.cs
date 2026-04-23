using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;

namespace Tpv
{
    public partial class TxatLeihoa : Window
    {
        private const string ChatHost = "127.0.0.1";
        private const int ChatPort = 5555;
        private static readonly TimeSpan ConnectTimeout = TimeSpan.FromSeconds(5);

        private TcpClient socketa;
        private StreamReader irakurlea;
        private StreamWriter idazlea;
        public ObservableCollection<Contact> Contacts { get; set; }
        
        private Contact _selectedContact;

        public TxatLeihoa()
        {
            InitializeComponent();
            Contacts = new ObservableCollection<Contact>(CreateDefaultContacts());
            lstContacts.ItemsSource = Contacts;

            Loaded += async (_, __) => await KonektatuZerbitzariaAsync();
        }

        private IEnumerable<Contact> CreateDefaultContacts()
        {
            return Enumerable.Range(1, 5).Select(mahaiaId => new Contact
            {
                Name = $"Mahaia {mahaiaId}",
                Initials = BuildInitials($"Mahaia {mahaiaId}", mahaiaId),
                Id = $"mahaia{mahaiaId}",
                MesaId = mahaiaId
            });
        }

        private async Task KonektatuZerbitzariaAsync()
        {
            try
            {
                socketa = new TcpClient();
                var connectTask = socketa.ConnectAsync(ChatHost, ChatPort);
                var completedTask = await Task.WhenAny(connectTask, Task.Delay(ConnectTimeout));

                if (completedTask != connectTask || !socketa.Connected)
                {
                    try { socketa.Close(); } catch { }
                    socketa = null;
                    MessageBox.Show("Ezin izan da ChatServidor-era konektatu. Egiaztatu zerbitzaria martxan dagoela TPV honetan.");
                    Close();
                    return;
                }

                var stream = socketa.GetStream();
                irakurlea = new StreamReader(stream);
                idazlea = new StreamWriter(stream) { AutoFlush = true };
                idazlea.WriteLine("REGISTER|TPV");

                Thread haria = new Thread(Entzun);
                haria.IsBackground = true;
                haria.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Errorea konektatzean: " + ex.Message);
                Close();
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
            var zatiak = rawMezua.Split('|');
            if (zatiak.Length < 2) return;

            if (zatiak[0] == "CONTACT" && zatiak.Length >= 4 && zatiak[1] == "MESA" && int.TryParse(zatiak[2], out var kontaktuMahaiaId))
            {
                EnsureContact(kontaktuMahaiaId, Decode(zatiak[3]));
                return;
            }

            if (zatiak[0] != "CHAT" || zatiak.Length < 5 || zatiak[1] != "MESA" || !int.TryParse(zatiak[2], out var mahaiaId))
            {
                return;
            }

            var senderName = Decode(zatiak[3]);
            var contact = EnsureContact(mahaiaId, senderName);

            string tipoMezua = zatiak.Length >= 7 ? zatiak[6] : "TEXT";

            if (tipoMezua == "FILE" && zatiak.Length >= 7)
            {
                var arxiboIzena = zatiak[4];
                var arxiboCifratua = zatiak[5];
                var arxiboBase64 = Decode(arxiboCifratua);

                contact.Messages.Add(new Mezua
                {
                    Testua = $"[Fitxategia: {arxiboIzena}]",
                    Tpvkoa = false,
                    SenderName = string.IsNullOrWhiteSpace(senderName) ? contact.Name : senderName,
                    TimeStamp = DateTime.Now.ToString("HH:mm"),
                    TipoMezua = "FILE",
                    FitxategiIzena = arxiboIzena,
                    FitxategiDataBase64 = arxiboBase64
                });
            }
            else if (tipoMezua == "EMOJI")
            {
                var emoji = Decode(zatiak[4]);
                contact.AddMessage(emoji, false, senderName, "EMOJI");
            }
            else
            {
                var content = Decode(zatiak[4]);
                contact.AddMessage(content, false, senderName, "TEXT");
            }

            if (_selectedContact != contact)
            {
                contact.UnreadCount++;
            }
            else
            {
                ScrollToBottom();
            }

            UpdateTitle();
        }

        private void LstContacts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstContacts.SelectedItem is Contact contact)
            {
                _selectedContact = contact;
                contact.UnreadCount = 0;
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
                _selectedContact.AddMessage(text, true, "TPV", "TEXT");
                ScrollToBottom();

                try
                {
                    idazlea.WriteLine(BuildTpvChatMessage(_selectedContact.MesaId, text, "TEXT"));
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Errorea bidaltzean: " + ex.Message);
                }

                txtMezua.Clear();
            }
        }

        private void BtnArxibo_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedContact == null)
            {
                MessageBox.Show("Aukeratu kontaktu bat lehenengo.");
                return;
            }

            var dialog = new OpenFileDialog();
            var result = dialog.ShowDialog();
            if (result == true && !string.IsNullOrWhiteSpace(dialog.FileName))
            {
                BidaliArxibo(dialog.FileName);
            }
        }

        private void BtnEmoji_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedContact == null)
            {
                MessageBox.Show("Aukeratu kontaktu bat lehenengo.");
                return;
            }

            var button = sender as Button;
            if (button == null) return;

            var menu = new ContextMenu();
            var emojis = new[] { "😊", "😂", "👍", "❤️", "😮" };
            foreach (var emoji in emojis)
            {
                var item = new MenuItem
                {
                    Header = emoji,
                    FontSize = 16
                };
                item.Click += (_, __) => BidaliEmoji(emoji);
                menu.Items.Add(item);
            }

            button.ContextMenu = menu;
            menu.IsOpen = true;
        }

        private void LstMezuak_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lstMezuak.SelectedItem is Mezua mezua && mezua.TipoMezua == "FILE" && !string.IsNullOrWhiteSpace(mezua.FitxategiDataBase64))
            {
                var dialog = new SaveFileDialog();
                dialog.FileName = string.IsNullOrWhiteSpace(mezua.FitxategiIzena) ? "fitxategia" : mezua.FitxategiIzena;
                var result = dialog.ShowDialog();
                if (result == true)
                {
                    try
                    {
                        byte[] data = Convert.FromBase64String(mezua.FitxategiDataBase64);
                        File.WriteAllBytes(dialog.FileName, data);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Errorea fitxategia gordetzean: " + ex.Message);
                    }
                }
            }
        }

        public void BidaliEmoji(string emoji)
        {
            if (_selectedContact == null) return;

            _selectedContact.AddMessage(emoji, true, "TPV", "EMOJI");
            ScrollToBottom();

            try
            {
                idazlea.WriteLine(BuildTpvChatMessage(_selectedContact.MesaId, emoji, "EMOJI"));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Errorea bidaltzean: " + ex.Message);
            }
        }

        public void BidaliArxibo(string arxiboPath)
        {
            if (_selectedContact == null || !File.Exists(arxiboPath)) return;

            try
            {
                byte[] arxiboData = File.ReadAllBytes(arxiboPath);
                string arxiboIzena = Path.GetFileName(arxiboPath);
                string arxiboBase64 = Convert.ToBase64String(arxiboData);
                string arxiboCifratua = Encode(arxiboBase64);

                _selectedContact.Messages.Add(new Mezua
                {
                    Testua = $"[Fitxategia: {arxiboIzena}]",
                    Tpvkoa = true,
                    SenderName = "TPV",
                    TimeStamp = DateTime.Now.ToString("HH:mm"),
                    TipoMezua = "FILE",
                    FitxategiIzena = arxiboIzena,
                    FitxategiDataBase64 = arxiboBase64
                });
                ScrollToBottom();

                idazlea.WriteLine(BuildTpvFileMessage(_selectedContact.MesaId, arxiboIzena, arxiboCifratua));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Errorea fitxategia bidaltzean: " + ex.Message);
            }
        }

        private void ScrollToBottom()
        {
            if (lstMezuak.Items.Count > 0)
            {
                lstMezuak.ScrollIntoView(lstMezuak.Items[lstMezuak.Items.Count - 1]);
            }
        }

        private Contact EnsureContact(int mahaiaId, string name)
        {
            var normalizedName = string.IsNullOrWhiteSpace(name) ? $"Mahaia {mahaiaId}" : name;
            var existing = Contacts.FirstOrDefault(c => c.MesaId == mahaiaId);
            if (existing != null)
            {
                existing.Name = normalizedName;
                existing.Initials = BuildInitials(normalizedName, mahaiaId);
                return existing;
            }

            var contact = new Contact
            {
                Name = normalizedName,
                Initials = BuildInitials(normalizedName, mahaiaId),
                Id = $"mahaia{mahaiaId}",
                MesaId = mahaiaId
            };
            Contacts.Add(contact);
            return contact;
        }

        private string BuildTpvChatMessage(int mahaiaId, string text, string tipoMezua = "TEXT")
        {
            return $"CHAT|TPV|{mahaiaId}|{Encode("TPV")}|{Encode(text)}|{tipoMezua}";
        }

        private string BuildTpvFileMessage(int mahaiaId, string arxiboIzena, string arxiboCifratua)
        {
            return $"CHAT|TPV|{mahaiaId}|{Encode("TPV")}|{arxiboIzena}|{arxiboCifratua}|FILE";
        }

        private string Encode(string value)
        {
            try
            {
                return ChatProtokoloa.Kodetu(value ?? string.Empty);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errorea zifratzean: {ex.Message}");
                return string.Empty;
            }
        }

        private string Decode(string value)
        {
            try
            {
                return ChatProtokoloa.Dekodetu(value ?? string.Empty);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errorea deszifratzean: {ex.Message}");
                return string.Empty;
            }
        }

        private string BuildInitials(string name, int mahaiaId)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                var parts = name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    return $"{parts[0][0]}{parts[1][0]}".ToUpper();
                }

                if (parts.Length == 1 && parts[0].Length >= 2)
                {
                    return parts[0].Substring(0, 2).ToUpper();
                }
            }

            return $"M{mahaiaId}";
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            try { irakurlea?.Close(); } catch { }
            try { idazlea?.Close(); } catch { }
            try { socketa?.Close(); } catch { }
            base.OnClosing(e);
        }
    }

    public class Contact : INotifyPropertyChanged
    {
        private string _name;
        private string _initials;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public string Initials
        {
            get => _initials;
            set
            {
                _initials = value;
                OnPropertyChanged(nameof(Initials));
            }
        }

        public string Id { get; set; }
        public int MesaId { get; set; }

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

        public void AddMessage(string text, bool isTpv, string senderName, string tipoMezua = "TEXT")
        {
            Messages.Add(new Mezua
            {
                Testua = text,
                Tpvkoa = isTpv,
                SenderName = string.IsNullOrWhiteSpace(senderName) ? (isTpv ? "TPV" : Name) : senderName,
                TimeStamp = DateTime.Now.ToString("HH:mm"),
                TipoMezua = tipoMezua
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
        public string TipoMezua { get; set; } = "TEXT";
        public string FitxategiIzena { get; set; }
        public string FitxategiDataBase64 { get; set; }
        
        public SolidColorBrush NameColor => Tpvkoa ? 
            (SolidColorBrush)Application.Current.FindResource("BrandBlack") :
            (SolidColorBrush)Application.Current.FindResource("BrandGold");

        public Color AvatarColor => Tpvkoa ? 
            ((SolidColorBrush)Application.Current.FindResource("BrandBlack")).Color : 
            ((SolidColorBrush)Application.Current.FindResource("BrandGold")).Color;
            
        public string Initials => SenderName.Length >= 2 ? SenderName.Substring(0, 2).ToUpper() : SenderName.Substring(0, 1).ToUpper();
    }
}
