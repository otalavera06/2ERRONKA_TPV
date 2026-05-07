using ChatServidor;
using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

class Program
{
    static void Main(string[] args)
    {
        var portua = LortuPortua(args);
        Zerbitzaria zerbitzaria = new Zerbitzaria(portua);

        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            zerbitzaria.Itxi();
            Console.WriteLine("ChatServidor gelditzen...");
        };

        try
        {
            zerbitzaria.Hasi();
            ErakutsiEntzutekoHelbideak(portua);

            while (zerbitzaria.Konektatuta())
            {
                TcpClient tcpBezeroa = zerbitzaria.OnartuKonexioa();
                Console.WriteLine("Bezero berria: " + tcpBezeroa.Client.RemoteEndPoint);

                Bezeroa bezeroa = new Bezeroa(tcpBezeroa);
                zerbitzaria.GehituBezeroa(bezeroa);

                BezeroarenKonexioa konexioa = new BezeroarenKonexioa(bezeroa, zerbitzaria);
                konexioa.Hasi();
            }
        }
        catch (SocketException ex)
        {
            Console.WriteLine($"Ezin da ChatServidor martxan jarri {portua} portuan: {ex.Message}");
            Console.WriteLine("Egiaztatu portua libre dagoela eta firewall-ak TCP sarrera baimentzen duela.");
        }
        catch (ObjectDisposedException)
        {
        }
        catch (Exception ex)
        {
            Console.WriteLine("Errorea ChatServidor-en: " + ex.Message);
        }
    }

    private static int LortuPortua(string[] args)
    {
        var argPortua = args
            .Select(arg => arg.StartsWith("--port=", StringComparison.OrdinalIgnoreCase) ? arg.Substring("--port=".Length) : arg)
            .FirstOrDefault(arg => int.TryParse(arg, out _));

        if (int.TryParse(argPortua, out var portuaArgumentutik))
        {
            return portuaArgumentutik;
        }

        if (int.TryParse(ConfigurationManager.AppSettings["ChatPort"], out var portuaKonfiguraziotik))
        {
            return portuaKonfiguraziotik;
        }

        return 5555;
    }

    private static void ErakutsiEntzutekoHelbideak(int portua)
    {
        Console.WriteLine($"ChatServidor martxan 0.0.0.0:{portua}");
        Console.WriteLine("Bezeroek IP hauetako batera konektatu behar dute:");

        var helbideak = NetworkInterface.GetAllNetworkInterfaces()
            .Where(ni => ni.OperationalStatus == OperationalStatus.Up)
            .SelectMany(ni => ni.GetIPProperties().UnicastAddresses)
            .Select(ua => ua.Address)
            .Where(ip => ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip))
            .Distinct();

        foreach (var ip in helbideak)
        {
            Console.WriteLine($"  {ip}:{portua}");
        }
    }
}
