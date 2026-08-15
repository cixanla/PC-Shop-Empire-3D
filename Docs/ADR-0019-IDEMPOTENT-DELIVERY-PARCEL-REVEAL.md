# ADR-0019 — Idempotent Delivery Parcel Reveal

**Tarih:** 15 Ağustos 2026  
**Durum:** Kabul edildi ve Issue #41 ile uygulandı  
**Bağlam:** Epic #8 — fiziksel teslimat kolisi açma

## Bağlam

Issue #40 teslimatı authoritative Receiving container'ına kabul edip aynı serialized item'ı ellere ve rafa taşıdı; ancak dış kargo kolisi ile satılabilir ürün kutusu aynı görünür nesneydi. Koli açma eylemi eklenirken Inventory quantity/revision'ının ikinci kez değişmesi, tekrar açmada duplicate ürün oluşması veya hatalı manifest/binding durumunda ürünün görünür hâle gelmesi engellenmeliydi.

## Karar

- `DeliveryParcelProjection` yalnız Presentation/world görünüm durumudur: `Sealed` veya `Opened`. Inventory, Orders veya ekonomik authority değildir.
- Manifest item kimliğini arrival aşamasında sabitler; acceptance aynı item kaydını Receiving'e oluşturur. Opening yeni item yaratmaz, quantity veya domain revision değiştirmez.
- Parcel yalnız order `Accepted`, exact manifest tek beklenen serialized item'ı taşıyor ve aynı Inventory item Receiving container'ındaysa açılabilir.
- Açma idempotenttir. Aynı parcel'a tekrar open komutu başarı döndürür fakat ikinci transition, world item veya stok mutation üretmez.
- Geçersiz order state, binding identity, manifest veya container konumu parcel'ı kapalı bırakır ve no-mutation sonucu verir.
- Dış kapalı koli görseli açılırken gizlenir; exact ürün kutusu ve Receiving'de kalan açık dış kabuk görünür olur. Ürün elde/rafa taşındığında kabuk dünyada kalır.
- `PlayerCarryController` aynı `Interact` eylemini duruma göre acceptance → unpack → pickup olarak sıralar; klavye ve gamepad etkin binding promptları her adımı gösterir.
- `PhysicalItemProjection` pickup collider sözleşmesi mutually-exclusive görseller için yalnız aktif ve enabled collider setini doğrular; inaktif görsel collider'ları fizik sözleşmesini yanlış biçimde bozmaz.
- İlk dilim tek serialized item içindir. Çoklu line/quantity fiziksel unpack layout'u, hasarlı/eksik claim, fiyat ve satış ayrı paketlerdir.

## Sonuçlar

- Oyuncu dış teslimat kolisini gerçek ayrı bir adımda açar ve iç ürünün görünür olduğunu görür.
- Acceptance sonrası stok `1` kalırken opening sırasında Inventory ve Orders revision'ı değişmez.
- Repeated open duplicate üretmez; exact item ID pickup, drop ve shelf boyunca korunur.
- Açılmamış ürün alınamaz; invalid state/binding/location fail-closed kalır.

## Kanıt

- Feature commit: `3766f3f06df624093f4774ef8fa4e7f1286d1c01`
- EditMode: `192/192`
- PlayMode: `17/17`
- Universal macOS build ve Apple M4/Metal runtime: `accepted=ok parcel-open=ok carry=ok world-floor=ok stable=ok quantity=1`
- Ayrıntı: `Docs/Evidence/DELIVERY-PARCEL-UNPACKING-CHECKPOINT-2026-08-15.md`
