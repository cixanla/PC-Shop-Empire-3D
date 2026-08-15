# ADR-0029 — Bounded Single-Customer Consultation and Recommendation Gate

**Tarih:** 15 Ağustos 2026<br>
**Durum:** Kabul edildi ve tamamlandı; Issue #51 kapalı, Roadmap `Done`<br>
**Bağlam:** Epic #9 — `Browsing` müşterinin ihtiyacını gerçek oyuncu görüşmesiyle kaydetme ve mevcut tek-offer `Buy/Leave` kararını canonical consultation receipt ile kapılama

## Bağlam

Issue #46–#50 zinciri deterministic müşteri ziyareti, açıklanabilir tek-offer kararı, stale-safe `Buy/Leave`, checkout navigation ve exact-cash Economy settlement'ını kurdu. Ancak müşteri `Browsing` durumuna gelir gelmez karar otomatik görünüyordu; oyuncunun müşteriyi gerçekten dinlediğini kanıtlayan authoritative bir kayıt yoktu. Presentation odağını veya ekrandaki konuşma metnini karar yetkisi saymak; görüşmeyi atlama, stale snapshot kullanma, aynı `E` basışının iki etkileşimce tüketilmesi ve görünmeyen stoktan öneri üretilmiş izlenimi yaratabilirdi.

## Karar

- `PSE.Actors` içindeki `CustomerConsultationAuthority`, belirli bir `CustomerVisitAuthority` instance'ına bir kez canonical olarak bağlanır. Aynı visit authority için ikinci consultation authority `actors.customer-consultation.authority-already-attached` ile fail-closed olur.
- Immutable `CustomerConsultationRecord`; stable consultation, customer, visit ve intent kimliklerini; need, requested product, exact `Browsing` state'i, source visit `LastUpdatedAt` değeri ve integer simulation `RecordedAt` zamanını taşır. Sonraki visit veya commerce geçişleri bu historical provenance'ı yeniden yazmaz.
- Yeni kayıt yalnız authority'nin exact current visit record referansı `Browsing` iken ve komut zamanı hem visit snapshotına hem visit authority'nin observed-time watermark'ına göre monotonikken oluşur. Visit başına tek receipt vardır; exact replay success/no-mutation, aynı ID ile farklı payload identity conflict, ikinci ID ile aynı visit `visit-already-consulted` üretir.
- `CustomerOfferDecisionEvaluator` sırası sabittir: structural input → consultation varlığı → `Browsing` state → customer/visit/intent/need/product/state eşliği → exact visit timestamp freshness → canonical owner/source doğrulaması → supported need → currency ve tek-offer karşılaştırması. Eksik receipt `consultation-required`, foreign/mismatched receipt `consultation-mismatch`, eski visit snapshotı `consultation-stale` üretir.
- `CustomerOfferDecision` exact consultation receipt'i immutable provenance ve equality/hash kapsamına alır. `CustomerOfferDecisionActionAuthority`, aynı visit authority'ye bağlı canonical consultation authority'yi zorunlu alır; `Buy/Leave` öncesinde receipt sahipliğini, receipt zamanını ve current offer/visit kararını yeniden doğrular. Value-equal kopya veya başka authority receipt'i yetki değildir.
- Garage görüşmesi yalnız current `Browsing`, görünür ve henüz görüşülmemiş müşteri için; pause kapalıyken, en çok `2,75 m` mesafede, `24°` focus sınırında ve kameradan müşterinin `1,35 m` odak noktasına unobstructed raycast LOS varken açılır. Ray'in müşteriye veya child collider'ına çarpması gerekir; trigger'lar yok sayılır.
- Gerçek görüşme girdisi `E / Gamepad South`tur ve prompt etkin binding'i dinamik gösterir. Başarıdan sonra renge bağlı olmayan kısa Türkçe ihtiyaç cevabı görünür: `EKRAN KARTIMI YÜKSELTMEK İSTİYORUM`.
- Shared Interact action versioned `TryConsumeInteractPressThisFrame` ile tek tüketicilidir. `GarageCustomerFlowRuntime`, `[DefaultExecutionOrder(100)]` ile motorun default `Update`ından sonra görüşme odağını değerlendirip basışı tüketir; `PlayerCarryController` bunu daha sonra `LateUpdate`ta yeniden kullanamaz. Görüşme uygun değilse basış carry etkileşimine açık kalır.
- Play mode'da `PlayerInputAdapter` source `InputActionAsset`i doğrudan etkinleştirmez veya sahiplenmez; runtime clone üretir. Reconfigure eski owned clone'u yok eder, callback aboneliğini yeni clone'a taşır ve source assetleri korur; disable/enable yalnız runtime map'i etkiler.
- Bu pakette “recommendation”, yalnız matching receipt sonrası mevcut immutable RAF A `ShelfOfferRecord` için açılan açıklanabilir `Buy/Leave` sonucudur. Evaluator Inventory authority almaz, stok enumerate etmez, alternatif ürün veya gizli/backroom inventory aramaz; ranking, utility scoring ve RNG yoktur.
- Başarılı görüşme yalnız consultation authority revisionını bir kez ilerletir. Görüşme ve salt decision okuması Actors visit, Inventory, Orders, ShelfOffer, Basket, Checkout ve Economy state/revisionlarını değiştirmez. Sonraki `Buy/Leave` action'ları yalnız önceki ADR'lerdeki mevcut mutation sınırlarını korur.
- Pause, patience timeout, iki denemeli route fallback ve güvenli exit görüşme beklenirken çalışmaya devam eder; Presentation domain intent, receipt veya recommendation uyduramaz.

## Sonuçlar

- Oyuncu müşteriyi range+LOS içinde gerçekten dinlemeden tek-offer kararı ve downstream `Buy/Leave` yetkisi oluşmaz.
- Exact replay güvenlidir; stale, foreign, forged/value-equal veya yanlış-zaman receipt bütün authority'lerde no-mutation kapanır.
- Aynı `E / Gamepad South` basışı görüşme ile pickup/cart etkileşimini aynı frame'de birlikte tetiklemez; runtime input clone yaşam döngüsü source assetleri bozmaz.
- Existing Buy, Leave, reservation, checkout, exact-cash settlement, route fallback ve fiziksel stok akışları matching receipt sonrasında korunur.
- Graybox müşteri gövdesi, konuşma metni ve büyük status panoları kabul kanıtıdır; final diyalog, karakter, animasyon veya UI değildir.

## Bilinçli kapsam dışı

- Branching/free-form/generative diyalog, voice-over, çoklu soru ve final yüz/vücut animasyonu.
- Çoklu customer/offer/product, queue, hidden profile discovery, alternatif veya görünmeyen Inventory önerisi, ranking, utility scoring, RNG ve negotiation.
- Satisfaction, reputation, loyalty ve ilişki geçmişi.
- Save/journal/migration/recovery, Guardian ve gerçek Windows doğrulaması.
- Epic #9 altındaki sıradaki bounded paket olan fiziksel checkout station ve yalnız matching customer `AwaitingCheckout` iken etkin cash payment henüz uygulanmadı.

## Kanıt

- Issue: [#51](https://github.com/cixanla/PC-Shop-Empire-3D/issues/51)
- Feature commit: `846eb5d9912150a6ef3aae9a37678d71348f92a3`
- Tree: `9052d219f013fe007dd2bf16d4fc06726b2914eb`
- EditMode: `347/347`; failed/skipped `0`; `editmode-issue51-r7.xml`
- PlayMode: `23/23`; failed/skipped `0`; `playmode-issue51-r6.xml`
- Universal macOS build: `327837998` bayt; `build-macos-issue51-r4.log`; Mach-O `x86_64 + arm64`
- Scene SHA-256: `353424cd5d4a1e48d4b632f21e7343eb211762e4d1468b1e5bf9e45ebc8cbbaf`
- Runtime: Apple M4/Metal, `1280×720`; stock ve customer smoke r4 başarılı; marker `garage-customer-consultation-r20-v1`
- Feature Repository Guard: [31888147505](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31888147505), başarılı
- Source/docs commit: `f9bc38d8861f575909e36a331ab1cc6476a237a5`; tree `cb087b2a36a5030485c5835ababfcb8f6555ac98`; [Repository Guard 31888842125](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31888842125), başarılı
- Issue #51 acceptance `16/16`; Issue kapalı, Roadmap `Done`, parent Epic #9 açık/`In Progress`
- USB milestone: `2026-08-15_STAGE_B_BOUNDED_SINGLE_CUSTOMER_CONSULTATION_AND_RECOMMENDATION_GATE`; manifest SHA-256 `f8d3ce98e7daa5a014d3d4c79b9a247ac5e15f737914746bd130c191289ccf20`; 578/578 readback, 572/572 Git-blob ve 5/5 evidence eşliği
- Ayrıntı: `Docs/Evidence/BOUNDED-SINGLE-CUSTOMER-CONSULTATION-AND-RECOMMENDATION-GATE-CHECKPOINT-2026-08-15.md`
