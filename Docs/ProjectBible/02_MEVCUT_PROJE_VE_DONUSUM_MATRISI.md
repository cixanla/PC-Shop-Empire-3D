# Mevcut PC Shop Empire Denetimi ve 3D Dönüşüm Matrisi

**Denetim tarihi:** 10 Ağustos 2026; USB hash doğrulaması 11 Ağustos 2026  
**Yöntem:** Salt okunur dosya envanteri, kaynak incelemesi, test çalıştırma ve kopyalar arası karşılaştırma.  
**Güvenlik:** Eski proje, kayıt alanı ve USB üzerinde hiçbir dosya değiştirilmedi.

## Yönetici sonucu

Mevcut PC Shop Empire 1.1.6 bir oyun motoru projesi değil; Electron üzerinde çalışan tek ekranlı bir yönetim simülasyonudur. Tema ve işletme sistemlerinin çoğu değerli bir başlangıç şartnamesi sunuyor. Buna karşılık kod mimarisi, 3D sahne ihtiyacı, fizik, animasyon, NPC navigasyonu ve yeni ürün ölçeği nedeniyle “port” yaklaşımı teknik olarak yanlış olur.

Karar:

- **Konu, ilerleme omurgası, veri anlamları ve 14 Dashboard bölümü korunur.**
- **Kod, HTML/CSS arayüz, görseller ve marka/model kataloğu taşınmaz.**
- Eski oyun, “legacy davranış şartnamesi” olarak arşivlenir.
- Yeni Unity projesi ayrı bir kaynak zinciri ve veri şemasıyla başlar.

## İncelenen kaynaklar

### Ana yerel inceleme kopyası

`/Users/cixanla/Documents/Codex/2026-08-02/new-chat/work/transfer_review/CIXANLA/02_PC_SHOP_EMPIRE_MASAUSTU_1.1.6/KAYNAK_KODU`

### Doğrulanmış canonical USB legacy kaynağı

`/Volumes/cixanla/CIXANLA/02_PC_SHOP_EMPIRE_MASAUSTU_1.1.6/KAYNAK_KODU`

USB ve ana yerel inceleme kopyası 26/26 dosyada; göreli yol, boyut ve SHA-256 düzeyinde birebir eşleşir. Karşılaştırma yalnız okuma ile yapıldı.

### Sonraki macOS çalışma kopyası

`/Users/cixanla/Documents/Codex/2026-08-02/new-chat/work/builds/pc-shop-empire-mac`

Ana oyun dosyaları (`game.js`, `index.html`, `main.js`, `preload.js`, `src/*`, testler ve görseller) canonical kaynakla SHA-256 düzeyinde eşleşiyor. `THIRD_PARTY_NOTICES.txt`, `forge.config.js`, `package-lock.json`, `package.json` ve `styles.css`; macOS paketleme, ad-hoc imza, bağımlılık/lisans envanteri ve font değişiklikleri nedeniyle ayrışıyor.

### Mevcut Git deposu

`/Users/cixanla/Documents/Codex/2026-08-02/new-chat/work/git/PC-Shop-Empire`

Bu depoda gerçek kaynak kod yok; yalnız README ve sürüm yönlendirmesi var. Dolayısıyla şu anda doğrulanmış, uzaktan yedekli bir canonical kaynak geçmişi bulunmuyor.

### USB durumu

USB 11 Ağustos 2026'da bağlıyken salt-okunur envanter ve SHA-256 karşılaştırması tamamlandı. USB kaynağı ile yerel inceleme kopyasında eksik, fazla veya farklı dosya yoktur. Hiçbir birleştirme, üzerine yazma veya izin değişikliği yapılmadı.

### Canlı kayıt konumu

`/Users/cixanla/Library/Application Support/PC Shop Empire`

Konum keşfedildi; canlı kayıt içeriği değiştirilmedi.

## Teknik profil

| Alan | Bulgu |
|---|---|
| Uygulama | PC Shop Empire 1.1.6 |
| Çalışma zamanı | Electron 43.1.1 |
| Paketleme | Electron Forge 7.11.2 |
| Oyun kodu | Vanilla JavaScript |
| Arayüz | HTML + CSS |
| Masaüstü kabuğu | Node.js/Electron |
| Save schema | 3 |
| Dil | Türkçe, İngilizce, Almanca |
| Ana `game.js` | 17.639 satır |
| Yaklaşık toplam kaynak/doküman/test | 28.203 satır |
| Kaynak teslimatı | Yaklaşık 4,6 MB |
| macOS çalışma kopyası, bağımlılıklarla | Yaklaşık 742 MB |
| 3D sahne/model/rig | Yok |
| Fizik/navmesh/NPC | Yok |
| Ses dosyaları | Yok; UI sesleri WebAudio osilatörü |
| Görsel varlıklar | İkonlar ve tek ana menü mağaza görseli |

`package.json`, `forge.config.js`, `game.js`, `index.html`, `styles.css`, `main.js`, `preload.js` ve `src/` temel yapıyı oluşturuyor. Kaynak README'si de kullanılan teknolojinin Electron + JavaScript/HTML/CSS olduğunu açıkça doğruluyor.

## Olumlu mevcut temeller

Electron yapılandırmasında yeni 3D oyun için doğrudan kullanılmayacak fakat güvenlik kültürü olarak korunabilecek iyi niyetler var. Bunlar kaynak/config düzeyinde görülmüştür; paketlenmiş binary fuse durumunun ayrıca doğrulandığı anlamına gelmez:

- Context isolation.
- Sandbox.
- Node entegrasyonunun kapatılması.
- Büyük ölçüde self kaynaklı CSP; `style-src 'unsafe-inline'` ve `img-src data:` istisnaları vardır.
- Haricî pencere ve navigasyonun engellenmesi.
- Forge config'inde Electron fuse sertleştirmesi ve ASAR bütünlüğü ayarları; paket binary'si ayrıca denetlenmedi.
- Görünür uygulama sürümü ile save schema'nın ayrılması.
- Eski kayıtlar için uyumluluk niyeti.
- Erişilebilirlik ayarlarının daha ilk üründe düşünülmüş olması.

Unity'de bunların karşılığı; en az yetki, dış girdiyi doğrulama, sürümlü kayıt, bağımlılık kilidi, lisans envanteri ve güvenli raporlama olacaktır.

## Çalıştırılan testler

Mevcut iki test geçti:

1. **Smoke test:** Uyumlu market teklifleri, save geçişi, parça geri satışı, kredi, olay, kampanya, servis, ihale ve analitik temel kontrolleri.
2. **45 günlük simülasyon:** Sayısal değerlerin finite kalması, market döngüsü, hedefler, servis/ihale kuyruk limitleri ve finans geçmişi.

Bu sonuç, arşivin tamamen bozuk olmadığını gösterir; fakat kalite garantisi değildir. 45 günlük test her gün yapay gelir eklediği için şunları doğrulamaz:

- Gerçek ekonomi dengesi.
- UI akışı veya erişilebilirlik.
- Save bozulması ve kurtarma.
- Oyuncunun yapabileceği sıra dışı işlem kombinasyonları.
- Görsel performans.
- NPC, fizik veya 3D sistemler.

## Mevcut veri omurgası

Başlangıç durumu kabaca şu grupları içerir:

- Para, itibar, seviye, XP ve takvim.
- Zaman, duraklatma ve hız.
- Garaj/lokasyon ve aylık gider.
- Elektrik, internet ve sigorta sağlayıcıları.
- Parça envanteri ve pazar teklifleri.
- Toplanmış bilgisayarlar.
- Müşteri siparişleri.
- Personel ve otomasyon.
- Depo, atölye, pazarlama, güvenlik, muhasebe ve otomasyon geliştirmeleri.
- Günlük ve yaşam boyu ölçümler.
- Aktivite günlüğü.
- Servis merkezi, ihaleler, marketing, pazar dinamikleri, analitik ve kariyer.

Bu alanların yeni oyuna birebir sınıf olarak aktarılması gerekmiyor; anlamları kaybolmadan yeniden modellenmeleri gerekiyor.

## 14 Dashboard ekranının 3D karşılığı

| Mevcut ekran | Mevcut işleyiş | Yeni 3D karşılığı | Karar |
|---|---|---|---|
| Dashboard | Para, itibar, PC, sipariş, depo, personel ve aktivite özeti | Ofis bilgisayarı/tablet/yönetim terminalindeki operasyon özeti; kritik durumlar fiziksel dünyada da okunur | **Koru ve dönüştür** |
| Component Market | Teklif seç, 1/5 adet al; ürün anında envantere geçer | Tedarikçi karşılaştırması, sipariş, ETA, taşıma, fiziksel teslimat, sayım, hasar ve kabul | **Yeniden tasarla** |
| Inventory | Adet + ortalama maliyet listesi, anlık geri satış | Depo/raf/rezervasyon/el/çalışan konumu, seri veya batch, kondisyon, garanti, barkod | **Dönüştür** |
| Assembly Workshop | Sekiz parça seçip tek tuşla PC üretme | Kasa seçme, parça kit'i, tezgâh, montaj, kablo, firmware, OS, sürücü, test, paket | **Tam yeniden tasarım** |
| Customers | Deadline'lı sipariş kartı ve hazır PC seçerek teslim | Fiziksel NPC, ihtiyaç görüşmesi, mağaza gezme, danışmanlık, kasa, özel sipariş, şikâyet/iade | **Kavramı koru, davranışı yeniden tasarla** |
| Staff | Beş rol, sayaç dolunca görünmez fonksiyon çağrısı | Kalıcı NPC, vardiya, rota, iş istasyonu, görev rezervasyonu, beceri, yorgunluk, yardım/escalation | **Büyük yeniden tasarım** |
| Service Center | Standard/premium düğmesi ve başarı zar atışı | Intake, veri izni, teşhis, teklif, onay, fiziksel onarım, test, burn-in ve teslim | **Tam yeniden tasarım** |
| Store & Rent | Garajdan merkeze soyut kapasite/bonus | Gerçek lokasyonlar, kira/depozito, iç plan, taşınma, modüler genişleme ve eski yeri yeniden amaçlandırma | **İlerleme omurgasını koru** |
| Finance | Gün sonu kira/maaş/gider/vergi/kredi | Cash flow, COGS, vade takvimi, borç, forecast, yapılandırma ve işlem ledger'ı | **Yeniden tasarla** |
| Brand & Market | Kampanya, tedarikçi güveni, döngü ve rakip baskısı | Terminalde karar; trafik, segment, talep ve tedarik koşullarında fiziksel sonuç | **Koru ve derinleştir** |
| Business Intelligence | 7 günlük tahmin, marj, stok dönüşü, risk ve ledger | Nedensel, güven aralıklı, ödemeye bağlı karar desteği | **Güçlü biçimde koru** |
| Upgrades | Altı soyut alanın 10 seviyesi | Raf, forklift/arabası, tezgâh, test cihazı, CCTV, yazılım, eğitim ve renovasyon gibi gerçek varlık/politika | **Soyut seviyeyi çıkar, fiziksel karşılık yap** |
| Activity Log | Son 160 olay ve tür filtresi | Oyuncunun operasyon günlüğü, işlem izi ve açık kurtarma bildirimi; Guardian'ın ayrıntılı geliştirici raporu bu ekranda görünmez | **Koru** |
| Career | Günlük hedef, altı başarım, seviye/kilometre taşı | Bölüm hedefleri, sistemik krizler, hikâye kırıntıları ve nitelikli Steam başarımları | **Koru, görevleri değiştir** |

Sağdaki global “Staff Automation” anahtarı yeni oyunda kaldırılmalı. Bunun yerine çalışan başına rol, vardiya, bölge, öncelik, görev politikası, bütçe sınırı ve escalation kuralı bulunmalı.

## Mevcut önemli sistemler ve yeni anlamları

### Lokasyonlar

Eski veri beş seviye içeriyor: garaj, mahalle, şehir mağazası, mega mağaza ve merkez. Yeni kariyerin onaylanmış dört ana sahnesi garaj, mahalle dükkânı, gelişmiş teknoloji mağazası ve amiral mağazadır. “Merkez ofis” ayrı oynanabilir lokasyon veya geç oyun yönetim modülü olarak daha sonra değerlendirilebilir.

Eski kapasite, müşteri ve atölye bonusları doğrudan yüzde buff olarak taşınmamalı. Karşılıkları:

- Gerçek raf ve depo hacmi.
- Teslimat kapısı/palet erişimi.
- Kasa sayısı ve kuyruk kapasitesi.
- Tezgâh/test istasyonu sayısı.
- Personel dolabı, dinlenme ve vardiya kapasitesi.
- Güvenlik görüş alanı.
- Mağaza bölümleri ve müşteri dolaşımı.

### Sağlayıcılar

Elektrik, internet ve sigorta sağlayıcıları iyi bir işletme kararı kaynağıdır. Fakat “daha pahalı internet = markette daha çok teklif” gibi oyunlaştırılmış bonuslar gerçekçi karşılığa dönmeli:

- Elektrik: sabit ücret, kapasite, kesinti güvenilirliği, UPS/jeneratör, yoğun test tezgâhı yükü.
- İnternet: kurulum/download/test süresi, online sipariş ve POS sürekliliği.
- Sigorta: kapsam, muafiyet, claim süresi, güvenlik koşulu ve prim geçmişi.

### Çalışan rolleri

Eski rollerde satışçı, teknisyen, satın almacı, muhasebeci ve yönetici bulunuyor. Yeni sistem bu anlamları korur; ayrıca kasiyer, depo, temizlik ve güvenlik rolleri eklenebilir. “Kalite puanı” tek sayı olmayacak; role özgü beceri vektörü kullanılacak.

### Müşteri türleri

Öğrenci, ofis kullanıcısı, oyuncu ve yayıncı gibi mevcut segmentler korunabilir. Fakat tek minimum performans puanı yerine amaç profili kullanılacak:

- Uygulama/oyun iş yükleri.
- Bütçe ve toplam sahip olma maliyeti.
- Gürültü/ısı/enerji hassasiyeti.
- Taşınabilirlik veya yükseltilebilirlik.
- Görünüm ve marka algısı.
- Teslim tarihi ve garanti beklentisi.
- Teknik bilgi ve tavsiye ihtiyacı.

### Pazar döngüleri

Sakin, arz fazlası, kıtlık, oyun sezonu, iş dalgası ve fiyat savaşı fikirleri güçlüdür. Yeni sistemde tek çarpan yerine kategori, nesil, tedarikçi, teslim süresi, müşteri segmenti ve rakip davranışı üzerinden açıklanmalıdır.

### Kariyer ve başarımlar

Eski hedefler çoğunlukla günlük gelir, PC sayısı, satış sayısı, servis sayısı ve otomatik görev sayısıdır. Bunlar onboarding veya kısa meydan okuma olabilir; fakat uzun vadeli başarımlar ham hacim grind'ına dönüşmemeli. Yeni örnekler:

- Yanlış teşhisi ek maliyet oluşturmadan düzeltmek.
- Kriz döneminde kritik okul/ofis teslimini zamanında yapmak.
- Aynı müşterinin üç yaşam aşamasındaki ihtiyacını doğru karşılamak.
- Hasarlı sevkiyatı doğru kayıtla claim etmek.
- Belirli süre sıfır stok uyuşmazlığıyla çalışmak.
- Düşük kârla güven kazanıp kurumsal referansa dönüşen satış yapmak.

## Teknik borç ve port etmeme gerekçesi

### Monolit ve global durum

`game.js` içinde yaklaşık 201 üst düzey fonksiyon bildirimi var. Oyun durumu, UI üretimi, ekonomi, kayıt, müşteri, personel ve zaman aynı global alanda yaşıyor. Bu, küçük Electron uygulamasında çalışsa da büyük 3D projede test ve sahiplik sınırlarını bozar.

### Yinelenen fonksiyonlar

En az 10 fonksiyon adı birden fazla tanımlanmış. `saveGame`, `loadGame`, `deleteSaveGame` ve `prepareNewGame` üçer kez görülüyor. Eski `checkFinancialFailure` kaydı silip menüye döndürürken dosyanın aşağısındaki yeni sürüm bu davranışı sessizce geçersiz kılıyor. Sonuç, kod sırasına bağımlı.

### Monkey-patch katmanı

`src/release-bootstrap.js`, temel fonksiyonları yakalayıp global isimleri tekrar değiştiriyor. Bu, geriye uyumluluk için pratik bir yama olsa da yeni projede sürdürülebilir modül mimarisi değildir.

### Hard-coded ve oturumlar arası değişebilen ürün kataloğu

Kod sekiz kategori için 2.023 parça üretiyor. Bazı fiyat/puan ayrıntıları uygulama açılışındaki rastgele hesaplara bağlı; aynı `partId` farklı oturumda değişebilir. Bu, deterministik save, dengeleme ve bug yeniden üretimi için risklidir.

### Kayıt güvenliği

Mevcut yaklaşım bütün `gameState` nesnesini JSON'a çevirip `localStorage` içine yazıyor. Atomik dosya değişimi, checksum, iki aşamalı commit, migration test zinciri veya bozuk nesne karantinası yok.

### 3D temeli yok

Proje; sahne, transform, prefab, mesh, collider, rigidbody, navmesh, animasyon, IK, material, shader, LOD veya occlusion sistemlerinden hiçbirini içermiyor. Bu nedenle Unity'ye “dönüştürme” aslında yeni oyun yazmakla aynı maliyete yakın, fakat eski borcu da taşır.

## Mevcut oyunda gerçekten olmayan alanlar

- Tekil PC parçası ve periferik raf satışı.
- Kasa, ödeme, para üstü ve müşteri kuyruğu.
- Raf bazlı fiyat etiketi.
- Kurye, teslimat, kutu açma ve taşıma.
- Fiziksel mağaza planı, vitrin, depo ve atölye.
- NPC müşteri veya çalışan navigasyonu.
- Fiziksel montaj sırası ve görünür eller.
- Kablo yönetimi, firmware/BIOS, OS, driver, benchmark ve paketleme.
- RMA, garanti iş emri ve parça seri geçmişi.
- Gerçek teşhis ve veri mahremiyeti.
- Online sipariş, şube ve servis merkezi operasyonu.
- Monitör, klavye, mouse, kulaklık, webcam, kablo ve sarflar için perakende veri modeli.

“Müşteri AI” şu anda deadline'lı sipariş kaydı; “çalışan AI” süre dolduğunda görünmez fonksiyon çağrısıdır. Bu bir eleştiri değil, yeni işin boyutunu doğru tanımlayan sınırdır.

## Dönüşüm sınıfları

### Korunacaklar

- Garajdan büyüyen teknoloji işletmesi teması.
- Perakende, özel PC, servis ve kurumsal işlerin aynı şirket içinde bulunması.
- PC uyumluluğu fikri.
- Ortalama maliyet, stok değeri ve finans geçmişi.
- Müşteri segmenti, bütçe, ihtiyaç, tolerans ve teslim tarihi.
- İtibar ve çalışan beceri/enerji/moral/yorgunluk/uzmanlık fikirleri.
- Tedarikçi güveni, market döngüsü ve rekabet baskısı.
- Servis, ihale, analitik ve kariyer omurgası.
- Üç şirket profili/kayıt yuvası fikri.
- Türkçe/İngilizce/Almanca veri deneyimi; çıkış kapsamı ayrıca GDB'de belirlenir.
- Erişilebilirliği ayarlara sonradan eklememe yaklaşımı.

### 3D karşılığına dönüştürülecekler

- Dashboard → fiziksel terminal/tablet.
- Envanter → raf, depo, rezervasyon, el ve seri/batch ledger'ı.
- Müşteri → fiziksel NPC + görüşme + davranış.
- Çalışan → fiziksel NPC + vardiya + görev kuyruğu.
- Lokasyon → gerçek sahne ve işlevsel alan.
- Upgrades → yerleştirilen ekipman, renovasyon ve politikalar.
- Market olayı → kategorisel fiyat/ETA/talep ve fiziksel trafik sonucu.
- Aktivite günlüğü → oyuncuya açık operasyon/işlem izi; Guardian tanıları ayrı ve oyuncuya kapalı teknik kayıt.

### Baştan yeniden tasarlanacaklar

- Tek tuş PC toplama.
- Tek tuş servis sonucu.
- Anında sipariş ve stok.
- Tek performans puanı.
- Personel otomasyon anahtarı.
- Yüzde buff tabanlı binalar ve sağlayıcılar.
- Rastgele para cezası/ödülü olayları.
- Günlük vergi ve sihirli muhasebeci indirimi.
- Acil kredi ve iflas zinciri.
- Hard-coded SKU üretimi.
- `localStorage` save dump.

### Eklenecekler

- Birinci şahıs hareket, eller, kamera, IK ve etkileşim.
- Serbest taşıma + hassas snap fiziği.
- Kutu, koli, palet, el arabası ve teslimat kabulü.
- Raf etiketi, fiyat, kasa, iade ve garanti.
- NPC ihtiyaç, bilgi, sabır, sıra ve sadakat sistemi.
- İş/ürün/çalışan rezervasyonları ve görev sahipliği.
- Fiziksel PC montajı, firmware, OS, test ve paketleme.
- Evidence-based servis ve veri mahremiyeti.
- İkinci el/refurbish zinciri.
- Online/kurumsal/e-spor/okul-ofis gelir kanalları.
- Açıklanabilir ekonomi, nakit tahmini ve rakipler.
- Atomik save/recovery/cloud conflict.
- PSE Guardian Core.
- Asset provenans ve üçüncü taraf lisans kayıt defteri.

### Çıkarılacaklar

- Dashboard'dan anlık üretim/teslim.
- Fiziksel süreç olmadan “quick sell”.
- Standard/premium düğmesiyle zar atılan servis.
- Tek tık kurumsal ihale teslimi.
- Global staff automation aç/kapat.
- Ticari onay olmadan gerçek marka/model kataloğu.
- Eski CSS mağaza illüzyonu ve arayüzün birebir kopyası.
- Salt üretim sayısına dayalı endgame grind'ı.

## Marka, görsel ve hak zinciri

Eski katalog AMD, Intel, NVIDIA/RTX/Radeon/Arc ile ASUS, MSI, Gigabyte, Samsung, Corsair, Kingston, Noctua ve NZXT gibi gerçek adlar içeriyor. Yeni oyunda bu liste kullanılmayacak. Metinsel tanımlayıcı, logo, kutu görünümü ve ayırt edici endüstriyel tasarım için ayrı hukuk/lisans değerlendirmesi gerekebilir.

Mevcut iki PNG içinde C2PA/JUMBF manifest izleri var. Bu yalnız provenans sinyalidir; ticari kullanım lisansı değildir. Ana menü görselinin üretim ve lisans zinciri final kullanımdan önce kanıtlanamadığı için yeni oyuna alınmamalıdır.

`PC Shop Empire`, `cixanla`, kurgusal tedarikçi ve şirket adları da yayın öncesinde benzerlik/marka taramasından geçmelidir.

## Canonical arşiv durumu ve sonraki güvenli işlem

Tamamlananlar:

1. Aday kaynaklar yalnız okuma ile listelendi.
2. USB kaynağı için boyut ve SHA-256 manifest çıkarıldı.
3. USB ile yerel inceleme kopyası 26/26 eşleşti.
4. Mac çalışma kopyasının beş ayrışan dosyası ayrıca raporlandı.
5. USB'deki `KAYNAK_KODU`, canonical legacy snapshot olarak belirlendi; yerel inceleme kopyası onun doğrulanmış aynasıdır.

Kurulum/uygulama onayından sonra yapılacaklar:

1. Orijinalleri değiştirmeden yeni, tarihli ve yazma korumalı legacy arşiv hazırla.
2. Manifesti arşivin yanında ve ayrı bir yedek hedefinde sakla.
3. Yeni Unity projesini legacy arşivin dışında, temiz depoda başlat.

Bu ayrım, eski oyunu kaybetmeden yeni oyunu ondan teknik olarak bağımsız kılar.
