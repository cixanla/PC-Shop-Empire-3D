# Küçük Kutu İstifleme — Doğrulama Kanıtı

**Tarih:** 15 Ağustos 2026
**Epic / issue:** [#6](https://github.com/cixanla/PC-Shop-Empire-3D/issues/6) / [#35](https://github.com/cixanla/PC-Shop-Empire-3D/issues/35)
**Feature commit:** `2e11e30a1a4b3435046ae18001004cacc170079e`
**Feature tree:** `e2cb49e318ea84b4a8db08ab3dd79d9b833b2483`

## Görünür sonuç

GarageGraybox artık iki stable kimlikli küçük kutu içerir. Oyuncu ilk kutuyu `E / Gamepad South` ile alır, `Mouse Left / Gamepad RT` ile placement önizlemesini açar ve `G / Gamepad East` ile ikinci kutunun üstüne yerleştirir. Geçerli hedef `İSTİF GEÇERLİ` yazar. Merkez/90° snap, tam taban desteği, overlap engeli, tek üst kutu ve dolu tabanı alma kilidi uygulanır.

## Otomatik ve çalıştırılabilir kanıt

| Kanıt | Sonuç | SHA-256 |
|---|---|---|
| `stacking-editmode-final.xml` | `131/131`, failed `0`, skipped `0` | `b1b7039bf5ff42f8f82b1f9575bdb34b5c4ce306fbaed711a184ccb563fea737` |
| `stacking-playmode-final.xml` | `12/12`, failed `0`, skipped `0` | `0993555d45b11392c748fd7ef5d355d6887eaec449d40d1dfffc663b3351911e` |
| `stacking-macos-build.log` | Universal development build, `327.217.897` bayt | `51ad3ad892c6ef91f42494fbdff7c3db2bf2a1ac855d1d66314e9ab6234a7de7` |
| Player executable | Mach-O `x86_64 + arm64` | `7ee0d3418135a381f6751a026d4c58e2eabfb78ba3fe86fa1cf1acc8c661a356` |
| `stacking-macos-runtime.log` | Apple M4/Metal, 1280×720, `stacking=ok` | `13e030c4a45b8bd9e782e4dd63cf892814a5da2a5e32d4a3c457a1c92bd33472` |

Ham kanıtlar repository dışındaki `../TestResults` klasöründedir. GitHub Repository Guard [31856764087](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31856764087) başarılıdır.

## Korunan invariantlar ve sınır

- Pickup/drop, placement rotation, large-box carry, stable kimlik ve recovery regresyonları geçti.
- Geçersiz rotation/footprint veya dinamik destek kutuyu serbest bırakmaz.
- İstif kinematic ve deterministiktir; serbest fizik yığını değildir.
- Raf `Inventory` authority, palet/çok katlı istif ve taşıma arabası bu pakete eklenmedi.
