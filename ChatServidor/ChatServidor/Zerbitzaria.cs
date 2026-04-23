using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace ChatServidor
{
    public class Zerbitzaria
{
    private TcpListener entzulea;
    private List<Bezeroa> bezeroak = new List<Bezeroa>();
    private int portua;
    private readonly object bezeroakLock = new object();

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
        lock (bezeroakLock)
        {
            bezeroak.Add(bezeroa);
        }
    }

    public void KenduBezeroa(Bezeroa bezeroa)
    {
        lock (bezeroakLock)
        {
            bezeroak.Remove(bezeroa);
        }
    }

    public void Erregistratu(Bezeroa bezeroa, TxatPaketea paketea)
    {
        bezeroa.Rola = paketea.Rola;
        bezeroa.MahaiaId = paketea.MahaiaId;
        bezeroa.Izena = string.IsNullOrWhiteSpace(paketea.Bidaltzailea)
            ? (paketea.Rola == TxatRola.Tpv ? "TPV" : $"Mahaia {paketea.MahaiaId}")
            : paketea.Bidaltzailea;

        if (bezeroa.Rola == TxatRola.Tpv)
        {
            foreach (var mahaia in LortuMahaiAktiboak())
            {
                bezeroa.Bidali(ChatProtokoloa.SortuKontaktua(mahaia.MahaiaId.Value, mahaia.Izena));
            }

            return;
        }

        if (bezeroa.Rola == TxatRola.Mahaia && bezeroa.MahaiaId.HasValue)
        {
            var kontaktuLerroa = ChatProtokoloa.SortuKontaktua(bezeroa.MahaiaId.Value, bezeroa.Izena);
            foreach (var tpv in LortuTpvak())
            {
                tpv.Bidali(kontaktuLerroa);
            }
        }
    }

        public void ProzesatuPaketea(Bezeroa bezeroa, TxatPaketea paketea)
        {
            if (paketea.Komandoa == "REGISTER")
            {
                Erregistratu(bezeroa, paketea);
                return;
            }

            if (paketea.Komandoa != "CHAT") return;

            if (bezeroa.Rola == TxatRola.Mahaia && bezeroa.MahaiaId.HasValue)
            {
                string mezua;

                if (paketea.TipuaMezua == "FILE")
                {
                    mezua = ChatProtokoloa.SortuFitxategiMezua(
                        TxatRola.Mahaia,
                        bezeroa.MahaiaId.Value,
                        bezeroa.Izena,
                        paketea.FitxategiIzena ?? string.Empty,
                        paketea.Testua ?? string.Empty);
                }
                else if (paketea.TipuaMezua == "EMOJI")
                {
                    mezua = ChatProtokoloa.SortuEmojiMezua(
                        TxatRola.Mahaia,
                        bezeroa.MahaiaId.Value,
                        bezeroa.Izena,
                        paketea.Testua ?? string.Empty);
                }
                else
                {
                    mezua = ChatProtokoloa.SortuTxatMezua(
                        TxatRola.Mahaia,
                        bezeroa.MahaiaId.Value,
                        bezeroa.Izena,
                        paketea.Testua ?? string.Empty);
                }

                foreach (var tpv in LortuTpvak())
                {
                    tpv.Bidali(mezua);
                }
                return;
            }

            if (bezeroa.Rola == TxatRola.Tpv && paketea.MahaiaId.HasValue)
            {
                string mezua;

                if (paketea.TipuaMezua == "FILE")
                {
                    mezua = ChatProtokoloa.SortuFitxategiMezua(
                        TxatRola.Tpv,
                        paketea.MahaiaId.Value,
                        string.IsNullOrWhiteSpace(bezeroa.Izena) ? "TPV" : bezeroa.Izena,
                        paketea.FitxategiIzena ?? string.Empty,
                        paketea.Testua ?? string.Empty);
                }
                else if (paketea.TipuaMezua == "EMOJI")
                {
                    mezua = ChatProtokoloa.SortuEmojiMezua(
                        TxatRola.Tpv,
                        paketea.MahaiaId.Value,
                        string.IsNullOrWhiteSpace(bezeroa.Izena) ? "TPV" : bezeroa.Izena,
                        paketea.Testua ?? string.Empty);
                }
                else
                {
                    mezua = ChatProtokoloa.SortuTxatMezua(
                        TxatRola.Tpv,
                        paketea.MahaiaId.Value,
                        string.IsNullOrWhiteSpace(bezeroa.Izena) ? "TPV" : bezeroa.Izena,
                        paketea.Testua ?? string.Empty);
                }

                foreach (var mahaia in LortuMahaikoBezeroak(paketea.MahaiaId.Value))
                {
                    mahaia.Bidali(mezua);
                }
            }
        }

    private List<Bezeroa> LortuTpvak()
    {
        lock (bezeroakLock)
        {
            return bezeroak.Where(b => b.Rola == TxatRola.Tpv).ToList();
        }
    }

    private List<Bezeroa> LortuMahaiAktiboak()
    {
        lock (bezeroakLock)
        {
            return bezeroak
                .Where(b => b.Rola == TxatRola.Mahaia && b.MahaiaId.HasValue)
                .GroupBy(b => b.MahaiaId)
                .Select(g => g.First())
                .ToList();
        }
    }

    private List<Bezeroa> LortuMahaikoBezeroak(int mahaiaId)
    {
        lock (bezeroakLock)
        {
            return bezeroak
                .Where(b => b.Rola == TxatRola.Mahaia && b.MahaiaId == mahaiaId)
                .ToList();
        }
    }
}
}
