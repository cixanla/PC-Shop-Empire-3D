# PC Shop Empire 3D — Yaşayan Proje Bible ve Ana Handoff

**Belge rolü:** Projenin ana fikrini, güncel durumunu, teknik sınırlarını, yapılmış ve yapılacak işleri tek giriş noktasında tutar.  
**Son kapsam güncellemesi:** 15 Ağustos 2026
**Authoritative ayrıntılar:** [`Docs/ProjectBible/`](Docs/ProjectBible/) ve tarihli ADR'ler.  
**Güncelleme kuralı:** Her GitHub checkpoint/pull request, etkilediği durum ve sıradaki işi bu belgede güncellemek zorundadır.

## 1. Kuzey yıldızı

PC Shop Empire 3D, oyuncunun küçük bir garajda başlayıp fiziksel olarak büyüyen bir teknoloji perakende ve servis işletmesi kurduğu birinci şahıs 3D simülasyondur.

Oyunun ayırt edici birleşimi:

> **Fiziksel teknoloji perakendesi + teknik PC ustalığı + uzun vadeli müşteri güveni.**

Oyuncu yalnız menülerde sayı yönetmez. Siparişi terminalden verir; teslimatı fiziksel olarak kabul eder, kutuyu taşır, ürünü depoya veya rafa yerleştirir, müşteriye danışmanlık yapar, kasayı işletir ve özel PC'yi tezgâhta parça parça toplar. Dashboard yönetim katmanıdır; fiziksel işi sihirli biçimde tamamlamaz.

## 2. Ürün hedefi ve platform sırası

| Alan | Kilit karar |
|---|---|
| Ana platform | Windows x64 ve Steam zorunlu |
| Geliştirme bilgisayarı | Apple Silicon MacBook günlük geliştirme/prototip için uygun |
| Windows doğrulaması | İlk oynanabilirden önce gerçek Windows x64 PC; vertical slice sonrası düzenli native test |
| macOS | Windows/Steam 1.0 tamamlandıktan ve bütçe uygun olduktan sonra ayrı port, imzalama, notarization ve QA |
| Linux | Zorunlu değil; düşük maliyetle mümkünse daha sonra değerlendirilebilir |
| Kamera | Birinci şahıs; görünür eller ve fiziksel iş animasyonları |
| İş modeli | Öncelik premium tek oyunculu Steam oyunu; manipülatif monetization yok |
| Motor | Unity 6000.3.21f1 + URP başlangıç tabanı; alpha öncesi kontrollü LTS yükseltme kapısı |
| Sanat yönü | Okunaklı yarı gerçekçilik: gerçek oran/PBR malzeme/zemine oturan ışık/doğal ağırlık, hafif stilize okunabilirlik; fotogerçekçilik veya gerçek marka/asset kopyası yok ([ADR-0013](Docs/ADR-0013-READABLE-SEMI-REALISTIC-VISUAL-DIRECTION.md)) |
| Araç bütçesi | Ücretsiz araçlar varsayılan; yalnız büyük ve ölçülebilir etki sağlayan düşük maliyetli araç ayrı onay kapısı |

## 3. Oyuncu fantezisi

Oyuncu üç rolü aynı işletmede birleştirir:

1. **Mağaza sahibi:** nakit, kira, fiyat, stok, tedarikçi, itibar ve büyümeyi yönetir.
2. **Saha operatörü:** teslimat, kutu, raf, kasa, temizlik, güvenlik ve mağaza düzeniyle fiziksel olarak ilgilenir.
3. **PC uzmanı:** ihtiyacı dinler, uyumlu sistem tasarlar, parça parça toplar, test eder, paketler, teslim eder ve satış sonrasını yürütür.

Uzun vadeli bağ yalnız daha büyük sayılar değildir. Oyuncu garajdan mahalle dükkânına, gelişmiş teknoloji mağazasına ve çok bölümlü profesyonel işletmeye geçerken operasyon, müşteri beklentisi, risk ve uzmanlık da değişir.

## 4. Ana oyun döngüleri

### Günlük mağaza döngüsü

`Planla → sipariş ver → teslimatı kabul et → taşı/depolandır → rafla/fiyatlandır → müşteriye hizmet et → kasa/teslim → kapanış ve analiz`

Her aşama aynı authoritative stok ve para gerçeğini kullanır. Ürün hem ekonomide satılmış hem rafta fiziksel olarak var olamaz.

### Özel PC işi

`İhtiyaç görüşmesi → bütçe/öncelik → teklif → parça rezervasyonu → fiziksel montaj → kablo/soğutma → kurgusal OS → test/benchmark → kalite kapısı → paketleme → teslim/satış sonrası`

Uyumsuz, eksik veya kalitesiz montaj; yeniden iş, gecikme, masraf, müşteri memnuniyetsizliği veya arıza riski doğurur. Sistem gerçekçilik uğruna yorucu vida grind'ına dönüşmez; ustalıkla güvenli hızlandırmalar açılır.

### Servis ve yenileme döngüsü

`Cihaz kabulü → görünür kondisyon/veri izni → belirti → teşhis hipotezi → müşteri onayı → onarım/parça → test → temizlik → teslim/garanti`

İkinci el alım-satım, refurbish ve parça geri kazanımı aynı seri/kondisyon/maliyet kayıtlarına bağlanır.

### Büyüme döngüsü

`Kârlı ve güvenilir işletme → yeni kapasite → daha karmaşık müşteri/ürün → çalışan ve uzmanlık → yeni hizmet → bölge/şube`

Büyüme yalnız alan açmaz; kira, stok riski, hizmet standardı, rekabet, çalışan koordinasyonu ve tedarik ilişkisi de zorlaşır.

## 5. Dünya ve fiziksel oynanış

### Oyuncu

- Birinci şahıs hareket, hassasiyet/FOV ve yeniden atanabilir kontroller.
- Görünür eller; alma, bırakma, inceleme, kutu açma ve araç kullanma.
- Küçük nesnede elde taşıma; büyük kutuda görüş/hız kısıtı; ağır işte taşıma arabası.
- Serbest rigidbody kaosu yerine hassas görevlerde yönlendirilmiş snap ve doğrulanmış etkileşim.
- Erişilebilir hedef büyütme, hold/toggle seçenekleri, renk dışı işaretler ve hareket rahatsızlığı ayarları.

### Mağaza

- Raf, vitrin, kasa, depo, atölye, ofis, servis kabulü, teslimat alanı, güvenlik ve dekorasyon.
- Yerleştirme; serbest önizleme + grid/snap + erişim/nav doğrulaması.
- Raf planogramı bir zorunlu puzzle değil; okunabilirlik, kapasite ve müşteri bulma süresini etkileyen yönetim aracıdır.
- Fizik projeksiyonu bozulsa bile ekonomik ürün silinmez; son güvenli konum/karantina mekanizması kullanılır.

## 6. Ürün ve PC sistemi

İlk ürün aileleri; CPU, GPU, anakart, RAM, SSD/HDD, PSU, kasa, soğutucu, fan, monitör, klavye, mouse, kulaklık, webcam, oyun kolu, kablo, termal macun ve hazır sistemleri kapsar.

Her ürün tanımı şunları ayırır:

- Kurgusal marka, model, nesil ve kategori.
- Teknik özellik/uyumluluk.
- Performans, kalite, garanti ve arıza profili.
- Alış maliyeti, piyasa/talep ve önerilen fiyat sinyali.
- Fiziksel boyut, kutu/raf davranışı ve görsel varlık bağlantısı.

Her fiziksel ürün örneği ayrıca instance/batch kimliği, maliyet, tedarikçi, kondisyon, garanti, rezervasyon, konum ve test/hasar geçmişi taşır.

Uyumluluk tek bir yeşil/kırmızı sonuç değildir. Soket, chipset/BIOS, RAM nesli/kapasitesi, form factor, PSU güç/connector/headroom, GPU/soğutucu fiziksel açıklığı, depolama bağlantısı, termal yük ve müşteri gereksinimi ayrı neden kodlarıyla değerlendirilir.

## 7. Müşteriler ve çalışanlar

### Müşteri modeli

Müşteriler; bütçe, amaç, teknik bilgi, sabır, zaman baskısı, kalite/garanti hassasiyeti, marka eğilimi, pazarlık, sadakat ve geçmiş deneyim bakımından ayrılır.

Davranış zinciri:

`Giriş → yön bulma → göz atma → ürün/yardım → değerlendirme → kasa/teklif → çıkış → yorum/iade/takip`

AI sonsuza kadar aynı hedefe yürüyemez; her durumda timeout, yeniden çözümleme ve güvenli fallback vardır. Müşteri kararı anlaşılır nedenlere dayanır; gizli hileyle stok veya para üretilmez.

### Çalışan rolleri

- Satış danışmanı
- Kasa görevlisi
- PC teknisyeni
- Depo/raf çalışanı
- Temizlik görevlisi
- Yönetici
- Güvenlik görevlisi

Hız, uzmanlık, hata riski, maaş, eğitim, memnuniyet, güvenilirlik ve uzmanlık alanı farklıdır. Oyuncu her hareketi tek tek söylemek yerine görev ve politika verir; kritik kalite kapıları yetkin kişi onayı ister.

## 8. Ekonomi ve işletme

- Tek authoritative stok, rezervasyon ve transaction gerçeği.
- Nakit, gelir, COGS, kira, maaş, vergi karşılığı, fatura, kredi ve vadeli ödeme ayrımı.
- Talep, yeni ürün çıkışı, değer kaybı, tedarik kıtlığı, kampanya ve müşteri trendi.
- Fiyat değişimi işlem ortasında satış sonucunu değiştiremez.
- Tedarikçi; fiyat, minimum sipariş, kalite, teslim süresi, vade, hasar/eksik risk ve ilişki bakımından ayrılır.
- Şoklar bounded ve önceden sinyallidir; save reload ederek reroll yapılamaz.
- İflas ani ekran değildir: uyarı → nakit baskısı → kısıtlı seçenek → yeniden yapılandırma → kontrollü başarısızlık. Oyuncu isterse daha düşük zorlukla veya yeni şirket koşuluyla yeniden başlayabilir.

Gelir çeşitleri mağaza satışı dışında özel PC, servis, ikinci el/refurbish, online sipariş, kurumsal/okul/ofis anlaşması, e-spor sistemi ve ileride uygun kiralama modellerini içerir.

## 9. Dashboard

Dashboard fiziksel ofis bilgisayarı/tablet/terminal üzerinden açılır. Varsayılan olarak zamanı durdurabilir; isteyen oyuncu canlı simülasyonu seçebilir.

Ana modüller:

- Özet/KPI ve operasyon uyarıları
- Parça pazarı ve tedarikçi siparişi
- Stok ve seri/kondisyon takibi
- Fiyatlandırma ve kampanya
- Finans/muhasebe ve rapor
- Personel, vardiya, maaş ve görev
- Müşteri/özel PC/servis siparişleri
- Mağaza yükseltmeleri ve yerleşim planı
- Reklam, itibar ve müşteri yorumları
- Anlaşmalar ve tedarikçi ilişkileri
- Borç, vergi, kira ve faturalar
- Pazar trendleri ve rakip sinyalleri
- Garanti, iade, RMA ve servis
- Kariyer/hedef/başarım geçmişi

Dashboard sipariş verir ama kutuyu rafa ışınlamaz; PC işi kabul eder ama montaj/test/teslimi otomatik üretmez.

## 10. PSE Guardian sınırı

PSE Guardian, yayınlanan oyunun içinde oyuncuya kapalı çalışan bir tanı ve bütünlük katmanıdır; ChatGPT/OpenAI bağımlılığı değildir.

Yapabilecekleri:

- Olay zinciri ve invariant ihlali kaydetmek.
- Duplicate event, stok–dünya uyuşmazlığı, negatif para/quantity, takılmış görev ve bozuk save sinyali bulmak.
- Yalnız önceden tanımlı güvenli toparlamaları uygulamak.
- Offline ayrıntılı yerel rapor üretmek; açık opt-in ile online olduğunda pseudonymous rapor göndermek.

Yapamayacakları:

- Kendi kendine kaynak kodu değiştirmek.
- Codex/insan onayı olmadan patch indirmek veya oyun kuralı yazmak.
- Oyuncuya gizli avantaj/dezavantaj sağlamak.
- Para, ürün veya karar sonucu uydurmak.
- Kullanıcı dosyalarını ya da kişisel veriyi izinsiz toplamak.

Hard/native crash aynı proses içinden her zaman yakalanamaz; breadcrumb ve sonraki açılışta unclean-shutdown tespiti kullanılır. Crash SDK/online telemetry ayrı lisans, gizlilik ve onay kapısıdır.

## 11. Teknik mimari

Alan mantığı Unity nesnelerinden ayrıdır. Unity; input, fizik, animasyon, ses ve sunum adaptörüdür. Para, stok, uyumluluk ve iş kuralları saf C# modüllerinde test edilir.

| Modül | Sorumluluk |
|---|---|
| `PSE.Core` | Stable ID, sonuç/failure, deterministik zaman, sürümlü PRNG, event sözleşmeleri, temel invariant |
| `PSE.Catalog` | Ürün tanımı, teknik özellik, kalite, garanti |
| `PSE.Inventory` | Instance/batch, konteyner, konum, rezervasyon, kondisyon |
| `PSE.Orders` | Satın alma, satış, özel PC, servis ve kurumsal iş emirleri |
| `PSE.Economy` | Ledger, nakit, COGS, borç ve ödeme takvimi |
| `PSE.Retail` | Fiyat, sepet, checkout, kampanya, iade/garanti |
| `PSE.Assembly` | Build graph, uyumluluk, montaj, kalite ve benchmark |
| `PSE.Service` | Intake, teşhis, onarım, RMA ve refurbish |
| `PSE.Actors` | Müşteri/çalışan profili, ihtiyaç ve görev durumu |
| `PSE.World` | 3D etkileşim, placement, station ve nav rezervasyonu |
| `PSE.Dashboard` | Salt-okunur view model ve yetkili komutlar |
| `PSE.Save` | Sürümlü snapshot, journal, migration ve recovery |
| `PSE.Guardian` | Gözlem, invariant, anomali ve güvenli toparlama |
| `PSE.Presentation` | Unity sahne, prefab, animasyon, VFX, ses ve UI |
| `PSE.Platform` | Dosya sistemi, Steam, cloud ve izinli telemetry adaptörleri |

Bağımlılık yönü sunumdan alana doğrudur; `PSE.Core` Unity/Editor referansı taşımaz. Dairesel bağımlılık ve Dashboard'un sahne nesnesini doğrudan authoritative state olarak düzenlemesi kabul edilmez.

## 12. Determinizm, kayıt ve güvenlik

- Oyun zamanı integer ve açık fixed-step clock üzerinden ilerler; pause sırasında ilerlemez.
- Eventler stable ID/type, one-based sequence, schema, simulation timestamp ve zorunlu correlation/direct-causation bağlamı taşır; in-memory dispatcher global FIFO, breadth-first nested enqueue, duplicate/conflict ve handler hata izolasyonu uygular.
- Temel PRNG `pcg32-xsh-rr-64-32-v1` kimliğiyle sürümlüdür; raw state+odd increment snapshot/restore ve bias üretmeyen bounded integer davranışı testlidir.
- Root RNG seed save-safe canonical hex taşır; sürümlü SHA-256 framed domain/context türetmesi çağrı sırasından bağımsız PCG32 akışı üretir. Eksik veya bilinmeyen save metadata'sı sessiz fallback yapmaz; reload-reroll çekirdek testleriyle engellenir.
- Save; sürümlü snapshot, sınırlı journal, checksum, katalog fingerprint ve döner sağlam kopyalar kullanır.
- Yazma geçici dosya → flush/doğrulama → atomik replace yaklaşımıyla yapılır; gerçek platform fault-injection testi olmadan “kayıp olmaz” iddiası kurulmaz.
- Steam Cloud çatışması kullanıcıdan habersiz son-yazan-kazan yapmaz.
- Secret, token, sertifika, kişisel telemetry ve build cache Git'e girmez.

## 13. Kapsam ve tekrar önleme

Vertical slice'ın kilit çekirdeği:

- Tek garaj ve teslimat önü.
- Birinci şahıs hareket, görünür eller ve hibrit taşıma.
- Sipariş → fiziksel teslimat → depo/raf → fiyat → müşteri → kasa.
- Baştan sona tek özel PC işi.
- Temel Dashboard, ekonomi, save/recovery ve Guardian olay zinciri.
- Yaklaşık 50–80 anlamlı SKU; teknik prototipte daha az.

Vertical slice'a çalışan ordusu, şube ağı, yüzlerce ürün, geniş servis, tam online satış ve final sanat yığılmaz. Önce zincirin doğruluğu ve eğlencesi kanıtlanır.

Monotonluğu azaltma ilkeleri:

- Ustalıkla güvenli otomasyon; ayrıntılı moda geri dönüş.
- Toplu ama açıklanabilir görev/politika atama.
- Aynı fiziksel işi amaçsız tekrar ettirmeyen ergonomik etkileşim.
- Kriz ve trendlerin önceden sinyalli olması.
- İçerik sayısının değil yeni karar üretmesinin ölçülmesi.
- Zorlayıcı fakat adım adım, kurtarılabilir finansal başarısızlık.

## 14. Yol haritası ve güncel durum

| Faz | Hedef | Durum |
|---:|---|---|
| 0 | Keşif, ortak anlayış, kaynak güvenliği | Tamamlandı |
| A | Unity/paket/build/VCS teknik kurulum | Tamamlandı; private GitHub authoritative, UVCS beklemede |
| 1 | Proje temeli ve graybox etkileşim | Devam ediyor; hareket, küçük kutu pickup/drop/placement/rotation/istif, güvenli büyük-kutu taşıma, yüklü platform arabası ve ilk görsel benchmark tamam |
| 2 | Temel mağaza döngüsü | Devam ediyor; Catalog/Inventory, purchase-order receiving, fiziksel teslimat/raf, offer ve customer basket reservation tamam; checkout snapshot sırada |
| 3 | PC toplama teknik prototipi | Planlandı |
| 4 | Vertical slice entegrasyonu | Planlandı |
| 5 | Çalışanlar ve gelişmiş müşteri AI | Planlandı |
| 6 | Servis, iade, garanti, ikinci el | Planlandı |
| 7 | Dinamik ekonomi ve tedarik | Planlandı |
| 8 | İtibar, büyüme, reklam, rekabet | Planlandı |
| 9 | İçerik, sanat, ses ve kariyer | Planlandı |
| 10 | Alpha, denge, optimizasyon, erişilebilirlik | Planlandı |
| 11 | Demo/Steam Playtest | Planlandı |
| 12 | Beta ve Windows/Steam 1.0 | Planlandı |
| 13 | Ayrı macOS portu ve QA | Windows 1.0 + bütçe sonrası |

Ayrıntılı bağımlılık, zorluk, risk ve kabul ölçütleri: [`Docs/ProjectBible/05_GELISTIRME_YOL_HARITASI.md`](Docs/ProjectBible/05_GELISTIRME_YOL_HARITASI.md).

## 15. Bugüne kadar tamamlanan teknik işler

| Checkpoint | Kanıt |
|---|---|
| Legacy keşif | Electron + düz JS/HTML/CSS; 14 Dashboard alanı haritalandı |
| Canonical legacy | USB ile yerel ayna 26/26 yol/boyut/SHA-256 eşleşti |
| Unity Stage A | Unity 6000.3.21f1 URP, paket kilidi, macOS Universal smoke ve Windows x64 Mono cross-build |
| VCS | Private [`cixanla/PC-Shop-Empire-3D`](https://github.com/cixanla/PC-Shop-Empire-3D), `main` ve Stage A etiketi; UVCS ikinci authoritative sistem değil |
| İş birliği/devir | Yaşayan Bible, governance, issue/PR şablonları, repo guard, 22 epic ve [private Project](https://github.com/users/cixanla/projects/2) |
| Legacy repository referansı | 26 canonical dosya private repoda byte-exact snapshot + SHA-256 manifest olarak korunuyor |
| Core assembly | `PSE.Core` `noEngineReferences`; Unity/Editor bağımlılık testi |
| Kimlik/sonuç | `StableId<TScope>`, `Failure.Code`, `OperationResult` |
| Zaman/olay | Integer açık-adımlı `SimulationClock`, pause güvenliği, event ID/type/sequence/schema zarfı |
| Rastgelelik | Sürümlü PCG32, 63-bit benzersiz stream selector, snapshot/restore, official golden vector ve bias'sız bounded integer |
| Bağlamsal stream | Canonical root seed, SHA-256 framed domain/context derivation, iki golden vector ve reload-reroll engeli |
| Event dispatch | Correlation/causation, global FIFO, breadth-first nested enqueue, duplicate/conflict, bounded drain ve handler hata izolasyonu |
| Catalog çekirdeği | Unity bağımsız `PSE.Catalog`; stable ürün/kategori kimliği, serialized/batch tracking policy, doğrulanmış görünür ad, bounded garanti ve immutable sıralı katalog |
| Inventory authority | Unity bağımsız `PSE.Inventory`; serialized item, bölünebilir batch position, unit-capacity container, atomik transfer, claim reservation, consume/release, revision ve invariant audit |
| Purchase order receiving | Unity bağımsız `PSE.Orders`; stable order/supplier/delivery, monotonik lifecycle, exact manifest ve tek-revision Inventory intake |
| Authoritative dünya/stok projection'ı | Görünür teslimat kabulü; aynı serialized item için Receiving→ActorHands→Shelf/WorldFloor domain-first transfer, rollback ve recovery |
| Fiziksel teslimat kolisi açma | Kapalı dış parcel → idempotent exact ürün reveal; opening domain revision/quantity değiştirmez, açık kabuk Receiving'de kalır |
| Authoritative RAF A teklifi | Unity bağımsız `PSE.Retail`; stable offer/product/shelf kimliği, 3 harf currency, pozitif integer minor-unit fiyat, idempotent publish/update revision ve failure no-mutation |
| Customer basket rezervasyonu | Stable customer/basket/line, exact offer + serialized item + Inventory claim; duplicate engeli, idempotent reserve, release, cross-authority no-mutation ve reserved pickup kilidi |
| Oynanabilir garaj | `PSE.World`/`PSE.Presentation`, GarageGraybox, connected PlayerRig, görünür prototip eller, klavye/fare + gamepad hareket/kamera, sprint, pause ve rebind store |
| Fiziksel pickup/drop | Stable ürün kimliği, range+LOS hedefleme, tek slot, fizik snapshot/restore, dinamik prompt, güvenli drop ve recovery |
| Kontrollü küçük kutu placement | İşaretli stock surface, 0,25 m grid/90° yaw snap, tam destek/overlap doğrulaması, yeşil-kırmızı ghost + metin, stabil kinematic placement |
| Büyük kutu taşıma profili | Ayrı boyut/kimlik, iki-el pozu, 0,65× hareket, sprint kilidi, motion-safe bounded FOV, fail-closed drop ve recovery |
| Kontrollü küçük kutu rotation | `R / Right Shoulder` ile deterministik 90° adım, etkin binding/açı promptu, döndürülmüş footprint doğrulaması ve ghost/confirm poz eşitliği |
| Kontrollü küçük kutu istifleme | Stable küçük kutu desteği, merkez/90° snap, beş noktalı tam destek, overlap engeli, tek kat ilişkisi, dolu taban kilidi ve gerçek keyboard/gamepad akışı |
| Yüklü taşıma arabası | Tek `LargeBox` kapasitesi, hands→cart→hands stable ownership, dört noktalı destek + swept obstruction, 0,85× yüklü hız, sprint kilidi, dinamik prompt ve fail-closed recovery |
| Görsel yön sözleşmesi | Gerçek oran, PBR yüzey, zemine oturan ışık ve doğal ağırlık taşıyan okunaklı yarı gerçekçilik; ilk uygulama tek benchmark köşesiyle sınırlı |
| Garaj görsel benchmarkı | Bevel'lı tezgâh/raf, prosedürel PBR yüzeyler, görev ışığı, ACES/bloom/reflection probe; gameplay collider ve kimlik sözleşmeleri korunuyor |
| Güncel USB milestone | `2026-08-15_STAGE_B_CUSTOMER_BASKET_RESERVATION`; source `109237a`, 498 tracked kaynak + 4 test/build/runtime kanıtı + source kaydı, 503 satırlı `ff868e4c…20d7` SHA-256 manifest/readback ve source checksum ile doğrulandı; cache/build/credential/AppleDouble dışarıda |
| Son test/build | Customer basket reservation sonrası Edit Mode `220/220`, Play Mode `17/17`; Universal macOS build ve Apple M4/Metal 1280×720 `shelf-offer=ok basket-reservation=ok release=ok stable=ok quantity=1` gerçek player smoke geçti |

Önceki zaman/olay Core commit'i `8af2ad3d05906839c4b607e4958650e723060465`, iş birliği/devir checkpoint'i `2ee421193833111f76c85dabb33910240c36db03` olarak korunur. Güncel PRNG feature ve checkpoint commitleri `Docs/ProjectBible/10_DEVAM_CHECKPOINT.md` içinde kayıtlıdır.

## 16. Sıradaki uygulama sırası

1. [Issue #8](https://github.com/cixanla/PC-Shop-Empire-3D/issues/8) altında basket line + exact offer + Inventory reservation'ı doğrulayıp fiyatı satış başlangıcında donduran Unity-bağımsız checkout transaction snapshot'ını kur.
2. Reservation consume/sale commit sınırını ayrı bounded pakette kur; ödeme, ledger ve Save sınırını açık tut.
3. Benchmark görsel dilini yalnız tamamlanan gameplay alanlarına kademeli yay; bütün sahneyi henüz final art sayma.
4. İlk gerçek Windows x64 test cihazı erişim tarihini Faz 1 kapanmadan sabitle.

Her adım ayrı issue, test, commit ve checkpoint olarak kapanır. Büyük asset, ücretli araç, Steam/Apple ödemesi veya gerçek Windows IL2CPP kurulumu ayrı maliyet/izin kapısıdır.

## 17. Açık büyük kararlar

- Nihai ticari oyun adı ve marka araştırması.
- Gerçek Windows x64 geliştirme/test cihazı ve erişim takvimi.
- Büyük binary asset öncesi Git LFS politikası.
- Steamworks onboarding ve mağaza sayfası zamanlaması.
- Online crash/telemetry sağlayıcısı kullanılıp kullanılmayacağı; gizlilik/opt-in sınırı.
- Windows 1.0 sonrası macOS bütçesi, imzalama ve Apple Developer planı.

## 18. Başlıca riskler

- Tek kişi için kapsamın sürdürülemez büyümesi.
- Ellerde/fizikte titreme ve hassas montajın yorucu olması.
- Müşteri/çalışan AI'nin performans ve edge-case yükü.
- Save migration ve stok/para invariant hataları.
- Gerçek Windows doğrulamasının geç kalması.
- Kurgusal ürün içeriğinin teknik doğruluk ve üretim yükü.
- Üçüncü taraf asset/lisans/provenans kaybı.
- Public paylaşımda marka, kişisel veri veya proprietary kaynak sızıntısı.

Riskler [`Docs/ProjectBible/06_PROJE_HAFIZASI.md`](Docs/ProjectBible/06_PROJE_HAFIZASI.md) içinde ID'lerle izlenir.

## 19. Repository gerçeği

Authoritative remote private [`cixanla/PC-Shop-Empire-3D`](https://github.com/cixanla/PC-Shop-Empire-3D), authoritative dal `main`, yerel çalışma kökü ise bu Unity Git deposudur. Codex'te yanlışlıkla oluşturulan ayrı `Game` proje kaydı 13 Ağustos 2026'da kaldırıldı; kaynak klasörü, `.git` ve GitHub remote'u değişmedi. Günlük konuşma mevcut ana `PC Shop Empire Similator` projesinde sürer. Kaynak türleri:

- **Canlı:** Unity kaynakları, root `PROJECT_BIBLE.md`, `Docs/`, `SourceAssets/`, `Tools/`.
- **Salt okunur geçmiş:** `LegacyReference/PC-Shop-Empire-1.1.6/Source/`; manifest değişmeden korunur.
- **Yeniden üretilebilir ve Git dışı:** `Library`, `Temp`, `Logs`, `UserSettings`, IDE dosyaları, build çıktıları.
- **Asla Git'e girmez:** token, credential, certificate/private key, kullanıcı telemetry ham verisi.

GitHub Issues iş birimi, [PC Shop Empire 3D — Development Roadmap](https://github.com/users/cixanla/projects/2) ise görünür durum panosudur. Tasarım gerçeği issue yorumlarında kaybolmaz; kalıcı karar root Bible, ilgili ayrıntılı belge veya ADR'ye işlenir. Eski public `cixanla/PC-Shop-Empire` repository'si yalnız legacy release/indirme geçmişidir ve bu migration sırasında değiştirilmemiştir.

## 20. Yeni geliştirici için 15 dakikalık devir

1. Bu belgeyi ve [`Docs/DEVELOPER-HANDOFF.md`](Docs/DEVELOPER-HANDOFF.md) dosyasını oku.
2. Private depoyu clone et; `main` üzerinde doğrudan deneme yapma.
3. Unity Hub ile tam `ProjectSettings/ProjectVersion.txt` sürümünü kur.
4. `./Tools/verify-repository.sh` çalıştır.
5. Edit Mode 220/220 ve Play Mode 17/17 baseline testlerini doğrula.
6. GitHub Project'te atanmış issue'yu ve kabul ölçütünü oku.
7. Küçük branch aç; gameplay ile mimari migration'ı aynı PR'a yığma.
8. Test, `PROJECT_BIBLE`, ilgili ADR/provenans ve changelog kontrolünü tamamla.

Tam komutlar ve platform notları handoff belgesindedir.

## 21. Her değişiklikte zorunlu yaşayan kayıt

Her push/PR şu sorulara cevap vermelidir:

- Ne değişti ve neden?
- Hangi issue/karar ve kabul ölçütüne bağlı?
- Hangi sistem, ekonomi, AI, save, performans veya içerik yükünü etkiliyor?
- Hangi test/manuel doğrulama geçti?
- Yeni asset/paket/veri varsa kaynağı ve lisansı nedir?
- Şimdi tamamlanan nedir, sıradaki tek iş nedir?

Material değişiklikte güncellenecek yerler:

1. Bu dosyanın **güncel durum**, **tamamlananlar**, **sıradaki sıra** veya **risk** bölümü.
2. Ayrıntı için ilgili `Docs/ProjectBible` belgesi.
3. Kalıcı teknik karar için yeni/tarihsel ADR.
4. Kullanıcıya görünen/depo yapısını etkileyen değişiklik için `CHANGELOG.md`.
5. Asset/paket için `Docs/PROVENANCE.md`.

Pull request şablonu bu kontrolü zorunlu hatırlatır. Kapsam değişmediyse “Project Bible değişikliği gerekmiyor” gerekçesi açıkça yazılır.

## 22. Ayrıntılı belge haritası

| Belge | İçerik |
|---|---|
| [`00_OKU_BENI`](Docs/ProjectBible/00_OKU_BENI.md) | Ana dizin ve güncel sonuç |
| [`01_GAME_DESIGN_BIBLE`](Docs/ProjectBible/01_GAME_DESIGN_BIBLE.md) | Bütün oyun sistemleri ve deneyim |
| [`02_DONUSUM_MATRISI`](Docs/ProjectBible/02_MEVCUT_PROJE_VE_DONUSUM_MATRISI.md) | Legacy Dashboard ve korunacak/dönüşecek/çıkarılacaklar |
| [`03_RAKIP_ARASTIRMASI`](Docs/ProjectBible/03_RAKIP_ARASTIRMASI_VE_FARKLILASMA.md) | Rakip güçlü/zayıf yanları ve özgün fark |
| [`04_TEKNIK_MIMARI`](Docs/ProjectBible/04_TEKNIK_MIMARI_ARACLAR_VE_GUARDIAN.md) | Modüller, save, Guardian, araç/lisans |
| [`05_YOL_HARITASI`](Docs/ProjectBible/05_GELISTIRME_YOL_HARITASI.md) | Fazlar, bağımlılıklar, risk ve doğrulama |
| [`06_PROJE_HAFIZASI`](Docs/ProjectBible/06_PROJE_HAFIZASI.md) | Karar ID'leri, varsayımlar, riskler, açık kapılar |
| [`07_KAYNAKLAR`](Docs/ProjectBible/07_KAYNAKLAR.md) | Araştırma kaynak defteri |
| [`08_KURULUM_PLANI`](Docs/ProjectBible/08_CANONICAL_KAYNAK_VE_KURULUM_PLANI.md) | Canonical kaynak, araç sürümü ve geri alma |
| [`09_STAGE_A_RAPORU`](Docs/ProjectBible/09_STAGE_A_KURULUM_RAPORU.md) | Kurulum/build/test kanıtı |
| [`10_CHECKPOINT`](Docs/ProjectBible/10_DEVAM_CHECKPOINT.md) | Son sağlam devam noktası ve kullanım protokolü |
| [`11_BIRLESIK_CODEX_HAFIZASI`](Docs/ProjectBible/11_BIRLESIK_CODEX_PROJE_HAFIZASI.md) | Üç Codex görevinin ortak bağlamı, üretim geçmişi ve tek-kanal devam protokolü |
| [`CODEX_HISTORY`](Docs/CodexHistory/README.md) | Tam kullanıcı/Codex konuşmaları, dosya değişiklik envanteri ve Git dosya geçmişi |
| [`GITHUB_HANDOFF`](Docs/Evidence/GITHUB-HANDOFF-2026-08-11.md) | Private remote, Project, Codex, fresh clone ve USB devir özeti |

## 23. Telif ve özgünlük ilkesi

Rakip araştırması yalnız tasarım ilkesi ve oyuncu sorunlarını anlamak içindir. Başka oyunun kodu, adı, UI'ı, görseli, sesi, logosu, metni veya özgün içeriği kopyalanmaz. Ürün markaları kurgusaldır; gerçek teknik ilişkiler özgün veri modeliyle uygulanır. Her dış katkı/asset için lisans ve katkı hakkı yazılı kayda bağlanır.

Bu Bible, projenin yaşayan ana haritasıdır; kod gerçeğinin yerine geçmez ama kodun nedenini ve sonraki yönünü kaybetmemeyi sağlar.
