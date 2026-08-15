# ADR-0027 — Stale-Safe Leave Action and Offer-Declined Exit

**Tarih:** 15 Ağustos 2026<br>
**Durum:** Kabul edildi ve Issue #49 ile uygulandı<br>
**Bağlam:** Epic #9 — immutable tek-offer `Leave` kararını authoritative müşteri çıkışına dönüştürme

## Bağlam

Issue #47, `Browsing` ziyaretinin RAF A teklifi için immutable ve açıklanabilir `Buy/Leave` kararı üretmesini; Issue #48 ise `Buy` kararının stale-safe reservation ve checkout navigation eylemine dönüşmesini sağladı. `Leave` yalnız ekranda görünen bir sonuç olarak kaldığında müşteri deterministik biçimde çıkamıyor; Presentation'ın doğrudan Actors geçişi çağırması ise explicit Actors↔Retail binding, current offer revalidation ve historical action receipt sınırlarını atlayabiliyordu.

## Karar

- `CustomerOfferDecisionActionAuthority`, aynı visit başına tek action ledger'ında `Buy` ve `Leave` kayıtlarını kind-discriminated biçimde tutar. `Buy` exact reservation kimlikleri ister; `Leave` hiçbir Basket/Inventory/Checkout kimliği taşımaz.
- `ApplyLeave`, immutable source decision'ı current Actors visit'i ve current ShelfOffer kaydıyla Issue #47 evaluator'ı üzerinden tekrar değerlendirir. Sonuç source decision ile tam value-equal ve `Leave` değilse `retail.offer-action.decision-stale` ile bütün authority'leri değiştirmeden kapanır.
- Actors↔Retail müşteri kimliği immutable `CustomerRetailIdentityBinding` ile explicit eşlenir. String dönüşümü veya Presentation varsayımı authority değildir.
- `PSE.Actors`, `Browsing → Exiting` ve `OfferDeclined` için side-effect-free, revision/owner/observed-time-bound plan üretir. Bu plan yüzeyi `internal`dır ve yalnız `PSE.Retail` ile EditMode test assembly'sine friend olarak açılır; Presentation doğrudan çıkış üretemez.
- Bütün validation ve Actors preflight ilk mutation'dan önce tamamlanır. Başarı yalnız Actors visit revision'ını ve ortak offer-action ledger revision'ını birer kez ilerletir; Inventory, Basket, Checkout, ShelfOffer ve Orders değişmez.
- Exact `Leave` replay stored success döndürür. Aynı ActionId ile farklı payload veya cross-kind replay identity conflict; aynı visit için ikinci ActionId `visit-already-actioned` üretir. Historical receipt müşteri `Exited` olduktan sonra da invariant-safe kalır.
- Actors receipt ledger'ı `BeginOfferDeclinedExit` komutunu `BeginExit/Fulfilled` yolundan ayırır. Exit arrival, bounded route fallback ve exit timeout `OfferDeclined` nedenini korur.
- Garage NavMesh başlangıç kontratı `Browse → Exit` complete path'ini açıkça doğrular. Gerçek `G / Gamepad East`, yalnız exact RAF A ürünü focus altındayken, visit `Browsing` ve gösterilen immutable karar `Leave` iken action authority'yi çağırır.
- Başarı `TEKLİF REDDEDİLDİ • ÇIKIYOR`; stale/preflight failure `AYRILMA ENGELLİ • <stable-code>` olarak renkten bağımsız görünür. Mevcut `Buy`, reservation, checkout ve fulfillment akışları değişmeden korunur.

## Sonuçlar

- Current `Leave`, stok veya kasa kaydı üretmeden müşteriyi güvenli çıkış rotasına gönderir; raftaki serialized item satışa açık kalır.
- Gösterilen karar sonrası offer/visit drift'i eski UI cache'ini commerce veya lifecycle yetkisine dönüştüremez.
- Public Actors bypass ve cross-kind replay kapıları kapalıdır; action ledger ile visit lifecycle birbirinden habersiz ilerleyemez.
- Büyük world/status metinleri graybox kabul kanıtıdır; final production UI, karakter modeli, animasyon, sanat veya ses değildir.

## Bilinçli kapsam dışı

- Ödeme yöntemi, `PSE.Economy` ledger, nakit, gelir, COGS, vergi, indirim ve fiş/fatura.
- Çoklu customer/offer/product, alternatif item seçimi, ranking, utility scoring, danışmanlık ve memnuniyet/itibar.
- Save/journal/migration/recovery, Guardian, final UI/model/animasyon/ses ve gerçek Windows doğrulaması.

## Kanıt

- Feature commit: `67d858aff773610cff6d6c221c792cd793f27a1b`
- Tree: `dc76a89a5a9f0f9349509aca7374f30518b1c308`
- EditMode: `298/298`
- PlayMode: `22/22`
- Universal macOS build ve Apple M4/Metal runtime: `garage-leave-action-r18-v1`, `customer-leave-action=ready`, `leave-action=ok`, `stale-leave-blocked=ok`, `authority-isolated=ok`
- Repository Guard: [31882228394](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31882228394), başarılı
- Source/docs checkpoint `868885af9065d4e9fb274c3862fd525b040e1cc2`; Repository Guard [31882508496](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31882508496), başarılı
- Ayrıntı: `Docs/Evidence/STALE-SAFE-LEAVE-ACTION-AND-OFFER-DECLINED-EXIT-CHECKPOINT-2026-08-15.md`
