using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using YG;

internal static class MinerLocalization
{
    private static readonly Dictionary<string, string[]> Translations = new(StringComparer.Ordinal)
    {
        ["Restore {0}% durability · Watch ad"] = new[] { "{0}% прочности за видео", "Reklam izle · %{0} dayanıklılık yenile", "Recupera un {0}% de durabilidad · Ver anuncio", "Recupere {0}% da durabilidade · Ver anúncio" },
        ["Loading video…"] = new[] { "Загрузка видео…", "Video yükleniyor…", "Cargando vídeo…", "Carregando vídeo…" },
        ["Video unavailable · Try again"] = new[] { "Видео недоступно · Повторить", "Video kullanılamıyor · Tekrar dene", "Vídeo no disponible · Reintentar", "Vídeo indisponível · Tente novamente" },
        ["Play"] = new[] { "Играть", "Oyna", "Jugar", "Jogar" },
        ["Exit"] = new[] { "Выход", "Çıkış", "Salir", "Sair" },
        ["Shop"] = new[] { "Магазин", "Mağaza", "Tienda", "Loja" },
        ["Developers"] = new[] { "Разработчики", "Geliştiriciler", "Desarrolladores", "Desenvolvedores" },
        ["Settings"] = new[] { "Настройки", "Ayarlar", "Ajustes", "Configurações" },
        ["Display"] = new[] { "Экран", "Ekran", "Pantalla", "Tela" },
        ["Audio"] = new[] { "Звук", "Ses", "Sonido", "Áudio" },
        ["Master volume"] = new[] { "Общая громкость", "Ana ses", "Volumen general", "Volume geral" },
        ["Music"] = new[] { "Музыка", "Müzik", "Música", "Música" },
        ["SFX"] = new[] { "Эффекты", "Ses efektleri", "Efectos", "Efeitos" },
        ["Fullscreen"] = new[] { "Полный экран", "Tam ekran", "Pantalla completa", "Tela cheia" },
        ["Graphics quality"] = new[] { "Качество графики", "Grafik kalitesi", "Calidad gráfica", "Qualidade gráfica" },
        ["Low"] = new[] { "Низкое", "Düşük", "Baja", "Baixa" },
        ["Medium"] = new[] { "Среднее", "Orta", "Media", "Média" },
        ["High"] = new[] { "Высокое", "Yüksek", "Alta", "Alta" },
        ["MINER BASE"] = new[] { "БАЗА ШАХТЁРА", "MADENCİ ÜSSÜ", "BASE MINERA", "BASE DO MINEIRO" },
        ["Enchanting Table"] = new[] { "Стол зачарования", "Büyü Masası", "Mesa de encantamientos", "Mesa de encantamentos" },
        ["Enchantments"] = new[] { "Чары", "Büyüler", "Encantamientos", "Encantamentos" },
        ["Forge"] = new[] { "Кузница", "Demirhane", "Forja", "Forja" },
        ["Inventory"] = new[] { "Инвентарь", "Envanter", "Inventario", "Inventário" },
        ["Enter Mine"] = new[] { "В шахту", "Madene gir", "Entrar a la mina", "Entrar na mina" },
        ["Pickaxe stats"] = new[] { "Параметры кирки", "Kazma özellikleri", "Datos del pico", "Atributos da picareta" },
        ["PICKAXE BROKEN"] = new[] { "КИРКА СЛОМАЛАСЬ", "KAZMA KIRILDI", "PICO ROTO", "PICARETA QUEBRADA" },
        ["Coins earned"] = new[] { "Монет заработано", "Kazanılan para", "Monedas ganadas", "Moedas ganhas" },
        ["Blocks mined"] = new[] { "Блоков добыто", "Kazılan blok", "Bloques minados", "Blocos minerados" },
        ["Total coins"] = new[] { "Всего монет", "Toplam para", "Monedas totales", "Total de moedas" },
        ["Play again"] = new[] { "Ещё раз", "Tekrar oyna", "Volver a jugar", "Jogar de novo" },
        ["Tap to start"] = new[] { "Коснись, чтобы начать", "Başlamak için dokun", "Toca para empezar", "Toque para começar" },
        ["Go to base?\nYour earned coins will be saved."] = new[] { "Вернуться на базу?\nЗаработанные монеты сохранятся.", "Üsse dönülsün mü?\nKazandığın paralar kaydedilecek.", "¿Volver a la base?\nSe guardarán las monedas ganadas.", "Voltar à base?\nAs moedas ganhas serão salvas." },
        ["Go to base"] = new[] { "На базу", "Üsse dön", "Volver a la base", "Voltar à base" },
        ["DEPTH"] = new[] { "ГЛУБИНА", "DERİNLİK", "PROFUNDIDAD", "PROFUNDIDADE" },
        ["Pause"] = new[] { "Пауза", "Duraklat", "Pausa", "Pausar" },
        ["Paused"] = new[] { "Пауза", "Duraklatıldı", "En pausa", "Jogo pausado" },
        ["Continue"] = new[] { "Продолжить", "Devam et", "Continuar", "Continuar" },
        ["Main Menu"] = new[] { "Главное меню", "Ana Menü", "Menú principal", "Menu principal" },
        ["Leave run?"] = new[] { "Выйти из шахты?", "Madenden çıkılsın mı?", "¿Salir de la mina?", "Sair da mina?" },
        ["Cancel"] = new[] { "Отмена", "İptal", "Cancelar", "Cancelar" },
        ["Leave run"] = new[] { "Выйти", "Çık", "Salir", "Sair" },
        ["Go to the shop?\nRun progress will not be saved."] = new[] { "Перейти в магазин?\nПрогресс вылазки не сохранится.", "Mağazaya gidilsin mi?\nBu turun ilerlemesi kaydedilmeyecek.", "¿Ir a la tienda?\nNo se guardará el progreso de la partida.", "Ir para a loja?\nO progresso da partida não será salvo." },
        ["Go to the main menu?\nRun progress will not be saved."] = new[] { "Перейти в главное меню?\nПрогресс вылазки не сохранится.", "Ana menüye dönülsün mü?\nBu turun ilerlemesi kaydedilmeyecek.", "¿Volver al menú principal?\nNo se guardará el progreso de la partida.", "Voltar ao menu principal?\nO progresso da partida não será salvo." },
        ["No enchantment"] = new[] { "Без чар", "Büyü yok", "Sin encantamiento", "Sem encantamento" },
        ["Equip a pickaxe"] = new[] { "Возьмите кирку", "Bir kazma kuşan", "Equipa un pico", "Equipe uma picareta" },
        ["Roll"] = new[] { "Зачаровать", "Büyüle", "Encantar", "Encantar" },
        ["Rolling..."] = new[] { "Зачарование…", "Büyüleniyor…", "Encantando…", "Encantando…" },
        ["Roll  /  {0} coins"] = new[] { "Зачаровать  /  {0} мон.", "Büyüle  /  {0} altın", "Encantar  /  {0} monedas", "Encantar  /  {0} moedas" },
        ["Lightning"] = new[] { "Молния", "Yıldırım", "Rayo", "Relâmpago" },
        ["Dynamite"] = new[] { "Динамит", "Dinamit", "Dinamita", "Dinamite" },
        ["Luck"] = new[] { "Удача", "Şans", "Suerte", "Sorte" },
        ["Wooden"] = new[] { "Деревянная", "Tahta", "De madera", "De madeira" },
        ["Stone"] = new[] { "Каменная", "Taş", "De piedra", "De pedra" },
        ["Copper"] = new[] { "Медная", "Bakır", "De cobre", "De cobre" },
        ["Iron"] = new[] { "Железная", "Demir", "De hierro", "De ferro" },
        ["Diamond"] = new[] { "Алмазная", "Elmas", "De diamante", "De diamante" },
        ["Netherite"] = new[] { "Незеритовая", "Netherit", "De netherita", "De netherita" },
        ["Titan Pickaxe"] = new[] { "Титановая кирка", "Titanyum Kazma", "Pico de titanio", "Picareta de titânio" },
        ["Buy\n{0}"] = new[] { "Купить\n{0}", "Satın al\n{0}", "Comprar\n{0}", "Comprar\n{0}" },
        ["Opening payment…"] = new[] { "Открываем оплату…", "Ödeme açılıyor…", "Abriendo el pago…", "Abrindo pagamento…" },
        ["Retry payment\n{0}"] = new[] { "Повторить оплату\n{0}", "Tekrar dene\n{0}", "Reintentar pago\n{0}", "Tentar de novo\n{0}" },
        ["Golden"] = new[] { "Золотая", "Altın", "De oro", "De ouro" },
        ["Equipped"] = new[] { "Выбрана", "Kuşanıldı", "Equipado", "Equipada" },
        ["Equip"] = new[] { "Выбрать", "Kuşan", "Equipar", "Equipar" },
        ["Buy\n{0}$"] = new[] { "Купить\n{0}$", "Satın al\n{0}$", "Comprar\n{0}$", "Comprar\n{0}$" },
        ["Damage"] = new[] { "Урон", "Hasar", "Daño", "Dano" },
        ["Durability"] = new[] { "Прочность", "Dayanıklılık", "Durabilidad", "Durabilidade" },
        ["Range of digging"] = new[] { "Дальность копания", "Kazma menzili", "Alcance de excavación", "Alcance de escavação" },
        ["Multiplier"] = new[] { "Множитель", "Çarpan", "Multiplicador", "Multiplicador" },
        ["Reward multiplier"] = new[] { "Множитель награды", "Ödül çarpanı", "Multiplicador de recompensa", "Multiplicador de recompensa" },
        ["Regeneration"] = new[] { "Восстановление", "Yenilenme", "Regeneración", "Regeneração" },
        ["Regen"] = new[] { "Восст.", "Yenilenme", "Regeneración", "Regeneração" },
        ["Critical Chance"] = new[] { "Шанс крита", "Kritik şansı", "Prob. de crítico", "Chance de crítico" },
        ["Critical Damage"] = new[] { "Крит. урон", "Kritik hasarı", "Daño crítico", "Dano crítico" },
        ["Crit chance"] = new[] { "Шанс крита", "Kritik şansı", "Prob. de crítico", "Chance de crítico" },
        ["Crit damage"] = new[] { "Крит. урон", "Kritik hasarı", "Daño crítico", "Dano crítico" },
        ["Lvl {0}"] = new[] { "Ур. {0}", "Sv. {0}", "Nv. {0}", "Nv. {0}" },
        ["MAX"] = new[] { "МАКС", "MAKS", "MÁX", "MÁX" },
        ["+{0:0.##} durability / sec per level"] = new[] { "+{0:0.##} прочн./с за ур.", "Sv. başına +{0:0.##} dayanıklılık/sn", "+{0:0.##} durab./s por nivel", "+{0:0.##} durab./s por nível" },
        ["+{0:0.##}x/lvl"] = new[] { "+{0:0.##}x/ур.", "+{0:0.##}x/sv.", "+{0:0.##}x/nv.", "+{0:0.##}x/nv." },
        ["+{0:P0}/lvl"] = new[] { "+{0:P0}/ур.", "+{0:P0}/sv.", "+{0:P0}/nv.", "+{0:P0}/nv." },
        ["{0} / s"] = new[] { "{0} / с", "{0} / sn", "{0} / s", "{0} / s" },
        ["Run shop"] = new[] { "Лавка в шахте", "Maden dükkânı", "Tienda de la mina", "Loja da mina" },
        ["{0} RP"] = new[] { "{0} ОВ", "{0} TP", "{0} PE", "{0} PE" },
        ["+1 Damage"] = new[] { "+1 к урону", "+1 hasar", "+1 de daño", "+1 de dano" },
        ["+8 Max durability"] = new[] { "+8 к макс. прочности", "+8 azami dayanıklılık", "+8 de durabilidad máx.", "+8 de durabilidade máx." },
        ["Repair +12"] = new[] { "Починка +12", "Onarım +12", "Reparación +12", "Reparo +12" },
        ["Full durability"] = new[] { "Полная прочность", "Dayanıklılık tam", "Durabilidad completa", "Durabilidade cheia" },
        ["Regen +{0}/s"] = new[] { "Восст. +{0}/с", "Yenilenme +{0}/sn", "Regeneración +{0}/s", "Regeneração +{0}/s" },
        ["Dash ready"] = new[] { "Рывок готов", "Atılma hazır", "Impulso listo", "Arrancada pronta" },
        ["Dash {0}/{1}"] = new[] { "Рывок {0}/{1}", "Atılma {0}/{1}", "Impulso {0}/{1}", "Arrancada {0}/{1}" },
        ["BOSS"] = new[] { "БОСС", "PATRON", "JEFE", "CHEFE" },
        ["{0} / {1} HP"] = new[] { "{0} / {1} ОЗ", "{0} / {1} CP", "{0} / {1} PS", "{0} / {1} PV" },
        ["CRIT {0}"] = new[] { "КРИТ {0}", "KRİTİK {0}", "CRÍTICO {0}", "CRÍTICO {0}" },
    };

    // Authored scene labels and the buttons created by RunPauseMenu.Awake.
    private static readonly HashSet<string> SceneLabels = new(StringComparer.Ordinal)
    {
        "Play",
        "Exit",
        "Shop",
        "Developers",
        "Settings",
        "Display",
        "Audio",
        "Master volume",
        "Music",
        "SFX",
        "Fullscreen",
        "Graphics quality",
        "Low",
        "Medium",
        "High",
        "MINER BASE",
        "Enchanting Table",
        "Enchantments",
        "Forge",
        "Inventory",
        "Enter Mine",
        "Pickaxe stats",
        "PICKAXE BROKEN",
        "Coins earned",
        "Blocks mined",
        "Total coins",
        "Play again",
        "Tap to start",
        "Go to base",
        "DEPTH",
        "Pause",
        "Paused",
        "Continue",
        "Main Menu",
        "Leave run?",
        "Cancel",
        "Leave run",
    };

    private static readonly List<(TMP_Text Label, string Source)> Labels = new();
    private static readonly List<(TMP_Dropdown Dropdown, string[] Sources)> Dropdowns = new();
    private static int _language = -1;

    internal static event Action Changed;

    internal static string Text(string source) =>
        _language >= 0 && Translations.TryGetValue(source, out var translations) ? translations[_language] : source;

    internal static string Format(string source, params object[] values) => string.Format(Text(source), values);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        _language = -1;
        Changed = null;
        Labels.Clear();
        Dropdowns.Clear();
        SceneManager.sceneLoaded -= BindScene;
        SceneManager.sceneLoaded += BindScene;
    }

    private static void BindScene(Scene scene, LoadSceneMode mode)
    {
        // PluginYG resets its callbacks as part of its BeforeSceneLoad initialization.
        YG2.onSwitchLang -= ApplyLanguage;
        YG2.onSwitchLang += ApplyLanguage;
        Labels.RemoveAll(binding => binding.Label == null);
        Dropdowns.RemoveAll(binding => binding.Dropdown == null);

        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (var label in root.GetComponentsInChildren<TMP_Text>(true))
            {
                string source = label.text?.Trim();
                if (source == null || !SceneLabels.Contains(source))
                    continue;

                // The dropdown manages its caption and item text from its option data.
                var dropdown = label.GetComponentInParent<TMP_Dropdown>(true);
                if (dropdown != null && (dropdown.captionText == label || dropdown.itemText == label))
                    continue;

                Labels.Add((label, source));
            }

            foreach (var dropdown in root.GetComponentsInChildren<TMP_Dropdown>(true))
            {
                var sources = dropdown.options.ConvertAll(option => option.text).ToArray();
                Dropdowns.Add((dropdown, sources));
            }
        }

        if (YG2.isSDKEnabled)
            ApplyLanguage(YG2.lang);
        else
            RefreshSceneLabels();
    }

    private static void ApplyLanguage(string code)
    {
        string language = code?.Split('-', '_')[0].ToLowerInvariant();
        int index = language switch
        {
            "ru" or "be" or "kk" or "uk" or "uz" => 0,
            "tr" => 1,
            "es" => 2,
            "pt" => 3,
            _ => -1
        };

        bool changed = _language != index;
        _language = index;
        RefreshSceneLabels();
        if (changed)
            Changed?.Invoke();
    }

    private static void RefreshSceneLabels()
    {
        foreach (var (label, source) in Labels)
        {
            if (label == null)
                continue;

            string translated = Text(source);
            if (translated != source && !label.enableAutoSizing)
            {
                label.fontSizeMin = label.fontSize * 0.65f;
                label.fontSizeMax = label.fontSize;
                label.enableAutoSizing = true;
            }
            label.text = translated;
        }

        foreach (var (dropdown, sources) in Dropdowns)
        {
            if (dropdown == null)
                continue;

            for (int index = 0; index < sources.Length; index++)
                dropdown.options[index].text = Text(sources[index]);

            dropdown.RefreshShownValue();
        }
    }
}
