# PC Shop Empire 3D — Devam ve Kullanım Güvenliği Checkpoint'i

**Tarih:** 15 Ağustos 2026<br>
**Durum:** Issue #46 kapsamındaki deterministik tek-müşteri ziyaret ve runtime NavMesh graybox dilimi teknik olarak tamamlandı; yaşayan belge, Issue/Project ve USB kapanışı sürüyor<br>
**Authoritative kaynak:** private GitHub `cixanla/PC-Shop-Empire-3D`, `main`

## En yeni checkpoint — Issue #46 / Epic #9

- Feature commit `b37b056271fac317e99ec47df0833b8ef219cf83`, tree `cca44dcf50f262e64fa9d6b43b48d25722978f64`.
- `PSE.Actors`, kararlı müşteri/intent/visit kimliklerini, immutable ziyaret state'ini ve en fazla sekiz kayıt taşıyan bounded command receipt ledger'ını Unity'den bağımsız tutar.
- Lifecycle yalnız `Entering → Browsing → NavigatingToCheckout → AwaitingCheckout → Exiting → Exited` yönünde ilerler; terminal ve exact replay davranışı deterministiktir.
- Route denemeleri ikiyle sınırlıdır. NavMesh route bulunamazsa `RouteUnavailable`; sabır veya çıkış süresi dolarsa açıklanabilir timeout fallback'i üretilir. Hiçbir fallback stok, sepet, checkout, sipariş veya para uydurmaz.
- Receipt doğrulaması route provenance/window, aktif state route sayısı, fulfilled çıkış başlangıcı ve normal `Exited` varış kanıtını fail-closed denetler.
- Garaj runtime'ı görünür müşteri projection'ını explicit giriş, RAF A göz atma, checkout bekleme ve çıkış noktalarına bağlar. Domain state NPC transformundan yönetilmez.
- Pause sırasında `SimulationClock` ve müşteri akışı ilerlemez. Runtime smoke hem normal authored-route akışını hem de route/timeout fallback'lerini ve diğer authority revision'larının izolasyonunu doğrular.
- EditMode `255/255`, gerçek Input System PlayMode `18/18`; failed/skipped `0`.
- Universal macOS development build ve Apple M4/Metal 1280×720 native runtime smoke başarılıdır.
- Karar: `Docs/ADR-0024-DETERMINISTIC-CUSTOMER-VISIT-AND-BOUNDED-ROUTE-FALLBACK.md`; kanıt: `Docs/Evidence/DETERMINISTIC-CUSTOMER-VISIT-CHECKPOINT-2026-08-15.md`.

## Kullanım güvenliği protokolü

- Kullanıcı durdurana veya gerçek bir dış engel oluşana kadar bağımlılık sırasındaki küçük, geri alınabilir paketler soru gerektirmeden sürdürülür.
- Her bounded paket kod/test → Git commit → private push → CI → yaşayan belgeler → ayrı USB milestone sırasıyla kapanır.
- Büyük indirme, ücretli araç, Steam/Apple ödemesi, üçüncü taraf asset, motor/proje migration'ı ve destructive işlem ayrı açıklama/onay kapısıdır.
- Kullanıcı değişikliği, credential, legacy canonical kaynak ve ilgisiz çalışma ağacı değişiklikleri korunur.
- Yardımcı Codex görevleri yalnız ayrık, bounded işler alır; ana Git/Unity deposunun tek doğruluk kaynağı olma niteliğini değiştirmez.

## Son sağlam teknik durum

- Unity proje kökü: `/Users/cixanla/Developer/PCShopEmpire3D/Game`; Unity `6000.3.21f1`, URP `17.3.0`, C#.
- Branch: `main`; feature checkpointte yerel HEAD ile `origin/main` eşittir.
- Core stable ID/result/time, sürümlü PCG32, SHA-256 stream derivation ve deterministic event dispatcher tamamdır.
- Catalog/Inventory/Orders/Retail zinciri; authoritative teslim alma, parcel açma, shelf offer, basket reservation, checkout snapshot ve atomik fulfillment katmanlarını içerir.
- Actors sınırı; kararlı müşteri intent/visit modeli, monotonik lifecycle, bounded route retry/fallback ve receipt ledger'ını içerir.
- İlk oynanabilir sahne: `Assets/Scenes/Prototypes/GarageGraybox.unity`; runtime marker `garage-customer-visit-r15-v1`.
- Küçük kutu placement/rotation/stacking, büyük kutu carry, yüklü platform arabası, stable item ID, domain-first rollback ve recovery invariantları korunur.
- Görsel hedef okunaklı yarı gerçekçiliktir; mevcut primitive garaj, kutular, eller ve müşteri final sanat değildir.
- Gerçek Windows x64 runtime/DirectX/Steam/IL2CPP testi dış platform kapısıdır.

## Feature checkpoint ve doğrulama kanıtı

- Epic/issue: [#9](https://github.com/cixanla/PC-Shop-Empire-3D/issues/9) / [#46](https://github.com/cixanla/PC-Shop-Empire-3D/issues/46).
- Feature commit: `b37b056271fac317e99ec47df0833b8ef219cf83`.
- Feature tree: `cca44dcf50f262e64fa9d6b43b48d25722978f64`.
- Feature Repository Guard: [31875039147](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31875039147), başarılı.
- Source/docs checkpoint: bu belgeyi taşıyan kapanış commitinde oluşturulacak; exact SHA ve Guard sonucu USB kapanışında buraya yazılacaktır.
- EditMode XML: `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/editmode-customer46-r17-final.xml`; `255/255`; SHA-256 `8e4e8ab5f628214f07ccd7955e31788c95a64d3a031433df746b1eeaa7d6c6a8`.
- PlayMode XML: `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/playmode-customer46-r17-final.xml`; `18/18`; SHA-256 `3787c37f5871866b1e4926fcaa070a65a79e31bcf2fc3bd3abf9c49edfe9c811`.
- Build log: `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/build-customer46-macos-r17-final.log`; `STAGE_A_BUILD_OK target=StandaloneOSX bytes=327697921`; SHA-256 `3b6b2338469bf2b1957fff74b7826579bd55fcb60f64383d08d2f3344ebd6378`.
- Universal app executable: Mach-O `x86_64 + arm64`; SHA-256 `f62879d166ed6359ee3a0df80a771aaf52d9c93efe2ceb456960ca256a4302aa`.
- Normal runtime log: `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/runtime-customer46-macos-r17-final.log`; Apple M4/Metal 1280×720; SHA-256 `83fa744459883cb5b871254f583171d132888d7fa3d455d1796ceeda38482514`.
- Leak diagnostic runtime log: `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/runtime-customer46-macos-r16-leakdiag.log`; SHA-256 `e254918c22650d719ad915f7156c535b8fb0c6acb8cb0c044cc69de426e26ba4`.
- Sahne: `Assets/Scenes/Prototypes/GarageGraybox.unity`; SHA-256 `16376412909b92e06eceae83e412111770ca06a6503c9cbd975427d8d25ed685`.
- Runtime ready: `customer-visit=ready customer-navmesh=ready lookdev=ok`.
- Runtime smoke: `customer-visit=ok runtime-route=ok pause=ok fulfilled=ok domain-route-fallback=ok domain-timeout-fallback=ok authority-isolated=ok stock-consumed=ok stock-projection-hidden=ok customer-hidden=ok`.
- Normal ve leakdiag loglarında exception, smoke failure veya `JobTempAlloc` sızıntısı yoktur.

## Bilinçli kapsam dışı

- Utility scoring, çoklu ürün/offer seçimi, çoklu müşteri ve sıra kapasitesi.
- Ödeme yöntemi, Economy ledger, nakit, gelir, COGS, vergi, indirim ve fiş/fatura.
- Memnuniyet/itibar, çalışan AI, Save/Guardian, final model/animasyon/ses ve gerçek Windows doğrulaması.

## USB güvenli checkpoint durumu

- Son tamamlanmış USB milestone: `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-15_STAGE_B_ATOMIC_CHECKOUT_FULFILLMENT`.
- Issue #46 için yeni snapshot, source/docs checkpoint commitinden üretilecektir; `.git`, Unity cache/build, token, parola, credential ve AppleDouble dosyaları dahil edilmeyecektir.
- Snapshot tamamlanmadan path, dosya sayısı, bayt ve manifest hash'i kesin kanıt olarak yazılmaz.

## Sıradaki bounded paket

Issue #9 altında tek müşterinin tek shelf offer için deterministik ve açıklanabilir `buy/leave` kararı:

1. Stable intent/visit ile immutable offer/product snapshot'ını revision-aware değerlendirmek.
2. Sonucu stable reason code ve idempotent receipt ile üretmek; stok, sepet, Economy veya Save'i doğrudan mutate etmemek.
3. Garajda mevcut reservation/checkout zincirine yalnız açık adapter üzerinden bağlamak.
4. Çoklu ürün/müşteri, fiyat optimizasyonu, Economy, memnuniyet ve final karakter sanatını sonraki bounded paketlerde tutmak.

## Güvenli devam komutu

Önce bu checkpointi taşıyan docs commitini oluşturup push/Repository Guard ile doğrula; Issue #46'yı Done yap; exact Git kaynağından doğrulanmış USB snapshotını üret; kapanış commitini ve Guard'ı kaydet. Ardından Issue #9 altındaki tek-offer `buy/leave` paketine geç.
