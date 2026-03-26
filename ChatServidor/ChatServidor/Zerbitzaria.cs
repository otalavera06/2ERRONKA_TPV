using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

public class Zerbitzaria
{
    private TcpListener entzulea;
    private List<Bezeroa> bezeroak = new List<Bezeroa>();
    private int portua;

    public Zerbitzaria(int portua)
    {
        this.portua = portua;
    }

    public void Hasi()
    {
        entzulea = new TcpListener(IPAddress.Any, portua);
        entzulea.Start();
        Console.WriteLine($"Zerbitzaria martxan portuan {portua}");
    }

    public void Itxi()
    {
        entzulea.Stop();
    }

    public bool Konektatuta()
    {
        return entzulea != null;
    }

    public TcpClient OnartuKonexioa()
    {
        return entzulea.AcceptTcpClient();
    }

    public void GehituBezeroa(Bezeroa bezeroa)
    {
        bezeroak.Add(bezeroa);
    }

    public void BidaliMezuaGuztiei(string mezua)
    {
        foreach (var b in bezeroak)
        {
            b.Bidali(mezua);
        }
    }
}
