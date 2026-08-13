# Küçük Kutu Placement Rotation Checkpoint'i — 13 Ağustos 2026

## Görünür sonuç

Oyuncu küçük kutuyu placement modunda klavyede `R`, gamepad'de `Right Shoulder` ile kasıtlı `90°` adımlarla döndürebilir. Ghost anında güncellenir, etkin binding ve mevcut açı HUD'da görünür. Döndürülmüş kutu duvar, oyuncu veya başka nesneyle çakışırsa yerleştirme fail-closed kalır; başarılı yerleştirme görülen pozla aynı, stabil kinematic sonuçtur.

## Kaynak kanıtı

- Feature commit: `661f2dcc64246a8282fd63fbf303454ec856ea40`
- Feature tree: `d841329fcc351db4c9053a43ce5403855ffb57a0`
- Epic/issue: [#6](https://github.com/cixanla/PC-Shop-Empire-3D/issues/6) / [#33](https://github.com/cixanla/PC-Shop-Empire-3D/issues/33)
- Karar: [`ADR-0012-CONTROLLED-SMALL-BOX-ROTATION.md`](../ADR-0012-CONTROLLED-SMALL-BOX-ROTATION.md)
- Remote Repository Guard: [Actions run 31683991075](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31683991075), başarılı.

## Otomatik doğrulama

Ham çıktılar Git dışındaki `/Users/cixanla/Developer/PCShopEmpire3D/TestResults` klasöründedir.

| Kanıt | Sonuç | SHA-256 |
|---|---|---|
| `rotation-editmode.xml` | 127/127 geçti | `8fd8c1245fcbf106bddf20a51196a58298619d19a36d0ed3e2cdded9501a569b` |
| `rotation-editmode.log` | Unity batch temiz tamamlandı | `61311119851dc6f9ef3180afcaa17114485e489fbd334f6328b627583d7df371` |
| `rotation-playmode-final.xml` | 10/10 geçti | `e858cd0c42b7fa94a7a826d3823dc6c1d515fafe2f4fdc40228a2069d827e8a2` |
| `rotation-playmode-final.log` | Gerçek Input System ve `rotation=ok` | `2e84f437674b8f19b60c9072f99e6b23ac1e53d172e6fde4a6485c7b7a92999b` |

EditMode quarter-turn normalizasyonunu, action/binding sözleşmesini ve sahne ölçülerini doğrular. PlayMode gerçek keyboard/gamepad device-state olaylarıyla dönüşü, etkin promptu, ghost/confirm poz eşitliğini, döndürülmüş obstruction kontrolünü, büyük-kutu izolasyonunu ve önceki pickup/drop/recovery regresyonlarını doğrular.

## Build ve gerçek player smoke

| Kanıt | Sonuç | SHA-256 |
|---|---|---|
| `rotation-macos-build.log` | Universal development build; 326.160.273 bayt | `33a4cd4e2ab11229be86e3b5587d6a708365bd51909e656ed0e57312d55aa2e7` |
| Player executable | Mach-O `x86_64 + arm64` | `258483b14034f9043298ae07635f6f50ca629a923e82e65eeafd9fa1c741f743` |
| `rotation-macos-runtime.log` | Unity 6000.3.21f1, Apple M4/Metal, 1280×720, `rotation=ok` | `7ffbd6e3847c8df022a70983718359ef51f122dc3f6246b2a1eedcd308661e7a` |
| `rotation-macos-runtime.png` | Dikdörtgen kutu, görünür eller ve `R / RB ... [90°]` promptu | `6f07afe2daf4b9bb2543c0d719511490dc4d3811660d23842a9bc1310c1b67d1` |

Runtime açılışında crash, missing script veya `NullReferenceException` görülmedi; player normal kapatıldı. Mac kanıtı gerçek Windows x64, DirectX, Steam veya IL2CPP doğrulamasının yerine geçmez.

## Açık sınır

- Serbest/sürekli rotation, pitch/roll ve büyük-kutu placement yoktur.
- Kutu üstü istifleme ve taşıma arabası henüz yoktur.
- Dünya nesnesi authoritative stok değildir; Catalog/Inventory/Orders bağlantısı Issue #7/#8'e aittir.
- Dikdörtgen kutu, yön işareti, eller ve garaj graybox'tır; final sanat değildir. Görsel kalite hedefi [`ADR-0013-READABLE-SEMI-REALISTIC-VISUAL-DIRECTION.md`](../ADR-0013-READABLE-SEMI-REALISTIC-VISUAL-DIRECTION.md) içinde tanımlıdır.
