# Küçük Kutu Placement Teknik Kanıtı — 13 Ağustos 2026

## Kaynak

- Feature commit: `720e6d4ac2b2afad9ee86f907c533cbabb1bf5ed`
- Tree: `27652c274c849a127a20b1f52960f435760111eb`
- Epic: [#6 — Hibrit kutu taşıma ve placement](https://github.com/cixanla/PC-Shop-Empire-3D/issues/6)
- Bounded issue: [#31 — Küçük kutu kontrollü placement ve ghost önizleme](https://github.com/cixanla/PC-Shop-Empire-3D/issues/31)
- Karar: [`ADR-0010`](../ADR-0010-CONTROLLED-SMALL-BOX-PLACEMENT.md)

## Görünür oyun sonucu

- Garajda teal renkli, işaretli küçük-kutu stok yüzeyi ve kısmi engel bulunur.
- Kutuyu aldıktan sonra `Mouse Left / Gamepad RT` yeşil veya kırmızı ghost'u açar; HUD effective binding ile `GEÇERLİ/ENGELLİ` durumunu yazar.
- `G / Gamepad East` geçerli placement'ı onaylar. Engelde kutu elde kalır; geçerli placement aynı stable ID'yi 0,25 m grid/90° yaw snap pozunda sabitler.
- Placement modu kullanılmazsa önceki güvenli drop ve recovery davranışı devam eder.

## Doğrulama

| Kanıt | Sonuç | SHA-256 |
|---|---|---|
| `placement-editmode-final.xml` | 123/123 geçti | `694d5f139e019c83de36d6d3981965cd784f144738319060e7d1d075536aa8d6` |
| `placement-playmode-final.xml` | 8/8 geçti | `fb9ce67ddc62f6d603bd707c300de9c3c618dfed63eedaf37d0b0909181b1674` |
| `placement-macos-build-final.log` | Universal development build, 326.147.564 bayt | `5bab0c612bd756db2dba8f5183479aac3e08c00f75007b91ae17b15077351097` |
| `placement-macos-runtime-final.log` | Apple M4/Metal, 1280×720, `placement=ok` | `a669109c6638a428f89e1d7c87f743c711aac5aaa566624cd6b844e14d61bf4f` |
| `placement-macos-runtime-final.png` | Gerçek player'da kırmızı `ENGELLİ` ghost | `c4208a2ea1227591ffa407f64ef0b6a3e5c12915648dc2dfe8389fad55a39122` |

Ham test/build/runtime dosyaları Git dışındaki `../TestResults` klasöründedir. `./Tools/verify-repository.sh` ve `git diff --check` geçti. Windows native/IL2CPP/DirectX/Steam doğrulaması bu Mac kanıtının dışında kalır.

## USB milestone

- Hedef: `90_BACKUPS/PCShopEmpire3D/2026-08-13_STAGE_B_SMALL_BOX_PLACEMENT`
- Snapshot source commit: `7794e2ab82c3b26c1149af526ed582f1cc406acb`
- Payload: 336 tracked dosya / 5.928.850 mantıksal bayt.
- Manifest SHA-256: `b4df8efde544cbe3557bf67f67c13034733949821bdc7848ce612af1129be0fb`.
- İki source→USB checksum dry-run ve iki tam manifest readback doğrulaması geçti. exFAT zaman damgası çözünürlüğü dışında fark yoktur; `.git`, cache, build, log ve credential dahil edilmedi.

## Kapsam sınırı

Bu checkpoint küçük kutunun kontrollü stok placement'ını tamamlar. Büyük kutu hız/görüş bedeli, kullanıcı rotation inputu, istifleme, taşıma arabası ve authoritative Inventory sonraki bağımlı dilimlerdir.
