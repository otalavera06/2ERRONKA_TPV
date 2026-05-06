# TPV - Kaxa beltzeko probak

## Helburua

TPVko eskaeren portaera erabiltzailearen ikuspegitik balioztatzea: mahaia edo barra aukeratu, produktuak eta platerak gehitu, stocka kontrolatu, totala kalkulatu, deskontua aplikatu eta editatzen ari den eskaera eguneratu.

## Kasuak

| ID | Funtzioa | Sarrera edo ekintza | Espero den emaitza |
| --- | --- | --- | --- |
| TPV-BB-01 | Produktua gehitu | Mahaia ireki eta stocka duen produktu arrunt bat gehitu | Produktua uneko eskaeran agertzen da eta totala produktuaren prezioarekin igotzen da. |
| TPV-BB-02 | Platera gehitu | Mahaia ireki eta plater bat gehitu | Platera uneko eskaeran agertzen da eta totala plateraren prezioarekin igotzen da. |
| TPV-BB-03 | Hainbat produktu batu | Produktu eta plater bat baino gehiago gehitu | Totala produktu guztien prezioen batura da. |
| TPV-BB-04 | Eskaera hutsa | Produkturik gehitu gabe eskaera jasotzen saiatu | Aplikazioak abisua erakusten du eta ez du eskaerarik bidaltzen. |
| TPV-BB-05 | Stock nahikoa | Produktu baten eskatutako kantitatea stockaren berdina edo txikiagoa da | Eskaera baliozkoa da eta bidaltzeko prest geratzen da. |
| TPV-BB-06 | Stock gutxiegi | Produktu baten eskatutako kantitatea stocka baino handiagoa da | Aplikazioak stock faltaren abisua erakusten du eta ez du eskaera gordetzen. |
| TPV-BB-07 | Plateraren stocka | Plater bat gehitu, nahiz eta bere stock lokala 0 izan | Platera ez da stock lokalagatik blokeatzen. |
| TPV-BB-08 | Barra | Barra aukeratu eta produktu bat eskatu | Sortutako zerbitzuak `MahaiakId = 6` dauka. |
| TPV-BB-09 | Mahaia | 1etik 5era arteko mahai bat aukeratu eta produktu bat eskatu | Sortutako zerbitzuak aukeratutako mahaiaren IDa dauka. |
| TPV-BB-10 | Deskontua | Deskontatutako totala aplikatuta dagoenean eskaera eraiki | Zerbitzuaren `PrezioTotala` deskontatutako balioa da. |
| TPV-BB-11 | Editatzen dagoen eskaera | Lehendik zegoen produktu baten hasierako kantitatea kontuan hartuta eguneratu | Stock erabilgarria hasierako kantitatearekin handitzen da eta eskaera ez da oker blokeatzen. |
| TPV-BB-12 | Katalogoko produktua berreskuratu | Zerbitzu zahar bat editatzean produktua katalogoan badago | Katalogoko produktu bera erabiltzen da, stock eta datu eguneratuekin. |
| TPV-BB-13 | Katalogotik kanpoko produktua | Zerbitzu zahar bat editatzean produktua katalogoan ez badago | Produktua eskaerako datuekin sortzen da eta aurreko kantitatea stock erabilgarri gisa erabiltzen da. |
| TPV-BB-14 | Katalogotik kanpoko platera | Zerbitzu zahar bat editatzean platera katalogoan ez badago | Platera eskaerako datuekin sortzen da eta stock handia esleitzen zaio. |

## Proba automatikoak

Kasu hauek `Tpv/Testak/EskaeraKalkulagailuaTest.cs` fitxategian automatizatuta daude. Testek ez dute leihoa irekitzen; eskaeren portaera publikoa egiaztatzen dute datu sarrera eta irteeren bidez.
