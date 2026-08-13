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
