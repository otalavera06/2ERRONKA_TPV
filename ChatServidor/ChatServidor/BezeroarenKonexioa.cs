using System;
using System.Threading;

public class BezeroarenKonexioa
{
    private Bezeroa bezeroa;
    private Zerbitzaria zerbitzaria;
    private Thread haria;

    public BezeroarenKonexioa(Bezeroa bezeroa, Zerbitzaria zerbitzaria)
    {
        this.bezeroa = bezeroa;
        this.zerbitzaria = zerbitzaria;
        haria = new Thread(Run);
    }

    public void Hasi()
    {
        haria.Start();
    }

    private void Run()
    {
        try
        {
            while (bezeroa.Konektatuta())
            {
                string mezua = bezeroa.Irakurri();
                if (mezua == null) break;

                if (!ChatProtokoloa.SaiatuPaketeaIrakurtzen(mezua, out var paketea))
                {
                    Console.WriteLine("Pakete ezezaguna: " + mezua);
                    continue;
                }

                if (bezeroa.Rola == TxatRola.Ezezaguna && paketea.Komandoa != "REGISTER")
                {
                    Console.WriteLine("Erregistratu gabeko bezero baten mezua baztertuta.");
                    continue;
                }

                zerbitzaria.ProzesatuPaketea(bezeroa, paketea);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Errorea bezeroan: " + ex.Message);
        }
        finally
        {
            zerbitzaria.KenduBezeroa(bezeroa);
            bezeroa.Itxi();
        }
    }
}
