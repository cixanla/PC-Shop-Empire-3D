# ADR-0002 — Yerel Git güvenlik temeli

**Durum:** Kabul edildi ve uygulandı  
**Tarih:** 11 Ağustos 2026

## Bağlam

Unity Cloud içindeki private UVCS repo oluşturuldu; ancak ilk repository preflight/check-in iki resmî istemci yolunda aynı uzak `connection reset by peer` hatasıyla durdu. Yerel `.plastic` workspace veya uzak changeset oluşmadı. Oynanış üretimine sürüm geçmişi olmadan başlamak gereksiz geri alma riski yaratır.

## Karar

- Mevcut Apple Git 2.50.1 ile Unity proje kökünde yerel Git deposu oluşturulur.
- İlk commit, doğrulanmış Stage A kaynak temelidir.
- UVCS şimdilik beklemeye alınır; Git ve UVCS aynı anda iki authoritative sistem olarak işletilmez.
- Bu ADR tarihinde Git deposunun uzak remote'u yoktu; off-device güvenlik katmanı SHA-256 doğrulamalı USB milestone snapshot'ıydı. 11 Ağustos 2026 tarihli ADR-0006, private GitHub remote'u yeni canonical iş birliği katmanı olarak ekler.
- Kaynak henüz küçük ve metin ağırlıklı olduğundan Git LFS kurulmaz. Büyük binary asset kabulünden önce Git LFS ve remote seçimi ayrı kapıda kesinleştirilir.
- Git kimliği yalnız bu depoda `Cixanla <cixanla@users.noreply.local>` olarak tutulur; global Git ayarı değiştirilmez.

## Güvenlik kapıları

- `Library`, `Temp`, log/cache, IDE çıktıları, buildler, credential ve canlı UVCS metadata commit edilmez.
- Her commit öncesi staged dosya listesi, boyutu ve bilinen secret kalıpları kontrol edilir.
- Remote ekleme kararı ADR-0006 ile onaylanmıştır. Git LFS kurulumu veya UVCS'ye geri dönüş hâlâ ayrı karar ve doğrulama gerektirir.
