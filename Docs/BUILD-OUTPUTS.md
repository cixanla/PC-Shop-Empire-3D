# Builds

`../../Builds/Local/` yalnız yerel doğrulama buildleri içindir ve UVCS workspace'inin dışında kalır. Yayın buildleri ayrı, imzalı ve denetlenmiş bir süreçte üretilecektir.

## Stage B accepted custom-PC request, immutable quote and exact reservation doğrulaması — 24 Ağustos 2026

Issue #64 feature `c7d38845ffccb5ae6e5365e580c238d70f8dac95` aynı kaynak kimliğiyle iki gerçek platformda doğrulandı:

- macOS: Development + StrictMode Universal Mach-O (`arm64` + `x86_64`), Unity report `329.396.456` bayt; executable `117.179` bayt, SHA-256 `9cfdbf7d17583135550bd6a507164f644b8242e9bfbcfaf26641191a69c249bf`.
- macOS testleri: EditMode `647/647`, PlayMode `59/59`; failed/skipped/inconclusive `0`.
- macOS runtime: aktif Apple M1/Metal, 1280×720; readiness `garage-custom-pc-quote-reservation-r33-v1` ve exact custom-PC quote/reservation smoke başarılı.
- Windows: Development + StrictMode x64 IL2CPP, Unity report `1.326.137.709` bayt. `PC Shop Empire 3D.exe` `667.136` bayt / SHA-256 `9688ae089d590352dbe0cba8722328926cd4d9b7722f68ec9942243925c1282a`; `GameAssembly.dll` `44.777.472` bayt / `2978b79b47d4c6aefef58d81f7235940b9df4d4794fb0935dfa3a5233b960021`; `UnityPlayer.dll` `84.237.744` bayt / `cc018c912f461b0f7bdcaeadd5c1d8d9361d92fe5a1c3aeac27a0ac1186a4a59`.
- Windows testleri: EditMode `647/647`, PlayMode `59/59`; failed/skipped/inconclusive `0`.
- Windows runtime: aktif console oturumunda Intel Iris Xe Graphics, Direct3D 11.0 feature level 11.1; readiness r33 ve exact custom-PC smoke bir kez başarılı. Windows masaüstü kısayolu bu doğrulanmış IL2CPP player'a okunarak bağlandı.
- Kısa görünür Mac insan turu sağ/geri/sol konum değişimini, pause sırasında sıfır hareketi ve resume sonrası fresh-input hareketini gözledi. Fare sürükleme aracı pencere erişimini kaybettiği için ayrıca manuel mouse-look iddiası kurulmadı.

Bu çıktılar geliştirme doğrulamasıdır; Steam packaging/signing, performans matrisi ve release candidate iddiası değildir. Ayrıntılı hashler tarihli Issue #64 evidence belgesindedir.

## Stage A doğrulama çıktıları — 11 Ağustos 2026

| Çıktı | Tür | Unity rapor boyutu | Yerel disk kullanımı | Sonuç |
|---|---|---:|---:|---|
| `macOS/PC Shop Empire 3D.app` | Development Player, Universal Mach-O (`arm64` + `x86_64`) | 325.608.373 bayt | yaklaşık 311 MiB | Build başarılı; `-batchmode -nographics` açılış/kapanış smoke testi başarılı |
| `Windows-Mono-x64/PC Shop Empire 3D.exe` | Development Player, PE32+ Windows x86-64, Mono | 166.141.340 bayt | yaklaşık 159 MiB | Build başarılı; gerçek Windows cihazında çalıştırma henüz yapılmadı |

Ana çalıştırılabilir dosya SHA-256 değerleri:

- macOS: `667db19ec9d71e1493ed412fc006a7323ae56834bb27e4e5e11f803a075254b5`
- Windows: `c8b0d73dc40e4f2cddbf656cfb7257fcb8273da22e44e12a8694cd8e275c6fb2`

Tüm dosyaların `SHA-256 + mantıksal boyut + göreli yol` satırlarının sıralı akışından alınan içerik-kümesi özetleri:

- macOS: `c02d984d0f83221ab2eb64bb31a552f7f549d6fc64b64cdcd8d5048db4bc32dd`
- Windows: `c4cbdb8b391a5130e4da383bfd031d7da1a584f4e84771a33cdb660a2ae5e949`

Windows çıktısı yalnız erken taşınabilirlik kanıtıdır. Final Windows sürümü gerçek Windows x64 makinede IL2CPP, DirectX/GPU, sürücü ve Steam testlerinden geçmeden yayın adayı sayılmaz. macOS çıktısı imzalı/notarize edilmiş dağıtım paketi değildir.

## Stage B küçük kutu placement doğrulaması — 13 Ağustos 2026

`macOS/PC Shop Empire 3D.app` güncel kaynakla yeniden üretildi:

- Tür: Development Player, Universal Mach-O (`arm64` + `x86_64`).
- Unity build raporu: 326.147.564 bayt.
- Ana executable SHA-256: `f338b7479b28766ffe965548e1b0167a31af42cc72dc3d366e481a3c761476bb`.
- Runtime: Apple M4/Metal, pencereli 1280×720; `motor=ok input=ok carry=ok placement=ok`.
- Görsel smoke: gerçek player'da pickup, dinamik placement prompt'u ve kırmızı `ENGELLİ` ghost doğrulandı.

Bu çıktı yalnız yerel geliştirme kanıtıdır; imzalı/notarize yayın paketi veya Windows native doğrulaması değildir.

## Stage B authoritative motherboard seating doğrulaması — 15 Ağustos 2026

`macOS/PC Shop Empire 3D.app`, Issue #53 feature `582a3cf` kaynak durumuyla yeniden üretildi:

- Tür: Development + StrictMode Player, Universal Mach-O (`arm64` + `x86_64`).
- Unity build raporu: `328.020.817` bayt; app disk kullanımı `320.976 KiB`.
- Ana executable: `117.179` bayt; SHA-256 `cad75f5e070dfabe0335f9c6ee8d50659dc3ceddd1e036cb63c83b787e5da0f0`.
- Runtime: Apple M4/Metal, pencereli 1280×720; `garage-motherboard-seating-r22-v1` readiness ve exact assembly smoke başarılı.
- Test kapıları: EditMode `394/394`, PlayMode `26/26`; failed/skipped `0`.

Bu çıktı yalnız yerel geliştirme kanıtıdır. Gerçek Windows x64 hostta IL2CPP/DirectX/Steam kapısı ve yayın imzası henüz tamamlanmamıştır.

## Stage B deterministic CPU socket seating ve retention doğrulaması — 16 Ağustos 2026

`macOS/PC Shop Empire 3D.app`, Issue #55 feature `99cadad` kaynak durumuyla yeniden üretildi:

- Tür: Development + StrictMode Player, Universal Mach-O (`arm64` + `x86_64`).
- Unity build raporu: `328.144.884` bayt.
- Ana executable: `117.179` bayt; SHA-256 `d87710b6c5f12fc832bd0a8a1eba317e1074e913beae24daa3d39436737e24f0`.
- Runtime: Apple M4/Metal, pencereli 1280×720; `garage-cpu-socket-retention-r24-v1` readiness ve exact CPU socket smoke başarılı.
- Test kapıları: EditMode `430/430`, PlayMode `31/31`; failed/skipped/inconclusive `0`.
- Build log: `build-macos-issue55-r2.log`, `582.457` bayt, SHA-256 `042ffeeb60f45013dcf5c0c03a1d0a308e1cf1406fd5d3daa83e5e38c17ac34f`.
- Runtime log: `runtime-processor-issue55-r2.log`, `5.000` bayt, SHA-256 `b9d0fd1dff5d702f3c74d67e09c1b11dc5e30028effaece3045cd7993581e799`.

Bu çıktı yalnız yerel geliştirme kanıtıdır. İmzalı/notarize dağıtım paketi değildir; gerçek Windows x64 hostta IL2CPP/DirectX/Steam kapısı ayrı kalır.

## Stage B deterministic single air-cooler seating ve four-point retention doğrulaması — 22 Ağustos 2026

`macOS/PC Shop Empire 3D.app`, Issue #58 feature `e2f10a2` kaynak durumuyla yeniden üretildi:

- Tür: Development + StrictMode Player, Universal Mach-O (`arm64` + `x86_64`), ad-hoc signed.
- Unity build raporu: `328.534.723` bayt.
- Ana executable: `117.179` bayt.
- Runtime: aktif Apple Silicon/Metal makinesi Apple M1, pencereli 1280×720; `garage-processor-cooler-r27-v1` readiness ve exact cooler smoke başarılı.
- Test kapıları: EditMode `521/521`, PlayMode `38/38`; failed/skipped/inconclusive `0`.
- Build log: `build-macos-issue58-final.log`, `585.965` bayt, SHA-256 `e32a2a1c8b661a8320e14511eee9d415d6b07c649594cd503221c9e23de99bed`.
- Runtime log: `runtime-cooler-issue58-metal-final.log`, `5.282` bayt, SHA-256 `365bfd3ad8302f65af5a2121a4c36f0c5029d4128694a263cce1dc439b3f32d1`.

Bu çıktı yalnız yerel geliştirme kanıtıdır. İmzalı/notarize dağıtım paketi değildir; gerçek Windows x64 hostta IL2CPP/DirectX/Steam kapısı ayrı kalır.

## Stage B deterministic single ATX24 split-PSU cable routing doğrulaması — 23 Ağustos 2026

`macOS/PC Shop Empire 3D.app`, Issue #61 feature `1fc29f1` kaynak durumuyla yeniden üretildi:

- Tür: Development + StrictMode Player, Universal Mach-O (`arm64` + `x86_64`), ad-hoc signed.
- Unity build raporu: `329.082.160` bayt.
- Ana executable: `117.179` bayt; SHA-256 `04060db71ecd39f083a526b88e9468bd26ca26c18b12499b6e9dca3da19d85ab`.
- Runtime: aktif Apple M1/Metal makinesi, pencereli 1280×720; `garage-atx24-power-cable-routing-r30-v1` readiness ve exact cable smoke başarılı.
- Test kapıları: EditMode `589/589`, PlayMode `49/49`; failed/skipped/inconclusive `0`.
- Build log: `build-macos-issue61-final.log`, `605.068` bayt, SHA-256 `e8730ba8f2975c16fbdc8034f6554aeb32313048c2ce64202087795d7d930c4e`.
- Runtime log: `runtime-power-cable-issue61-final.log`, `5.628` bayt, SHA-256 `c25c8cb9e95039d57b0ec70294f95efb69112705cd32baa171d65e34833df2d3`.

Bu çıktı yalnız yerel geliştirme kanıtıdır. İmzalı/notarize dağıtım paketi değildir; gerçek Windows x64 hostta IL2CPP/DirectX/Steam kapısı ayrı kalır.

## Stage B deterministic single ATX PS/2 PSU seating ve four-screw rear retention doğrulaması — 23 Ağustos 2026

`macOS/PC Shop Empire 3D.app`, Issue #60 feature `f998d7d` + authored-clearance fix `b6c3ff8` kaynak durumuyla yeniden üretildi:

- Tür: Development + StrictMode Player, Universal Mach-O (`arm64` + `x86_64`), ad-hoc signed.
- Unity build raporu: `328.937.592` bayt.
- Ana executable: `117.179` bayt; SHA-256 `44045bf514841be7bd268e9032448583499bc416fe809ceac0196dd51b0e91f6`.
- Runtime: aktif Apple Silicon/Metal makinesi Apple M1, pencereli 1280×720; `garage-psu-four-screw-r29-v1` readiness ve exact PSU smoke başarılı.
- Test kapıları: EditMode `577/577`, PlayMode `47/47`; failed/skipped/inconclusive `0`.
- Build log: `build-macos-issue60-final.log`, `585.248` bayt, SHA-256 `462d0f5d3d07de4314ab89b356adc529e854541a332d1d43bf954a457e2dd305`.
- Runtime log: `runtime-psu-issue60-final-activated.log`, `7.468` bayt, SHA-256 `574eb272912dcac4ca18590954a18fd6e711c4ef88576f713bccaba14b437b40`.

Bu çıktı yalnız yerel geliştirme kanıtıdır. İmzalı/notarize dağıtım paketi değildir; gerçek Windows x64 hostta IL2CPP/DirectX/Steam kapısı ayrı kalır.

## Stage B deterministic single PCIe x16 graphics-card seating ve rear-bracket retention doğrulaması — 22 Ağustos 2026

`macOS/PC Shop Empire 3D.app`, Issue #59 feature `1b29ad2` kaynak durumuyla yeniden üretildi:

- Tür: Development + StrictMode Player, Universal Mach-O (`arm64` + `x86_64`), ad-hoc signed.
- Unity build raporu: `328.781.520` bayt.
- Ana executable: `117.179` bayt; SHA-256 `c3849bc7dfa05c1116c772952ad77085cad86f7feab22bf8dcca43478ff8fbea`.
- Runtime: aktif Apple Silicon/Metal makinesi Apple M1, pencereli 1280×720; `garage-gpu-rear-bracket-r28-v1` readiness ve exact GPU smoke başarılı.
- Test kapıları: EditMode `548/548`, PlayMode `43/43`; failed/skipped/inconclusive `0`.
- Build log: `build-macos-issue59-final-r2.log`, `584.629` bayt, SHA-256 `ed9ff2282c816a159eb6947c15c5076f7c91125b52ca70a84ef7a27a5a6f80d9`.
- Runtime log: `runtime-gpu-issue59-metal-final-r2.log`, `5.386` bayt, SHA-256 `f8c1d5d8c79c58a7fc3b2a7ca162a8d6f3a1d27b30ae44a2046f77ebee1fccd2`.

Bu çıktı yalnız yerel geliştirme kanıtıdır. İmzalı/notarize dağıtım paketi değildir; gerçek Windows x64 hostta IL2CPP/DirectX/Steam kapısı ayrı kalır.

## Stage B deterministic single M.2 NVMe seating ve captive screw doğrulaması — 16 Ağustos 2026

`macOS/PC Shop Empire 3D.app`, Issue #57 feature `4f14e7b` kaynak durumuyla yeniden üretildi:

- Tür: Development + StrictMode Player, Universal Mach-O (`arm64` + `x86_64`), ad-hoc signed.
- Unity build raporu: `328.362.356` bayt.
- Ana executable: `117.179` bayt.
- Runtime: Apple M4/Metal, pencereli 1280×720; `garage-m2-nvme-captive-screw-r26-v1` readiness ve exact storage smoke başarılı.
- Test kapıları: EditMode `490/490`, PlayMode `35/35`; failed/skipped/inconclusive `0`.
- Build log: `build-macos-issue57-final.log`, `600.974` bayt, SHA-256 `560a20ee380ffe5fd76e12b5c48d5dc843557e27b5a571ea90c6eefac51baad3`.
- Runtime log: `runtime-storage-issue57-final.log`, `5.206` bayt, SHA-256 `5e8a250452c5a487692646b0626dd6aa03ccacd68267a6c37cab62e083ebb858`.

Bu çıktı yalnız yerel geliştirme kanıtıdır. İmzalı/notarize dağıtım paketi değildir; gerçek Windows x64 hostta IL2CPP/DirectX/Steam kapısı ayrı kalır.

## Stage B deterministic single DIMM seating ve dual-latch retention doğrulaması — 16 Ağustos 2026

`macOS/PC Shop Empire 3D.app`, Issue #56 feature `7482fc9` kaynak durumuyla yeniden üretildi:

- Tür: Development + StrictMode Player, Universal Mach-O (`arm64` + `x86_64`).
- Unity build raporu: `328.268.700` bayt.
- Ana executable: `117.179` bayt; SHA-256 `eba2a0baeecb9a214a3d0520f4a94641e84b697b3d79f785ec124e4d1932eb50`.
- Runtime: Apple M4/Metal, pencereli 1280×720; `garage-dimm-dual-latch-r25-v1` readiness ve exact DIMM smoke başarılı.
- Test kapıları: EditMode `461/461`, PlayMode `33/33`; failed/skipped/inconclusive `0`.
- Build log: `build-macos-issue56-final.log`, `582.591` bayt, SHA-256 `49fd863b79bb50b3138471c6efbf7d33a33f66e2f482175abf529b18baa38c3d`.
- Runtime log: `runtime-dimm-issue56-final.log`, `5.140` bayt, SHA-256 `03d45cac685bbe1295ec2181ff7d3a36aed16289ce272bb813b1de4f46b6cc4f`.

Bu çıktı yalnız yerel geliştirme kanıtıdır. İmzalı/notarize dağıtım paketi değildir; gerçek Windows x64 hostta IL2CPP/DirectX/Steam kapısı ayrı kalır.
