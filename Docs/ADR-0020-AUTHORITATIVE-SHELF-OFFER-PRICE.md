# ADR-0020 — Authoritative Shelf Offer ve Integer Fiyat

**Tarih:** 15 Ağustos 2026
**Durum:** Kabul edildi ve Issue #42 ile uygulandı
**Bağlam:** Epic #8 — RAF A fiyatlandırma dilimi

## Bağlam

Issue #41 sonunda aynı serialized ürün sipariş, teslimat, koli açma, oyuncu eli ve RAF A arasında authoritative Inventory kimliğiyle taşınıyordu. Ancak raftaki görünen metin fiyat gerçeği değildi. Fiyatı `TextMesh`, Unity component'i, fiziksel item veya Inventory quantity üzerinde tutmak; satış anında ayrışan değer, float yuvarlama, duplicate raf teklifi ve başarısız komutta kısmi mutation riski yaratacaktı.

## Karar

- `PSE.Retail`, Unity/Editor referansı taşımayan ayrı domain assembly'sidir; yalnız `PSE.Core`, `PSE.Catalog` ve `PSE.Inventory` yönünde bağımlıdır.
- `ShelfOfferAuthority` raf/ürün teklifinin tek authoritative sahibidir. Inventory yalnız container'ın mevcut ve `Shelf` türünde olduğunu doğrular; fiyat state'i veya revision'ı taşımaz.
- Her teklif stable `ShelfOfferId`, exact `ProductDefinitionId` ve exact shelf `ContainerId` ile bağlıdır. Aynı shelf+product çifti için yalnız bir teklif bulunabilir.
- Para birimi ilk sözleşmede tam üç büyük ASCII harften oluşur. Fiyat float/double değildir; iki ondalıklı pozitif `long` minor-unit olarak tutulur ve `999.999.999` minor-unit üst sınırı vardır.
- Exact aynı `SetOffer` komutu idempotent başarıdır ve revision değiştirmez. Aynı kimliğin fiyat güncellemesi authority ve offer revision'ını tam bir kez artırır.
- Geçersiz kimlik/para/fiyat, bilinmeyen ürün/raf, shelf olmayan container, duplicate shelf+product veya identity conflict state ve revision'ı değiştirmeden başarısız olur.
- Domain teklifi boş rafta da yaşayabilir; stok adedi ile fiyat politikası birbirine bağlanmaz. İlk fiziksel Presentation akışında ise oyuncu publish eylemini yalnız exact ürün authoritative RAF A container'ındayken yapabilir.
- RAF A ürünü fiyatlanmamışsa `E / Gamepad South` etkin binding prompt'u `549,99 EUR` teklifini kasıtlı olarak yayınlar. Başarıdan önce dünya etiketi `FİYAT YOK`, başarıdan sonra `549,99 EUR` gösterir.
- Publish sırasında Inventory ve Orders revision/quantity değişmez. Satış transaction snapshot'ı, sepet, rezervasyon tüketimi, vergi, indirim, iade ve ledger ayrı atomik paketlerdir.
- Bu ilk görünür dilim sabit prototip fiyatının kasıtlı publish eylemini kanıtlar. Oyuncunun sayısal fiyat düzenleme UI'si aynı `SetOffer` update sözleşmesini kullanacak sonraki sunum işidir.

## Sonuçlar

- Raf etiketi artık Unity metninden değil başarılı domain komutundan türetilir.
- Fiyat ve stok authority'leri ayrıdır; fiyat yayınlamak item yaratmaz, taşımaz veya sipariş durumunu değiştirmez.
- Integer minor-unit sözleşmesi ilk checkout/snapshot paketine yuvarlama güvenli bir giriş sağlar.
- Keyboard ve gamepad ile fiziksel `koli → el → raf → fiyat` zinciri aynı item kimliği üzerinde görünür biçimde çalışır.
- Dinamik piyasa, müşteri kararı ve para muhasebesi bu kararla erken veya gizli biçimde sisteme sokulmaz.

## Kanıt

- Feature commit: `7a23cd92be6ff1169ff49530319b0759965cadf5`
- EditMode: `207/207`
- PlayMode: `17/17`
- Universal macOS build ve Apple M4/Metal runtime: `shelf-offer=ok price-minor=54999 currency=EUR stable=ok quantity=1`
- Repository Guard: [31866681324](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31866681324), başarılı
- Ayrıntı: `Docs/Evidence/AUTHORITATIVE-SHELF-OFFER-CHECKPOINT-2026-08-15.md`
