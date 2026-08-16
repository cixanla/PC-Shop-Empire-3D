# Changelog

Bu dosya teknik ve proje yönetimi checkpointlerini izler. Ayrıntılı oyun kararları `PROJECT_BIBLE.md`, `Docs/ProjectBible/06_PROJE_HAFIZASI.md` ve ADR'lerde tutulur.

## Unreleased

### Added

- Tek canonical serialized DDR5 UDIMM, immutable A2/Channel A/Bank 2 topology'si, capacity-1 managed MemorySlot ve atomik Workbench+ProcessorSocket+MemorySlot triple claim; `EmptyOpen ↔ MemoryModuleSeatedOpen ↔ MemoryModuleRetained` Assembly authority'si, exact seat/close/open/remove receipt lineage'ı ve delayed replay ile eklendi.
- DIMM yalnız secured motherboard üzerinde `0° ↔ 180°` keyed toggle ile oturur. İki görünür latch sol→sağ kapanıp sağ→sol açılırken tek retention revision/receipt üretir; retained remove, installed-DIMM motherboard detach, duplicate seat, stale/conflict/full-hands ve recovery failure yolları cross-authority no-mutation fail-closed'dur.
- GarageGraybox r25'e dört materyalli yarı-gerçekçi UDIMM package, matching notch, A2 slot bed/rail ve iki ayrı latch pivotu eklendi. Generic placement/stack/cart bypass'ı kapalı; same-instance recovery ve `25 Renderer / 13 Collider / 1 TextMesh` assembly bütçesi testlidir.
- Gerçek keyboard/mouse ve gamepad pickup→guided mode→180° toggle→seat→dual-latch close/open→remove→recovery akışı, compact dynamic HUD ownership'i, mode-kapalı ghost/PhysX sıfır sorgu sözleşmesi ve co-edge/pause drain testleriyle kilitlendi.
- Issue #56 feature checkpoint'i `7482fc9`, tree `291b23c`, [Repository Guard `31919985055`](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31919985055), EditMode 461/461, PlayMode 33/33, 328268700 bayt Universal macOS build ve Apple M4/Metal `garage-dimm-dual-latch-r25-v1 dimm-flow=ok ... keyed-orientation=ok latch-order=ok replay=ok authority-isolated=ok identity=stable recovery=ok` kanıtıyla oluşturuldu.
- Issue #53–#55 birleşik fiziksel assembly USB milestone'u `07364b79` kaynak commitinden 640 tracked source + 12 final kanıt + source kaydıyla oluşturuldu; 653/653 SHA-256 hash/boyut/yol geri okuması, 640/640 exact Git source ve 12/12 evidence eşliği geçti. `0b5f3c61…aaba9e` manifestinde forbidden/cache/credential/AppleDouble/sibling-sidecar mismatch `0`; Issue #53 acceptance `18/18`, kapalı ve Roadmap `Done`dur.
- Tek canonical serialized CPU, capacity-1 managed ProcessorSocket container ve atomik Workbench+Socket pair claim'i; `EmptyOpen ↔ ProcessorSeatedOpen ↔ ProcessorRetained` Assembly authority'si, exact seat/close/open/remove receipt lineage'ı ve delayed replay ile eklendi.
- CPU yalnız secured motherboard üzerinde keyed orientation ile oturur; retained remove, installed-CPU motherboard detach, unsecured-host retention close, wrong orientation, stale/conflict/full-hands ve recovery failure yolları cross-authority no-mutation fail-closed'dur.
- GarageGraybox r24'e 45 × 37,5 mm notched LGA-style CPU, ayrı PCB/IHS PBR materyalleri, hard-surface UV/normaller, matching triangular socket key, simetrik aperture load plate ve görünür retention lever eklendi; `21 Renderer / 11 Collider / 1 TextMesh` bütçesi korunur.
- Gerçek keyboard/mouse ve gamepad pickup→guided mode→rotate→seat→retain→open→remove→recovery akışı, compact dynamic HUD prompt ownership'i, mode-kapalı ghost/PhysX sıfır sorgu sözleşmesi ve co-edge/pause drain testleriyle kilitlendi.
- Issue #55 feature checkpoint'i `99cadad`, tree `fea116a`, [Repository Guard `31914489537`](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31914489537), EditMode 430/430, PlayMode 31/31, 328144884 bayt Universal macOS build ve Apple M4/Metal `garage-cpu-socket-retention-r24-v1 cpu-socket-flow=ok ... keyed-orientation=ok retained-remove-gate=ok replay=ok identity=stable` kanıtıyla oluşturuldu; USB kullanıcı talimatıyla ertelendi.
- Issue #55 source/docs `d9d0722`, final metadata `07364b79` ve başarılı [Repository Guard `31914774370`](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31914774370) + [31914933915](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31914933915) ile private `main`e ulaştı; acceptance `20/20`, Issue kapalı ve Roadmap `Done` oldu. Issue #53–#55 birleşik USB snapshotı doğrulandı.
- Assembly-owned stable motherboard fastener kimliği, `SeatedUnsecured ↔ SeatedSecured` secure/unsecure komutları, immutable receipt/historical replay fold'u, Inventory-isolated revision ve secured direct detach gate'i eklendi.
- GarageGraybox r23'e görünür captive screw, deterministic 4 mm secured depth, cross recess, screwdriver, fiziksel tek-satır status plate ve renk dışı compact keyboard/gamepad promptları eklendi; büyük/yüzen fastener debug metni kullanılmadı.
- NonAlloc range/focus/LOS/pause/obstruction solver, near-hit deterministic tie-break, projection drift invariantı, blocked/pause same-frame edge drain'i ve gerçek Input System release–repress testleri eklendi.
- Issue #54 feature checkpoint'i `b681239`, tree `192f9d8`, EditMode 411/411, PlayMode 29/29, 328057977 bayt Universal macOS build ve Apple M4/Metal `garage-motherboard-fastener-r23-v1 assembly-flow=ok ... secure-delayed-replay=ok ... detach-authority-blocked=ok ... recovery=ok` kanıtıyla oluşturuldu; USB kullanıcı talimatıyla ertelendi.
- Issue #54 source/docs `7cec7cc` ve başarılı [Repository Guard `31909940414`](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31909940414) ile private `main`e ulaştı; acceptance `18/18`, Issue kapalı ve Roadmap `Done` oldu.
- Unity bağımsız `PSE.Assembly`, managed Workbench custody'si, stable build/chassis/slot/operation kimlikleri ve immutable `SeatedUnsecured` attach/detach receipt'leriyle eklendi.
- GarageGraybox'a açık kasa, keyed motherboard tray/slot, standoff/connector işaretleri ve tek canonical hassas anakart projection'ı eklendi; guided range/focus/LOS/orientation/support/obstruction preview'su gerçek commit pozu ile aynıdır.
- Klavye/fare ve gamepad al→guided preview→oturt→sök→recovery akışı, dynamic prompt, same-frame Primary+Drop tek-geçiş kuralı ve failed world-drop retry ile kilitlendi.
- Issue #53 feature checkpoint'i `582a3cf`, tree `fc80b7c`, EditMode 394/394, PlayMode 26/26, 328020817 bayt Universal macOS build ve Apple M4/Metal `garage-motherboard-seating-r22-v1 assembly-flow=ok ... recovery=ok` kanıtıyla oluşturuldu; USB kullanıcı talimatıyla ertelendi.
- Issue #53 source/docs `8c6abe4`, tree `387bcba` ve başarılı [Repository Guard `31905540378`](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31905540378) ile private `main`e ulaştı.
- Private GitHub collaboration/devir yapısı, living Project Bible, governance ve katkı şablonları tamamlandı.
- Full design/research package repository içine taşındı.
- Canonical PC Shop Empire 1.1.6 legacy kaynak snapshot'ı ve manifesti eklendi.
- Private `cixanla/PC-Shop-Empire-3D` remote, repository guard workflow, 22 epic ve Development Roadmap Project oluşturuldu.
- Sürümlü `pcg32-xsh-rr-64-32-v1` deterministik rastgele akışı, raw state snapshot/restore, official golden vector ve modulo-bias üretmeyen bounded integer eklendi.
- Save-safe canonical root seed ve sürümlü SHA-256 framed domain/context stream derivation eklendi; reload-reroll ve çağrı sırası bağımsızlığı golden testlerle kilitlendi.
- Domain event correlation/causation ve deterministik FIFO dispatcher; duplicate/conflict politikası, breadth-first nested enqueue, bounded drain ve handler hata izolasyonuyla eklendi.
- `PSE.World`/`PSE.Presentation`, oynanabilir GarageGraybox, connected PlayerRig, klavye/fare + gamepad hareket/kamera, rebind store ve görünür prototip eller eklendi.
- Stable fiziksel ürün kimliği, menzil/görüş hattı hedefleme, tek taşıma slotu, görünür el pozları, dinamik binding prompt'u ve güvenli pickup/drop eklendi.
- Engelli/zeminsiz drop fail-closed kaldı; player disable ve dünya-altı düşüş aynı nesneyi son güvenli pozuna kurtarıyor.
- Küçük kutu için işaretli stock surface, 0,25 m grid/90° yaw snap, tam destek/overlap doğrulaması, renk + metin ghost ve stabil kinematic placement eklendi.
- Mouse-left/gamepad RT placement modunu açar; `G / Gamepad East` mod açıkken onaylar, kapalıyken önceki güvenli drop'u korur.
- Büyük kutu için ayrı stable kimlik/boyut ve carry profili, turuncu bantlı graybox, geniş iki-el pozu, `0,65×` hareket, sprint kilidi ve motion-safe bounded FOV eklendi.
- Büyük kutu placement moduna girmez; etkin `G / Gamepad East` promptu, gerçek boyutlu fail-closed drop, engelli geri bildirim ve disable recovery aynı item kimliğini korur.
- Küçük kutu placement moduna `R / Right Shoulder` ile deterministik 90° clockwise rotation, etkin binding/açı promptu ve döndürülmüş footprint güvenlik doğrulaması eklendi.
- Dikdörtgen küçük kutu ve üst yön işareti GarageGraybox'ta rotation'ı görünür kılar; ghost ile onaylanan poz aynı solver sonucunu kullanır.
- Okunaklı yarı gerçekçilik görsel yönü kabul edildi: gerçek oran/PBR yüzey/zemine oturan ışık/doğal ağırlık, hafif stilize okunabilirlik ve ölçülü performans bütçesi.
- GarageGraybox tek-köşe benchmarkına bevel'lı tezgâh/raf, prosedürel beton/duvar/metal/karton/ahşap yüzeyler, etiket detayları, görev ışığı, ACES, ölçülü bloom ve reflection probe eklendi.
- Stable küçük kutu üstüne merkez/90° snap, beş noktalı tam footprint, overlap engeli, tek kat/tek üst ilişkisi, dolu taban pickup kilidi ve `İSTİF GEÇERLİ` geri bildirimi eklendi.
- Tek `LargeBox` kapasiteli stable platform arabası; hands→cart→hands ownership geçişi, dört noktalı zemin desteği, swept obstruction kontrolü, yüklü/boş hız profili, sprint kilidi, dinamik binding prompt'u ve fail-closed recovery ile eklendi.
- Klavye/fare ve gamepad ile yükle→sür→bırak→geri al zinciri gerçek Input System testleriyle; yüklü araba aynı stable item kimliği ve physics snapshot'ıyla doğrulandı.
- Unity bağımsız `PSE.Catalog`; immutable ürün tanımı, stable ürün/kategori kimliği, serialized/batch tracking policy, doğrulanmış görünür ad, bounded garanti ve deterministic katalog ile eklendi.
- Unity bağımsız authoritative `PSE.Inventory`; serialized item, batch position, unit-capacity container, atomik transfer, claim reservation, release/consume, revision, deterministic sorgu ve invariant audit ile eklendi.
- Catalog/Inventory assembly bağımlılıkları ve failure no-mutation davranışları saf domain testleriyle kilitlendi.
- Unity bağımsız `PSE.Orders`; stable purchase order/supplier/delivery kimliği, deterministic satırlar, monotonik lifecycle ve exact delivery manifest ile eklendi.
- Mixed serialized+batch `InventoryIntake`, tam preflight sonrası bütün teslimatı tek Inventory revision'ında receiving container'a kabul edecek şekilde eklendi.
- Sipariş/onay/yol/arrival aşamalarında stok yaratmama; manifest/capacity/identity failure'ında order+Inventory no-mutation ve duplicate acceptance engeli testlerle kilitlendi.
- GarageGraybox'a görünür authoritative teslimat alanı, carton, durum panosu ve gerçek RAF A stock surface eklendi; HUD order/container durumunu dinamik gösteriyor.
- Aynı serialized item için `Arrived → Receiving → ActorHands → Shelf/WorldFloor` zinciri açık Presentation adaptörüyle bağlandı; domain-first world mutation, rollback ve recovery ile çift/kayıp stok engellendi.
- `InventoryContainerKind.WorldFloor`, `InventoryItemWorldBinding`, `InventoryPlacementZone` ve deterministik `GarageStockFlowSession` eklendi; binding bulunmayan önceki prototype item davranışı korunuyor.
- Kapalı dış teslimat kolisi, açıldığında görünür exact ürün kutusu ve Receiving'de kalan açık koli kabuğu ayrı world projection durumları olarak eklendi.
- Acceptance → unpack → pickup aynı Interact binding'iyle sıralandı; opening exact manifest/item/container doğrulaması, idempotent transition ve domain revision no-mutation sözleşmesiyle kilitlendi.
- Mutually-exclusive parcel/product görsellerinde pickup yalnız aktif collider setini doğruluyor; kapalı ürün alınamıyor, invalid state/identity/location parcel'ı kapalı bırakıyor.
- Unity bağımsız `PSE.Retail`; stable shelf-offer/product/shelf kimliği, üç harf currency, pozitif bounded integer minor-unit fiyat, idempotent set/update revision, deterministic query ve failure no-mutation sözleşmesiyle eklendi.
- Exact ürün authoritative RAF A'dayken `E / Gamepad South` kasıtlı fiyat publish yapıyor; dünya etiketi yalnız başarıdan sonra `FİYAT YOK` → `549,99 EUR` değişiyor ve Inventory/Orders state'i sabit kalıyor.
- `PSE.Retail` içine stable customer/basket/line ve exact shelf offer + serialized item + Inventory claim bağlayan authoritative basket reservation eklendi; duplicate item/customer conflict, drift ve bütün validation failure yolları cross-authority no-mutation kalıyor.
- Fiyatlanmış RAF A ürününde `G / Gamepad East` demo müşteri rezervasyonunu açıp kapatıyor; etiket/pano `1 ÜRÜN • AYRILDI` durumunu gösteriyor, ayrılmış ürün `E / Gamepad South` pickup'a fail-closed yanıt veriyor ve release available quantity'yi geri getiriyor.
- `PSE.Retail` içine stable checkout transaction ve deterministic immutable line snapshot authority'si eklendi; exact basket/offer/item/reservation preflight, tek currency, overflow-safe integer total, idempotent begin ve failure no-mutation sözleşmesi kilitlendi.
- Reserved RAF A ürününde Mouse Left/Gamepad RT checkout başlatıyor; shelf etiketi/HUD/prompt dondurulmuş `549,99 EUR` fiyatını gösteriyor. Sonraki offer update'i açık işlemi değiştirmiyor; checkout aktif release/pickup fail-closed kalıyor.
- Inventory'ye exact reservation setini tamamen preflight edip serialized ve aggregate batch hedeflerini tek revision'da tüketen atomik bulk API; Retail'e stable immutable checkout completion kaydı ve Basket/Inventory commit sınırı eklendi.
- Aktif checkout'ta ikinci Mouse Left/Gamepad RT fulfillment'ı tamamlıyor; ürün projection'ı raftan kaldırılıyor, stok/sepet/reservation `0`, shelf/HUD `TAMAMLANDI` gösteriyor. Exact tekrar idempotent; identity/time/drift failure cross-authority no-mutation kalıyor.
- Unity bağımsız `PSE.Actors`; stable customer/intent/visit kimlikleri, immutable lifecycle kayıtları, bounded exact command receipt ledger'ı, deterministic query/revision ve invariant audit ile eklendi.
- Müşteri yaşam döngüsü `Entering → Browsing → NavigatingToCheckout → AwaitingCheckout → Exiting → Exited`; state başına iki route denemesi, `RouteUnavailable`, patience/exit timeout ve güvenli terminal fallback ile sınırlandı.
- GarageGraybox'a runtime `NavMeshSurface`, explicit giriş/RAF A/checkout/çıkış anchor'ları, görünür graybox müşteri ve checkout köşesi eklendi. Offer, reservation ve fulfillment mevcut authority sonuçlarından projection transition'ı tetikliyor; NPC transformu stok/checkout authority'sini değiştirmiyor.
- Customer runtime smoke transient state yarışına karşı bounded başlangıç pause'u/diagnostic hız penceresi kullanıyor; normal ve leakdiag player koşuları canlı route, pause, fulfillment, route/timeout fallback, authority isolation ve güvenli despawn sonucunu doğruluyor.
- Tek yönlü `PSE.Retail → PSE.Actors` bağımlılığıyla pure/stateless `CustomerOfferDecisionEvaluator`, immutable/equatable exact provenance, deterministic `Buy/Leave` reasonları ve stable invalid-input failure kodları eklendi.
- Exact replay, historical offer/Browsing snapshot, literal public code, validation precedence ve aynı outcome taşıyan farklı accepted-price provenance testleri eklendi; evaluator hiçbir authority/cache/revision/receipt/action taşımaz.
- Garage müşteri status'u yalnız `Browsing` sırasında renge bağımlı kalmadan `KARAR: SATIN AL / AYRIL` ve stable reason code gösteriyor; gerçek keyboard/gamepad akışında karar okuması Actors/Inventory/Orders/Offer/Basket/Checkout state'ini değiştirmiyor.
- Edit Mode baseline `267/267`, Play Mode baseline `18/18` teste yükseldi; Universal macOS build ve Apple M4/Metal 1280×720 `garage-offer-decision-r16-v1 offer-decision=ok authority-isolated=ok` gerçek player smoke geçti.
- Explicit Actors↔Retail customer binding ve `CustomerOfferDecisionActionAuthority`; immutable `Buy` kararını current visit/offer ile stale-safe yeniden doğrulayıp exact serialized action-owned Basket/Inventory reservation ve `Browsing → NavigatingToCheckout` geçişi üretecek şekilde eklendi.
- Inventory/Basket/Actors prepared planları, `ConsumeOnly` action ownership, public release/consume bypass engelleri, exact/conflicting replay ve historical action receipt invariantları domain testleriyle kilitlendi.
- Garage'da gerçek `G / Gamepad East` current Buy action'ını uygular; stale offer stable `DecisionStale` metniyle Action/Basket/Inventory/Actors/Checkout/Orders mutation üretmeden engellenir.
- Edit Mode baseline `287/287`, Play Mode baseline `19/19` teste yükseldi; Universal macOS build ve Apple M4/Metal 1280×720 `garage-buy-action-r17-v1 buy-action=ok stale-blocked=ok authority-isolated=ok` gerçek player smoke geçti.
- Aynı kind-discriminated offer-action ledger'ına stale-safe `Leave` eklendi; current visit/offer exact revalidation sonrası internal Actors planı `Browsing → Exiting` ve stable `OfferDeclined` üretirken Basket/Inventory/Checkout/Offer/Orders değişmez.
- Exact Leave replay, conflicting/cross-kind replay, aynı visit için ikinci action, historical terminal receipt, owner/revision/watermark-bound plan ve route-fallback reason korunumu domain testleriyle kilitlendi.
- Garage'da gerçek `G / Gamepad East` current Leave'i uygular; `Browse → Exit` NavMesh kontratı, `TEKLİF REDDEDİLDİ` başarı ve `AYRILMA ENGELLİ • <stable-code>` stale geri bildirimi klavye/gamepad testleriyle doğrulandı.
- Edit Mode baseline `298/298`, Play Mode baseline `22/22` teste yükseldi; Universal macOS build ve Apple M4/Metal 1280×720 `garage-leave-action-r18-v1 leave-action=ok stale-leave-blocked=ok authority-isolated=ok` gerçek player smoke geçti.
- Purchase order satırından teslimat manifesti ve Inventory intake üzerinden serialized item/batch kaydına uzanan immutable acquisition unit-cost provenance eklendi; transferler maliyeti koruyor, maliyet uyuşmazlığı iki authority'de de mutasyonsuz engelleniyor.
- Downstream Unity bağımsız `PSE.Economy`, exact-cash ödeme makbuzu ve dengeli `Cash / Sales Revenue / COGS / Inventory Asset` dört-posting işlemiyle eklendi; gelir, satılan mal maliyeti ve brüt marj integer minor-unit sözleşmesiyle izleniyor.
- Inventory/Basket/Checkout için authority-owner ve revision-bound atomik prepared planlar eklendi; stale/foreign planlar mutasyonsuz reddediliyor ve public checkout-fulfillment bypass'ı Economy settlement sınırının arkasına kapatıldı.
- Garage `garage-cash-settlement-r19-v1` akışı ikinci Mouse Left/Gamepad RT ile nakit ödemeyi alıyor; müşteri çıkışı completion yerine settlement receipt'e bağlandı ve replay/conflict yolları stok ile muhasebeyi değiştirmeden sonuçlanıyor.
- Issue #50 feature checkpoint'i `547cf97` ve başarılı [Repository Guard `31884497043`](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31884497043) ile doğrulandı: 328/328 EditMode, 22/22 PlayMode, Universal macOS build ve Apple M4/Metal 1280×720 stock/customer smoke; `cash-payment=ok payment-receipt=ok economy-settlement=ok cash-ledger=ok ledger-balanced=ok authority-isolated=ok`.
- Issue #50 source/docs `aea6e2b`, tree `84b1464` ve başarılı [Repository Guard `31884807638`](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31884807638) ile kapatıldı; acceptance `18/18`, Roadmap `Done`, parent Epic #9 açık/`In Progress` kaldı.
- `PSE.Actors` içine current canonical `Browsing` visit için one-per-visit `CustomerConsultationAuthority` ve exact customer/visit/intent/need/product/time provenance taşıyan immutable receipt eklendi; replay/conflict/foreign/historical/stale/time/invariant yolları no-mutation fail-closed'dur.
- Tek-offer `CustomerOfferDecision` ve stale-safe `Buy/Leave` action artık matching canonical consultation receipt olmadan kilitlidir. Garage'da gerçek `E / Gamepad South`, `2,75 m` range, `24°` focus, LOS, dinamik `İHTİYACI SOR` promptu ve Türkçe ihtiyaç cevabı görüşmeyi görünür kılar.
- Versioned tek-consumer Interact, explicit customer execution order ve runtime-owned `InputActionAsset` clone/reconfigure sözleşmeleri aynı basışın pickup/cart'a sızmasını ve source asset mutationını engeller.
- Issue #51 feature checkpoint'i `846eb5d`, tree `9052d21` ve başarılı [Repository Guard `31888147505`](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31888147505) ile doğrulandı: 347/347 EditMode, 23/23 PlayMode, 327837998 bayt Universal macOS build ve Apple M4/Metal 1280×720 `garage-customer-consultation-r20-v1 consultation=ok decision-gated=ok stale-consultation-blocked=ok authority-isolated=ok` stock/customer smoke.
- Issue #51 source/docs `f9bc38d`, tree `cb087b2` ve başarılı [Repository Guard `31888842125`](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31888842125) ile kapatıldı; 572 tracked source + 5 final evidence + source kaydı, 578 satırlı `f8d3ce98…ccf20` USB manifest/readback, acceptance `16/16`, Roadmap `Done`, parent Epic #9 açık/`In Progress` kaldı.
- Stable `world.checkout-station.garage-001` kimlikli görünür fiziksel checkout station eklendi; RAF A checkout/ödeme bypass'ı kapatıldı ve işlem yalnız exact matching customer visit `AwaitingCheckout` durumundayken `Mouse Left / Gamepad RT` ile etkinleşiyor.
- İlk primary press immutable checkout snapshotını, release/repress sonrasındaki ikinci edge exact-cash `PSE.Economy` settlement'ını tek kez üretir; held/same-frame/replay, pause, range/focus/LOS ve stale/foreign provenance yolları bütün authority'lerde no-mutation fail-closed kalır.
- Canonical payment receipt artık exact settlement/transaction/completion/checkout/customer/payment/currency/amount/COGS, `Buy` action, line ve ledger provenance'ını kapılar; ürün projection'ı ile customer fulfillment yalnız matching receipt sonrasında ilerler.
- Customer focus collider'ı station çevresinde oyuncuyu fiziksel olarak sıkıştırmayan trigger'a çevrildi; consultation raycast'i trigger hedefini korur ve üç ardışık final customer runtime smoke güvenli çıkışı doğruladı.
- Issue #52 feature checkpoint'i `92a0f7b`, tree `4150bd3` ve başarılı [Repository Guard `31892420515`](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31892420515) ile doğrulandı: 352/352 EditMode, 24/24 gerçek Input System PlayMode, 327864494 bayt Universal macOS build ve Apple M4/Metal 1280×720 `garage-physical-checkout-station-r21-v1 checkout-station=ok shelf-bypass-blocked=ok checkout-start=ok cash-payment=ok authority-isolated=ok customer-hidden=ok` smoke.
- Issue #52 source/docs `d6cd203`, tree `6d73d5a` ve başarılı [Repository Guard `31892875650`](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31892875650) ile kapatıldı; 576 tracked source + 7 final evidence + source kaydı, 584 satırlı `7fbb5f0c…31fbd` USB manifest/readback, acceptance `17/17`, Roadmap `Done` ve Issue kapalıdır.
- Parent Epic #9; path fallback, ihtiyaca göre bounded öneri, danışmanlık, patience, stale-safe Buy/Leave, exact-cash Economy settlement ve fiziksel checkout doğruluğu kanıtlarıyla kapatıldı; Roadmap `Done`dur.

### Changed

- PCG32'nin yalnız 63-bit benzersiz stream alanı açık sözleşmeye dönüştürüldü; high-bit selector alias'ı sessizce kabul edilmiyor.
- Yanlışlıkla oluşturulan ayrı Codex `Game` proje kaydı kaldırıldı; Unity kaynak klasörü ve GitHub bağlantısı korunuyor.
- Repository Guard checkout action, Node.js 20 deprecation uyarısını kaldırmak için resmî güncel major `actions/checkout@v7`ye yükseltildi.
- Pickup/drop + kontrollü placement milestone'ı ayrı USB hedefinde 336 tracked dosya ve SHA-256 manifest ile geri okunarak doğrulandı.
- Kontrollü küçük-kutu istifleme milestone'ı final tracked kaynak ve test/build/runtime kanıtlarıyla ayrı USB hedefinde SHA-256 manifest/readback kapısından geçirildi.
- Yüklü taşıma arabası milestone'ı 396 tracked kaynak ve 6 test/build/runtime kanıtıyla ayrı USB hedefinde 403 satırlı SHA-256 manifest/readback ve source checksum kapısından geçirildi.
- Catalog/Inventory milestone'ı 428 tracked kaynak, 4 test kanıtı ve source kaydıyla ayrı USB hedefinde 433 satırlı SHA-256 manifest/readback ve source checksum kapısından geçirildi; AppleDouble `0` olarak doğrulandı.
- Order Receiving milestone'ı 449 tracked kaynak, 4 test kanıtı ve source kaydıyla ayrı USB hedefinde 454 satırlı SHA-256 manifest/readback ve source checksum kapısından geçirildi; AppleDouble `0` olarak doğrulandı.
- Authoritative Stock Flow milestone'ı 467 tracked kaynak, 4 test/build/runtime kanıtı ve source kaydıyla ayrı USB hedefinde 472 satırlı SHA-256 manifest/readback ve source checksum kapısından geçirildi; forbidden/credential/AppleDouble `0` olarak doğrulandı.
- Delivery Parcel Unpacking milestone'ı 471 tracked kaynak, 5 scene/test/build/runtime kanıtı ve source kaydıyla ayrı USB hedefinde 477 satırlı SHA-256 manifest/readback ve source checksum kapısından geçirildi; forbidden/credential/AppleDouble `0` olarak doğrulandı.
- Authoritative Shelf Offer milestone'ı 488 tracked kaynak, 5 scene/test/build/runtime kanıtı ve source kaydıyla ayrı USB hedefinde 494 satırlı SHA-256 manifest/readback ve source path/checksum kapısından geçirildi; forbidden/credential/AppleDouble `0` olarak doğrulandı.
- Customer Basket Reservation milestone'ı 498 tracked kaynak, 4 test/build/runtime kanıtı ve source kaydıyla ayrı USB hedefinde 503 satırlı SHA-256 manifest/readback ve source path/checksum kapısından geçirildi; forbidden/credential/AppleDouble `0` olarak doğrulandı.
- Immutable Checkout Snapshot milestone'ı 508 tracked kaynak, 4 test/build/runtime kanıtı ve source kaydıyla ayrı USB hedefinde 513 satırlı SHA-256 manifest/readback ve source path/checksum kapısından geçirildi; forbidden/credential/AppleDouble `0` olarak doğrulandı.
- Atomic Checkout Fulfillment milestone'ı 510 tracked kaynak, 4 test/build/runtime kanıtı ve source kaydıyla ayrı USB hedefinde 515 satırlı `ce72122a…db50b` SHA-256 manifest/readback ve Git blob eşliği kapısından geçirildi; hash/boyut/path/source mismatch, forbidden/credential/AppleDouble `0` olarak doğrulandı.
- Deterministic Customer Visit milestone'ı source/docs `d163328` ile 535 tracked kaynak, 5 test/build/runtime kanıtı ve source kaydıyla ayrı USB hedefinde 541 satırlı `c82fc76d…cfd` SHA-256 manifest/readback kapısından geçirildi; 535/535 Git-blob eşliği, forbidden/credential/AppleDouble `0` ve 9.715.834 payload baytı doğrulandı. Issue #46 kapatılıp Roadmap'te Done yapıldı; Epic #9 açık/In Progress kaldı.
- Explainable Single-Offer Customer Decision milestone'ı source/docs `8832c13` ile 541 tracked kaynak, 4 final test/build/runtime kanıtı ve source kaydıyla ayrı USB hedefinde 546 satırlı `d46e2433…d1b1` SHA-256 manifest/readback kapısından geçirildi; 541/541 Git-blob eşliği, forbidden/cache/credential/AppleDouble/sibling sidecar `0` ve 9.780.828 payload baytı doğrulandı. Issue #47 kapatılıp Roadmap'te Done yapıldı; Epic #9 açık/In Progress kaldı.
- Stale-Safe Buy Action and Checkout Navigation milestone'ı source/docs `aa61700` ile 547 tracked kaynak, 4 final test/build/runtime kanıtı ve source kaydıyla ayrı USB hedefinde 552 satırlı `05ed8205…e76f6` SHA-256 manifest/readback kapısından geçirildi; 547/547 Git-blob eşliği, evidence/forbidden/cache/credential/AppleDouble/sibling sidecar `0` ve 9.902.727 payload baytı doğrulandı. Issue #48 kapatılıp Roadmap'te Done yapıldı; Epic #9 açık/In Progress kaldı.
- Stale-Safe Leave Action and Offer-Declined Exit milestone'ı source/docs `868885a` ile 549 tracked kaynak, 4 final test/build/runtime kanıtı ve source kaydıyla ayrı USB hedefinde 554 satırlı `d685de7a…4209` SHA-256 manifest/readback kapısından geçirildi; 554/554 hash/boyut/path readback, 549/549 Git-blob, 4/4 evidence ve forbidden/cache/credential/AppleDouble/sibling sidecar mismatch `0`, 10.003.704 payload baytı doğrulandı. Issue #49 `15/15` acceptance ile kapatılıp Roadmap'te Done yapıldı; Epic #9 açık/In Progress kaldı.
- Atomic Cash Checkout and Initial Economy Settlement milestone'ı source/docs `aea6e2b` ile 566 tracked kaynak, 5 final test/build/runtime kanıtı ve source kaydıyla ayrı USB hedefinde 572 satırlı `b3168162…ecf8` SHA-256 manifest/readback kapısından geçirildi; 572/572 hash/boyut/path readback, 566/566 Git-blob, 5/5 evidence ve forbidden/cache/credential/AppleDouble/sibling sidecar mismatch `0`, 10.227.122 payload baytı doğrulandı. Issue #50 `18/18` acceptance ile kapatılıp Roadmap'te Done yapıldı; Epic #9 açık/In Progress kaldı.

## 2026-08-11 — Stage B Core Foundation

### Added

- Unity bağımsız `PSE.Core` assembly sınırı.
- Tür kapsamlı `StableId<TScope>`.
- Makine-okunur `Failure.Code`, `OperationResult` ve `OperationResult<T>`.
- Integer `SimulationDuration` / `SimulationTimestamp`.
- Açık-adımlı, pause güvenli `SimulationClock`.
- Stable metadata ve schema taşıyan immutable domain event envelope.
- Toplam 42 geçen Edit Mode testi.

## 2026-08-11 — Stage A Technical Baseline

### Added

- Unity 6000.3.21f1 + URP projesi ve kilitli resmî paketler.
- macOS Universal development build/headless smoke.
- Windows x64 Mono cross-build.
- Yerel Git `main` geçmişi ve `stage-a-baseline-2026-08-11` etiketi.
- Hash doğrulamalı USB milestone snapshot'ı.

### Known limitations

- Gerçek Windows x64 runtime/IL2CPP/DirectX/Steam testi henüz yapılmadı.
- UVCS ilk check-in'i uzak bağlantı reseti nedeniyle beklemede; Git tek authoritative VCS'dir.
- Küçük kutu alma/bırakma/placement/rotation/tek-kat istif, büyük-kutu güvenli taşıma, tek yüklü platform arabası ve authoritative Catalog/Inventory çekirdeği çalışıyor; gelişmiş el animasyonu, çoklu/palet taşıma, gerçek raf adaptörü ve final sanat henüz tamamlanmadı.
