# ADR-0021 — Customer Basket Serialized Item Reservation

**Tarih:** 15 Ağustos 2026
**Durum:** Kabul edildi ve Issue #43 ile uygulandı
**Bağlam:** Epic #8 — raftaki exact ürünün müşteri talebi için ayrılması

## Bağlam

Issue #42 sonunda RAF A'daki Northstar A60 ürünü stable offer/product/shelf kimliği ve integer minor-unit fiyatla yayınlanabiliyordu. Ancak görünür bir müşteri talebi authoritative stok üzerinde hak oluşturmuyordu. Sepet durumunu yalnız Unity etiketi veya fiziksel nesne üzerinde tutmak; aynı serialized item'ın iki müşteriye ayrılması, raftan taşınmasına rağmen satılabilir görünmesi ve Retail ile Inventory revision'larının kısmi değişmesi riskini yaratacaktı.

## Karar

- `RetailBasketAuthority`, Unity/Editor bağımlılığı taşımayan `PSE.Retail` içinde stable customer, basket ve basket-line kimliklerini yönetir.
- Her basket satırı exact shelf offer, exact serialized `ItemInstanceId`, exact Inventory reservation ve claim kimliğine bağlanır.
- Rezervasyon yalnız item offer ürünüyle eşleşiyor ve offer'ın exact Shelf container'ında duruyorsa başlar. Unknown offer/item, product/shelf mismatch, duplicate item, reservation conflict ve basket/customer conflict mutation üretmez.
- Retail bütün preflight kontrollerini Inventory mutation'ından önce tamamlar. Başarılı reserve Retail ile Inventory revision'ını birer kez artırır; exact tekrar yalnız cross-authority kayıt hâlâ tutarlıysa idempotent başarıdır.
- Inventory reservation exact serialized item'ı unavailable yapar fakat total quantity'yi değiştirmez. Release iki authority'yi birer kez ilerletir ve item'ı yeniden available yapar.
- Basket satırı fiyat kopyalamaz. Offer fiyatının checkout başlangıcında immutable transaction snapshot'a alınması sonraki atomik sınırdır; raf fiyatındaki sonradan değişiklik açık basket satırını sessizce yeniden yazmaz.
- İlk görünür kanıtta ürün RAF A üzerinde kalır. `G / Gamepad East` demo müşteri için ayırır ve aynı binding rezervasyonu kaldırır; etiket/pano `MÜŞTERİ: 1 ÜRÜN • AYRILDI` durumunu authority'den türetir.
- Ayrılmış item'a `E / Gamepad South` ile pickup isteği fail-closed olur. Inventory transfer API'sinin rezervasyonu taşıyabilen genel davranışı değiştirilmez; bu bounded shelf presentation adaptörü müşteri claim'ini açıkça korur.
- Fiziksel müşteri AI, ürünün gerçek sepete taşınması, checkout/ödeme, transaction price snapshot, vergi/indirim, Economy ledger, Save ve final UI bu karara dahil değildir.

## Sonuçlar

- Aynı serialized ürün aynı anda iki müşteri/sepet talebine ayrılamaz.
- Dünya etiketi, prompt, available quantity ve Retail basket satırı tek authoritative sonucu yansıtır.
- Başarısız komutlarda Retail ve Inventory birlikte sabit kalır; dış mutation kaynaklı drift exact tekrar/release sırasında sessiz başarıya dönüştürülmez.
- Önceki acceptance, parcel open, pickup/drop, placement, stacking, cart ve recovery sözleşmeleri korunur.
- Checkout paketi artık stable basket line + exact offer + Inventory reservation üzerinde kurulabilir; fiyat snapshot'ı ve satış tüketimi henüz yapılmış sayılmaz.

## Kanıt

- Feature commit: `45c2cdc4f4f437824567c7e7cb5b6fcea1ecb4ce`
- EditMode: `220/220`
- PlayMode: `17/17`
- Universal macOS build ve Apple M4/Metal runtime: `basket-reservation=ok release=ok`
- Repository Guard: [31867913964](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31867913964), başarılı
- Ayrıntı: `Docs/Evidence/CUSTOMER-BASKET-RESERVATION-CHECKPOINT-2026-08-15.md`
