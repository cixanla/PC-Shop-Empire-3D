# PC Shop Empire 3D – Game Design Bible

**Belge sürümü:** 0.1  
**Durum:** Ön üretim vizyonu; kullanıcı kararları ve prototip verileriyle yaşayan belge  
**Çalışma adı:** PC Shop Empire; final ad açık karardır.

## 1. Yüksek konsept

Oyuncu küçük bir garajda sınırlı para, raf ve ekipmanla teknoloji işi kurar. Ürün sipariş eder, fiziksel teslimat kabul eder, kutuları açıp depolar ve raflara dizer, müşterilere doğru ürünü önerir, kasa ve iadeleri yönetir. Aynı işletmenin atölyesinde müşteriye özel bilgisayar toplar; arıza teşhis eder, servis ve yenileme işi yapar. Zamanla çalışan, tedarikçi, finans, itibar ve yeni lokasyonlar yöneterek çok bölümlü bir teknoloji mağazası imparatorluğuna ulaşır.

Oyun iki yarının yan yana konulması değildir. Her parça ve her işlem aynı sistemde yaşar:

> Tedarikçi → taşıma → kabul → depo/raf/rezervasyon → satış veya montaj → test → müşteri → garanti/servis → ikinci el/geri dönüşüm.

## 2. Oyuncu vaadi

“Bu dükkânı ellerimle kurdum; sattığım sistemi neden seçtiğimi biliyorum; müşteriler bana yalnız ucuz olduğum için değil, güvendikleri için dönüyor.”

Oyuncu şu üç duyguyu düzenli yaşamalı:

1. **Fiziksel sahiplik:** Kutunun, rafın, tezgâhın ve mağaza düzeninin gerçekten kendisine ait olması.
2. **Teknik ustalık:** Bir PC'nin neden doğru, hızlı, sessiz, kararlı veya sorunlu olduğunu anlayabilmek.
3. **İşletme büyümesi:** Kendi yaptığı işi doğru insan, araç ve politikaya devredip daha değerli kararlar vermek.

## 3. Tasarım sütunları

### 3.1 Fiziksel fakat akıcı

Ürünler dünyaya gelir, taşınır, yerleştirilir ve satılır. Buna rağmen oyun tam fizik simülasyonunun jank'ine teslim olmaz. Kutu ve mobilyada serbest taşıma; PC montajı, kasa ve hassas işlerde snap, IK ve yönlendirilmiş animasyon kullanılır.

### 3.2 Gerçekçi fakat öğretilebilir teknik derinlik

Temel uyumluluk gerçeğe benzer; oyun bunu ezber sınavına çevirmez. Her blokajın nedeni ve çözüm yolu açıklanır. İleri ayrıntılar oyuncu büyüdükçe açılır.

### 3.3 Açıklanabilir ekonomi ve sonuç

Oyuncu “neden zarar ettim?”, “neden müşteri memnun olmadı?”, “neden teknisyen durdu?” sorularının cevabını bulabilmelidir. Gizli ceza, reload ile değişen sonuç ve hile yapan rakip yoktur.

### 3.4 Angaryayı değil kararı büyüt

İlk kez yapılan iş ayrıntılıdır. Ustalık, araç, şablon ve çalışan tekrar eden güvenli adımları hızlandırır. Oyuncu büyüdükçe aynı sayıda kutuyu daha hızlı taşımak yerine yeni bir operasyon problemi çözer.

### 3.5 Güven, ana uzun vadeli para birimi

Doğru tavsiye kısa vadede daha düşük sepet bırakabilir; fakat sadakat, tavsiye, kurumsal iş, düşük iade ve daha güçlü tedarikçi ilişkisi yaratır. Reklam görünürlük sağlar, güven satın almaz.

### 3.6 Sistem güvenilirliği bir özellik

Save kaybı, kaybolan ürün, takılan çalışan ve çözülemez iş yalnız “bug listesi” değildir; oyuncunun işletmesine olan bağını bozar. Save/recovery, çözüm kanıtı, görev watchdog ve Guardian ilk günden mimariye girer.

## 4. Tasarım karşıtı ilkeler

Oyun şunlara dönüşmemeli:

- Aynı kutuyu yüzlerce kez taşıma işi.
- Tek puanı yüksek PC üretme fabrikası.
- Rarity/loot renklerinin teknik değerin yerine geçtiği ekonomi.
- Dashboard'dan her işi tek tıkla yapan tablo oyunu.
- Çalışanın ürün kaybettiği veya sebepsiz durduğu bir pathfinding demosu.
- Her yeni mağazanın yalnız daha büyük ve aynı olduğu bir genişleme zinciri.
- İflasın tek kötü gün veya gizli ücretle aniden geldiği ceza oyunu.
- Birkaç hazır asset ve genel AI görselinden oluşan kimliksiz simulator.
- Çıkışta vaat edilen fakat tamamlanmamış dev özellik listesi.

## 5. Hedef oyuncu

### Birincil

- Shop simulator, tycoon ve management seven oyuncular.
- PC donanımına meraklı fakat uzman olması gerekmeyen oyuncular.
- Fiziksel “iş yapma” hissi ile stratejik büyümeyi birlikte isteyen oyuncular.
- Uzun süre tek kayıtla bağ kurmayı sevenler.

### İkincil

- Gerçek PC toplama/servis sürecini güvenli biçimde öğrenmek isteyenler.
- Dekorasyon ve mağaza düzeni seven cozy oyuncular.
- Ekonomi optimizasyonu ve sistemik challenge arayan uzman oyuncular.

### Katmanlama kuralı

Yeni başlayan, doğru soruyu sorup oyunun neden açıklamasını izleyerek başarılı olabilir. Uzman oyuncu daha az yardımla daha dar marj, termal/gürültü hedefi ve gelişmiş BIOS/OC seçeneklerinden avantaj çıkarır. Bilgi sahibi olmak ödül verir; gizli bilgiye sahip olmamak cezalandırmaz.

## 6. Platform, kamera, sanat ve ses

- Ad: `PC Shop Empire` yalnız çalışma adıdır; final ad marka araştırmasından sonra kilitlenecek.
- Ana platform: Windows 10/11 64-bit, Steam.
- Sonraki olası platform: macOS; Windows 1.0 ve bütçe sonrası.
- Linux: yerel sürüm vaadi yok; Proton/Steam Deck daha sonra best-effort.
- Kamera: birinci şahıs.
- Beden: görünür eller; tutulan nesne, alet ve montaj animasyonları.
- Karakter: sessiz, isim ve görünüm seçenekli kurucu.
- Sanat: inandırıcı ölçü/malzeme/ışık; hafif stilize insan ve kurgusal ürün.
- Ses: ortam HVAC, kapı, kutu, plastik/metal, fan, kasa, bildirim ve mağaza akustiği; tüm önemli bilgi metin/görselle de verilir.
- Çıkış dili: insan kontrollü Türkçe ve İngilizce. Diğer diller talep/bütçe verisiyle.

### Ticari model

- Premium, tek seferlik satın alma.
- Reklam, abonelik, loot box, gacha ve mikro ödeme yok.
- Hata düzeltmeleri ve temel iyileştirmeler ücretsiz.
- Yalnız temel oyun eksiksiz olduktan sonra gerçekten büyük, bağımsız genişleme için ücretli DLC düşünülebilir; ana vaat DLC'ye bölünmez.

## 7. Dünya ve kariyer yapısı

Oyun tek kesintisiz açık dünya yerine ayrıntılı iş lokasyonlarından oluşur. Şehir haritası; mağaza, depo, servis merkezi, tedarikçi ve ileride şube arasında geçiş sağlar. V1'de araç sürüşü yok; teslim ve seyahat süreleri takvim/harita kararıdır.

### Aşama 1 – Garaj Atölyesi

**Kimlik:** Kurucunun her şeyi yaptığı, dar ve kişisel başlangıç.

- Düşük kira veya aile mülkü benzeri küçük sabit gider.
- Tek küçük vitrin/raf alanı.
- Bir montaj masası, basit test köşesi.
- Manuel teslimat ve kasa.
- Az müşteri, çok yüz yüze ilişki.
- Temel parçalar, kablo/sarf ve birkaç periferik.
- İtibar yerel çevre ve tavsiye ile oluşur.

Yeni zorluk: nakit ve alan; yanlış stok bağlama ciddi ama kurtarılabilir.

### Aşama 2 – Mahalle Bilgisayar Dükkânı

**Kimlik:** Perakende ve servis aynı anda çalışmaya başlar.

- Ayrı satış alanı, depo ve atölye.
- Kasa kuyruğu, daha fazla walk-in müşteri.
- İlk çalışan ve vardiya kararı.
- Farklı tedarikçi, vade ve teslimat penceresi.
- İade/garanti yükü ve sadık müşteriler.
- Online siparişin temeli.

Yeni zorluk: oyuncu atölyedeyken mağazanın kapsanması; stok doğruluğu ve görev önceliği.

### Aşama 3 – Gelişmiş Teknoloji Mağazası

**Kimlik:** Bölümlere ayrılan profesyonel teknoloji perakendesi.

- PC parçaları, hazır sistem, periferik, ekran ve servis bölümleri.
- Daha büyük teslimat, palet ve receiving alanı.
- Satış, kasa, teknisyen, depo ve temizlik ekipleri.
- Kurumsal teklif, okul/ofis ve e-spor işleri.
- İkinci el/refurbished bölümü.
- Yerel rakipler ve tedarikçi anlaşmaları.
- Pazarlama kampanyası ve etkinlikler.

Yeni zorluk: çalışan politikası, kategori sermayesi, talep tahmini ve servis SLA.

### Aşama 4 – Amiral Mağaza

**Kimlik:** Çok bölümlü, topluluk ve marka merkezi.

- Birden çok kasa/servis bankosu/atölye.
- Showroom, build clinic, etkinlik alanı.
- Merkezi depo veya ayrı servis merkezi bağlantısı.
- Yöneticiler, vardiya planı ve departman KPI'ları.
- Büyük kurumsal sözleşmeler ve teknoloji lansmanı.
- Şube/önceki lokasyonları uzaktan yönetme.

Yeni zorluk: sermaye, organizasyon, kriz dayanıklılığı, marka tutarlılığı ve güven kalitesi.

### Eski lokasyonlar

Oyuncu eski yeri:

- Satabilir veya kiradan çıkabilir.
- Kapatabilir.
- Depo, servis, online fulfillment veya ikinci el merkezine çevirebilir.
- Yönetici atayarak hafif simülasyonda işletebilir.

Bu bir “her şeyi aynı anda canlı simüle et” sistemi değildir. Aktif lokasyon tam fiziksel; uzaktaki lokasyon aynı domain kurallarını daha düşük sunum ayrıntısıyla çalıştırır.

## 8. Döngü hiyerarşisi

### 8.1 Anlık döngü – 5–60 saniye

- Bak → yaklaş → etkileşimi oku.
- Al/taşı/aç/yerleştir.
- Müşteriye soru sor/ürün göster.
- Parça tak/kablo bağla/test başlat.
- Sonucu ses, el animasyonu, durum etiketi ve kısa neden metniyle gör.

### 8.2 İş döngüsü – 2–15 dakika

- Teslimat kabul et ve stokla.
- Rafı fiyatla/doldur.
- Müşteri talebini anlayıp satışı tamamla.
- Bir özel PC aşamasını bitir.
- Bir servis teşhis veya onarım aşamasını tamamla.

### 8.3 Gün döngüsü – hedef 25–30 dakika, geçici

1. Hazırlık.
2. Açılış ve ilk teslimat.
3. Müşteri/raf/kasa/atölye dengelemesi.
4. Yoğunluk penceresi veya olay.
5. Son işler ve mağaza kapatma.
6. Kasa mutabakatı, backlog ve ertesi gün planı.

### 8.4 Hafta döngüsü

- Maaş, kira, faturalar ve vergi karşılığı.
- Vardiya ve eğitim.
- Kampanya, pazar trendi ve tedarikçi görüşmesi.
- Stok sayımı ve kategori performansı.
- Servis SLA ve garanti maliyeti.
- Büyük sipariş veya etkinlik.

### 8.5 Kariyer döngüsü

- İş modelini güçlendir.
- İtibar/sermaye/operasyon yeterliliği oluştur.
- Yeni lokasyon veya bölüm aç.
- Yeni müşteri, ürün ve risk katmanı öğren.
- Rutin işi çalışan/politikaya devret.
- Daha büyük ve nitelikli problemi çöz.

## 9. Örnek tam oyun günü

### 08:00–09:00 Hazırlık

- Dashboard'da nakit, vade ve kritik stokları kontrol et.
- Teslimat penceresini ve servis randevusunu gör.
- Çalışan vardiya/alan/önceliğini ayarla.
- Raf eksiklerini doldur; kasa ve atölyeyi hazırla.
- Mağazayı erken veya zamanında aç.

### Açılış

- Kurye gelir; purchase order ile kutu/palet sayılır.
- Hasarlı kutu foto/etiket durumu soyut kanıtla kaydedilir.
- Kabul edilen ürün authoritative stok ledger'ına girer.
- Hızlı satılacak ürün rafa, özel işe ayrılan parça rezerv alana gider.

### Müşteri akışı

- Bazıları ürünü bilerek gelir; bazıları yardım ister.
- Oyuncu bütçe, amaç, mevcut sistem ve aciliyeti sorar.
- Öneri; fiyat, uyum, garanti ve kullanım sonucu ile açıklanır.
- Müşteri satın alır, erteler, alternatif ister veya özel PC teklifi açar.

### Atölye

- Kabul edilmiş özel işin kiti hazırlanır.
- Oyuncu ESD/araç kontrolü sonrası montaja başlar.
- İş devam ederken mağaza kapsaması yoksa müşteri kabulü geçici sınırlandırılabilir; oyuncu kontrol dışı cezalandırılmaz.
- OS/driver/test çalışırken oyuncu kısa mağaza işine dönebilir.

### Yoğunluk/olay

- Öğle/iş çıkışı yoğunluğu.
- Geç teslimat, hasarlı paket, arıza veya beklenmedik kurumsal talep gibi açıklanabilir olay.
- Olayın ön sinyali, etki alanı ve seçenekleri vardır; saf para zarı değildir.

### Kapanış

- Yeni müşteri alımı durur, içeridekiler tamamlanır.
- Kasa ve satış ledger'ı mutabık hâle gelir.
- Yarım PC güvenli work-in-progress durumunda kaydedilir.
- Günlük rapor: satış, COGS, brüt kâr, işçilik, iade, stok farkı, müşteri nedeni ve yarınki vadeler.

## 10. Oyuncu, hareket, kamera ve etkileşim

### Hareket

- Yürüme, isteğe bağlı kısa koşu, çömelme.
- Baş salınımı, motion blur, kamera sarsıntısı ve FOV ayarlanabilir/kapatılabilir.
- Ağır yük hareketi yavaşlatır; aşırı yavaş ceza yerine araba/palet çözümü öğretir.
- Dar alanda nesneyle dönme ve kapı geçişi snap/ghost yardımına sahiptir.

### Etkileşim dili

- Tek odak reticle; nesne adı, ana eylem ve durum.
- Tap/hold seçenekleri.
- İkincil eylem menüsü yalnız gerçekten alternatif varsa.
- Renk yanında ikon, metin ve şekil.
- Kritik/hatalı eylemde önce neden; geri döndürülemez eylemde onay.

### Eller ve araçlar

- Boş el, kutu/ürün tutma, scanner, tornavida, kablo bağı, temizlik ve paketleme temel seti.
- Her ürün için benzersiz tam animasyon gerekmez; sınıf tabanlı grip/pose + hero eylem animasyonu.
- Sol el destek, sağ el araç; solak kontrol/görsel seçeneği erişilebilirlik backlog'unda erken test edilir.

### Envanter

- Büyük PC/kutu/monitör elde veya arabada.
- Küçük sarf ve alet sınırlı kemerde.
- “PC'yi cebe koyma” yok.
- Kit kutusu, belirli müşteri işine ayrılmış parçaları tek taşıma biriminde düzenler; içerik ledger'da görünür.

## 11. Mağaza düzenleme ve alanlar

### İşlevsel bölgeler

- Vitrin/showroom.
- Parça/periferik rafları.
- Kilitli yüksek değer vitrini.
- Kasa ve kuyruk.
- Servis kabul/teslim bankosu.
- Depo ve receiving.
- PC montaj tezgâhı.
- Teşhis/test/burn-in alanı.
- Ofis/Dashboard terminali.
- Çalışan dinlenme/dolap.
- Temizlik ve atık/e-waste alanı.
- Güvenlik/CCTV.

### Yerleştirme sistemi

- Serbest taşıma ve döndürme.
- Grid, yüzey, kenar ve hizalama snap'i ayrı aç/kapat.
- Şeffaf ghost ve ölçü.
- Geçiş genişliği, kapı açılımı, müşteri/çalışan erişimi ve servis mesafesi doğrulaması.
- “Geçerli ama kötü” yerleşimde uyarı; gerçekten kullanılamaz yerleşimde blokaj.
- Dekor daha serbest; iş ekipmanı daha katı.
- Taslak modunda mağaza kapalıyken toplu düzenleme ve geri al/yinele.
- Kaydedilebilir yerleşim şablonları ileride QoL.

### Dekorun etkisi

Dekor tek “güzellik yüzdesi” vermez. Etkiler sınırlı ve okunabilir:

- Bölüm bulunabilirliği.
- Aydınlık/konfor.
- Marka tutarlılığı.
- Bekleme alanı konforu.
- Akustik/gürültü.
- Temizlik kolaylığı.

Pahalı dekor kötü fiyat/servisi maskeleyemez.

### Güvenlik

- Görüş alanı, kilitli vitrin, alarm kapısı, CCTV ve görevli.
- Hırsızlık sürekli arcade kesintisi değil; stok değeri, güvenlik açığı ve yoğunlukla ilişkili nadir olay.
- Yanlış pozitif kovalamaca yok; kanıt ve prosedürle yönetilir.

## 12. Ürün, stok ve teslimat

### Ürün sınıfları

1. **PC çekirdeği:** CPU, anakart, RAM, GPU, SSD, HDD, PSU, kasa, soğutucu, fan.
2. **Periferik:** monitör, klavye, mouse, kulaklık, webcam, mikrofon, hoparlör, gamepad.
3. **Ağ/bağlantı:** router, switch, Wi‑Fi adaptör, kablo, dönüştürücü, hub.
4. **Sarf/araç:** termal macun, temizleyici, kablo bağı, vida, ESD ekipmanı, paket malzemesi.
5. **Hazır sistem:** masaüstü, mini PC, iş istasyonu.
6. **Servis/ikinci el:** trade-in cihazı, donor parça, refurbished ürün.

### Ürün veri boyutları

- Stable product ID ve nesil.
- Kurgusal marka/seri/model.
- Kategori ve fiziksel boyut.
- Tedarik maliyeti, önerilen pazar aralığı, vergi sınıfı.
- Teknik özellik ve uyumluluk.
- Performans profili.
- Kalite, arıza riski, garanti ve tedarikçi desteği.
- Kondisyon ve kozmetik grade.
- Kutu hacmi/ağırlığı.
- Talep segmenti ve trend duyarlılığı.
- Asset/provenans kaydı.

### Fiziksel temsil katmanları

- Yüksek değerli ve aktif iş parçası: tekil seri kimliği.
- Normal raf ürünü: gerektiğinde tekil satış birimi, kapalı kutuda batch.
- Düşük değerli sarf: batch/adet.
- Kapalı palet/koli: içerik ledger + fiziksel container.

Bu sayede 500 ürünü binlerce aktif rigidbody olarak simüle etmek gerekmez.

### Teslimat akışı

1. Purchase order.
2. Tedarikçi onayı ve ETA aralığı.
3. Kargo olayı: zamanında/geç/hasarlı/eksik, önceden tanımlı olasılık ve kanıt.
4. Fiziksel geliş.
5. Sayım, dış hasar ve belge kontrolü.
6. Kabul, şartlı kabul veya ret/claim.
7. Depo lokasyonu.
8. Raf veya iş rezervasyonu.

Receiving kapasitesi yetersizse oyun sipariş öncesi uyarır; teslimat gelince sürpriz deadlock yaratmaz.

### Stok doğruluğu

Dashboard “sayı” ile fiziksel dünya ayrışırsa bu bir oyun olayı değil, sistem hatasıdır. Her hareket sahiplik ve konum transferidir:

`Container/Location A → görev rezervasyonu → taşıyan aktör → Container/Location B`

Kesintide ürün son güvenli sahipte kalır. Çalışan kovulunca elindeki ürün silinmez; güvenli bırakma işlemi yapılır.

## 13. Müşteri yapay zekâsı

### Müşteri profili

- Bütçe ve ödeme esnekliği.
- Satın alma amacı.
- Teknik bilgi.
- Sabır ve zaman baskısı.
- Fiyat, kalite, garanti ve marka hassasiyeti.
- Görünüm/gürültü/enerji tercihi.
- Pazarlık eğilimi.
- Sadakat ve geçmiş deneyim.
- İade/şikâyet davranışı.
- Özel sipariş ve servis ihtimali.

### Bilgi modeli

Oyuncu bütün profili doğrudan görmez. Bilgi üç kaynaktan açılır:

1. Müşterinin söylediği.
2. Oyuncunun doğru sorusu.
3. Mevcut cihaz/iş yükü/test gibi kanıt.

Rahat zorlukta güçlü soru önerileri ve risk uyarısı; uzman modda daha az yönlendirme vardır. Asıl gereksinim hiçbir modda rastgele gizlenmez.

### Durum akışı

`Dışarıda → giriş → yön bulma → göz atma → ürün bulma/yardım isteme → değerlendirme → kasa veya teklif → çıkış → olası takip/yorum/iade`

Her durumda timeout ve güvenli alternatif vardır. Raf taşındığında müşteri eski koordinata sonsuza kadar yürümez; ürün stable ID ile yeniden çözülür.

### Müşteri karar puanı

Tek bir gizli sayı yerine açıklanabilir bileşenler:

- İhtiyaç uygunluğu.
- Fiyat/adillik.
- Bekleme ve hizmet hızı.
- Danışmanlık güveni.
- Stok/teslim güvenilirliği.
- Mağaza konforu.
- Geçmiş ilişki.

Satın almama nedeni oyuncuya doğal cümle/etiketle döner: “Bu kasa masama sığmıyor”, “Garanti süresi benim için kısa”, “Bekleme sürem doldu”, “Önerilen PSU yükseltme planıma yetmiyor.”

### Kalıcı müşteriler

- Hikâye veya yüksek değerli iş müşterileri kalıcı.
- Garanti/servis/özel build müşterileri ilişki kaydı taşır.
- Sıradan walk-in müşteriler hafif kalır; anlamlı etkileşim yaşarsa kalıcıya terfi edebilir.
- Müşteri geçmişi mahremiyet dostu soyut veri içerir; kişisel hassas içerik yok.

### Pazarlık

- Her müşteride zorunlu mini oyun değil.
- Büyük sepet, ikinci el, özel PC veya kurumsal işte anlamlı.
- İndirim yerine garanti, teslim, aksesuar veya servis paketi takası olabilir.
- Söz verilen her şey yazılı teklife girer ve maliyet doğurur.

## 14. Kasa, ödeme, iade ve garanti

### Kasa

- Ürünü barkodla, sepeti doğrula, ödeme yöntemini tamamla, fiş/fatura ve garanti kaydı.
- Başlangıçta oyuncu yapar; ileride kasiyer/self-checkout.
- Nakit varsa erişilebilir para üstü; zorluk ayarı hızlı/doğrudan hesap seçebilir.
- Her satış tek atomic transaction; stok düşüşü, para, vergi, COGS, seri ve garanti aynı commit'te.

### İade

İade nedeni:

- Fikir değişikliği/politika kapsamı.
- Ayıplı ürün.
- Yanlış tavsiye.
- Uyumsuzluk.
- Taşıma/kurulum hasarı.
- Kötüye kullanım.

Sonuç:

- Tam iade.
- Değişim.
- Servis/onarım.
- Kısmi indirim.
- Haklı ret ve açıklama.

Oyun gerçek bir ülkenin tüketici hukukunu birebir öğretmez; kurgusal ama adil ve açık kuralları kullanır. Gerçek ilhamda AB, ayıplı üründe satıcı sorumluluğu ve onarım/değişim gibi çözümleri vurgular: [AB tüketici garantileri](https://europa.eu/youreurope/business/selling-in-eu/consumer-contracts-guarantees/consumer-guarantees/index_en.htm).

### Garanti

- Ürün, işçilik ve ek servis sözleri ayrı.
- Seri, satış tarihi ve kapsam kaydı.
- Tedarikçi RMA ile mağazanın müşteriye karşı sorumluluğu aynı şey değil.
- İyi QA daha az garanti maliyeti yaratır.
- Garanti reddi kolay para kazanma yöntemi olamaz; itibar ve kanıt sonucu vardır.

## 15. Fiziksel PC toplama

### Başlangıç: teklif

Müşterinin işi şunları içerir:

- Kullanım amacı ve öncelik sırası.
- Sert bütçe ve hedef bütçe.
- Teslim tarihi.
- Boyut/görünüm/gürültü.
- Mevcut/yeniden kullanılacak parça.
- Yükseltme beklentisi.
- Garanti ve kabul metrikleri.
- Depozito ve değişiklik onayı.

İş üreticisi veya teklif ekranı en az bir geçerli çözüm, erişilebilir tedarik ve olası teslim planı bulmadan işi kesinleştirmez.

### Parça hazırlama

- Ürünler stoktan fiziksel olarak seçilir.
- İşe rezerv edilir ve kit kutusuna konur.
- Yanlış seri/iş parçası seçilirse sistem okunaklı uyarır; oyuncu bilinçli override'ı ancak uygun durumda yapabilir.
- ESD ve doğru araç hazırlığı kalite/risk etkiler.

### Montaj sırası

Sistem tek katı sıra istemez. Teknik bağımlılıklar izin verdiği sürece alternatif sıralar geçerlidir. Örnek:

1. Kasa ve panel hazırlığı.
2. Anakart üzerinde CPU, termal arayüz, soğutucu/bracket, RAM, M.2.
3. Standoff ve anakart montajı.
4. PSU, depolama, fan/radyatör.
5. GPU ve ek kartlar.
6. Güç/data/front-panel/fan/RGB bağlantıları.
7. Kablo düzeni ve clearance kontrolü.
8. İlk POST.
9. Firmware/ayar.
10. Kurgusal OS, sürücü ve güncelleme.
11. Benchmark/stabilite/termal/gürültü.
12. QA, temizlik, belge, paketleme.

Adımlar “görev listesi” değil, parça grafiğinin durumundan tamamlanır. Oyuncu RAM'i tutorial söylemeden önce taktıysa adım otomatik tanınır.

### Uyumluluk katmanları

**Temel:**

- CPU socket ve chipset.
- RAM nesli/türü/slot/kapasite.
- Anakart ve kasa form factor.
- GPU/soğutucu/radyatör fiziksel boyutu.
- PSU watt, ray/headroom ve konektör.
- Depolama arayüzü/slot.
- Termal kapasite.

**Orta:**

- BIOS/firmware gereksinimi.
- PCIe lane paylaşımı.
- Fan/header/hub kapasitesi.
- RAM profil kararlılığı.
- Airflow yönü ve basınç.
- Gürültü hedefi.

**İleri/opsiyonel:**

- Overclock/undervolt.
- Timing ve fan curve.
- Gelişmiş sıvı soğutma.
- Güç transient ve ayrıntılı termal tuning.

### Hata ve kalite

- İmkânsız takma bloklanır.
- Yanlış ama fiziksel olarak mümkün bağlantı, açık uyarıya rağmen yapılırsa POST/kararlılık/hasar sonucu olabilir.
- Zorlamak, düşürmek, yanlış macun/soğutucu teması veya gevşek konektör dereceli risk taşır.
- Hata sonucu “PC kötü” değil; kanıt: no POST, thermal throttle, intermittent reset, gürültü, kablo sürtmesi, benchmark düşüşü.

### Benchmark

Kurgusal araçlar şu profilleri ölçer:

- Gaming: resolution/fps sınıfı.
- Productivity: compile/render/multitask.
- Thermals: peak/sustained ve throttling.
- Noise: idle/load.
- Power: idle/load/headroom.
- Stability: kısa test + gerektiğinde burn-in.
- Storage: throughput/latency/capacity.

Müşterinin kabulü bütün skorların en yükseği değil, kendi amacının eşiğidir.

### Ustalık ve otomasyon

- İlk montajlarda vida/kablo ve açıklama ayrıntılı.
- Ustalıkla doğru vida otomatik seçme, toplu sabitleme, kablo rotası şablonu, OS image ve test preset açılır.
- Oyuncu isterse ayrıntılı moda dönebilir.
- Çalışan build yapabilir; kritik QA gate oyuncu veya yetkin kıdemli tarafından onaylanır.
- Dashboard hiçbir zaman fiziksel PC'yi anında üretmez.

## 16. Servis, teşhis ve bakım

### Intake

- Cihaz kimliği, görünür kondisyon ve aksesuar.
- Belirti ve ne zaman oluştuğu.
- Yeniden üretme izni.
- Veri erişimi/yedekleme/silme tercihi.
- Bütçe, aciliyet ve tahmin ücreti.
- Teslim edilen parola gerçek metin olarak gösterilmez; erişim token'ı gibi soyutlanır.

### Teşhis akışı

1. Şikâyeti doğrula.
2. Görsel ve bağlantı kontrolü.
3. Güvenli açılış/repro.
4. Kurgusal log ve sensör verisi.
5. Güç, sıcaklık ve temel test.
6. Known-good parça swap veya alt sistem izolasyonu.
7. Hipotezleri daralt.
8. Güven düzeyiyle teklif.

Oyuncu rastgele tüm PC'yi sökerek otomatik kazanmaz; gereksiz sökme süre, risk ve işçilik maliyeti yaratır. Araçlar “arızalı GPU” cevabını tek tık vermez; kanıt alanını daraltır.

### Onarım

- Müşteri onayı sonrası parça/sarf rezervasyonu.
- Fiziksel değişim, temizlik, bakım ve kablo düzeni.
- OS/driver/veri işlemi yalnız izin kapsamında.
- Onarım sonrası aynı belirtiyi yeniden üretme denemesi ve test.
- Yeni arıza veya kapsam değişiminde change order.

### Veri mahremiyeti

- Okunabilir kişisel dosya, mesaj veya fotoğraf yok.
- Veri, “iş dosyaları”, “kişisel arşiv” gibi soyut sınıf ve boyutla temsil edilir.
- Erişim log'u ve müşteri izni.
- Yeniden satışta sanitize sertifikası.
- Gerçek dünyadan ilham için güncel NIST SP 800-88 Rev.2, medya sanitizasyon programı ve doğrulanabilir yöntemleri vurgular: [NIST duyurusu](https://csrc.nist.gov/News/2025/guidelines-for-media-sanitization-rev-2).

## 17. İkinci el ve refurbished döngüsü

1. Satıcı/sahiplik ve cihaz geçmişi.
2. Hızlı dış değerlendirme ve riskli alım teklifi.
3. Teşhis ve veri sanitizasyonu.
4. Temizlik ve kozmetik işlem.
5. Arızalı parça değişimi veya donor kullanımı.
6. Benchmark ve burn-in.
7. Grade: kondisyon, performans, garanti ve açıklanmış kusur.
8. Fiyat, raf/online listeleme ve satış.

Kondisyon yalnız yüzde değildir: kozmetik, işlev, pil/SSD sağlık, gürültü, termal ve geçmiş ayrı. Satıcı ilanıyla gerçek durum arasında fark olabilir; fakat oyun risk işaretlerini ve doğrulama araçlarını verir. Reload ile durum değişmez.

## 18. Çalışan yapay zekâsı ve yönetim

### Roller

- Satış danışmanı.
- Kasiyer.
- Teknisyen.
- Depo/receiving çalışanı.
- Temizlik.
- Yönetici.
- Güvenlik.

Rol katı sınıf değildir; çalışan birincil/ikincil uzmanlığa sahip olabilir.

### Özellikler

- Rol becerileri.
- İş hızı.
- Hata eğilimi.
- Güvenilirlik/devamlılık.
- Öğrenme hızı.
- Moral ve yorgunluk.
- İletişim/teknik anlatım.
- Maaş ve beklenti.

### Görev sistemi

Her iş:

- Stable task ID.
- Kaynak ve hedef.
- Gereken beceri/araç/ürün.
- Öncelik ve deadline.
- Rezervasyon.
- İptal/rollback kuralı.
- Blocked reason.
- Completion evidence.

Çalışan görevi claim eder. Hedef taşınırsa yeniden planlar. Erişemezse sonsuza kadar yürümez; belirli sürede durur, nedeni bildirir ve güvenli biçimde yeniden kuyruğa bırakır.

### Yönetim arayüzü

- Vardiya.
- Bölge.
- Rol/izin.
- Öncelik sırası.
- Minimum stok ve sipariş bütçe sınırı.
- Müşteri/servis kabul kapasitesi.
- QA zorunluluğu.
- Ne zaman oyuncuya escalation yapacağı.

Oyuncu her kutuyu çalışana tek tek söylemez; politika verir. Yine de acil durumda doğrudan görev atayabilir.

### Eğitim

- Gözetimli görev.
- Kıdemli mentorluk.
- Eğitim modülü/sertifika.
- Gerçek iş deneyimi.
- Hata sonrası geri bildirim.

Saatlerce gerçek zaman bekleyerek seviye satın alma yok. Eğitim maaş, kapasite ve kısa dönem hız maliyeti taşır.

### Çalışan hatası

Sabit “%5 ürün yok et” zarı kullanılmaz. Hata olasılığı:

- Becerinin iş zorluğuna oranı.
- Yorgunluk ve moral.
- Araç/istasyon kalitesi.
- İş yükü ve zaman baskısı.
- QA gate.

Kritik uyumsuzluk veya güvenlik riskinde çalışan durup yardım ister. Küçük hatalar; yanlış raf, eksik etiket, yavaş işlem veya yeniden test ihtiyacı gibi kurtarılabilir sonuçlar doğurur.

## 19. Dashboard ve yönetim terminali

Dashboard kaybolmaz; fiziksel ofis bilgisayarı, tablet veya yönetim terminalinden açılır. Varsayılan olarak zamanı durdurur; isteyen oyuncu canlı simülasyonu seçebilir.

### Modüller

1. **Özet:** Nakit, bugünkü satış, kuyruk, kritik stok, teslimat, servis SLA, çalışan blocker.
2. **Tedarik:** Ürün, tedarikçi, fiyat, MOQ, ETA, güvenilirlik, garanti ve vade.
3. **Stok:** Konum, adet, rezervasyon, kondisyon, seri/batch, stok dönüşü.
4. **Fiyatlandırma:** Manuel fiyat, kategori kuralı, marj, pazar aralığı, kampanya.
5. **Finans:** Ledger, COGS, brüt kâr, nakit, vade, borç, kira, fatura, vergi karşılığı ve forecast.
6. **Personel:** İşe alım, maaş, vardiya, rol, eğitim, moral ve görev politikası.
7. **Müşteri/CRM:** Teklif, özel PC, servis, garanti, iade, sadakat ve takip.
8. **Mağaza:** Renovasyon, ekipman, bölüm KPI ve lokasyon.
9. **Marka:** Kampanya, yerel etkinlik, bilinirlik, yorum ve itibar boyutları.
10. **Anlaşmalar:** Tedarikçi ve kurumsal sözleşme.
11. **Pazar:** Trend, nesil, kıtlık/fazla, rakip ve sinyal.
12. **Servis/RMA:** Intake, onay, parça bekleme, test, teslim ve claim.
13. **Intelligence:** Nedenli tahmin, risk, kapasite ve öneri; otomatik karar değil.
14. **Faaliyet ve güvenilirlik:** Oyuncunun satış, sipariş, görev ve açık kurtarma bildirimleri. Guardian'ın ham tanı, neden zinciri ve geliştirici raporu bu ekranda gösterilmez; ayrı, oyuncuya kapalı teknik kayıttır.

### Dashboard sınırı

Dashboard şunları yapabilir:

- Sipariş ve politika oluşturmak.
- Fiyat ve vardiya ayarlamak.
- İş/teslimat/finans bilgisini göstermek.
- Teklif ve anlaşma onaylamak.

Şunları yapamaz:

- Ürünü fiziksel olarak rafa ışınlamak.
- PC'yi tek tıkla monte etmek.
- Test/servis/paketlemeyi atlamak.
- Teslimatı alınmış saymak.
- Çalışan hatasını görünmezce silmek.

## 20. Ekonomi

### Temel hesap ayrımı

- Nakit.
- Gelir.
- Satılan mal maliyeti (COGS).
- Brüt kâr.
- İşçilik.
- Kira/fatura/servis/marketing.
- Vergi karşılığı.
- Borç ve vade.
- Stok ve work-in-progress değeri.

Oyuncu satış yaptığı için zengin görünürken aslında nakit sıkışıklığı yaşayabilir; Dashboard bu ayrımı öğretir.

### Fiyatlandırma

Başlangıçta ürün etiketi tek tek veya scanner ile. Büyüdükçe:

- Kategori markup kuralı.
- Pazar medyanına göre hedef.
- Yuvarlama kuralı.
- Promosyon ve bundle.
- Minimum marj.
- İstisna listesi.

Sistem öneri ve aralık verir; oyuncu onaylamadan toplu fiyat değiştirmez. Talep/kâr forecast kesin sayı değil, güven aralığıdır.

### Pazar

Fiyat/talep etkileri:

- Ürün nesli ve lansman.
- Sezon/hafta sonu/etkinlik.
- Arz fazlası/kıtlık.
- Tedarikçi sorunu.
- Yerel rakip fiyat/itibar.
- Oyuncu kampanyası ve stok geçmişi.
- Müşteri trendi.

Şoklar bounded olur ve sinyal verir: haber, tedarikçi ETA değişimi, ön sipariş, rakip reklamı. RNG seed save'de saklanır.

### Tedarikçiler

Her tedarikçi şu trade-off'larla ayrılır:

- Birim fiyat.
- Minimum sipariş.
- Hız ve ETA güvenilirliği.
- Hasar/eksik oranı.
- RMA hızı ve garanti desteği.
- Ödeme vadesi.
- Ürün uzmanlığı.
- Kriz erişimi.
- İlişki ve exclusivity şartı.

Tek “en iyi” satıcı yok. Ucuz tedarikçi yüksek işletme sermayesi ve yavaş RMA isteyebilir; pahalı yerel dağıtıcı acil parçayı aynı gün getirebilir.

### Rakipler

Az sayıda kalıcı rakip:

- Düşük fiyat zinciri.
- Premium danışmanlık mağazası.
- Online hızlı satıcı.
- Servis uzmanı.

Rakip para hilesiyle oyuncuya yetişmez. Kapasite, uzmanlık ve stratejisine göre hareket eder. Etkisi market payı tek bar değil; müşteri seçimi, reklam, yorum, tedarikçi anlaşması ve iş piyasasında görünür.

## 21. İtibar, pazarlama ve topluluk

### İtibar boyutları

- Fiyat adaleti.
- Teknik uzmanlık.
- Hizmet hızı.
- Montaj/onarım kalitesi.
- Garanti ve sorun çözme.
- Teslim güvenilirliği.

Genel marka algısı bunların bileşimidir; her segment farklı ağırlık verir.

### Yorumlar

- Gerçek işlemlerden türetilir.
- Neden ve etki gösterir.
- Meşru yorum silinmez.
- Oyuncu yanıt, telafi, yeniden onarım veya politika değişikliği yapabilir.
- Rastgele review bombing ana ekonomi olayı değildir.

### Pazarlama

- Yerel arama ve broşür.
- Sosyal medya ürün/atölye paylaşımı.
- Topluluk build clinic.
- Açılış/lansman etkinliği.
- Kurumsal outreach.
- Sadakat/referral programı.

Kampanya hedef kitleye görünürlük getirir. Kötü stok veya hizmette daha fazla insanın kötü deneyim yaşamasına da yol açabilir.

## 22. Gün/gece, takvim ve olaylar

- Hazırlık, açık saat ve kapanış sonrası.
- Hafta sonu ve iş çıkışı yoğunluğu.
- Maaş, kira, fatura ve vade günleri.
- Kurgusal teknoloji lansmanı.
- Okula dönüş, tatil, turnuva, yerel etkinlik.
- Mevsim ışığı/hava görsel ve talep etkisi; aşırı içerik maliyeti olmadan.

Olay kuralı:

1. Ön sinyal veya makul neden.
2. Oyuncunun en az iki anlamlı cevabı.
3. Tam kayıp yerine risk azaltma fırsatı.
4. Sonuç ve neden raporu.
5. Reload ile farklı zar yok.

## 23. Görevler, hikâye ve başarımlar

### Görev türleri

- Öğretici milestone.
- Müşteri hikâye işi.
- Operasyon meydan okuması.
- Tedarik/kriz senaryosu.
- Kurumsal teklif.
- Topluluk etkinliği.
- Uzman teknik iş.
- Mağaza dönüşüm hedefi.

### Hikâye sunumu

- Tekrarlanan müşteriler.
- Çalışanların iş gelişimi.
- Rakip ve tedarikçi temsilcileri.
- E-posta/mesaj ve yüz yüze kısa sahne.
- Çevresel hikâye: fotoğraf, teslim edilen eski PC, mağaza duvarı, başarı köşesi.

Sabit konuşan kahraman ve uzun zorunlu cutscene yok. Oyun içi sistem sonucu hikâyeyi değiştirir.

### Başarımlar

Hacim yalnız doğal milestone ise kullanılır. Ana başarımlar:

- Ustalık.
- Farklı strateji.
- Zor kriz çözümü.
- Müşteri güveni.
- Sürdürülebilir iş.
- Nadir sistemik kombinasyon.

Başarım save-scum veya 5.000 kez aynı eylem istemez.

## 24. Mağaza dışı gelir alanları

### Online sipariş

- Aynı stoktan rezervasyon.
- Pick/pack/ship.
- Kargo ve iade riski.
- Fiyat kanalı ve teslim sözü.
- Çalışan/fulfillment kapasitesi.

### Kurumsal anlaşma

- Okul/ofis sistemleri.
- E-spor takımı.
- Yaratıcı stüdyo/iş istasyonu.
- Bakım SLA ve yedek cihaz.
- Milestone ödeme ve kabul testi.

### Servis sözleşmesi

- Periyodik bakım.
- Öncelikli müdahale.
- Yedek parça stoğu.
- Aylık gelir karşılığında kapasite taahhüdü.

### İkinci el/refurbished

- Daha yüksek bilgi ve risk, farklı müşteri segmenti ve marj.

### Kiralama

V1'in ileri aşamasında ancak varlık takibi, depozito, kondisyon ve dönüş test sistemi hazırsa. Kısa etkinlik PC'si veya kurumsal geçici cihaz; tüketici borç/abonelik karmaşası olmadan.

Her yeni gelir kanalı yeni ayrı mini oyun icat etmek yerine mevcut stok, iş, test, çalışan ve ledger sistemini yeniden kullanır.

## 25. İflas, zorluk ve yeniden başlama

### Kademeli başarısızlık

1. Nakit uyarısı ve forecast.
2. Harcama dondurma/indirimli sipariş iptali.
3. Vade görüşmesi veya kontrollü kısa kredi.
4. Varlık/stok satışı ve vardiya azaltma.
5. Küçük lokasyona dönüş/şube kapatma.
6. Yeniden yapılandırma dönemi.
7. Şirket kapanışı veya yeni şirket seçeneği.

Bir fatura veya rastgele olay anında kayıt silmez. Oyuncu kötü karar zincirini görebilir ve adım adım müdahale eder.

### Zorluklar

- **Rahat:** Geniş marj, uzun sabır/deadline, güçlü açıklama, düşük şok.
- **Standart:** Tasarımın ana dengesi.
- **Uzman:** Dar marj, daha karmaşık müşteri/uyumluluk, daha az öneri.
- **Hardcore:** Açıkça belirtilen daha ciddi hasar/borç/operasyon riski; yine hile yok.

Şirket zorluğu başlangıçta sabittir. Erişilebilirlik seçenekleri her zorlukta serbesttir.

### Yeniden başlama

- Aynı veya daha düşük zorluk.
- Bilgi arşivi, tutorial skip ve bazı düzen şablonları taşınabilir.
- Para, stok, çalışan, tedarikçi güveni ve mağaza avantajı taşınmaz.
- Eski kayıt korunur.

## 26. Erişilebilirlik

Başlangıç mimarisine dahil:

- Tam kontrol yeniden atama.
- Klavye/fare + controller ve doğru glyph.
- Hassasiyet, deadzone, acceleration, invert.
- Hold/toggle ve tekrarlı basış azaltma.
- FOV, head bob, camera shake, motion blur ayarı.
- UI ölçeği, büyük metin ve büyük hedef.
- Kontrast ve renk profilleri; renk yanında şekil/ikon/metin.
- Bütün konuşma ve önemli ses için altyazı/caption.
- Zaman baskısını azaltma seçenekleri.
- Hassas montaj için snap yardım gücü.
- Nakit para üstü, vida ve kablo adımlarını basitleştirme.
- Pause-anywhere; Dashboard pause.

Microsoft XAG'leri tam kontrol eşleme, çoklu zorluk, okunabilir metin, renk dışı işaret ve bütün ses bilgisinin metin karşılığını önerir: [XAG ana dizin](https://learn.microsoft.com/en-us/xbox/accessibility/guidelines), [Input](https://learn.microsoft.com/en-us/xbox/accessibility/xbox-accessibility-guidelines/107), [Difficulty](https://learn.microsoft.com/en-us/xbox/accessibility/xbox-accessibility-guidelines/108), [Subtitles](https://learn.microsoft.com/en-us/xbox/accessibility/xbox-accessibility-guidelines/104).

## 27. Tutorial ve onboarding

### İlk saat

1. Garajı ve terminali tanı.
2. Küçük ürün siparişi ver.
3. Teslim al, kutuyu aç, rafa koy ve fiyatla.
4. İlk müşteriye satış yap.
5. Basit özel PC teklifini al.
6. Parça kiti ve montajın temelini öğren.
7. POST/OS/test/paket/teslim.
8. Gün sonu raporu ve ilk serbest plan.

### Softlock karşı kuralları

- Hedefler eylem çağrısını değil world state'i kontrol eder.
- Oyuncu adımı önceden yaptıysa kabul edilir.
- Alternatif geçerli sıra desteklenir.
- Her tutorial bölümü reset/skip/replay.
- Gerekli ürün satıldı/kaybolduysa ücretsiz tutorial recovery veya yeniden tahsis.
- İş üretmeden önce çözüm kanıtı.
- Erişilemeyen UI için klavye/controller geri dönüş yolu.

## 28. Kayıt sistemi

- Yerel offline öncelikli.
- Birden çok şirket profili.
- Manuel save ve döner autosave.
- Gün başı, büyük alım, kredi, renovasyon ve kriz öncesi checkpoint.
- Geçici dosyaya yaz → doğrula → atomic replace.
- Schema version ve migration.
- Checksum ve domain invariant kontrolü.
- Bozuk nesneyi karantinaya alıp geri kalan şirketi kurtarma.
- Son sağlam checkpoint'e dönüş.
- Steam Cloud; küçük, ayrılmış dosyalar ve açık conflict ekranı.

Steam Cloud, sık ve seyrek değişen verinin ayrı küçük dosyalara bölünmesini önerir; büyük veya çok sayıda dosya upload ve kapanışı geciktirebilir: [Steam Cloud best practices](https://partner.steamgames.com/doc/features/cloud?language=english).

## 29. Hile ve exploit karşı tasarım

- RNG seed save'de; reload reroll yok.
- Satış tek transaction; fiyat satış ortasında değiştirilemez.
- Online ve mağaza aynı stoktan reservation alır.
- Return refund, seri ve orijinal işlemle bağlı.
- Personel ürün üretmez; gerçek stok transfer eder.
- Offscreen lokasyon aynı maliyet ve kuralları kullanır.
- Tedarikçi iptal/claim, fiziksel kabul kanıtına bağlı.
- Benchmark cache, donanım/ayar değişince geçersiz olur.
- Save hatasında oyuncuya para cezası değil güvenli recovery.
- Modlu save ileride ayrı işaretlenir; resmi olmayan veri başarımları etkileyebilir.

## 30. Monotonluk karşı sistemi

Bir eylem şu üç koşuldan hiçbirini sağlamıyorsa hızlandırılmalı veya çıkarılmalı:

1. Yeni beceri öğretiyor mu?
2. Anlamlı karar/risk içeriyor mu?
3. Güçlü dokunsal/duygusal ödül veriyor mu?

Araçlar:

- Batch açma/etiketleme.
- El arabası/palet.
- Kit hazırlama.
- Kaydedilmiş fiyat ve sipariş kuralı.
- OS image/test preset.
- Otomatik vida/kablo yardımcıları.
- Çalışan politikası.
- Manager exception report.
- Yoğunluk dışında zaman planlama.
- Farklı müşteri amaçları ve iş kısıtları.
- Eski müşterinin yeni hikâyesi.

## 31. Kariyer sonu ve tekrar oynanabilirlik

İlk zafer geçici olarak 40–60 saat hedeflenir:

- Amiral mağaza açılmış.
- Sürdürülebilir kârlılık.
- Yüksek ama dengeli itibar.
- Güvenilir servis/garanti.
- Yönetilebilir çalışan ve tedarik operasyonu.

Zafer sahnesi/kredi sonrası aynı kayıt endless devam eder. İsteğe bağlı New Company+; farklı şehir bölgesi, piyasa seed'i, başlangıç kısıtı veya uzmanlaşmayla yeniden başlayabilir. Ana şirket kaydı hiçbir sezonla silinmez.

## 32. Vertical slice

### Amaç

“Bu iki oyun gerçekten tek, güvenilir ve eğlenceli deneyim oluyor mu?” sorusunu kanıtlamak.

### Dahil

- Tek garaj ve teslimat dış alanı.
- First-person controller, görünür eller, taşıma.
- Terminal siparişi, fiziksel teslim, kutu, depo/raf, fiyat.
- Müşteri giriş, gezinme, basit ihtiyaç görüşmesi, kuyruk ve kasa.
- Baştan sona bir özel PC işi.
- Temel uyumluluk, OS ve çok boyutlu test.
- Paketleme ve teslim.
- Basit ekonomi/ledger/Dashboard.
- Atomik save ve recovery iskeleti.
- Guardian event/invariant/report iskeleti.
- Slice sonunda yaklaşık 50–80 anlamlı SKU; teknik prototipte daha az.

### Dahil değil

- Çalışan.
- Şube.
- Tam servis/refurbish.
- Gelişmiş rakip.
- Online/kurumsal kanal.
- Tam şehir veya araç.
- Co-op/mod/Workshop.
- macOS ticari sürümü.

### Başarı ölçütleri

- Yeni oyuncu yardım almadan ilk satış ve özel PC zincirini tamamlar.
- Alternatif geçerli montaj sırası tutorial'ı bozmaz.
- Ürün sayısı Dashboard, raf ve fiziksel dünya arasında hiçbir testte ayrışmaz.
- 20 ardışık gün simülasyonunda ekonomi NaN/deadlock üretmez.
- Save, her kritik işlem noktasından geri yüklenir.
- NPC/ürün blocker recovery veriyi kaybetmez.
- Orta sınıf hedef Windows makinede belirlenen frame-time bütçesi korunur.
- Oyuncu “neden başarılı/başarısız olduğunu” test görüşmesinde açıklayabilir.
- Fiziksel eylem ilk saatten sonra angarya olarak işaretlenmiyorsa ve tekrar isteği varsa çekirdek doğrulanır.

## 33. İçerik ölçeği hedefi

- Teknik prototip: yalnız sistemi kanıtlayan minimum ürün aileleri.
- Vertical slice: 50–80 anlamlı SKU üst sınırı, tekrar testine göre.
- Tam oyun hedefi: yaklaşık 300–500 anlamlı SKU; ürün sayısı kalite kapılarına bağlı.
- Kurgusal birkaç teknoloji nesli; eski donanım bütçe, servis ve refurb niche'inde kalır.
- Her SKU yeni karar veya ekonomik rol getirmelidir; yalnız isim/renk varyasyonu katalog başarısı sayılmaz.

## 34. GDB değişiklik kuralı

Bu belge bir vaat listesi değil, doğrulanacak ürün anayasasıdır. Yeni fikir:

1. Hangi oyuncu problemini çözüyor?
2. Hangi mevcut sistemle bağlanıyor?
3. İçerik, AI, performans, save ve test maliyeti ne?
4. Vertical slice kanıtına gerek var mı?
5. Başka bir özellik çıkarılmadan eklenebilir mi?

sorularından geçmeden çekirdek kapsama girmez.
