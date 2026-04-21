using System;
using System.Text;

public enum TxatRola
{
    Ezezaguna,
    Tpv,
    Mahaia
}

public class TxatPaketea
{
    public string Komandoa { get; set; }
    public TxatRola Rola { get; set; }
    public int? MahaiaId { get; set; }
    public string Bidaltzailea { get; set; }
    public string Testua { get; set; }
}

public static class ChatProtokoloa
{
    private static string Kodetu(string balioa)
    {
        var testua = balioa ?? string.Empty;
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(testua));
    }

    private static string Deskodetu(string balioa)
    {
        if (string.IsNullOrWhiteSpace(balioa)) return string.Empty;
        return Encoding.UTF8.GetString(Convert.FromBase64String(balioa));
    }

    public static string SortuErregistroTpv()
    {
        return "REGISTER|TPV";
    }

    public static string SortuErregistroMahaia(int mahaiaId, string izena)
    {
        return $"REGISTER|MESA|{mahaiaId}|{Kodetu(izena)}";
    }

    public static string SortuKontaktua(int mahaiaId, string izena)
    {
        return $"CONTACT|MESA|{mahaiaId}|{Kodetu(izena)}";
    }

    public static string SortuTxatMezua(TxatRola rola, int mahaiaId, string bidaltzailea, string testua)
    {
        var rolIzena = rola == TxatRola.Tpv ? "TPV" : "MESA";
        return $"CHAT|{rolIzena}|{mahaiaId}|{Kodetu(bidaltzailea)}|{Kodetu(testua)}";
    }

    public static bool SaiatuPaketeaIrakurtzen(string lerroa, out TxatPaketea paketea)
    {
        paketea = null;
        if (string.IsNullOrWhiteSpace(lerroa)) return false;

        var zatiak = lerroa.Split('|');
        if (zatiak.Length < 2) return false;

        if (zatiak[0] == "REGISTER")
        {
            if (zatiak[1] == "TPV")
            {
                paketea = new TxatPaketea
                {
                    Komandoa = "REGISTER",
                    Rola = TxatRola.Tpv,
                    Bidaltzailea = "TPV"
                };
                return true;
            }

            if (zatiak[1] == "MESA" && zatiak.Length >= 4 && int.TryParse(zatiak[2], out var mahaiaId))
            {
                paketea = new TxatPaketea
                {
                    Komandoa = "REGISTER",
                    Rola = TxatRola.Mahaia,
                    MahaiaId = mahaiaId,
                    Bidaltzailea = Deskodetu(zatiak[3])
                };
                return true;
            }

            return false;
        }

        if (zatiak[0] == "CHAT" && zatiak.Length >= 5 && int.TryParse(zatiak[2], out var txatMahaiaId))
        {
            TxatRola rola;
            if (zatiak[1] == "TPV") rola = TxatRola.Tpv;
            else if (zatiak[1] == "MESA") rola = TxatRola.Mahaia;
            else return false;

            paketea = new TxatPaketea
            {
                Komandoa = "CHAT",
                Rola = rola,
                MahaiaId = txatMahaiaId,
                Bidaltzailea = Deskodetu(zatiak[3]),
                Testua = Deskodetu(zatiak[4])
            };
            return true;
        }

        return false;
    }
}
