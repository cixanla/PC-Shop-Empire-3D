# PC Shop Empire 3D — Birleşik Codex Proje Hafızası

**Konsolidasyon tarihi:** 15 Ağustos 2026
**Tek aktif yazma hattı:** `PC Shop Empire 3D — ANA GÖREV`; proje içindeki yardımcı Codex görevleri yalnız bounded salt-okunur denetim yapar
**Ana görev kimliği:** `01a03e98-bf8c-7190-850e-1bff81843fa8`
**Canonical Unity/Git kökü:** `/Users/cixanla/Developer/PCShopEmpire3D/Game`
**Private GitHub:** `cixanla/PC-Shop-Empire-3D`
**Authoritative Git state:** `main` `0ea82e826aff9d245e0d4002386193278f25b483`; PR #116; main Guard `33127652290`

Bu belge, `PC Shop Empire Similator` altındaki üç Codex görevinin proje açısından anlamlı bütün bilgisini tek uygulanabilir hafızada birleştirir. Tam kullanıcı/Codex konuşmaları [CodexHistory indeksinde](../CodexHistory/README.md) korunur. Günlük teknik devam noktası için her zaman [10_DEVAM_CHECKPOINT.md](10_DEVAM_CHECKPOINT.md) daha günceldir.

### 27 Ağustos 2026 üstün gelen güncel durum — Issue #114 Retail/Checkout Hero Readability

- GarageGraybox r56 customer approach, gerçek authoritative shelf offer/reserved basket ve gerçek checkout/payment/receipt durumlarını üç 1280x720 native composition'da ayırır. `RetailCheckoutHeroProjection` existing authority state'ini sunar; ikinci item/placement/reservation/payment/receipt authority üretmez.
- Issue #111 baseline→r56 authored MeshRenderer `490→499` (`+9`), light/camera `4→5 / 1→1`; runtime `502/478`. Dokuz hero renderer Ignore Raycast/no-collider/no-shadow/no-motion-vector, fill light shadowless'tır. NavMesh, route, collider, waypoint, input ve stable identity untouched; pre-existing `StarterShelf` collider borcu #115'e taşınmıştır.
- Technical/main `0ea82e826aff9d245e0d4002386193278f25b483`, tree `8cbe7bd7c7628d923930213de30e1bda73cb7619`; Mac hero `2/2`, full `754/754 + 158/158`; Windows full `754/754 + 158/158`; accepted failures/skips/inconclusive `0`.
- Universal Mac report `330481405` bayt ve Apple M1/Metal retail + Assembly regression smoke geçti. Clean Windows x64 IL2CPP/only-D3D11 report `1351471280` bayt; Intel Iris Xe runtime `27/27`, graceful exit, glare `0`, minimum contrast `1.348` ve final residue `0` verdi.
- Windows evidence Mac'te bağımsız hash/dimension/metric readback aldı. Technical Guard `33109651186`, PR #116 fast-forward merge ve main Guard `33127652290` geçti; Issue closed/Done. USB yoktu/yazılmadı; physical checkpoint açık, claim `human=false`tır.
- Parent Epic #18 ve Steam 1.0 Goal sürer. Sıradaki bounded source işi #115 legacy `StarterShelf` collider/NavMesh consolidation'dır. Mac tek write lane; UTM yardımcı portability kontrolü olabilir ama physical Windows/Iris Xe kanıtı değildir.

### Önceki üstün gelen durum — Issue #111 Assembly Workbench Hero Readability

- GarageGraybox r55, açık chassis/motherboard/GPU/PSU ile loose/preview/routed cable durumlarını aynı 1280x720 composition'da ayırır. Shared non-emissive `CableConnectorPolymer` beş connector/PSU-intake renderer'ı ve on dört GPU fan blade'i; `WorkshopMatteHardware` iki rear bracket'i glare-safe matte sunuma taşır. Existing Workbench light `0.4 / 2.8 / 62°`; authority/collider/anchor/waypoint/topology/identity/input untouched, ProjectSettings byte-exact'tir.
- Base→r55 authored MeshRenderer `486→490` (`+4`), light/camera delta `0/0`; runtime total/initial-active/smoke-active `493/473/484`. Dört ek hero renderer Ignore Raycast, collider/light/shadow/motion-vector içermez. Mac P1 `7/7`, full `753/753 + 157/157`; Windows full `753/753 + 157/157`; accepted fail/skip/inconclusive `0`.
- Universal Mac report `330428946` bayt, strict/deep-valid `x86_64 + arm64`, Apple M1/Metal three-state smoke başarılıdır. Clean Windows x64 IL2CPP/only-D3D11 report `1350529280` bayt; Intel Iris Xe/D3D11 runtime final `26/26`, exit `0`, graceful shutdown ve central glare `0/64`dır.
- Windows evidence Mac'e exact readback edildi. Disposable root, iki exact firewall rule ve beş temp dosya kaldırıldı; final process/task/firewall/temp residue `0/0/0/0`. USB `D:` Intenso Alu Line serial `900B00076010` `Healthy/OK` olarak yalnız okundu, yazılmadı. Claim `human=false`; physical HID/gamepad/endurance Steam 1.0 sertifikasyonunda kalır.
- Technical Guard `33089682114`, docs Guard `33093360490`, PR #112 merge `d20d5bc9fa1a67bf0e9441253834a9de962046e8` ve main Guard `33093461437` geçti. Issue #111 `CLOSED/COMPLETED`, Roadmap `Done`; parent Epic #18 ve full Steam 1.0 Goal sürer. Sıradaki bounded visual aday müşteri-facing retail floor/checkout readability dilimidir.
- Tek kanonik iletişim merkezi ve yazma hattı bu Game projesine bağlı ana görevdir. Ürün kuzey yıldızı Dashboard parity'yi fiziksel 3D oynanışa, küçük→büyük mağazalara, çalışan/müşterilere, işlevsel mahalle/kişisel ev/araç-lojistiğe, dünya NPC ekolojisine ve retail çekirdekten ayrılmış güvenli offline Local Advisor/Guardian sınırına bağlar. Kontrolsüz self-modifying code yoktur; Mac tek write lane, Windows clean native worker'dır.

### Önceki üstün gelen durum — Issue #109 PCIe/GPU 6+2 BuildKit→Route Reversible Assembly

- Exact reserved PCIe/GPU 6+2 cable yalnız owned work order/ticket/allocation içindeki tam line/kind/`ModularPcie8PinPsuToGraphicsCard` family/product/item/reservation/staging-receipt lineage ile seçilir; historical ten-receipt `10/10` aggregate, live #89/#91/#93/#95/#97/#99/#102 receipts ve exact routed #105 ATX24 + #107 EPS12V sonrasında ayrı stable operation exact PCIe/GPU BuildKit → ActorHands → existing Issue #63 route → ActorHands custody'sini açar.
- Inventory yalnız kayıtlı PCIe/GPU BuildKit release ve exact GpuPowerCableRoute↔Hands reversible transferini kabul eder. Same Unity instance/stable ItemId, live reservation/allocation, immutable staging history ve exact replay/revision korunur; installed seven prerequisites ile routed ATX24/EPS12V protected, generic transfer/drop/box/stack/cart/raw Inventory ve receipt-free bypass fail-closed'dur.
- GarageGraybox r54 keyboard/mouse + Input System virtual-gamepad pickup, existing keyed 8-pin + 6+2 connector/three-waypoint guided preview, clearance/obstruction gates, route, generic-drop/dependent-remove block ve exact unroute akışını single-consumer input ile taşır. Doğrudan focus edilen routed physical cable fixed cable-priority stealing'i engeller; authority-first projection/recovery same-instance exactly-once kalır.
- Existing Issue #63 route/unroute/replay authority tek PCIe/GPU Assembly gerçeğidir. Authored route-collider allowlist narrow, foreign obstruction bloklanır. Üç canonical power cable routed olsa da electrical readiness/power-on üretilmez; assembly `BuildIncomplete` kalır ve ProjectSettings değişmemiştir.
- Technical commit `1acba166855efffa906112e2df24b9b5cef550a7`, tree `eb40a392169e5288e29bc59ae75367029cc00f57`; targeted Mac domain EditMode `87/87`, scene `9/9`, P1 PlayMode `4/4`; full Mac EditMode `752/752`, PlayMode `156/156`; full Windows EditMode `752/752`, PlayMode `156/156`; Universal Mac report `330366591` bayt ve technical Guard `33054757532` başarılıdır.
- Clean Windows exact source/clone x64 IL2CPP/only-D3D11 report `1349053878` bayt ve fatal-token `0` verdi. Intel Iris Xe/D3D11 exact r54 runtime host/readiness/success `1/1/1`, forbidden `0`, graceful shutdown ve scoped residue `0` ile geçti.
- Accepted foreground Session-2 Win32 OS input W/A/S/D `1/1`, relative mouse `18/18`, combined W+D+mouse `3/3` ve held mouse `30/30` zincirini exact foreground guard ile doğruladı. Runtime forbidden `0`; Windows final audit `28/28`; validation-created temp firewall rules guarded temizlendi; process/task/firewall residue `0`. Exact evidence tar Mac'e `4599837` bayt / `924792e2…74e4` readback ile döndü; `30/30` manifest evidence, üç native record ve iki self-referential dosya eşleşti.
- Mac readback sonrasında exact Windows temp clone/build/evidence kökü kaldırıldı; final residue `0/0/0`. ADR-0063/tarihli Evidence exact kanıtları bağlar. Claim `human=false`; fiziksel keyboard/gamepad/endurance Steam 1.0 sertifikasyonunda kalır. Kabul sırasında USB yoktu; sonrasında Windows `D:` Intenso Alu Line serial `900B00076010` `Healthy/OK` olarak yalnız okundu, yazılmadı. PR #110 integration record'dur; parent Epic #10 ve Steam 1.0 Goal electrical/visual/product işlerine devam eder.
- Canonical ten-part BuildKit→installed/routed physical chain tamamlandı. Sıradaki bounded ürün işi yeni foundation açmak yerine Assembly Workbench hero readability görsel kalite dilimidir; ardından electrical/POST/BIOS/OS/benchmark authority zinciri sürer.
- Tek kanonik iletişim merkezi ve yazma hattı bu Game projesine bağlı ana görevdir. Ürün kuzey yıldızı Dashboard parity'yi fiziksel 3D oynanışa, küçük→büyük mağazalara, çalışan/müşterilere, işlevsel mahalle/kişisel ev/araç-lojistiğe, dünya NPC ekolojisine ve retail çekirdekten ayrılmış güvenli offline Local Advisor/Guardian sınırına bağlar. Kontrolsüz self-modifying code yoktur; Mac tek write lane, Windows clean native worker'dır.

### Önceki üstün gelen durum — Issue #107 EPS12V BuildKit→Route Reversible Assembly

- Exact reserved EPS12V cable yalnız owned work order/ticket/allocation içindeki tam line/kind/`ModularEps12v8PinPsuToMotherboard` family/product/item/reservation/staging-receipt lineage ile seçilir; historical ten-receipt `10/10` aggregate, live #89/#91/#93/#95/#97/#99/#102 receipts ve exact routed #105 ATX24 sonrasında ayrı stable operation exact EPS12V BuildKit → ActorHands → existing Issue #62 route → ActorHands custody'sini açar.
- Inventory yalnız kayıtlı EPS12V BuildKit release ve exact CpuPowerCableRoute↔Hands reversible transferini kabul eder. Same Unity instance/stable ItemId, live reservation/allocation, immutable staging history ve exact replay/revision korunur; installed seven prerequisites, routed ATX24 ve staged PCIe/GPU lineage protected, generic transfer/drop/box/stack/cart/raw Inventory ve receipt-free bypass fail-closed'dur.
- GarageGraybox r53 keyboard/mouse + Input System gamepad pickup, existing keyed two-endpoint/three-waypoint guided preview, clearance/obstruction gates, route, generic-drop/dependent-remove block ve exact unroute akışını single-consumer input ile taşır. Authority-first projection/recovery same-instance exactly-once kalır.
- Existing Issue #62 route/unroute/replay authority tek EPS12V Assembly gerçeğidir. Solver yalnız exact chassis-right-rail/GPU/PCIe-connector colliders'ını narrowly authored host exclusion kabul eder; foreign obstruction bloklanır. Electrical readiness üretilmez: ATX24+EPS12V routed ve PCIe eksikken `BuildIncomplete`, EPS12V unroute sonrasında `PowerCableMissing`; ProjectSettings değişmemiştir.
- Technical commit `9cd3276d60c03cec1b5b15049027523dddbee8b6`, tree `01f3edc99dd94aeeb125323048bf8532891c028a`; targeted Mac domain EditMode `83/83`, scene `9/9`, P1 PlayMode `4/4`; full Mac EditMode `748/748`, PlayMode `152/152`; full Windows EditMode `748/748`, PlayMode `152/152`; Universal Mac report `330340220` bayt ve technical Guard `33044086315` başarılıdır.
- Complete bundle `7708889` bayt / `ffd2d43a…e55` ile detached-clean Windows x64 IL2CPP/only-D3D11 report `1348030823` bayt ve fatal-token `0` verdi. Intel Iris Xe/D3D11 exact r53 runtime host/readiness/success `1/1/1`, forbidden `0`, graceful shutdown ve scoped residue `0` ile geçti.
- Accepted foreground Session-2 Win32 OS input W/A/S/D `1/1`, relative mouse `18/18`, combined W+D+mouse `3/3` ve held mouse `30/30` zincirini exact foreground guard ile doğruladı. Runtime forbidden `0`; Windows final audit `28/28`; validation-created temp firewall rules guarded temizlendi; process/task/firewall residue `0`. Exact evidence tar Mac'e `3146658` bayt / `239614d2…525` readback ile döndü ve `32/32` manifest artifact plus iki self-referential dosya eşleşti.
- ADR-0062/tarihli Evidence exact kanıtları bağlar. Claim `human=false`; fiziksel keyboard/gamepad/endurance Steam 1.0 sertifikasyonunda kalır. Windows USB/removable disk bulmadı ve yazmadı. PR #108 source/docs integration sonrasında Issue #107/Roadmap kapanır; parent Epic #10 ve Steam 1.0 Goal PCIe/electrical/product işlerine devam eder.
- Tek kanonik iletişim merkezi ve yazma hattı bu Game projesine bağlı ana görevdir. Ürün kuzey yıldızı Dashboard parity'yi fiziksel 3D oynanışa, küçük→büyük mağazalara, çalışan/müşterilere, işlevsel mahalle/kişisel ev/araç-lojistiğe, dünya NPC ekolojisine ve retail çekirdekten ayrılmış güvenli offline Local Advisor/Guardian sınırına bağlar. Kontrolsüz self-modifying code yoktur; Mac tek write lane, Windows clean native worker'dır.

### Önceki üstün gelen durum — Issue #105 ATX24 BuildKit→Route Reversible Assembly

- Exact reserved ATX24 cable yalnız owned work order/ticket/allocation içindeki tam line/kind/`ModularAtx24SplitPsuToMotherboard` family/product/item/reservation/staging-receipt lineage ile seçilir; historical ten-receipt `10/10` aggregate ve live #89/#91/#93/#95/#97/#99/#102 receipts sonrasında ayrı stable operation exact ATX24 BuildKit → ActorHands → existing Issue #61 route → ActorHands custody'sini açar.
- Inventory yalnız kayıtlı ATX24 BuildKit release ve exact ATX24 route↔Hands reversible transferini kabul eder. Same Unity instance/stable ItemId, live reservation/allocation, immutable staging history ve exact replay/revision korunur; installed seven prerequisites ve other-two cables untouched, generic transfer/drop/box/stack/cart/raw Inventory ve receipt-free bypass fail-closed'dur.
- GarageGraybox r52 keyboard/mouse + Input System gamepad pickup, existing keyed endpoint/ordered-waypoint guided preview, clearance/obstruction gates, route, generic-drop block, routed-cable PSU-unretain block ve exact unroute akışını single-consumer input ile taşır. Domain-first projection/recovery same-instance exactly-once kalır.
- Existing Issue #61 route/unroute/replay authority tek ATX24 Assembly gerçeğidir. Solver yalnız exact installed cooler/GPU/chassis-right-rail roots'u authored host exclusion kabul eder; foreign obstruction bloklanır. Electrical readiness üretilmez: routed durumda EPS12V/PCIe eksikken `BuildIncomplete`, unroute sonrasında `PowerCableMissing`; ProjectSettings değişmemiştir.
- Technical commit `5d6a39892cf3c585abd1046cc799a93418329cd0`, tree `263307821aeba8df6648a39756bec431e548938f`; targeted Mac EditMode `79/79`, P1 PlayMode `4/4`; full Mac EditMode `744/744`, PlayMode `148/148`; full Windows EditMode `744/744`, PlayMode `148/148`; Universal Mac report `330311979` bayt ve technical Guard `33038180913` başarılıdır.
- Complete bundle `7678445` bayt / `a9c331a4…503` ile detached-clean Windows x64 IL2CPP/only-D3D11 report `1347195309` bayt ve fatal-token `0` verdi. Intel Iris Xe/D3D11 exact r52 runtime host/readiness/success `1/1/1`, forbidden `0`, graceful shutdown ve scoped residue `0` ile geçti.
- Accepted foreground Session-2 r2 Win32 OS input W/A/S/D `1/1`, relative mouse `18/18`, combined W+D+mouse `3/3` ve held mouse `30/30` zincirini exact foreground guard ile doğruladı. Runtime forbidden `0`; Windows final audit `33/33`; validation-created temp firewall rules temizlendi; process/task/firewall residue `0`. Exact evidence tar Mac'e `6091832` bayt / `5e3674de…0c21` readback ile döndü ve `36/36` taşınan hash eşleşti.
- ADR-0061/tarihli Evidence exact kanıtları bağlar. Claim `human=false`; fiziksel keyboard/gamepad/endurance Steam 1.0 sertifikasyonunda kalır. Windows USB/removable disk bulmadı ve yazmadı. PR #106 merge `00a3f545fa7db0b08f5f4337e8229d0f69cb7781` sonrasında Issue #105/Roadmap kapandı; parent Epic #10 ve Steam 1.0 Goal EPS12V/PCIe/electrical/product işlerine devam eder.
- Tek kanonik iletişim merkezi ve yazma hattı bu Game projesine bağlı ana görevdir. Ürün kuzey yıldızı Dashboard parity'yi fiziksel 3D oynanışa, küçük→büyük mağazalara, çalışan/müşterilere, işlevsel mahalle/kişisel ev/araç-lojistiğe, dünya NPC ekolojisine ve retail çekirdekten ayrılmış güvenli offline Local Advisor/Guardian sınırına bağlar. Kontrolsüz self-modifying code yoktur; Mac tek write lane, Windows clean native worker'dır.

### Önceki üstün gelen durum — Issue #102 Power Supply BuildKit→Bay/Four-Fastener Assembly

- Exact reserved ATX PS/2 PSU yalnız owned work order/ticket/allocation içindeki tam line/product/item/reservation/staging-receipt lineage ile seçilir; historical ten-receipt `10/10` aggregate, live Issue #89 secured motherboard, #91 retained CPU, #93 retained A2 DDR5, #95 secured primary M.2, #97 retained cooler ve #99 retained GPU receipts sonrasında ayrı stable operation exact PowerSupply BuildKit → ActorHands → existing PowerSupplyBay custody'sini açar.
- Inventory yalnız kayıtlı PSU BuildKit release ve exact PowerSupplyBay↔Hands reversible transferini kabul eder. Same Unity instance/stable ItemId, live reservation/allocation, immutable staging history ve exact replay/revision korunur; installed six prerequisites ve three cables untouched, generic transfer/drop/box/stack/cart ve receipt-free Assembly bypass fail-closed'dur.
- GarageGraybox r51 keyboard/mouse + gamepad pickup, existing two-orientation ATX PS/2 guided preview, support/rear-plane/clearance/obstruction gates, invalid-orientation block, seat, stable four-fastener retain, retained-remove block, reverse unretain, detach ve reseat akışını single-consumer input ile taşır. Current obstruction recovery'yi fail-closed tutar; clear sonrasında same-instance exactly-once recovery geçer.
- Existing Issue #60 compatibility/orientation/seat/four-fastener/replay authority tek PSU Assembly gerçeğidir. Issue #61/#62/#63 cable item/product/container/state/revision/receipt/operation authority exact untouched kalır; routed cable unretain/remove'u bloklar ve Issue #102 route üretmez. Lower PSU chamber authored tray/status geometry ile açılmış, ProjectSettings değiştirilmemiştir.
- Technical commit `740a8869e2efc1f525b9560d4d5638343c957eb5`, tree `d64e70bb6bd2d7f0d8583555146050f7060db0f2`; targeted Mac EditMode `6/6`, PlayMode `5/5`, scene/readiness/recovery `1/1`; full Mac EditMode `739/739`, PlayMode `144/144`; full Windows EditMode `739/739`, PlayMode `144/144`; Universal Mac report `330279904` bayt ve technical Guard `33027397901` başarılıdır.
- Complete bundle `7632290` bayt / `3936b661…3fb53f` ile detached-clean Windows x64 IL2CPP/only-D3D11 report `1346115186` bayt ve fatal-token `0` verdi. Intel Iris Xe/D3D11 exact r51 runtime host/readiness/success `1/1/1`, forbidden `0`, graceful shutdown, deleted task ve scoped residue `0` ile geçti.
- Foreground Session-2 Win32 OS input W/A/S/D `1/1`, relative mouse `18/18`, combined W+D+mouse `3/3` ve held mouse `30/30` zincirini doğruladı. Windows final audit `27/27` check geçti; exact evidence tar Mac'e `6587392` bayt / `c15e21ff…44a0` readback ile döndü. Claim `human=false`; fiziksel keyboard/gamepad/endurance Steam 1.0 sertifikasyonunda kalır. USB bulunmadı/yazılmadı.
- ADR-0060/tarihli Evidence exact kanıtları bağlar. Source/docs `988591c18dd5fbbdcb2f16146cc1330daec87657`/Guard `33029851072`, final PR head `7ee80bba8964ccfb8edf1c3f06d89ac293fdc1a0`/Guard `33029974821`, PR #103 merge `a66c19be79b9265d1a01ff1127373136146fcd1e` ve main Guard `33030020415` geçti. Acceptance `27/27`, Issue `CLOSED`, Roadmap `Done`; parent Epic #10 ve Steam 1.0 Goal sürer.
- Tek kanonik iletişim merkezi ve yazma hattı bu Game projesine bağlı ana görevdir. Ürün kuzey yıldızı Dashboard parity'yi fiziksel 3D oynanışa, küçük→büyük mağazalara, çalışan/müşterilere, işlevsel mahalle/kişisel ev/araç-lojistiğe, dünya NPC ekolojisine ve retail çekirdekten ayrılmış güvenli offline Local Advisor/Guardian sınırına bağlar. Kontrolsüz self-modifying code yoktur; Mac tek write lane, Windows clean native worker'dır.

### Önceki üstün gelen durum — Issue #99 Graphics Card BuildKit→PCIe x16 Retention Assembly exact kapanışı

- Exact reserved graphics card yalnız owned work order/ticket/allocation içindeki tam line/product/item/reservation/staging-receipt lineage ile seçilir; historical ten-receipt `10/10` aggregate, live Issue #89 secured motherboard, #91 retained CPU, #93 retained A2 DDR5, #95 secured primary M.2 ve #97 retained processor-cooler receipts sonrasında ayrı stable operation exact Graphics Card BuildKit → ActorHands → existing GraphicsCardSlot custody'sini açar.
- Inventory yalnız kayıtlı GPU BuildKit release ve exact GraphicsCardSlot↔Hands reversible transferini kabul eder. Same Unity instance/stable ItemId, live reservation/allocation, immutable staging history ve exact replay/revision korunur; installed prerequisites ve other-four untouched, generic transfer/drop/box/stack/cart ve receipt-free Assembly bypass fail-closed'dur.
- GarageGraybox r50 keyboard/mouse + gamepad pickup, existing PCIe x16 guided preview, invalid-orientation block, seat, slot-latch + rear-bracket retain, retained-remove block, unretain, detach ve reseat akışını single-consumer input ile taşır. Current obstruction recovery'yi fail-closed tutar; clear sonrasında same-instance exactly-once recovery geçer.
- Existing Issue #59 compatibility/orientation/seat/latch/rear-bracket/replay authority tek GPU Assembly gerçeğidir. Issue #63 PCIe power-cable item/product/container/state/revision/receipt/operation authority exact untouched kalır; routed cable removal'ı bloklamaya devam eder ve Issue #99 route üretmez.
- Exact hardening commit `034f862cfdc85b93e44cc0c9dded26aafdffbee6`, tree `191e9e1bfd85ef20c000fc171523c1861f3ecb21`; Mac kritik PlayMode `8/8`, full EditMode `733/733`, izole full PlayMode `140/140`; Windows kritik `8/8`, full EditMode `733/733`, full PlayMode `140/140`; Universal Mac report `330252284` bayt ve source Guard `33015982332` başarılıdır.
- Clean Windows x64 IL2CPP/only-D3D11 report `1350304438` bayt verdi. Intel Iris Xe/D3D11 exact r50 runtime host/readiness/success `1/1/1`, exit `0`, graceful shutdown ve exact process/task residue `0` ile geçti. Foreground Session-2 Win32 scan-code acceptance S/D, relative mouse ve W+D-held + mouse zincirini gerçek player penceresinde doğruladı.
- ADR-0059 ve 27 Ağustos closure addendum'u exact Mac/Windows/OS-input hashlerini bağlar. Bir transient scene-import full koşusu `137/140` olarak korunur; exact isolation `3/3` ve dış etkisiz full tekrar `140/140` geçti, scene blob'u HEAD ile byte-exact kaldı. Claim `human=false`; fiziksel keyboard/gamepad ve 15 dakikalık insan turu Steam 1.0 yayın sertifikasyonunda açık kalır. Önceki `d5532bb`/`0f25960` local ve sağlıklı USB checkpoint geçmişi korunur, exact `034f862` USB diye sunulmaz ve bu kapanışta USB yazımı yapılmaz.
- Closure docs `ba07775fa1ee7dadd0a7485533b80fa7f6eaf125`, PR Guard `33021624279`, PR #100 merge `9b3d2f22b20bc04159236f2571bfe2c6b6471886` ve `main` Guard `33021671295` ile authoritative Git zincirine girdi. Bu kayıt bounded Issue #99 Issue/Roadmap kapanışını yetkilendirir; parent Epic #10 ve Steam 1.0 hedefi sürer.
- Tek kanonik iletişim merkezi ve yazma hattı bu Game projesine bağlı ana görevdir. Ürün kuzey yıldızı Dashboard parity'yi fiziksel 3D oynanışa, küçük→büyük mağazalara, çalışan/müşterilere, işlevsel mahalle/kişisel ev/araç-lojistiğe, dünya NPC ekolojisine ve retail çekirdekten ayrılmış güvenli offline Local Advisor/Guardian sınırına bağlar. Kontrolsüz self-modifying code yoktur; Mac tek write lane, Windows clean native worker'dır.

### Önceki üstün gelen durum — Issue #97 Processor Cooler BuildKit→Four-Point Retention Assembly

- Exact reserved processor cooler yalnız owned work order/ticket/allocation içindeki tam line/product/item/reservation/staging-receipt lineage ile seçilir; historical ten-receipt `10/10` aggregate, live Issue #89 secured motherboard, #91 retained CPU, #93 retained A2 DDR5 ve #95 secured primary M.2 receipts sonrasında ayrı stable operation exact ProcessorCooler BuildKit → ActorHands → existing ProcessorCoolerSlot custody'sini açar.
- Inventory yalnız kayıtlı cooler BuildKit release ve exact ProcessorCoolerSlot↔Hands reversible transferini kabul eder. Same Unity instance/stable ItemId, live reservation/allocation, immutable staging history ve exact replay/revision korunur; installed prerequisites ve other-five untouched, generic transfer/drop/box/stack/cart ve receipt-free Assembly bypass fail-closed'dur.
- GarageGraybox r49 keyboard/mouse + gamepad pickup, existing two-orientation guided seat, pre-applied TIM consume-once, `1→3→2→4` retain, retained-remove block, `4→2→3→1` unretain ve detach akışını single-consumer input ile taşır. Consumed-TIM reseat atomik reddedilir ve aynı cooler ellerde kalır.
- Bounded geometri görevi gerçek M.2/cooler collider kesişmesini buldu. M.2 authored plane düzeltildi, geçici collision exemption kaldırıldı ve iki 180° orientation actual collider separation ile doğrulandı. Bounded invariant görevi operation/receipt/replay/revision/custody ayrımında P0/P1 bulmadı.
- Technical commit `b45806f5a584d219de74be33ed97a580af59fd68`, tree `6f62c8653ad2c8505e2927ecc80ac6987399e232`; full EditMode `726/726`, PlayMode `133/133`; Universal Mac report `330220810` bayt, Apple M1/Metal exact r49 smoke ve technical Guard `32973861692` başarılıdır.
- Complete bundle `7630681` bayt / `2751e62e…3537` ile collision-free detached-clean Windows `issue97-b45806f5a584-r1` x64 IL2CPP/only-D3D11 report `1344385080` bayt ve fatal-token `0` verdi. Intel Iris Xe/D3D11 exact r49 runtime host/readiness/success `1/1/1`, forbidden `0`, exit `0`, graceful shutdown, task deletion ve exact residue `0` ile geçti.
- ADR-0058/tarihli Evidence ve `issue97` verifier contract'ı Mac+Windows teknik kanıtını bağlar. Canonical evidence `13/14`; source/docs Guard, final receipt `14/14`, immutable local/healthy physical USB ve exact-r49 insan oturumu bekler. Strict acceptance `29/30`; Issue #97 ve draft PR #98 açık/In Progress kalır.
- Ürün kuzey yıldızı Dashboard parity'yi fiziksel 3D oynanışa, küçük→büyük mağazalara, çalışan/müşterilere, işlevsel mahalle/kişisel ev/araç-lojistiğe, dünya NPC ekolojisine ve retail çekirdekten ayrılmış güvenli offline Local Advisor/Guardian sınırına bağlar. Kontrolsüz self-modifying code yoktur; Mac tek write lane, Windows clean native worker'dır.

### Önceki üstün gelen durum — Issue #95 M.2 BuildKit→Primary Slot/Captive Screw Assembly

- Exact reserved M.2 2280 NVMe yalnız owned work order/ticket/allocation içindeki tam line/product/item/reservation/staging-receipt lineage ile seçilir; historical ten-receipt `10/10` aggregate, live Issue #89 secured-motherboard, Issue #91 retained-CPU ve Issue #93 retained-A2-DDR5 receipts sonrasında ayrı stable operation exact Storage BuildKit → ActorHands → existing primary M.2 Slot custody'sini açar.
- Inventory yalnız kayıtlı Storage BuildKit release ve exact M.2 Slot↔Hands reversible transferini kabul eder. Same Unity instance/stable ItemId, live reservation/allocation, immutable staging history ve exact replay/revision korunur; motherboard/CPU/DDR5 ve other-six untouched, generic transfer/drop/box/stack/cart ve receipt-free Assembly bypass fail-closed'dur.
- GarageGraybox r48 keyboard/mouse + gamepad pickup, existing M-key primary-slot 18° guided insertion, flat seat, captive-screw tighten, secured-remove block, loosen, detach ve reseat akışını single-consumer input ile taşır. Motherboard detach/unsecure storage seated/retained iken; storage remove captive screw tight iken fail-closed kalır ve CPU/DDR5 state değişmez.
- Pickup physical projection'ı reversibly stage eder, held-input state yayınlamadan exact authority commit eder; authority rejection aynı instance'ı exact BuildKit safe pose'una döndürür. Fault-boundary testleri stage failure ve authority-rejection rollback için duplicate/ghost/loss `0` kanıtlar. Bu, Issue #95'in eski domain-first taslak cümlesini observable atomicity sözleşmesiyle düzeltir.
- Yardımcı input denetimleri pause/focus resume lurch'ünü ve karşıt keyboard controls aggregate-zero köşe durumunu buldu. Final teknik çözüm resolved Move controls'ü ayrı ayrı denetler; neutral olmadan held hareket/look sızmaz, fresh pointer delta korunur.
- Technical commit `42c1ae4dff2421b38879c0bfc82b4bf52522be1e`, tree `16304340da0ae7e42d8e7dd1ea6aef66ffe27efc`; full EditMode `722/722`, PlayMode `130/130`; Universal Mac report `330195891` bayt, Apple M1/Metal exact r48 smoke ve technical Guard `32962078481` başarılıdır.
- Complete bundle `7626490` bayt / `f11c846b…3748` ile collision-free detached-clean Windows `issue95-42c1ae4-opposing-neutral-v3` x64 IL2CPP/only-D3D11 report `1343654204` bayt ve fatal-token `0` verdi. Intel Iris Xe Direct3D 11.0 level 11.1 exact r48 runtime host/readiness/success `1/1/1`, forbidden `0`, exit `0`, graceful shutdown, task deletion ve player/Unity/task residue `0` ile geçti.
- ADR-0057/tarihli Evidence ve `issue95` verifier contract'ı Mac+Windows teknik kanıtını bağlar. Canonical evidence `13/14`; source/docs Guard, final receipt `14/14`, immutable local/healthy physical USB ve exact-r48 insan oturumu bekler. Strict acceptance `26/27`; Issue #95 ve draft PR #96 açık/In Progress, `Warning / Full Repair Needed` Windows D: USB read-only kalır.
- Ürün kuzey yıldızı Dashboard parity'yi fiziksel 3D oynanışa, küçük→büyük mağazalara, çalışan/müşterilere, işlevsel mahalle/kişisel ev/araç-lojistiğe, dünya NPC ekolojisine ve retail çekirdekten ayrılmış güvenli offline Local Advisor/Guardian sınırına bağlar. Kontrolsüz self-modifying code yoktur; Mac tek write lane, Windows clean native worker'dır.

### Önceki üstün gelen durum — Issue #93 DDR5 BuildKit→A2 Dual-Latch Assembly

- Exact reserved DDR5 UDIMM yalnız owned work order/ticket/allocation içindeki tam line/product/item/reservation/staging-receipt lineage ile seçilir; historical ten-receipt `10/10` aggregate, live Issue #89 secured-motherboard ve Issue #91 retained-CPU receipts sonrasında ayrı stable operation exact BuildKit → ActorHands → existing A2 MemorySlot custody'sini açar.
- Inventory yalnız kayıtlı DDR5 BuildKit release ve exact A2 MemorySlot↔Hands reversible transferini kabul eder. Same Unity instance/stable ItemId, live reservation/allocation, immutable staging history, exact replay/revision ve domain-first recovery korunur; motherboard/CPU ve other-seven untouched, generic transfer/drop/box/stack/cart ve receipt-free Assembly bypass fail-closed'dur.
- GarageGraybox r47 keyboard/mouse + gamepad pickup, existing notch-aligned A2 seat, dual-latch close→open, detach ve reseat akışını single-consumer input ile taşır. Motherboard detach/unsecure DIMM seated/retained iken; DIMM remove latch closed iken fail-closed kalır ve CPU retained state değişmez.
- Technical commit `0caca090d2859dfb78219abb089274fe599eaca2`, tree `e52c75872a8ec59a98b63c0c46d5e3f6f9c5e084`; full EditMode `718/718`, PlayMode `125/125`; Universal Mac report `330173019` bayt, Apple M1/Metal exact r47 smoke ve technical Guard `32946849858` başarılıdır.
- Complete bundle `7594847` bayt / `039ec06b…1572` ile collision-free detached-clean Windows `issue93-0caca09-hardened-v1` x64 IL2CPP/only-D3D11 report `1342974093` bayt ve fatal-token `0` verdi. Intel Iris Xe Direct3D 11.0 level 11.1 exact r47 runtime host/readiness/success `1`, forbidden `0`, exit `0`, graceful shutdown, task deletion ve player/Unity/task residue `0` ile geçti.
- ADR-0056/tarihli Evidence ve `issue93` verifier contract'ı Mac+Windows teknik kanıtını bağlar. Canonical evidence `13/14`; source/docs Guard, final receipt `14/14`, immutable local/healthy physical USB ve exact-r47 insan oturumu bekler. Strict acceptance `25/26`; Issue #93 ve draft PR #94 açık/In Progress, Dirty/`Full Repair Needed` Windows D: USB read-only kalır.
- Ürün kuzey yıldızı Dashboard parity'yi fiziksel 3D oynanışa, küçük→büyük mağazalara, çalışan/müşterilere, işlevsel mahalle/kişisel ev/araç-lojistiğe, dünya NPC ekolojisine ve retail çekirdekten ayrılmış güvenli offline Local Advisor/Guardian sınırına bağlar. Kontrolsüz self-modifying code yoktur; Mac tek write lane, Windows clean native worker'dır.

### Önceki üstün gelen durum — Issue #91 CPU BuildKit→Processor Socket/Retention Assembly

- Exact reserved CPU yalnız owned work order/ticket/allocation içindeki tam line/product/item/reservation/staging-receipt lineage ile seçilir; authoritative historical ten-receipt `10/10` aggregate, live Issue #89 motherboard handoff, exact Workbench custody, `SeatedSecured` ve source attach/secure receipts sonrasında ayrı stable operation exact BuildKit → ActorHands → existing ProcessorSocket custody'sini açar.
- Inventory yalnız kayıtlı Processor BuildKit release ve exact ProcessorSocket↔Hands reversible transferini kabul eder. Same Unity instance/stable ItemId, live reservation/allocation, immutable staging history, exact replay/revision ve domain-first recovery korunur; secured motherboard ve other-eight untouched, generic transfer/drop/box/stack/cart ve receipt-free Assembly bypass fail-closed'dur.
- GarageGraybox r46 keyboard/mouse + gamepad pickup, existing keyed processor seat, retention close→open, detach ve reseat akışını single-consumer input ile taşır. Motherboard unsecure/detach CPU seated/retained iken; CPU remove retention closed iken fail-closed kalır.
- Technical commit `003c93f2de191ff3b295a8a88454e74617521970`, tree `1e46049a9a253559b2f9f4ab41524e8be5e0f9ab`; full EditMode `715/715`, PlayMode `122/122`; Universal Mac report `330127900` bayt, Apple M1/Metal exact r46 smoke ve technical Guard `32937325469` başarılıdır.
- Detached-clean Windows `issue91-hardened-v2` x64 IL2CPP report `1342422475` bayt ve Intel Iris Xe Direct3D 11.0 level 11.1 exact r46 runtime ile geçti; graceful exit, task deletion, player/Unity/build-task residue `0`, checkout exact-head clean kaldı. ADR-0055/tarihli Evidence ve `issue91` verifier contract'ı Mac+Windows teknik kanıtını bağlar. Canonical teknik evidence `13/14`; source/docs Guard, final canonical `14/14`, immutable local/healthy physical USB ve exact-r46 insan oturumu bekler; strict acceptance `24/25`, Issue #91 ve draft PR #92 açık/In Progress kalır.
- Ürün kuzey yıldızı Dashboard parity'yi fiziksel 3D oynanışa, küçük→büyük mağazalara, çalışan/müşterilere, işlevsel mahalle/kişisel ev/araç-lojistiğe, dünya NPC ekolojisine ve retail çekirdekten ayrılmış güvenli offline Local Advisor/Guardian sınırına bağlar. Kontrolsüz self-modifying code yoktur; Mac tek write lane, Windows clean native worker'dır.

### Önceki üstün gelen durum — Issue #89 Motherboard BuildKit→Chassis Assembly

- Exact reserved motherboard yalnız owned work order/ticket/allocation içindeki tam line/product/item/reservation lineage ile seçilir; authoritative historical ten-receipt `10/10` aggregate sonrasında ayrı stable operation exact BuildKit → ActorHands → existing Assembly Workbench custody'sini açar.
- Inventory yalnız kayıtlı BuildKit release ve exact Workbench↔Hands reversible transferini kabul eder. Same Unity instance/stable ItemId, live reservation/allocation, immutable staging history, exact replay/revision ve domain-first recovery korunur; other-nine untouched, generic transfer/drop/box/stack/cart ve receipt-free Assembly bypass fail-closed'dur.
- GarageGraybox r45 keyboard/mouse + gamepad pickup, existing guided seat, canonical fastener secure→unsecure, detach ve reseat akışını single-consumer input ile taşır. CPU/DIMM/storage/cooler/GPU/PSU/cable installation secure motherboard öncesinde bloklu kalır.
- Technical commit `2fdf371206bc58c32e1c20d471f4abe7c0bfba01`, tree `c5e6de5942993a98735984caca4a04fd396105f6`; full EditMode `712/712`, PlayMode `119/119`; Universal Mac report `330104684` bayt, Apple M1/Metal exact r45 smoke ve technical Guard `32930403290` başarılıdır.
- Complete bundle ile collision-free detached-clean Windows x64 IL2CPP/only-D3D11 report `1340592635` bayt, `issue89-hardened-v1` fatal-token `0`; Intel Iris Xe/Direct3D 11.0 feature level 11.1 exact runtime, graceful exit, task deletion ve player/Unity/task residue `0` ile geçti.
- ADR-0054/tarihli Evidence ve `issue89` verifier contract'ı teknik kanıtı bağlar. Source/docs Guard, final canonical `14/14`, immutable local/healthy physical USB ve exact-r45 insan oturumu bekler; strict acceptance `22/25`, Issue #89 ve draft PR #90 açık/In Progress kalır.
- Ürün kuzey yıldızı Dashboard parity'yi fiziksel 3D oynanışa, küçük→büyük mağazalara, çalışan/müşterilere, işlevsel mahalle/kişisel ev/araç-lojistiğe, dünya NPC ekolojisine ve retail çekirdekten ayrılmış güvenli offline Local Advisor/Guardian sınırına bağlar. Kontrolsüz self-modifying code yoktur; Mac tek write lane, Windows clean native worker'dır.

### Önceki üstün gelen durum — Issue #81 Power Supply BuildKit

- Exact reserved `PowerSupply`, staged motherboard+CPU+DDR5+M.2+processor-cooler+graphics-card prerequisites sonrasında ayrı stable operation ve capacity-one managed BuildKit slotuyla source → ActorHands → power-supply BuildKit custody'sine taşınır; visible work-ticket `6/10 → 7/10` olur.
- Domain commit world mutation'dan önce gelir. Aynı Unity instance/stable ItemId, canlı reservation/allocation, exact replay ve stable recovery korunur; generic transfer/drop/box/stack/cart/Assembly bypass'ları kapalıdır.
- GarageGraybox r41 gerçek keyboard/mouse + gamepad, keyed `0° ↔ 180°` preview ve tek-consumer BuildKit/PSU-bay/cable-route input arbitration taşır. Issue #60 PSU bay/retention ile Issue #61–#63 ATX24/EPS12V/PCIe route state/revision/receipts değişmez.
- Native prerequisite harness production Update order'ını atlayan forced `InputSystem.Update()` + doğrudan station işleme kısayolundan arındırıldı; neutral → pressed → released gerçek player frame'leri ve release öncesi pressed-frame diagnostics kullanılır.
- Technical commit `f3d80629e09c05afde97fa778c4b220ca456c5f0`, tree `851954879c1ff1e2ef98bc9a7a8469750304d992`; EditMode `697/697`, PlayMode `105/105`; Universal Mac report `329907140` bayt ve Apple M1/Metal exact r41 smoke başarılıdır.
- Collision-free detached-clean exact-head Windows x64 IL2CPP/only-D3D11 report `1335888266` bayt, `issue81-hardened-v1` fatal-token `0`; Intel Iris Xe/Direct3D 11.0 feature level 11.1 runtime exact host/readiness/success `1`, forbidden `0`, exit `0`, graceful shutdown, task deleted ve player/Unity/task residue `0` ile geçti.
- On üç immutable evidence artifact'ı Mac'e exact readback ile döndü. Clean source/docs `dc118bf0d26a11f3937cb114ef12f85666facc48`, tree `ac9fcb5d38855ed37f2ee36449100b5094287cb8` ve Guard `32896033674` sonrasında source receipt üretildi; canonical evidence exact `14/14` oldu.
- Immutable local final ve doğru Windows-attached fiziksel USB incoming/final hedefleri aynı `1002/1002` payload, `987/987` exact Git source, `14/14` evidence, `19368668` bayt ve `69cc892b…06ab` manifest sonucunu verdi. Incoming/AppleDouble/final-sidecar ve Windows işlem artığı `0`dır. Fiziksel metadata `ff935452c68bc77e66eb0742e0c3e6c0eb2894c7`, tree `e4f03fc3c2d6dfd44da61eaae3a161af4f104eae`, Guard `32897672990` başarılıdır. Exact r41 insan oturumu kaydedilmediği için acceptance `23/24`, Issue #81 açık/In Progress ve PR #82 taslak kalır; parent Epic #10 açık/In Progress kalır. Güncel ayrıntı ADR-0050, tarihli Evidence ve `10_DEVAM_CHECKPOINT.md` içindedir.

### Önceki üstün gelen durum — Issue #79 Graphics Card BuildKit

- Exact reserved `GraphicsCard`, staged motherboard+CPU+DDR5+M.2+processor-cooler prerequisites sonrasında ayrı stable operation ve capacity-one managed BuildKit slotuyla source → ActorHands → graphics-card BuildKit custody'sine taşınır; visible work-ticket `5/10 → 6/10` olur.
- Domain commit world mutation'dan önce gelir. Aynı Unity instance/stable ItemId, canlı reservation/allocation, exact replay ve stable recovery korunur; generic transfer/drop/box/stack/cart/Assembly bypass'ları kapalıdır.
- GarageGraybox r40 gerçek keyboard/mouse + gamepad, keyed 180° half-turn preview ve tek-consumer BuildKit/GPU-seat/PCIe-route input arbitration taşır. Issue #59 GPU seat/latch/rear-bracket ve Issue #63 PCIe route state/revision/receipts değişmez.
- Technical commit `f40ef21058caf1a2aca3054218abfc1dd7305c01`, tree `c7500e7300f75f5d9b089bf23657750dccc5ffed`; EditMode `690/690`, PlayMode `100/100`; Universal Mac report `329839788` bayt ve Apple M1/Metal exact r40 smoke başarılıdır.
- Collision-free detached-clean exact-head Windows x64 IL2CPP/only-D3D11 report `1334256694` bayt, `issue79-hardened-v3` fatal-token `0`; Intel Iris Xe/Direct3D 11.0 feature level 11.1 runtime exact host/readiness/success `1`, forbidden `0`, exit `0`, graceful shutdown, task deleted ve residue `0` ile geçti.
- On üç immutable evidence artifact'ı Mac'e exact readback ile döndü. Clean source/docs `dd607d0af346bd1f0e28449f606761bc97e1495c`, tree `010b3a460c3241ed69d315bfb44047c1be82cb10` ve Guard `32874685021` sonrasında source receipt üretildi; canonical evidence exact `14/14` oldu.
- Immutable local final ve doğru Windows-attached fiziksel USB incoming/final hedefleri aynı `990/990` payload, `975/975` exact Git source, `14/14` evidence, `20086932` bayt ve `d2d399fa…b324` manifest sonucunu verdi. Incoming/AppleDouble/final-sidecar `0`dır. Fiziksel metadata `880523fcb71208796cce96564556a2170363c92a` ve Guard `32876194890` başarılıdır; acceptance `24/24`, Issue #79 `CLOSED`, Roadmap `Done`, Issue #77 ve parent Epic #10 açık/In Progress kalır. Güncel ayrıntı ADR-0049, tarihli Evidence ve `10_DEVAM_CHECKPOINT.md` içindedir.

### Önceki üstün gelen durum — Issue #77 Processor Cooler BuildKit

- Exact reserved `ProcessorCooler`, staged motherboard+CPU+DDR5+M.2 prerequisites sonrasında ayrı stable operation ve capacity-one managed BuildKit slotuyla source → ActorHands → processor-cooler BuildKit custody'sine taşınır; visible work-ticket `4/10 → 5/10` olur.
- Domain commit world mutation'dan önce gelir. Aynı Unity instance/stable ItemId, canlı reservation/allocation, exact replay ve stable recovery korunur; generic transfer/drop/box/stack/cart/Assembly bypass'ları kapalıdır.
- GarageGraybox r39 gerçek keyboard/mouse + gamepad, keyed 90° quarter-turn preview ve tek-consumer BuildKit/cooler-seat input arbitration taşır. Issue #58 four-point-retention/TIM Assembly state/revision/receipts değişmez.
- Technical commit `197233688c4fe587097dbfc1cbee843cfc78603e`, tree `58458f400a7efaa68e452a0e85e35d6d7eb5a3ab`; EditMode `686/686`, PlayMode `96/96`; Universal Mac report `329787583` bayt ve Apple M1/Metal exact r39 smoke başarılıdır.
- Collision-free detached-clean exact-head Windows x64 IL2CPP/only-D3D11 report `1333221634` bayt, `issue77-hardened-v2` fatal-token `0`; Intel Iris Xe/Direct3D 11.0 feature level 11.1 runtime exact host/readiness/success `1`, forbidden `0`, exit `0`, graceful shutdown, task deleted ve residue `0` ile geçti.
- On üç immutable evidence artifact'ı Mac'e exact readback ile döndü. Exact dokuz dosyalık source/docs commit/Guard, final receipt `14/14`, immutable local+physical USB double-readback, final metadata/Guard ve acceptance `24/24` henüz bekliyor. Issue #77 açık/In Progress; parent Epic #10 açık/In Progress kalır. Güncel ayrıntı ADR-0048, tarihli Evidence ve `10_DEVAM_CHECKPOINT.md` içindedir.

### Önceki üstün gelen durum — Issue #75 M.2 NVMe Storage BuildKit

- Exact reserved M.2 NVMe `StorageDevice`, staged motherboard+CPU+DDR5 prerequisites sonrasında ayrı stable operation ve capacity-one managed BuildKit slotuyla source → ActorHands → Storage BuildKit custody'sine taşınır; visible work-ticket `3/10 → 4/10` olur.
- Domain commit world mutation'dan önce gelir. Aynı Unity instance/stable ItemId, canlı reservation/allocation, exact replay ve stable recovery korunur; generic transfer/drop/box/stack/cart/Assembly bypass'ları kapalıdır.
- GarageGraybox r38 gerçek keyboard/mouse + gamepad, 180° keyed preview ve tek-consumer BuildKit/M.2-seat input arbitration taşır. Issue #57 M.2 guided insertion/captive-screw Assembly state/revision/receipts değişmez.
- Technical commit `646e66cfa269a217ecb1f6942f9accb77f9e463c`, tree `ee9b0b2c0bb5e1fb07de397da222d00a7480b23c`; EditMode `683/683`, PlayMode `90/90`; Universal Mac report `329735698` bayt ve Apple M1/Metal exact r38 smoke başarılıdır.
- Collision-free detached-clean exact-head Windows x64 IL2CPP/only-D3D11 report `1332182927` bayt, `issue75-hardened-v2` fatal-token `0`; Intel Iris Xe/Direct3D 11.0 feature level 11.1 runtime exact host/readiness/success `1`, forbidden `0`, exit `0`, graceful shutdown, task deleted ve residue `0` ile geçti.
- On üç immutable evidence artifact'ı Mac'e exact readback ile döndü. Clean source/docs `af6578aa224b931fdcfdd6293dccfcfd77a29eac`, tree `39ec1c0573223899d2982f72fb877dbea58306ba` ve Guard `32849988087` sonrasında source receipt üretildi; canonical evidence exact `14/14` oldu.
- Immutable local final ve doğru Windows-attached fiziksel USB incoming/final hedefleri aynı `966/966` payload, `951/951` exact Git source, `14/14` evidence, `19598907` bayt ve `958ba6bc…f9d2b` manifest sonucunu verdi. Incoming/AppleDouble/final-sidecar `0`dır. Fiziksel metadata `b113c86f5c2b375b0bc31081a5764fe264c2af9d` ve Guard `32851553662` başarılıdır; acceptance `23/23`, Issue #75 `CLOSED`, Roadmap `Done`, parent Epic #10 açık/In Progress kalır. Güncel ayrıntı ADR-0047, tarihli Evidence ve `10_DEVAM_CHECKPOINT.md` içindedir.

### Önceki üstün gelen durum — Issue #73 DDR5 DIMM BuildKit

- Exact reserved DDR5 `MemoryModule`, staged motherboard+CPU prerequisites sonrasında ayrı stable operation ve capacity-one managed BuildKit slotuyla source → ActorHands → memory-module BuildKit custody'sine taşınır; visible work-ticket `2/10 → 3/10` olur.
- Domain commit world mutation'dan önce gelir. Aynı Unity instance/stable ItemId, canlı reservation/allocation, exact replay ve stable recovery korunur; generic transfer/drop/box/stack/cart/Assembly bypass'ları kapalıdır.
- GarageGraybox r37 gerçek keyboard/mouse + gamepad, 180° keyed preview ve tek-consumer BuildKit/A2 input arbitration taşır. Issue #56 A2/dual-latch Assembly state/revision/receipts/latch state değişmez.
- Technical commit `a2df663d6fa0e9d2004697bfb038a65a5e6c3d81`, tree `e32a8e143049c4059e402bafbfcd39b9760cd025`; EditMode `680/680`, PlayMode `86/86`; Universal Mac report `329681642` bayt ve Apple M1/Metal exact r37 smoke başarılıdır. Initial Guard `32839956810` geçti.
- Collision-free detached-clean exact-head Windows x64 IL2CPP/only-D3D11 report `1330930513` bayt, hardened-v2 fatal-token `0`; Intel Iris Xe/Direct3D 11.0 feature level 11.1 runtime exact host/readiness/success `1`, forbidden `0`, exit `0`, graceful shutdown, task deleted ve residue `0` ile geçti.
- On üç immutable evidence artifact'ı Mac'e exact readback ile döndü. Clean source/docs `e45f6e1b463cbe9686a9c349d0c6912a9657a28e`, tree `16f014a807a7733210bc9197981b4a8608c3d687` ve Guard `32841321015` sonrasında source receipt üretildi; canonical evidence atomik exact `14/14` oldu.
- Immutable local final ve doğru Windows-attached fiziksel USB incoming/final hedefleri aynı `954/954` payload, `939/939` exact Git source, `14/14` evidence, `19379146` bayt ve `912e35ff…e9cc8` manifest sonucunu verdi. Incoming/AppleDouble/final-sidecar `0`dır. Fiziksel metadata `28df8283b7fa5187fa1a0dd6ec72acebd6d539d4` ve Guard `32842669488` başarılıdır; acceptance `23/23`, Issue #73 `CLOSED`, Roadmap `Done`, parent Epic #10 açık/In Progress kalır. Güncel ayrıntı ADR-0046, tarihli Evidence ve `10_DEVAM_CHECKPOINT.md` içindedir.

## 1. Konsolidasyon kararı ve görev sınırı

Kullanıcı şu kararları açıkça onayladı:

- Yalnız `PC Shop Empire Similator` altında görünen üç Codex görevi birleştirilecektir.
- Güncel geliştirme görevi tek ana görev olarak kalacaktır.
- Eski ana planlama görevi ve birleştirme görevi, aktarım doğrulandıktan sonra arşivlenecektir; kalıcı olarak silinmeyecektir.
- Bütün kullanıcı/Codex konuşmaları, üretilen/değiştirilen dosyalar, kararlar, tamamlanan ve yapılacak işler merkezî arşive aktarılacaktır.
- Projenin bundan sonraki çalışması tek Codex kanalı üzerinden sürdürülecektir.
- Sistem/developer talimatları, iç düşünce zincirleri, ham kimlik doğrulama verileri ve güvenlik açısından taşınmaması gereken token/parola çıktıları aktarılmaz. Bunların proje üzerinde oluşturduğu sonuçlar aktarılır.

Birleştirilen görevler:

1. `019fec8c-cae9-7973-9ca2-33663c84e991` — uzun vadeli vizyon, araştırma, mimari, Stage A, deterministik Core, ilk oynanabilir garaj ve ilk fiziksel etkileşim geçmişi.
2. `019ff9d8-089c-71a1-93c5-8cb614d0b5ca` — Issue #6 altındaki placement, büyük kutu, rotation, lookdev ve stacking geliştirmeleri; bu görev artık ana görevdir.
3. `01a002ff-fbc6-74d1-819a-3844c98c6ce3` — kapsam belirleme, tam aktarım, ana görev seçimi ve arşivleme işlemi.

## 2. Projenin nihai gayesi

Mevcut PC Shop Empire, eski Electron/HTML tabanlı 2D yönetim oyunundan bağımsız olarak Unity 6 ve URP ile sıfırdan geliştirilen, büyük kapsamlı bir 3D bilgisayar mağazası ve teknoloji perakendesi simülasyonuna dönüşecektir.

Temel oyuncu fantezisi:

- Oyuncu küçük bir garajda sınırlı para, alan, stok ve ekipmanla başlar.
- Garajdan mahalle dükkânına, profesyonel mağazaya ve çok bölümlü büyük teknoloji işletmesine büyür.
- Mağazada birinci şahıs olarak yürür; görünür ellerle kutu, ürün ve PC parçalarını fiziksel olarak taşır.
- Sipariş verir, teslimat alır, stok alanını ve rafları düzenler, müşterilere satış yapar, kasayı ve servisi yönetir.
- Bilgisayarları tek düğmeyle menüden üretmez; fiziksel çalışma masasında parçaları seçer, takar, kablolar, test eder, paketler ve teslim eder.
- Çalışanlar satış, kasa, depo, teknisyenlik, temizlik, yönetim ve güvenlik gibi gerçek roller üstlenir.
- Müşteriler farklı bütçe, ihtiyaç, sabır, teknik bilgi, tercih ve memnuniyet davranışlarına sahip olur.
- Ekonomi; tedarikçi, stok, talep, fiyat, ürün eskimesi, garanti, iade, servis, ikinci el, reklam, itibar ve büyüme sistemleriyle birbirine bağlı çalışır.

İlham kaynaklarından yalnız tasarım ilkeleri alınır. Başka oyunların kodu, adı, görseli, sesi, arayüzü veya telifli özgün içeriği kopyalanmaz. Gerçek marka/model verisi doğrulanmadan kullanılmaz; özgün veya kurgusal içerik tercih edilir.

## 3. Kesinleşmiş deneyim kararları

- Kamera: birinci şahıs.
- Oyuncu gövdesi: en az görünür eller; ileride gelişmiş el modeli ve animasyon.
- Temel fiziksel işler 3D dünyada yapılır.
- Dashboard kaybolmaz; oyun içindeki fiziksel bilgisayar, tablet veya yönetim terminalinden açılan yönetim katmanı olur.
- Dashboard sipariş, stok, fiyat, finans, çalışan, görev, müşteri siparişi, reklam, anlaşma, kira/fatura/vergi, pazar ve servis yönetir.
- Dashboard fiziksel montaj, kutu taşıma, raf yerleştirme ve ürün teslimi yerine geçmez.
- Okunaklı yarı gerçekçi görsel yön kullanılır: gerçek oran, PBR yüzey, zemine oturan ışık, doğal ağırlık ve ölçülü stilizasyon.
- Mevcut primitive garaj, kutular ve eller final sanat değildir; mekanik ve kalite kanıtıdır.
- Ana ticari hedef Windows x64 ve Steam 1.0'dır.
- Geliştirme Mac üzerinde yapılabilir; gerçek Windows/DirectX/IL2CPP/Steam doğrulaması Faz 1 kapanmadan zorunludur.
- macOS sürümü Windows 1.0 sonrasındaki ayrı maliyet, signing ve notarization kapısıdır.
- Oyuncuyu yoran gereksiz mikro-yönetimden, tekdüze tekrardan ve gizli hileden kaçınılır.
- Guardian sistemi tanılama ve raporlama yapar; insan/Codex onayı olmadan üretim kodunu kendiliğinden değiştirmez.

## 4. Authoritative teknik temel

- Unity: `6000.3.21f1`.
- Render pipeline: URP `17.3.0`.
- Dil: C#.
- Core assembly: `PSE.Core`; Unity/Editor bağımlılığı yoktur.
- Gameplay sınırları: `PSE.World` ve `PSE.Presentation`.
- İlk oynanabilir sahne: `Assets/Scenes/Prototypes/GarageGraybox.unity`.
- Connected oyuncu prefabı: `Assets/Prefabs/Prototype/PlayerRig.prefab`.
- Legacy kaynak: `LegacyReference/PC-Shop-Empire-1.1.6/Source`; tasarım ve veri semantiği referansıdır, yeni Unity mimarisine doğrudan port edilmez.
- Git ve private GitHub tek authoritative sürüm kontrolüdür.
- Unity Version Control ilk uzak check-in bağlantı reseti nedeniyle tamamlanmadı; ikinci authoritative VCS sayılmaz.

Tamamlanmış Core sözleşmeleri:

- Scope tipli kararlı kimlikler ve canonical doğrulama.
- `Failure` ve `OperationResult`.
- Deterministik süre, timestamp, pause destekli simulation clock.
- Stable domain event type/ID, one-based sequence ve immutable envelope.
- PCG32 `pcg32-xsh-rr-64-32-v1`, golden vector, snapshot/restore ve bias'sız bounded integer.
- SHA-256 framed stream derivation `sha256-framed-be-pcg32-v1`.
- Correlation/direct-causation, global FIFO, breadth-first nested enqueue ve bounded in-memory dispatcher.

## 5. Tamamlanan üretim kilometre taşları

Korunan temel commit çizgisi:

- Stage A baseline: `b7ac8c36d9fb7be1eacf08b8f2b273d9c2574166`.
- Core assembly: `8ecb05df48257d22dc7f4549c8dbfe7b261772a9`.
- Stable identity/result: `4cd2d928dbfda1886632bacce4a141c2a43161df`.
- Deterministic time/event: `8af2ad3d05906839c4b607e4958650e723060465`.
- PCG32: `bbb3648c6e34eedd77e1bec948d5ee630f89679c`.
- Seed derivation: `43e92174ca3866dfde436fb180785a615772a886`.
- Event dispatcher hardening: `3d819e533fd3635bc9b32787730d6dd9be110875`.
- İlk oynanabilir garaj: `c7a3a26075998252d9ae8b88824d8285e5067069`.
- Güvenli fiziksel pickup/drop: `44b816289f942e57fc176b26b203711090d0e61c`.
- Kontrollü küçük-kutu placement: `720e6d4ac2b2afad9ee86f907c533cbabb1bf5ed`.
- Güvenli büyük-kutu taşıma: `e94419862b04f6f03f97ef2e43c9da393c5d30a9`.
- Deterministik placement rotation: `661f2dcc64246a8282fd63fbf303454ec856ea40`.
- Okunaklı yarı gerçekçi benchmark: `c7214afab81a360a3ca10a88cbdd29f67e741994`.
- Güvenli küçük-kutu stacking feature: `2e11e30a1a4b3435046ae18001004cacc170079e`.
- Stacking yaşayan checkpoint: `74070f7bbab041b1a978ef5f889f64b1cfcd6ff9`.
- Codex proje konsolidasyonu: `2c10873a7e6ec3984292418121bed19072dd6d79`.
- Yüklü taşıma arabası feature: `82bf74f90fd5bce9f4f17244aea6afde4a7ef2c1`.
- Atomik checkout fulfillment feature: `bb89b0c297400f6eed22407df76dc1c85912cd74`.
- Deterministik customer visit ve runtime NavMesh feature: `b37b056271fac317e99ec47df0833b8ef219cf83`.
- Atomik nakit checkout ve ilk Economy settlement feature: `547cf971882239c912d8221f344706afc993a37b`.
- Bounded tek-müşteri danışmanlık ve recommendation gate feature: `846eb5d9912150a6ef3aae9a37678d71348f92a3`.

Tamamlanan oynanabilir sistemler:

- CharacterController tabanlı birinci şahıs hareket.
- Klavye/fare ve gamepad Input System sözleşmesi.
- Rebind override store.
- FOV, hassasiyet, invert ve motion-reduce ayarları.
- Pause/cursor ve runtime-ready tanısı.
- Görünür prototip eller.
- İki metre hedef çözümleme, tek taşıma slotu ve stable item identity.
- Küçük kutuyu `E / Gamepad South` ile alma.
- Güvenli bırakma ve disable/world-floor recovery.
- Küçük kutu placement modu; grid/yaw snap, tam destek ve overlap doğrulaması.
- Yeşil/kırmızı ghost ve geçerli/engelli geri bildirimi.
- `R / Right Shoulder` ile deterministik 90° rotation.
- Büyük kutu için iki-el pozu, 0,65× hareket, sprint kilidi ve motion-safe FOV bedeli.
- Büyük kutu için gerçek boyuta göre fail-closed güvenli bırakma.
- Stable küçük kutu üzerinde merkez/90° snap, beş noktalı footprint, tek kat/tek üst ilişkisi ve dolu tabanı alma kilidi.
- Tek `LargeBox` kapasiteli stable platform arabasına hands→cart→hands transferi.
- Dört noktalı zemin desteği, swept obstruction, yüklü/boş hız profili, sprint kilidi, gerçek keyboard/gamepad kontrolü ve fail-closed cargo recovery.
- Tek referans garaj köşesinde bevel, prosedürel PBR yüzey, görev ışığı, ACES/bloom ve reflection probe.
- Unity-bağımsız `PSE.Actors` sınırında kararlı müşteri/intent/visit kimliği, immutable lifecycle state'i ve bounded command receipt ledger'ı.
- Garajda runtime-built NavMesh üzerinde giriş → RAF A göz atma → checkout bekleme → çıkış müşteri projection'ı.
- İki denemeli route fallback, patience/exit timeout, pause-safe `SimulationClock` ve stock/checkout/order authority izolasyonu.
- Current `Browsing` visit için canonical one-per-visit consultation receipt; `2,75 m` range, `24°` focus, LOS ve gerçek `E / Gamepad South` görüşmesi olmadan tek-offer `Buy/Leave` kararı açılmaz.
- Versioned tek-consumer Interact, explicit customer execution order ve owned runtime `InputActionAsset` clone yaşam döngüsü aynı basışın carry/pickup'a sızmasını veya source assetin bozulmasını engeller.

## 6. Konsolidasyon anındaki kesin durum — tarihsel baz

- Son doğrulanmış kaynak feature: `2e11e30a1a4b3435046ae18001004cacc170079e`.
- Son yaşayan checkpoint ve konsolidasyon öncesi HEAD: `74070f7bbab041b1a978ef5f889f64b1cfcd6ff9`.
- `main` ve `origin/main` eşittir.
- Çalışma ağacı temizdir.
- Issue #35 tamamlanmış ve `Done` durumundadır.
- EditMode: `131/131` geçti.
- PlayMode: `12/12` geçti.
- Universal macOS development build başarılıdır.
- Apple M4/Metal 1280×720 gerçek player smoke: `rotation=ok stacking=ok lookdev=ok`.
- Repository Guard run `31856764087` başarılıdır.
- Taşıma arabası kodu başlamamıştır.
- Konsolidasyon sırasında açılan geçici taslak ve Issue #36 tamamen kaldırılmıştır.
- Bu konsolidasyon belgeleri dışında kullanıcıya ait veya ilişkisiz açık değişiklik yoktur.

### Konsolidasyon sonrası güncel checkpoint

- Son doğrulanmış kaynak feature: `92a0f7b814ad5e597d8d4ca033f2e533f618f719`, tree `4150bd36fa65d4043061e5979e08efb502338fc6`; [Repository Guard 31892420515](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31892420515) başarılıdır.
- Issue #52 stable `world.checkout-station.garage-001` fiziksel checkout station ekledi. Station pause kapalı, `2,75 m` range, `24°` focus ve raycast LOS gerektirir; RAF A üzerindeki checkout/payment primary action bypass'ı kapalıdır.
- Yalnız exact matching current customer/visit/basket/offer/item/reservation/Buy-action provenance'ı ve `AwaitingCheckout` state'i station'ı yetkilendirir. Stale, foreign, historical veya forged/value-equal zincir bütün authority'lerde no-mutation fail-closed'dur.
- İlk `Mouse Left / Gamepad RT` edge'i immutable checkout snapshotını bir kez üretir; fiyat/currency/unit cost donar. Held/same-frame/replay ödeme değildir; release/repress sonrasındaki ikinci edge exact-cash Economy settlement'ını bir kez üretir.
- Canonical receipt exact settlement/transaction/completion/checkout/customer/payment/currency/amount/COGS/action/line/ledger/time provenance'ını kapılar. Stock projection ve customer fulfillment yalnız matching receipt sonrasında ilerler.
- Customer focus collider'ı trigger yapılarak station çevresindeki fiziksel player/NPC stall'ı kaldırıldı; consultation LOS trigger hedefini görür. Üç ardışık final customer smoke güvenli exit'i kanıtladı.
- EditMode `352/352`, gerçek Input System PlayMode `24/24` geçti; failed/skipped `0`. XML SHA-256 değerleri `c6bd6e4f…ac6d` ve `8c05afec…9230`dur.
- Universal macOS development build `327864494` bayt, Mach-O `x86_64 + arm64`; build log SHA-256 `c9a0780e…69c`, executable SHA-256 `cf66c67f…79b2`dir.
- Apple M4/Metal 1280×720 runtime markerı `garage-physical-checkout-station-r21-v1`dir. Stock r4 ve customer r6/r7/r8 smoke station access, shelf bypass, checkout-start, cash-payment, receipt/Economy/ledger, authority isolation, stock projection ve customer safe-exit kapılarını geçti.
- `GarageGraybox.unity` `1397931` bayt, SHA-256 `509e6c25…d3fe`dir. Primitive checkout terminali, müşteri ve büyük diagnostic/status textleri final POS/karakter/UI değildir.
- Source/docs `d6cd203c5b9837c8eecc63ee3974dd2e76351bdc`, tree `6d73d5ac6d675733c939f181d087da3aef90f496` ve [Repository Guard 31892875650](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31892875650) başarılıdır. USB 584/584 manifest, 576/576 exact Git source ve 7/7 evidence kapısını geçti. Issue #52 acceptance `17/17`, kapalı/Done; parent Epic #9 kapalı/Done'dır.

## 7. Sıradaki işler ve bağımlılık sırası

Issue #52 kaynak/test/build/runtime/CI/USB ve Issue metadata zinciri tamamlandı; acceptance `17/17`, kapalı/Done. Parent Epic #9 geniş kabulü de kapalı/Done'dır. Bu paragraf Epic #10 başlangıcındaki tarihsel sırayı korur.

26 Ağustos 2026 itibarıyla üstün gelen güncel durum: Epic #10 altında motherboard/CPU/DIMM/M.2/cooler/GPU/PSU/ATX24/EPS12V/PCIe-GPU 6+2 fiziksel authority dilimleri ile Issue #64 accepted request→immutable ten-line quote/BOM→exact reservation ve Issue #66 BuildOrder/WorkTicket→exact allocation sınırları korunur. Issue #68, #71, #73, #75, #77, #79, #81, #83, #85 ve #87 sırasıyla canonical on parçayı gerçek pickup/carry/placement ile ayrı capacity-one BuildKit slotlarına taşımış; work-ticket fiziksel staging ilerlemesi `0/10 → 10/10` olmuştur. Güncel Issue #89 technical source `2fdf371206bc58c32e1c20d471f4abe7c0bfba01` aynı reserved motherboard instance'ını historical `10/10` BuildKit'ten ActorHands ve existing chassis Assembly Workbench'e taşır; guided seat, secure→unsecure→detach→reseat, live reservation/allocation, immutable history ve other-nine untouched sınırlarını korur. Fresh `712/712` EditMode, `119/119` PlayMode, Universal Mac/Apple M1 Metal r45, technical Guard `32930403290` ve Windows `issue89-hardened-v1` IL2CPP/Intel Iris Xe D3D11 r45 native kapıları geçmiştir. Source/docs Guard, final provenance `14/14`, immutable local+sağlıklı fiziksel USB ve exact-r45 insan kabulü bekler; strict acceptance `22/25`, Issue #89 açık/In Progress durumundadır. Motherboard Assembly handoff tamamlanması diğer component installation veya electrical readiness değildir; CPU/DIMM/storage/cooler/GPU/PSU install, cable routing, power-on/POST/OS/benchmark, Save/Guardian, işlevsel mahalle/ev/araç katmanı ve final art ayrı kalır.

Sonraki ana geliştirme sırası:

- Issue #8: Sipariş, teslimat ve gerçek raf döngüsü.
- Issue #9: Müşteri gezinme, danışmanlık ve kasa.
- Issue #10: Fiziksel PC toplama teknik prototipi.
- Issue #11: Save, journal, migration ve recovery.
- Issue #12: Guardian event/invariant/report iskeleti.
- Issue #13: Baştan sona vertical slice.
- Sonraki fazlar: çalışanlar ve gelişmiş müşteri AI, servis/garanti/iade/ikinci el, dinamik ekonomi, itibar/reklam/rekabet, içerik/sanat/ses, alpha/erişilebilirlik/optimizasyon, Steam Playtest, Windows 1.0 ve en son macOS portu.

Henüz tamamlanmayan önemli alanlar:

- Çok satırlı/çok adetli delivery parcel unpack layout'u ve claim akışı.
- Çoklu slot/palet taşıma arabası ve lojistik ekipmanı.
- Çok katlı veya palet istifi.
- Gelişmiş el modeli/animasyonu.
- Garajın bütününe yayılmış final sanat.
- Orders'ın satış/servis varyantları, ilk exact-cash satış settlement'ı ötesindeki Economy kapsamı ve diğer domain assembly'leri; Catalog/Inventory/Orders/Economy event-save entegrasyonu.
- Save/Guardian runtime.
- Steam entegrasyonu.
- Steam entegrasyonu, depot/signing/release matrisi ve geniş Windows donanım/uzun oturum QA'sı. Temel Windows x64 IL2CPP/DirectX r32, r33 ve r34 native build/runtime kapıları başarılıdır.

## 8. Yaşayan belgeler ve kanıtlar

Yeni bir çalışma şu sırayla başlamalıdır:

1. `PROJECT_BIBLE.md`.
2. `Docs/ProjectBible/00_OKU_BENI.md`.
3. `Docs/ProjectBible/10_DEVAM_CHECKPOINT.md`.
4. Bu belge: `Docs/ProjectBible/11_BIRLESIK_CODEX_PROJE_HAFIZASI.md`.
5. `Docs/DEVELOPER-HANDOFF.md`.
6. `Docs/GITHUB-PROJECT-MAP.md`.
7. İlgili `Docs/ADR-*.md` ve `Docs/Evidence/*.md`.
8. Gerektiğinde [tam Codex geçmişi](../CodexHistory/README.md).

Tam konuşma ve dosya geçmişi:

- [Birleşik Codex geçmişi indeksi](../CodexHistory/README.md).
- [Codex dosya değişiklik envanteri](../CodexHistory/FILE_CHANGE_INVENTORY.md).
- [Git commit ve dosya geçmişi](../CodexHistory/GIT_COMMIT_AND_FILE_HISTORY.md).

## 8.1 Issue #53 authoritative motherboard seating checkpoint'i

- Epic #10'un ilk child paketi [Issue #53](https://github.com/cixanla/PC-Shop-Empire-3D/issues/53) ile sınırlandı: tek açık kasa, tek serialized `MicroAtx` anakart, tek doğru slot ve yalnız `SeatedUnsecured` sonucu.
- Feature `582a3cf3e81a2905e39148065bd5f6c7e35bbc06`, source/docs `8c6abe45d1f9b6c72def9b686b9c81bf3704d10d`, tree `387bcba701b8a959681e92bf29dc48a4d09f0ab7` ve başarılı [Repository Guard 31905540378](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31905540378); `PSE.Assembly` mevcut Catalog/Inventory authority'lerini kullanır, managed Workbench dışında shadow authority yoktur.
- Attach/detach exact item identity, immutable receipt, replay/conflict, revision ve failure no-mutation sözleşmelerini taşır. World projection domain transaction'dan sonra değişir; failed drop/recovery aynı fiziksel instance ve last-safe pose'u korur.
- GarageGraybox açık kasa/keyed tray/standoff/anakart graybox'ını içerir. Solver pause/range/focus/LOS/orientation/support/obstruction kapılarını deterministic uygular; preview ve commit pozu aynıdır.
- Primary+Drop aynı frame'de yalnız seat-mode geçişi üretir. Gerçek Input System keyboard/mouse ve gamepad akışları, dynamic prompt ve release–repress ile testlidir.
- Final EditMode `394/394`, PlayMode `26/26`, Universal macOS `328020817` bayt ve Apple M4/Metal 1280×720 `garage-motherboard-seating-r22-v1 assembly-flow=ok ... recovery=ok` başarılıdır.
- Bu tarihsel USB gecikmesi 16 Ağustos 2026'da Issue #53–#55 birleşik milestone'uyla kapandı: source `07364b79`, 640 tracked source + 12 final evidence + source kaydı, 653/653 readback ve `0b5f3c61…aaba9e` manifesti; bütün güvenlik mismatch sayaçları `0`.
- Bu tarihsel checkpointin sonraki adımı Issue #54 motherboard fastener secure/unsecure idi; aşağıdaki güncel kayıtla tamamlandı.

## 8.2 Issue #54 deterministic motherboard fastener checkpoint'i

- Epic #10'un ikinci child paketi [Issue #54](https://github.com/cixanla/PC-Shop-Empire-3D/issues/54) ile tek Assembly-owned fastener, tek visible screwdriver ve `SeatedUnsecured ↔ SeatedSecured` geçişine sınırlandı.
- Feature `b6812394f835d64d5bf8422d8e7996ec433cd0f1`, tree `192f9d8f1334cf9e1ff1d21382c44a847bbfa7e6`; secure/unsecure exact receipt, historical replay, Inventory revision izolasyonu ve secured presentation+authority detach gate'i ekledi.
- Source/docs `7cec7cc4b6fd80997acd0dc2d6943ef08850f4ad` ve [Repository Guard 31909940414](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31909940414) başarılıdır; acceptance `18/18`, Issue kapalı/Roadmap `Done`dur.
- GarageGraybox r23 captive screw/cross recess, solid focus target, screwdriver ve plate'e bağlı tek satır metin taşır. Solver pause/range/focus/LOS/obstruction fail-closed'dur; screw/tool pose yalnız projection'dır ve drift authority'yi mutate etmeden invariantı bozar.
- Valid/blocked fastener context Primary/Interact/Drop edge'lerinin tek sahibidir. Gerçek keyboard/mouse ve gamepad PlayMode testleri dynamic prompt, same-frame blocker drain, pause co-edge ve release–repress sözleşmesini taşır.
- Final EditMode `411/411`, PlayMode `29/29`, Universal macOS `328057977` bayt ve Apple M4/Metal 1280×720 `garage-motherboard-fastener-r23-v1 assembly-flow=ok ... secure-delayed-replay=ok ... detach-authority-blocked=ok ... recovery=ok` başarılıdır.
- Issue #54 final kanıtları aynı doğrulanmış Issue #53–#55 birleşik USB milestone'undadır; 12/12 evidence ve 640/640 exact Git source eşliği geçti.
- Bu tarihsel checkpointin sonraki adımı Issue #55 CPU socket seating + retention idi; aşağıdaki güncel kayıtla doğrulandı.

## 8.3 Issue #55 deterministic CPU socket seating ve retention checkpoint'i

- Epic #10'un üçüncü child paketi [Issue #55](https://github.com/cixanla/PC-Shop-Empire-3D/issues/55) ile tek canonical serialized CPU, tek capacity-1 socket ve tek retention mechanism akışına sınırlandı.
- Feature `99cadad414789d3f440e08cc6e42e727c2b7a2ad`, tree `fea116af021d66efb31b96b4f3e7523929f8b8ad`; atomik managed container pair claim, four-operation Assembly authority/receipt lineage, secured-host gate ve same-instance recovery ekledi.
- Source/docs `d9d0722a1592a83b89938529f72b3170f17e94eb` ve [Repository Guard 31914774370](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31914774370) başarılıdır; acceptance `20/20`, Issue kapalı/Roadmap `Done`dur.
- GarageGraybox r24 notched LGA-style package, ayrı substrate/IHS materyali, triangular mating key, simetrik aperture load plate ve retention lever taşır. Presentation authority değildir; drift invariantı fail-closed'dur ve `21/11/1` render/physics/text bütçesi korunur.
- Gerçek keyboard/mouse ve gamepad PlayMode testleri guided mode, keyed quarter-turn rejection, seat/retain/open/remove, CPU-installed motherboard detach gate, dynamic compact HUD, co-edge/pause drain ve recovery'yi taşır.
- Final EditMode `430/430`, PlayMode `31/31`, Universal macOS `328144884` bayt ve Apple M4/Metal 1280×720 `garage-cpu-socket-retention-r24-v1 cpu-socket-flow=ok ... keyed-orientation=ok retained-remove-gate=ok replay=ok identity=stable` başarılıdır.
- Issue #53–#55 birleşik USB milestone'u `2026-08-16_STAGE_B_PHYSICAL_ASSEMBLY_MOTHERBOARD_FASTENER_AND_CPU_SOCKET_RETENTION` adıyla doğrulandı: source `07364b79`, 653 satırlı `0b5f3c61…aaba9e` manifest, 13.500.119 payload baytı; hash/boyut/yol, Git source, evidence, forbidden, credential ve AppleDouble mismatch `0`.
- Sonraki bounded Epic #10 adımı yalnız dual-latch DIMM/RAM seating akışıdır. GPU/cooler/storage, tam build/benchmark, genel Inventory revision-max hardening, Save/Guardian ve Windows/Steam ayrı kalır.

## 8.4 Issue #56 deterministic single DIMM seating ve dual-latch retention checkpoint'i

- Epic #10'un dördüncü child paketi [Issue #56](https://github.com/cixanla/PC-Shop-Empire-3D/issues/56) ile tek canonical serialized DDR5 UDIMM, tek immutable A2/Channel A/Bank 2 topology ve tek dual-latch retention aggregate akışına sınırlandı.
- Feature `7482fc9aabe6a3a27ba41730db12c60e18aac515`, tree `291b23cb2fe774cb44ba71b26716d7c8131370a2`; atomik managed triple claim, four-operation Assembly authority/receipt lineage, secured-host gate, installed-DIMM detach gate ve same-instance recovery ekledi.
- Source/docs `01c2b5a49f11b27b52af9e299d4d2e48cef3c962`; [Repository Guard 31919985055](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31919985055) ve [31920258176](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31920258176) başarılıdır.
- GarageGraybox r25 dört materyalli UDIMM package, matching notch, hard-surface A2 bed/rail ve iki ayrı latch pivotu taşır. Close sol→sağ, open sağ→sol görünür sıradadır; tek Assembly revision/receipt korunur ve `25/13/1` render/physics/text bütçesi sabittir.
- Gerçek keyboard/mouse ve gamepad PlayMode testleri pickup, guided mode, yalnız 0°↔180° keyed toggle, seat, dual-latch close/open, retained remove, DIMM-installed motherboard detach, dynamic compact HUD, co-edge/pause drain ve recovery'yi taşır.
- Final EditMode `461/461`, PlayMode `33/33`, Universal macOS `328268700` bayt ve Apple M4/Metal 1280×720 `garage-dimm-dual-latch-r25-v1 dimm-flow=ok ... keyed-orientation=ok latch-order=ok replay=ok authority-isolated=ok identity=stable recovery=ok` başarılıdır.
- Ayrı `2026-08-16_STAGE_B_DETERMINISTIC_SINGLE_DIMM_DUAL_LATCH_RETENTION` USB milestone'u 663 tracked source + 4 final evidence + source kaydıyla 668/668 readback, `8658b50a…c50` manifest ve 12.073.868 payload baytıyla doğrulandı; bütün güvenlik/AppleDouble mismatch sayaçları `0`dır. USB metadata `17af550`, Guard `31920923402`, acceptance `21/21`, Issue `Completed` ve Roadmap `Done`dur.
- Sonraki bounded Epic #10 adayı yalnız tek M.2 2280 NVMe SSD seating + captive retention screw akışıdır. İkinci storage yolu, SATA/RAID, GPU/cooler, tam build/benchmark, genel Inventory hardening, Save/Guardian ve Windows/Steam ayrı kalır.

## 9. USB ve yedek güvenlik katmanı

Korunan milestone snapshotları:

- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-11_STAGE_A_BASELINE`.
- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-11_GITHUB_HANDOFF`.
- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-13_STAGE_B_RNG`.
- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-13_STAGE_B_SMALL_BOX_PLACEMENT`.
- Issue #35 stacking için 15 Ağustos 2026 tarihli doğrulanmış USB checkpointi yaşayan checkpoint belgesinde kayıtlıdır.
- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-15_STAGE_B_LOADED_TRANSPORT_CART`; 396 tracked source + 6 evidence, 403 satırlı manifest ve SHA-256/readback doğrulaması geçti.
- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-15_STAGE_B_CATALOG_INVENTORY`; 428 tracked source + 4 test evidence + source kaydı, 433 satırlı `f481ddfa…49dc9` manifest, tam readback/source checksum ve AppleDouble `0` kapısı geçti.
- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-15_STAGE_B_ORDER_RECEIVING`; 449 tracked source + 4 test evidence + source kaydı, 454 satırlı `07480d15…485cff` manifest, tam readback/source checksum ve AppleDouble `0` kapısı geçti.
- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-15_STAGE_B_AUTHORITATIVE_STOCK_FLOW`; source `f20fd17`, 467 tracked source + 4 test/build/runtime evidence + source kaydı, 472 satırlı `5521f869…22a3` manifest, tam readback/source checksum ve forbidden/credential/AppleDouble `0` kapısı geçti.
- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-15_STAGE_B_DELIVERY_PARCEL_UNPACKING`; source `756547f`, 471 tracked source + 5 scene/test/build/runtime evidence + source kaydı, 477 satırlı `37f95b3c…58ac` manifest, tam readback/source checksum ve forbidden/credential/AppleDouble `0` kapısı geçti.
- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-15_STAGE_B_AUTHORITATIVE_SHELF_OFFER`; source `6ae294e`, 488 tracked source + 5 scene/test/build/runtime evidence + source kaydı, 494 satırlı `a95d8457…de7a` manifest, tam readback/source path+checksum ve forbidden/credential/AppleDouble `0` kapısı geçti.
- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-15_STAGE_B_CUSTOMER_BASKET_RESERVATION`; source `109237a`, 498 tracked source + 4 test/build/runtime evidence + source kaydı, 503 satırlı `ff868e4c…20d7` manifest, tam readback/source path+checksum ve forbidden/credential/AppleDouble `0` kapısı geçti.
- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-15_STAGE_B_IMMUTABLE_CHECKOUT_SNAPSHOT`; source `0936cc0`, 508 tracked source + 4 test/build/runtime evidence + source kaydı, 513 satırlı `30c1e7fa…16efa` manifest, tam readback/source path+checksum ve forbidden/credential/AppleDouble `0` kapısı geçti.
- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-15_STAGE_B_ATOMIC_CHECKOUT_FULFILLMENT`; source `80eea8f`, 510 tracked source + 4 test/build/runtime evidence + source kaydı, 515 satırlı `ce72122a…db50b` manifest, 9.373.684 bayt; tam readback/source path+Git-blob ve forbidden/credential/AppleDouble `0` kapısı geçti.
- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-15_STAGE_B_DETERMINISTIC_CUSTOMER_VISIT`; source/docs `d163328`, 535 tracked source + 5 test/build/runtime evidence + source kaydı, 541 satırlı `c82fc76d…cfd` manifest, 9.715.834 payload baytı; tam hash/boyut/path readback, 535/535 Git-blob ve forbidden/credential/AppleDouble `0` kapısı geçti.
- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-15_STAGE_B_EXPLAINABLE_SINGLE_OFFER_CUSTOMER_DECISION`; source/docs `8832c13`, 541 tracked source + 4 final test/build/runtime evidence + source kaydı, 546 satırlı `d46e2433…d1b1` manifest, 9.780.828 payload baytı; 546/546 hash/boyut/path readback, 541/541 Git-blob ve forbidden/cache/credential/AppleDouble/sibling sidecar `0` kapısı geçti.
- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-15_STAGE_B_STALE_SAFE_BUY_ACTION_AND_CHECKOUT_NAVIGATION`; source/docs `aa61700`, 547 tracked source + 4 final test/build/runtime evidence + source kaydı, 552 satırlı `05ed8205…e76f6` manifest, 9.902.727 payload baytı; 552/552 hash/boyut/path readback, 547/547 Git-blob ve evidence/forbidden/cache/credential/AppleDouble/sibling sidecar `0` kapısı geçti.
- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-15_STAGE_B_STALE_SAFE_LEAVE_ACTION_AND_OFFER_DECLINED_EXIT`; source/docs `868885a`, 549 tracked source + 4 final test/build/runtime evidence + source kaydı, 554 satırlı `d685de7a…4209` manifest, 10.003.704 payload baytı; 554/554 hash/boyut/path readback, 549/549 Git-blob, 4/4 evidence ve forbidden/cache/credential/AppleDouble/sibling sidecar mismatch `0` kapısı geçti.
- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-15_STAGE_B_ATOMIC_CASH_CHECKOUT_AND_INITIAL_ECONOMY_SETTLEMENT`; source/docs `aea6e2b`, 566 tracked source + 5 final test/build/runtime evidence + source kaydı, 572 satırlı `b3168162…ecf8` manifest, 10.227.122 payload baytı; 572/572 hash/boyut/path readback, 566/566 Git-blob, 5/5 evidence ve forbidden/cache/credential/AppleDouble/sibling sidecar mismatch `0` kapısı geçti.
- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-15_STAGE_B_BOUNDED_SINGLE_CUSTOMER_CONSULTATION_AND_RECOMMENDATION_GATE`; source/docs `f9bc38d`, 572 tracked source + 5 final evidence + source kaydı, 578 satırlı `f8d3ce98…ccf20` manifest, 10.366.388 payload baytı; 578/578 readback, 572/572 Git-blob, 5/5 evidence ve güvenlik mismatch `0` kapısı geçti.

Snapshotlara `.git`, Unity cache, build, geçici log, token, parola veya credential eklenmez. Her snapshot manifest ve SHA-256 ile doğrulanır; kaynak Git geçmişinin yerine geçmez.

## 10. Bundan sonraki tek-kanal çalışma protokolü

## 9.1 Issue #57 deterministic single M.2 NVMe seating checkpoint'i

- Epic #10'un beşinci bounded child paketi [Issue #57](https://github.com/cixanla/PC-Shop-Empire-3D/issues/57) ile tek canonical serialized M.2 2280 NVMe, tek M-key/2280 standoff ve motherboard-owned captive screw akışına sınırlandı.
- Feature `4f14e7bcb946f2f8e713a70d16d0e8f04216dbe1`, tree `1aedb833983df256c500c6a1815b075fa29c254c`; dört-container atomic claim, `EmptyOpen ↔ StorageDeviceSeatedUnsecured ↔ StorageDeviceSecured`, exact receipt replay, host-detach gate ve same-instance recovery ekledi.
- GarageGraybox r26'da 18° guided pose, flat seated pose, PCB/controller/NAND/label/gold M-key contacts, standoff ve captive screw görünürdür. Presentation authority değildir; generic placement/stack/cart yolu fail-closed'dur.
- Gerçek keyboard/mouse ve gamepad akışı, dynamic compact HUD, pause/co-edge drain, wrong-orientation/obstruction/secured-remove no-mutation kapılarıyla testlidir.
- Final EditMode `490/490`, PlayMode `35/35`, Universal macOS `328362356` bayt ve Apple M4/Metal exact storage smoke başarılıdır. İkinci M.2/SATA/RAID, tam benchmark ve diğer PC bileşenleri ayrı kalır.
- Source/docs `6e0627e`, Guard `31970813717` ve 689/689 `19da758c…21b8` USB readback başarılıdır; Issue #57 ve Roadmap `Done`dur.

## 9.2 Issue #58 deterministic single air-cooler seating checkpoint'i

- Epic #10'un altıncı bounded child paketi [Issue #58](https://github.com/cixanla/PC-Shop-Empire-3D/issues/58) ile tek canonical serialized LGA1700 top-down air cooler, pre-applied single-use TIM ve stable four-point retention akışına sınırlandı.
- Feature `e2f10a22c37101cb12c5d6530c8f104deb72e99d`, tree `55d5f0d733530a2e4c1400f4f83c29f37dcafff8`; five-container atomic claim, `EmptyOpen ↔ CoolerSeatedUnsecured ↔ CoolerRetained`, exact receipt replay, TIM consumption, CPU-retention/motherboard-detach host gates ve same-instance recovery ekledi.
- GarageGraybox r27'de cold plate/TIM yüzeyi, fin stack, fan, bracket ve dört retention point görünürdür. Retain `1→3→2→4`, release ters sıradadır; presentation authority değildir ve generic placement/stack/cart yolu fail-closed'dur.
- Gerçek keyboard/mouse ve gamepad akışı, dynamic compact HUD, pause/co-edge drain, wrong-orientation/RAM-clearance/obstruction/retained-remove/consumed-TIM no-mutation kapılarıyla testlidir.
- Final EditMode `521/521`, PlayMode `38/38`, Universal macOS `328534723` bayt ve aktif Apple Silicon/Metal 1280×720 exact cooler smoke başarılıdır. Source/docs `2e848e3`, Guard `32591206866` + `32591381804`; USB-erteleme kapanış metadatası `fce6bfa`, Guard `32593034745`; acceptance `19/19`, Issue/Roadmap `Done`dur.
- Fiziksel USB kullanıcı talimatıyla ertelendi; 717/717 doğrulanmış `f7b2b9bafee9529d95431bbc90914ba51ab24e01de9a0d5d77a53f26cb5626a5` yerel staging hazırdır. Kullanıcı USB'nin bağlandığını söyleyene kadar USB sorgulanmaz ve gameplay geliştirmesi sürer.
- Ayrı paste/reapplication, liquid cooling, GPU/PSU/cabling, tam benchmark ve Windows/Steam ayrı bounded kapılardır.

## 9.3 Issue #59 deterministic single PCIe x16 graphics-card seating checkpoint'i

- Epic #10'un yedinci bounded child paketi [Issue #59](https://github.com/cixanla/PC-Shop-Empire-3D/issues/59) ile canonical Northstar A60 ProductId'sini kullanan ayrı exact serialized assembly GPU item'ı, tek PCIe x16 slot, slot latch, rear bracket ve bracket fastener akışına sınırlandı; shadow SKU oluşturulmadı.
- Feature `1b29ad29d6e4d8cc1ce09b0989a038a72297585c`, tree `e7b3d5d4ad13bf08f822570966660ab6c48e6a55`; six-container atomic claim, `EmptyOpen ↔ GraphicsCardSeatedUnsecured ↔ GraphicsCardRetained`, exact receipt replay, installed-GPU motherboard-detach gate ve same-instance recovery ekledi.
- GarageGraybox r28'de dual-fan shroud, PCB, PCIe contacts, rear bracket, slot latch ve screw görünürdür. Presentation authority değildir; generic placement/stack/cart yolu fail-closed'dur.
- Gerçek keyboard/mouse ve gamepad akışı, dynamic compact HUD, pause/co-edge drain, wrong-orientation/interface/chassis-clearance/cooler-clearance/obstruction/duplicate-seat/retained-remove no-mutation kapılarıyla testlidir.
- Final EditMode `548/548`, PlayMode `43/43`, Universal macOS `328781520` bayt ve aktif Apple Silicon/Metal 1280×720 exact GPU smoke başarılıdır. Feature `1b29ad2`, source/docs `a5bbca4`; Guard `32599710154` + `32600012769`; acceptance `20/20`, Issue `CLOSED/COMPLETED`, Roadmap `Done`; parent Epic #10 açık/In Progress kalır.
- Fiziksel USB kullanıcı talimatıyla ertelendi; Issue #58'in 717/717 staging'i korunur, Issue #59 için henüz milestone/readback iddiası yoktur. Kullanıcı USB'nin bağlandığını söyleyene kadar USB sorgulanmaz ve gameplay geliştirmesi sürer.
- PSU, PCIe power cabling, alternate GPU dimensions/slots, tam benchmark ve Windows/Steam ayrı bounded kapılardır.

## 9.4 Issue #60 deterministic single ATX PS/2 PSU seating checkpoint'i

- Epic #10'un sekizinci bounded child paketi [Issue #60](https://github.com/cixanla/PC-Shop-Empire-3D/issues/60) ile tek canonical serialized ATX PS/2 PSU, chassis-owned bay/rear mount ve dört distinct fastener akışına sınırlandı; shadow SKU veya ikinci authority oluşturulmadı.
- Feature `f998d7d1c400c9328afa226f0727e6591c02d4e2`, tree `78d62c46354cda45422ca947df10ba9d6823b7c9`; authored-clearance fix `b6c3ff8e95a4da75161b51cdbcbab87cb529f076`, tree `a15865346f52b6b39d84cec49c70babbc6550b89`; seven-container atomic claim, `EmptyOpen ↔ PowerSupplySeatedUnsecured ↔ PowerSupplyRetained`, exact receipt replay, alternate-order isolation ve same-instance recovery ekledi.
- GarageGraybox r29'da PSU housing, fan/grille, filtered floor intake, AC inlet, rocker switch, disconnected modular panel, rear plate ve dört screw görünürdür. Production clearance gerçek dört authored chassis collider'ına bağlıdır; support ve future cable blocker'ları ayrıdır.
- Gerçek keyboard/mouse ve gamepad akışı, dynamic compact HUD, pause/co-edge drain, wrong-orientation/interface/support/rear-plane/chassis-clearance/obstruction/duplicate-seat/retained-remove no-mutation kapılarıyla testlidir.
- Final EditMode `577/577`, PlayMode `47/47`, Universal macOS `328937592` bayt ve aktif Apple Silicon/Metal 1280×720 exact PSU smoke başarılıdır. Feature `f998d7d`, fix `b6c3ff8`, source/docs `4939a04`; Guard `32606958882` + `32607437408` + `32607886160`; acceptance `20/20`, Issue `CLOSED/COMPLETED`, Roadmap `Done`; parent Epic #10 açık/In Progress kalır.
- Exact source-plus-evidence staging 770 Git source + 4 evidence + source kaydıyla 775/775 readback, 770/770 Git-blob, 14.729.691 bayt ve `705784c6…53d4` manifestiyle doğrulandı. macOS beklenen harici fiziksel USB'yi mount etmedi; yanlış volume'a yazılmadı ve fiziksel milestone/readback iddiası yoktur.
- ATX/EPS/PCIe/SATA cabling, electrical power-on, wattage/headroom, POST/BIOS/OS, completed benchmark, final art ve Windows/Steam ayrı bounded kapılardır.

## 9.5 Issue #61 deterministic single ATX24 split-PSU cable routing checkpoint'i

- Epic #10'un dokuzuncu bounded child paketi [Issue #61](https://github.com/cixanla/PC-Shop-Empire-3D/issues/61) ile tek canonical serialized ATX24 power cable, typed PSU 18-pin + PSU 10-pin + motherboard 24-pin endpoint'leri ve üç stable ordered route waypoint'ine sınırlandı; connector child'lar ayrı ürün/Inventory item değildir.
- Feature `1fc29f13171925c2445eaa7334158e0f058e76a5`, tree `d265332f1d6655639e55db31f9b5a11e3d177f49`; eight-container atomic claim, capacity-one CableRoute, `Loose ↔ Routed`, exact route/unroute receipt history/replay, host lineage ve same-instance recovery ekledi.
- GarageGraybox r30'da tek kinematic cable root, üç visible connector/latch/key child ve jointsiz authored branch/trunk route görünürdür. Route focus görünür motherboard connector üzerinden deliberate unroute'a izin verirken gerçek chassis obstruction fail-closed kalır.
- Gerçek keyboard/mouse ve gamepad route mode, iki keyed orientation, dynamic compact HUD, pause/co-edge drain; wrong-key/host/range/focus/LOS/clearance/duplicate/dependent-detach/generic-bypass failure yollarıyla testlidir.
- Final EditMode `589/589`, PlayMode `49/49`, Universal macOS `329082160` bayt ve aktif Apple M1/Metal 1280×720 exact cable smoke başarılıdır. Feature Guard `32613813494`; source/docs `52795b66fee1eb933d0d9c4ff8cbd7eca512d718`, tree `d0bdb7bd39bb09a27565ed1d4a0fd77e22b7dfa3` ve Guard `32614187494` başarılıdır.
- Ayrı `2026-08-23_STAGE_B_DETERMINISTIC_SINGLE_ATX24_SPLIT_PSU_CABLE_ROUTING` USB milestone'u atomik adlandırmayla sabitlendi: iki tam 801/801 hash/boyut/yol readback, 796/796 exact Git source, 4/4 evidence, 15.237.662 payload baytı ve `f2145ecb…1365` manifest; bütün güvenlik/AppleDouble mismatch sayaçları `0`dır. USB metadata `f9a5da8`, Guard `32632615041`, acceptance `20/20`; Issue #61 `CLOSED/COMPLETED`, Roadmap `Done`, parent #10 açık/In Progress'tir.
- EPS/CPU, PCIe/GPU, SATA/Molex/fan/front-panel/data/RGB cabling, electrical power-on, wattage/headroom, POST/BIOS/OS, completed benchmark, free-rope physics, final art ve Windows/Steam ayrı bounded kapılardır.

## 9.6 Issue #62 deterministic single EPS12V/CPU power cable routing teknik checkpoint'i

- [Issue #62](https://github.com/cixanla/PC-Shop-Empire-3D/issues/62) parent Epic #10 altında tamamlanmış Assembly/P0/Critical bounded dilimidir; teknik, yerel staging, fiziksel USB ve GitHub durum kapıları tamamlandı.
- Feature `15d83aeba0d71238a31bf7b7db5fab3dbd9b5951`, tree `c14524fecee561eff3a144bd15e67be5a48f8335`; tek canonical serialized EPS12V/CPU cable, iki typed/keyed 8-pin endpoint, üç ordered waypoint, capacity-one `CpuPowerCableRoute` ve dokuz-container all-or-none claim ekledi.
- Retained PSU + secured motherboard + retained CPU lineage, exact Hands↔route custody, immutable route/unroute receipts, immediate/delayed replay, receipt-history fold, ATX24 isolation ve dependent-detach/generic-bypass no-mutation testlidir.
- GarageGraybox r31 tek kinematic root, iki connector/latch/key, loose braided presentation ve üç-waypoint authored route taşır. Gerçek keyboard/mouse ve gamepad akışı aynı Unity component instance/stable ItemId'yi pickup→route→unroute→recovery boyunca korur.
- Final EditMode `610/610`, PlayMode `51/51`, Universal macOS `329206153` bayt ve aktif Apple M1/Metal 1280×720 exact EPS12V smoke başarılıdır. Feature Guard `32642211422`; source/docs `cff75f8`, tree `aa5acd7`, Guard `32642638437` geçti.
- Yerel final staging iki tam 832/832 payload, 826/826 exact Git source ve 5/5 evidence readback'ini geçti; payload 15.757.786 bayt, manifest `afa89feb…6a73`, bütün fark sayaçları `0`dır.
- Scene marker `garage-eps12v-cpu-power-cable-routing-r31-v1`; exact native marker `GARAGE_EPS12V_POWER_CABLE_RUNTIME_SMOKE ... identity=stable recovery=ok` sözleşmesidir.
- Doğru fiziksel USB milestone'u atomik final adlandırmasıyla sabitlendi; iki tam 832/832 payload, 826/826 exact Git source ve 5/5 evidence readback manifest `afa89feb…6a73` ile sıfır fark/AppleDouble verdi. USB metadata `2db7cf9`, Guard `32672086464`; acceptance `21/21`, Issue #62 `CLOSED/COMPLETED`, Roadmap `Done`, parent Epic #10 açık/In Progress durumundadır. Electrical power-on, completed benchmark, diğer kablo aileleri, Save/Guardian, free-rope physics, final art ve Windows/Steam ayrı bounded kapılardır.

## 9.7 Issue #63 deterministic single PCIe/GPU 6+2 power cable routing teknik checkpoint'i

- [Issue #63](https://github.com/cixanla/PC-Shop-Empire-3D/issues/63) tek canonical serialized PCIe/GPU 8-pin cable, PSU-side monolitik 8-pin, GPU-side ayrı 6-pin + 2-pin housing, typed/keyed endpoint'ler, üç ordered waypoint ve capacity-one `GpuPowerCableRoute` ekler.
- Feature `ea1e51f8`, explicit visual fix `d655f1a5`; exact Hands↔route custody, retained PSU + secured motherboard + retained GPU lineage, immutable route/unroute receipts, replay/history fold, ATX24/EPS12V isolation ve dependent-detach/generic-bypass fail-closed kapıları testlidir.
- Mac exact committed-scene EditMode `626/626`, PlayMode `53/53`, Universal build `329334656` bayt ve Apple M1/Metal r32 smoke başarılıdır.
- Final source/docs `d597941a20afd0491547513abbc68e0b9d890aab`; clean Windows clone StrictMode x64 IL2CPP build report `1320679269` bayt üretmiş, Intel Iris Xe/Direct3D 11.0 feature level 11.1 exact r32 readiness ve `GARAGE_PCIE_GPU_POWER_CABLE_RUNTIME_SMOKE ... recovery=ok` markerını vermiştir. Build/runtime log SHA-256 değerleri sırasıyla `459e95bb…4799` ve `853dd5bd…f9e`dir; Guard `32677495639` başarılıdır.
- Mac+Windows teknik kapıları tamamdır. Issue #63 final fiziksel USB paketi, iki tam readback ve Issue/Roadmap metadata kapanışı henüz yapılmadığı için açık/In Progress kalır; önceki exit `198` lisans denemeleri yalnız tarihsel tanıdır.

## 9.8 Issue #64 accepted custom-PC request, immutable quote ve exact reservation teknik checkpoint'i

- [Issue #64](https://github.com/cixanla/PC-Shop-Empire-3D/issues/64) feature `c7d38845ffccb5ae6e5365e580c238d70f8dac95`, tree `615c9c4398f6a0be16c3a693dd812aa3f5541291`; exact customer/visit/consultation provenance'ına bağlı graphics-first request ve immutable on-satırlı quote/BOM ekler.
- BOM motherboard, CPU, DIMM, M.2 SSD, cooler, GPU, PSU, ATX24, EPS12V ve PCIe/GPU 6+2 rollerini stable line/reservation identity, integer price/currency, compatibility ve budget kapılarıyla bağlar. Inventory exact serialized seti tek managed operation/claim ve tek revision ile atomik reserve eder; exact replay, interrupted-publication recovery, drift/conflict ve direct release/consume bypass'ları fail-closed'dur.
- GarageGraybox `garage-custom-pc-quote-reservation-r33-v1`; consultation→accepted request→visible quote/BOM→10 exact reservations akışını gerçek keyboard/mouse ve gamepad ile taşır. Range/focus/LOS/pause/release-repress/single-consumer ve accepted-deadline kapıları testlidir.
- Mac ve Windows full regression EditMode `647/647`, PlayMode `59/59`; failed/skipped/inconclusive `0`. Universal Mac build `329396456` bayt ve Apple M1/Metal r33 smoke; Windows x64 IL2CPP report `1326137709` bayt ve Intel Iris Xe/Direct3D 11.0 r33 smoke başarılıdır.
- Guard `32698054990` başarılı, draft PR #65 açıktır. Mac ve Windows masaüstü bağlantıları doğrulanmış build'lere çözülür. İnsan oynayışı görünür turunda hareket ve pause/resume no-lurch gözlendi; mouse-look için ayrı manuel başarı iddiası kurulmadı.
- Kullanıcı USB kablosunun bağlı olduğunu bildirmiştir; fakat son salt-okunur aygıt denetiminde `/Volumes/cixanla/CIXANLA` mount'u görünmedi. Yanlış volume'a yazılmadı. Final docs/CI kimliği, fiziksel USB iki-readback ve Issue/Roadmap kapanışı bekler.

## 9.9 Issue #66 immutable custom-PC work order ve physical work ticket teknik checkpoint'i

- [Issue #66](https://github.com/cixanla/PC-Shop-Empire-3D/issues/66) core feature `f9545605baff423f05615e7326902e24dc82aeeb`, current technical head `f8afd62326c74aff23fa10bb33ef79ecb9a656b6`, tree `69ea366cc49e99b653f5d02d9c0f238b4906de69`. Stable typed BuildOrderId, WorkTicketId ve OperationId exact customer/request/quote/claim/workbench ile on line/item/reservation identity'yi immutable dondurur.
- Inventory exact managed reservation setini commit öncesi yeniden doğrular ve bir terminal operation-keyed allocation receipt'i exactly-once yayınlar. On reservation ve serialized item live/in-place/unchanged kalır; generic release/consume kullanılmaz, exact replay revision drift üretmez ve interrupted publication yalnız exact stored allocation'dan recover edilir.
- GarageGraybox `garage-custom-pc-work-ticket-r34-v1`; canonical workbench'te job identity, `10/10` ve `MONTAJA HAZIR • HENÜZ BAŞLAMADI` gösteren collider-safe physical ticket taşır. Range/focus/LOS/empty-hands/fresh Interact, pause/co-edge/competing-target ve gerçek keyboard/mouse/gamepad customer→workbench rotası testlidir. Quote ve Assembly authority'leri izoledir.
- Full EditMode `661/661`, PlayMode `66/66`; Universal Mac `329478891` bayt ve Apple M1/Metal exact r34 smoke başarılıdır. Exact clean Windows x64 IL2CPP build `1328828053` report baytı üretmiş, only-Direct3D11 ve byte-exact ProjectSettings restore/readback vermiştir. Physical ticket/carry/cart Interact ownership ve complete no-teleport snapshot matrisi current source üzerinde yeniden doğrulanmıştır.
- Interactive Windows player Intel Iris Xe/Direct3D 11.0 feature level 11.1 üzerinde Windows host, r34 readiness ve exact work-ticket markerlarını birer kez, forbidden tokenı sıfır vermiştir. Technical-source Guard `32721069982` başarılıdır. Exact source/docs `4e1ef4322d9ef049e3aac915c611474f6bee92fd`, tree `4df76fb1b50da53bdee7e65cb64acf0e73a5c018`, Guard `32723213686` başarılı ve draft PR #67 bu checkpoint'e bağlıdır.
- ADR-0043 ve tarihli Issue #66 Evidence exact artifact hashes/bytes/markersı ve `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/issue66-f8afd62` canonical `9/9` kaynağını taşır. Yerel immutable milestone incoming ve final doğrulamalarında `906/906` manifest, `896/896` exact Git source, `9/9` evidence, `17.330.935` bayt ve `1514481a…4121` manifest ile geçti. Doğru external physical USB ve önceki Issue #62 milestone zinciri salt-okunur doğrulandı; collision-free `.incoming-issue66-6752927` ilk tam readback sonrasında atomik final adına taşındı ve final ikinci tam readback'i aynı `906/906`, `896/896`, `9/9`, `17.330.935` bayt ve `1514481a…4121` sonuçlarıyla geçti. Internal/sibling AppleDouble ve incoming residue `0`dır. Fiziksel metadata `a80e325`, Guard `32726202296`, acceptance `18/18`, Issue #66 `CLOSED/COMPLETED`, Roadmap `Done`; parent Epic #10 açık/In Progress kalır. PR #67 accepted checkpoint'in integration aracıdır.
- Sınır: component transfer/build-kit completion, physical assembly completion, electrical power-on, POST/BIOS/OS, benchmark/QA, packaging/delivery, final settlement, Save/Guardian, final art ve Steam ayrı dependent paketlerdir.

## 9.10 Issue #68 canonical motherboard physical build-kit tamamlanmış checkpoint'i

- [Issue #68](https://github.com/cixanla/PC-Shop-Empire-3D/issues/68) feature chain `2a69436` + `b0d2a97`, current technical head `480874191ee2c950e046ab2aee8be92d61d79fe4`, tree `e229788741df4c456840d356633e2a4bc1702516`. Canonical reserved motherboard exact work-order/ticket/allocation line/product/item/reservation tuple'ıyla seçilir.
- Stable child operation ve immutable pickup/place receipts exact replay/recovery ile revision drift üretmez. Capacity-one managed BuildKit, Assembly custody'sinden ayrıdır; narrow allocation bridge source → ActorHands → BuildKit hareketinde reservation/allocation'ı live/exact tutar ve generic bypass'ları kapatır.
- Presentation domain-first aynı Unity component/stable ItemId'yi carry/rotation/preview/placement/recovery boyunca korur. Work ticket exact `0/10 → 1/10`; diğer dokuz reservation/item, quote price ve Assembly state/receipts untouched'dır. Real keyboard/mouse + Input System gamepad ve bütün custody/input/failure gates testlidir.
- Exact detached-clean technical clone Unity 6000.3.21f1 full EditMode `675/675`, PlayMode `73/73`; Universal Mac report `329571495` bayt + Apple M1/Metal exact r35 smoke; exact Windows x64 IL2CPP/only-D3D11 report `1327308678` bayt + Intel Iris Xe/feature level 11.1 exact interactive r35 smoke kapılarını geçti.
- Technical-source Guard `32744068996`; exact source/docs `374094ceda9f8f65991e3906c62e1e4ba768b134`, tree `65418d089bc88c9f3dd435b93536c754fd4fef41` ve Guard `32750065918` başarılıdır. PR #69 accepted checkpoint'i merge etmiştir. ADR-0044 ve tarihli Evidence, exact artifact/procedure/source/task-cleanup bağını ve `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/issue68-4808741` canonical `14/14` evidence kaynağını taşır.
- Collision-free local immutable incoming/final ile doğru physical USB incoming/atomik final hedefleri dört tam readback'te aynı `929/929` payload, `914/914` exact Git source, `14/14` evidence, `18.882.211` bayt ve `6d59ddb9…112a9` manifest sonucunu verdi; AppleDouble/incoming residue `0`, USB final readback sonrası güvenle eject edildi. Physical metadata `3e1de00`, tree `0973c18`, Guard `32751777063`; acceptance `20/20`, Issue #68 `CLOSED/COMPLETED`, Roadmap `Done`, PR #69 merge commit `f60464d` ile birleşti. Parent Epic #10 açık/In Progress kalır. Diğer dokuz kit transferi, 10/10 completion, job-specific assembly, electrical/POST/OS/benchmark, packaging/delivery, Save/Guardian, final art ve Steam ayrı paketlerdir.

## 9.11 Issue #71 canonical processor physical BuildKit tamamlanmış checkpoint'i

- [Issue #71](https://github.com/cixanla/PC-Shop-Empire-3D/issues/71) technical source `11683c8b567ad6edcd6777610875aeebd0e509ef`, tree `6890157f3f3625661314b34700259e0933ff2677`; canonical reserved processor exact work-order/ticket/allocation line/product/item/reservation tuple'ıyla seçilir ve staged motherboard önkoşulu fail-closed doğrulanır.
- Domain-first physical transfer aynı stable serialized processor identity'sini source → ActorHands → processor-specific BuildKit custody boyunca korur. Capacity-one placement, immutable pickup/place receipts, exact replay/recovery, live reservation/allocation ve revision invariants korunur; generic drop/custody bypass'ları reddedilir.
- GarageGraybox `garage-processor-build-kit-r36-v1`; gerçek keyboard/mouse ile pickup, carry, 90° rotation ve visible `1/10 → 2/10` placement akışını taşır. Processor socket mode/authority untouched, Assembly revision/receipt untouched, serialized item/projection sayısı exact ve no-duplicate-loss kapıları testlidir.
- Fresh EditMode `677/677`, PlayMode `81/81`; Universal Mac build report `329627927` bayt, Apple M1/Metal exact r36 native smoke ve technical-source Guard `32827174483` başarılıdır.
- İlk Windows x64 IL2CPP/only-D3D11 player ve Intel Iris Xe exact r36 runtime smoke başarıyla sonuçlanmış olsa da ham build logunun erken import safhasındaki toparlanmış Burst linker hatası eski forbidden filtresince sayılmamıştır; bu evidence provisional geçmiş olarak ayrılmıştır. Collision-free exact-source `hardened-v2` Windows report `1329802474` bayt, genişletilmiş Burst/native-link fatal-token `0`, üç binary/procedure exact readback, Intel Iris Xe exact r36 runtime, task deletion ve residue `0` ile canonical native closure'ı geçti.
- ADR-0045 ve tarihli Evidence yaşayan kapanış kaydıdır. Source/docs/provenance `7501fa74335ca977364033025eb51f4f4fc7bebf`, tree `0fcfd59000cc5cdca915d86d4854862c3879f435`, Guard `32833455406`; immutable local + doğru physical Windows USB incoming/final readback `942/942` payload, `927/927` exact Git source, `14/14` evidence, `19.139.923` bayt ve `f38ae282…3cb8ed` manifest ile geçti. Exact-target/internal AppleDouble ve incoming residue `0`; acceptance `22/22`, Roadmap `Done`, PR #72 hazırdır. Önceki `2026-08-24_STAGE_B_ISSUE71_CPU_BUILDKIT_DOMAIN_FOUNDATION` ve pre-hardening local package yalnız immutable ara checkpoint'lerdir; üzerlerine yazılmaz.

- Kullanıcıyla proje hakkındaki bütün yeni konuşma ve geliştirme yalnız `PC Shop Empire 3D — ANA GÖREV` içinde yapılır.
- Eski iki görev geçmiş kayıt olarak arşivde kalır; normal geliştirme için yeniden açılmaz.
- Aynı karar kullanıcıya tekrar sorulmadan önce bu belge ve tam konuşma arşivi aranır.
- Küçük ve geri alınabilir teknik kararlar ana görev tarafından uygulanabilir.
- Büyük kapsam değişikliği, ücretli araç, büyük indirme, uygulama kurulumu, dış yayın, destructive işlem veya vizyon değişikliği kullanıcı onayı ister.
- Her bounded paket: salt-okunur repo doğrulaması → kod/test → gerçek Unity test/build/runtime kanıtı → yaşayan belge/ADR/Evidence → küçük commit → private push → CI/Repository Guard → gerekiyorsa USB milestone sırasıyla kapatılır.
- Kullanıcıya ait veya ilişkisiz değişiklikler silinmez, üzerine yazılmaz ya da başka pakete karıştırılmaz.
- Credential, token, parola, özel anahtar ve gizli dosyalar Git, Codex konuşma arşivi veya USB snapshotına alınmaz.
- Kalan kullanım düşükse yeni uzun paket başlatılmaz; en yakın temiz commit sınırında checkpoint bırakılır.

## 11. Hızlı devam cümlesi

Ana görev bir sonraki turda şu anlamla devam etmelidir:

> Issue #89 technical source `2fdf371`, tree `c5e6de5`; fresh EditMode `712/712`, PlayMode `119/119`, Universal Mac report `330104684` bayt, Apple M1/Metal exact r45 native smoke ve technical Guard `32930403290` kapılarını geçti. Canonical reserved motherboard historical `10/10` BuildKit'ten exact ActorHands ve existing Assembly Workbench custody'sine aynı Unity instance/stable ItemId ile taşınır; live reservation/allocation, guided seat/fastener authority, secure→unsecure→detach→reseat, immutable history, other-nine untouched, replay/recovery ve no-duplicate-loss korunur. Collision-free `issue89-hardened-v1` exact-source Windows IL2CPP/Direct3D11 report `1340592635` bayt, geniş fatal-token `0`, Intel Iris Xe exact r45 runtime, task deletion ve player/Unity/task residue `0` ile native kapıyı geçti. Source/docs Guard, final canonical `14/14`, immutable local+sağlıklı fiziksel USB ve exact-r45 insan kabulü henüz bekler; strict acceptance `22/25`, Issue #89 ve draft PR #90 açık/In Progress kalır. Sıradaki bounded teknik iş CPU socket/retention zinciridir ve önceki immutable checkpoint'ler değiştirilmez.
