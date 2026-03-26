using System.IO;
using System.Net.Sockets;

public class Bezeroa
{
    private TcpClient socketa;
    private StreamReader irakurlea;
    private StreamWriter idazlea;

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
        idazlea.WriteLine(mezua);
    }
}
