# PC Shop Empire 3D – Teknik Mimari, Araçlar ve PSE Guardian

**Belge durumu:** Ön üretim teknik tasarımı 0.1  
**Tarih:** 11 Ağustos 2026  
**Kapsam:** Kodlama veya kurulum yapılmadan önce önerilen mimari, üretim araçları, platform planı, kalite sistemi ve gömülü tanılama tasarımı.

## Teknik sonuç

Yeni oyun için önerilen temel:

- Başlangıç tabanı olarak **Unity 6.3 LTS**, Universal Render Pipeline ve C#; alpha öncesi zorunlu LTS yükseltme kapısı.
- MacBook Air M4/32 GB üzerinde günlük geliştirme.
- Windows 10/11 64-bit ve Steam için erken, tekrarlanan gerçek donanım testleri.
- Durumu Unity sahnesine gömmeyen, veri güdümlü ve modüler bir simülasyon çekirdeği.
- Tek bir stok ve finans gerçeği; fiziksel dünya bunun görsel/etkileşimli karşılığı.
- Küçük, doğrulanmış ürün kataloğuyla başlayıp kombinasyon testleri geçtikçe genişleme.
- Offline çalışan, dış modele bağlanmayan ve kendini değiştiremeyen **PSE Guardian Diagnostic Runtime**.
- Ücretsiz araçlarla başlangıç; yalnız yüksek etkili ücretler için ayrı onay kapısı.

Bu karar büyük oyunu bir anda kolaylaştırmaz. Fakat en pahalı hata sınıflarını—kayıp stok, çift satış, çözülemez PC işi, bozuk kayıt, takılan NPC, açıklanamayan ekonomi ve platforma özel sorunları—başlangıçta kontrol altına alır.

## 1. Motor kararı

### Neden Unity 6.3 LTS?

| Ölçüt | Unity 6.3 LTS | Unreal Engine 5 | Godot 4 |
|---|---|---|---|
| Bu proje için ana dil | C#; simülasyon verisi ve testleri için uygun | C++/Blueprint; güçlü fakat solo üretimde derleme ve mimari yükü daha yüksek | GDScript/C#; hızlı ve açık kaynak |
| Mac geliştirme → Windows hedefi | Yerleşik çoklu platform akışı; Windows için yine gerçek PC kapıları gerekir | Mümkün; proje/derleme yükü MacBook Air için daha ağır olabilir | Mümkün; kurulum ve çalışma alanı hafif |
| Birinci şahıs, fizik, nav, UI, profil araçları | Olgun ve dengeli | Görsel kalite ve büyük dünya araçları çok güçlü | Küçük/orta 3D projelerde yeterli; bu kapsam için daha fazla özel altyapı riski |
| Varlık/eklenti ekosistemi | Çok geniş; lisans ve kalite denetimi şart | Çok geniş ve yüksek kaliteli | Daha küçük; ücretsiz/açık seçenekleri güçlü |
| Solo üretim riski | Orta | Orta-yüksek | Orta; motor maliyeti düşük, özel üretim maliyeti artabilir |
| Öneri | **Seçildi** | Bu proje için gereğinden ağır başlangıç | Güçlü yedek seçenek, fakat seçilmedi |

Unity seçimi “en güçlü motor” iddiası değildir. Bu oyunun C# ağırlıklı simülasyon, birinci şahıs fizik, veri güdümlü katalog, Steam/Windows ve sonradan macOS ihtiyaçları için en dengeli araç olduğu yönündeki proje kararıdır.

Unity 6.3, Aralık 2025'te LTS olarak yayımlandı ve yalnız Aralık 2027'ye kadar standart destek planlanıyor. Bu projenin 30–54+ aylık ihtimal aralığı daha uzundur; dolayısıyla 6.3 **başlangıç tabanıdır**, bütün üretimin son motoru olduğu varsayılmaz. Kurulum gününde destek ve yama durumu yeniden doğrulanmalıdır: [Unity 6.3 LTS duyurusu](https://unity.com/blog/unity-6-3-lts-is-now-available), [Unity 6 destek politikası](https://unity.com/releases/unity-6/support).

### Lisans sınırı

Unity Personal güncel 2026 koşullarında, son 12 aylık `Total Finances` 200.000 ABD dolarını aşmıyorsa kullanılabilir. `Total Finances` hesabı birey, tüzel kişi ve üçüncü tarafa hizmet veren taraf için farklı tanımlandığından yalnız “oyun geliri” diye sadeleştirilmez. Tanım ve eşik zamanla değişebileceğinden kurulum, plan yenileme ve yayın öncesinde tekrar kontrol edilir: [Unity fiyatlandırma güncellemesi](https://unity.com/products/pricing-updates), [Editor Software Terms](https://unity.com/legal/editor-terms-of-service/software).

Motor sürümü proje ortasında sebepsiz yükseltilmez. Politika:

1. İlk depo, onaylanan tek bir Unity 6.3 LTS yama sürümüne kilitlenir.
2. Güvenlik veya bloklayıcı düzeltme olmadıkça milestone ortasında sürüm değişmez.
3. Yükseltme ayrı dal/kopyada, kayıt göçü ve Windows build testinden sonra kabul edilir.
4. Preview paketler üretim temelinin parçası yapılmaz.
5. **Alpha öncesinde zorunlu motor kapısı:** O tarihte desteklenen üretim sürümüne yükseltme; paket/plugin/API envanteri, görsel golden scene, save migration, performans ve gerçek Windows IL2CPP regresyonu geçmeden eski sürüm bırakılmaz.

## 2. MacBook ile Windows oyunu üretme planı

### Yapılabilecekler

MacBook Air M4/32 GB üzerinde şunlar yapılabilir:

- Unity editöründe sahne, etkileşim, ekonomi ve NPC geliştirme.
- C# derleme, birim ve editör testleri.
- Blender ile düşük/orta yoğunlukta modelleme ve UV çalışması.
- URP shader, UI, ses ve kayıt geliştirme.
- macOS editöründe hızlı oynanış doğrulaması.
- macOS editöründeki **Windows Build Support (Mono)** modülüyle erken Windows x64 build çıktısı üretme.

### Mac'in doğrulayamayacağı alanlar

Mac'te çalışan editör testi şu alanların kanıtı değildir:

- DirectX 11/12 sürücü davranışı.
- NVIDIA, AMD ve Intel GPU performansı.
- Windows dosya yolları, izinleri, antivirüs ve kullanıcı klasörü davranışı.
- Steam overlay, Steam Input, Cloud ve başarımların Windows paketi.
- Çoklu monitör, ultrawide, DPI ölçekleme ve farklı klavye düzenleri.
- Windows IL2CPP final derlemesi ve platforma özgü native eklentiler.

Unity 6.3 modül dağılımında macOS/ARM64 editörü Windows Build Support (Mono) sunar; Windows IL2CPP modülü native Windows editör/toolchain tarafındadır. Final Windows pipeline'ı gerçek Windows x64 makine ister. Apple Silicon üzerindeki Windows ARM sanal makinesi yalnız sınırlı smoke test olabilir; DirectX/GPU/Steam/performance QA'nın yerine geçmez: [Unity 6000.3.21f1 modülleri](https://unity.com/releases/editor/whats-new/6000.3.21f1), [Unity 6.3 IL2CPP](https://docs.unity3d.com/6000.3/Documentation/Manual/il2cpp-introduction.html).

Gerçek Windows x64 PC şu anda doğrulanmış mevcut ekipman değildir; ilk oynanabilirden önce sağlanması gereken dış bağımlılık olarak risk ve takvimde tutulur.

### Zorunlu test kapıları

| Kapı | Windows'ta kanıtlanacaklar | Geçme koşulu |
|---|---|---|
| İlk oynanabilir | Açılış, input, taşıma, kayıt, bir sahne performansı | Bloklayıcı yok; kayıt yükleniyor; temel etkileşim tamamlanıyor |
| Vertical slice | Tam mağaza–PC işi döngüsü, çözünürlükler, GPU/CPU profili | Hedef donanım sınıfında ölçülen frame-time ve sıfır kritik akış hatası |
| Alpha | Uzun kayıt, Steam test AppID, kontrolcü, dil, cloud çatışması | 20+ saatlik soak; kayıt kaybı yok; P0/P1 hatalar kapalı |
| Release candidate | Temiz makine kurulumu, Steam review build, farklı donanımlar | Yayın kontrol listesi ve Steam incelemesi geçildi |

Windows bilgisayarı yalnız finalde bulmak geç olacaktır. İlk oynanabilir çekirdek ortaya çıktığında erişim sağlamak, platform borcunu ucuzken yakalar.

### MacBook Air ısı ve iş yükü

Bu model fansızdır. Alt soğutucu/stand hava akışına ve ergonomiye yardımcı olabilir fakat aktif fanlı workstation'a dönüştürmez. Asıl önlemler:

- Büyük importları küçük paketlere ayırmak.
- Işık bake'i yerine prototipte çoğunlukla dinamik/basit ışık kullanmak.
- Apple Silicon editörde CPU lightmapping desteklenmediği için gerekirse GPU lightmapper kullanmak; bake süresi/belleğini milestone benchmark'ıyla ölçmek.
- 2K doku varsayılanı; yalnız hero varlıklarda gerekçeli 4K.
- Editörde sınırsız NPC ve fizik nesnesi çalıştırmamak.
- Gece boyu kontrolsüz build yerine otomatik ama ölçülü iş kuyrukları.
- Unity `Library` ve Blender cache için yeterli boş alan bırakmak.

## 3. Mimari ilkeler

### 3.1 Tek durum, çok görünüm

Stok, para veya sipariş hem Dashboard'da hem 3D dünyada ayrı ayrı tutulmayacak. Tek authoritative simülasyon durumu bulunacak:

```text
Simülasyon durumu
  ├─ stok/seri/batch/rezervasyon
  ├─ sipariş ve iş emirleri
  ├─ muhasebe defteri
  ├─ müşteri/çalışan görevleri
  └─ zaman/pazar/itibar
        │
        ├── 3D dünya görünümü
        ├── Dashboard görünümü
        ├── kayıt anlık görüntüsü
        └── Guardian gözlem akışı
```

Örneğin oyuncu GPU kutusunu depodan tezgâha taşıdığında yeni bir sahte GPU yaratılmaz. Aynı `ItemInstanceId` yalnız konum ve sahiplik durumu değiştirir. Dashboard bunu anında “tezgâhta, İş #421'e ayrılmış” olarak görür.

### 3.2 İşlem sınırları

Ekonomiyi veya stoğu değiştiren her eylem atomik işlem olur:

- Satın alma siparişi oluşturma.
- Teslimatı kabul etme veya eksik/hasarlı claim açma.
- Ürünü iş emrine ayırma.
- Kasada fiyat anlık görüntüsü alma ve satışı tamamlama.
- İade, garanti değişimi ve tedarikçi RMA.
- Çalışan maaşı, kira, vergi karşılığı ve kredi ödemesi.

İşlem ya bütünüyle başarılı olur ya hiçbir parçası kalıcılaşmaz. “Para düştü ama ürün gelmedi” ve “müşteri aldı ama raf sayısı azalmadı” sınıfı hatalar bu sınırda engellenir.

### 3.3 Deterministik simülasyon zamanı

- Oyun zamanı tek bir simülasyon saatinden akar.
- Dünya sunumu her karede çalışabilir; ekonomi ve iş durumları sabit simülasyon adımlarında ilerler.
- Rastgele sonuçlar kayıtlı seed ve olay kimliğiyle üretilir.
- Kayıt yüklemek pazar fiyatını veya arıza sonucunu yeniden zar attırmaz.
- Dashboard zamanı durdurduğunda simülasyon olayı ilerlemez.

Bu tam ağ determinismi vaadi değildir. Ama ekonomi testlerinin tekrarlanabilmesini ve hata raporunun aynı seed ile yeniden üretilebilmesini sağlar.

### 3.4 Unity nesnelerinden bağımsız alan mantığı

Para, uyumluluk, görev veya stok kuralları `MonoBehaviour`, sahne objesi ya da animasyon durumunun içine gömülmez. Unity katmanı girdiyi alır ve sonucu sunar; alan çekirdeği saf C# veri/servisleriyle test edilir. Böylece:

- Sahne açmadan binlerce ekonomi günü test edilebilir.
- PC kombinasyonları otomatik taranabilir.
- Kayıt migration'ı headless doğrulanabilir.
- NPC sunumu kapalıyken dahi iş sonuçları aynı kurallarla yürür.

## 4. Önerilen modüller

| Modül | Sorumluluk | Bağımlılık kuralı |
|---|---|---|
| `PSE.Core` | Kimlikler, zaman, sonuç türleri, olaylar, temel invariant'lar | Hiçbir sunum modülüne bağlı değil |
| `PSE.Catalog` | Ürün aileleri, teknik özellikler, marka, nesil, kalite, garanti | Core dışında düşük bağımlılık |
| `PSE.Inventory` | Item instance, batch, konteyner, konum, rezervasyon, kondisyon | Catalog + Core |
| `PSE.Orders` | Satın alma, satış, özel PC, servis, online ve kurumsal iş emirleri | Inventory ve sözleşme veri tipleri |
| `PSE.Economy` | Çift taraflı/izlenebilir defter, nakit, COGS, borç, ödeme takvimi | Alan olaylarını tüketir; sunuma bağlı değil |
| `PSE.Retail` | Raf fiyatı, sepet, checkout, kampanya, iade, garanti | İlk offer dilimi Core + Catalog + Inventory; Orders/Economy yalnız checkout işlemleri geldikçe açık sınırla eklenir |
| `PSE.Assembly` | Build graph, uyumluluk, montaj adımları, kalite, test ve benchmark | Catalog + Inventory + Orders |
| `PSE.Service` | Intake, teşhis hipotezleri, izin, onarım, RMA, refurbish | Assembly + Inventory + Orders |
| `PSE.Actors` | Müşteri profili, çalışan becerisi, ihtiyaç ve görev durumu | Alan servisleriyle açık arayüzler |
| `PSE.World` | Etkileşim hedefleri, placement, nav rezervasyonları, istasyonlar | Alan kimliklerini 3D nesnelere bağlar |
| `PSE.Dashboard` | Salt-okunur view model ve yetkili komutlar | Alan servislerinin arayüzleri; state kopyası tutmaz |
| `PSE.Save` | Snapshot, journal, migration, doğrulama, recovery | Tüm kalıcı şemaları açık sözleşmeyle bilir |
| `PSE.Guardian` | Olay gözlemi, invariant, anomali, güvenli toparlama, rapor | Alan arayüzlerini gözler; oyun kuralı sahibi olmaz |
| `PSE.Presentation` | Unity sahne/prefab/animasyon/VFX/ses/UI | Alan durumunu sunar |
| `PSE.Platform` | Dosya sistemi, Steam, cloud, telemetry çıkışı | Arayüz arkasında; core'a Steam tipi sızdırmaz |

Bağımlılık yönleri otomatik assembly definition testleriyle korunmalıdır. Dairesel bağımlılık veya Dashboard'un doğrudan sahne nesnesini düzenlemesi kabul edilmez.

## 5. Veri ve katalog mimarisi

### 5.1 Stable ID zorunluluğu

Her kalıcı nesnenin insan tarafından değiştirilmeyen benzersiz kimliği olur:

- `ProductDefinitionId`: ürün tanımı.
- `ItemInstanceId`: tekil değerli parça/ürün.
- `BatchId`: sarf veya toplu ürün grubu.
- `ContainerId`: kutu, raf bölmesi, palet, çalışan eli, sepet.
- `OrderId`, `JobId`, `CustomerId`, `EmployeeId`, `TransactionId`.

Oyuncuya görünen isim değişse bile kimlik değişmez. Save ve telemetry metin adına güvenmez.

### 5.2 Ürün tanımı ile ürün örneğini ayırma

`NovaCore N5-360` gibi kurgusal ürün tanımı; soket, güç, performans ve garanti varsayımlarını içerir. Mağazadaki tekil kutu ise ayrıca şunları taşır:

- Seri veya batch.
- Satın alma maliyeti.
- Tedarikçi ve teslimat.
- Kondisyon/grade.
- Garanti başlangıcı.
- Rezervasyon ve fiziksel konum.
- Hasar veya test geçmişi.

Bu ayrım ikinci el, iade, RMA ve stok değerini mümkün kılar.

### 5.3 Yazar verisi ve çalışma verisi

Tasarımcı verisi Unity içinde kolay düzenlenebilir dosyalardan hazırlanır; build sırasında salt-okunur çalışma tablolarına dönüştürülür. Her katalog build'i:

- Şema kontrolü.
- Eksik çeviri kontrolü.
- Stable ID çakışma kontrolü.
- Fiyat ve performans aykırı değer kontrolü.
- En az bir uyumlu yapı yolu kontrolü.
- Referans edilen prefab/ikon/lisans kaydı kontrolü.
- `CatalogFingerprint` üretimi.

ScriptableObject düzenleme için kullanılabilir; fakat canlı save içine Unity nesne referansı yazılmaz.

### 5.4 İlk katalog sınırı

Vertical slice hedefi **50–80 anlamlı SKU** ve yalnız kanıtlanmış kombinasyonlardır. Tam oyun için geçici hedef **300–500 SKU**'dur. Bunlar aynı kabuğun farklı renginden oluşan yapay sayılar değil, yeni karar yaratan ürünler olmalıdır.

Katalog genişlemesi bir içerik kapısıdır: yeni ürün ailesi tüm uyumluluk, fiziksel çakışma, UI, save ve performans testlerini geçmeden ana dala girmez.

## 6. Fizik, etkileşim ve yerleştirme

### Hibrit yaklaşım

- Kutular, arabalar, paletler ve büyük ürünler taşıma sırasında fizik hissi verir.
- Raf ve iş istasyonu yerleştirmesi serbest önizleme + snap/grid kullanır.
- Hassas PC montajı tamamen serbest rigidbody kaosu değildir; doğru bölge, yön ve montaj sırasına snap olur.
- Eller, aracın ve parçanın durumuna göre IK/animasyon kullanır.
- Bırakılan küçük parça kaybolmaz; güvenli erişim noktasına veya “düşmüş nesne” durumuna geçer.

### Yetkili durum ile fizik ayrımı

RigidBody bir kutunun görsel pozisyonunu belirler; stok sahipliği yalnız doğrulanmış konteyner transferiyle değişir. Fizik nesnesi duvardan geçerse ürün ekonomik olarak silinmez. Guardian bunu “world projection mismatch” olarak raporlar ve nesneyi belirlenmiş karantina/son güvenli konuma alabilir.

### Yerleşim doğrulaması

Bir raf, kasa veya tezgâh kurulmadan önce:

- Kapı ve yangın/kaçış koridoru benzeri oyun içi erişim şeritleri kapanmamalı.
- En az bir müşteri ve çalışan yaklaşım noktası nav sistemiyle erişilebilir olmalı.
- Etkileşim, yeniden stoklama ve bakım boşluğu bulunmalı.
- Çakışan ürün spawn noktası olmamalı.

Hatalı yerleşim “kırmızı oldu, nedenini bul” biçiminde kalmaz; engellenen rota ve gerekli boşluk görünür biçimde açıklanır.

## 7. Müşteri ve çalışan yapay zekâsı

### Müşteri karar modeli

Müşteri davranışı katmanlıdır:

1. **İhtiyaç modeli:** kullanım amacı, bütçe, zaman, bilgi, tercih, risk ve servis beklentisi.
2. **Utility değerlendirmesi:** fiyat, uygunluk, kalite, bekleme, güven, stok ve tavsiye.
3. **Durum makinesi:** giriş, gezinme, yardım arama, danışma, ürün alma, sıra, ödeme, çıkış/şikâyet.
4. **Yol bulma ve yerel kaçınma:** yalnız sunum/rota katmanı.
5. **Bellek:** önceki satın alma, hizmet sonucu, dürüst tavsiye ve çözülmemiş sorun.

Karar sistemi hile yapmaz; bilmediği arka oda stoğunu bilmez. Memnuniyet sonucunda en etkili gerekçeler saklanır ve oyuncuya açıklanabilir.

### Çalışan görev modeli

Çalışanlar her kare “en yakın işi” kapmaz. Merkezi ama görünür bir görev sistemi kullanılır:

- Rol ve yetki.
- Vardiya ve mola.
- Bölge/istasyon ataması.
- Öncelik politikası.
- Beceri, araç ve sertifika gereksinimi.
- Ürün/istasyon rezervasyonu.
- Görev timeout ve güvenli yeniden kuyruk.

İki çalışan aynı GPU'yu veya kasayı alamaz. Kritik bir gereksinim eksikse çalışan ürünü yok etmek veya döngüye girmek yerine işi bekletir ve yardım ister.

### Simulation LOD

Yakındaki NPC tam animasyon, rota ve görsel etkileşim kullanır. Görüş dışı/uzak çalışan aynı görev ve ekonomi kurallarıyla, daha seyrek ve soyut sunum adımlarıyla ilerler. Uzak şubeler çalışan başına fizik çalıştırmaz; günlük/olay tabanlı kapasite simülasyonu kullanır.

“LOD” sonucu değiştirmemeli. Aynı beceri, araç, stok ve zaman koşulu benzer çıktı üretmelidir.

## 8. PC toplama ve uyumluluk motoru

### Build graph

Bir PC, tek bir toplam puan değil, yuvalar ve bağlantılardan oluşan grafiktir:

- Kasa/form faktörü ve fiziksel hacim.
- Anakart/soket/chipset/RAM türü.
- CPU, soğutma ve termal kapasite.
- GPU uzunluk/kalınlık/güç bağlantıları.
- PSU watt, ray/pay ve konektörler.
- Depolama arayüzleri, yuvalar ve lane paylaşımı.
- Fan, header, airflow ve kablo yolu.
- Firmware/OS/sürücü/test durumu.

Her kural şu yapıda sonuç vermelidir:

- Seviye: engel, risk, öneri veya bilgi.
- Beklenen/gerçek değer.
- Etkilenen bileşenler.
- Neden.
- En az bir düzeltme yolu.

### Katmanlı gerçekçilik

İlk oyuncuya soket, RAM türü, form, güç ve fiziksel sığma öğretilir. BIOS, lane, header, termal/gürültü optimizasyonu ve ince ayar daha sonra açılır. Zorluk, kuralları gizleyerek değil; daha dar bütçe, daha karmaşık gereksinim ve daha az hata toleransıyla artar.

### İş üretmeden önce solvability proof

Her otomatik özel PC veya servis işi yayımlanmadan önce sistem şunları kanıtlar:

1. Oyuncunun açılmış katalog düzeyinde en az bir geçerli çözüm var.
2. Parçalar mevcut stokta veya teslim süresi içinde sipariş edilebilir.
3. Toplam maliyet, ödeme ve kabul edilen marj sınırında.
4. Gerekli istasyon/araç erişilebilir.
5. İş adımları ve teslim süresi takvimde mümkün.

Bu kanıt başarısızsa iş müşteriye oluşturulmaz. Seed ve kanıt özeti test kaydına yazılır.

### Kombinasyon testi

- Kural başına birim testleri.
- Ürün ailesi çapraz kombinasyon taraması.
- Geometrik kritik eşleşmeler için prefab test fixture'ları.
- Rastgele ama seed'li binlerce geçerli/geçersiz build üretimi.
- Beklenen hata nedeninin stabil metin anahtarı.
- Save/load sonrası build graph eşitliği.

## 9. Kayıt ve Steam Cloud güvenliği

### Yerel kayıt yapısı

Her kayıt yuvası şunları içerir:

- Küçük metadata: sürüm, şirket, oyun zamanı, son sahne, durum.
- Sürümlü snapshot.
- Snapshot'tan sonra sınırlı işlem journal'ı.
- Checksum ve şema kimliği.
- Catalog/build fingerprint.
- Döner önceki sağlam kopyalar.

Yazma sırası:

1. Yeni dosya geçici hedefe yazılır.
2. Tam dosya kapanır ve yeniden okunarak doğrulanır.
3. Checksum/şema/invariant testleri geçer.
4. Önceki sağlam kayıt yedeğe döndürülür.
5. Yeni dosya atomik ad değişimiyle etkinleştirilir.

Bu zincir eski sağlam kaydı korumayı **hedefler**, fakat geçici dosya + rename tek başına elektrik kesintisine karşı mutlak garanti değildir. Uygulama, desteklenen platformda dosya flush/fsync eşdeğerini ve mümkünse dizin metadata dayanıklılığını kullanır; Windows/macOS replace semantiği, antivirüs/cloud müdahalesi, cache ve yarım yazma fault injection ile test edilir. Loader; etkin dosya geçmezse sıralı biçimde doğrulanmış önceki snapshot/journal'a düşer. “Kayıt kaybı yok” yalnız bu testler geçtikten sonra kalite iddiası olabilir.

### Migration

- Her kalıcı şema sürümlüdür.
- Migration'lar sırayla, tek yönlü ve testlidir.
- Eski dosya doğrudan üzerine yazılmaz; dönüşüm kopyada denenir.
- Başarısız migration kaynak dosyayı korur ve anlaşılır hata üretir.
- Release build, desteklenen eski sürümlerden golden save örneklerini yükler.

### Steam Cloud

Steam Cloud kolaylık katmanıdır, kayıt motoru değildir. Yerel oyun offline çalışır. Yerel ve bulut sürümleri ayrışırsa tarih yanında oyun günü, şirket, ilerleme ve cihaz bilgisiyle kullanıcı seçimi istenir; sessiz üzerine yazma yapılmaz. Resmî entegrasyon kaynağı: [Steam Cloud](https://partner.steamgames.com/doc/features/cloud?language=english).

## 10. PSE Guardian Diagnostic Runtime

### 10.1 Ne olduğu

Guardian, yayınlanan oyuna gömülü **offline tanılama ve bütünlük katmanıdır**. Üretken yapay zekâ, dil modeli veya kendi kendine kod yazan ajan değildir. “AI” ifadesi mağaza pazarlamasında kullanılmamalı; teknik adı davranışını doğru anlatmalıdır.

Guardian'ın bileşenleri:

- Deterministik invariant kuralları.
- İşlem öncesi/sonrası bütünlük kontrolleri.
- Olay zinciri ve durum fingerprint'i.
- Sınırlı, cihaz içi istatistiksel baseline.
- Önceden tanımlı güvenli kurtarma eylemleri.
- Geliştirici için ayrıntılı neden raporu.

### 10.2 Ne olmadığı

Guardian:

- OpenAI, ChatGPT, Codex veya başka bir API kullanmaz.
- Harici model ağırlığı indirmez.
- Kaynak kodu veya build dosyalarını değiştirmez.
- Yeni kod üretmez, derlemez veya çalıştırmaz.
- Shell/terminal açamaz.
- Plugin yükleyemez.
- Kendini güncelleyemez.
- İnternetten komut alamaz; inbound bağlantı dinlemez.
- Para, stok, müşteri skoru veya zorluğu keyfî biçimde değiştiremez.
- Oyuncuya gizli dinamik zorluk uygulamaz.
- Bir hatayı gizlice “başarılı iş” saymaz.

Codex ve insan onayı olmadan temel oyunda değişiklik yapamaması yalnız politika değil, teknik yetki yokluğuyla sağlanır.

### 10.3 İzlenecek invariant örnekleri

| Alan | Örnek invariant |
|---|---|
| Stok | Bir `ItemInstanceId` aynı anda yalnız bir konteynerde ve tek bir sahiplik durumunda olabilir. |
| Rezervasyon | Satılmış, iade edilmiş veya RMA'daki ürün aktif işe ayrılamaz. |
| Satış | Tamamlanmış satıştaki ürün sayısı, sepet transferi ve muhasebe kaydı eşleşir. |
| Para | Nakit değişimi dengeli bir transaction ve neden koduna bağlıdır. |
| PC işi | Teslim edilen build, kabul snapshot'ındaki zorunlu gereksinimleri karşılar veya açık müşteri onayı taşır. |
| Görev | Çalışan aynı anda çakışan iki fiziksel görevde bulunamaz. |
| Dünya | Ekonomik olarak var olan fiziksel ürünün geçerli projection veya karantina kaydı vardır. |
| Kayıt | Yüklenen snapshot şema, checksum, stable ID ve çapraz referans kontrolünü geçer. |
| Zaman | Son ödeme veya teslim tarihi geriye doğru sebepsiz hareket etmez. |

### 10.4 Sınırlı yerel öğrenme

Kullanıcının istediği “kendi kendine gelişme” güvenli biçimde yalnız **baseline ayarlama** olarak yorumlanır. Örneğin Guardian aynı donanımda son oturumların:

- Frame-time hareketli dağılımını.
- Bir görev durumunda normal kalma süresini.
- Nav yeniden planlama sayısını.
- İşlem kuyruğu gecikmesini.
- Kategori bazlı beklenen miktar aralığını.

EWMA, medyan/quantile ve sıkı alt/üst sınırlarla öğrenebilir. Öğrendiği veri yalnız “bu oturum olağandışı mı?” sorusunu etkiler. Oyun tasarım değerlerini, ekonomiyi, hata eşiklerinin güvenlik çekirdeğini veya kodu değiştiremez. Baseline silinebilir ve sürüm değişince kontrollü sıfırlanır.

### 10.5 Güvenli kurtarma eylemleri

Yalnız önceden yazılmış, test edilmiş ve idempotent eylemler:

- Başarısız atomik işlemi tamamen reddetmek.
- Süresi dolmuş sahipsiz rezervasyonu bırakmak.
- Timeout olmuş çalışan görevini bir kez güvenli kuyruğa almak.
- Dünyanın dışına düşen projeksiyonu son doğrulanmış erişim noktasına/karantinaya taşımak.
- Bozuk yeni save yerine son doğrulanmış snapshot'ı önermek.
- Çift çalıştırılan event'i transaction kimliğiyle ikinci kez uygulamamak.

Guardian sessizce para basmaz, ürün yaratmaz, oyuncu kararını geri almaz veya ekonomiyi “dengelemek” için sonuç değiştirmez. Maddi bir recovery olduğunda iç raporda ve oyuncuya uygun genel bildirimde olay kimliği tutulur.

### 10.6 Ayrıntılı neden raporu

Her rapor en az şu alanları içerir:

| Alan | İçerik |
|---|---|
| `ReportId` | Tekil rapor kimliği |
| `BuildFingerprint` | Oyun sürümü, commit/build, platform ve katalog fingerprint'i |
| `SchemaVersion` | Rapor ve save şema sürümü |
| `DetectedAt` | Oyun zamanı ve monotonic oturum zamanı |
| `Severity` | Bilgi, uyarı, kurtarıldı, ciddi, kritik |
| `Subsystem` | Stok, satış, NPC, save, performans vb. |
| `InvariantId` | Bozulan açık kural |
| `Expected` / `Observed` | Beklenen ve görülen durum |
| `Entities` | Rapor/oturum kapsamlı, doğrudan kimlik taşımayan fakat ilişkilendirilebilir teknik ID'ler |
| `EventChain` | Hatanın öncesindeki sınırlı, sıralı olaylar |
| `Transaction` | İşlem kimliği ve commit/rollback durumu |
| `Evidence` | Sayaç, süre, state hash, konum bölgesi, test sonucu |
| `LikelyCause` | Kural tabanlı olası nedenler ve güven düzeyi |
| `Recovery` | Denenen eylem, ön/son durum ve başarı |
| `Impact` | Oyuncu, ekonomi, kayıt veya performans etkisi |
| `Reproduction` | Seed, sahne, iş türü ve yeniden üretim adımları |
| `Redaction` | Hangi alanların çıkarıldığı |

“Çalışan takıldı” yerine örnek neden zinciri:

> Görev `RESTOCK-1842`, Raf B-04 yaklaşım noktasını 46 saniye içinde üç kez planladı. Son geçerli rota, oyuncunun 08:14'te yerleştirdiği Vitrin D-02 sonrası kayboldu. İşçi ürünü hâlâ El Konteyneri E-17'de tutuyor; stok kaybı yok. Görev durduruldu, ürün karantina rafına taşındı ve nav erişim uyarısı üretildi.

Bu ayrıntı, “ne oldu, neden oldu, ne etkilendi, sistem ne yaptı?” sorularını tek raporda yanıtlar.

### 10.7 Oyuncuya görünürlük ve geliştirici erişimi

Guardian oyun içinde bir karakter, sohbet, Dashboard sayfası veya oyuncu aracı değildir. Ayarlarda yalnız anlaşılır gizlilik seçeneği bulunur: “İsteğe bağlı tanılama verisi gönder.” “Anonim” sözü verilmez; geliştirici modu retail build'de açılmaz.

Bir kullanıcının kendi bilgisayarındaki build'i veya dosyaları teknik olarak incelemesini mutlak biçimde engellemek mümkün değildir. Hedef “gizli ve okunamaz” bir güvenlik iddiası değil; oyuncuya kontrol yüzeyi sunmamak, gereksiz kişisel/linklenebilir veriyi toplamamak ve sistemi oyunu manipüle edemeyecek yetkide çalıştırmaktır.

### 10.8 Online raporlama ve gizlilik

Varsayılan davranış:

- Guardian tamamen offline çalışır.
- Yerel raporlar boyut sınırlı ring buffer'da tutulur.
- Otomatik gönderim **açık rıza/opt-in** olmadan yapılmaz.
- Gönderim tek yönlü HTTPS çıkışıdır; uzaktan komut kabul edilmez.
- Uygulama payload'ında kalıcı kullanıcı/Steam kimliği, ad, e-posta, arkadaş, serbest metin, dosya yolu veya ham IP alanı yoktur; teknik ID rapor/oturum kapsamına indirilir.
- Hassas değerler allowlist yaklaşımıyla dışarı alınır; “sonra redakte ederiz” yaklaşımı kullanılmaz.
- Ağ ve barındırma katmanı IP'yi teknik olarak görebileceğinden veri mutlak anonim değil, koşula göre pseudonymous/kişisel veri olabilir. Sunucu/CDN logları, processor/DPA, erişim kontrolü, aktarım ve saklama şifrelemesi, retention, silme/geri çekme ve veri sorumlusu yayın öncesi belgelenir.
- Çökme/bug raporu gönderilemezse oyun işleyişi etkilenmez.

AB veri minimizasyonu ve amaç sınırlaması için resmî GDPR ilkeleri yayın öncesi hukuk kontrolünün tabanıdır: [Avrupa Komisyonu GDPR ilkeleri](https://commission.europa.eu/law/law-topic/data-protection/rules-business-and-organisations/principles-gdpr/overview-principles/what-data-can-we-process-and-under-which-conditions_en), [IP ve pseudonymous veri kapsamı](https://commission.europa.eu/law/law-topic/data-protection/information-business-and-organisations/application-gdpr_en).

### 10.9 Steam ve AI beyanı

Steam içerik anketi ve güncel yayın kuralları release tarihinde tekrar cevaplanmalıdır. Guardian üretken AI olmadığı ve içerik üretmediği için öyle pazarlanmamalıdır. Kodlama asistanının geliştirme verimliliği için kullanılması tek başına burada otomatik bir sonuç olarak yorumlanmaz; asıl olarak oyuncuya ulaşan shipped AI üretimli içerik ve varsa live-generated kullanım, o tarihteki formda dürüstçe açıklanır: [Steam içerik anketi](https://partner.steamgames.com/doc/gettingstarted/contentsurvey?language=english).

### 10.10 Hard crash sınırı

Guardian oyun prosesinin içindedir; native crash, işletim sistemi sonlandırması veya güç kesintisinde her olayı yakalayamaz. Yapabilecekleri:

- Crash öncesi sınırlı breadcrumb/event ring buffer tutmak.
- Son başarılı save/transaction kimliğini kalıcılaştırmak.
- Sonraki açılışta “unclean shutdown” algılamak.
- Kullanıcı izin verirse redakte edilmiş önceki oturum raporunu göndermek.

Native crash dump/handler gerekiyorsa ayrı ürün kararıdır: lisans, binary boyutu, sembol sunucusu, redaksiyon, aktarım, saklama, erişim ve silme politikası ayrıca onaylanır. Steam Error Reporting modern çözüm varsayılmaz; Valve belgesi hizmetin ömrünün sonuna yaklaştığını ve istemcinin yalnız Windows 32-bit olduğunu belirtiyor: [Steam Error Reporting](https://partner.steamgames.com/doc/features/error_reporting).

Bu belge hukuki görüş değildir. Steam şartları ve mevzuat yayıma yakın tarihte yeniden doğrulanır.

## 11. Dashboard teknik sınırı

Dashboard, ayrı bir simülasyon oyunu olmayacak. Fiziksel ofis bilgisayarı/tabletinden açılan view model katmanıdır.

Dashboard'un yetkileri:

- Sipariş veya iş emri oluşturmak.
- Fiyat ve politika belirlemek.
- Vardiya/görev önceliği atamak.
- Finans, stok, pazar, garanti ve rapor görmek.

Dashboard'un yapamayacakları:

- Ürünü fiziksel kabul olmadan stoğa ışınlamak.
- PC'yi tek tıkla monte/test/paketlemek.
- Servisi zar atışıyla bitirmek.
- Teslim edilmiş fiziksel ürünü geriye dönük fiyatlandırmak.
- Nav veya kapasite engelini yok sayarak çalışan işi tamamlamak.

UI salt kopya state tutmaz. Komut gönderir, alan servisi doğrular ve yeni view model olaylardan üretilir.

## 12. Performans ve ölçek planı

### Geçici hedefler

Nihai minimum/önerilen sistem gereksinimi vertical slice Windows profillemesi olmadan ilan edilmeyecek. İlk mühendislik hedefi:

- 1080p'de ölçeklenebilir ayarlarla tutarlı 60 FPS hedefi.
- Daha düşük donanım için 30 FPS seçeneği.
- 16 GB RAM ve SSD'yi geliştirme referansı olarak sınamak.
- GTX 1660 Super / benzer sınıf ve daha düşük bir test PC'sinde ölçüm yapmak.

Bu, yayımlanmış sistem gereksinimi değil, test hipotezidir.

### Bütçe ilkeleri

- 60 FPS için toplam kare yaklaşık 16,7 ms; CPU ve GPU eşzamanlı bütçeleri profiler verisiyle ayrılır.
- NPC kararları her kare hesaplanmaz; farklı frekansta güncellenir.
- Görüş dışı ürünler tam rigidbody olarak simüle edilmez.
- Raf ürünü instance/LOD/batching kullanır; değerli tekil durum alan çekirdeğinde korunur.
- Fiziksel aktif kutu ve elde taşınan nesne sayısına sahne bütçesi konur.
- Uzak şubeler fiziksel simülasyon kullanmaz.
- GC allocation, save spike, nav yeniden hesaplama ve UI listeleri profiler testlerine dahildir.

### Ölçülmeden optimizasyon yok, ölçülmeden kapsam artışı da yok

Her milestone için referans sahne ve otomatik performance capture saklanır. Katalog veya NPC sayısı artırılırken yalnız ortalama FPS değil yüzde 1 düşük kareler, ana thread spike, save süresi ve bellek tepe değeri izlenir.

## 13. Test stratejisi

| Katman | Ne test edilir | Örnek |
|---|---|---|
| Saf birim | Tek kural ve hesap | PSU payı, COGS, vergi karşılığı, garanti günü |
| Property/fuzz | Çok sayıda veri kombinasyonu | Stok miktarı asla negatif olmamalı |
| Katalog doğrulama | ID, çeviri, fiyat, prefab, uyumluluk | Her CPU ailesinin en az bir satılabilir build'i |
| Edit Mode | Unity dışı/asset bağı | Serializer, migration, view model |
| Play Mode | Sahne etkileşimi | Kutuyu kabulden rafa aktarma |
| Entegrasyon | Birden çok sistem | Satış → stok → ledger → itibar → save |
| Golden ekonomi | Seed'li uzun simülasyon | 365 gün; para yaratılmıyor; iflas basamakları adil |
| AI soak | NPC/görev kuyruğu | 8–24 saat hızlandırılmış mağaza; kayıp item yok |
| Save fault injection | Yazma/çökme/bozulma | Yarım dosya, eski şema, cloud çatışması |
| Assembly matrisi | Mantıksal ve geometrik eşleşme | Kasa–GPU–radyatör–anakart kombinasyonları |
| Erişilebilirlik | Input/görsel/süre | Rebind, controller-only, renk körlüğü, motion reduce |
| Platform | Windows/Steam/donanım | Temiz kurulum, overlay, cloud, ultrawide, farklı GPU |
| İnsan oynanış testi | Anlaşılabilirlik ve eğlence | Oyuncu yardım almadan ilk satış/PC işini tamamlıyor mu? |

Unity Test Framework proje başlangıcında kullanılır: [Unity Test Framework](https://docs.unity3d.com/Manual/testing-editortestsrunner.html). Otomatik testler insan oynanış testinin yerine geçmez.

### Hata öncelikleri

- **P0:** kayıt kaybı, güvenlik/gizlilik, açılmama, kalıcı ekonomi bozulması.
- **P1:** ana döngü bloklanması, çözülemez iş, kaybolan stok, yaygın crash.
- **P2:** önemli yanlış sonuç, ciddi NPC/UI/performance sorunu; workaround var.
- **P3:** kozmetik, nadir sunum veya küçük kullanım sorunu.

Vertical slice'ta açık P0/P1 ile yeni büyük sistem eklenmez.

## 14. Erişilebilirlik teknik tabanı

Başlangıçtan kurulacaklar:

- Unity Input System ile klavye/fare ve gamepad action map'leri.
- Her eylem için yeniden atama ve çakışma uyarısı.
- Hold/toggle, hassasiyet, deadzone ve invert seçenekleri.
- Cihaz değişince otomatik doğru glyph.
- UI ölçeği, güvenli alan, ultrawide ve yüksek DPI testleri.
- Renge ek şekil/metin/ikon; kontrast profilleri.
- Hareket azaltma, head-bob kapatma, FOV ve kamera sarsıntısı.
- Montaj için büyük hedef/snap toleransı ve süre baskısı seçenekleri.
- Tüm bilgi için metin; tam seslendirme zorunluluğu yok.
- Öğretici yeniden girilebilir, sırası esnek, resetlenebilir ve atlanabilir.

Kaynaklar: [Unity Input System](https://docs.unity3d.com/Manual/com.unity.inputsystem.html), [Xbox Accessibility Guidelines](https://learn.microsoft.com/en-us/xbox/accessibility/guidelines), [Unity Practical Game Accessibility](https://learn.unity.com/course/practical-game-accessibility).

## 15. Araç planı, maliyet ve disk etkisi

Rakamlar yaklaşık planlama değeridir; sürüm, cache ve seçilen modüllere göre değişir. Her kurulumdan önce güncel indirme boyutu ve lisans yeniden gösterilir.

| Araç | Amaç | Öneri | Maliyet | Yaklaşık disk etkisi | Ne zaman |
|---|---|---|---|---:|---|
| Unity Hub + Unity 6.3 LTS + URP | Ana motor | Zorunlu | Personal sınırlarında ücretsiz | 15–25 GB; modül/cache ile artar | Onay sonrası ilk kurulum |
| Mac: Windows Build Support (Mono) | Erken Windows x64 build | Zorunlu | Unity ile | Birkaç GB ek | İlk oynanabilirden önce |
| Windows editör + IL2CPP toolchain | Final native Windows x64 build | Zorunlu dış bağımlılık | Unity ile; Windows PC erişimi gerekir | Windows cihazda ayrıca motor/cache | İlk oynanabilir test kapısından itibaren |
| Kod editörü | C# | Önce ücretsiz destekli editör; ücretli Rider yalnız verim kanıtlanırsa | €0 başlangıç | 1–5 GB | Proje kurulumu |
| Blender | Model, UV, basit rig/animasyon | Zorunlu üretim aracı | Ücretsiz/açık kaynak | Yaklaşık 2–4 GB + cache | Graybox sonrası |
| Git veya Unity Version Control | Sürüm kontrolü | Aşağıdaki karara göre | Ücretsiz kota ile başla | Repo + geçmiş | İlk proje dosyasından önce |
| Audacity | Kayıt ve basit ses düzenleme | Başlangıç için yeterli | Ücretsiz/açık kaynak | 1 GB'dan az + ses | Ses prototipi |
| REAPER | Gelişmiş ses üretimi | Yalnız ihtiyaç kanıtlanırsa | İndirimli lisans güncel olarak 60 USD | Küçük; projeler ayrıca | Sonraki üretim |
| Krita/GIMP benzeri | 2D/UI/doku düzenleme | İhtiyaca göre ücretsiz | Ücretsiz | 1–3 GB | UI/art üretimi |
| Issue board | İş/kabul kriteri takibi | Önce repo/UVCS ile basit ücretsiz çözüm | €0 | İhmal edilebilir | Proje açılışı |
| Profiler/Frame Debugger | Performans | Unity yerleşik araçları | €0 | Motor içinde | İlk prototipten itibaren |
| Steamworks SDK | Steam entegrasyonu | AppID sonrasında | Steam Direct ücreti ayrı | Küçük | Vertical slice sonrası |

Unity ve bağımlılık cache'leri nedeniyle gerçek çalışma alanı motor boyutundan büyüktür. Güvenli rezerv:

- Teknik prototip: toplam **50–80 GB** boş alan.
- Vertical slice: proje, cache, build ve yedeklerle **120–200 GB** rezerv.
- Tam üretim: asset politikası ve geçmişe göre yeniden hesaplanır.

### Sürüm kontrolü önerisi

İlk öneri **Unity Version Control (UVCS)** ile tek authoritative Unity deposu ve ayrıca tarihli, çevrimdışı yedektir. Gerekçe: binary varlık kilitleme ve Unity odaklı akış; Unity'nin güncel ücretsiz başlangıç kotası 25 GB olarak belirtiliyor. Kurulum gününde fiyat/kota doğrulanır: [Unity DevOps fiyatlandırması](https://docs.unity.com/en-us/devops/pricing).

Alternatifler:

- **Git + Git LFS:** kod ve açık geçmiş için çok iyi; binary geçmişi hızla kota tüketir. GitHub Free LFS güncel belgede 10 GiB depolama ve 10 GiB aylık bant genişliği içerir; her yeni binary sürümü tam dosya boyutu sayılır: [Git LFS faturalandırması](https://docs.github.com/en/billing/concepts/product-billing/git-lfs?apiVersion=2022-11-28).
- **Perforce Helix Core:** binary ve kilitlemede güçlü; küçük ekip için self-host ücretsiz sınırı vardır fakat sunucu yönetimi solo başlangıç için fazladır: [Helix Core fiyatlandırması](https://www.perforce.com/resources/vcs/helix-core-pricing).

UVCS seçimi kurulum onayı değildir. USB kaynak hash'i, yedek hedefi ve hesap sahipliği netleştikten sonra ayrı kapıda uygulanır. `Library`, `Temp`, cache ve build çıktıları sürüm kontrolüne alınmaz.

### Yedek 3-2-1 hedefi

- 3 kopya: çalışma, yerel/harici yedek, uzak sürüm kontrolü.
- 2 farklı ortam.
- 1 kopya fiziksel olarak ayrı/offsite.

USB tek başına yedek değildir. Aynı şekilde bulut deposu da sürüm kontrolünün yerine geçmez.

## 16. 3D varlık ve lisans politikası

### Üretim sırası

1. Ölçek ve etkileşim graybox'ı.
2. Modüler mağaza kit'i.
3. Hero eller, PC kasası ve montaj istasyonu.
4. Ürün aileleri için ortak geometrik standart.
5. LOD, collider, pivot, snap ve materyal doğrulaması.
6. Dekor ve varyantlar.

Hero eller/montaj, rastgele ucuz asset paketine bırakılmamalıdır; oyunun hissini belirler. Genel kutu, raf ve küçük dekorlar uygun lisanslı kitlerle hızlandırılabilir.

### Teknik varlık standardı

Her 3D varlık için:

- Metre ölçeği ve eksen standardı.
- Pivot ve snap noktaları.
- Basit, ayrı collision mesh.
- LOD veya açık “LOD gereksiz” kararı.
- Materyal/doku isimleri ve 2K varsayılanı.
- Kaynak dosya, export ayarı ve lisans kaydı.
- Kurgusal logo/ürün tasarımı.
- Performans bütçesi ve test prefab'ı.

### Ücretsiz kaynaklar

- Poly Haven varlıkları CC0 olarak sunulur: [Poly Haven lisansı](https://polyhaven.com/license).
- Kenney varlıklarının oyun varlıkları CC0 olarak yayımlanır: [Kenney destek/lisans bilgisi](https://kenney.nl/support).
- Blender GPL lisansı, Blender ile üretilen sanat çıktısını otomatik olarak GPL yapmaz: [Blender lisans açıklaması](https://docs.blender.org/manual/en/4.3/getting_started/about/license.html).

CC0 dahi bir fotoğraftaki marka, kişi, patent veya ayırt edici ürün tasarımı için her hakkı otomatik temizlemez. Her indirilen varlığın kaynak URL'si, tarih, lisans metni, yazar ve değişiklikleri `Asset Register` içinde saklanır. Asset Store içeriği Unity Asset Store şartlarına göre ayrı izlenir.

### Gerçek marka kullanımı

Temel plan kurgusal markalardır. Gerçek marka/model, logo, kutu tasarımı veya özgün ürün geometrisi yalnız:

- Yazılı lisans/izin.
- Coğrafi ve platform kapsamı.
- Süre ve kaldırma şartı.
- 3D model/görsel sağlama hakkı.
- Güncelleme ve uyumluluk test bütçesi.

netleşirse değerlendirilir. Eski oyundaki hard-coded gerçek marka listesi otomatik taşınmaz.

## 17. Ses ve müzik

- İlk prototipte işlevsel sesler: alma, bırakma, snap, vida, kutu, kasa, hata ve başarı.
- Ses, montaj doğruluğunun ikinci kanalıdır; yalnız dekor değildir.
- Müzik özgün, sipariş edilmiş veya açıkça ticari kullanıma uygun lisanslı olmalı.
- Her dosya için source/license/author/edit kaydı tutulur.
- Tam seslendirme yerine kısa evrensel NPC sesleri ve metin kullanılır.
- Müşteri hikâyesi ses dosyasına kilitlenmez; TR/EN güncellemesi sürdürülebilir kalır.

Audacity ücretsiz/açık başlangıç aracıdır: [Audacity](https://www.audacityteam.org/). REAPER ancak gelişmiş düzenleme ihtiyacı kanıtlanırsa düşünülür: [REAPER lisansı](https://www.reaper.fm/purchase.php).

## 18. Steam, Windows ve macOS yayın kapıları

### Steam

Steam Direct başvuru ücreti uygulama başına güncel olarak 100 USD'dir ve ürün 1.000 USD Adjusted Gross Revenue'a ulaştığında geri kazanılabilir; yayıma kadar fiyat ve koşul tekrar doğrulanır: [Steam Direct Fee](https://partner.steamgames.com/doc/gettingstarted/appfee).

Planlanan kapılar:

- Steamworks hesabı ve vergi/banka onboarding'i.
- Final isim ve marka benzerlik araştırması.
- Store sayfası varlık kuralları ve dürüst özellik listesi.
- İçerik anketi ve AI kullanımı beyanı.
- Steam Input/Cloud/başarım testleri.
- Steam Playtest veya demo ile kontrollü geri bildirim.
- Valve build/store review sürelerine tampon.
- Release checklist ve iki haftalık “coming soon” gibi güncel zorunlulukları resmî sayfadan doğrulama.

Kaynaklar: [Steam onboarding](https://partner.steamgames.com/doc/gettingstarted/onboarding), [store/build review](https://partner.steamgames.com/doc/store/review_process?language=english), [releasing](https://partner.steamgames.com/doc/store/releasing?language=english), [Steam Playtest](https://partner.steamgames.com/doc/features/playtest?language=english).

Oyuncu incelemeleri araştırma için okunabilir; Steam kullanıcı yorumları reklama/promosyona izinsiz taşınmamalıdır: [Steam User Reviews](https://partner.steamgames.com/doc/store/reviews).

### Türkiye ve fikrî haklar

Türkiye Kültür ve Turizm Bakanlığının güncel sayfası yerli veya ithal bilgisayar oyunlarını zorunlu kayıt-tescil kapsamına dahil etmektedir. Ücretler ve süreç yayıma yakın yeniden doğrulanmalı; uzman hukuk/muhasebe desteği alınmalıdır: [Zorunlu kayıt-tescil](https://telifhaklari.ktb.gov.tr/TR-332371/zorunlu-kayit-tescil.html), [eser sahibi açıklaması](https://telifhaklari.ktb.gov.tr/TR-332390/eser-sahibi-kimdir.html), [TÜRKPATENT araştırma](https://www.turkpatent.gov.tr/arastirma-yap?form=patent).

Bu belge hukuki veya vergi danışmanlığı değildir.

### macOS, Windows 1.0 sonrasında

macOS portunun ek işleri:

- Metal shader ve platform asset bundle testi.
- Apple Silicon/Intel hedef kararının güncel pazarla verilmesi.
- Dosya yolu, izin, klavye ve pencere davranışı.
- Steam macOS depot'u.
- Developer ID imzası ve notarization.
- Ayrı performans/QA turu.

Apple Developer Program güncel olarak yıllık 99 USD'dir; o tarihte yeniden kontrol edilir: [Apple Developer Program](https://developer.apple.com/programs/). Notarization: [Apple belgeleri](https://developer.apple.com/documentation/security/notarizing_macos_software_before_distribution).

Windows sürümü tamamlanmadan macOS release tarihi verilmez. Ancak shader, dosya formatı ve platform arayüzleri baştan taşınabilir tutulur.

## 19. En büyük teknik riskler ve karşılıkları

| Risk | Olasılık/etki | Erken karşılık |
|---|---|---|
| Kapsamın solo üretimi aşması | Çok yüksek / çok yüksek | Küçük vertical slice, aşama kapıları, sistem eklemeden önce kalite borcunu kapatma |
| PC geometrik kombinasyon patlaması | Yüksek / çok yüksek | Kurgusal az aile, standart ölçüler, otomatik matris ve fixture testi |
| Stok/ekonomi desync | Orta / çok yüksek | Tek authoritative durum, atomik transaction, Guardian invariant |
| Save kaybı | Orta / çok yüksek | Atomik save, journal, döner yedek, fault injection, cloud çatışma ekranı |
| NPC pathfinding ve placement | Yüksek / yüksek | Yaklaşım noktaları, placement doğrulama, task reservation, LOD/timeout |
| Fizik kaynaklı nesne kaybı | Orta / yüksek | Ekonomik state–projection ayrımı, son güvenli konum, karantina |
| Mac'te iyi, Windows'ta kötü build | Yüksek / yüksek | İlk oynanabilirden itibaren Windows kapıları |
| Fazla gerçekçilikten angarya | Yüksek / yüksek | Ustalık kısayolları, delegasyon, açık otomasyon ilkesi |
| Asset/lisans belirsizliği | Orta / çok yüksek | Asset register, kurgusal marka, kaynak kanıtı, release hukuk kontrolü |
| Guardian'ın “self-healing AI” diye yanlış anlaşılması | Orta / yüksek | Doğru adlandırma, yetki sınırı, opt-in, Steam beyan denetimi |
| Fansız Mac'te import/build darboğazı | Orta / orta | Küçük paketler, cache disiplini, Windows/build makinesi, ölçüm |
| Ücretsiz araçların dolaylı işçilik maliyeti | Yüksek / orta | Yüksek etkili cüzi ücret için ROI kapısı |

## 20. Kurulum öncesi onay kapısı

Henüz hiçbir araç kurulmayacak. İlk uygulama kapısında kullanıcıya tek paket hâlinde şu kesin bilgiler sunulmalıdır:

1. Seçilen başlangıç Unity 6.3 LTS yama sürümü ve alpha öncesi LTS yükseltme kapısı.
2. Kurulacak modüller ve güncel tahmini disk kullanımı.
3. Kod editörü seçimi ve lisansı.
4. UVCS/Git kararı, hesap sahipliği ve yedek hedefleri.
5. İlk prototip klasörü ile eski kaynak arasında fiziksel ayrım.
6. USB canonical kaynak hash doğrulaması — **tamamlandı, 11 Ağustos 2026; 26/26 eşleşme**.
7. Geri alma ve kaldırma planı.

Bu kapı onaylanmadan Unity projesi, repo, paket veya ücret oluşturulmaz.

## 21. Teknik “tamam” ölçütü

Temel mimari ancak şu kanıtlarla kabul edilir:

- Aynı ürün kimliği sipariş, kutu, raf, sepet, iş emri ve Dashboard'da kaybolmadan izleniyor.
- Satış işlemi yarıda kesildiğinde para ve stok eski sağlam durumda.
- Seed'li PC işi en az bir geçerli çözüm kanıtı taşıyor.
- Kayıt yazarken zorla kapanma testinde önceki save yükleniyor.
- NPC görevi timeout olduğunda ürün kaybolmuyor.
- Guardian raporu neden, etki, kanıt ve recovery zincirini içeriyor.
- Mac'te üretilen çalışma Windows gerçek donanımında aynı temel akışı tamamlıyor.
- Tüm bunlar sahneye bağlı olmayan otomatik testlerle tekrarlanabiliyor.

Bu kanıtlar olmadan katalog, şube veya çalışan sayısını büyütmek ilerleme değil, teknik borç büyütmektir.
