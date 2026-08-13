# Garage Graybox Checkpoint — 13 Ağustos 2026

## Sonuç

PC Shop Empire 3D artık yalnız teknik çekirdek değildir. Yeni `GarageGraybox` sahnesi editör ve development player içinde açılır; oyuncu klavye/fare veya gamepad ile yürür, sprint yapar, kamerayı çevirir ve pause durumuna geçer. Birinci şahıs görünür eller prototip olarak kameraya bağlıdır.

## Kaynak kanıtı

- Feature commit: `c7a3a26075998252d9ae8b88824d8285e5067069`
- Feature tree: `6d63d724e40b18efdc29269c5b5d305ccf5a4373`
- Scene: `Assets/Scenes/Prototypes/GarageGraybox.unity`
- Player prefab: `Assets/Prefabs/Prototype/PlayerRig.prefab`
- Assembly yönü: `PSE.Presentation → PSE.World → PSE.Core`; Core Unity bağımsız kalır.

## Otomatik doğrulama

- Edit Mode: 114/114 geçti.
- Play Mode: 2/2 geçti.
- Play Mode gerçek Input System device-state olaylarıyla keyboard sprint/move, mouse look ve gamepad move/look davranışlarını motor üzerinde doğruladı.
- Repository Guard: geçti.
- `git diff --check`: geçti.
- Bağımsız gameplay ve repo incelemelerinde kritik/önemli açık kalmadı.

## Player doğrulaması

- macOS Universal development build: başarılı, 325.932.692 bayt.
- Mimari: `arm64 + x86_64`.
- Headless smoke: `GARAGE_GRAYBOX_RUNTIME_READY version=garage-graybox-g1-v1 scene=GarageGraybox ... motor=ok input=ok`.
- Smoke sırasında crash, missing script veya NullReference görülmedi.

## Bilinen sınırlar

- Eller placeholder geometridir; final sanat veya animasyon değildir.
- Interact/PrimaryAction/Drop inputları tanımlıdır fakat fiziksel pickup/drop henüz uygulanmadı.
- Head-bob ve sprint FOV varsayılan kapalıdır.
- Jump/crouch prototip kapsamında değildir.
- Mac build Windows x64, DirectX, Steam veya IL2CPP kanıtı değildir.

## Sonraki kabul kapısı

Issue #5: oyuncu garajdaki uygun bir kutuyu erişim/line-of-sight kurallarıyla alır, elde güvenli taşır ve geçerli bir noktaya bırakır; nesne/ekonomi kimliği kaybolmaz ve başarısız drop güvenli fallback üretir.
