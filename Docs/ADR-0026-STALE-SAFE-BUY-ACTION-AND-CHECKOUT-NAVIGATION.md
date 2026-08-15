# ADR-0026 — Stale-Safe Buy Action and Checkout Navigation

**Tarih:** 15 Ağustos 2026<br>
**Durum:** Kabul edildi ve Issue #48 ile uygulandı<br>
**Bağlam:** Epic #9 — immutable tek-offer `Buy` kararını authoritative reservation ve müşteri lifecycle eylemine dönüştürme

## Bağlam

Issue #47, `Browsing` ziyaretinin tek RAF A teklifi için immutable ve açıklanabilir `Buy/Leave` kararı üretti. Bu karar tarihsel provenance taşısa da kendi başına güncel commerce veya lifecycle yetkisi değildi. Presentation katmanının eski bir kararı doğrudan Basket/Inventory ve Actors komutlarına çevirmesi; fiyat değişimi, visit ilerlemesi, yanlış müşteri kimliği veya kısmi preflight hatasında reservation ile müşteri durumunu ayırabilirdi.

## Karar

- `PSE.Retail` içindeki `CustomerOfferDecisionActionAuthority`, yalnız `Buy` dalının write sınırıdır. `Leave` bu pakette uygulanmaz ve `retail.offer-action.kind-not-buy` ile no-mutation reddedilir.
- Actors `CustomerIdScope` ile Retail `RetailCustomerIdScope`, immutable/equatable `CustomerRetailIdentityBinding` üzerinden explicit bağlanır. String cast, isim benzerliği veya Presentation varsayımı kimlik yetkisi sayılmaz.
- Her yeni action, current visit ve current shelf offer kayıtlarını authority'lerden yeniden okur; Issue #47 evaluator'ını source accepted-price ile yeniden çalıştırır. Current sonuç source decision ile tam value-equal değilse `retail.offer-action.decision-stale` döner.
- Caller exact action, line, basket, serialized item, reservation ve claim kimliklerini verir. Authority alternatif item aramaz, skorlamaz ve RNG kullanmaz.
- Inventory serialized reservation, Retail Basket line ve Actors checkout-navigation için side-effect-free, authority/revision-bound prepared planlar kullanılır. Bütün preflight'lar ilk mutation'dan önce tamamlanır.
- `ApplyBuy` tek process ve tek senkron çağrı sınırında Basket/Inventory planını, ardından bağımsız Actors planını commit eder. Basket commit'i Actors state'ini değiştirmediği için hazırlanmış Actors planını bu çağrı içinde stale yapacak bir ara mutation yoktur; beklenmeyen ikinci commit failure invariant ihlalidir.
- Basket ile action evaluator aynı exact `ShelfOfferAuthority` örneğine bağlı olmak zorundadır. Ayrışmış offer authority kompozisyonu creation aşamasında fail-closed reddedilir.
- Action-owned Basket hazırlama yüzeyi `internal`dır. Inventory reservation `ConsumeOnly` policy taşır; public release ve public tekli/toplu consume yolları mutation öncesi reddeder. Yalnız Retail checkout'un internal fulfillment sınırı bu reservation'ı tüketebilir.
- Inventory, Basket ve Actors prepared planlarında exact immutable record/receipt reference replay'i revision kontrolünden önce tanınır. Exact action replay stored success döndürür; aynı ActionId ile farklı payload identity conflict üretir; aynı visit için ikinci ActionId reddedilir.
- Action receipt historical kayıttır. Fulfillment reservation/line'ı tüketip visit'i ilerlettikten sonra canlı alt kayıtlara bağımlı olmadan invariant-safe kalır.
- Garage'da gerçek `G / Gamepad East`, yalnız görünür current `Buy` bağlamında action authority'yi çağırır. Başarı ve stale/preflight failure stable metinle gösterilir; renk tek bilgi kanalı değildir.
- Ayrı public action-level `PrepareBuy/CommitPreparedBuy` API'si eklenmez. Mevcut bounded senkron command, alt authority planlarını ilk committen önce hazırladığı için yeni public plan yüzeyi ek atomiklik sağlamadan dış stale/replay yüzeyini büyütürdü.

## Sonuçlar

- Stale visit/offer, mapping uyuşmazlığı ve bütün reservation/navigation preflight failure yolları action receipt, Basket, Inventory ve Actors state'ini değiştirmez.
- Başarılı `Buy`; exact action receipt, action-owned Basket reservation ve `Browsing → NavigatingToCheckout` geçişini birer revision ile üretir. Offer, Orders ve Checkout bu aşamada değişmez.
- Legacy `G` toggle veya doğrudan Inventory API'si müşteriyi reservation'sız kasaya gönderemez; fulfillment mevcut checkout snapshot/consume sınırından ilerler.
- Presentation, gösterdiği immutable kararı action anına kadar saklayabilir; authority eski kararı current snapshotla yeniden doğruladığı için UI cache commerce yetkisi olmaz.
- Büyük world/status metinleri graybox kabul kanıtıdır; final production UI, model, animasyon, sanat veya ses değildir.

## Bilinçli kapsam dışı

- `Leave` action, `OfferDeclined` exit reason ve `Browsing → Exiting` lifecycle.
- Checkout başlatma, ödeme, Economy ledger/nakit/COGS/vergi/indirim ve fiş/fatura.
- Çoklu customer/offer, alternatif item seçimi, ranking, utility scoring veya RNG.
- Save/journal/migration/recovery, Guardian, final UI/model/animasyon/ses ve gerçek Windows doğrulaması.

## Kanıt

- Feature commit: `6951869c4a9f33662f322c02348fa4282b9cdbb6`
- Tree: `5f4c956423bbc07b9087d47f7886ab36cc6992f1`
- EditMode: `287/287`
- PlayMode: `19/19`
- Universal macOS build ve Apple M4/Metal runtime: `garage-buy-action-r17-v1`, `buy-action=ok`, `stale-blocked=ok`, `authority-isolated=ok`
- Repository Guard: [31880394269](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31880394269), başarılı
- Ayrıntı: `Docs/Evidence/STALE-SAFE-BUY-ACTION-AND-CHECKOUT-NAVIGATION-CHECKPOINT-2026-08-15.md`
