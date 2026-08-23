# PC Shop Empire 3D — Yaşayan Proje Bible ve Ana Handoff

**Belge rolü:** Projenin ana fikrini, güncel durumunu, teknik sınırlarını, yapılmış ve yapılacak işleri tek giriş noktasında tutar.  
**Son kapsam güncellemesi:** 23 Ağustos 2026
**Authoritative ayrıntılar:** [`Docs/ProjectBible/`](Docs/ProjectBible/) ve tarihli ADR'ler.  
**Güncelleme kuralı:** Her GitHub checkpoint/pull request, etkilediği durum ve sıradaki işi bu belgede güncellemek zorundadır.

## Güncel geliştirme checkpoint'i — Issue #62 teknik dilimi tamamlandı / USB kapanışı bekliyor

Epic #10'un tek EPS12V/CPU güç kablosu dilimi feature `15d83aeba0d71238a31bf7b7db5fab3dbd9b5951` ile private `main`e ulaştı. Tek canonical serialized kablo; iki typed/keyed 8-pin endpoint, üç stable ordered waypoint, capacity-one `CpuPowerCableRoute` ve retained PSU + secured motherboard + retained CPU lineage'ını atomik dokuz-container Inventory claim'i içinde kullanır. GarageGraybox r31; aynı Unity instance ve stable ItemId ile pickup→keyed preview→authored route→exact unroute→recovery akışını, ATX24'ten izole authority'yi ve dependent-detach/generic-bypass fail-closed kapılarını görünür biçimde taşır. EditMode `610/610`, gerçek Input System PlayMode `51/51`, Universal macOS `329206153` bayt ve aktif Apple M1/Metal 1280×720 exact EPS12V smoke geçti; feature Guard [32642211422](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32642211422), source/docs `cff75f8876f893888ca3a98fe5f149dab0f74a1b` Guard [32642638437](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32642638437) başarılıdır. Yerel staging iki tam 832/832 payload, 826/826 exact Git source ve 5/5 evidence readback'ini `afa89feb…6a73` manifestiyle geçti. [Issue #62](https://github.com/cixanla/PC-Shop-Empire-3D/issues/62) acceptance `20/21` durumundadır ve fiziksel USB milestone/readback henüz olmadığı için `OPEN/In Progress` kalır; electrical power-on, completed benchmark ve diğer kablo aileleri kapsam dışıdır.

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
| 2 | Temel mağaza döngüsü | Tamamlandı; Catalog/Inventory, purchase-order receiving, fiziksel teslimat/raf, offer, basket, deterministic müşteri ziyareti + runtime NavMesh, consultation-gated stale-safe `Buy/Leave` ve `AwaitingCheckout`-gated fiziksel kasa üzerinden exact-cash `PSE.Economy` settlement kaynak/test/build/runtime/CI/USB kapılarıyla kapandı; Epic #9 Done |
| 3 | PC toplama teknik prototipi | Devam ediyor; authoritative tek-anakart seating, captive fastener, CPU socket/retention ve single-DIMM dual-latch kapıları kaynak/test/build/runtime/CI/USB düzeyinde tamamlandı |
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
| Purchase order receiving | Unity bağımsız `PSE.Orders`; stable order/supplier/delivery, monotonik lifecycle, exact manifest, immutable unit-cost provenance ve tek-revision Inventory intake |
| Authoritative dünya/stok projection'ı | Görünür teslimat kabulü; aynı serialized item için Receiving→ActorHands→Shelf/WorldFloor domain-first transfer, rollback ve recovery |
| Fiziksel teslimat kolisi açma | Kapalı dış parcel → idempotent exact ürün reveal; opening domain revision/quantity değiştirmez, açık kabuk Receiving'de kalır |
| Authoritative RAF A teklifi | Unity bağımsız `PSE.Retail`; stable offer/product/shelf kimliği, 3 harf currency, pozitif integer minor-unit fiyat, idempotent publish/update revision ve failure no-mutation |
| Customer basket rezervasyonu | Stable customer/basket/line, exact offer + serialized item + Inventory claim; duplicate engeli, idempotent reserve, release, cross-authority no-mutation ve reserved pickup kilidi |
| Immutable checkout başlangıcı | Stable checkout/basket/customer ve deterministic line snapshot; exact offer/item/reservation preflight, integer minor-unit currency/total, idempotent begin, fiyat güncellemesine karşı immutable kayıt ve aktif checkout release/pickup kilidi |
| Atomik checkout fulfillment | Owner/revision-bound Inventory/Basket/Checkout prepared planı; side-effect-free tam preflight, tek Inventory/Basket/Checkout revision, public completion bypass'ı kapalı, exact repeat idempotency ve drift no-mutation |
| Atomik nakit ve ilk Economy settlement | Downstream Unity bağımsız `PSE.Economy`; immutable checkout fiyatı + alış maliyeti, exact cash, stable receipt, dengeli Cash/SalesRevenue/COGS/InventoryAsset postingleri, replay/conflict/no-mutation ve receipt-gated müşteri çıkışı |
| Deterministic müşteri ziyareti | Unity bağımsız `PSE.Actors`; stable customer/intent/visit, monotonik state + bounded receipt ledger, iki denemeli route fallback, patience/exit timeout ve Inventory/Retail/Orders izolasyonu |
| Bounded tek-müşteri danışmanlık/öneri kapısı | Unity bağımsız `CustomerConsultationAuthority`; current canonical `Browsing` ziyareti için tek immutable customer/visit/intent/need/product/timestamp provenance'ı, exact replay idempotency ve foreign/stale/non-browsing/conflict yollarında no-mutation |
| Runtime NavMesh müşteri projection'ı | Offer sonrası giriş→RAF A ve görünür yardım bekleme; odaklı `E / Gamepad South` danışmanlığı sonrası karar, Buy reservation sonrası checkout, Economy receipt sonrası çıkış; pause güvenli simulation clock, görünür durum/neden ve güvenli terminal gizleme |
| Fiziksel checkout station | Stable `world.checkout-station.garage-001`; `2,75 m` range, `24°` focus, LOS ve pause gate'i; RAF A ödeme bypass'ı kapalı; ilk `Mouse Left / Gamepad RT` immutable checkout, release/repress sonrası ikinci edge exact-cash settlement; canonical receipt-gated stock/customer completion |
| İlk authoritative PC assembly dilimi | Unity bağımsız `PSE.Assembly`; mevcut Catalog/Inventory ile tek serialized `MicroAtx` anakartın ActorHands↔managed Workbench transferi, immutable attach/detach receipt'i, stable identity/replay ve GarageGraybox'ta range/focus/LOS/support/obstruction gated `SeatedUnsecured` fiziksel slot akışı |
| Deterministic motherboard fastener | Assembly-owned stable fastener ID, exact secure/unsecure receipt ve historical replay; secured detach kilidi, Inventory-isolated revision, NonAlloc range/focus/LOS/pause/obstruction solver, gerçek keyboard/gamepad input ownership'i ve görünür screw/screwdriver/status-plate projection'ı |
| Deterministic CPU socket ve retention | Tek canonical serialized CPU için capacity-1 managed socket; `EmptyOpen → ProcessorSeatedOpen → ProcessorRetained` reversible authority, keyed 90° orientation, secured-host close gate'i, exact four-operation replay/lineage, aynı fiziksel instance recovery'si, gerçek keyboard/gamepad input ve r24 yarı-gerçekçi LGA package/load-plate/lever projection'ı |
| Deterministic single DIMM ve dual-latch retention | Tek canonical serialized DDR5 UDIMM, immutable A2/Channel A/Bank 2 topology, atomik managed triple claim; `EmptyOpen → MemoryModuleSeatedOpen → MemoryModuleRetained`, yalnız 0°↔180° keyed input, sol→sağ close/sağ→sol open, exact four-operation replay/lineage, installed-DIMM host gate, same-instance recovery, gerçek keyboard/gamepad ve r25 dört materyalli DIMM/slot/dual-latch projection'ı |
| Deterministic M.2 NVMe ve captive-screw retention | Tek canonical serialized M.2 2280 NVMe, atomik dört-container claim, immutable M-key/2280/captive-screw topology; 18° guided insertion, reversible seat/secure/unsecure/remove, exact replay, installed-storage host gate, same-instance recovery, gerçek keyboard/gamepad ve r26 SSD/slot projection'ı |
| Deterministic processor cooler ve four-point retention | Tek canonical serialized LGA1700 top-down air cooler, atomik beş-container claim, immutable slot/bracket/dört-point topology; iki keyed orientation, pre-applied TIM'in tek tüketimi, `1→3→2→4` retain/ters release, host gates, same-instance recovery, gerçek keyboard/gamepad ve r27 cold-plate/TIM/fin/fan projection'ı |
| Deterministic PCIe x16 ekran kartı ve rear-bracket retention | Canonical Northstar A60 ProductId'sini kullanan ayrı serialized assembly item, atomik altı-container claim, immutable PCIe x16 slot/latch/rear-bracket/fastener topology; keyed 0°/180° orientation, chassis+cooler clearance, reversible seat/retain/unretain/remove, installed-GPU host gate, same-instance recovery, gerçek keyboard/gamepad ve r28 dual-fan/PCB/contact/bracket projection'ı |
| Deterministic ATX PS/2 güç kaynağı ve four-screw retention | Tek canonical serialized PSU item, atomik yedi-container claim, immutable chassis-owned bay/rear-mount/four-fastener topology; iki keyed fan orientation, filtered-floor support, gerçek authored chassis clearance, reversible seat/retain/unretain/remove, alternate-order authority isolation, same-instance recovery, gerçek keyboard/gamepad ve r29 housing/fan-grille/AC/modular-panel/rear-plate projection'ı |
| Deterministic ATX24 split-PSU kablo routing | Tek canonical serialized power cable, typed PSU 18-pin + PSU 10-pin + motherboard 24-pin endpoints, üç ordered waypoint ve capacity-one CableRoute; atomik sekiz-container claim, `Loose ↔ Routed`, exact route/unroute replay, retained-PSU + secured-motherboard host gates, aynı fiziksel instance, gerçek keyboard/gamepad ve r30 connector/latch/authored-route projection'ı |
| Deterministic EPS12V/CPU güç kablosu routing | Tek canonical serialized EPS12V cable, iki typed/keyed 8-pin endpoint, üç ordered waypoint ve capacity-one `CpuPowerCableRoute`; atomik dokuz-container claim, retained PSU + secured motherboard + retained CPU lineage, ATX24 isolation, reversible same-instance route/recovery, gerçek keyboard/gamepad ve r31 connector/latch/braided-route projection'ı |
| Açıklanabilir tek-offer müşteri kararı | Tek yönlü `PSE.Retail → PSE.Actors`; owned current consultation + immutable visit/offer/accepted-price provenance, deterministic `Buy/Leave`, stable reason/failure code, exact replay ve bütün gameplay authority'lerinde no-mutation |
| Stale-safe müşteri Buy eylemi | Explicit Actors↔Retail kimlik bağı, current visit/offer yeniden doğrulaması, exact serialized action-owned reservation, `Browsing → NavigatingToCheckout`, idempotent replay ve stale no-mutation |
| Stale-safe müşteri Leave eylemi | Aynı kind-discriminated action ledger'ında current visit/offer revalidation, internal Actors prepared planı, `Browsing → Exiting`, stable `OfferDeclined`, Browse→Exit NavMesh ve bütün commerce authority'lerinde no-mutation |
| Oynanabilir garaj | `PSE.World`/`PSE.Presentation`, GarageGraybox, connected PlayerRig, görünür prototip eller, klavye/fare + gamepad hareket/kamera, sprint, pause ve rebind store |
| Fiziksel pickup/drop | Stable ürün kimliği, range+LOS hedefleme, tek slot, fizik snapshot/restore, dinamik prompt, güvenli drop ve recovery |
| Kontrollü küçük kutu placement | İşaretli stock surface, 0,25 m grid/90° yaw snap, tam destek/overlap doğrulaması, yeşil-kırmızı ghost + metin, stabil kinematic placement |
| Büyük kutu taşıma profili | Ayrı boyut/kimlik, iki-el pozu, 0,65× hareket, sprint kilidi, motion-safe bounded FOV, fail-closed drop ve recovery |
| Kontrollü küçük kutu rotation | `R / Right Shoulder` ile deterministik 90° adım, etkin binding/açı promptu, döndürülmüş footprint doğrulaması ve ghost/confirm poz eşitliği |
| Kontrollü küçük kutu istifleme | Stable küçük kutu desteği, merkez/90° snap, beş noktalı tam destek, overlap engeli, tek kat ilişkisi, dolu taban kilidi ve gerçek keyboard/gamepad akışı |
| Yüklü taşıma arabası | Tek `LargeBox` kapasitesi, hands→cart→hands stable ownership, dört noktalı destek + swept obstruction, 0,85× yüklü hız, sprint kilidi, dinamik prompt ve fail-closed recovery |
| Görsel yön sözleşmesi | Gerçek oran, PBR yüzey, zemine oturan ışık ve doğal ağırlık taşıyan okunaklı yarı gerçekçilik; ilk uygulama tek benchmark köşesiyle sınırlı |
| Garaj görsel benchmarkı | Bevel'lı tezgâh/raf, prosedürel PBR yüzeyler, görev ışığı, ACES/bloom/reflection probe; gameplay collider ve kimlik sözleşmeleri korunuyor |
| Son tamamlanmış USB milestone (Issue #61) | `2026-08-23_STAGE_B_DETERMINISTIC_SINGLE_ATX24_SPLIT_PSU_CABLE_ROUTING`; source/docs `52795b66`, 801/801 iki tam SHA-256 hash/boyut/yol readback, 796/796 exact Git source, 4/4 evidence, 15.237.662 payload baytı ve `f2145ecb…1365` manifest; güvenlik/AppleDouble farkı `0` |
| Issue #50 kapanış checkpoint'i | Feature `547cf971882239c912d8221f344706afc993a37b`, source/docs `aea6e2bd01642f4f72f1a9ee70f07e3dd0e5072b`, tree `84b14646fd549ce93e390bc33a626a8a7a6335fb`; [Repository Guard 31884807638](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31884807638) başarılı; acceptance `18/18`, Issue kapalı ve Roadmap `Done` |
| Issue #51 kapanış checkpoint'i | Feature `846eb5d9912150a6ef3aae9a37678d71348f92a3`, source/docs `f9bc38d8861f575909e36a331ab1cc6476a237a5`, tree `cb087b2a36a5030485c5835ababfcb8f6555ac98`; [Repository Guard 31888842125](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31888842125) başarılı; acceptance `16/16`, Issue kapalı ve Roadmap `Done` |
| Issue #52 kapanış checkpoint'i | Feature `92a0f7b814ad5e597d8d4ca033f2e533f618f719`, source/docs `d6cd203c5b9837c8eecc63ee3974dd2e76351bdc`, tree `6d73d5ac6d675733c939f181d087da3aef90f496`; [Repository Guard 31892875650](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31892875650) başarılı; acceptance `17/17`, Issue kapalı/Roadmap `Done`; parent Epic #9 kapalı/Done |
| Issue #53 kapanış checkpoint'i | Feature `582a3cf3e81a2905e39148065bd5f6c7e35bbc06`, source/docs `8c6abe45d1f9b6c72def9b686b9c81bf3704d10d`, tree `387bcba701b8a959681e92bf29dc48a4d09f0ab7`; [Repository Guard 31905540378](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31905540378) başarılı; acceptance `18/18`, Issue kapalı/Roadmap `Done`; birleşik USB milestone doğrulandı |
| Issue #54 kapanış checkpoint'i | Feature `b6812394f835d64d5bf8422d8e7996ec433cd0f1`, source/docs `7cec7cc4b6fd80997acd0dc2d6943ef08850f4ad`, tree `214381bd6c9d06a7ab2b2c5ea5e902437dca5914`; [Repository Guard 31909940414](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31909940414) başarılı; acceptance `18/18`, Issue kapalı/Roadmap `Done`; birleşik USB milestone doğrulandı |
| Issue #55 kapanış checkpoint'i | Feature `99cadad414789d3f440e08cc6e42e727c2b7a2ad`, source/docs `d9d0722a1592a83b89938529f72b3170f17e94eb`, final metadata `07364b79ad111aa778493c8936a7709c84b48464`; [Repository Guard 31914774370](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31914774370) ve [31914933915](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31914933915) başarılı; acceptance `20/20`, Issue kapalı/Roadmap `Done`; birleşik USB milestone doğrulandı |
| Issue #56 kapanış checkpoint'i | Feature `7482fc9aabe6a3a27ba41730db12c60e18aac515`, source/docs `01c2b5a49f11b27b52af9e299d4d2e48cef3c962`, USB metadata `17af550856e8bca288ed5c17924bc82586c76c27`; [Repository Guard 31919985055](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31919985055), [31920258176](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31920258176) ve [31920923402](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31920923402) başarılı; ayrı USB milestone 668/668 doğrulandı; acceptance `21/21`, Issue kapalı/Roadmap `Done` |
| Issue #57 kapanış checkpoint'i | Feature `4f14e7bcb946f2f8e713a70d16d0e8f04216dbe1`, source/docs `6e0627ec7a76a70abdba8bb507e6ef6979e34236`; [Repository Guard 31970813717](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31970813717) başarılı; ayrı USB milestone 689/689 doğrulandı; acceptance `21/21`, Issue kapalı/Roadmap `Done` |
| Issue #58 kapanış checkpoint'i | Feature `e2f10a22c37101cb12c5d6530c8f104deb72e99d`, source/docs `2e848e3bdc5795a349e6c857973c7c88fef36cd7`, tree `55d5f0d733530a2e4c1400f4f83c29f37dcafff8`; [Guard 32591206866](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32591206866) ve [32591381804](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32591381804) başarılı; acceptance `19/19`, Issue/Roadmap `Done`; fiziksel USB ertelendi, 717/717 doğrulanmış yerel staging hazır |
| Issue #59 kapanış checkpoint'i | Feature `1b29ad29d6e4d8cc1ce09b0989a038a72297585c`, source/docs `a5bbca473e81455c44d2f95469c8faf2a11046ff`, tree `e7b3d5d4ad13bf08f822570966660ab6c48e6a55`; [feature Guard 32599710154](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32599710154) ve [source/docs Guard 32600012769](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32600012769) başarılı; acceptance `20/20`, Issue `CLOSED/COMPLETED`, Roadmap `Done`; fiziksel USB ertelendi ve Issue #59 için milestone/readback iddiası yok |
| Issue #60 kapanış checkpoint'i | Feature `f998d7d1c400c9328afa226f0727e6591c02d4e2`, authored-clearance fix `b6c3ff8e95a4da75161b51cdbcbab87cb529f076`, source/docs `4939a041635a8864f53f6613a9dc9b4e8972f235`; [Guard 32606958882](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32606958882), [32607437408](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32607437408) ve [32607886160](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32607886160) başarılı; acceptance `20/20`, Issue `CLOSED/COMPLETED`, Roadmap `Done`; 775/775 local staging hazır, fiziksel USB readback iddiası yok |
| Issue #61 kapanış checkpoint'i | Feature `1fc29f13171925c2445eaa7334158e0f058e76a5`; source/docs `52795b66fee1eb933d0d9c4ff8cbd7eca512d718`; USB metadata `f9a5da8b23dedd3719c96d50846d4ba3143cc87f`; Guard `32613813494` + `32614187494` + `32632615041` başarılı; ayrı USB milestone 801/801 ve 796/796 doğrulandı; acceptance `20/20`, Issue `CLOSED/COMPLETED`, Roadmap `Done`; parent #10 açık/In Progress |
| Issue #62 teknik checkpoint'i | Feature `15d83aeba0d71238a31bf7b7db5fab3dbd9b5951`, source/docs `cff75f8876f893888ca3a98fe5f149dab0f74a1b`; Guard `32642211422` + `32642638437` başarılı; iki tam 832/832 yerel staging readback'i geçti, fiziksel USB milestone/readback beklediği için Issue `OPEN/In Progress` |
| Son test/build | Issue #62 sonrası Edit Mode `610/610` (`issue62-r31-editmode-canonical-final.xml`), gerçek Input System Play Mode `51/51` (`issue62-r31-playmode-canonical-final.xml`), failed/skipped/inconclusive `0`; Universal macOS build `329206153` bayt ve aktif Apple M1/Metal makinesinde 1280×720 `garage-eps12v-cpu-power-cable-routing-r31-v1` readiness + exact EPS12V smoke geçti; scene SHA-256 `ab4c8fc87979c357f07679ffdd99735424dafbfa2c5d4a185bf1ff234fb22f3a` |

Önceki zaman/olay Core commit'i `8af2ad3d05906839c4b607e4958650e723060465`, iş birliği/devir checkpoint'i `2ee421193833111f76c85dabb33910240c36db03` ve Issue #50–#61 checkpointleri tarihsel olarak korunur. Issue #62 teknik checkpoint'i `Docs/ProjectBible/10_DEVAM_CHECKPOINT.md`, ADR-0040 ve tarihli evidence belgesinde kayıtlıdır; fiziksel USB kapanışı henüz yoktur.

## 16. Sıradaki uygulama sırası

1. Issue #62 için doğrulanmış yerel staging paketini değiştirmeden koru; fiziksel USB yeniden bağlandığında yalnız doğrulanmış doğru volume üzerinde çakışmayan `.incoming-*` kopyası, atomik milestone adlandırması ve iki tam manifest readback yapıp Issue/Project'i kapat.
2. PCIe/SATA/fan/front-panel cabling, electrical power-on, POST/BIOS/OS ve tamamlanmış benchmarkı bu USB kapanışına gizlice ekleme; sonraki gameplay child'ını Issue #62 kapanmadan başlatma.
3. Genel Inventory revision-max hardening'ini ayrı P1 teknik issue olarak kaydet; kapalı assembly issue'larına geri bağlama. Graybox/debug metinlerini bağlamsal prompt ve fiziksel terminal katmanına kademeli taşı; mevcut sahneyi final art sayma.
4. İlk gerçek Windows x64 test cihazında IL2CPP/DirectX/Steam kapısını ayrı dış-platform acceptance olarak çalıştır.

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
5. Edit Mode 461/461 ve Play Mode 33/33 baseline testlerini doğrula.
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
