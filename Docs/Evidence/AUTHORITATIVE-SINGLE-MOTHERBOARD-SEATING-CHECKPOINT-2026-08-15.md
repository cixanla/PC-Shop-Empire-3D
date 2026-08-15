# Authoritative Single-Motherboard Seating Checkpoint — 15 Ağustos 2026

## Görünür sonuç

Issue [#53](https://github.com/cixanla/PC-Shop-Empire-3D/issues/53), Epic [#10](https://github.com/cixanla/PC-Shop-Empire-3D/issues/10) altındaki ilk bounded fiziksel PC assembly dilimidir:

1. GarageGraybox artık açık bir PC kasası, keyed motherboard tray/slot, standoff işaretleri ve tek hassas anakart projection'ı içerir.
2. Oyuncu aynı serialized anakartı `E / Gamepad South` ile alır; `Mouse Left / Gamepad RT` ile guided seat moduna girer; range, focus, LOS, pause, doğru yön, fiziksel destek ve obstruction kapıları geçerse `G / Gamepad East` ile `SeatedUnsecured` durumuna oturtur.
3. Seated board generic pickup/cart/stack/box-placement yollarına giremez. `E / Gamepad South` aynı unsecured item'ı tekrar ele alır; stable Inventory item ID ve tek fiziksel instance korunur.
4. Attach/detach yalnız `PSE.Assembly` + mevcut `PSE.Inventory` transaction'ıyla ilerler. Aynı operation replay idempotent; foreign/stale/overflow/identity/capacity hataları Assembly, Inventory ve world projection'ını değiştirmez.
5. Preview ile committed pose aynıdır. Compact şekil+metin+renk geri bildirimi ve son kullanılan cihaz ailesine göre `E/G/R/LMB` veya `A/B/RB/RT` promptları görünürdür.
6. Aynı frame Primary+Drop co-edge yalnız seat-mode geçişini tüketir; attach veya world drop üretmez. Failed world-drop kapasite reddi anakartı elde ve son güvenli pozunda bırakır; retry aynı kimlikle başarılıdır.
7. Recovery, attach→replay→detach→recovery receipt lineage'ını ve exact Assembly/Inventory revision artışlarını doğrular; duplicate/ghost custody oluşmaz.

## Kaynak checkpoint'i

- Branch: `main`
- Feature commit: `582a3cf3e81a2905e39148065bd5f6c7e35bbc06`
- Feature tree: `fc80b7cd72e0fd8bc48f5917f9c303e84d72f4cd`
- Source/docs commit: `8c6abe45d1f9b6c72def9b686b9c81bf3704d10d`
- Source/docs tree: `387bcba701b8a959681e92bf29dc48a4d09f0ab7`
- Source/docs Repository Guard: [31905540378](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31905540378), başarılı
- ADR: `Docs/ADR-0031-AUTHORITATIVE-SINGLE-MOTHERBOARD-SEATING.md`
- Marker: `garage-motherboard-seating-r22-v1`
- Yerel Repository Guard: `REPOSITORY_GUARD_OK unity=6000.3.21f1 legacy=26 project_bible_docs=12 tracked=615`

## Otomatik doğrulama

Final kanıtlar repo dışında, Git'e girmeyen yerel arşivde korunur:

`/Users/cixanla/Developer/PCShopEmpire3D/Builds/Local/Evidence/Issue53-2026-08-15`

| Kanıt | Sonuç | Boyut | SHA-256 |
|---|---|---:|---|
| `editmode-issue53-final-r12.xml` | 394/394 geçti; failed/skipped 0 | 330.118 bayt | `3543d6d8b53b2667c44ef6ed06917dc92f40aaf8aaed19ac83b27372702e5669` |
| `playmode-issue53-final-r12.xml` | 26/26 geçti; failed/skipped 0 | 42.885 bayt | `d5c2b57346f429eedf9dbaeddb322b7da3abae82399bb55945065e59a5de45f7` |
| `macos-build-issue53-final-r12.log` | Development/StrictMode build; `STAGE_A_BUILD_OK ... bytes=328020817` | 584.331 bayt | `acff0a627d4cf70a1740ce0d0aefe9a19d624996096fec0640f2c8907a4cca4a` |
| `macos-runtime-issue53-final-r12.log` | Apple M4/Metal, 1280×720; readiness ve exact smoke geçti | 5.194 bayt | `ce0dfd24a196bac87cc7ec372acdeade69fa5067852e8ff737b28d9460f10dd1` |
| `Assets/Scenes/Prototypes/GarageGraybox.unity` | r22 authored chassis/seat/motherboard composition | 1.542.496 bayt | `76e4ca7dff6c2b778d8f5b7c6f779217d474e177e8d5255fc68d44b8a5f42bda` |
| macOS app ana executable | Universal Mach-O `x86_64 + arm64` | 117.179 bayt | `cad75f5e070dfabe0335f9c6ee8d50659dc3ceddd1e036cb63c83b787e5da0f0` |

App bundle yerel disk kullanımı `320.976 KiB`dir. Build imzalı/notarize dağıtım paketi değildir.

Runtime işaretleri:

```text
GARAGE_GRAYBOX_RUNTIME_READY version=garage-motherboard-seating-r22-v1 scene=GarageGraybox resolution=1280x720 ... assembly=ready motherboard-seat=ready motherboard-identity=stable lookdev=ok
GARAGE_MOTHERBOARD_ASSEMBLY_RUNTIME_SMOKE assembly-flow=ok compatible=ok mismatch-blocked=ok attach=ok attach-replay=ok detach=ok input-single-consumer=ok authority-isolated=ok identity-stable=ok recovery=ok
```

Native player, exact başarı marker'ı kaydedildikten sonra kontrollü olarak kapatıldı. Final XML/loglarda test failure, skipped test, assertion veya unhandled exception yoktur.

## Test kapsamı

- Unity bağımsız Catalog/Inventory-backed Assembly authority, canonical IDs, immutable snapshot/receipt, replay/conflict ve monotonic revision.
- Managed Workbench claim, occupied/foreign/stale/overflow/capacity failure no-mutation ve raw batch/reservation ingress yasağı.
- Kind/form-factor compatibility; exact serialized identity ve ActorHands↔Workbench transfer lineage'ı.
- Default/reset/disabled seat fail-closed; gerçek geometric range/focus, LOS, pause, support, keyed rotation ve obstruction dalları.
- Authored scene references, tek canonical projection, chassis/tray/standoff/connector geometry ve preview ölçüsü.
- Gerçek Keyboard/Mouse ve Gamepad Input System pickup→preview→confirm→detach; dynamic prompt, same-frame co-edge, held/replay ve failed-drop retry.
- Mevcut pickup/drop/placement/rotation/stacking/cart, parcel/stock/offer/customer/checkout/Economy/NavMesh regresyonları.

## Bilinçli kapsam dışı ve devam sınırı

- Sonuç yalnız `SeatedUnsecured`dır. Vida/fastener, screwdriver/torque ve secured motherboard sonraki bounded Epic #10 child paketidir.
- CPU, RAM, GPU, storage, PSU, cooler, cable, POST/BIOS/OS/driver ve benchmark score bu dilimde yoktur.
- Inventory authority Issue #7/#8 kapsamı yeniden açılmaz; genel revision-max hardening ayrı issue olarak ele alınır.
- Final art/UI/animasyon/ses ve gerçek Windows/DirectX/Steam/IL2CPP dış kapıdır.

## GitHub ve USB durumu

- Issue #53 acceptance `18/18` tamamlandı; Issue `Completed`, Development Roadmap `Done`dur.
- Issue #53–#55 birleşik milestone'u `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-16_STAGE_B_PHYSICAL_ASSEMBLY_MOTHERBOARD_FASTENER_AND_CPU_SOCKET_RETENTION` altında source `07364b79`, 640 tracked source + 12 final evidence + source kaydıyla doğrulandı.
- `MANIFEST.tsv` 653/653 hash/boyut/yol readback ve 640/640 exact Git source eşliğini geçti; SHA-256 `0b5f3c6100abeb3dc28e292ed515186fffabaa17f4c3ec66aef3399572aaba9e`; güvenlik ve AppleDouble mismatch `0`.
