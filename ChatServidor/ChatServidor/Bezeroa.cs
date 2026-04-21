using System.IO;
using System.Net.Sockets;

public class Bezeroa
{
    private TcpClient socketa;
    private StreamReader irakurlea;
    private StreamWriter idazlea;
    private readonly object bidalketaLock = new object();

    public TxatRola Rola { get; set; } = TxatRola.Ezezaguna;
    public int? MahaiaId { get; set; }
    public string Izena { get; set; } = "Ezezaguna";

    public Bezeroa(TcpClient socketa)
    {
        this.socketa = socketa;
        var stream = socketa.GetStream();
        irakurlea = new StreamReader(stream);
        idazlea = new StreamWriter(stream) { AutoFlush = true };
    }

    public bool Konektatuta()
    {
        return socketa.Connected;
    }

    public string Irakurri()
    {
        return irakurlea.ReadLine();
    }

    public void Bidali(string mezua)
    {
        lock (bidalketaLock)
        {
            idazlea.WriteLine(mezua);
        }
    }

    public void Itxi()
    {
        try { irakurlea?.Close(); } catch { }
        try { idazlea?.Close(); } catch { }
        try { socketa?.Close(); } catch { }
    }
}
