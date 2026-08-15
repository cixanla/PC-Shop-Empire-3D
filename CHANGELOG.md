# Changelog

Bu dosya teknik ve proje yönetimi checkpointlerini izler. Ayrıntılı oyun kararları `PROJECT_BIBLE.md`, `Docs/ProjectBible/06_PROJE_HAFIZASI.md` ve ADR'lerde tutulur.

## Unreleased

### Added

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
- Edit Mode baseline `255/255`, Play Mode baseline `18/18` teste yükseldi; Universal macOS build ve Apple M4/Metal 1280×720 `runtime-route=ok pause=ok fulfilled=ok domain-route-fallback=ok domain-timeout-fallback=ok authority-isolated=ok` gerçek player smoke geçti.

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
