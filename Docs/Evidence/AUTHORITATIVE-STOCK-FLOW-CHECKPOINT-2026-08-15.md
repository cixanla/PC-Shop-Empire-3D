# Authoritative Stock Flow Checkpoint — 15 Ağustos 2026

## Kapsam

Issue [#40](https://github.com/cixanla/PC-Shop-Empire-3D/issues/40), Epic [#8](https://github.com/cixanla/PC-Shop-Empire-3D/issues/8) altında ilk görünür ve authoritative teslimat dilimini tamamlar:

1. Tek exact serialized ürün `Arrived` durumunda görünür teslimat alanında başlar; stok miktarı `0`dır.
2. İlk `E / Gamepad South` teslimatı kabul eder ve ürünü authoritative Receiving container'ına ekler.
3. İkinci `E / Gamepad South`, aynı item kimliğini Receiving → ActorHands taşır ve fiziksel kutuyu ele alır.
4. RAF A üzerindeki geçerli placement, ActorHands → Shelf transferini fiziksel yerleştirmeden önce tamamlar.
5. `G / Gamepad East` güvenli bırakma, ActorHands → WorldFloor yapar; recovery son güvenli container ve pozu geri kurar.

Garajda teslimat alanı, teslimat kutusu, durum panosu/ışığı ve işaretli RAF A görünürdür. HUD sipariş ve authoritative konum durumunu dinamik gösterir. Önceki pickup/drop, placement, rotation, stacking ve cart akışları korunur.

## Kaynak checkpoint'i

- Branch: `main`
- Feature commit: `9d75573a86e395d2fa74f3808d43310e4d65f760`
- Tree: `6779e31aaa6ad186acfa3b1143653d51f47e75b7`
- Marker: `garage-authoritative-stock-flow-r9-v1`
- Repository Guard: [31864259779](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31864259779), başarılı

## Otomatik doğrulama

Ham çıktılar Git dışındaki `/Users/cixanla/Developer/PCShopEmpire3D/TestResults` klasöründedir.

| Kanıt | Sonuç | SHA-256 |
|---|---|---|
| `stock-flow-editmode-final.xml` | 188/188 geçti; failed/skipped 0 | `a68f13ed6ad0acded29bbd2bb9f7d2022d33640d8e4d139eec3ac9dea6b61932` |
| `stock-flow-playmode-final.xml` | 17/17 geçti; failed/skipped 0 | `199a022b9e45556c23e7c2e6bcf5d68fdc5102c832c78445f10644522aff81b4` |
| `stock-flow-macos-build.log` | Universal development build; 327.462.869 bayt | `0cedd45fb8bf69cba64f2e2f991f42c1d2b009060bcfb6dc47d91dd150b39ab9` |
| Player executable | Mach-O `x86_64 + arm64` | `53cf8ab1e929fc0aace8f5eedbc4314ad29a05f33cf83690ab56c6406e71a642` |
| `stock-flow-macos-runtime.log` | Apple M4/Metal, 1280×720, smoke başarılı | `83991ad781087259a0426ead6eb9a2dd65c6764c7d170db4382b9817108d0c0b` |

PlayMode kapsamı gerçek Input System device state kullanır:

- Klavye ile Arrived → Accepted/Receiving → ActorHands → RAF A/Shelf tam akışı.
- Gamepad South → South → East ile Receiving → ActorHands → WorldFloor akışı.
- Dolu ActorHands container failure'ında fiziksel pickup yapılmaması; order item'ının Receiving'de ve dünyada kalması.

Runtime işaretleri:

```text
GARAGE_GRAYBOX_RUNTIME_READY version=garage-authoritative-stock-flow-r9-v1 inventory-flow=arrived
GARAGE_STOCK_FLOW_RUNTIME_SMOKE stock-flow=ok accepted=ok carry=ok world-floor=ok stable=ok quantity=1
```

macOS oturumu kilitli olduğu için bu turda oynanabilir pencerenin güvenilir ekran görüntüsü alınamadı; bu durum test, build ve native player log doğrulamasını etkilemedi. Görsel kanıt iddiası yapılmamıştır.

## Fail-closed sözleşmeleri

- Inventory transferi başarısızsa world ownership değişmez.
- Transfer başarılı, fiziksel mutation başarısızsa Inventory önceki container'a rollback edilir.
- Bound item dünyada bağımsız biçimde alınamaz; binding bulunmayan eski prototype item davranışı değişmez.
- Recovery authoritative container ile fiziksel pozu birlikte son güvenli dünya durumuna taşır.
- Dünya nesnesi miktar kaynağı değildir; quantity Inventory audit'inden gelir.

## Bilinçli kapsam dışı

- Çok satırlı teslimat kolisini açma ve manifestten birden çok dünya birimi çıkarma.
- Fiyat/para/ledger, müşteri seçimi, checkout ve satış.
- Save/journal/crash atomikliği.
- Final sanat, gelişmiş el animasyonu ve ses.
- Gerçek Windows x64 IL2CPP/DirectX/Steam doğrulaması.

## Uzak ve USB kapanışı

- Feature Repository Guard [31864259779](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31864259779): başarılı.
- Docs checkpoint commit `f20fd1741a7d51b1350e7c1e2785e72c0718be84`; Repository Guard [31864541173](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31864541173): başarılı.
- Issue #40 completed olarak kapatıldı ve Roadmap item'ı Done yapıldı; Epic #8 In Progress kaldı.
- USB: `2026-08-15_STAGE_B_AUTHORITATIVE_STOCK_FLOW`; 467 tracked source + 4 evidence + source kaydı, 472 manifest satırı.
- Manifest SHA-256: `5521f869703d1ec480912f21fb70e21fdf0b235f7c15e4be65431e1fc0ae22a3`.
- Tam readback/hash/boyut ve Git source karşılaştırmasında mismatch `0`; forbidden directory, credential filename ve AppleDouble `0`.
