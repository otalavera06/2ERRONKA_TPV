using System;
using System.Collections.Generic;
using Tpv.DTO;
using Xunit;

namespace Tpv.Testak
{
    public class EskaeraKalkulagailuaTest
    {
        [Fact]
        public void KalkulatuTotala_produktu_arruntekin_batura_itzultzen_du()
        {
            var produktuak = new List<ProduktuaDto>
            {
                Produktua(1, "Kafea", 1.50m, 10),
                Produktua(2, "Ura", 2.00m, 10),
                Produktua(3, "Ogitartekoa", 4.25m, 10)
            };

            var totala = EskaeraKalkulagailua.KalkulatuTotala(produktuak);

            Assert.Equal(7.75m, totala);
        }

        [Fact]
        public void KalkulatuTotala_zerrenda_hutsarekin_zero_itzultzen_du()
        {
            var totala = EskaeraKalkulagailua.KalkulatuTotala(new List<ProduktuaDto>());

            Assert.Equal(0, totala);
        }

        [Fact]
        public void BalidatuStocka_stock_nahikoarekin_ondo_itzultzen_du()
        {
            var produktuak = new List<ProduktuaDto>
            {
                Produktua(1, "Kafea", 1.50m, 2),
                Produktua(1, "Kafea", 1.50m, 2)
            };

            var emaitza = EskaeraKalkulagailua.BalidatuStocka(produktuak);

            Assert.True(emaitza.Nahikoa);
        }

        [Fact]
        public void BalidatuStocka_stock_gutxiegirekin_produktua_eta_kantitateak_itzultzen_ditu()
        {
            var produktuak = new List<ProduktuaDto>
            {
                Produktua(1, "Kafea", 1.50m, 1),
                Produktua(1, "Kafea", 1.50m, 1)
            };

            var emaitza = EskaeraKalkulagailua.BalidatuStocka(produktuak);

            Assert.False(emaitza.Nahikoa);
            Assert.Equal("Kafea", emaitza.Produktua.Izena);
            Assert.Equal(2, emaitza.EskatutakoKantitatea);
            Assert.Equal(1, emaitza.StockErabilgarria);
        }

        [Fact]
        public void BalidatuStocka_platerarekin_stocka_ez_du_blokeatzen()
        {
            var produktuak = new List<ProduktuaDto>
            {
                Platera(50, "Menua", 12.00m, 0),
                Platera(50, "Menua", 12.00m, 0)
            };

            var emaitza = EskaeraKalkulagailua.BalidatuStocka(produktuak);

            Assert.True(emaitza.Nahikoa);
        }

        [Fact]
        public void BalidatuStocka_editatzen_hasierako_kantitatea_kontuan_hartzen_du()
        {
            var produktuak = new List<ProduktuaDto>
            {
                Produktua(1, "Kafea", 1.50m, 1),
                Produktua(1, "Kafea", 1.50m, 1),
                Produktua(1, "Kafea", 1.50m, 1)
            };
            var hasierakoak = new Dictionary<int, int> { { 1, 2 } };

            var emaitza = EskaeraKalkulagailua.BalidatuStocka(produktuak, hasierakoak);

            Assert.True(emaitza.Nahikoa);
        }

        [Fact]
        public void EraikiZerbitzua_mahaitik_produktu_eta_platerarekin_dto_osoa_sortzen_du()
        {
            var data = new DateTime(2026, 5, 6, 12, 30, 0);
            var produktuak = new List<ProduktuaDto>
            {
                Produktua(1, "Kafea", 1.50m, 10),
                Platera(20, "Entsalada", 8.50m, 999)
            };

            var zerbitzua = EskaeraKalkulagailua.EraikiZerbitzua(produktuak, 2, null, data);

            Assert.Equal(2, zerbitzua.MahaiakId);
            Assert.Equal(10.00m, zerbitzua.PrezioTotala);
            Assert.Equal(2, zerbitzua.Eskaerak.Count);
            Assert.False(zerbitzua.Eskaerak[0].IsPlatera);
            Assert.True(zerbitzua.Eskaerak[1].IsPlatera);
            Assert.All(zerbitzua.Eskaerak, e => Assert.Equal(data, e.Data));
        }

        [Fact]
        public void EraikiZerbitzua_barratik_mahaia_sei_erabiltzen_du()
        {
            var produktuak = new List<ProduktuaDto>
            {
                Produktua(1, "Kafea", 1.50m, 10)
            };

            var zerbitzua = EskaeraKalkulagailua.EraikiZerbitzua(produktuak, 6);

            Assert.Equal(6, zerbitzua.MahaiakId);
            Assert.Single(zerbitzua.Eskaerak);
        }

        [Fact]
        public void EraikiZerbitzua_deskontatutako_totala_erabiltzen_du()
        {
            var produktuak = new List<ProduktuaDto>
            {
                Produktua(1, "Kafea", 3.00m, 10),
                Produktua(2, "Tarta", 4.00m, 10)
            };

            var zerbitzua = EskaeraKalkulagailua.EraikiZerbitzua(produktuak, 1, 5.00m);

            Assert.Equal(5.00m, zerbitzua.PrezioTotala);
        }

        [Fact]
        public void SortuProduktuaEskaeratik_katalogoan_badago_katalogokoa_itzultzen_du()
        {
            var katalogokoa = Produktua(1, "Kafea", 1.50m, 4);
            var eskaera = new EskaeraDto { ProduktuaId = 1, Izena = "Kafe zaharra", Prezioa = 2.00m, IsPlatera = false };

            var produktua = EskaeraKalkulagailua.SortuProduktuaEskaeratik(eskaera, new[] { katalogokoa }, 0);

            Assert.Same(katalogokoa, produktua);
        }

        [Fact]
        public void SortuProduktuaEskaeratik_katalogoan_ez_badago_eskaerako_datuekin_sortzen_du()
        {
            var eskaera = new EskaeraDto { ProduktuaId = 9, Izena = "Ura", Prezioa = 2.00m, IsPlatera = false };

            var produktua = EskaeraKalkulagailua.SortuProduktuaEskaeratik(eskaera, new List<ProduktuaDto>(), 3);

            Assert.Equal(9, produktua.Id);
            Assert.Equal("Ura", produktua.Izena);
            Assert.Equal(2.00m, produktua.Prezioa);
            Assert.Equal(3, produktua.Stock);
            Assert.False(produktua.IsPlatera);
        }

        [Fact]
        public void SortuProduktuaEskaeratik_platera_katalogoan_ez_badago_stock_handiarekin_sortzen_du()
        {
            var eskaera = new EskaeraDto { ProduktuaId = 30, Izena = "Paella", Prezioa = 14.00m, IsPlatera = true };

            var produktua = EskaeraKalkulagailua.SortuProduktuaEskaeratik(eskaera, new List<ProduktuaDto>(), 0);

            Assert.Equal(999, produktua.Stock);
            Assert.True(produktua.IsPlatera);
        }

        private static ProduktuaDto Produktua(int id, string izena, decimal prezioa, int stock)
        {
            return new ProduktuaDto
            {
                Id = id,
                Izena = izena,
                Prezioa = prezioa,
                Stock = stock,
                IsPlatera = false
            };
        }

        private static ProduktuaDto Platera(int id, string izena, decimal prezioa, int stock)
        {
            return new ProduktuaDto
            {
                Id = id,
                Izena = izena,
                Prezioa = prezioa,
                Stock = stock,
                IsPlatera = true
            };
        }
    }
}
