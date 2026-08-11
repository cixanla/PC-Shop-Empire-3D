# ADR-0003 — Saf çekirdek assembly sınırı

**Durum:** Kabul edildi ve uygulandı  
**Tarih:** 11 Ağustos 2026

## Karar

- İlk Stage B paketi yalnız `PSE.Core` runtime assembly sınırını kurar.
- Assembly `noEngineReferences: true` kullanır; `UnityEngine` ve `UnityEditor` referansları alan çekirdeğine giremez.
- Şimdilik yalnız assembly'nin kararlı adını sağlayan bir anchor tipi vardır. Kimlik, zaman, sonuç veya olay API'leri sözleşmeleri ve davranış testleri kesinleşmeden eklenmez.
- Edit Mode mimari testleri assembly adını ve Unity sunum bağımlılığı bulunmadığını doğrular.

## Sonuç

Bu paket gameplay davranışı veya içerik üretmez. Sonraki alan modülleri `PSE.Core` yönüne bağımlı olabilir; `PSE.Core` onların veya Unity sunum katmanının tiplerini bilemez.
