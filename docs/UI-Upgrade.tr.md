# Unity 6.6 ve arayüz güncellemesi

Ana sahne: `Assets/Scenes/PlayGround.unity`.

## Görünümü değiştirme

- Inspector: `MainCanvas > Game UI Theme Controller > Appearance`.
- `Themed`: bölümün blok paletini takip eden vurgu renkleri, düzenlenmiş ana menü, tutarlı yazılar ve HUD.
- `Legacy`: özgün renkler, yazı özellikleri, tıklama hedefleri ve ana menü yerleşimi.
- Menüden de seçilebilir: `Tools > Knock Balls > UI > Themed / Legacy`.

Edit Mode'da seçip sahneyi kaydederseniz tercih sonraki açılışta ve build'de korunur. Play Mode'daki seçim Unity'nin normal davranışı gereği oyun durdurulunca geri alınır. Özgün değerler bileşenin içinde saklanır; yeni görünümle sahneyi kaydetmek eski görünümü silmez.

Palet kaynağı `LevelthemeController > Themes` içindeki `blockHighColor` değeridir. Bölüm teması değişince arayüz de güncellenir.

Yeni görünümde kaybetme ekranı ana arayüz gibi 1080×1920 referans çözünürlüğüyle ölçeklenir; dar ekranlarda geri sayımın kesilmesi giderilir. Legacy seçeneği özgün ölçek ayarını geri getirir.

## Uyumluluk

Nesne aramalarında [Unity 6.6'nın güncel API'leri](https://docs.unity.com/en-us/engine/6000.6/script-reference/unityengine/object/findobjectsbytype) kullanılır. DOTween eklenti import ayarları güncellenir; DLL GUID'leri korunur. Eski görsel/font import ayarları Unity ile yeniden serialize edilir.

Repo içindeki doğrulama raporu `docs/unity-upgrade-validation.txt`, arayüz görselleri `docs/images` altındadır. Yerel çalışma klasörünün logları ve yedekleri public repoya dahil edilmemiştir. Bu doğrulama, görsel assetleri bulunan tam yerel projede yapılmıştır; assetleri çıkarılmış public checkout doğrudan oynanabilir bir build değildir.

Unity 6000.6.4f1 ile derleme, kısa Play Mode kontrolü, oynama düğmesi, top atışı, tema değişimi ve kaybetme/iptal akışı geçti. Yeni görünümle sahneyi kaydetme ve yeniden açma sonrası Legacy renk, yerleşim, yazı, sprite ve tıklama ayarlarının geri geldiği doğrulandı. Tam platform build'i ve uzun oyun oturumu bu kontrolün kapsamına dahil değildir.
