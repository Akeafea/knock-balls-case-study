# Knock Balls · İki Günlük Clone Case Study

[English](README.md) · [Teknik inceleme](docs/ARCHITECTURE.md) · [Asset kurulumu](docs/ASSET-SETUP.md)

![Knock Balls — iki günlük Unity case study](docs/images/banner.svg)

**Knock Balls'un temel oyun döngüsünü iki günde yeniden oluşturduğum bir Unity case study projesi.** Oyuncu bir topu yönlendirerek blok yapılarını yıkmaya ve sınırlı mühimmatla aşamaları tamamlamaya çalışır.

Bu çalışmada hedefim; fizik tabanlı oynanışı bölüm ilerlemesi, skor, güçlendirme ve oyun sonu akışlarıyla tamamlayarak kısa sürede bütünlüklü bir prototip üretmekti. Mevcut repodaki Unity 6.6 uyumluluk ve arayüz iyileştirmeleri, ilk iki günlük çalışmadan sonra yapılan bakım güncellemeleridir.

## Oyundan görüntüler

| Nişan alma ve mühimmat | Patlama ve skor geri bildirimi | Bölüm ve tema çeşitliliği |
| --- | --- | --- |
| ![Topla nişan alma](docs/images/precision-shot.png) | ![Blokların patlamayla dağılması](docs/images/explosion-feedback.png) | ![Yeşil temalı sonraki bölüm](docs/images/level-theme.png) |

## Teknik çalışmalar

- **3 ana bölüm ve 15 alt bölüm** içeren ScriptableObject tabanlı veri yapısı.
- Toplar, bloklar, cam parçaları, efektler ve skor yazıları için Object Pooling.
- Kırılabilir cam, patlayıcı variller ve skorla dolan seri atış güçlendirmesi.
- Mühimmat takibi, kamera sarsıntısı, geri tepme ve ses bağlantıları.
- Menü, tamamlanma, kaybetme geri sayımı ve yeniden başlatma akışları.
- Rekor, ana bölüm ilerlemesi ve ses tercihinin kaydı.
- Sahnedeki işaretleyicilerden bölüm verisi çıkaran editör aracı.
- Bölüm paletini takip eden arayüz; editörden **Themed / Legacy** seçimi.

**Teknolojiler:** Unity 6000.6.4f1, C#, URP, TextMesh Pro, DOTween.

## Açık kaynak kapsamı

Bu repo **kaynak kodu ve proje yapısını** paylaşır. Asset Store VFX paketleri ve yeniden dağıtım hakkı doğrulanmamış model, doku, UI ve ses dosyaları dahil değildir. Görseller, bu assetlerin bulunduğu tam yerel prototipi gösterir. Public checkout doğrudan oynanabilir bir build değildir; [kurulum ve eksik bağımlılıklar](docs/ASSET-SETUP.md) açıklanmıştır.

Özgün kod ve projeye ait veri yapıları [MIT lisansı](LICENSE) altındadır. Üçüncü taraf içeriklerin hakları kendi sahiplerinde kalır. Knock Balls'un geliştiricileri veya yayıncılarıyla bağlantısı olmayan, eğitim ve portfolyo amacıyla hazırlanmış bir clone case study'dir.

**Geliştirici:** [Akif Duman](https://github.com/Akeafea)
