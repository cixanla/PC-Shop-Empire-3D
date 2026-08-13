# ADR-0006 — Private GitHub canonical remote ve yaşayan devir paketi

**Durum:** Uygulandı; Codex proje kaydı maddesi 13 Ağustos 2026'da D-157 ile revize edildi<br>
**Tarih:** 11 Ağustos 2026

## Bağlam

Yerel Git ve USB snapshot geri alma sağlasa da farklı bilgisayardan veya yeni collaborator ile kesintisiz devam için erişilebilir private remote, görev panosu ve kendi kendini açıklayan repository gereklidir. Mevcut public `cixanla/PC-Shop-Empire` eski Electron 1.1.6 release/indirme kimliğidir ve yalnız README ana branch'i taşır.

## Karar

- Yeni Unity oyunu için private `cixanla/PC-Shop-Empire-3D` repository oluşturulur.
- Mevcut public `cixanla/PC-Shop-Empire` silinmez, force-push edilmez veya yeni authoritative source yapılmaz; legacy release/download geçmişi olarak kalır.
- Yerel `main` geçmişi ve `stage-a-baseline-2026-08-11` etiketi private remote'a push edilir.
- Tam araştırma/tasarım paketi `Docs/ProjectBible`, genel handoff root `PROJECT_BIBLE.md` içinde tutulur.
- Canonical legacy 1.1.6 kaynak snapshot'ı, hak/third-party notice dosyaları ve SHA-256 manifesti private repo içine salt okunur referans olarak alınır.
- GitHub Issues görev gerçeği; GitHub Project görünür yol haritasıdır. Kalıcı kararlar repository belgelerine geri yazılır.
- Her material PR Project Bible/checkpoint güncelliğini doğrular.
- Codex'te gerçek Unity Git kökü ayrı Project olarak erişilebilir hâle getirilir. **Sonraki revizyon:** Ayrı kayıt gereksiz bulundu ve 13 Ağustos 2026'da kaldırıldı; çalışma mevcut ana Codex projesinde sürer.

## Güvenlik ve maliyet

- Repository private başlar; public dönüşüm ayrı secret, lisans, marka ve release incelemesi ister.
- Build/cache/log, token, certificate/private key ve gerçek kullanıcı verisi push edilmez.
- Mevcut kaynak küçük olduğundan Git LFS eklenmez; büyük binary asset öncesi storage/bandwidth kapısı vardır.
- Unity lisansı gerektiren cloud CI bu kararla otomatik etkinleştirilmez. Hafif repository guard yalnız kaynak hijyeni ve belge/manifest bütünlüğünü kontrol eder.

## Sonuç

Private [`cixanla/PC-Shop-Empire-3D`](https://github.com/cixanla/PC-Shop-Empire-3D) oluşturuldu; `main`, Stage A etiketi, yaşayan devir belgeleri, repository guard/workflow ve canonical legacy snapshot normal push ile gönderildi. [Development Roadmap Project #2](https://github.com/users/cixanla/projects/2), 22 epic ve Codex'te `Game` Project kaydı oluşturuldu. Eski public repository değiştirilmedi.

`Game` Codex kaydı 13 Ağustos 2026'da kaldırıldı. Bu, yalnız Codex uygulama kaydı değişikliğidir; Unity klasörü, Git geçmişi, private remote, Issues ve Project #2 bu revizyondan etkilenmedi.

Bu ADR, ADR-0002 içindeki “remote yok” durumunu ileriye dönük olarak değiştirir; Stage A root commit/tag ve UVCS bekleme kararı korunur.
