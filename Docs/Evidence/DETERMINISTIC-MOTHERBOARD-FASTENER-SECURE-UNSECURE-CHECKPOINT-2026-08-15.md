# Deterministic Motherboard Fastener Secure/Unsecure Checkpoint — 15 Ağustos 2026

## Görünür sonuç

Issue [#54](https://github.com/cixanla/PC-Shop-Empire-3D/issues/54), Epic [#10](https://github.com/cixanla/PC-Shop-Empire-3D/issues/10) altındaki ikinci bounded fiziksel PC assembly dilimidir:

1. GarageGraybox açık kasasında tek captive motherboard fastener, ayrı solid focus target, cross recess ayrıntısı, fiziksel screwdriver ve küçük status plate bulunur.
2. Oyuncu seated anakartın vidasını `Mouse Left / Gamepad RT` ile sıkar veya gevşetir. Son kullanılan cihaz ailesine göre `LMB/E` ya da `RT/A` promptları görünür.
3. Range, focus, LOS, pause ve obstruction kapıları domain mutation öncesi fail-closed çalışır. Blocked bağlam Primary/Interact/Drop edge'lerinin üçünü de tüketir; blocker aynı Input System frame'inde kalksa dahi eski edge replay edilemez.
4. `SeatedUnsecured → SeatedSecured → SeatedUnsecured` geçişleri stable fastener kimliği, immutable receipt ve exact attach/secure lineage'ıyla yürür. Secure/unsecure Inventory custody veya revision değiştirmez.
5. Secured anakart hem presentation pickup yolunda hem doğrudan Assembly detach authority'sinde sökülemez. Unsecure sonrası aynı canonical fiziksel anakart ve Inventory item ID korunur.
6. Screw head secured durumda authored local forward yönünde exact `4 mm` ilerler; screw ve screwdriver rotation durumu değişir. Pose drift `assembly-seat.projection-invariant` üretir, authority'yi değiştirmez ve authoritative re-apply ile düzelir.
7. Dünya metni fiziksel plate üzerinde tek satırdır; `[ ] ANAKARTI OTURT`, `[O] VİDA GEVŞEK`, `[OK] VİDA SIKILI`, blocked `[X] ÖNÜNÜ AÇ` sözleşmesi renk dışında da okunur. Büyük/yüzen sentetik debug yazısı yoktur.
8. Runtime smoke immediate ve delayed secure/unsecure replay'i, presentation + authority detach kilidini, detach/recovery lineage'ını, Inventory izolasyonunu ve tek canonical identity'yi birlikte doğrular.

## Kaynak checkpoint'i

- Branch: `main`
- Feature commit: `b6812394f835d64d5bf8422d8e7996ec433cd0f1`
- Feature tree: `192f9d8f1334cf9e1ff1d21382c44a847bbfa7e6`
- Source/docs commit: `SOURCE_DOCS_COMMIT_PENDING`
- Source/docs tree: `SOURCE_DOCS_TREE_PENDING`
- Source/docs Repository Guard: `SOURCE_DOCS_GUARD_PENDING`
- ADR: `Docs/ADR-0032-DETERMINISTIC-MOTHERBOARD-FASTENER-SECURE-UNSECURE-GATE.md`
- Marker: `garage-motherboard-fastener-r23-v1`
- Feature-tree yerel Repository Guard: `REPOSITORY_GUARD_OK unity=6000.3.21f1 legacy=26 project_bible_docs=12 tracked=616`
- Source/docs çalışma snapshotı yerel Repository Guard: `REPOSITORY_GUARD_OK unity=6000.3.21f1 legacy=26 project_bible_docs=12 tracked=622`

## Otomatik doğrulama

Final kanıtlar repo dışında, Git'e girmeyen yerel arşivde korunur:

`/Users/cixanla/Developer/PCShopEmpire3D/TestResults`

| Kanıt | Sonuç | Boyut | SHA-256 |
|---|---|---:|---|
| `editmode-issue54-r4.xml` | 411/411 geçti; failed/skipped 0 | 344.651 bayt | `ac41d217a24b14dd59dd3ef32991f045db1bed392bb1583968741239957f8dbc` |
| `playmode-issue54-r4.xml` | 29/29 geçti; failed/skipped 0 | 48.857 bayt | `efed1fea5e22856c33b52ca8fa13329f7bd12d6c9a87779031acb37997cc0453` |
| `build-macos-issue54-r4.log` | Development/StrictMode build; `STAGE_A_BUILD_OK ... bytes=328057977` | 582.303 bayt | `efa55e5af3d53bd2a9563bee21feccfafd0b322d6bb83f576e39327361c64fa8` |
| `runtime-assembly-issue54-r6.log` | Apple M4/Metal, 1280×720; readiness ve exact smoke geçti | 5.417 bayt | `3a7d7f5cc16797a965f80d3b73d99f2f35cb3ffa9abff9cda34e735c3c053c3f` |
| `Assets/Scenes/Prototypes/GarageGraybox.unity` | r23 fastener/screwdriver/status-plate composition | feature tree içinde | `2358218819984b78274afd898e771299be3b6df83b83d69d5767f08b417bb0e4` |
| macOS app ana executable | Universal Mach-O `x86_64 + arm64` | 117.179 bayt | `f9cc04036d4185d8ce6c7f8c61e6edf7d129ec514a5538927ef889364a7aad69` |

App bundle yerel disk kullanımı `321.004 KiB`dir. Build imzalı/notarize dağıtım paketi değildir.

Runtime işaretleri:

```text
GARAGE_GRAYBOX_RUNTIME_READY version=garage-motherboard-fastener-r23-v1 scene=GarageGraybox resolution=1280x720 ... assembly=ready motherboard-seat=ready motherboard-fastener=ready screwdriver=ready motherboard-identity=stable lookdev=ok
GARAGE_MOTHERBOARD_ASSEMBLY_RUNTIME_SMOKE assembly-flow=ok compatible=ok mismatch-blocked=ok attach=ok attach-replay=ok fastener=ok secure=ok secure-replay=ok secure-delayed-replay=ok detach-blocked=ok detach-authority-blocked=ok unsecure=ok unsecure-replay=ok unsecure-delayed-replay=ok detach=ok duplicate-seat-confirm-blocked=ok authority-isolated=ok identity-stable=ok recovery=ok
```

Native player exact başarı marker'ı kaydedildikten sonra yalnız kaydedilen PID üzerinden kontrollü kapatıldı. Final XML/loglarda test failure, skipped test, smoke failure, assertion, unhandled exception veya `JobTempAlloc` eşleşmesi yoktur.

## Test kapsamı

- Assembly-owned fastener ID, seat-state invariantı, secure/unsecure receipt/replay/conflict ve historical lineage fold'u.
- Wrong item/slot/fastener/state/lineage, stale revision ve overflow failure no-mutation.
- Secure/unsecure boyunca exact Inventory custody/revision izolasyonu ve secured direct detach gate'i.
- NonAlloc range/focus/LOS/pause/obstruction solver; coincident/near-hit deterministic tie-break ve hit saturation fail-closed davranışı.
- Screw/tool pose, collider, material, single-line TextMesh, status plate render maliyeti ve r23 serialized scene reference/count sözleşmesi.
- Gerçek Keyboard/Mouse ve Gamepad Input System secure/unsecure; dynamic prompt, blocked same-frame edge drain, pause co-edge ve release–repress.
- Projection drift fail-closed/re-apply; mevcut pickup/drop/placement/rotation/stacking/cart, stock/customer/checkout/Economy/NavMesh regresyonları.

## Bilinçli kapsam dışı ve devam sınırı

- Bu sonuç yalnız motherboard fastener'ı tamamlar; `SeatedSecured` hâlâ `assembly.benchmark.build-incomplete` verir.
- CPU/socket, RAM, GPU, storage, PSU, cooler, thermal paste, cable, POST/BIOS/OS/driver ve benchmark score ayrı Epic #10 child paketleridir.
- Tool Inventory authority'si, çoklu vida/torque grind, Save/Guardian mutation, final art/UI/animasyon/ses ve Windows/Steam/IL2CPP bu dilimde yoktur.
- Genel Inventory revision-max hardening ayrı P1 backlog işidir ve bu kapanışın bağımlılığı değildir.

## GitHub ve USB durumu

- Issue #54 yerel kaynak/test/build/runtime kabulü tamamdır; private push, Repository Guard, Issue/Project `Done` durumu takip metadata commitinde kesinleştirilir.
- Kullanıcı USB'yi geçici olarak çıkardığını bildirdi. Bu turda `/Volumes` veya USB'ye erişilmedi; snapshot oluşturulmadı.
- Yerel final evidence arşivi korunur. USB yeniden bağlandığında Issue #53 ve #54 source/evidence paketleri ayrı SHA-256 manifest/readback milestone'ına aktarılacaktır.
