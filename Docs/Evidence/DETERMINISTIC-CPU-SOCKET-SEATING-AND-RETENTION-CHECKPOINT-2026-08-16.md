# Deterministic CPU Socket Seating and Retention Checkpoint — 16 Ağustos 2026

## Görünür sonuç

Issue [#55](https://github.com/cixanla/PC-Shop-Empire-3D/issues/55), Epic [#10](https://github.com/cixanla/PC-Shop-Empire-3D/issues/10) altındaki üçüncü bounded fiziksel PC assembly dilimidir:

1. GarageGraybox açık kasasında tek canonical 45 × 37,5 mm notched CPU, matching socket key, açık load plate ve retention lever görünür.
2. Oyuncu CPU'yu `E / Gamepad South` ile alır; `Mouse Left / Gamepad RT` guided mode, `R / Right Shoulder` keyed rotation ve `G / Gamepad East` seat üretir. Mode kapalıyken ghost/seat query yoktur.
3. Valid seat yalnız secured motherboard, doğru yön, menzil/focus/LOS ve obstruction-free insertion üzerinde mümkündür. Wrong orientation ayrı fresh confirm'de exact failure ve tam no-mutation üretir.
4. `EmptyOpen → ProcessorSeatedOpen → ProcessorRetained` ve ters akış stable slot/retention/item/product kimliği ile immutable receipt lineage'ı taşır. Dört operation'ın delayed replay'i aynı receipt referanslarını ve final state'i korur.
5. CPU retained iken remove engellenir. CPU takılıyken motherboard unsecure olabilir fakat detach engellenir; retention açılır, unsecured host üzerinde tekrar kapanmaz ve seated-open CPU güvenle çıkarılır.
6. Recovery aynı Unity instance, stable ItemId/ProductId, authored loose transform/parent, Rigidbody/safe pose ve tek canonical projection ile WorldFloor'a döner; Inventory Hands/Socket quantity sıfırlanır.
7. CPU package tek renderer/iki submesh ile `MotherboardPcb + BrushedSteel`, socket `WorkshopRubber`, load plate/lever `BrushedSteel` kullanır. Hard-surface UV/normaller, triangular key ve dört yönde 2 mm IHS aperture toleransı vardır.
8. Compact HUD son aktif cihaz ailesine göre doğru bindingleri gösterir; elde CPU varken checkout/customer metni gerçek ilk input consumer'ı gizleyemez. Renderer/collider/text bütçesi `21/11/1` kalır.

## Kaynak checkpoint'i

- Branch: `main`
- Feature commit: `99cadad414789d3f440e08cc6e42e727c2b7a2ad`
- Feature tree: `fea116af021d66efb31b96b4f3e7523929f8b8ad`
- Feature Repository Guard: [31914489537](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31914489537), başarılı
- Source/docs commit ve final Guard: checkpoint metadata turunda bağlanır
- ADR: `Docs/ADR-0033-DETERMINISTIC-CPU-SOCKET-SEATING-AND-RETENTION.md`
- Marker: `garage-cpu-socket-retention-r24-v1`
- Feature-tree yerel Repository Guard: `REPOSITORY_GUARD_OK unity=6000.3.21f1 legacy=26 project_bible_docs=12 tracked=624`

## Otomatik doğrulama

Final kanıtlar repo dışında, Git'e girmeyen yerel arşivde korunur:

`/Users/cixanla/Developer/PCShopEmpire3D/TestResults`

| Kanıt | Sonuç | Boyut | SHA-256 |
|---|---|---:|---|
| `editmode-issue55-r11.xml` | 430/430 geçti; failed/skipped/inconclusive 0 | 360.104 bayt | `7d2009f5f56c7737226d2fcd258610d25df612f8d439763b6f9bec745d533001` |
| `playmode-issue55-r6.xml` | 31/31 geçti; failed/skipped/inconclusive 0 | 54.364 bayt | `9c6512afaabb1818874ebacdc5f59b92b6953ec8db2e1bb44258598b181f988c` |
| `build-macos-issue55-r2.log` | Development/StrictMode build; `STAGE_A_BUILD_OK ... bytes=328144884` | 582.457 bayt | `042ffeeb60f45013dcf5c0c03a1d0a308e1cf1406fd5d3daa83e5e38c17ac34f` |
| `runtime-processor-issue55-r2.log` | Apple M4/Metal, 1280×720; readiness ve exact smoke geçti | 5.000 bayt | `b9d0fd1dff5d702f3c74d67e09c1b11dc5e30028effaece3045cd7993581e799` |
| `Assets/Scenes/Prototypes/GarageGraybox.unity` | r24 CPU/socket/load-plate/lever composition | feature tree içinde; 1.568.037 bayt | `84b10221c389ceda35170456300584b7415737fd28d3c2e5d36ad5ee87e03b4f` |
| `PrototypeProcessorPackage.asset` | 54 vertex, iki submesh/material, UV0 | 7.603 bayt | `f82f641a2296237df69d30630d0b8082474a17620b090ead9cbd578ebe6a8788` |
| `ProcessorSocketBase.asset` | 138 vertex hard-surface housing + triangular key | 13.094 bayt | `b81c50fb7b6220e23633c5a1e8b304618f4ab24a639c1365f19670e1502a500b` |
| `ProcessorLoadPlate.asset` | 96 vertex UV'li aperture frame | 10.175 bayt | `67a67ec2c758a01ff9f80b184cd3c0ea946d948abb9a96086459a04761a4e673` |
| macOS app ana executable | Universal Mach-O `x86_64 + arm64` | 117.179 bayt | `d87710b6c5f12fc832bd0a8a1eba317e1074e913beae24daa3d39436737e24f0` |

Build raporu `328.144.884` bayttır. App imzalı/notarize dağıtım paketi değildir.

Runtime işaretleri:

```text
GARAGE_GRAYBOX_RUNTIME_READY version=garage-cpu-socket-retention-r24-v1 scene=GarageGraybox resolution=1280x720 ... assembly=ready motherboard-seat=ready motherboard-fastener=ready screwdriver=ready motherboard-identity=stable processor-socket=ready processor-retention=ready processor-identity=stable lookdev=ok
GARAGE_CPU_SOCKET_RUNTIME_SMOKE cpu-socket-flow=ok preflight=ok retention-cycle=ok recovery=ok keyed-orientation=ok retained-remove-gate=ok replay=ok identity=stable
```

Native player exact başarı marker'ı kaydedildikten sonra yalnız kaydedilen PID üzerinden kontrollü kapatıldı. Final XML/loglarda test failure, skipped/inconclusive test, smoke failure, unhandled runtime exception veya crash eşleşmesi yoktur.

## Test kapsamı

- Catalog component/socket family ve exact serialized CPU identity.
- Inventory pair claim atomicity, capacity-1 ProcessorSocket custody, raw bypass ve failure no-partial-claim.
- Assembly seat/close/open/remove receipt/replay/conflict/historical lineage, wrong identity/family/state, stale revision, overflow ve full-hands no-mutation.
- Secured/unsecured host gates, retained remove, CPU-installed motherboard detach ve exact recovery state.
- NonAlloc range/focus/LOS/orientation/overlap/insertion solver; near-hit tie ve saturation fail-closed davranışı.
- Hard-surface CPU/socket/load-plate mesh, material, UV, aperture/key/clearance, workbench contact ve `21/11/1` scene bütçesi.
- Gerçek Keyboard/Mouse ve Gamepad Input System; guided mode, quarter-turn rejection, dynamic prompt/HUD ownership, co-edge/pause drain ve release–repress.
- Exact transform/parent/Rigidbody/safe-pose/same-instance recovery, four-operation delayed replay ve mevcut gameplay authority regresyonları.

## Bilinçli kapsam dışı ve devam sınırı

- Bu sonuç yalnız CPU/socket/retention'ı tamamlar; retained CPU hâlâ `assembly.benchmark.build-incomplete` verir.
- RAM/DIMM, GPU, storage, PSU, cooler, thermal paste, cable, POST/BIOS/OS/driver ve benchmark score ayrı Epic #10 child paketleridir.
- Save/Guardian mutation, final art/UI/animasyon/ses ve gerçek Windows/Steam/IL2CPP bu dilimde yoktur.
- Genel Inventory revision-max hardening ayrı P1 backlog işidir ve bu kapanışın bağımlılığı değildir.

## GitHub ve USB durumu

- Feature private `main`e ulaştı ve Repository Guard `31914489537` başarılıdır. Acceptance/Issue/Project ve source/docs final metadata turunda bağlanır.
- Kullanıcı USB'yi geçici olarak çıkardığını bildirdi. Bu turda `/Volumes` veya USB'ye erişilmedi; snapshot oluşturulmadı.
- Yerel final evidence arşivi korunur. USB yeniden bağlandığında Issue #53, #54 ve #55 source/evidence paketleri ayrı SHA-256 manifest/readback milestone'una aktarılacaktır.
