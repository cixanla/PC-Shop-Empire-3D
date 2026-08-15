# Purchase Order + Receiving Checkpoint Kanıtı — 15 Ağustos 2026

## Kapsam

- Epic [#8](https://github.com/cixanla/PC-Shop-Empire-3D/issues/8), alt iş [#39](https://github.com/cixanla/PC-Shop-Empire-3D/issues/39).
- Feature commit: `e596e079d90b6d5b9d94714d7821502574eba3c9`.
- `PSE.Orders`: stable purchase order/supplier/delivery kimliği, deterministic order lines, monotonik lifecycle ve exact delivery manifest.
- `PSE.Inventory`: immutable mixed intake ve bütün satırlar için preflight sonrası tek-revision kabul.

## Otomatik doğrulama

| Kanıt | Sonuç | SHA-256 |
|---|---|---|
| `orders-receiving-editmode.xml` | `184/184` geçti, `0` failed/skipped | `4114e3483ed820f5061210402599bedc0e2116cdcdd7cf21305793024f2d42df` |
| `orders-receiving-playmode.xml` | `14/14` geçti, `0` failed/skipped | `1e2bbae00d8116b363d7dc069bb677973a6264c2dbc6840cf34e1706435ef07b` |
| `./Tools/verify-repository.sh` | `REPOSITORY_GUARD_OK`, Unity `6000.3.21f1`, legacy `26/26` | Komut sonucu |

Ham raporlar repository dışında `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/` altındadır. Paket saf domain kodu/testidir; sahne, prefab veya runtime sunumu değişmediği için player yeniden build edilmedi. Son Universal macOS ve Apple M4/Metal fiziksel cart smoke kanıtı geçerliliğini korur.

## Kabul kanıtı

- Purchase order onayı, dispatch ve arrival sırasında Inventory quantity `0` kalır.
- Exact iki serialized + dört batch-unit manifest kabulünde receiving quantity tek komutla `6` olur.
- Eksik, ekstra, yanlış delivery ID veya tracking policy manifesti `Arrived` durumuna geçemez.
- Capacity, yanlış container kind veya önceden var olan geç bir item identity kabulü tamamen reddeder; order `Arrived`, Inventory aynı state/revision'da kalır.
- İkinci acceptance geçersiz state transition'dır ve quantity `6` olarak kalır.
- Order/line/intake sorguları stable ID ordinal sırasında döner; iki authority invariant audit'i geçer.

## Kapsam ve provenans sınırı

Yeni asset, paket, marka verisi veya üçüncü taraf bağımlılık eklenmedi. Fiyat/para, ödeme, partial/damaged claim, event publication, persistence, Dashboard ve 3D delivery/raf projection bu kanıtın dışında açık kalır. Native Windows x64/IL2CPP doğrulaması yapılmış sayılmaz.

## Remote ve USB kapanışı

- Checkpoint commit: `c4aed4b050dd92c1f5aaa65261d4d1bc009528b3`.
- Repository Guard: [31862730318](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31862730318), başarılı.
- Issue #39 Closed/Done; Epic #8 In Progress.
- USB: `2026-08-15_STAGE_B_ORDER_RECEIVING`; 449 tracked source, 4 test evidence, 1 source kaydı ve 454 manifest satırı.
- USB manifest SHA-256: `07480d15d2f2b187d7e84383c6f45f011be1f8a0056c4075f06103d92f485cff`; manifest/source mismatch, yasak yol ve AppleDouble sayısı `0`.
