using System;
using System.Collections.Generic;
using System.Linq;
using Tpv.DTO;

namespace Tpv
{
    public class EskaeraStockEmaitza
    {
        public bool Nahikoa { get; set; }
        public ProduktuaDto Produktua { get; set; }
        public int EskatutakoKantitatea { get; set; }
        public int StockErabilgarria { get; set; }
    }

    public static class EskaeraKalkulagailua
    {
        public static decimal KalkulatuTotala(IEnumerable<ProduktuaDto> produktuak)
        {
            return produktuak == null ? 0 : produktuak.Sum(p => p.Prezioa);
        }

        public static EskaeraStockEmaitza BalidatuStocka(IEnumerable<ProduktuaDto> produktuak, IDictionary<int, int> editatzenHasierakoKantitateak = null)
        {
            var hautatutakoProduktuak = produktuak == null ? new List<ProduktuaDto>() : produktuak.ToList();

            var stockFalta = hautatutakoProduktuak
                .GroupBy(p => p.Id)
                .Select(g => new EskaeraStockEmaitza
                {
                    Produktua = g.First(),
                    EskatutakoKantitatea = g.Count(),
                    StockErabilgarria = g.First().Stock + LortuHasierakoKantitatea(editatzenHasierakoKantitateak, g.Key)
                })
                .FirstOrDefault(x => !x.Produktua.IsPlatera && x.EskatutakoKantitatea > x.StockErabilgarria);

            return stockFalta ?? new EskaeraStockEmaitza { Nahikoa = true };
        }

        public static ZerbitzuaSortuDto EraikiZerbitzua(IEnumerable<ProduktuaDto> produktuak, int mahaiId, decimal? deskontatutakoTotala = null, DateTime? data = null)
        {
            var hautatutakoProduktuak = produktuak == null ? new List<ProduktuaDto>() : produktuak.ToList();
            var zerbitzuData = data ?? DateTime.Now;

            var zerbitzua = new ZerbitzuaSortuDto
            {
                Data = zerbitzuData,
                MahaiakId = mahaiId,
                PrezioTotala = deskontatutakoTotala ?? KalkulatuTotala(hautatutakoProduktuak),
                Eskaerak = new List<EskaerakSortuDto>()
            };

            foreach (var produktua in hautatutakoProduktuak)
            {
                zerbitzua.Eskaerak.Add(new EskaerakSortuDto
                {
                    ProduktuaId = produktua.Id,
                    Izena = produktua.Izena,
                    Prezioa = produktua.Prezioa,
                    Data = zerbitzuData,
                    Egoera = 0,
                    IsPlatera = produktua.IsPlatera
                });
            }

            return zerbitzua;
        }

        public static ProduktuaDto SortuProduktuaEskaeratik(EskaeraDto eskaera, IEnumerable<ProduktuaDto> katalogoa, int editatzenHasierakoKantitatea)
        {
            var produktuak = katalogoa == null ? new List<ProduktuaDto>() : katalogoa.ToList();
            var katalogokoProduktua = eskaera.IsPlatera
                ? produktuak.FirstOrDefault(p => p.IsPlatera && string.Equals(p.Izena, eskaera.Izena, StringComparison.OrdinalIgnoreCase))
                : produktuak.FirstOrDefault(p => !p.IsPlatera && p.Id == eskaera.ProduktuaId);

            if (katalogokoProduktua != null)
            {
                return katalogokoProduktua;
            }

            return new ProduktuaDto
            {
                Id = eskaera.ProduktuaId,
                Izena = eskaera.Izena,
                Prezioa = eskaera.Prezioa,
                Stock = eskaera.IsPlatera ? 999 : editatzenHasierakoKantitatea,
                IsPlatera = eskaera.IsPlatera
            };
        }

        private static int LortuHasierakoKantitatea(IDictionary<int, int> editatzenHasierakoKantitateak, int produktuaId)
        {
            int kantitatea;
            return editatzenHasierakoKantitateak != null && editatzenHasierakoKantitateak.TryGetValue(produktuaId, out kantitatea) ? kantitatea : 0;
        }
    }
}
