# ADR-0025 — Explainable Single-Offer Customer Decision

**Tarih:** 15 Ağustos 2026<br>
**Durum:** Kabul edildi ve Issue #47 ile uygulandı<br>
**Bağlam:** Epic #9 — Browsing ziyaretinin tek immutable raf teklifi için saf `Buy/Leave` değerlendirmesi

## Bağlam

Issue #46 müşterinin intent/visit lifecycle'ını ve NavMesh projection sınırını kurdu; ancak `Browsing` durumundaki müşterinin tek bir raf teklifini neden kabul ettiği veya reddettiği için deterministic bir domain sözleşmesi yoktu. Kararı Presentation, frame zamanı, gizli RNG, mutable authority sorguları ya da doğrudan basket/checkout eylemi içinde hesaplamak; açıklanabilirliği, replay'i ve no-mutation sınırını bozardı.

## Karar

- Bağımlılık tek yönlü `PSE.Retail → PSE.Actors` olur. `PSE.Actors`, yalnız `PSE.Core + PSE.Catalog` sınırında kalır; dairesel referans kurulmaz.
- `CustomerOfferDecisionEvaluator`, yalnız immutable `CustomerVisitRecord`, `ShelfOfferRecord` ve doğrulanmış integer minor-unit `ShelfPrice maximumAcceptedPrice` tüketen static ve stateless bir saf fonksiyondur.
- Yalnız `Browsing` durumundaki `GraphicsUpgrade` ihtiyacı değerlendirilebilir. Null/default/yapısal bozuk girdi, yanlış visit state, desteklenmeyen need ve currency uyumsuzluğu stable failure code üretir.
- Karar sırası sabittir: currency doğrulamasından sonra product mismatch `Leave`; exact product fakat limit üstü fiyat `Leave`; exact product ve limite eşit/altı fiyat `Buy` üretir. Geçerli `Leave`, operation failure değildir.
- Sonuç immutable ve value-equal'dır. Customer/visit/intent kimliği, source visit timestamp/state, need, intent/offer product kimliği, offer ID/revision/shelf/price, accepted limit, decision ve reason code provenance olarak kopyalanır.
- Exact input replay'i farklı object referansı olsa da value-equal sonuç verir. Eski offer veya eski Browsing visit snapshot'ı kendi tarihsel kararını replay edebilir; bu sonuç güncel commerce/lifecycle action yetkisi değildir.
- Evaluator authority, Inventory, Orders, Basket, Checkout, clock, RNG, NavMesh, cache, revision, receipt veya journal kabul etmez ve hiçbir mutation yapmaz.
- Garage presentation, yalnız `Browsing` sırasında kısa `KARAR: SATIN AL / AYRIL` metni ve stable reason code gösterir. Karar okumak reservation, checkout navigation, exit veya başka visit transition'ı başlatmaz.

## Sonuçlar

- Tek teklif kararı frame rate, OS zamanı, RNG ve Unity nesnesinden bağımsız olarak deterministic ve açıklanabilirdir.
- Equality/hash bütün saklanan provenance alanlarını kapsar; aynı outcome taşıyan farklı limit/snapshot değerleri yanlışlıkla eşit sayılmaz.
- Klavye/fare ve gamepad Garage akışları kararı görünür kılarken Actors, Inventory, Orders, ShelfOffer, Basket ve Checkout revision/count değerlerini sabit tutar.
- Mevcut büyük world/status metni graybox kabul kanıtıdır; final production UI, karakter modeli veya animasyonu değildir.
- Basket action, stale snapshot revalidation, Actors↔Retail customer kimlik köprüsü, çoklu teklif/müşteri, ödeme/Economy, Save ve utility scoring sonraki bounded paketlerdir.

## Kanıt

- Feature commit: `f97ded34f00e0d0637fbf9b41c0c0d33a7969b8e`
- Tree: `e8cddbc13166b35a081786fed895417cf6270c16`
- EditMode: `267/267`
- PlayMode: `18/18`
- Universal macOS build ve Apple M4/Metal runtime: `garage-offer-decision-r16-v1`, `offer-decision=ok`, `authority-isolated=ok`
- Repository Guard: [31876993251](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31876993251), başarılı
- Ayrıntı: `Docs/Evidence/EXPLAINABLE-SINGLE-OFFER-CUSTOMER-DECISION-CHECKPOINT-2026-08-15.md`
