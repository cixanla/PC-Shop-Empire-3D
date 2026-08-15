# ADR-0024 — Deterministic Customer Visit and Bounded Route Fallback

**Tarih:** 15 Ağustos 2026
**Durum:** Kabul edildi ve Issue #46 ile uygulandı
**Bağlam:** Epic #9 — authoritative müşteri ziyareti ve yalnız sunum olan NavMesh sınırı

## Bağlam

Issue #45 sonunda ürün RAF A'dan atomik olarak tüketiliyor ve immutable checkout completion kaydı oluşuyordu; fakat mağazaya giren müşterinin niyeti, yaşam döngüsü, sabrı ve yol bulma başarısızlığı için Unity'den bağımsız bir authority yoktu. NPC transformunu veya `NavMeshAgent` durumunu stok, checkout ya da müşteri gerçeği saymak; frame hızına bağlı sonuç, sonsuz yürüyüş, duplicate komut ve kayıp ürün riski yaratırdı.

## Karar

- `PSE.Actors`, yalnız `PSE.Core + PSE.Catalog` bağımlılığıyla stable customer/intent/visit kimliklerini ve immutable visit kayıtlarını yönetir; Unity, Inventory, Orders ve Retail referansı taşımaz.
- İlk bounded intent exact ürün kimliği ve açıklanabilir `GraphicsUpgrade` ihtiyacıdır. Rastgele ürün, stok, para veya müşteri kararı üretmez.
- Authoritative zincir `Entering → Browsing → NavigatingToCheckout → AwaitingCheckout → Exiting → Exited` olarak monotoniktir. Her state integer simulation timestamp, deadline, revision ve invariant audit taşır.
- Exact lifecycle ve route-failure komutları bounded immutable receipt ledger'ında saklanır. Terminal state ve ilerlemiş global zaman watermark'ı sonrasında gelen exact tekrar state/revision değiştirmeden başarılı replay olur.
- Route her state için en çok iki kez denenir. Bütçe tükenirse `RouteUnavailable` ile açıklanabilir güvenli çıkış başlar; çıkış rotası da tükenirse terminal/despawn-safe sonuç oluşur.
- Aktif-state timeout'u `PatienceExpired` çıkışına, çıkış timeout'u güvenli terminale gider. Pause sırasında `SimulationClock` ilerlemez; deadline ve NavMesh projection'ı donar.
- `GarageCustomerFlowRuntime`, explicit giriş/RAF A/checkout/çıkış anchor'larını runtime `NavMeshSurface` üzerinde örnekleyen bir `PSE.Presentation` projection'ıdır. Yalnız arrival veya route-failure raporlar; domain intent ya da stok sonucuna karar vermez.
- Offer yayınlanması ziyareti başlatır; basket reservation checkout rotasını, fulfillment ise `Fulfilled` çıkışını tetikler. NPC transformu Inventory/Retail/Orders authority'lerini doğrudan değiştirmez.
- Runtime smoke transient state yarışını başlangıç pause'u ve yalnız tanılama akışındaki geçici düşük ajan hızıyla bounded hale getirir; gerçek browse/checkout/exit rotaları authored hızla tamamlanır.

## Sonuçlar

- Müşteri yaşam döngüsü frame rate, OS saati ve NavMesh transformundan bağımsız olarak exact replay edilebilir.
- Geçmiş timestamp, state atlama/geri dönüş, kimlik çakışması, bilinmeyen ürün ve invalid route raporu no-mutation kalır.
- Route ve patience başarısızlıkları sonsuz loop veya gizli stok/para mutation'ı yerine açıklanabilir terminal sonuç üretir.
- Gerçek keyboard/mouse ve gamepad stok akışı; müşterinin giriş, RAF A, checkout ve fulfillment sonrası çıkış projection'ını doğrular.
- Utility scoring, çoklu ürün/müşteri, derin danışmanlık, ödeme/Economy, Save/Guardian ve final karakter modeli/animasyonu ayrı bounded paketlerdir.

## Kanıt

- Feature commit: `b37b056271fac317e99ec47df0833b8ef219cf83`
- Tree: `cca44dcf50f262e64fa9d6b43b48d25722978f64`
- EditMode: `255/255`
- PlayMode: `18/18`
- Universal macOS build ve Apple M4/Metal runtime: `customer-visit=ready customer-navmesh=ready runtime-route=ok pause=ok fulfilled=ok domain-route-fallback=ok domain-timeout-fallback=ok authority-isolated=ok`
- Repository Guard: [31875039147](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31875039147), başarılı
- Ayrıntı: `Docs/Evidence/DETERMINISTIC-CUSTOMER-VISIT-CHECKPOINT-2026-08-15.md`
