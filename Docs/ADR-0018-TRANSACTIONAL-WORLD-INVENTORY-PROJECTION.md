# ADR-0018 — Transactional World/Inventory Projection

**Tarih:** 15 Ağustos 2026  
**Durum:** Kabul edildi ve Issue #40 ile uygulandı  
**Bağlam:** Epic #8 — sipariş, teslimat ve raf döngüsü

## Bağlam

`PSE.Orders` ve `PSE.Inventory` teslimatın mantıksal gerçeğini taşırken GarageGraybox'taki kutular yalnız fiziksel dünya nesneleriydi. Bu iki katmanın birbirinden bağımsız değişmesi; rafta görünen fakat stokta bulunmayan ürün, stokta bulunan fakat dünyada kaybolmuş ürün veya başarısız fizik hareketinden sonra çift sahiplik oluşturabilirdi.

## Karar

- Dünya nesnesi stok miktarı authority'si değildir. Authoritative konum `InventoryContainerKind` ve container kimliğiyle belirlenir; Unity nesnesi bunun görünür projeksiyonudur.
- `PSE.Presentation` yüksek seviyeli ve açık bir adaptör olarak Catalog, Inventory, Orders ve World sınırlarını koordine eder. Alt domain assembly'leri Unity veya Presentation'a bağımlı olmaz.
- Garage prototype'u tek, serialized Northstar A60 item kimliği kullanır. İlk `E / Gamepad South` arrived teslimatı atomik olarak Receiving'e kabul eder; ikinci etkileşim Receiving → ActorHands transferini yapar ve ancak sonra fiziksel pickup gerçekleşir.
- Yerleştirme çözümü seçtiği gerçek `PlacementSurface` bilgisini taşır. Yalnız `InventoryPlacementZone` ile açıkça eşlenmiş geçerli yüzeyler authoritative container hedefi olabilir.
- RAF A placement'ında önce ActorHands → Shelf domain transferi yapılır, sonra fiziksel world mutation uygulanır. Güvenli bırakmada hedef `WorldFloor` container'ıdır.
- Domain transferi başarısızsa fiziksel sahiplik değişmez. Domain transferinden sonra fiziksel mutation başarısızsa transfer önceki container'a geri alınır; işlem fail-closed kalır.
- Recovery hem authoritative container'ı hem de görünür nesneyi son güvenli dünya container/pose durumuna döndürür.
- `InventoryContainerKind.WorldFloor` kalıcı enum değeri `9` ile eklenmiştir; mevcut değerler yeniden numaralanmaz.
- Prototype composition deterministik sahne kanıtıdır; save, ekonomi, fiyat, müşteri satışı veya çok satırlı koli açma authority'si değildir.

## Sonuçlar

- Receiving → ActorHands → Shelf/WorldFloor zincirinde aynı item kimliği korunur.
- Başarısız capacity/domain/world işlemleri stok ve fiziksel nesneyi ayrıştırmaz.
- Mevcut bağımsız pickup/drop/placement/stack/cart prototipleri binding yoksa aynı davranışı korur.
- Gerçek teslimat kolisi açma, çoklu manifest satırı projeksiyonu, fiyatlandırma ve satış daha küçük sonraki Epic #8 paketlerinde uygulanacaktır.

## Kanıt

- Feature commit: `9d75573a86e395d2fa74f3808d43310e4d65f760`
- EditMode: `188/188`
- PlayMode: `17/17`
- Universal macOS build ve Apple M4/Metal runtime smoke: `stock-flow=ok accepted=ok carry=ok world-floor=ok stable=ok quantity=1`
- Ayrıntı: `Docs/Evidence/AUTHORITATIVE-STOCK-FLOW-CHECKPOINT-2026-08-15.md`
