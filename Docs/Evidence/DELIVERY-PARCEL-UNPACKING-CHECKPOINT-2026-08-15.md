# Delivery Parcel Unpacking Checkpoint — 15 Ağustos 2026

## Görünür sonuç

Issue [#41](https://github.com/cixanla/PC-Shop-Empire-3D/issues/41), Epic [#8](https://github.com/cixanla/PC-Shop-Empire-3D/issues/8) altında fiziksel koli açma dilimini tamamlar:

1. Büyük dış teslimat kolisi kapalı, bantlı ve manifest etiketli görünür.
2. İlk `E / Gamepad South` mevcut atomik acceptance kapısından aynı exact item'ı Receiving'e alır; koli kapalı kalır.
3. İkinci `E / Gamepad South` exact manifest/container sözleşmesini doğrular, dış koliyi tek sefer açar ve Northstar A60 perakende kutusunu görünür yapar.
4. Açık dış koli kabuğu Receiving'de kalır; Inventory quantity/revision değişmez ve tekrar open duplicate üretmez.
5. Üçüncü `E / Gamepad South` aynı item'ı Receiving → ActorHands taşır; mevcut shelf/drop akışları devam eder.

HUD ve dünya panosu `KOLİ: KAPALI` / `AÇILDI • ÜRÜN HAZIR` durumunu; etkileşim prompt'u acceptance, açma ve pickup eylemlerini etkin klavye/gamepad binding'iyle gösterir.

## Kaynak checkpoint'i

- Branch: `main`
- Feature commit: `3766f3f06df624093f4774ef8fa4e7f1286d1c01`
- Tree: `3b03406dc9e9d6cd9323261664735900fe6b1f83`
- Marker: `garage-delivery-unpacking-r10-v1`
- Repository Guard: [31865403562](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31865403562), başarılı

## Otomatik doğrulama

Ham kanıtlar `/Users/cixanla/Developer/PCShopEmpire3D/TestResults` altındadır.

| Kanıt | Sonuç | SHA-256 |
|---|---|---|
| `parcel-scene-build.log` | GarageGraybox r10 üretildi | `334e5837a2600e78435a7b31d97e584ea2be01ec946cf94d683b69933039117c` |
| `parcel-editmode-final.xml` | 192/192 geçti; failed/skipped 0 | `e21ab3813bd3630e48c356941d9e438fe3f7ec88f3b5ea2491b6f8bfeff2b2c1` |
| `parcel-playmode-final.xml` | 17/17 geçti; failed/skipped 0 | `df13a3f83434f33307c082dc1b1c488c776a2bff911a07a7c032f2719b062a4c` |
| `parcel-macos-build.log` | Universal development build; 327.475.393 bayt | `bb44ef92621554c4391cff38d12387f740a2254646ddebd46c8f1055690971b0` |
| Player executable | Mach-O `x86_64 + arm64` | `8d011cb9ede0fb847fdd1fa3696e94390b726dc4388c94ba47bc85de666c0da3` |
| `parcel-macos-runtime-1.log` | Apple M4/Metal, 1280×720, parcel smoke başarılı | `eeb1fd8cf84bc92b5e1caef3f6a435c3badad5b68b08630a8557ced6c91ba6f7` |

Runtime işaretleri:

```text
GARAGE_GRAYBOX_RUNTIME_READY version=garage-delivery-unpacking-r10-v1 inventory-flow=arrived parcel=sealed lookdev=ok
GARAGE_STOCK_FLOW_RUNTIME_SMOKE stock-flow=ok accepted=ok parcel-open=ok carry=ok world-floor=ok stable=ok quantity=1
```

Gerçek Input System PlayMode kapsamı:

- Klavye: acceptance → open → exact pickup → RAF A placement.
- Gamepad South: acceptance → open → exact pickup; Gamepad East: güvenli WorldFloor drop.
- ActorHands doluyken domain failure fiziksel pickup yapmaz.
- Opening öncesi, identity mismatch ve Receiving dışında item durumlarında no-reveal/no-mutation.
- Repeated open transition count `1`; Inventory ve Orders revision opening boyunca sabit.

macOS oturumu kilitli olduğu için yeni pencere ekran görüntüsü alınmadı; sahne yapısı, gerçek Input System testleri, Universal build ve native runtime logu başarılıdır. Görsel ekran görüntüsü iddiası yapılmamıştır.

## Bilinçli kapsam dışı

- Çok satırlı veya çok adetli parcel içeriğini fiziksel düzende çıkarma.
- Eksik/hasarlı teslimat ve claim/partial acceptance.
- Fiyat/etiket authority'si, para/ledger, müşteri ve checkout/satış.
- Save/journal/crash atomikliği, final model/animasyon/ses.
- Gerçek Windows x64 IL2CPP/DirectX/Steam doğrulaması.
