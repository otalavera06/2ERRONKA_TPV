# TPV - Eskaeren bideen estaldura

## Aztertutako metodoak

- `EskaeraKalkulagailua.KalkulatuTotala`
- `EskaeraKalkulagailua.BalidatuStocka`
- `EskaeraKalkulagailua.EraikiZerbitzua`
- `EskaeraKalkulagailua.SortuProduktuaEskaeratik`
- `EskaerakPantaila.BtnEskaeraJaso_Klik`
- `EskaerakPantaila.EraikiZerbitzuaSortuDto`
- `EskaerakPantaila.SortuProduktuaEskaeratik`
- `EskaerakPantaila.KalkulatuUnekoTotala`

## Bideen laburpena

| Bidea | Metodoa | Baldintza nagusia | Espero den irteera | Lotutako testa |
| --- | --- | --- | --- | --- |
| B1 | `KalkulatuTotala` | Produktu zerrenda hutsik dago | `0` itzultzen du | `KalkulatuTotala_zerrenda_hutsarekin_zero_itzultzen_du` |
| B2 | `KalkulatuTotala` | Produktu bat edo gehiago daude | Prezioen batura itzultzen du | `KalkulatuTotala_produktu_arruntekin_batura_itzultzen_du` |
| B3 | `BalidatuStocka` | Produktu arrunten kantitatea stockaren barruan dago | `Nahikoa = true` | `BalidatuStocka_stock_nahikoarekin_ondo_itzultzen_du` |
| B4 | `BalidatuStocka` | Produktu arrunten kantitatea stocka baino handiagoa da | `Nahikoa = false` eta produktuaren datuak | `BalidatuStocka_stock_gutxiegirekin_produktua_eta_kantitateak_itzultzen_ditu` |
| B5 | `BalidatuStocka` | Produktua platera da | Ez du stock lokalagatik blokeatzen | `BalidatuStocka_platerarekin_stocka_ez_du_blokeatzen` |
| B6 | `BalidatuStocka` | Editatzen ari den eskaerak hasierako kantitatea dauka | Stock erabilgarria handitzen da | `BalidatuStocka_editatzen_hasierako_kantitatea_kontuan_hartzen_du` |
| B7 | `EraikiZerbitzua` | Mahaia aukeratu da | `MahaiakId` aukeratutako mahaia da | `EraikiZerbitzua_mahaitik_produktu_eta_platerarekin_dto_osoa_sortzen_du` |
| B8 | `EraikiZerbitzua` | Barra aukeratu da | `MahaiakId = 6` | `EraikiZerbitzua_barratik_mahaia_sei_erabiltzen_du` |
| B9 | `EraikiZerbitzua` | Deskontatutako totala dago | `PrezioTotala` deskontatutako balioa da | `EraikiZerbitzua_deskontatutako_totala_erabiltzen_du` |
| B10 | `SortuProduktuaEskaeratik` | Produktua katalogoan dago | Katalogoko instantzia itzultzen du | `SortuProduktuaEskaeratik_katalogoan_badago_katalogokoa_itzultzen_du` |
| B11 | `SortuProduktuaEskaeratik` | Produktu arrunta katalogoan ez dago | Eskaerako datuekin produktua sortzen du | `SortuProduktuaEskaeratik_katalogoan_ez_badago_eskaerako_datuekin_sortzen_du` |
| B12 | `SortuProduktuaEskaeratik` | Platera katalogoan ez dago | Platera stock handiarekin sortzen du | `SortuProduktuaEskaeratik_platera_katalogoan_ez_badago_stock_handiarekin_sortzen_du` |

## Argazkiak gehitzeko lekua

| Metodoa | Kaptura |
| --- | --- |
| `KalkulatuTotala` |  |
| `BalidatuStocka` |  |
| `EraikiZerbitzua` |  |
| `SortuProduktuaEskaeratik` |  |
| `BtnEskaeraJaso_Klik` |  |
