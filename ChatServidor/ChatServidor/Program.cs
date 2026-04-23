using ChatServidor;
using System;
using System.Net.Sockets;

class Program
{
    static void Main(string[] args)
    {
        Zerbitzaria zerbitzaria = new Zerbitzaria(5555);
        zerbitzaria.Hasi();

        while (zerbitzaria.Konektatuta())
        {
            TcpClient tcpBezeroa = zerbitzaria.OnartuKonexioa();
            Bezeroa bezeroa = new Bezeroa(tcpBezeroa);
            zerbitzaria.GehituBezeroa(bezeroa);

            BezeroarenKonexioa konexioa = new BezeroarenKonexioa(bezeroa, zerbitzaria);
            konexioa.Hasi();
        }
    }
}
