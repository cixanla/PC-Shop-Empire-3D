# PC Shop Empire 3D — Devam ve Kullanım Güvenliği Checkpoint'i

**Tarih:** 15 Ağustos 2026<br>
**Durum:** Issue #48 stale-safe `Buy` özelliği kod/test/build/runtime/feature CI ile tamamlandı; source/docs, USB ve GitHub kapanışı yürütülüyor<br>
**Authoritative kaynak:** private GitHub `cixanla/PC-Shop-Empire-3D`, `main`

## En yeni checkpoint — Issue #48 / Epic #9

- Feature commit `6951869c4a9f33662f322c02348fa4282b9cdbb6`, tree `5f4c956423bbc07b9087d47f7886ab36cc6992f1`.
- `CustomerOfferDecisionActionAuthority`, immutable `Buy` kararını current visit ve current RAF A offer'ıyla yeniden değerlendirir; stale karar `retail.offer-action.decision-stale` ile bütün authority'lerde no-mutation kalır.
- Actors↔Retail customer kimliği immutable typed binding ile explicit eşlenir. Caller exact action/line/basket/item/reservation/claim kimliklerini verir; alternatif item seçimi veya RNG yoktur.
- Inventory, Basket ve Actors side-effect-free prepared planları bütün preflight'ları ilk mutation'dan önce kapatır. Başarı exact serialized reservation ve `Browsing → NavigatingToCheckout` geçişini birer revision ile üretir.
- Action-owned reservation `ConsumeOnly`dır; legacy Basket toggle, public release ve public tekli/toplu consume mutation öncesi reddedilir. Mevcut checkout fulfillment internal consume sınırı korunur.
- Exact action replay idempotent; conflicting replay ve aynı visit için ikinci ActionId fail-closed'dur. Historical action receipt fulfillment/exit sonrası invariant-safe kalır.
- Garage'da gerçek `G / Gamepad East` current `Buy` eylemini uygular; başarı ve stale failure renkten bağımsız stable metinle görünür.
- EditMode `287/287`, gerçek Input System PlayMode `19/19`; failed/skipped `0`.
- Universal macOS development build ve Apple M4/Metal 1280×720 native runtime `garage-buy-action-r17-v1`, `buy-action=ok stale-blocked=ok authority-isolated=ok` ile başarılıdır.
- Karar: `Docs/ADR-0026-STALE-SAFE-BUY-ACTION-AND-CHECKOUT-NAVIGATION.md`; kanıt: `Docs/Evidence/STALE-SAFE-BUY-ACTION-AND-CHECKOUT-NAVIGATION-CHECKPOINT-2026-08-15.md`.

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
- Catalog/Inventory/Orders/Retail zinciri; authoritative teslim alma, parcel açma, shelf offer, basket reservation, checkout snapshot, atomik fulfillment, saf tek-offer kararı ve stale-safe Buy action katmanlarını içerir.
- Actors sınırı; kararlı müşteri intent/visit modeli, monotonik lifecycle, bounded route retry/fallback ve receipt ledger'ını içerir.
- İlk oynanabilir sahne: `Assets/Scenes/Prototypes/GarageGraybox.unity`; runtime marker `garage-buy-action-r17-v1`.
- Küçük kutu placement/rotation/stacking, büyük kutu carry, yüklü platform arabası, stable item ID, domain-first rollback ve recovery invariantları korunur.
- Görsel hedef okunaklı yarı gerçekçiliktir; mevcut primitive garaj, kutular, eller ve müşteri final sanat değildir.
- Gerçek Windows x64 runtime/DirectX/Steam/IL2CPP testi dış platform kapısıdır.

## Feature checkpoint ve doğrulama kanıtı

- Epic/issue: [#9](https://github.com/cixanla/PC-Shop-Empire-3D/issues/9) / [#48](https://github.com/cixanla/PC-Shop-Empire-3D/issues/48).
- Feature commit: `6951869c4a9f33662f322c02348fa4282b9cdbb6`.
- Feature tree: `5f4c956423bbc07b9087d47f7886ab36cc6992f1`.
- Feature Repository Guard: [31880394269](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31880394269), başarılı.
- Source/docs commit, USB milestone ve Issue/Project kapanışı bu checkpointten sonra tamamlanacaktır.
- EditMode XML: `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/editmode-action48-r6-final.xml`; `287/287`; SHA-256 `3bd1e3169cfda36a8b13e6b4d5bbf5f4f7fa7b9c5e9b5ccc2acc0aebc32c9bd3`.
- PlayMode XML: `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/playmode-action48-r7-full.xml`; `19/19`; SHA-256 `caee9b22125f698c6b3e6758c6f983e2be84b7cf25276e1e391a4b867df8735e`.
- Build log: `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/build-action48-macos-r2-final.log`; `STAGE_A_BUILD_OK target=StandaloneOSX bytes=327737593`; SHA-256 `404bd6148bcc7a268f54d39e34722ae8701fe27bf0c8e547389af220dd0ef35c`.
- Universal app executable: Mach-O `x86_64 + arm64`; SHA-256 `b58c255a9ffcfca2032cf2bbf5008c372f0b7da8d20e020aaa267a909a2bb88d`.
- Runtime log: `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/runtime-action48-macos-r2-final.log`; Apple M4/Metal 1280×720; SHA-256 `084b139a37337b4dcf5a4dea53d942ad206bf046c76945b41aa153abe7657585`.
- Sahne: `Assets/Scenes/Prototypes/GarageGraybox.unity`; SHA-256 `16376412909b92e06eceae83e412111770ca06a6503c9cbd975427d8d25ed685`.
- Runtime ready: `garage-buy-action-r17-v1 customer-buy-action=ready customer-visit=ready customer-navmesh=ready lookdev=ok`.
- Runtime smoke: `customer-visit=ok runtime-route=ok pause=ok offer-decision=ok buy-action=ok stale-blocked=ok fulfilled=ok domain-route-fallback=ok domain-timeout-fallback=ok authority-isolated=ok stock-consumed=ok stock-projection-hidden=ok customer-hidden=ok`.
- Final loglarda assertion, unhandled exception, smoke failure veya `JobTempAlloc` sızıntısı yoktur.

## Bilinçli kapsam dışı

- `Leave` action, `OfferDeclined` exit reason ve `Browsing → Exiting` lifecycle.
- Utility scoring, çoklu ürün/offer seçimi, çoklu müşteri ve sıra kapasitesi.
- Ödeme yöntemi, Economy ledger, nakit, gelir, COGS, vergi, indirim ve fiş/fatura.
- Memnuniyet/itibar, çalışan AI, Save/Guardian, final model/animasyon/ses ve gerçek Windows doğrulaması.

## USB güvenli checkpoint durumu

- Güncel milestone: `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-15_STAGE_B_EXPLAINABLE_SINGLE_OFFER_CUSTOMER_DECISION`.
- Source/docs `8832c1372566ede623f08e04b5d9385b6ad23739`; 541 tracked `SOURCE`, 4 final `EVIDENCE`, bir `SOURCE_COMMIT.txt`; 546 manifest payload satırı, toplam 548 dosya ve 9.780.828 payload baytı.
- `MANIFEST.tsv` SHA-256: `d46e2433a7729587c71904479c09ec2e24f6df1f956794880d627a6448c0d1b1`.
- 546/546 hash/boyut/path readback ve 541/541 Git-blob eşliği geçti; forbidden/cache/credential/AppleDouble ve sibling sidecar `0`.
- `.git`, Unity cache/build, token, parola, credential ve AppleDouble snapshot dışında kaldı.
- Snapshot bağımsız salt-okunur denetimde de aynı sayım, manifest, Git-blob ve güvenlik kapılarıyla başarılı bulundu.

## Sıradaki bounded paket

Issue #9 altında `Leave` kararını güvenli eyleme dönüştüren ayrı bounded paket:

1. Immutable `Leave` kararını current visit/offer snapshotlarıyla fail-closed yeniden doğrulamak.
2. Explicit Actors↔Retail binding ile `Browsing → Exiting` ve stable `OfferDeclined` nedenini no-mutation preflight sonrası uygulamak.
3. Tamamlanan `Buy` reservation/navigation zincirini ve historical action receipt invariantlarını regresyon olarak korumak.
4. Checkout başlatma, ödeme/Economy, çoklu offer/customer, memnuniyet, Save ve final karakter sanatını kapsam dışında tutmak.

## Güvenli devam komutu

Issue #48 feature `6951869`, Guard `31880394269`, EditMode `287/287`, PlayMode `19/19` ve `garage-buy-action-r17-v1 buy-action=ok stale-blocked=ok authority-isolated=ok` ile doğrulandı. Önce source/docs + USB + Issue/Project kapanışını bitir; sonra Epic #9 altında bounded `Leave/OfferDeclined` action dilimine geç. Checkout/payment/Economy/Save sınırlarını karıştırma.
