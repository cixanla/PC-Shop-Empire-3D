# Catalog + Inventory Checkpoint Kanıtı — 15 Ağustos 2026

## Kapsam

- Epic [#7](https://github.com/cixanla/PC-Shop-Empire-3D/issues/7), alt iş [#38](https://github.com/cixanla/PC-Shop-Empire-3D/issues/38).
- Feature commit: `71935f11b80d02d03f9dcc1a3f08cafca7e301ff` (`feat: add authoritative catalog and inventory core`).
- `PSE.Catalog`: immutable ürün tanımı, stable kimlik, kategori, tracking policy, görünür ad ve bounded garanti.
- `PSE.Inventory`: serialized item, batch position, container unit capacity, atomik transfer, claim reservation, release/consume, deterministic query ve invariant audit.
- Unity dünya projeksiyonu, Orders, para/fiyat, event publication ve save kapsam dışıdır.

## Otomatik doğrulama

| Kanıt | Sonuç | SHA-256 |
|---|---|---|
| `catalog-inventory-editmode.xml` | `161/161` geçti, `0` failed/skipped | `626757772e5cae48ce1531ddca35b544ebba986bd34a9f32ddea6b7f758663f0` |
| `catalog-inventory-playmode.xml` | `14/14` geçti, `0` failed/skipped | `69d89a0f7d2943ceb2793cf75db2cffd689bed37ae54d6303e013510835d21f8` |
| `./Tools/verify-repository.sh` | `REPOSITORY_GUARD_OK`, Unity `6000.3.21f1`, legacy `26/26` | Komut sonucu |

Ham Unity raporları repository dışında `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/` altında tutulur. Bu paket saf C# domain assembly'leri ve testlerden oluştuğu, sahne/prefab/runtime sunumu değiştirmediği için yeni player build üretilmedi; son doğrulanmış Universal macOS player ve Apple M4/Metal cart smoke kanıtı geçerliliğini korur.

## Kabul kanıtı

- Invalid/unknown/duplicate ID, tracking mismatch, sıfır-negatif quantity, mixed capacity aşımı, over-transfer ve over-reserve failure sonuçları state ile revision'ı değiştirmez.
- Serialized reservation exclusivity, transfer sonrası sahiplik ve tek consume davranışı testlidir.
- Batch split aynı kimliği ve toplam miktarı korur; source reservation kilidi ayrılmamış miktarın taşınmasını engeller.
- Catalog, container, item ve batch-position listeleri ordinal stable-ID sırasındadır.
- Karma serialized+batch state sonunda invariant audit başarıyla tamamlanır.

## Provenans ve platform sınırı

Yeni asset, paket, marka verisi veya üçüncü taraf bağımlılık eklenmedi. Uygulama mevcut Unity/.NET ve `PSE.Core` primitives üzerine kuruldu. Native Windows x64/IL2CPP kapısı bu saf domain checkpointiyle çözülmüş sayılmaz.

## Remote ve USB kapanışı

- Checkpoint commit: `9e0cb2d6476ab0bfac8918368454a0917744ee36`.
- Repository Guard: [31861777253](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31861777253), başarılı.
- Issue #38 ve Epic #7 Closed/Done.
- USB: `2026-08-15_STAGE_B_CATALOG_INVENTORY`; 428 tracked source, 4 test evidence, 1 source kaydı ve 433 manifest satırı.
- USB manifest SHA-256: `f481ddfaf6627bdd34137225fe754e90065b85e7cfc012a1a19c651337c49dc9`; manifest/source mismatch, yasak yol ve AppleDouble sayısı `0`.
