# Büyük Kutu Taşıma Checkpoint'i — 13 Ağustos 2026

## Görünür sonuç

GarageGraybox artık küçük parçalar kutusunun yanında ayrı boyut ve kimliğe sahip turuncu bantlı büyük kargo kutusu içerir. Oyuncu kutuyu `E / Gamepad South` ile iki-el pozunda alır; hareketi `0,65×` olur, sprint kapanır ve etkin `G / Gamepad East` promptuyla güvenli bırakır. Duvar, oyuncu veya nesne çakışmasında bırakma fail-closed kalır ve kutu kaybolmaz.

## Kaynak kanıtı

- Feature commit: `e94419862b04f6f03f97ef2e43c9da393c5d30a9`
- Feature tree: `da877668c89850e4d384c30aefe7e5cc175d317d`
- Epic/issue: [#6](https://github.com/cixanla/PC-Shop-Empire-3D/issues/6) / [#32](https://github.com/cixanla/PC-Shop-Empire-3D/issues/32)
- Karar: [`ADR-0011-LARGE-BOX-CARRY-PROFILE.md`](../ADR-0011-LARGE-BOX-CARRY-PROFILE.md)
- Remote Repository Guard: [Actions run 31680394879](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31680394879), başarılı.

## Otomatik doğrulama

Ham çıktılar Git dışındaki `/Users/cixanla/Developer/PCShopEmpire3D/TestResults` klasöründedir.

| Kanıt | Sonuç | SHA-256 |
|---|---|---|
| `large-box-editmode-final.xml` | 126/126 geçti | `aabf0920d0105fe78e1ea55275360e8d7a17b74c5d8d9c510a18995fb7562812` |
| `large-box-editmode-final.log` | Unity batch temiz tamamlandı | `2b6fd80014e2f531eeacd42c9037cf3ad84b0e14a401417e9b7e53b931820cce` |
| `large-box-playmode-final.xml` | 10/10 geçti | `d5be5ce304af1dc95c59b1b2ba44e068c819a00a02dfa3f692b5b6fb761cd5fb` |
| `large-box-playmode-final.log` | Gerçek Input System ve `large-carry=ok` | `fa8218d42297a9634163daf40f4e1552fa1608694c177564965e2c713eb2b171` |

PlayMode; keyboard ve gamepad pickup/drop zincirlerini, etkin binding promptlarını, placement girişinin büyük kutuda kapalı kalmasını, sprint engelini, `0,65×` hızı, bounded FOV geçişini, iki-el durumunu, blocked drop'u, stable ID'yi ve büyük-kutu disable recovery'sini doğrular. Önceki küçük-kutu pickup/drop/placement testleri aynı koşuda geçer.

## Build ve gerçek player smoke

| Kanıt | Sonuç | SHA-256 |
|---|---|---|
| `large-box-macos-build.log` | Universal development build; 326.157.117 bayt | `984122a4028f633667f76548917989421dbeb9659b570c5fb352785b803e4f0c` |
| Player executable | Mach-O `x86_64 + arm64` | `571b84ed43da87f2bd0c348771f8ff97e10180e4e3bcc5a35fcf4a7a744ffe11` |
| `large-box-macos-runtime.log` | Unity 6000.3.21f1, Apple M4/Metal, 1280×720, `large-carry=ok` | `d4807fd9112a7f2c29774db0ca2f0b7d188876b99ab832b3f0f94d636c51bb41` |
| `large-box-macos-runtime.png` | Gerçek player'da küçük ve büyük kutunun görünür graybox kanıtı | `15da3b5de9078298368e8dd21711020cc32dc2199e59b1722df80190fcf89ec1` |

Runtime açılışında crash, missing script veya `NullReferenceException` görülmedi; oyuncu penceresi normal kapatıldı. Mac kanıtı gerçek Windows x64, DirectX, Steam veya IL2CPP doğrulamasının yerine geçmez.

## Açık sınır

- Büyük kutu kontrollü placement, rotation ve stacking desteklemez.
- Taşıma arabası henüz yoktur.
- Dünya nesnesi authoritative stok değildir; Catalog/Inventory/Orders bağlantısı Issue #7/#8'e aittir.
- Eller ve kutular graybox geometridir; final sanat/animasyon değildir.

Sıradaki bounded iş [#33](https://github.com/cixanla/PC-Shop-Empire-3D/issues/33) ile yalnız küçük-kutu placement moduna kasıtlı `90°` rotation inputu eklemektir; istifleme ayrı acceptance paketi olarak kalır.
