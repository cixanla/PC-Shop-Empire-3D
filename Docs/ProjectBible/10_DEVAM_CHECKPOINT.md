# PC Shop Empire 3D — Devam ve Kullanım Güvenliği Checkpoint'i

**Tarih:** 15 Ağustos 2026<br>
**Durum:** Issue #47 feature commit/push/CI tamamlandı; açıklanabilir tek-offer `Buy/Leave` kararı için source/docs ve USB kapanışı sürüyor<br>
**Authoritative kaynak:** private GitHub `cixanla/PC-Shop-Empire-3D`, `main`

## En yeni checkpoint — Issue #47 / Epic #9

- Feature commit `f97ded34f00e0d0637fbf9b41c0c0d33a7969b8e`, tree `e8cddbc13166b35a081786fed895417cf6270c16`.
- `PSE.Retail`, tek yönlü `PSE.Actors` referansıyla immutable `CustomerVisitRecord`, `ShelfOfferRecord` ve integer minor-unit accepted-price limitini saf/stateless biçimde değerlendirir. `PSE.Actors` bağımlılıkları değişmez.
- Validation sırası structural input → `Browsing` → supported need → currency → product → fiyat olarak sabittir. Geçerli mismatch/above-limit `Leave`, exact product equal/below-limit `Buy` üretir.
- Immutable karar exact customer/visit/intent/offer/price provenance ve stable reason code taşır. Exact replay value-equal; eski offer ve Browsing snapshot replay'i güncel action yetkisi değildir.
- Evaluator authority, cache, revision, receipt, clock, RNG, Inventory, Basket, Checkout, NavMesh veya Unity nesnesi kabul etmez; bütün gameplay authority'lerinde no-mutation kalır.
- Garage yalnız `Browsing` sırasında `KARAR: SATIN AL / AYRIL` ve reason code gösterir. Okuma reservation, checkout veya visit transition başlatmaz; mevcut explicit input akışı ayrı kalır.
- EditMode `267/267`, gerçek Input System PlayMode `18/18`; failed/skipped `0`.
- Universal macOS development build ve Apple M4/Metal 1280×720 native runtime `garage-offer-decision-r16-v1`, `offer-decision=ok authority-isolated=ok` ile başarılıdır.
- Karar: `Docs/ADR-0025-EXPLAINABLE-SINGLE-OFFER-CUSTOMER-DECISION.md`; kanıt: `Docs/Evidence/EXPLAINABLE-SINGLE-OFFER-CUSTOMER-DECISION-CHECKPOINT-2026-08-15.md`.

## Kullanım güvenliği protokolü

- Kullanıcı durdurana veya gerçek bir dış engel oluşana kadar bağımlılık sırasındaki küçük, geri alınabilir paketler soru gerektirmeden sürdürülür.
- Her bounded paket kod/test → Git commit → private push → CI → yaşayan belgeler → ayrı USB milestone sırasıyla kapanır.
- Büyük indirme, ücretli araç, Steam/Apple ödemesi, üçüncü taraf asset, motor/proje migration'ı ve destructive işlem ayrı açıklama/onay kapısıdır.
- Kullanıcı değişikliği, credential, legacy canonical kaynak ve ilgisiz çalışma ağacı değişiklikleri korunur.
- Yardımcı Codex görevleri yalnız ayrık, bounded işler alır; ana Git/Unity deposunun tek doğruluk kaynağı olma niteliğini değiştirmez.

## Son sağlam teknik durum

- Unity proje kökü: `/Users/cixanla/Developer/PCShopEmpire3D/Game`; Unity `6000.3.21f1`, URP `17.3.0`, C#.
- Branch: `main`; source/docs checkpointte yerel HEAD ile `origin/main` eşittir.
- Core stable ID/result/time, sürümlü PCG32, SHA-256 stream derivation ve deterministic event dispatcher tamamdır.
- Catalog/Inventory/Orders/Retail zinciri; authoritative teslim alma, parcel açma, shelf offer, basket reservation, checkout snapshot, atomik fulfillment ve saf tek-offer müşteri kararı katmanlarını içerir.
- Actors sınırı; kararlı müşteri intent/visit modeli, monotonik lifecycle, bounded route retry/fallback ve receipt ledger'ını içerir.
- İlk oynanabilir sahne: `Assets/Scenes/Prototypes/GarageGraybox.unity`; runtime marker `garage-offer-decision-r16-v1`.
- Küçük kutu placement/rotation/stacking, büyük kutu carry, yüklü platform arabası, stable item ID, domain-first rollback ve recovery invariantları korunur.
- Görsel hedef okunaklı yarı gerçekçiliktir; mevcut primitive garaj, kutular, eller ve müşteri final sanat değildir.
- Gerçek Windows x64 runtime/DirectX/Steam/IL2CPP testi dış platform kapısıdır.

## Feature checkpoint ve doğrulama kanıtı

- Epic/issue: [#9](https://github.com/cixanla/PC-Shop-Empire-3D/issues/9) / [#47](https://github.com/cixanla/PC-Shop-Empire-3D/issues/47).
- Feature commit: `f97ded34f00e0d0637fbf9b41c0c0d33a7969b8e`.
- Feature tree: `e8cddbc13166b35a081786fed895417cf6270c16`.
- Feature Repository Guard: [31876993251](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31876993251), başarılı.
- Source/docs checkpoint ve Guard: bu yaşayan belge turundaki commit/push sonrasında eklenecek.
- Issue #47 açık/In Progress; doğrulanmış USB snapshot sonrasında Done/close yapılacak. Epic #9 açık/In Progress kalır.
- EditMode XML: `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/editmode-offer47-r3-final.xml`; `267/267`; SHA-256 `06847e5696aa29a73d99672bb00e894205c4e840a950256398d91a81b9446129`.
- PlayMode XML: `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/playmode-offer47-r2-final.xml`; `18/18`; SHA-256 `133c26469fa0c074b365be265567326bff1f84fcd25b04e71f0ccadfb960677c`.
- Build log: `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/build-offer47-macos-r1.log`; `STAGE_A_BUILD_OK target=StandaloneOSX bytes=327708376`; SHA-256 `b2c109d4232c97e6ff17229057eb207e10299c60105611e1d3341b5555c95522`.
- Universal app executable: Mach-O `x86_64 + arm64`; SHA-256 `68fd897fafc53d2560bd3a1261767ff3a91a09c7a41df6da2df0b61493cd67de`.
- Runtime log: `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/runtime-offer47-macos-r1.log`; Apple M4/Metal 1280×720; SHA-256 `d28254dc7e74a2723215fe20d0b82c84c4fd688e864840ef2ba92e0c2a023195`.
- Sahne: `Assets/Scenes/Prototypes/GarageGraybox.unity`; SHA-256 `16376412909b92e06eceae83e412111770ca06a6503c9cbd975427d8d25ed685`.
- Runtime ready: `garage-offer-decision-r16-v1 customer-visit=ready customer-navmesh=ready lookdev=ok`.
- Runtime smoke: `customer-visit=ok runtime-route=ok pause=ok offer-decision=ok fulfilled=ok domain-route-fallback=ok domain-timeout-fallback=ok authority-isolated=ok stock-consumed=ok stock-projection-hidden=ok customer-hidden=ok`.
- Final loglarda assertion, unhandled exception, smoke failure veya `JobTempAlloc` sızıntısı yoktur.

## Bilinçli kapsam dışı

- Kararı basket/lifecycle action'a uygulama, stale/current snapshot revalidation ve Actors↔Retail customer ID köprüsü.
- Utility scoring, çoklu ürün/offer seçimi, çoklu müşteri ve sıra kapasitesi.
- Ödeme yöntemi, Economy ledger, nakit, gelir, COGS, vergi, indirim ve fiş/fatura.
- Memnuniyet/itibar, çalışan AI, Save/Guardian, final model/animasyon/ses ve gerçek Windows doğrulaması.

## USB güvenli checkpoint durumu

- Güncel milestone: `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-15_STAGE_B_DETERMINISTIC_CUSTOMER_VISIT`.
- Source/docs `d163328`; 535 tracked `SOURCE`, 5 `EVIDENCE`, bir `SOURCE_COMMIT.txt`; 541 manifest payload satırı ve 9.715.834 payload baytı.
- `MANIFEST.tsv` SHA-256: `c82fc76dcf7b12f60e5106fb9dc78cf7f942824777b5a0db3902144125bb2cfd`.
- Hash/boyut/path readback, 535/535 Git-blob eşliği geçti; forbidden/credential/AppleDouble `0`.
- `.git`, Unity cache/build, token, parola, credential ve AppleDouble snapshot dışında kaldı.
- Issue #47 için yeni `EXPLAINABLE_SINGLE_OFFER_CUSTOMER_DECISION` milestone'ı source/docs commit ve Guard sonrasında oluşturulacak; tamamlanmadan #47 kapatılmayacak.

## Sıradaki bounded paket

Issue #9 altında `Buy/Leave` kararını güvenli eyleme dönüştüren ayrı bounded paket:

1. Kararın visit last-updated ve offer revision provenance'ını current authority snapshotlarıyla fail-closed yeniden doğrulamak.
2. Actors↔Retail customer kimlik eşlemesini explicit yapmak; stale kararın reservation/checkout/exit action'ı başlatmasını engellemek.
3. `Buy` reservation/checkout-navigation ve `Leave` güvenli exit davranışlarını atomic/no-mutation preflight ile birbirinden ayırmak.
4. Ödeme/Economy, çoklu offer/customer, memnuniyet, Save ve final karakter sanatını kapsam dışında tutmak.

## Güvenli devam komutu

Issue #47 feature `f97ded3` ve Guard `31876993251` başarılıdır. Önce source/docs Guard ve USB milestone kapanışını tamamla, #47'yi Done/close yap ve temiz `main` bırak; ardından Epic #9 altındaki stale-safe karar action paketini aç. Inventory/Retail/Economy/Save sınırlarını karıştırma.
