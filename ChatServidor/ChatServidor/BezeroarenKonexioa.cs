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
        bezeroa.Bidali("Ongi etorri jatetxeko txatera!");
        try
        {
            while (bezeroa.Konektatuta())
            {
                string mezua = bezeroa.Irakurri();
                if (mezua == null) break;
                zerbitzaria.BidaliMezuaGuztiei(mezua);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Errorea bezeroan: " + ex.Message);
        }
    }
}
