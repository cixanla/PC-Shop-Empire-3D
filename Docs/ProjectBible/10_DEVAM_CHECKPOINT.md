# PC Shop Empire 3D — Devam ve Kullanım Güvenliği Checkpoint'i

**Tarih:** 15 Ağustos 2026<br>
**Durum:** Issue #49 stale-safe `Leave/OfferDeclined` paketi source/docs/CI/USB dahil tamamlandı; Issue kapalı/Done, Epic #9 açık/In Progress<br>
**Authoritative kaynak:** private GitHub `cixanla/PC-Shop-Empire-3D`, `main`

## En yeni checkpoint — Issue #49 / Epic #9

- Feature commit `67d858aff773610cff6d6c221c792cd793f27a1b`, tree `dc76a89a5a9f0f9349509aca7374f30518b1c308`; Repository Guard `31882228394` başarılı.
- Source/docs commit `868885af9065d4e9fb274c3862fd525b040e1cc2`, tree `66c44529a5bb2cde92903d8fee06ef4d2ed7f667`; Repository Guard `31882508496` başarılı. Issue #49 `15/15` acceptance ile kapalı ve Roadmap'te `Done`dır.
- Ortak `CustomerOfferDecisionActionAuthority`, immutable `Leave` kararını current visit ve current RAF A offer'ıyla tam value-equal yeniden değerlendirir; stale karar `retail.offer-action.decision-stale` ile bütün authority'lerde no-mutation kalır.
- Actors↔Retail customer kimliği immutable typed binding ile explicit eşlenir. Leave hiçbir line/basket/item/reservation/claim kimliği taşımaz; stok ve kasa authority'leri değişmez.
- `PSE.Actors` internal friend prepared planı `Browsing → Exiting` ve stable `OfferDeclined` geçişini side-effect-free preflight sonrası uygular. Public Presentation bypass'ı ve cross-kind replay kapalıdır.
- Exact action replay idempotent; conflicting/cross-kind replay ve aynı visit için ikinci ActionId fail-closed'dur. Historical receipt exit sonrası invariant-safe kalır; route fallback/timeout `OfferDeclined` nedenini korur.
- Garage'da gerçek `G / Gamepad East` current `Leave` eylemini uygular; başarı/stale failure renkten bağımsız stable metinle görünür. NavMesh kontratı `Browse → Exit` yolunu kapsar.
- EditMode `298/298`, gerçek Input System PlayMode `22/22`; failed/skipped `0`.
- Universal macOS development build ve Apple M4/Metal 1280×720 native runtime `garage-leave-action-r18-v1`, `leave-action=ok stale-leave-blocked=ok authority-isolated=ok` ile başarılıdır.
- Karar: `Docs/ADR-0027-STALE-SAFE-LEAVE-ACTION-AND-OFFER-DECLINED-EXIT.md`; kanıt: `Docs/Evidence/STALE-SAFE-LEAVE-ACTION-AND-OFFER-DECLINED-EXIT-CHECKPOINT-2026-08-15.md`.

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
- Catalog/Inventory/Orders/Retail zinciri; authoritative teslim alma, parcel açma, shelf offer, basket reservation, checkout snapshot, atomik fulfillment, saf tek-offer kararı ve stale-safe Buy/Leave action katmanlarını içerir.
- Actors sınırı; kararlı müşteri intent/visit modeli, monotonik lifecycle, bounded route retry/fallback, `OfferDeclined` ve receipt ledger'ını içerir.
- İlk oynanabilir sahne: `Assets/Scenes/Prototypes/GarageGraybox.unity`; runtime marker `garage-leave-action-r18-v1`.
- Küçük kutu placement/rotation/stacking, büyük kutu carry, yüklü platform arabası, stable item ID, domain-first rollback ve recovery invariantları korunur.
- Görsel hedef okunaklı yarı gerçekçiliktir; mevcut primitive garaj, kutular, eller ve müşteri final sanat değildir.
- Gerçek Windows x64 runtime/DirectX/Steam/IL2CPP testi dış platform kapısıdır.

## Feature checkpoint ve doğrulama kanıtı

- Epic/issue: [#9](https://github.com/cixanla/PC-Shop-Empire-3D/issues/9) / [#49](https://github.com/cixanla/PC-Shop-Empire-3D/issues/49).
- Feature commit: `67d858aff773610cff6d6c221c792cd793f27a1b`.
- Feature tree: `dc76a89a5a9f0f9349509aca7374f30518b1c308`.
- Feature Repository Guard: [31882228394](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31882228394), başarılı.
- Source/docs commit: `868885af9065d4e9fb274c3862fd525b040e1cc2`; tree: `66c44529a5bb2cde92903d8fee06ef4d2ed7f667`; Repository Guard: [31882508496](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31882508496), başarılı.
- EditMode XML: `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/editmode-action49-r2.xml`; `298/298`; SHA-256 `be7e56fad9418de9883100653bdf90722ebd13bc896fcf5432ee86a195d1feea`.
- PlayMode XML: `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/playmode-action49-r2.xml`; `22/22`; SHA-256 `8856709e0fc3c193359d9e3576960512aa261d9a9efdff7b1b775b5ae0658ece`.
- Build log: `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/build-action49-macos-r1.log`; `STAGE_A_BUILD_OK target=StandaloneOSX bytes=327750560`; SHA-256 `c1e317068753a9668ec3737ca0cd69b0c6a37a77c77ea03653a4debd56a75df8`.
- Universal app executable: Mach-O `x86_64 + arm64`; SHA-256 `ec4bbd1a532f1cafb92baec1401c71a874576e4db441046f90238d94b0db605d`.
- Runtime log: `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/runtime-action49-macos-r1.log`; Apple M4/Metal 1280×720; SHA-256 `02c2ed8900937c0693cf867486f746923920ec048990397e1a2db3374fed6891`.
- Sahne: `Assets/Scenes/Prototypes/GarageGraybox.unity`; SHA-256 `16376412909b92e06eceae83e412111770ca06a6503c9cbd975427d8d25ed685`.
- Runtime ready: `garage-leave-action-r18-v1 customer-buy-action=ready customer-leave-action=ready customer-visit=ready customer-navmesh=ready lookdev=ok`.
- Runtime smoke: `customer-visit=ok runtime-route=ok pause=ok offer-decision=ok buy-action=ok stale-blocked=ok fulfilled=ok leave-action=ok stale-leave-blocked=ok domain-route-fallback=ok domain-timeout-fallback=ok authority-isolated=ok stock-consumed=ok stock-projection-hidden=ok customer-hidden=ok`.
- Final loglarda assertion, unhandled exception, smoke failure veya `JobTempAlloc` sızıntısı yoktur.

## Bilinçli kapsam dışı

- Utility scoring, çoklu ürün/offer seçimi, çoklu müşteri ve sıra kapasitesi.
- Ödeme yöntemi, Economy ledger, nakit, gelir, COGS, vergi, indirim ve fiş/fatura.
- Memnuniyet/itibar, çalışan AI, Save/Guardian, final model/animasyon/ses ve gerçek Windows doğrulaması.

## USB güvenli checkpoint durumu

- Güncel milestone: `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-15_STAGE_B_STALE_SAFE_LEAVE_ACTION_AND_OFFER_DECLINED_EXIT`.
- Source/docs `868885af9065d4e9fb274c3862fd525b040e1cc2`; 549 tracked `SOURCE`, 4 final `EVIDENCE`, bir `SOURCE_COMMIT.txt`; 554 manifest payload satırı, toplam 556 dosya ve 10.003.704 payload baytı.
- `MANIFEST.tsv` SHA-256: `d685de7afdd8df0dcba16369d2232c48725a365db15d67ad1cbdae78269a4209`.
- 554/554 hash/boyut/path readback, 549/549 Git-blob ve 4/4 evidence eşliği geçti; forbidden/cache/credential/AppleDouble ve sibling sidecar mismatch sayıları `0`.
- `.git`, Unity cache/build, token, parola, credential ve AppleDouble snapshot dışında kaldı.
- Snapshot bağımsız salt-okunur denetimde de aynı sayım, manifest, Git-blob ve güvenlik kapılarıyla başarılı bulundu.

## Sıradaki bounded paket

Issue #9 altında atomik ödeme ve ilk Economy settlement sınırı:

1. Immutable checkout completion snapshot'ını exact payment/settlement receipt'ine bağlamak.
2. Nakit, gelir ve COGS double-entry etkisini tek preflight sonrası atomik ve idempotent uygulamak.
3. Payment failure/conflict/stale replay'de Inventory/Basket/Checkout/Orders/Actors/Economy no-mutation kanıtlamak.
4. Vergi/indirim/fiş/fatura, çoklu ödeme yöntemi, çoklu customer/offer, Save ve final UI'ı ayrı tutmak.

## Güvenli devam komutu

Issue #49 feature `67d858a`, source/docs `868885a`, iki başarılı Guard, EditMode `298/298`, PlayMode `22/22`, Mac `leave-action=ok stale-leave-blocked=ok authority-isolated=ok` ve doğrulanmış USB milestone ile kapalı/Done'dır. Epic #9 altında atomik payment/ilk Economy settlement dilimine geç; vergi/indirim/Save sınırlarını karıştırma.
