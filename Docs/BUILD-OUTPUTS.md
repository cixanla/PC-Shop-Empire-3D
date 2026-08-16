# Builds

`../../Builds/Local/` yalnız yerel doğrulama buildleri içindir ve UVCS workspace'inin dışında kalır. Yayın buildleri ayrı, imzalı ve denetlenmiş bir süreçte üretilecektir.

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
