# Okunaklı yarı gerçekçi garaj benchmarkı — 13 Ağustos 2026

Issue #34 kapsamında GarageGraybox'ın tek referans köşesine gerçek ölçekli bevel'lı tezgâh/raf, metal-ahşap-karton-beton yüzey ayrımı, etiket detayları, sıcak görev ışığı, ACES tonemapping, ölçülü bloom ve reflection probe eklendi. Bu kalite benchmarkıdır; bütün garajın final sanatı değildir.

Tüm geometri ve 64×64 yüzey desenleri proje içinde deterministik üretildi; üçüncü taraf model, doku, marka veya ücretli asset eklenmedi. Pickup/drop/placement/rotation/recovery, stable ID, collider ve fiziksel yarı-boyut sözleşmeleri korundu. ProBuilder düzenleme bileşenleri kaydedilen runtime sahnesinden çıkarıldı.

- Feature commit: `c7214afab81a360a3ca10a88cbdd29f67e741994`; tree `cb734bdc31069f584999558c8d8bdb78e2c968cc`.
- Repository Guard: [31688852779](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31688852779), başarılı.
- EditMode: `128/128`; XML SHA-256 `7580bdaaabf47205031cff2fed2d1e63bb0754612730c7ef24ed16b9724fd3d3`.
- PlayMode: `10/10`; XML SHA-256 `95ae6c5e802647c2bd3eef72cd39aa72c1075e84114d7cc20f89a775672bbb79`; `lookdev=ok`.
- Universal macOS build: 327.211.997 bayt; executable Mach-O `x86_64 + arm64`.
- Apple M4/Metal gerçek player: 1280×720, `rotation=ok lookdev=ok`, normal kapanış.
- Görsel kanıt SHA-256: `64081d5039d2257b57e252452a1d14e0fb95a2b2834319b0614eee820fa7f5dd` (`../TestResults/lookdev-benchmark-detail-final.png`, Git dışı).

Windows x64/DirectX/IL2CPP/Steam doğrulaması ayrı platform kapısıdır. Sıradaki bounded iş küçük-kutu üstü tam destekli istiflemedir.
