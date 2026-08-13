# ADR-0013 — Okunaklı Yarı Gerçekçi Görsel Yön

**Durum:** Kabul edildi; kademeli üretim bekliyor<br>
**Tarih:** 13 Ağustos 2026<br>
**İlgili işler:** GitHub #18 ve gelecekteki bounded look-development alt işleri

## Bağlam

GarageGraybox mekanik doğrulama için bilinçli olarak basit primitive geometri ve düz renkler kullanır. Bu görünüm nihai kalite hedefi değildir ve kullanıcı tarafından fazla sentetik/yapay bulunmuştur. Projenin tek geliştirici, performans, lisans ve kapsam sınırları içinde daha inandırıcı bir dünya kurması; fakat pahalı fotogerçekçilik yarışına veya hazır-asset kolajı görünümüne girmemesi gerekir.

## Karar

Görsel hedef **okunaklı yarı gerçekçilik**tir:

- Mimari, mobilya, kutu, PC parçası, el ve etkileşim nesneleri gerçek dünyaya yakın oran ve kalınlık kullanır.
- URP Lit/PBR malzemelerinde fiziksel olarak inandırıcı albedo, metallic ve roughness ilişkisi korunur. Saf plastik renk blokları yerine yüzey türü, kenar kırılması, normal detayı ve kontrollü mikro kusur okunur.
- Sert 90° model kenarları gerektiğinde küçük bevel/normal desteği alır. Nesneler temas gölgesi, yumuşak yönlü ışık ve ölçülü sıcak-soğuk ayrımıyla zemine oturur.
- Kurgusal etiket, koli bandı, baskı, aşınma ve kullanım izi dünyayı yaşanmış gösterir; gerçek marka veya lisansı belirsiz asset kullanılmaz.
- Siluet, önemli etkileşim alanı ve durum geri bildirimi hafif stilizasyonla okunabilir kalır. Renk tek bilgi kanalı olmaz; ghost, metin, ikon/şekil ve kontrast birlikte kullanılır.
- Hareket ve etkileşimlerde doğal ağırlık, gecikme ve takip hissi hedeflenir; kontrolü bozan kamera sallantısı, serbest rigidbody titremesi veya yapay abartı kullanılmaz.
- Nihai kalite tek adımda uygulanmaz: önce bir referans garaj köşesi, ardından onaylı modüler kit ve hero asset'ler, sonra sahne geneli üretim yapılır.
- Windows x64 + Steam performansı ana kapıdır. Malzeme/ışık kalitesi LOD, instancing, texture bütçesi, ışık/bake stratejisi ve ölçülmüş frame-time ile sınırlandırılır.

## Kalite çubuğu

İlk görsel benchmark aynı karede en az beton/duvar, metal raf, ahşap/lamine tezgâh, karton kutu ve bir teknoloji nesnesi göstermelidir. Her yüzey yalnız renginden değil ışık tepkisi, roughness ve detay ölçeğinden ayırt edilebilmelidir. Ekran görüntüsü graybox'tan belirgin biçimde daha inandırıcı olmalı; gameplay hedefleri ve promptlar hâlâ ilk bakışta seçilmelidir.

## Sonuçlar

- Mevcut GarageGraybox ve prototip eller/kutular final grafik veya animasyon vaadi değildir; işlev ve regresyon kanıtıdır.
- Fotogerçekçilik, tam mağaza sanat dönüşümü, ücretli asset paketi, büyük texture indirmesi ve bütün sahneyi tek seferde yenileme bu kararla onaylanmış sayılmaz.
- İlk uygulama, mevcut Unity/URP araçlarıyla küçük ve geri alınabilir bir look-development benchmark paketi olmalıdır. Kalite ve performans kanıtlanmadan kapsam sahne geneline büyütülmez.
- Provenansı doğrulanmış özgün/modüler içerik, hazır asset miktarından daha önemlidir; R-008, R-017 ve R-020 riskleri bu kalite kapısında ölçülür.
