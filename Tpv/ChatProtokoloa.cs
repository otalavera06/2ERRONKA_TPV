using System;
using System.Text;

namespace Tpv
{
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
        public string TipuaMezua { get; set; }
        public string FitxategiIzena { get; set; }
        public long FitxategiTamaina { get; set; }
    }

    public static class ChatProtokoloa
    {
        public static string Kodetu(string balioa)
        {
            var testua = balioa ?? string.Empty;
            return CryptoHelper.Cifrar(testua);
        }

        public static string Dekodetu(string balioa)
        {
            if (string.IsNullOrWhiteSpace(balioa)) return string.Empty;
            try
            {
                return CryptoHelper.Descifrar(balioa);
            }
            catch
            {
                return string.Empty;
            }
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
            return $"CHAT|{rolIzena}|{mahaiaId}|{Kodetu(bidaltzailea)}|{Kodetu(testua)}|TEXT";
        }

        public static string SortuEmojiMezua(TxatRola rola, int mahaiaId, string bidaltzailea, string emoji)
        {
            var rolIzena = rola == TxatRola.Tpv ? "TPV" : "MESA";
            return $"CHAT|{rolIzena}|{mahaiaId}|{Kodetu(bidaltzailea)}|{Kodetu(emoji)}|EMOJI";
        }

        public static string SortuFitxategiMezua(TxatRola rola, int mahaiaId, string bidaltzailea, string fitxategiIzena, string fitxategiDataBes64)
        {
            var rolIzena = rola == TxatRola.Tpv ? "TPV" : "MESA";
            return $"CHAT|{rolIzena}|{mahaiaId}|{Kodetu(bidaltzailea)}|{fitxategiIzena}|{fitxategiDataBes64}|FILE";
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
                        Bidaltzailea = Dekodetu(zatiak[3])
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

                string tipoMezua = zatiak.Length >= 7 ? zatiak[6] : "TEXT";

                if (tipoMezua == "FILE" && zatiak.Length >= 7)
                {
                    paketea = new TxatPaketea
                    {
                        Komandoa = "CHAT",
                        Rola = rola,
                        MahaiaId = txatMahaiaId,
                        Bidaltzailea = Dekodetu(zatiak[3]),
                        FitxategiIzena = zatiak[4],
                        Testua = zatiak[5],
                        TipuaMezua = "FILE"
                    };
                    return true;
                }

                paketea = new TxatPaketea
                {
                    Komandoa = "CHAT",
                    Rola = rola,
                    MahaiaId = txatMahaiaId,
                    Bidaltzailea = Dekodetu(zatiak[3]),
                    Testua = Dekodetu(zatiak[4]),
                    TipuaMezua = tipoMezua
                };
                return true;
            }

            return false;
        }
    }
}
