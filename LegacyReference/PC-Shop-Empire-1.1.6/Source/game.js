"use strict";

/* =========================================================
   PC SHOP EMPIRE
   GAME.JS — BÖLÜM 1
   TEMEL VERİLER, DİLLER, KAYIT VE ÜRÜN ÜRETİMİ
========================================================= */

const APP_VERSION = "1.1.6";
const SAVE_SCHEMA_VERSION = 3;
const SAVE_KEY = "pc_shop_empire_save";
const SETTINGS_KEY = "pc_shop_empire_settings";
const SAVE_SLOT_COUNT = 3;

function isCompatibleSaveState(state) {
    return Boolean(
        state
        && (
            state.saveSchema === SAVE_SCHEMA_VERSION
            || state.version === SAVE_SCHEMA_VERSION
            || state.version === APP_VERSION
        )
    );
}

const DAY_START_MINUTES = 9 * 60;
const DAY_END_MINUTES = 19 * 60;

/*
Bir iş günü normal hızda yaklaşık 6 dakika sürer.

09:00 ile 19:00 arasında toplam 600 oyun dakikası vardır.
360 gerçek saniyede 600 oyun dakikası ilerler.
*/
const GAME_MINUTES_PER_REAL_SECOND = 600 / 360;

const runtime = {
    timer: null,
    lastTick: 0,
    selectedOfferId: null,
    selectedCustomerId: null,
    selectedPropertyId: null,
    selectedBuiltPcId: null,
    currentPage: "dashboard",
    modalResolve: null,
    staffTimers: {},
    renderLimiter: 0,
    activeSaveSlot: 1
};


/* =========================================================
   ÇEVİRİLER
========================================================= */

const translations = {
    tr: {
        continueGame: "Devam Et",
        newGame: "Yeni Oyun",
        saveSlots: "Kayıt Yuvaları",
        settings: "Ayarlar",
        language: "Dil",
        autoSaveEnabled: "Otomatik kayıt etkin",
        saved: "Kaydedildi",
        saving: "Kaydediliyor",
        management: "Yönetim",
        dashboard: "Genel Bakış",
        market: "Parça Pazarı",
        inventory: "Envanter",
        workshop: "Montaj Atölyesi",
        customers: "Müşteriler",
        staff: "Personel",
        properties: "Dükkân ve Kira",
        finance: "Finans",
        upgrades: "Geliştirmeler",
        activity: "Faaliyet Kaydı",
        workingDay: "İş günü",
        endDay: "Günü Bitir",
        staffAutomation: "Personel Otomasyonu",
        active: "Aktif",
        inactive: "Kapalı",
        noStaff: "Henüz personelin yok.",
        dayCompleted: "İş günü tamamlandı",
        startNextDay: "Yeni Güne Başla",
        yes: "Evet",
        no: "Hayır",
        cancel: "İptal",
        confirm: "Onayla",
        money: "Kasa",
        reputation: "İtibar",
        level: "Seviye",
        day: "Gün",
        year: "Yıl",
        month: "Ay",
        hour: "Saat",
        storeStage: "Mağaza Aşaması",
        readyPcs: "Hazır Bilgisayar",
        activeCustomers: "Aktif Müşteri",
        inventoryValue: "Envanter Değeri",
        todayRevenue: "Bugünkü Gelir",
        todayExpenses: "Bugünkü Gider",
        shopView: "Mağaza Görünümü",
        recentActivity: "Son Faaliyetler",
        salesArea: "Satış Alanı",
        assemblyArea: "Montaj Alanı",
        storageArea: "Depo",
        offerCount: "Pazar Teklifleri",
        filter: "Filtre",
        all: "Tümü",
        search: "Ara",
        category: "Kategori",
        condition: "Durum",
        newCondition: "Yeni",
        outletCondition: "Outlet",
        refurbishedCondition: "Yenilenmiş",
        price: "Fiyat",
        stock: "Stok",
        seller: "Satıcı",
        score: "Puan",
        specifications: "Özellikler",
        buyOne: "1 Adet Satın Al",
        buyFive: "5 Adet Satın Al",
        insufficientMoney: "Yetersiz bakiye.",
        insufficientStorage: "Depoda yeterli alan yok.",
        productPurchased: "Ürün satın alındı.",
        inventoryEmpty: "Envanterde ürün bulunmuyor.",
        computerParts: "Bilgisayar Parçaları",
        builtComputers: "Hazır Bilgisayarlar",
        quantity: "Adet",
        averageCost: "Ortalama Maliyet",
        totalValue: "Toplam Değer",
        quickSell: "Hızlı Sat",
        selectPc: "Bir bilgisayar seç.",
        selectParts: "Parçaları Seç",
        compatibility: "Uyumluluk",
        compatible: "Uyumlu",
        incompatible: "Uyumsuz",
        assemble: "Bilgisayarı Monte Et",
        missingPart: "Her kategoriden bir parça seçilmelidir.",
        pcBuilt: "Bilgisayar başarıyla monte edildi.",
        customer: "Müşteri",
        customerType: "Müşteri Türü",
        requestedScore: "İstenen Puan",
        payment: "Ödeme",
        deadline: "Kalan Süre",
        requirements: "Gereksinimler",
        deliverPc: "Bilgisayarı Teslim Et",
        rejected: "Müşteri bilgisayarı reddetti.",
        delivered: "Sipariş teslim edildi.",
        employee: "Personel",
        role: "Görev",
        quality: "Kalite",
        experience: "Deneyim",
        energy: "Enerji",
        salary: "Maaş",
        dailyEffect: "Günlük Etki",
        hire: "İşe Al",
        fire: "İşten Çıkar",
        train: "Eğitim Ver",
        automationTask: "Otomatik Görev",
        property: "Dükkân",
        rent: "Kira",
        size: "Boyut",
        capacity: "Kapasite",
        customerBonus: "Müşteri Bonusu",
        workshopBonus: "Atölye Bonusu",
        moveToProperty: "Bu Dükkâna Taşın",
        currentProperty: "Mevcut Dükkân",
        contract: "Sözleşme",
        monthly: "Aylık",
        sixMonths: "6 Aylık",
        yearly: "Yıllık",
        electricity: "Elektrik",
        internet: "İnternet",
        insurance: "Sigorta",
        maintenance: "Bakım",
        tax: "Vergi",
        provider: "Sağlayıcı",
        dailyCost: "Günlük Maliyet",
        choose: "Seç",
        current: "Mevcut",
        endDayQuestion: "Günü bitirmek istediğine emin misin?",
        newGameQuestion:
            "Yeni oyun başlatılırsa mevcut ilerleme silinecek. Devam edilsin mi?",
        returnMenuQuestion:
            "Oyun otomatik olarak kaydedilecek ve ana menüye dönülecek.",
        deleteSaveQuestion:
            "Kayıt dosyasını tamamen silmek istediğine emin misin?",
        noSave: "Kayıtlı oyun bulunamadı.",
        saveDeleted: "Kayıt silindi.",
        gameSaved: "Oyun kaydedildi.",
        autoTaskSales: "Uygun hazır bilgisayarları müşterilere satar.",
        autoTaskTechnician:
            "Envanterdeki uyumlu parçaları kullanarak bilgisayar toplar.",
        autoTaskBuyer:
            "Eksik parçaları pazardan uygun fiyatla satın alır.",
        autoTaskAccountant:
            "Giderleri azaltır, vergiyi düşürür ve günlük rapor hazırlar.",
        autoTaskManager:
            "Tüm çalışanların hızını ve başarı oranını artırır.",
        garageStore: "Garaj Dükkânı",
        neighborhoodStore: "Mahalle Bilgisayarcısı",
        cityStore: "Şehir Teknoloji Mağazası",
        megaStore: "Mega Teknoloji Merkezi",
        headquarters: "Teknoloji Genel Merkezi",
        storeOpened: "Mağaza açıldı.",
        newDayStarted: "Yeni iş günü başladı.",
        staffCompletedTask: "Personel otomatik bir görev tamamladı.",
        autoSaveDescription:
            "Oyun her önemli işlemden sonra ve uygulama kapanırken kaydedilir."
    },

    en: {
        continueGame: "Continue",
        newGame: "New Game",
        saveSlots: "Save Slots",
        settings: "Settings",
        language: "Language",
        autoSaveEnabled: "Auto-save enabled",
        saved: "Saved",
        saving: "Saving",
        management: "Management",
        dashboard: "Dashboard",
        market: "Parts Market",
        inventory: "Inventory",
        workshop: "Assembly Workshop",
        customers: "Customers",
        staff: "Staff",
        properties: "Store and Rent",
        finance: "Finance",
        upgrades: "Upgrades",
        activity: "Activity Log",
        workingDay: "Working day",
        endDay: "End Day",
        staffAutomation: "Staff Automation",
        active: "Active",
        inactive: "Disabled",
        noStaff: "You do not have any employees yet.",
        dayCompleted: "Working day completed",
        startNextDay: "Start Next Day",
        yes: "Yes",
        no: "No",
        cancel: "Cancel",
        confirm: "Confirm",
        money: "Cash",
        reputation: "Reputation",
        level: "Level",
        day: "Day",
        year: "Year",
        month: "Month",
        hour: "Time",
        storeStage: "Store Stage",
        readyPcs: "Ready Computers",
        activeCustomers: "Active Customers",
        inventoryValue: "Inventory Value",
        todayRevenue: "Today's Revenue",
        todayExpenses: "Today's Expenses",
        shopView: "Store View",
        recentActivity: "Recent Activity",
        salesArea: "Sales Area",
        assemblyArea: "Assembly Area",
        storageArea: "Storage",
        offerCount: "Market Offers",
        filter: "Filter",
        all: "All",
        search: "Search",
        category: "Category",
        condition: "Condition",
        newCondition: "New",
        outletCondition: "Outlet",
        refurbishedCondition: "Refurbished",
        price: "Price",
        stock: "Stock",
        seller: "Seller",
        score: "Score",
        specifications: "Specifications",
        buyOne: "Buy 1",
        buyFive: "Buy 5",
        insufficientMoney: "Insufficient funds.",
        insufficientStorage: "Not enough storage capacity.",
        productPurchased: "Product purchased.",
        inventoryEmpty: "There are no products in inventory.",
        computerParts: "Computer Parts",
        builtComputers: "Built Computers",
        quantity: "Quantity",
        averageCost: "Average Cost",
        totalValue: "Total Value",
        quickSell: "Quick Sell",
        selectPc: "Select a computer.",
        selectParts: "Select Components",
        compatibility: "Compatibility",
        compatible: "Compatible",
        incompatible: "Incompatible",
        assemble: "Assemble Computer",
        missingPart: "Select one component from every category.",
        pcBuilt: "Computer assembled successfully.",
        customer: "Customer",
        customerType: "Customer Type",
        requestedScore: "Requested Score",
        payment: "Payment",
        deadline: "Deadline",
        requirements: "Requirements",
        deliverPc: "Deliver Computer",
        rejected: "The customer rejected the computer.",
        delivered: "Order delivered.",
        employee: "Employee",
        role: "Role",
        quality: "Quality",
        experience: "Experience",
        energy: "Energy",
        salary: "Salary",
        dailyEffect: "Daily Effect",
        hire: "Hire",
        fire: "Dismiss",
        train: "Train",
        automationTask: "Automation Task",
        property: "Property",
        rent: "Rent",
        size: "Size",
        capacity: "Capacity",
        customerBonus: "Customer Bonus",
        workshopBonus: "Workshop Bonus",
        moveToProperty: "Move to This Store",
        currentProperty: "Current Store",
        contract: "Contract",
        monthly: "Monthly",
        sixMonths: "6 Months",
        yearly: "Yearly",
        electricity: "Electricity",
        internet: "Internet",
        insurance: "Insurance",
        maintenance: "Maintenance",
        tax: "Tax",
        provider: "Provider",
        dailyCost: "Daily Cost",
        choose: "Choose",
        current: "Current",
        endDayQuestion: "Are you sure you want to end the day?",
        newGameQuestion:
            "Starting a new game will erase the current progress. Continue?",
        returnMenuQuestion:
            "The game will be automatically saved before returning to the menu.",
        deleteSaveQuestion:
            "Are you sure you want to permanently delete the save?",
        noSave: "No saved game found.",
        saveDeleted: "Save deleted.",
        gameSaved: "Game saved.",
        autoTaskSales: "Sells suitable ready computers to customers.",
        autoTaskTechnician:
            "Builds computers automatically using compatible inventory parts.",
        autoTaskBuyer:
            "Purchases missing components from the market at favorable prices.",
        autoTaskAccountant:
            "Reduces expenses, lowers taxes and prepares financial reports.",
        autoTaskManager:
            "Improves the speed and success rate of every employee.",
        garageStore: "Garage Store",
        neighborhoodStore: "Neighborhood PC Store",
        cityStore: "City Technology Store",
        megaStore: "Mega Technology Center",
        headquarters: "Technology Headquarters",
        storeOpened: "The store has opened.",
        newDayStarted: "A new working day has started.",
        staffCompletedTask: "An employee completed an automated task.",
        autoSaveDescription:
            "The game is saved after important actions and when the application closes."
    },

    de: {
        continueGame: "Fortsetzen",
        newGame: "Neues Spiel",
        saveSlots: "Spielstände",
        settings: "Einstellungen",
        language: "Sprache",
        autoSaveEnabled: "Automatisches Speichern aktiv",
        saved: "Gespeichert",
        saving: "Wird gespeichert",
        management: "Verwaltung",
        dashboard: "Übersicht",
        market: "Teilemarkt",
        inventory: "Inventar",
        workshop: "Montagewerkstatt",
        customers: "Kunden",
        staff: "Personal",
        properties: "Geschäft und Miete",
        finance: "Finanzen",
        upgrades: "Verbesserungen",
        activity: "Aktivitätsprotokoll",
        workingDay: "Arbeitstag",
        endDay: "Tag beenden",
        staffAutomation: "Personalautomatisierung",
        active: "Aktiv",
        inactive: "Deaktiviert",
        noStaff: "Du hast noch keine Mitarbeiter.",
        dayCompleted: "Arbeitstag abgeschlossen",
        startNextDay: "Neuen Tag starten",
        yes: "Ja",
        no: "Nein",
        cancel: "Abbrechen",
        confirm: "Bestätigen",
        money: "Kasse",
        reputation: "Ruf",
        level: "Stufe",
        day: "Tag",
        year: "Jahr",
        month: "Monat",
        hour: "Uhrzeit",
        storeStage: "Geschäftsstufe",
        readyPcs: "Fertige Computer",
        activeCustomers: "Aktive Kunden",
        inventoryValue: "Inventarwert",
        todayRevenue: "Heutiger Umsatz",
        todayExpenses: "Heutige Ausgaben",
        shopView: "Geschäftsansicht",
        recentActivity: "Letzte Aktivitäten",
        salesArea: "Verkaufsbereich",
        assemblyArea: "Montagebereich",
        storageArea: "Lager",
        offerCount: "Marktangebote",
        filter: "Filter",
        all: "Alle",
        search: "Suchen",
        category: "Kategorie",
        condition: "Zustand",
        newCondition: "Neu",
        outletCondition: "Outlet",
        refurbishedCondition: "Generalüberholt",
        price: "Preis",
        stock: "Bestand",
        seller: "Verkäufer",
        score: "Punkte",
        specifications: "Eigenschaften",
        buyOne: "1 Stück kaufen",
        buyFive: "5 Stück kaufen",
        insufficientMoney: "Nicht genügend Geld.",
        insufficientStorage: "Nicht genügend Lagerplatz.",
        productPurchased: "Produkt gekauft.",
        inventoryEmpty: "Im Inventar befinden sich keine Produkte.",
        computerParts: "Computerteile",
        builtComputers: "Fertige Computer",
        quantity: "Menge",
        averageCost: "Durchschnittskosten",
        totalValue: "Gesamtwert",
        quickSell: "Schnellverkauf",
        selectPc: "Wähle einen Computer.",
        selectParts: "Komponenten auswählen",
        compatibility: "Kompatibilität",
        compatible: "Kompatibel",
        incompatible: "Nicht kompatibel",
        assemble: "Computer montieren",
        missingPart: "Wähle aus jeder Kategorie eine Komponente.",
        pcBuilt: "Computer erfolgreich montiert.",
        customer: "Kunde",
        customerType: "Kundentyp",
        requestedScore: "Gewünschte Leistung",
        payment: "Zahlung",
        deadline: "Restzeit",
        requirements: "Anforderungen",
        deliverPc: "Computer liefern",
        rejected: "Der Kunde hat den Computer abgelehnt.",
        delivered: "Auftrag ausgeliefert.",
        employee: "Mitarbeiter",
        role: "Aufgabe",
        quality: "Qualität",
        experience: "Erfahrung",
        energy: "Energie",
        salary: "Gehalt",
        dailyEffect: "Täglicher Effekt",
        hire: "Einstellen",
        fire: "Entlassen",
        train: "Schulen",
        automationTask: "Automatische Aufgabe",
        property: "Geschäft",
        rent: "Miete",
        size: "Größe",
        capacity: "Kapazität",
        customerBonus: "Kundenbonus",
        workshopBonus: "Werkstattbonus",
        moveToProperty: "In dieses Geschäft umziehen",
        currentProperty: "Aktuelles Geschäft",
        contract: "Vertrag",
        monthly: "Monatlich",
        sixMonths: "6 Monate",
        yearly: "Jährlich",
        electricity: "Strom",
        internet: "Internet",
        insurance: "Versicherung",
        maintenance: "Wartung",
        tax: "Steuer",
        provider: "Anbieter",
        dailyCost: "Tägliche Kosten",
        choose: "Auswählen",
        current: "Aktuell",
        endDayQuestion: "Möchtest du den Tag wirklich beenden?",
        newGameQuestion:
            "Ein neues Spiel löscht den aktuellen Fortschritt. Fortfahren?",
        returnMenuQuestion:
            "Das Spiel wird vor der Rückkehr zum Hauptmenü automatisch gespeichert.",
        deleteSaveQuestion:
            "Möchtest du den Spielstand wirklich dauerhaft löschen?",
        noSave: "Kein Spielstand vorhanden.",
        saveDeleted: "Spielstand gelöscht.",
        gameSaved: "Spiel gespeichert.",
        autoTaskSales:
            "Verkauft passende fertige Computer automatisch an Kunden.",
        autoTaskTechnician:
            "Baut automatisch Computer aus kompatiblen Lagerteilen.",
        autoTaskBuyer:
            "Kauft fehlende Komponenten zu günstigen Preisen auf dem Markt.",
        autoTaskAccountant:
            "Reduziert Ausgaben, Steuern und erstellt Finanzberichte.",
        autoTaskManager:
            "Verbessert Geschwindigkeit und Erfolgsquote aller Mitarbeiter.",
        garageStore: "Garagengeschäft",
        neighborhoodStore: "PC-Laden im Viertel",
        cityStore: "Technikgeschäft der Stadt",
        megaStore: "Mega-Technologiezentrum",
        headquarters: "Technologie-Hauptquartier",
        storeOpened: "Das Geschäft wurde eröffnet.",
        newDayStarted: "Ein neuer Arbeitstag hat begonnen.",
        staffCompletedTask:
            "Ein Mitarbeiter hat eine automatische Aufgabe abgeschlossen.",
        autoSaveDescription:
            "Das Spiel wird nach wichtigen Aktionen und beim Schließen gespeichert."
    }
};


function getLanguage() {
    return gameState?.language || "en";
}


function t(key, variables = {}) {
    const language = getLanguage();

    let text =
        translations[language]?.[key]
        ??
        translations.en[key]
        ??
        key;

    for (const [variable, value] of Object.entries(variables)) {
        text = text.replaceAll(
            `{${variable}}`,
            String(value)
        );
    }

    return text;
}


/* =========================================================
   EVRENSEL İSİMLER
========================================================= */

const UNIVERSAL_FIRST_NAMES = [
    "Alex",
    "Emma",
    "Liam",
    "Sofia",
    "Noah",
    "Mia",
    "Lucas",
    "Ava",
    "Daniel",
    "Elena",
    "Leo",
    "Nora",
    "Oliver",
    "Lina",
    "Ethan",
    "Maya",
    "Theo",
    "Clara",
    "Ryan",
    "Julia",
    "Adrian",
    "Zoe",
    "Samuel",
    "Emily",
    "David",
    "Laura",
    "Nathan",
    "Amelia",
    "Victor",
    "Isabella",
    "Max",
    "Anna",
    "Oscar",
    "Eva",
    "Marco",
    "Sara",
    "Julian",
    "Alice",
    "Nicolas",
    "Hannah",
    "Adam",
    "Lea",
    "Gabriel",
    "Ella",
    "Thomas",
    "Chloe",
    "Eric",
    "Luna",
    "Kevin",
    "Jasmine"
];

const UNIVERSAL_LAST_NAMES = [
    "Miller",
    "Smith",
    "Martin",
    "Anderson",
    "Brown",
    "Wilson",
    "Taylor",
    "Clark",
    "Walker",
    "Lewis",
    "Hall",
    "Young",
    "King",
    "Scott",
    "Green",
    "Baker",
    "Carter",
    "Hill",
    "Adams",
    "Nelson",
    "Lopez",
    "Garcia",
    "Rossi",
    "Costa",
    "Moretti",
    "Dubois",
    "Bernard",
    "Fischer",
    "Weber",
    "Schmidt",
    "Meyer",
    "Wagner",
    "Keller",
    "Kovac",
    "Novak",
    "Petrov",
    "Ivanov",
    "Santos",
    "Silva",
    "Kim",
    "Lee",
    "Chen",
    "Tanaka",
    "Yamamoto",
    "Singh",
    "Patel"
];


/* =========================================================
   YARDIMCI FONKSİYONLAR
========================================================= */

function randomItem(array) {
    return array[
        Math.floor(
            Math.random() * array.length
        )
    ];
}


function randomInt(minimum, maximum) {
    return Math.floor(
        Math.random()
        *
        (
            maximum - minimum + 1
        )
    )
    +
    minimum;
}


function randomFloat(minimum, maximum) {
    return (
        Math.random()
        *
        (
            maximum - minimum
        )
    )
    +
    minimum;
}


function clamp(value, minimum, maximum) {
    return Math.max(
        minimum,
        Math.min(
            maximum,
            value
        )
    );
}


function chance(probability) {
    return Math.random() < probability;
}


function deepCopy(value) {
    return JSON.parse(
        JSON.stringify(value)
    );
}


function createId(prefix) {
    gameState.nextIds[prefix] =
        (
            gameState.nextIds[prefix]
            ||
            1
        );

    const id =
        `${prefix}_${gameState.nextIds[prefix]}`;

    gameState.nextIds[prefix] += 1;

    return id;
}


function formatMoney(value) {
    const languageMap = {
        tr: "tr-TR",
        en: "en-US",
        de: "de-DE"
    };

    const locale =
        languageMap[getLanguage()]
        ||
        "en-US";

    return new Intl.NumberFormat(
        locale,
        {
            style: "currency",
            currency: "EUR",
            maximumFractionDigits: 0
        }
    ).format(value);
}


function formatNumber(value) {
    const languageMap = {
        tr: "tr-TR",
        en: "en-US",
        de: "de-DE"
    };

    return new Intl.NumberFormat(
        languageMap[getLanguage()] || "en-US"
    ).format(
        Math.round(value)
    );
}


function minutesToTime(minutes) {
    const normalized =
        Math.max(
            0,
            Math.floor(minutes)
        );

    const hours =
        Math.floor(
            normalized / 60
        );

    const remainingMinutes =
        normalized % 60;

    return (
        String(hours).padStart(2, "0")
        +
        ":"
        +
        String(remainingMinutes).padStart(2, "0")
    );
}


function getDateObject() {
    return new Date(
        gameState.calendar.year,
        gameState.calendar.month,
        gameState.calendar.day
    );
}


function formatGameDate() {
    const date = getDateObject();

    const locales = {
        tr: "tr-TR",
        en: "en-GB",
        de: "de-DE"
    };

    return date.toLocaleDateString(
        locales[getLanguage()] || "en-GB",
        {
            day: "numeric",
            month: "long",
            year: "numeric"
        }
    );
}


function addCalendarDay() {
    const date = getDateObject();

    date.setDate(
        date.getDate() + 1
    );

    gameState.calendar.year =
        date.getFullYear();

    gameState.calendar.month =
        date.getMonth();

    gameState.calendar.day =
        date.getDate();
}


function getDayProgress() {
    const passed =
        gameState.calendar.minutes
        -
        DAY_START_MINUTES;

    const total =
        DAY_END_MINUTES
        -
        DAY_START_MINUTES;

    return clamp(
        passed / total,
        0,
        1
    );
}


function getPartById(partId) {
    return PART_DATABASE.find(
        part => part.id === partId
    );
}


function getOfferById(offerId) {
    return gameState.marketOffers.find(
        offer => offer.id === offerId
    );
}


function getBuiltPcById(pcId) {
    return gameState.builtComputers.find(
        computer => computer.id === pcId
    );
}


function getCustomerById(customerId) {
    return gameState.customers.find(
        customer => customer.id === customerId
    );
}


function getPropertyById(propertyId) {
    return PROPERTY_OPTIONS.find(
        property => property.id === propertyId
    );
}


function getProviderById(
    providerType,
    providerId
) {
    return PROVIDERS[providerType].find(
        provider => provider.id === providerId
    );
}


/* =========================================================
   MAĞAZA SEÇENEKLERİ
========================================================= */

const PROPERTY_OPTIONS = [
    {
        id: "garage",
        nameKey: "garageStore",
        size: 45,
        rent: 290,
        storageCapacity: 65,
        customerBonus: 0,
        workshopBonus: 0,
        staffCapacity: 2,
        requiredReputation: 0,
        deposit: 600
    },
    {
        id: "neighborhood",
        nameKey: "neighborhoodStore",
        size: 90,
        rent: 620,
        storageCapacity: 130,
        customerBonus: 1,
        workshopBonus: 0.05,
        staffCapacity: 5,
        requiredReputation: 20,
        deposit: 1500
    },
    {
        id: "city",
        nameKey: "cityStore",
        size: 180,
        rent: 1450,
        storageCapacity: 260,
        customerBonus: 2,
        workshopBonus: 0.10,
        staffCapacity: 10,
        requiredReputation: 55,
        deposit: 4200
    },
    {
        id: "mega",
        nameKey: "megaStore",
        size: 420,
        rent: 3700,
        storageCapacity: 650,
        customerBonus: 4,
        workshopBonus: 0.18,
        staffCapacity: 22,
        requiredReputation: 110,
        deposit: 11000
    },
    {
        id: "headquarters",
        nameKey: "headquarters",
        size: 950,
        rent: 8900,
        storageCapacity: 1600,
        customerBonus: 8,
        workshopBonus: 0.28,
        staffCapacity: 45,
        requiredReputation: 220,
        deposit: 28000
    }
];


/* =========================================================
   HİZMET SAĞLAYICILARI
========================================================= */

const PROVIDERS = {
    electricity: [
        {
            id: "eco_power",
            name: "EcoPower",
            dailyCost: 65,
            buildCost: 7,
            outageRisk: 0.06,
            reputationBonus: 0
        },
        {
            id: "city_energy",
            name: "City Energy",
            dailyCost: 95,
            buildCost: 5,
            outageRisk: 0.025,
            reputationBonus: 0
        },
        {
            id: "volt_premium",
            name: "Volt Premium",
            dailyCost: 145,
            buildCost: 3,
            outageRisk: 0.005,
            reputationBonus: 1
        }
    ],

    internet: [
        {
            id: "basic_net",
            name: "BasicNet",
            dailyCost: 28,
            marketBonus: 0,
            automationBonus: 0
        },
        {
            id: "fiber_link",
            name: "FiberLink",
            dailyCost: 55,
            marketBonus: 8,
            automationBonus: 0.08
        },
        {
            id: "business_cloud",
            name: "BusinessCloud",
            dailyCost: 105,
            marketBonus: 18,
            automationBonus: 0.16
        }
    ],

    insurance: [
        {
            id: "starter_cover",
            name: "Starter Cover",
            dailyCost: 35,
            theftProtection: 0.10,
            warrantyProtection: 0.08
        },
        {
            id: "secure_shop",
            name: "SecureShop",
            dailyCost: 75,
            theftProtection: 0.38,
            warrantyProtection: 0.30
        },
        {
            id: "premium_guard",
            name: "Premium Guard",
            dailyCost: 145,
            theftProtection: 0.72,
            warrantyProtection: 0.60
        }
    ]
};


/* =========================================================
   PERSONEL ROLLERİ
========================================================= */

const STAFF_ROLES = {
    sales: {
        icon: "S",
        salaryBase: 120,
        hiringCost: 750,
        trainingCost: 480,
        taskKey: "autoTaskSales",
        baseIntervalMinutes: 125,
        description: {
            tr:
                "Müşterilere otomatik satış yapar. Kalitesi yükseldikçe daha doğru bilgisayar seçer, daha yüksek ödeme ve itibar kazanır.",
            en:
                "Automatically sells computers to customers. Higher quality improves matching, payment and reputation.",
            de:
                "Verkauft automatisch Computer. Höhere Qualität verbessert Auswahl, Zahlung und Ruf."
        }
    },

    technician: {
        icon: "T",
        salaryBase: 165,
        hiringCost: 1050,
        trainingCost: 650,
        taskKey: "autoTaskTechnician",
        baseIntervalMinutes: 155,
        description: {
            tr:
                "Uyumlu parçaları seçerek otomatik bilgisayar toplar. Kalitesi arttıkça montaj puanı, değer ve çalışma hızı yükselir.",
            en:
                "Builds compatible computers automatically. Quality improves speed, score and sale value.",
            de:
                "Baut automatisch kompatible Computer. Qualität erhöht Geschwindigkeit, Punkte und Verkaufswert."
        }
    },

    buyer: {
        icon: "B",
        salaryBase: 145,
        hiringCost: 900,
        trainingCost: 560,
        taskKey: "autoTaskBuyer",
        baseIntervalMinutes: 110,
        description: {
            tr:
                "Pazarı otomatik tarar. Eksik parçalardan ucuz olanları satın alır ve stok dengesini korur.",
            en:
                "Scans the market automatically, purchases discounted missing components and balances inventory.",
            de:
                "Durchsucht automatisch den Markt, kauft günstige fehlende Teile und gleicht das Lager aus."
        }
    },

    accountant: {
        icon: "A",
        salaryBase: 150,
        hiringCost: 950,
        trainingCost: 600,
        taskKey: "autoTaskAccountant",
        baseIntervalMinutes: 195,
        description: {
            tr:
                "Günlük giderleri, vergiyi ve hizmet masraflarını düşürür. Kalitesi arttıkça tasarruf miktarı yükselir.",
            en:
                "Reduces operating expenses, taxes and service costs. Higher quality increases savings.",
            de:
                "Senkt Betriebskosten, Steuern und Dienstleistungskosten. Höhere Qualität erhöht die Ersparnis."
        }
    },

    manager: {
        icon: "M",
        salaryBase: 230,
        hiringCost: 1800,
        trainingCost: 950,
        taskKey: "autoTaskManager",
        baseIntervalMinutes: 245,
        description: {
            tr:
                "Diğer çalışanların otomasyon hızını, başarı ihtimalini ve günlük enerjisini güçlendirir.",
            en:
                "Improves automation speed, success rate and daily energy of all employees.",
            de:
                "Verbessert Automatisierung, Erfolgsquote und tägliche Energie aller Mitarbeiter."
        }
    }
};


/* =========================================================
   PARÇA VERİ TABANI ÜRETİMİ
========================================================= */

function createPartDatabase() {
    const parts = [];
    let counter = 1;

    function addPart(
        type,
        brand,
        model,
        basePrice,
        score,
        specifications = {}
    ) {
        parts.push({
            id:
                `part_${String(counter).padStart(4, "0")}`,
            type,
            brand,
            model,
            name:
                `${brand} ${model}`,
            basePrice:
                Math.round(basePrice),
            score:
                Math.round(score),
            ...specifications
        });

        counter += 1;
    }

    /* -----------------------------------------------------
       İŞLEMCİLER
    ----------------------------------------------------- */

    const cpuModels = [
        ["AMD", "Ryzen 3 4100", "AM4", "DDR4", 65, 4, 75, 45],
        ["AMD", "Ryzen 5 4500", "AM4", "DDR4", 65, 6, 90, 58],
        ["AMD", "Ryzen 5 5500", "AM4", "DDR4", 65, 6, 105, 68],
        ["AMD", "Ryzen 5 5600", "AM4", "DDR4", 65, 6, 135, 82],
        ["AMD", "Ryzen 5 5600X", "AM4", "DDR4", 65, 6, 155, 91],
        ["AMD", "Ryzen 7 5700X", "AM4", "DDR4", 65, 8, 190, 108],
        ["AMD", "Ryzen 7 5800X", "AM4", "DDR4", 105, 8, 225, 119],
        ["AMD", "Ryzen 7 5800X3D", "AM4", "DDR4", 105, 8, 315, 142],
        ["AMD", "Ryzen 9 5900X", "AM4", "DDR4", 105, 12, 340, 151],
        ["AMD", "Ryzen 9 5950X", "AM4", "DDR4", 105, 16, 430, 170],

        ["AMD", "Ryzen 5 7500F", "AM5", "DDR5", 65, 6, 175, 102],
        ["AMD", "Ryzen 5 7600", "AM5", "DDR5", 65, 6, 215, 116],
        ["AMD", "Ryzen 5 7600X", "AM5", "DDR5", 105, 6, 245, 128],
        ["AMD", "Ryzen 7 7700", "AM5", "DDR5", 65, 8, 315, 146],
        ["AMD", "Ryzen 7 7700X", "AM5", "DDR5", 105, 8, 350, 157],
        ["AMD", "Ryzen 7 7800X3D", "AM5", "DDR5", 120, 8, 440, 185],
        ["AMD", "Ryzen 9 7900", "AM5", "DDR5", 65, 12, 470, 190],
        ["AMD", "Ryzen 9 7900X", "AM5", "DDR5", 170, 12, 525, 204],
        ["AMD", "Ryzen 9 7950X", "AM5", "DDR5", 170, 16, 640, 228],
        ["AMD", "Ryzen 9 7950X3D", "AM5", "DDR5", 120, 16, 710, 246],
        ["AMD", "Ryzen 7 9700X", "AM5", "DDR5", 65, 8, 410, 183],
        ["AMD", "Ryzen 9 9900X", "AM5", "DDR5", 120, 12, 570, 216],
        ["AMD", "Ryzen 9 9950X", "AM5", "DDR5", 170, 16, 760, 258],

        ["Intel", "Core i3-10100F", "LGA1200", "DDR4", 65, 4, 68, 43],
        ["Intel", "Core i5-10400F", "LGA1200", "DDR4", 65, 6, 95, 61],
        ["Intel", "Core i5-11400F", "LGA1200", "DDR4", 65, 6, 120, 72],
        ["Intel", "Core i7-11700K", "LGA1200", "DDR4", 125, 8, 230, 117],
        ["Intel", "Core i9-11900K", "LGA1200", "DDR4", 125, 8, 285, 135],

        ["Intel", "Core i3-12100F", "LGA1700", "DDR4", 60, 4, 105, 65],
        ["Intel", "Core i5-12400F", "LGA1700", "DDR4", 65, 6, 145, 82],
        ["Intel", "Core i5-12600K", "LGA1700", "DDR4", 150, 10, 235, 120],
        ["Intel", "Core i7-12700K", "LGA1700", "DDR4", 190, 12, 330, 151],
        ["Intel", "Core i9-12900K", "LGA1700", "DDR5", 241, 16, 450, 181],
        ["Intel", "Core i5-13400F", "LGA1700", "DDR4", 148, 10, 210, 111],
        ["Intel", "Core i5-13600K", "LGA1700", "DDR5", 181, 14, 315, 152],
        ["Intel", "Core i7-13700K", "LGA1700", "DDR5", 253, 16, 430, 184],
        ["Intel", "Core i9-13900K", "LGA1700", "DDR5", 253, 24, 610, 221],
        ["Intel", "Core i5-14400F", "LGA1700", "DDR4", 148, 10, 225, 117],
        ["Intel", "Core i5-14600K", "LGA1700", "DDR5", 181, 14, 345, 160],
        ["Intel", "Core i7-14700K", "LGA1700", "DDR5", 253, 20, 485, 199],
        ["Intel", "Core i9-14900K", "LGA1700", "DDR5", 253, 24, 690, 239],

        ["Intel", "Core Ultra 5 225F", "LGA1851", "DDR5", 121, 10, 275, 136],
        ["Intel", "Core Ultra 5 245K", "LGA1851", "DDR5", 159, 14, 360, 166],
        ["Intel", "Core Ultra 7 265K", "LGA1851", "DDR5", 250, 20, 480, 201],
        ["Intel", "Core Ultra 9 285K", "LGA1851", "DDR5", 250, 24, 650, 235]
    ];

    for (const cpu of cpuModels) {
        const [
            brand,
            model,
            socket,
            ramType,
            wattage,
            cores,
            price,
            score
        ] = cpu;

        addPart(
            "CPU",
            brand,
            model,
            price,
            score,
            {
                socket,
                ramType,
                wattage,
                cores
            }
        );
    }

    /* -----------------------------------------------------
       ANAKARTLAR
    ----------------------------------------------------- */

    const motherboardChipsets = {
        LGA1200: [
            ["H410", 70, 22],
            ["B460", 95, 29],
            ["Z490", 165, 42],
            ["B560", 115, 34],
            ["Z590", 190, 48]
        ],

        LGA1700: [
            ["H610", 85, 24],
            ["B660", 125, 34],
            ["Z690", 230, 50],
            ["B760", 155, 39],
            ["Z790", 285, 59]
        ],

        LGA1851: [
            ["B860", 210, 47],
            ["Z890", 350, 65]
        ],

        AM4: [
            ["A320", 60, 18],
            ["A520", 75, 22],
            ["B450", 90, 27],
            ["B550", 125, 36],
            ["X570", 220, 51]
        ],

        AM5: [
            ["A620", 105, 25],
            ["B650", 165, 39],
            ["B650E", 230, 49],
            ["X670", 295, 58],
            ["X670E", 380, 69],
            ["X870", 420, 76]
        ]
    };

    const motherboardBrands = [
        "ASUS",
        "MSI",
        "Gigabyte",
        "ASRock"
    ];

    const formFactors = [
        {
            name: "mATX",
            priceMultiplier: 0.92,
            scoreBonus: 0
        },
        {
            name: "ATX",
            priceMultiplier: 1.08,
            scoreBonus: 5
        },
        {
            name: "E-ATX",
            priceMultiplier: 1.35,
            scoreBonus: 11
        }
    ];

    for (
        const [
            socket,
            chipsets
        ]
        of Object.entries(motherboardChipsets)
    ) {
        let ramTypes;

        if (
            socket === "AM5"
            ||
            socket === "LGA1851"
        ) {
            ramTypes = ["DDR5"];
        } else if (socket === "LGA1700") {
            ramTypes = ["DDR4", "DDR5"];
        } else {
            ramTypes = ["DDR4"];
        }

        for (const chipset of chipsets) {
            const [
                chipsetName,
                basePrice,
                baseScore
            ] = chipset;

            for (const brand of motherboardBrands) {
                for (const ramType of ramTypes) {
                    const factorOptions =
                        chipsetName.startsWith("Z")
                        ||
                        chipsetName.startsWith("X")
                            ? formFactors
                            : formFactors.slice(0, 2);

                    for (const formFactor of factorOptions) {
                        addPart(
                            "Motherboard",
                            brand,
                            `${chipsetName} ${formFactor.name} ${ramType}`,
                            basePrice
                                *
                                formFactor.priceMultiplier
                                *
                                randomFloat(0.93, 1.12),
                            baseScore
                                +
                                formFactor.scoreBonus
                                +
                                randomInt(0, 4),
                            {
                                socket,
                                ramType,
                                formFactor:
                                    formFactor.name,
                                storageInterfaces: [
                                    "SATA",
                                    "NVMe"
                                ]
                            }
                        );
                    }
                }
            }
        }
    }

    /* -----------------------------------------------------
       EKRAN KARTLARI
    ----------------------------------------------------- */

    const gpuModels = [
        ["NVIDIA", "GTX 1650", 155, 55, 75, 220, 4],
        ["NVIDIA", "GTX 1660 Super", 210, 72, 125, 235, 6],
        ["NVIDIA", "RTX 2060", 255, 82, 160, 240, 6],
        ["NVIDIA", "RTX 2070 Super", 330, 101, 215, 270, 8],
        ["NVIDIA", "RTX 2080 Ti", 470, 128, 250, 300, 11],
        ["NVIDIA", "RTX 3050", 240, 80, 130, 242, 8],
        ["NVIDIA", "RTX 3060", 310, 103, 170, 250, 12],
        ["NVIDIA", "RTX 3060 Ti", 380, 121, 200, 260, 8],
        ["NVIDIA", "RTX 3070", 440, 139, 220, 275, 8],
        ["NVIDIA", "RTX 3070 Ti", 500, 149, 290, 285, 8],
        ["NVIDIA", "RTX 3080", 650, 177, 320, 310, 10],
        ["NVIDIA", "RTX 3080 Ti", 780, 190, 350, 320, 12],
        ["NVIDIA", "RTX 3090", 950, 207, 350, 330, 24],
        ["NVIDIA", "RTX 4060", 340, 119, 115, 245, 8],
        ["NVIDIA", "RTX 4060 Ti", 445, 142, 160, 260, 8],
        ["NVIDIA", "RTX 4070", 610, 168, 200, 285, 12],
        ["NVIDIA", "RTX 4070 Super", 690, 185, 220, 295, 12],
        ["NVIDIA", "RTX 4070 Ti Super", 860, 205, 285, 305, 16],
        ["NVIDIA", "RTX 4080 Super", 1120, 235, 320, 340, 16],
        ["NVIDIA", "RTX 4090", 1800, 278, 450, 360, 24],
        ["NVIDIA", "RTX 5060", 430, 145, 145, 255, 8],
        ["NVIDIA", "RTX 5070", 720, 197, 250, 300, 12],
        ["NVIDIA", "RTX 5080", 1350, 258, 360, 340, 16],
        ["NVIDIA", "RTX 5090", 2450, 325, 575, 370, 32],

        ["AMD", "RX 5500 XT", 175, 63, 130, 230, 8],
        ["AMD", "RX 5600 XT", 225, 77, 150, 245, 6],
        ["AMD", "RX 5700 XT", 290, 96, 225, 270, 8],
        ["AMD", "RX 6600", 235, 86, 132, 245, 8],
        ["AMD", "RX 6600 XT", 285, 101, 160, 255, 8],
        ["AMD", "RX 6700 XT", 380, 126, 230, 270, 12],
        ["AMD", "RX 6800", 520, 151, 250, 290, 16],
        ["AMD", "RX 6800 XT", 610, 168, 300, 310, 16],
        ["AMD", "RX 6900 XT", 760, 187, 300, 325, 16],
        ["AMD", "RX 7600", 320, 111, 165, 270, 8],
        ["AMD", "RX 7700 XT", 470, 149, 245, 290, 12],
        ["AMD", "RX 7800 XT", 590, 171, 263, 305, 16],
        ["AMD", "RX 7900 GRE", 670, 186, 260, 310, 16],
        ["AMD", "RX 7900 XT", 850, 210, 315, 335, 20],
        ["AMD", "RX 7900 XTX", 1050, 232, 355, 350, 24],
        ["AMD", "RX 9060 XT", 455, 146, 180, 285, 16],
        ["AMD", "RX 9070", 720, 196, 245, 315, 16],
        ["AMD", "RX 9070 XT", 870, 217, 304, 335, 16],

        ["Intel", "Arc A580", 210, 78, 185, 285, 8],
        ["Intel", "Arc A750", 260, 92, 225, 300, 8],
        ["Intel", "Arc A770", 340, 109, 225, 315, 16],
        ["Intel", "Arc B570", 275, 102, 150, 270, 10],
        ["Intel", "Arc B580", 340, 121, 190, 285, 12]
    ];

    const gpuManufacturers = [
        "ASUS",
        "MSI",
        "Gigabyte",
        "Sapphire",
        "PowerColor",
        "Zotac",
        "Palit"
    ];

    const gpuEditions = [
        {
            name: "Dual",
            priceBonus: 0,
            scoreBonus: 0,
            lengthBonus: 0,
            wattBonus: 0
        },
        {
            name: "Gaming OC",
            priceBonus: 55,
            scoreBonus: 7,
            lengthBonus: 10,
            wattBonus: 8
        },
        {
            name: "Premium",
            priceBonus: 105,
            scoreBonus: 13,
            lengthBonus: 18,
            wattBonus: 15
        }
    ];

    for (const gpu of gpuModels) {
        const [
            chipBrand,
            model,
            price,
            score,
            wattage,
            length,
            vram
        ] = gpu;

        for (const manufacturer of gpuManufacturers) {
            for (const edition of gpuEditions) {
                addPart(
                    "GPU",
                    manufacturer,
                    `${chipBrand} ${model} ${edition.name}`,
                    price
                        +
                        edition.priceBonus
                        +
                        randomInt(-15, 35),
                    score
                        +
                        edition.scoreBonus,
                    {
                        chipBrand,
                        wattage:
                            wattage
                            +
                            edition.wattBonus,
                        length:
                            length
                            +
                            edition.lengthBonus,
                        vram
                    }
                );
            }
        }
    }

    /* -----------------------------------------------------
       RAM
    ----------------------------------------------------- */

    const ramBrands = [
        "Kingston",
        "Corsair",
        "G.Skill",
        "Crucial",
        "TeamGroup",
        "Patriot"
    ];

    const ramOptions = [
        ["DDR4", 8, 3200, 28, 12],
        ["DDR4", 16, 3200, 45, 21],
        ["DDR4", 16, 3600, 52, 25],
        ["DDR4", 32, 3200, 82, 36],
        ["DDR4", 32, 3600, 94, 41],
        ["DDR4", 64, 3200, 155, 57],
        ["DDR4", 64, 3600, 175, 63],

        ["DDR5", 16, 4800, 60, 27],
        ["DDR5", 16, 5600, 68, 31],
        ["DDR5", 32, 5600, 105, 46],
        ["DDR5", 32, 6000, 118, 52],
        ["DDR5", 32, 6400, 135, 57],
        ["DDR5", 64, 5600, 205, 75],
        ["DDR5", 64, 6000, 225, 82],
        ["DDR5", 96, 6000, 335, 103],
        ["DDR5", 128, 5600, 415, 120],
        ["DDR5", 128, 6400, 480, 132]
    ];

    for (const brand of ramBrands) {
        for (const ram of ramOptions) {
            const [
                ramType,
                capacityGb,
                speed,
                price,
                score
            ] = ram;

            for (const edition of ["Classic", "RGB"]) {
                addPart(
                    "RAM",
                    brand,
                    `${capacityGb}GB ${ramType}-${speed} ${edition}`,
                    price
                        +
                        (
                            edition === "RGB"
                                ? 18
                                : 0
                        ),
                    score
                        +
                        (
                            edition === "RGB"
                                ? 2
                                : 0
                        ),
                    {
                        ramType,
                        capacityGb,
                        speed,
                        wattage:
                            5
                            +
                            Math.round(
                                capacityGb / 16
                            )
                    }
                );
            }
        }
    }

    /* -----------------------------------------------------
       DEPOLAMA
    ----------------------------------------------------- */

    const storageBrands = [
        "Samsung",
        "Western Digital",
        "Crucial",
        "Kingston",
        "Seagate",
        "Lexar",
        "Kioxia"
    ];

    const storageOptions = [
        ["SATA", 500, 45, 18],
        ["SATA", 1000, 65, 25],
        ["SATA", 2000, 115, 34],
        ["SATA", 4000, 230, 45],
        ["NVMe", 500, 55, 27],
        ["NVMe", 1000, 82, 39],
        ["NVMe", 2000, 145, 55],
        ["NVMe", 4000, 285, 73],
        ["NVMe", 8000, 620, 98]
    ];

    for (const brand of storageBrands) {
        for (const storage of storageOptions) {
            const [
                storageInterface,
                capacityGb,
                price,
                score
            ] = storage;

            for (const edition of ["Standard", "Pro"]) {
                const capacityLabel =
                    capacityGb >= 1000
                        ? `${capacityGb / 1000}TB`
                        : `${capacityGb}GB`;

                addPart(
                    "Storage",
                    brand,
                    `${capacityLabel} ${storageInterface} ${edition}`,
                    price
                        +
                        (
                            edition === "Pro"
                                ? 35
                                : 0
                        ),
                    score
                        +
                        (
                            edition === "Pro"
                                ? 10
                                : 0
                        ),
                    {
                        storageInterface,
                        capacityGb,
                        wattage:
                            storageInterface === "NVMe"
                                ? 5
                                : 3
                    }
                );
            }
        }
    }

    /* -----------------------------------------------------
       GÜÇ KAYNAĞI
    ----------------------------------------------------- */

    const psuBrands = [
        "Corsair",
        "Seasonic",
        "be quiet!",
        "Cooler Master",
        "Thermaltake",
        "MSI"
    ];

    const ratings = [
        {
            name: "Bronze",
            priceMultiplier: 1,
            score: 18
        },
        {
            name: "Gold",
            priceMultiplier: 1.35,
            score: 29
        },
        {
            name: "Platinum",
            priceMultiplier: 1.75,
            score: 41
        }
    ];

    for (const brand of psuBrands) {
        for (
            const wattage
            of [
                450,
                550,
                650,
                750,
                850,
                1000,
                1200,
                1500
            ]
        ) {
            for (const rating of ratings) {
                addPart(
                    "PSU",
                    brand,
                    `${wattage}W 80+ ${rating.name}`,
                    wattage
                        *
                        0.13
                        *
                        rating.priceMultiplier,
                    rating.score
                        +
                        Math.round(
                            wattage / 100
                        ),
                    {
                        wattage,
                        rating:
                            rating.name
                    }
                );
            }
        }
    }

    /* -----------------------------------------------------
       KASA
    ----------------------------------------------------- */

    const caseBrands = [
        "NZXT",
        "Corsair",
        "Fractal Design",
        "Lian Li",
        "Cooler Master",
        "be quiet!",
        "Thermaltake"
    ];

    const caseModels = [
        {
            model: "Mini Air",
            supportedForms: ["mATX"],
            maximumGpuLength: 285,
            price: 58,
            score: 17
        },
        {
            model: "Compact Flow",
            supportedForms: ["mATX", "ATX"],
            maximumGpuLength: 315,
            price: 82,
            score: 25
        },
        {
            model: "Glass Flow",
            supportedForms: ["mATX", "ATX"],
            maximumGpuLength: 345,
            price: 115,
            score: 34
        },
        {
            model: "Tower Air",
            supportedForms: [
                "mATX",
                "ATX",
                "E-ATX"
            ],
            maximumGpuLength: 385,
            price: 165,
            score: 46
        },
        {
            model: "Silent Pro",
            supportedForms: [
                "mATX",
                "ATX",
                "E-ATX"
            ],
            maximumGpuLength: 370,
            price: 195,
            score: 52
        }
    ];

    for (const brand of caseBrands) {
        for (const caseModel of caseModels) {
            for (
                const style
                of [
                    "Black",
                    "White",
                    "RGB"
                ]
            ) {
                addPart(
                    "Case",
                    brand,
                    `${caseModel.model} ${style}`,
                    caseModel.price
                        +
                        (
                            style === "RGB"
                                ? 35
                                : style === "White"
                                    ? 15
                                    : 0
                        ),
                    caseModel.score
                        +
                        (
                            style === "RGB"
                                ? 6
                                : 0
                        ),
                    {
                        supportedForms:
                            caseModel.supportedForms,
                        maximumGpuLength:
                            caseModel.maximumGpuLength,
                        wattage:
                            style === "RGB"
                                ? 9
                                : 4
                    }
                );
            }
        }
    }

    /* -----------------------------------------------------
       SOĞUTUCU
    ----------------------------------------------------- */

    const coolerBrands = [
        "Noctua",
        "Arctic",
        "Corsair",
        "Cooler Master",
        "be quiet!",
        "DeepCool"
    ];

    const coolerModels = [
        {
            model: "Low Profile",
            tdp: 85,
            price: 28,
            score: 16
        },
        {
            model: "Single Tower",
            tdp: 140,
            price: 45,
            score: 25
        },
        {
            model: "Dual Tower",
            tdp: 220,
            price: 78,
            score: 38
        },
        {
            model: "AIO 240",
            tdp: 260,
            price: 125,
            score: 51
        },
        {
            model: "AIO 280",
            tdp: 300,
            price: 155,
            score: 58
        },
        {
            model: "AIO 360",
            tdp: 380,
            price: 205,
            score: 69
        }
    ];

    const socketGroups = [
        ["AM4", "AM5"],
        ["LGA1200", "LGA1700"],
        ["LGA1700", "LGA1851"],
        [
            "AM4",
            "AM5",
            "LGA1200",
            "LGA1700",
            "LGA1851"
        ]
    ];

    for (const brand of coolerBrands) {
        for (const cooler of coolerModels) {
            for (
                let groupIndex = 0;
                groupIndex < socketGroups.length;
                groupIndex += 1
            ) {
                addPart(
                    "Cooler",
                    brand,
                    `${cooler.model} Kit ${groupIndex + 1}`,
                    cooler.price
                        +
                        groupIndex * 12,
                    cooler.score
                        +
                        groupIndex * 3,
                    {
                        supportedSockets:
                            socketGroups[groupIndex],
                        tdp:
                            cooler.tdp,
                        wattage:
                            cooler.model.includes("AIO")
                                ? 10
                                : 5
                    }
                );
            }
        }
    }

    return parts;
}


const PART_DATABASE =
    createPartDatabase();


/* =========================================================
   MÜŞTERİ TÜRLERİ
========================================================= */

const CUSTOMER_TYPES = [
    {
        id: "student",
        names: {
            tr: "Öğrenci",
            en: "Student",
            de: "Student"
        },
        minimumScore: [190, 300],
        scoreGap: [45, 100],
        budgetMultiplier: [2.15, 2.75],
        deadline: [2, 5],
        tolerance: 0.80,
        requiredReputation: 0,
        requirementPool: [
            "ram16",
            "storage1000"
        ]
    },
    {
        id: "office",
        names: {
            tr: "Ofis Kullanıcısı",
            en: "Office User",
            de: "Büronutzer"
        },
        minimumScore: [220, 350],
        scoreGap: [45, 100],
        budgetMultiplier: [2.3, 2.95],
        deadline: [2, 5],
        tolerance: 0.82,
        requiredReputation: 0,
        requirementPool: [
            "ram16",
            "storage1000",
            "cpu80"
        ]
    },
    {
        id: "gamer",
        names: {
            tr: "Oyuncu",
            en: "Gamer",
            de: "Gamer"
        },
        minimumScore: [340, 600],
        scoreGap: [75, 170],
        budgetMultiplier: [2.75, 3.65],
        deadline: [2, 5],
        tolerance: 0.87,
        requiredReputation: 8,
        requirementPool: [
            "ram32",
            "storage1000",
            "gpu120"
        ]
    },
    {
        id: "streamer",
        names: {
            tr: "Yayıncı",
            en: "Streamer",
            de: "Streamer"
        },
        minimumScore: [480, 760],
        scoreGap: [85, 190],
        budgetMultiplier: [3.0, 4.0],
        deadline: [2, 4],
        tolerance: 0.89,
        requiredReputation: 25,
        requirementPool: [
            "ram32",
            "storage2000",
            "gpu160",
            "cpu130"
        ]
    },
    {
        id: "designer",
        names: {
            tr: "Tasarımcı",
            en: "Designer",
            de: "Designer"
        },
        minimumScore: [520, 820],
        scoreGap: [90, 200],
        budgetMultiplier: [3.15, 4.2],
        deadline: [2, 5],
        tolerance: 0.90,
        requiredReputation: 35,
        requirementPool: [
            "ram64",
            "storage2000",
            "cpu150"
        ]
    },
    {
        id: "esports",
        names: {
            tr: "E-Spor Oyuncusu",
            en: "E-Sports Player",
            de: "E-Sport-Spieler"
        },
        minimumScore: [680, 980],
        scoreGap: [120, 230],
        budgetMultiplier: [3.5, 4.6],
        deadline: [1, 3],
        tolerance: 0.92,
        requiredReputation: 65,
        requirementPool: [
            "ram32",
            "gpu210",
            "cpu170"
        ]
    },
    {
        id: "developer",
        names: {
            tr: "Yazılım Geliştiricisi",
            en: "Software Developer",
            de: "Softwareentwickler"
        },
        minimumScore: [500, 850],
        scoreGap: [90, 210],
        budgetMultiplier: [3.1, 4.2],
        deadline: [2, 5],
        tolerance: 0.90,
        requiredReputation: 45,
        requirementPool: [
            "ram64",
            "storage2000",
            "cpu160"
        ]
    },
    {
        id: "ai",
        names: {
            tr: "Yapay Zekâ Uzmanı",
            en: "AI Engineer",
            de: "KI-Ingenieur"
        },
        minimumScore: [850, 1250],
        scoreGap: [140, 280],
        budgetMultiplier: [4.0, 5.2],
        deadline: [2, 4],
        tolerance: 0.94,
        requiredReputation: 100,
        requirementPool: [
            "ram96",
            "storage4000",
            "gpu240",
            "cpu190"
        ]
    },
    {
        id: "corporate",
        names: {
            tr: "Kurumsal Müşteri",
            en: "Corporate Client",
            de: "Firmenkunde"
        },
        minimumScore: [600, 1000],
        scoreGap: [70, 160],
        budgetMultiplier: [3.8, 4.9],
        deadline: [2, 4],
        tolerance: 0.95,
        requiredReputation: 85,
        requirementPool: [
            "ram64",
            "storage2000",
            "cpu150"
        ]
    }
];


/* =========================================================
   BAŞLANGIÇ OYUN DURUMU
========================================================= */

function createInitialState(language = "en") {
    return {
        version: APP_VERSION,
        saveSchema: SAVE_SCHEMA_VERSION,
        language,

        money: 14000,
        reputation: 10,
        level: 1,
        experience: 0,

        calendar: {
            year: 2026,
            month: 0,
            day: 1,
            minutes: DAY_START_MINUTES
        },

        paused: false,
        speed: 1,
        automationEnabled: true,

        propertyId: "garage",
        contractType: "monthly",

        providers: {
            electricity: "eco_power",
            internet: "basic_net",
            insurance: "starter_cover"
        },

        inventory: {},

        marketOffers: [],
        builtComputers: [],
        customers: [],
        staff: [],

        upgrades: {
            storage: 0,
            workshop: 0,
            marketing: 0,
            security: 0,
            accounting: 0,
            automation: 0
        },

        daily: {
            revenue: 0,
            expenses: 0,
            computersBuilt: 0,
            computersSold: 0,
            partsPurchased: 0,
            staffTasks: 0
        },

        lifetime: {
            revenue: 0,
            expenses: 0,
            computersBuilt: 0,
            computersSold: 0,
            partsPurchased: 0,
            daysCompleted: 0
        },

        activity: [],

        nextIds: {
            offer: 1,
            computer: 1,
            customer: 1,
            staff: 1,
            activity: 1
        },

        lastStaffActions: {}
    };
}


let gameState =
    createInitialState("en");


/* =========================================================
   KAYIT SİSTEMİ
========================================================= */

function saveGame(showNotification = false) {
    try {
        const saveState =
            document.getElementById(
                "auto-save-state"
            );

        if (saveState) {
            saveState.innerHTML =
                `
                    <span class="save-dot"></span>
                    <span>${t("saving")}</span>
                `;
        }

        localStorage.setItem(
            SAVE_KEY,
            JSON.stringify(gameState)
        );

        localStorage.setItem(
            SETTINGS_KEY,
            JSON.stringify({
                language:
                    gameState.language
            })
        );

        window.setTimeout(
            () => {
                if (saveState) {
                    saveState.innerHTML =
                        `
                            <span class="save-dot"></span>
                            <span>${t("saved")}</span>
                        `;
                }
            },
            260
        );

        if (
            showNotification
            &&
            typeof showToast === "function"
        ) {
            showToast(
                t("gameSaved"),
                t("autoSaveDescription"),
                "success"
            );
        }

        return true;
    } catch (error) {
        console.error(
            "Kayıt hatası:",
            error
        );

        return false;
    }
}


function hasSaveGame() {
    return Boolean(
        localStorage.getItem(
            SAVE_KEY
        )
    );
}


function loadGame() {
    const saveText =
        localStorage.getItem(
            SAVE_KEY
        );

    if (!saveText) {
        return false;
    }

    try {
        const loadedState =
            JSON.parse(saveText);

        if (
            !loadedState
            ||
            !isCompatibleSaveState(loadedState)
        ) {
            return false;
        }

        gameState = loadedState;

        gameState.calendar.minutes =
            clamp(
                gameState.calendar.minutes,
                DAY_START_MINUTES,
                DAY_END_MINUTES
            );

        gameState.paused = false;

        return true;
    } catch (error) {
        console.error(
            "Yükleme hatası:",
            error
        );

        return false;
    }
}


function deleteSaveGame() {
    localStorage.removeItem(
        SAVE_KEY
    );

    runtime.selectedOfferId = null;
    runtime.selectedCustomerId = null;
    runtime.selectedBuiltPcId = null;
}


function loadSavedLanguage() {
    try {
        const settingsText =
            localStorage.getItem(
                SETTINGS_KEY
            );

        if (!settingsText) {
            return "en";
        }

        const settings =
            JSON.parse(settingsText);

        if (
            ["tr", "en", "de"].includes(
                settings.language
            )
        ) {
            return settings.language;
        }
    } catch (error) {
        console.error(error);
    }

    return "en";
}


/* =========================================================
   ENVANTER İŞLEMLERİ
========================================================= */

function getInventoryQuantity(partId) {
    return (
        gameState.inventory[partId]?.quantity
        ||
        0
    );
}


function getInventoryAverageCost(partId) {
    const inventoryItem =
        gameState.inventory[partId];

    if (!inventoryItem) {
        return (
            getPartById(partId)?.basePrice
            ||
            0
        );
    }

    return inventoryItem.averageCost;
}


function addInventory(
    partId,
    quantity,
    unitCost
) {
    const current =
        gameState.inventory[partId]
        ||
        {
            quantity: 0,
            averageCost: 0
        };

    const totalQuantity =
        current.quantity + quantity;

    if (totalQuantity <= 0) {
        delete gameState.inventory[
            partId
        ];

        return;
    }

    const totalCost =
        (
            current.quantity
            *
            current.averageCost
        )
        +
        (
            quantity
            *
            unitCost
        );

    gameState.inventory[partId] = {
        quantity:
            totalQuantity,

        averageCost:
            totalCost / totalQuantity
    };
}


function removeInventory(
    partId,
    quantity = 1
) {
    const current =
        gameState.inventory[partId];

    if (
        !current
        ||
        current.quantity < quantity
    ) {
        return false;
    }

    current.quantity -= quantity;

    if (current.quantity <= 0) {
        delete gameState.inventory[
            partId
        ];
    }

    return true;
}


function getInventoryCount() {
    return Object.values(
        gameState.inventory
    ).reduce(
        (
            total,
            item
        ) =>
            total + item.quantity,
        0
    );
}


function getInventoryValue() {
    return Object.values(
        gameState.inventory
    ).reduce(
        (
            total,
            item
        ) =>
            total
            +
            (
                item.quantity
                *
                item.averageCost
            ),
        0
    );
}


function getStorageCapacity() {
    const property =
        getPropertyById(
            gameState.propertyId
        );

    return (
        property.storageCapacity
        +
        gameState.upgrades.storage
        * 45
    );
}


/* =========================================================
   PAZAR ÜRETİMİ
========================================================= */

const MARKET_SELLERS = [
    "TechDepot",
    "Hardware Direct",
    "ByteMarket",
    "Digital Components",
    "Euro Hardware",
    "MegaParts",
    "Silicon Market",
    "PC Warehouse",
    "Global Components",
    "Prime Hardware",
    "NextByte",
    "Hardware World"
];


function refreshMarket() {
    const internetProvider =
        getProviderById(
            "internet",
            gameState.providers.internet
        );

    const property =
        getPropertyById(
            gameState.propertyId
        );

    const offerCount =
        54
        +
        internetProvider.marketBonus
        +
        gameState.upgrades.marketing * 7
        +
        property.customerBonus * 3;

    const offers = [];

    const bundleCount = Math.max(
        7,
        Math.ceil(offerCount / REQUIRED_COMPONENT_TYPES.length)
    );

    for (let bundleIndex = 0; bundleIndex < bundleCount; bundleIndex += 1) {
        const bundle = createCompatibleMarketBundle();

        if (!bundle) {
            continue;
        }

        for (const part of bundle) {
            const condition = randomItem([
                "new",
                "new",
                "outlet",
                "refurbished"
            ]);

            const conditionMultiplier = {
                new: 1,
                outlet: 0.87,
                refurbished: 0.74
            }[condition];

            offers.push({
                id: createId("offer"),
                partId: part.id,
                condition,
                seller: randomItem(MARKET_SELLERS),
                price: Math.max(
                    12,
                    Math.round(
                        part.basePrice
                        * conditionMultiplier
                        * randomFloat(0.78, 1.20)
                    )
                ),
                stock: part.type === "CPU"
                    ? randomInt(12, 28)
                    : randomInt(6, 18),
                bundleId: bundleIndex + 1
            });
        }
    }

    gameState.marketOffers = offers;
}


function createCompatibleMarketBundle() {
    const byType = type =>
        PART_DATABASE.filter(part => part.type === type);

    for (let attempt = 0; attempt < 120; attempt += 1) {
        const cpu = randomItem(byType("CPU"));
        const motherboard = randomItem(
            byType("Motherboard").filter(part =>
                part.socket === cpu.socket
                && part.ramType === cpu.ramType
            )
        );

        if (!motherboard) {
            continue;
        }

        const ram = randomItem(
            byType("RAM").filter(part => part.ramType === motherboard.ramType)
        );
        const storage = randomItem(
            byType("Storage").filter(part =>
                motherboard.storageInterfaces.includes(part.storageInterface)
            )
        );
        const cooler = randomItem(
            byType("Cooler").filter(part =>
                part.supportedSockets.includes(cpu.socket)
                && part.tdp >= cpu.wattage * 1.10
            )
        );
        const gpu = randomItem(byType("GPU"));
        const computerCase = randomItem(
            byType("Case").filter(part =>
                part.supportedForms.includes(motherboard.formFactor)
                && part.maximumGpuLength >= gpu.length
            )
        );

        if (!ram || !storage || !cooler || !computerCase) {
            continue;
        }

        const partial = {
            CPU: cpu,
            Motherboard: motherboard,
            GPU: gpu,
            RAM: ram,
            Storage: storage,
            Case: computerCase,
            Cooler: cooler
        };
        const requiredWattage = calculateRequiredPsuWattage(
            calculateSystemPowerDraw(partial)
        );
        const psu = randomItem(
            byType("PSU").filter(part => part.wattage >= requiredWattage)
        );

        if (!psu) {
            continue;
        }

        const partsByType = { ...partial, PSU: psu };
        const bundle = REQUIRED_COMPONENT_TYPES.map(type => partsByType[type]);

        if (getCompatibilityErrors(bundle.map(part => part.id)).length === 0) {
            return bundle;
        }
    }

    return null;
}


/* =========================================================
   MÜŞTERİ ÜRETİMİ
========================================================= */

function generateCustomer() {
    const availableTypes =
        CUSTOMER_TYPES.filter(
            type =>
                gameState.reputation
                >=
                type.requiredReputation
        );

    const customerType =
        randomItem(availableTypes);

    const minimumScore =
        randomInt(
            customerType.minimumScore[0],
            customerType.minimumScore[1]
        );

    const maximumScore =
        minimumScore
        +
        randomInt(
            customerType.scoreGap[0],
            customerType.scoreGap[1]
        );

    const requirementCount =
        randomInt(
            1,
            Math.min(
                3,
                customerType.requirementPool.length
            )
        );

    const requirements =
        [...customerType.requirementPool]
            .sort(
                () => Math.random() - 0.5
            )
            .slice(
                0,
                requirementCount
            );

    const paymentMultiplier =
        randomFloat(
            customerType.budgetMultiplier[0],
            customerType.budgetMultiplier[1]
        );

    return {
        id:
            createId("customer"),

        name:
            `${randomItem(UNIVERSAL_FIRST_NAMES)} `
            +
            `${randomItem(UNIVERSAL_LAST_NAMES)}`,

        typeId:
            customerType.id,

        minimumScore,
        maximumScore,

        payment:
            Math.round(
                minimumScore
                *
                paymentMultiplier
                +
                randomInt(
                    180,
                    650
                )
            ),

        deadlineDays:
            randomInt(
                customerType.deadline[0],
                customerType.deadline[1]
            ),

        originalDeadline:
            customerType.deadline[1],

        tolerance:
            customerType.tolerance,

        requirements
    };
}


function generateCustomers(amount) {
    const maximumCustomers =
        12
        +
        getPropertyById(
            gameState.propertyId
        ).customerBonus
        * 4;

    for (
        let index = 0;
        index < amount;
        index += 1
    ) {
        if (
            gameState.customers.length
            >=
            maximumCustomers
        ) {
            break;
        }

        gameState.customers.push(
            generateCustomer()
        );
    }
}


/* =========================================================
   YENİ OYUN HAZIRLAMA
========================================================= */

function prepareNewGame(language = "en") {
    gameState =
        createInitialState(language);

    refreshMarket();
    generateCustomers(6);

    addActivity(
        t("storeOpened"),
        "store",
        "09:00"
    );

    saveGame();

    runtime.currentPage =
        "dashboard";

    runtime.selectedOfferId = null;
    runtime.selectedCustomerId = null;
    runtime.selectedBuiltPcId = null;
}


/* =========================================================
   FAALİYET KAYDI
========================================================= */

function addActivity(
    message,
    type = "info",
    time = null
) {
    const activityTime =
        time
        ||
        minutesToTime(
            gameState.calendar.minutes
        );

    gameState.activity.unshift({
        id:
            createId("activity"),

        message,
        type,
        time:
            activityTime,

        date:
            formatGameDate()
    });

    gameState.activity =
        gameState.activity.slice(
            0,
            160
        );
}


/* =========================================================
   SEVİYE VE AŞAMA
========================================================= */

function updateLevel() {
    const calculatedLevel =
        1
        +
        Math.floor(
            gameState.experience / 500
        );

    gameState.level =
        Math.max(
            1,
            calculatedLevel
        );
}


function getStoreStage() {
    const property =
        getPropertyById(
            gameState.propertyId
        );

    return {
        name:
            t(property.nameKey),

        level:
            PROPERTY_OPTIONS.indexOf(
                property
            )
            +
            1
    };
}


/* =========================================================
   GELİR VE GİDER KAYDI
========================================================= */

function registerRevenue(amount) {
    const normalizedAmount =
        Math.max(
            0,
            Math.round(amount)
        );

    gameState.money +=
        normalizedAmount;

    gameState.daily.revenue +=
        normalizedAmount;

    gameState.lifetime.revenue +=
        normalizedAmount;
}


function registerExpense(amount) {
    const normalizedAmount =
        Math.max(
            0,
            Math.round(amount)
        );

    gameState.money -=
        normalizedAmount;

    gameState.daily.expenses +=
        normalizedAmount;

    gameState.lifetime.expenses +=
        normalizedAmount;
}


/* =========================================================
   OTOMATİK KAYIT
========================================================= */

window.addEventListener(
    "beforeunload",
    () => {
        saveGame(false);
    }
);

let lastScheduledAutoSave = 0;

window.setInterval(
    () => {
        let settings = {};

        try {
            settings = JSON.parse(localStorage.getItem(SETTINGS_KEY) || "{}") || {};
        } catch (_error) {
            settings = {};
        }

        const interval = [30, 60, 120].includes(Number(settings.autoSaveInterval))
            ? Number(settings.autoSaveInterval) * 1000
            : 30000;
        const now = Date.now();

        if (
            settings.autoSave !== false
            && runtime.gameStarted
            &&
            document.visibilityState
            ===
            "visible"
            &&
            hasSaveGame()
            && now - lastScheduledAutoSave >= interval
        ) {
            lastScheduledAutoSave = now;
            saveGame(false);
        }
    },
    5000
);

/* =========================================================
   PC SHOP EMPIRE
   GAME.JS — BÖLÜM 2
   UYUMLULUK, MONTAJ, MÜŞTERİLER, PERSONEL VE ZAMAN
========================================================= */


/* =========================================================
   BİLEŞEN TÜRLERİ
========================================================= */

const REQUIRED_COMPONENT_TYPES = [
    "CPU",
    "Motherboard",
    "GPU",
    "RAM",
    "Storage",
    "PSU",
    "Case",
    "Cooler"
];


const COMPONENT_TYPE_LABELS = {
    CPU: {
        tr: "İşlemci",
        en: "Processor",
        de: "Prozessor"
    },

    Motherboard: {
        tr: "Anakart",
        en: "Motherboard",
        de: "Mainboard"
    },

    GPU: {
        tr: "Ekran Kartı",
        en: "Graphics Card",
        de: "Grafikkarte"
    },

    RAM: {
        tr: "RAM",
        en: "RAM",
        de: "Arbeitsspeicher"
    },

    Storage: {
        tr: "Depolama",
        en: "Storage",
        de: "Speicher"
    },

    PSU: {
        tr: "Güç Kaynağı",
        en: "Power Supply",
        de: "Netzteil"
    },

    Case: {
        tr: "Kasa",
        en: "Case",
        de: "Gehäuse"
    },

    Cooler: {
        tr: "Soğutucu",
        en: "Cooler",
        de: "Kühler"
    }
};


function getComponentTypeLabel(type) {
    return (
        COMPONENT_TYPE_LABELS[type]?.[
            getLanguage()
        ]
        ||
        type
    );
}


/* =========================================================
   PERSONEL ROL İSİMLERİ
========================================================= */

const STAFF_ROLE_NAMES = {
    sales: {
        tr: "Satış Danışmanı",
        en: "Sales Consultant",
        de: "Verkaufsberater"
    },

    technician: {
        tr: "Bilgisayar Teknisyeni",
        en: "Computer Technician",
        de: "Computertechniker"
    },

    buyer: {
        tr: "Satın Alma Uzmanı",
        en: "Purchasing Specialist",
        de: "Einkaufsspezialist"
    },

    accountant: {
        tr: "Muhasebeci",
        en: "Accountant",
        de: "Buchhalter"
    },

    manager: {
        tr: "Mağaza Müdürü",
        en: "Store Manager",
        de: "Filialleiter"
    }
};


function getStaffRoleName(role) {
    return (
        STAFF_ROLE_NAMES[role]?.[
            getLanguage()
        ]
        ||
        role
    );
}


/* =========================================================
   OYUN DURUMUNU TAMAMLAMA
========================================================= */

function normalizeGameState() {
    gameState.inventory =
        gameState.inventory || {};

    gameState.marketOffers =
        gameState.marketOffers || [];

    if (
        gameState.marketOffers.length > 0
        && gameState.marketOffers.some(offer => !offer.bundleId)
    ) {
        refreshMarket();
    }

    gameState.builtComputers =
        gameState.builtComputers || [];

    gameState.customers =
        gameState.customers || [];

    gameState.staff =
        gameState.staff || [];

    gameState.activity =
        gameState.activity || [];

    gameState.lastStaffActions =
        gameState.lastStaffActions || {};

    gameState.finance = {
        emergencyLoanBalance: 0,
        loanDaysRemaining: 0,
        lastLoanDay: -99,
        ...(gameState.finance || {})
    };

    gameState.operations = {
        activeIncident: null,
        incidentLog: [],
        nextIncidentAt: 0,
        ...(gameState.operations || {})
    };

    if (!gameState.operations.nextIncidentAt) {
        gameState.operations.nextIncidentAt =
            getAbsoluteGameMinutes() + randomInt(120, 260);
    }

    gameState.upgrades = {
        storage: 0,
        workshop: 0,
        marketing: 0,
        security: 0,
        accounting: 0,
        automation: 0,
        ...(gameState.upgrades || {})
    };

    gameState.daily = {
        revenue: 0,
        expenses: 0,
        computersBuilt: 0,
        computersSold: 0,
        partsPurchased: 0,
        staffTasks: 0,
        accountingSavings: 0,
        ...(gameState.daily || {})
    };

    gameState.lifetime = {
        revenue: 0,
        expenses: 0,
        computersBuilt: 0,
        computersSold: 0,
        partsPurchased: 0,
        daysCompleted: 0,
        ...(gameState.lifetime || {})
    };

    gameState.nextIds = {
        offer: 1,
        computer: 1,
        customer: 1,
        staff: 1,
        activity: 1,
        ...(gameState.nextIds || {})
    };

    for (const employee of gameState.staff) {
        employee.quality =
            employee.quality ?? 50;

        employee.experience =
            employee.experience ?? 0;

        employee.energy =
            employee.energy ?? 100;

        employee.status =
            employee.status || "idle";

        employee.currentTask =
            employee.currentTask || "";

        employee.nextActionAt =
            employee.nextActionAt
            ||
            (
                getAbsoluteGameMinutes()
                +
                randomInt(50, 140)
            );
    }
}


/* =========================================================
   GEREKSİNİM METİNLERİ
========================================================= */

function getRequirementText(requirement) {
    const texts = {
        ram16: {
            tr: "En az 16 GB RAM",
            en: "At least 16 GB RAM",
            de: "Mindestens 16 GB RAM"
        },

        ram32: {
            tr: "En az 32 GB RAM",
            en: "At least 32 GB RAM",
            de: "Mindestens 32 GB RAM"
        },

        ram64: {
            tr: "En az 64 GB RAM",
            en: "At least 64 GB RAM",
            de: "Mindestens 64 GB RAM"
        },

        ram96: {
            tr: "En az 96 GB RAM",
            en: "At least 96 GB RAM",
            de: "Mindestens 96 GB RAM"
        },

        storage1000: {
            tr: "En az 1 TB depolama",
            en: "At least 1 TB storage",
            de: "Mindestens 1 TB Speicher"
        },

        storage2000: {
            tr: "En az 2 TB depolama",
            en: "At least 2 TB storage",
            de: "Mindestens 2 TB Speicher"
        },

        storage4000: {
            tr: "En az 4 TB depolama",
            en: "At least 4 TB storage",
            de: "Mindestens 4 TB Speicher"
        },

        gpu120: {
            tr: "GPU puanı en az 120",
            en: "GPU score of at least 120",
            de: "GPU-Punktzahl mindestens 120"
        },

        gpu160: {
            tr: "GPU puanı en az 160",
            en: "GPU score of at least 160",
            de: "GPU-Punktzahl mindestens 160"
        },

        gpu210: {
            tr: "GPU puanı en az 210",
            en: "GPU score of at least 210",
            de: "GPU-Punktzahl mindestens 210"
        },

        gpu240: {
            tr: "GPU puanı en az 240",
            en: "GPU score of at least 240",
            de: "GPU-Punktzahl mindestens 240"
        },

        cpu80: {
            tr: "CPU puanı en az 80",
            en: "CPU score of at least 80",
            de: "CPU-Punktzahl mindestens 80"
        },

        cpu130: {
            tr: "CPU puanı en az 130",
            en: "CPU score of at least 130",
            de: "CPU-Punktzahl mindestens 130"
        },

        cpu150: {
            tr: "CPU puanı en az 150",
            en: "CPU score of at least 150",
            de: "CPU-Punktzahl mindestens 150"
        },

        cpu160: {
            tr: "CPU puanı en az 160",
            en: "CPU score of at least 160",
            de: "CPU-Punktzahl mindestens 160"
        },

        cpu170: {
            tr: "CPU puanı en az 170",
            en: "CPU score of at least 170",
            de: "CPU-Punktzahl mindestens 170"
        },

        cpu190: {
            tr: "CPU puanı en az 190",
            en: "CPU score of at least 190",
            de: "CPU-Punktzahl mindestens 190"
        }
    };

    return (
        texts[requirement]?.[
            getLanguage()
        ]
        ||
        requirement
    );
}


/* =========================================================
   BİLGİSAYAR PARÇALARINI TÜRÜNE GÖRE AYIRMA
========================================================= */

function getPartsByType(partIds) {
    const partsByType = {};

    for (const partId of partIds) {
        const part =
            getPartById(partId);

        if (part) {
            partsByType[part.type] =
                part;
        }
    }

    return partsByType;
}


/* =========================================================
   GÜÇ TÜKETİMİ
========================================================= */

function calculateSystemPowerDraw(
    partsByType
) {
    let powerDraw = 35;

    const powerUsingTypes = [
        "CPU",
        "GPU",
        "RAM",
        "Storage",
        "Cooler",
        "Case"
    ];

    for (const type of powerUsingTypes) {
        const part =
            partsByType[type];

        if (!part) {
            continue;
        }

        powerDraw +=
            Number(
                part.wattage || 0
            );
    }

    return Math.round(
        powerDraw
    );
}


function calculateRequiredPsuWattage(
    powerDraw
) {
    return (
        Math.ceil(
            powerDraw
            *
            1.30
            /
            50
        )
        *
        50
    );
}


/* =========================================================
   PARÇA UYUMLULUK KONTROLÜ
========================================================= */

function getCompatibilityErrors(
    partIds
) {
    const errors = [];

    if (
        !Array.isArray(partIds)
        ||
        partIds.length
        !==
        REQUIRED_COMPONENT_TYPES.length
    ) {
        return [
            t("missingPart")
        ];
    }

    const partsByType =
        getPartsByType(partIds);

    for (
        const requiredType
        of REQUIRED_COMPONENT_TYPES
    ) {
        if (!partsByType[requiredType]) {
            errors.push(
                `${getComponentTypeLabel(requiredType)} eksik.`
            );
        }
    }

    if (errors.length > 0) {
        return errors;
    }

    const cpu =
        partsByType.CPU;

    const motherboard =
        partsByType.Motherboard;

    const gpu =
        partsByType.GPU;

    const ram =
        partsByType.RAM;

    const storage =
        partsByType.Storage;

    const powerSupply =
        partsByType.PSU;

    const computerCase =
        partsByType.Case;

    const cooler =
        partsByType.Cooler;

    if (
        cpu.socket
        !==
        motherboard.socket
    ) {
        errors.push(
            getLanguage() === "de"
                ? (
                    `CPU-Sockel ${cpu.socket} ist nicht `
                    +
                    `mit Mainboard-Sockel `
                    +
                    `${motherboard.socket} kompatibel.`
                )
                : getLanguage() === "en"
                    ? (
                        `CPU socket ${cpu.socket} does not `
                        +
                        `match motherboard socket `
                        +
                        `${motherboard.socket}.`
                    )
                    : (
                        `İşlemci soketi ${cpu.socket}, `
                        +
                        `anakart soketi `
                        +
                        `${motherboard.socket} ile uyumsuz.`
                    )
        );
    }

    if (
        cpu.ramType
        &&
        cpu.ramType
        !==
        motherboard.ramType
    ) {
        errors.push(
            getLanguage() === "de"
                ? (
                    `Der Prozessor ist für ${cpu.ramType}, `
                    +
                    `das Mainboard für `
                    +
                    `${motherboard.ramType} ausgelegt.`
                )
                : getLanguage() === "en"
                    ? (
                        `The processor uses ${cpu.ramType}, `
                        +
                        `but the motherboard uses `
                        +
                        `${motherboard.ramType}.`
                    )
                    : (
                        `İşlemci ${cpu.ramType}, `
                        +
                        `anakart ise `
                        +
                        `${motherboard.ramType} kullanıyor.`
                    )
        );
    }

    if (
        ram.ramType
        !==
        motherboard.ramType
    ) {
        errors.push(
            getLanguage() === "de"
                ? (
                    `${ram.ramType}-RAM ist nicht mit `
                    +
                    `${motherboard.ramType}-Mainboard kompatibel.`
                )
                : getLanguage() === "en"
                    ? (
                        `${ram.ramType} RAM is not compatible `
                        +
                        `with a ${motherboard.ramType} motherboard.`
                    )
                    : (
                        `${ram.ramType} RAM, `
                        +
                        `${motherboard.ramType} anakartla uyumsuz.`
                    )
        );
    }

    if (
        !computerCase.supportedForms.includes(
            motherboard.formFactor
        )
    ) {
        errors.push(
            getLanguage() === "de"
                ? (
                    `Das Gehäuse unterstützt kein `
                    +
                    `${motherboard.formFactor}-Mainboard.`
                )
                : getLanguage() === "en"
                    ? (
                        `The case does not support a `
                        +
                        `${motherboard.formFactor} motherboard.`
                    )
                    : (
                        `Kasa ${motherboard.formFactor} `
                        +
                        `anakartı desteklemiyor.`
                    )
        );
    }

    if (
        gpu.length
        >
        computerCase.maximumGpuLength
    ) {
        errors.push(
            getLanguage() === "de"
                ? (
                    `Die Grafikkarte ist ${gpu.length} mm lang, `
                    +
                    `das Gehäuse unterstützt maximal `
                    +
                    `${computerCase.maximumGpuLength} mm.`
                )
                : getLanguage() === "en"
                    ? (
                        `The graphics card is ${gpu.length} mm long, `
                        +
                        `but the case supports up to `
                        +
                        `${computerCase.maximumGpuLength} mm.`
                    )
                    : (
                        `Ekran kartı ${gpu.length} mm, `
                        +
                        `kasa en fazla `
                        +
                        `${computerCase.maximumGpuLength} mm destekliyor.`
                    )
        );
    }

    if (
        !cooler.supportedSockets.includes(
            cpu.socket
        )
    ) {
        errors.push(
            getLanguage() === "de"
                ? (
                    `Der Kühler unterstützt den `
                    +
                    `${cpu.socket}-Sockel nicht.`
                )
                : getLanguage() === "en"
                    ? (
                        `The cooler does not support `
                        +
                        `${cpu.socket}.`
                    )
                    : (
                        `Soğutucu ${cpu.socket} `
                        +
                        `soketini desteklemiyor.`
                    )
        );
    }

    if (
        cooler.tdp
        <
        cpu.wattage * 1.10
    ) {
        errors.push(
            getLanguage() === "de"
                ? (
                    `Die Kühlleistung von ${cooler.tdp} W `
                    +
                    `reicht für den Prozessor nicht aus.`
                )
                : getLanguage() === "en"
                    ? (
                        `The cooler capacity of ${cooler.tdp} W `
                        +
                        `is insufficient for the processor.`
                    )
                    : (
                        `Soğutucunun ${cooler.tdp}W kapasitesi `
                        +
                        `işlemci için yetersiz.`
                    )
        );
    }

    if (
        !motherboard.storageInterfaces.includes(
            storage.storageInterface
        )
    ) {
        errors.push(
            getLanguage() === "de"
                ? (
                    `Das Mainboard unterstützt keinen `
                    +
                    `${storage.storageInterface}-Speicher.`
                )
                : getLanguage() === "en"
                    ? (
                        `The motherboard does not support `
                        +
                        `${storage.storageInterface} storage.`
                    )
                    : (
                        `Anakart ${storage.storageInterface} `
                        +
                        `depolamayı desteklemiyor.`
                    )
        );
    }

    const systemPowerDraw =
        calculateSystemPowerDraw(
            partsByType
        );

    const requiredPsuWattage =
        calculateRequiredPsuWattage(
            systemPowerDraw
        );

    if (
        powerSupply.wattage
        <
        requiredPsuWattage
    ) {
        errors.push(
            getLanguage() === "de"
                ? (
                    `Das Netzteil ist zu schwach. `
                    +
                    `Empfohlen werden mindestens `
                    +
                    `${requiredPsuWattage} W.`
                )
                : getLanguage() === "en"
                    ? (
                        `The power supply is insufficient. `
                        +
                        `At least ${requiredPsuWattage} W `
                        +
                        `is recommended.`
                    )
                    : (
                        `Güç kaynağı yetersiz. `
                        +
                        `En az ${requiredPsuWattage}W öneriliyor.`
                    )
        );
    }

    return errors;
}


/* =========================================================
   BİLGİSAYAR ÖZELLİKLERİ
========================================================= */

function calculateComputerSpecifications(
    partIds,
    technicianQuality = 0
) {
    const parts =
        partIds
            .map(getPartById)
            .filter(Boolean);

    const partsByType =
        getPartsByType(partIds);

    const property =
        getPropertyById(
            gameState.propertyId
        );

    const baseScore =
        parts.reduce(
            (
                total,
                part
            ) =>
                total
                +
                Number(
                    part.score || 0
                ),
            0
        );

    const workshopMultiplier =
        1
        +
        property.workshopBonus
        +
        gameState.upgrades.workshop
        * 0.045
        +
        technicianQuality
        * 0.0022;

    const finalScore =
        Math.round(
            baseScore
            *
            workshopMultiplier
        );

    const totalCost =
        partIds.reduce(
            (
                total,
                partId
            ) =>
                total
                +
                getInventoryAverageCost(
                    partId
                ),
            0
        );

    const powerDraw =
        calculateSystemPowerDraw(
            partsByType
        );

    const valueMultiplier =
        1.22
        +
        property.workshopBonus
        +
        gameState.upgrades.workshop
        * 0.025
        +
        technicianQuality
        * 0.0014;

    const estimatedValue =
        Math.round(
            Math.max(
                totalCost
                *
                valueMultiplier,

                finalScore
                *
                2.20
            )
        );

    return {
        baseScore,
        score:
            finalScore,

        powerDraw,

        totalCost:
            Math.round(
                totalCost
            ),

        estimatedValue,

        ramGb:
            Number(
                partsByType.RAM?.capacityGb
                ||
                0
            ),

        storageGb:
            Number(
                partsByType.Storage?.capacityGb
                ||
                0
            ),

        gpuScore:
            Number(
                partsByType.GPU?.score
                ||
                0
            ),

        cpuScore:
            Number(
                partsByType.CPU?.score
                ||
                0
            ),

        partsByType
    };
}


/* =========================================================
   BİLGİSAYAR MONTAJI
========================================================= */

function buildComputerFromParts(
    partIds,
    options = {}
) {
    const {
        automated = false,
        technician = null
    } = options;

    const errors =
        getCompatibilityErrors(
            partIds
        );

    if (errors.length > 0) {
        return {
            success: false,
            errors
        };
    }

    for (const partId of partIds) {
        if (
            getInventoryQuantity(
                partId
            )
            <= 0
        ) {
            return {
                success: false,
                errors: [
                    getLanguage() === "de"
                        ? "Mindestens ein ausgewähltes Teil ist nicht mehr auf Lager."
                        : getLanguage() === "en"
                            ? "At least one selected component is no longer in stock."
                            : "Seçilen parçalardan en az biri stokta kalmadı."
                ]
            };
        }
    }

    const technicianQuality =
        technician?.quality || 0;

    const specifications =
        calculateComputerSpecifications(
            partIds,
            technicianQuality
        );

    for (const partId of partIds) {
        removeInventory(
            partId,
            1
        );
    }

    const computer = {
        id:
            createId("computer"),

        partIds:
            [...partIds],

        score:
            specifications.score,

        baseScore:
            specifications.baseScore,

        powerDraw:
            specifications.powerDraw,

        ramGb:
            specifications.ramGb,

        storageGb:
            specifications.storageGb,

        gpuScore:
            specifications.gpuScore,

        cpuScore:
            specifications.cpuScore,

        cost:
            specifications.totalCost,

        value:
            specifications.estimatedValue,

        builtDate:
            formatGameDate(),

        builtTime:
            minutesToTime(
                gameState.calendar.minutes
            ),

        automated,

        technicianId:
            technician?.id || null
    };

    gameState.builtComputers.push(
        computer
    );

    gameState.daily.computersBuilt += 1;
    gameState.lifetime.computersBuilt += 1;

    gameState.experience +=
        Math.max(
            15,
            Math.round(
                computer.score / 25
            )
        );

    updateLevel();

    if (technician) {
        technician.experience +=
            randomInt(
                5,
                12
            );
    }

    addActivity(
        getLanguage() === "de"
            ? (
                `${computer.id} wurde mit `
                +
                `${computer.score} Punkten montiert.`
            )
            : getLanguage() === "en"
                ? (
                    `${computer.id} was assembled `
                    +
                    `with a score of ${computer.score}.`
                )
                : (
                    `${computer.id}, `
                    +
                    `${computer.score} puanla monte edildi.`
                ),
        "build"
    );

    saveGame(false);

    return {
        success: true,
        computer
    };
}


/* =========================================================
   ENVANTERDEKİ PARÇALARI TÜRÜNE GÖRE GETİRME
========================================================= */

function getInventoryPartsByType(type) {
    const parts = [];

    for (
        const [
            partId,
            inventoryItem
        ]
        of Object.entries(
            gameState.inventory
        )
    ) {
        if (
            inventoryItem.quantity
            <= 0
        ) {
            continue;
        }

        const part =
            getPartById(partId);

        if (
            part
            &&
            part.type === type
        ) {
            parts.push(part);
        }
    }

    return parts;
}


/* =========================================================
   OTOMATİK UYUMLU SİSTEM BULMA
========================================================= */

function findCompatibleInventoryBuild(
    technicianQuality = 50
) {
    const candidates = {};

    for (
        const type
        of REQUIRED_COMPONENT_TYPES
    ) {
        candidates[type] =
            getInventoryPartsByType(
                type
            );

        if (
            candidates[type].length
            ===
            0
        ) {
            return null;
        }
    }

    const qualityRatio =
        clamp(
            technicianQuality / 100,
            0,
            1
        );

    function sortCandidates(
        candidateParts
    ) {
        return [
            ...candidateParts
        ].sort(
            (
                first,
                second
            ) => {
                if (
                    qualityRatio
                    >=
                    0.65
                ) {
                    return (
                        second.score
                        -
                        first.score
                    );
                }

                return (
                    getInventoryAverageCost(
                        first.id
                    )
                    -
                    getInventoryAverageCost(
                        second.id
                    )
                );
            }
        ).slice(
            0,
            14
        );
    }

    const cpus =
        sortCandidates(
            candidates.CPU
        );

    const motherboards =
        sortCandidates(
            candidates.Motherboard
        );

    const graphicsCards =
        sortCandidates(
            candidates.GPU
        );

    const memoryModules =
        sortCandidates(
            candidates.RAM
        );

    const storageDevices =
        sortCandidates(
            candidates.Storage
        );

    const powerSupplies =
        sortCandidates(
            candidates.PSU
        );

    const computerCases =
        sortCandidates(
            candidates.Case
        );

    const coolers =
        sortCandidates(
            candidates.Cooler
        );

    const validBuilds = [];

    for (const cpu of cpus) {
        const matchingMotherboards =
            motherboards.filter(
                motherboard =>
                    motherboard.socket
                    ===
                    cpu.socket
                    &&
                    motherboard.ramType
                    ===
                    cpu.ramType
            );

        for (
            const motherboard
            of matchingMotherboards
        ) {
            const matchingMemory =
                memoryModules.filter(
                    ram =>
                        ram.ramType
                        ===
                        motherboard.ramType
                );

            const matchingCoolers =
                coolers.filter(
                    cooler =>
                        cooler.supportedSockets.includes(
                            cpu.socket
                        )
                        &&
                        cooler.tdp
                        >=
                        cpu.wattage
                        * 1.10
                );

            const matchingCases =
                computerCases.filter(
                    computerCase =>
                        computerCase.supportedForms.includes(
                            motherboard.formFactor
                        )
                );

            for (
                const computerCase
                of matchingCases
            ) {
                const matchingGraphicsCards =
                    graphicsCards.filter(
                        gpu =>
                            gpu.length
                            <=
                            computerCase.maximumGpuLength
                    );

                for (
                    const gpu
                    of matchingGraphicsCards
                ) {
                    const matchingStorage =
                        storageDevices.filter(
                            storage =>
                                motherboard.storageInterfaces.includes(
                                    storage.storageInterface
                                )
                        );

                    if (
                        matchingMemory.length === 0
                        ||
                        matchingCoolers.length === 0
                        ||
                        matchingStorage.length === 0
                    ) {
                        continue;
                    }

                    const ram =
                        matchingMemory[0];

                    const storage =
                        matchingStorage[0];

                    const cooler =
                        matchingCoolers[0];

                    const partialParts = {
                        CPU: cpu,
                        Motherboard:
                            motherboard,
                        GPU: gpu,
                        RAM: ram,
                        Storage: storage,
                        Case:
                            computerCase,
                        Cooler: cooler
                    };

                    const powerDraw =
                        calculateSystemPowerDraw(
                            partialParts
                        );

                    const requiredWattage =
                        calculateRequiredPsuWattage(
                            powerDraw
                        );

                    const powerSupply =
                        powerSupplies.find(
                            psu =>
                                psu.wattage
                                >=
                                requiredWattage
                        );

                    if (!powerSupply) {
                        continue;
                    }

                    const partIds = [
                        cpu.id,
                        motherboard.id,
                        gpu.id,
                        ram.id,
                        storage.id,
                        powerSupply.id,
                        computerCase.id,
                        cooler.id
                    ];

                    const errors =
                        getCompatibilityErrors(
                            partIds
                        );

                    if (
                        errors.length
                        ===
                        0
                    ) {
                        const specifications =
                            calculateComputerSpecifications(
                                partIds,
                                technicianQuality
                            );

                        validBuilds.push({
                            partIds,
                            score:
                                specifications.score,
                            cost:
                                specifications.totalCost,
                            value:
                                specifications.estimatedValue
                        });
                    }

                    if (
                        validBuilds.length
                        >=
                        24
                    ) {
                        break;
                    }
                }

                if (
                    validBuilds.length
                    >=
                    24
                ) {
                    break;
                }
            }

            if (
                validBuilds.length
                >=
                24
            ) {
                break;
            }
        }

        if (
            validBuilds.length
            >=
            24
        ) {
            break;
        }
    }

    if (
        validBuilds.length
        ===
        0
    ) {
        return null;
    }

    validBuilds.sort(
        (
            first,
            second
        ) => {
            if (
                technicianQuality
                >=
                70
            ) {
                return (
                    second.score
                    -
                    first.score
                );
            }

            const firstEfficiency =
                first.score
                /
                Math.max(
                    1,
                    first.cost
                );

            const secondEfficiency =
                second.score
                /
                Math.max(
                    1,
                    second.cost
                );

            return (
                secondEfficiency
                -
                firstEfficiency
            );
        }
    );

    return validBuilds[0];
}


/* =========================================================
   MÜŞTERİ GEREKSİNİM KONTROLÜ
========================================================= */

function getCustomerRequirementFailures(
    customer,
    computer
) {
    const failures = [];

    for (
        const requirement
        of customer.requirements
    ) {
        let failed = false;

        if (
            requirement === "ram16"
            &&
            computer.ramGb < 16
        ) {
            failed = true;
        }

        if (
            requirement === "ram32"
            &&
            computer.ramGb < 32
        ) {
            failed = true;
        }

        if (
            requirement === "ram64"
            &&
            computer.ramGb < 64
        ) {
            failed = true;
        }

        if (
            requirement === "ram96"
            &&
            computer.ramGb < 96
        ) {
            failed = true;
        }

        if (
            requirement === "storage1000"
            &&
            computer.storageGb < 1000
        ) {
            failed = true;
        }

        if (
            requirement === "storage2000"
            &&
            computer.storageGb < 2000
        ) {
            failed = true;
        }

        if (
            requirement === "storage4000"
            &&
            computer.storageGb < 4000
        ) {
            failed = true;
        }

        if (
            requirement === "gpu120"
            &&
            computer.gpuScore < 120
        ) {
            failed = true;
        }

        if (
            requirement === "gpu160"
            &&
            computer.gpuScore < 160
        ) {
            failed = true;
        }

        if (
            requirement === "gpu210"
            &&
            computer.gpuScore < 210
        ) {
            failed = true;
        }

        if (
            requirement === "gpu240"
            &&
            computer.gpuScore < 240
        ) {
            failed = true;
        }

        if (
            requirement === "cpu80"
            &&
            computer.cpuScore < 80
        ) {
            failed = true;
        }

        if (
            requirement === "cpu130"
            &&
            computer.cpuScore < 130
        ) {
            failed = true;
        }

        if (
            requirement === "cpu150"
            &&
            computer.cpuScore < 150
        ) {
            failed = true;
        }

        if (
            requirement === "cpu160"
            &&
            computer.cpuScore < 160
        ) {
            failed = true;
        }

        if (
            requirement === "cpu170"
            &&
            computer.cpuScore < 170
        ) {
            failed = true;
        }

        if (
            requirement === "cpu190"
            &&
            computer.cpuScore < 190
        ) {
            failed = true;
        }

        if (failed) {
            failures.push(
                getRequirementText(
                    requirement
                )
            );
        }
    }

    return failures;
}


/* =========================================================
   MÜŞTERİ İÇİN EN UYGUN BİLGİSAYARI BULMA
========================================================= */

function findBestComputerForCustomer(
    customer
) {
    const suitableComputers = [];

    for (
        const computer
        of gameState.builtComputers
    ) {
        const requirementFailures =
            getCustomerRequirementFailures(
                customer,
                computer
            );

        if (
            requirementFailures.length
            >
            0
        ) {
            continue;
        }

        const minimumAcceptedScore =
            Math.floor(
                customer.minimumScore
                *
                customer.tolerance
            );

        if (
            computer.score
            <
            minimumAcceptedScore
        ) {
            continue;
        }

        let matchScore = 0;

        if (
            computer.score
            >=
            customer.minimumScore
            &&
            computer.score
            <=
            customer.maximumScore
        ) {
            matchScore += 1000;
        }

        const distanceFromTarget =
            Math.abs(
                computer.score
                -
                customer.minimumScore
            );

        matchScore -=
            distanceFromTarget;

        const expectedProfit =
            customer.payment
            -
            computer.cost;

        matchScore +=
            expectedProfit
            *
            0.08;

        suitableComputers.push({
            computer,
            matchScore
        });
    }

    suitableComputers.sort(
        (
            first,
            second
        ) =>
            second.matchScore
            -
            first.matchScore
    );

    return (
        suitableComputers[0]?.computer
        ||
        null
    );
}


/* =========================================================
   MÜŞTERİYE TESLİMAT
========================================================= */

function deliverComputerToCustomer(
    customerId,
    computerId,
    options = {}
) {
    const {
        automated = false,
        employee = null
    } = options;

    const customer =
        getCustomerById(
            customerId
        );

    const computer =
        getBuiltPcById(
            computerId
        );

    if (
        !customer
        ||
        !computer
    ) {
        return {
            success: false,
            message:
                t("selectPc")
        };
    }

    const requirementFailures =
        getCustomerRequirementFailures(
            customer,
            computer
        );

    if (
        requirementFailures.length
        >
        0
    ) {
        return {
            success: false,
            message:
                requirementFailures.join(
                    "\n"
                )
        };
    }

    const minimumAcceptedScore =
        Math.floor(
            customer.minimumScore
            *
            customer.tolerance
        );

    if (
        computer.score
        <
        minimumAcceptedScore
    ) {
        return {
            success: false,
            message:
                t("rejected")
        };
    }

    let payout =
        customer.payment;

    let reputationChange = 0;
    let satisfaction = "";

    if (
        computer.score
        <
        customer.minimumScore
    ) {
        const shortage =
            customer.minimumScore
            -
            computer.score;

        const shortageRatio =
            shortage
            /
            customer.minimumScore;

        const discountRate =
            clamp(
                0.08
                +
                shortageRatio
                *
                1.15,
                0.08,
                0.34
            );

        payout =
            Math.round(
                customer.payment
                *
                (
                    1
                    -
                    discountRate
                )
            );

        reputationChange =
            -Math.max(
                1,
                Math.ceil(
                    shortageRatio
                    *
                    10
                )
            );

        satisfaction =
            getLanguage() === "de"
                ? "Unzufrieden"
                : getLanguage() === "en"
                    ? "Unsatisfied"
                    : "Memnun değil";
    } else if (
        computer.score
        <=
        customer.maximumScore
    ) {
        const salesQuality =
            employee?.quality || 0;

        const negotiationBonus =
            randomFloat(
                0.02,
                0.07
            )
            +
            salesQuality
            *
            0.0005;

        payout =
            Math.round(
                customer.payment
                *
                (
                    1
                    +
                    negotiationBonus
                )
            );

        reputationChange =
            employee
                ? (
                    employee.quality >= 70
                        ? 3
                        : 2
                )
                : 2;

        satisfaction =
            getLanguage() === "de"
                ? "Zufrieden"
                : getLanguage() === "en"
                    ? "Satisfied"
                    : "Memnun";
    } else {
        payout =
            Math.round(
                customer.payment
                *
                0.98
            );

        reputationChange = 1;

        satisfaction =
            getLanguage() === "de"
                ? "Leistungsstärker als benötigt"
                : getLanguage() === "en"
                    ? "More powerful than required"
                    : "İstenenden daha güçlü";
    }

    if (
        employee
        &&
        employee.role === "sales"
    ) {
        const additionalSalesBonus =
            Math.round(
                payout
                *
                clamp(
                    employee.quality
                    *
                    0.00048,
                    0,
                    0.055
                )
            );

        payout +=
            additionalSalesBonus;

        employee.experience +=
            randomInt(
                4,
                10
            );
    }

    registerRevenue(
        payout
    );

    gameState.reputation =
        Math.max(
            0,
            gameState.reputation
            +
            reputationChange
        );

    gameState.builtComputers =
        gameState.builtComputers.filter(
            item =>
                item.id
                !==
                computer.id
        );

    gameState.customers =
        gameState.customers.filter(
            item =>
                item.id
                !==
                customer.id
        );

    gameState.daily.computersSold += 1;
    gameState.lifetime.computersSold += 1;

    gameState.experience +=
        Math.max(
            18,
            Math.round(
                payout / 45
            )
        );

    updateLevel();

    addActivity(
        getLanguage() === "de"
            ? (
                `${customer.name}: ${computer.id} verkauft. `
                +
                `Einnahmen ${formatMoney(payout)}, `
                +
                `Ruf ${reputationChange >= 0 ? "+" : ""}`
                +
                `${reputationChange}.`
            )
            : getLanguage() === "en"
                ? (
                    `${customer.name}: ${computer.id} sold. `
                    +
                    `Revenue ${formatMoney(payout)}, `
                    +
                    `reputation `
                    +
                    `${reputationChange >= 0 ? "+" : ""}`
                    +
                    `${reputationChange}.`
                )
                : (
                    `${customer.name}: ${computer.id} satıldı. `
                    +
                    `Gelir ${formatMoney(payout)}, `
                    +
                    `itibar `
                    +
                    `${reputationChange >= 0 ? "+" : ""}`
                    +
                    `${reputationChange}.`
                ),
        "sale"
    );

    saveGame(false);

    return {
        success: true,
        payout,
        reputationChange,
        satisfaction,
        automated
    };
}


/* =========================================================
   PERSONEL KAPASİTESİ
========================================================= */

function getStaffCapacity() {
    const property =
        getPropertyById(
            gameState.propertyId
        );

    return (
        property.staffCapacity
        +
        gameState.upgrades.automation
    );
}


/* =========================================================
   PERSONEL OLUŞTURMA
========================================================= */

function createEmployee(role) {
    const roleInformation =
        STAFF_ROLES[role];

    const quality =
        randomInt(
            38,
            74
        );

    const salary =
        Math.round(
            roleInformation.salaryBase
            *
            (
                0.84
                +
                quality / 180
            )
        );

    const employee = {
        id:
            createId("staff"),

        name:
            `${randomItem(UNIVERSAL_FIRST_NAMES)} `
            +
            `${randomItem(UNIVERSAL_LAST_NAMES)}`,

        role,

        quality,

        experience:
            randomInt(
                0,
                25
            ),

        energy: 100,

        salary,

        status: "idle",

        currentTask: "",

        hiredDate:
            formatGameDate(),

        nextActionAt:
            getAbsoluteGameMinutes()
            +
            randomInt(
                60,
                roleInformation.baseIntervalMinutes
            )
    };

    return employee;
}


/* =========================================================
   PERSONEL İŞE ALMA
========================================================= */

function hireEmployee(role) {
    const roleInformation =
        STAFF_ROLES[role];

    if (!roleInformation) {
        return {
            success: false
        };
    }

    if (
        gameState.staff.length
        >=
        getStaffCapacity()
    ) {
        return {
            success: false,
            message:
                getLanguage() === "de"
                    ? "Die Mitarbeiterkapazität des Geschäfts ist voll."
                    : getLanguage() === "en"
                        ? "The store's employee capacity is full."
                        : "Dükkânın personel kapasitesi dolu."
        };
    }

    if (
        gameState.money
        <
        roleInformation.hiringCost
    ) {
        return {
            success: false,
            message:
                t("insufficientMoney")
        };
    }

    registerExpense(
        roleInformation.hiringCost
    );

    const employee =
        createEmployee(role);

    gameState.staff.push(
        employee
    );

    addActivity(
        getLanguage() === "de"
            ? (
                `${employee.name} wurde als `
                +
                `${getStaffRoleName(role)} eingestellt.`
            )
            : getLanguage() === "en"
                ? (
                    `${employee.name} was hired as `
                    +
                    `${getStaffRoleName(role)}.`
                )
                : (
                    `${employee.name}, `
                    +
                    `${getStaffRoleName(role)} olarak işe alındı.`
                ),
        "staff"
    );

    saveGame(false);

    return {
        success: true,
        employee
    };
}


/* =========================================================
   PERSONEL İŞTEN ÇIKARMA
========================================================= */

function fireEmployee(employeeId) {
    const employee =
        gameState.staff.find(
            item =>
                item.id
                ===
                employeeId
        );

    if (!employee) {
        return false;
    }

    gameState.staff =
        gameState.staff.filter(
            item =>
                item.id
                !==
                employeeId
        );

    gameState.reputation =
        Math.max(
            0,
            gameState.reputation - 1
        );

    delete gameState.lastStaffActions[
        employeeId
    ];

    addActivity(
        getLanguage() === "de"
            ? (
                `${employee.name} wurde entlassen. Ruf -1.`
            )
            : getLanguage() === "en"
                ? (
                    `${employee.name} was dismissed. Reputation -1.`
                )
                : (
                    `${employee.name} işten çıkarıldı. İtibar -1.`
                ),
        "staff"
    );

    saveGame(false);

    return true;
}


/* =========================================================
   PERSONEL EĞİTİMİ
========================================================= */

function trainEmployee(employeeId) {
    const employee =
        gameState.staff.find(
            item =>
                item.id
                ===
                employeeId
        );

    if (!employee) {
        return {
            success: false
        };
    }

    const roleInformation =
        STAFF_ROLES[
            employee.role
        ];

    const trainingPrice =
        Math.round(
            roleInformation.trainingCost
            *
            (
                1
                +
                employee.quality
                /
                120
            )
        );

    if (
        gameState.money
        <
        trainingPrice
    ) {
        return {
            success: false,
            message:
                t("insufficientMoney")
        };
    }

    registerExpense(
        trainingPrice
    );

    const qualityIncrease =
        randomInt(
            7,
            13
        );

    employee.quality =
        clamp(
            employee.quality
            +
            qualityIncrease,
            1,
            100
        );

    employee.experience +=
        randomInt(
            10,
            25
        );

    employee.salary =
        Math.round(
            roleInformation.salaryBase
            *
            (
                0.84
                +
                employee.quality
                /
                180
            )
        );

    addActivity(
        getLanguage() === "de"
            ? (
                `${employee.name} wurde geschult. `
                +
                `Qualität +${qualityIncrease}.`
            )
            : getLanguage() === "en"
                ? (
                    `${employee.name} completed training. `
                    +
                    `Quality +${qualityIncrease}.`
                )
                : (
                    `${employee.name} eğitim aldı. `
                    +
                    `Kalite +${qualityIncrease}.`
                ),
        "staff"
    );

    saveGame(false);

    return {
        success: true,
        trainingPrice,
        qualityIncrease
    };
}


/* =========================================================
   PERSONEL ETKİLERİ
========================================================= */

function getManagerStrength() {
    const managers =
        gameState.staff.filter(
            employee =>
                employee.role
                ===
                "manager"
        );

    if (
        managers.length
        ===
        0
    ) {
        return 0;
    }

    const totalQuality =
        managers.reduce(
            (
                total,
                manager
            ) =>
                total
                +
                manager.quality,
            0
        );

    return clamp(
        totalQuality
        /
        450,
        0,
        0.36
    );
}


function getAccountantDiscount() {
    const accountants =
        gameState.staff.filter(
            employee =>
                employee.role
                ===
                "accountant"
        );

    const staffDiscount =
        accountants.reduce(
            (
                total,
                accountant
            ) =>
                total
                +
                accountant.quality
                *
                0.001,
            0
        );

    const upgradeDiscount =
        gameState.upgrades.accounting
        *
        0.035;

    return clamp(
        staffDiscount
        +
        upgradeDiscount,
        0,
        0.48
    );
}


function getAutomationInterval(
    employee
) {
    const roleInformation =
        STAFF_ROLES[
            employee.role
        ];

    const qualityReduction =
        employee.quality
        *
        0.004;

    const experienceReduction =
        Math.min(
            0.18,
            employee.experience
            /
            1200
        );

    const managerReduction =
        getManagerStrength();

    const internetProvider =
        getProviderById(
            "internet",
            gameState.providers.internet
        );

    const internetReduction =
        internetProvider.automationBonus
        ||
        0;

    const upgradeReduction =
        gameState.upgrades.automation
        *
        0.035;

    const totalReduction =
        clamp(
            qualityReduction
            +
            experienceReduction
            +
            managerReduction
            +
            internetReduction
            +
            upgradeReduction,
            0,
            0.70
        );

    return Math.max(
        42,
        Math.round(
            roleInformation.baseIntervalMinutes
            *
            (
                1
                -
                totalReduction
            )
        )
    );
}


/* =========================================================
   PERSONELİN SONRAKİ GÖREVİNİ AYARLAMA
========================================================= */

function scheduleEmployeeNextAction(
    employee
) {
    employee.nextActionAt =
        getAbsoluteGameMinutes()
        +
        getAutomationInterval(
            employee
        )
        +
        randomInt(
            -12,
            18
        );
}


/* =========================================================
   OTOMATİK SATIŞ
========================================================= */

function runSalesAutomation(
    employee
) {
    if (
        gameState.customers.length
        ===
        0
        ||
        gameState.builtComputers.length
        ===
        0
    ) {
        employee.currentTask =
            getLanguage() === "de"
                ? "Wartet auf Kunden oder fertige Computer"
                : getLanguage() === "en"
                    ? "Waiting for customers or ready computers"
                    : "Müşteri veya hazır bilgisayar bekliyor";

        return false;
    }

    const sortedCustomers =
        [...gameState.customers]
            .sort(
                (
                    first,
                    second
                ) =>
                    first.deadlineDays
                    -
                    second.deadlineDays
            );

    for (
        const customer
        of sortedCustomers
    ) {
        const computer =
            findBestComputerForCustomer(
                customer
            );

        if (!computer) {
            continue;
        }

        const result =
            deliverComputerToCustomer(
                customer.id,
                computer.id,
                {
                    automated: true,
                    employee
                }
            );

        if (result.success) {
            employee.currentTask =
                getLanguage() === "de"
                    ? `${computer.id} an ${customer.name} verkauft`
                    : getLanguage() === "en"
                        ? `Sold ${computer.id} to ${customer.name}`
                        : `${computer.id}, ${customer.name} müşterisine satıldı`;

            return true;
        }
    }

    employee.currentTask =
        getLanguage() === "de"
            ? "Kein passender Computer gefunden"
            : getLanguage() === "en"
                ? "No suitable computer found"
                : "Uygun bilgisayar bulunamadı";

    return false;
}


/* =========================================================
   OTOMATİK MONTAJ
========================================================= */

function runTechnicianAutomation(
    employee
) {
    const build =
        findCompatibleInventoryBuild(
            employee.quality
        );

    if (!build) {
        employee.currentTask =
            getLanguage() === "de"
                ? "Wartet auf kompatible Teile"
                : getLanguage() === "en"
                    ? "Waiting for compatible components"
                    : "Uyumlu parçalar bekliyor";

        return false;
    }

    const result =
        buildComputerFromParts(
            build.partIds,
            {
                automated: true,
                technician:
                    employee
            }
        );

    if (result.success) {
        employee.currentTask =
            getLanguage() === "de"
                ? `${result.computer.id} montiert`
                : getLanguage() === "en"
                    ? `Assembled ${result.computer.id}`
                    : `${result.computer.id} monte edildi`;

        return true;
    }

    employee.currentTask =
        getLanguage() === "de"
            ? "Montage fehlgeschlagen"
            : getLanguage() === "en"
                ? "Assembly failed"
                : "Montaj başarısız";

    return false;
}


/* =========================================================
   OTOMATİK SATIN ALMA
========================================================= */

function runBuyerAutomation(
    employee
) {
    if (
        getInventoryCount()
        >=
        getStorageCapacity()
    ) {
        employee.currentTask =
            getLanguage() === "de"
                ? "Lager ist voll"
                : getLanguage() === "en"
                    ? "Storage is full"
                    : "Depo dolu";

        return false;
    }

    const stage =
        getStoreStage();

    const desiredStock =
        2
        +
        stage.level
        +
        Math.floor(
            employee.quality / 35
        );

    const typeStock = {};

    for (
        const type
        of REQUIRED_COMPONENT_TYPES
    ) {
        typeStock[type] =
            getInventoryPartsByType(
                type
            ).reduce(
                (
                    total,
                    part
                ) =>
                    total
                    +
                    getInventoryQuantity(
                        part.id
                    ),
                0
            );
    }

    const targetType =
        [...REQUIRED_COMPONENT_TYPES]
            .sort(
                (
                    first,
                    second
                ) =>
                    typeStock[first]
                    -
                    typeStock[second]
            )
            .find(
                type =>
                    typeStock[type]
                    <
                    desiredStock
            )
        ||
        randomItem(
            REQUIRED_COMPONENT_TYPES
        );

    const suitableOffers =
        gameState.marketOffers
            .filter(
                offer => {
                    if (
                        offer.stock
                        <=
                        0
                    ) {
                        return false;
                    }

                    const part =
                        getPartById(
                            offer.partId
                        );

                    return (
                        part?.type
                        ===
                        targetType
                    );
                }
            )
            .sort(
                (
                    first,
                    second
                ) => {
                    const firstPart =
                        getPartById(
                            first.partId
                        );

                    const secondPart =
                        getPartById(
                            second.partId
                        );

                    const firstRatio =
                        first.price
                        /
                        Math.max(
                            1,
                            firstPart.basePrice
                        );

                    const secondRatio =
                        second.price
                        /
                        Math.max(
                            1,
                            secondPart.basePrice
                        );

                    return (
                        firstRatio
                        -
                        secondRatio
                    );
                }
            );

    const selectedOffer =
        suitableOffers[0];

    if (!selectedOffer) {
        employee.currentTask =
            getLanguage() === "de"
                ? "Kein passendes Marktangebot"
                : getLanguage() === "en"
                    ? "No suitable market offer"
                    : "Uygun pazar teklifi bulunamadı";

        return false;
    }

    const negotiationDiscount =
        clamp(
            employee.quality
            *
            0.0016,
            0.02,
            0.18
        );

    const finalPrice =
        Math.max(
            1,
            Math.round(
                selectedOffer.price
                *
                (
                    1
                    -
                    negotiationDiscount
                )
            )
        );

    if (
        gameState.money
        <
        finalPrice
    ) {
        employee.currentTask =
            getLanguage() === "de"
                ? "Nicht genügend Geld für den Einkauf"
                : getLanguage() === "en"
                    ? "Not enough cash for purchasing"
                    : "Satın alma için yeterli para yok";

        return false;
    }

    registerExpense(
        finalPrice
    );

    addInventory(
        selectedOffer.partId,
        1,
        finalPrice
    );

    selectedOffer.stock -= 1;

    gameState.daily.partsPurchased += 1;
    gameState.lifetime.partsPurchased += 1;

    const purchasedPart =
        getPartById(
            selectedOffer.partId
        );

    employee.currentTask =
        getLanguage() === "de"
            ? `${purchasedPart.name} gekauft`
            : getLanguage() === "en"
                ? `Purchased ${purchasedPart.name}`
                : `${purchasedPart.name} satın alındı`;

    addActivity(
        getLanguage() === "de"
            ? (
                `${employee.name} kaufte `
                +
                `${purchasedPart.name} für `
                +
                `${formatMoney(finalPrice)}.`
            )
            : getLanguage() === "en"
                ? (
                    `${employee.name} purchased `
                    +
                    `${purchasedPart.name} for `
                    +
                    `${formatMoney(finalPrice)}.`
                )
                : (
                    `${employee.name}, `
                    +
                    `${purchasedPart.name} ürününü `
                    +
                    `${formatMoney(finalPrice)} karşılığında aldı.`
                ),
        "purchase"
    );

    return true;
}


/* =========================================================
   OTOMATİK MUHASEBE
========================================================= */

function runAccountantAutomation(
    employee
) {
    const baseSaving =
        35
        +
        gameState.daily.expenses
        *
        0.024;

    const saving =
        Math.round(
            baseSaving
            *
            (
                0.55
                +
                employee.quality
                /
                65
            )
        );

    gameState.daily.accountingSavings =
        (
            gameState.daily.accountingSavings
            ||
            0
        )
        +
        saving;

    employee.currentTask =
        getLanguage() === "de"
            ? `${formatMoney(saving)} Betriebskosten gespart`
            : getLanguage() === "en"
                ? `Saved ${formatMoney(saving)} in operating costs`
                : `${formatMoney(saving)} işletme tasarrufu sağladı`;

    addActivity(
        getLanguage() === "de"
            ? (
                `${employee.name} reduzierte die `
                +
                `voraussichtlichen Kosten um `
                +
                `${formatMoney(saving)}.`
            )
            : getLanguage() === "en"
                ? (
                    `${employee.name} reduced estimated `
                    +
                    `expenses by ${formatMoney(saving)}.`
                )
                : (
                    `${employee.name}, tahmini giderleri `
                    +
                    `${formatMoney(saving)} azalttı.`
                ),
        "finance"
    );

    return true;
}


/* =========================================================
   OTOMATİK MÜDÜR GÖREVİ
========================================================= */

function runManagerAutomation(
    employee
) {
    let restoredEnergy = 0;

    for (
        const otherEmployee
        of gameState.staff
    ) {
        if (
            otherEmployee.id
            ===
            employee.id
        ) {
            continue;
        }

        const energyGain =
            randomInt(
                9,
                18
            )
            +
            Math.floor(
                employee.quality / 15
            );

        const previousEnergy =
            otherEmployee.energy;

        otherEmployee.energy =
            clamp(
                otherEmployee.energy
                +
                energyGain,
                0,
                100
            );

        restoredEnergy +=
            otherEmployee.energy
            -
            previousEnergy;

        otherEmployee.experience += 1;
    }

    employee.currentTask =
        getLanguage() === "de"
            ? (
                `Team unterstützt, `
                +
                `${restoredEnergy} Energie wiederhergestellt`
            )
            : getLanguage() === "en"
                ? (
                    `Supported the team and restored `
                    +
                    `${restoredEnergy} energy`
                )
                : (
                    `Ekibi yönetti ve toplam `
                    +
                    `${restoredEnergy} enerji yeniledi`
                );

    return true;
}


/* =========================================================
   PERSONEL OTOMASYON GÖREVİNİ ÇALIŞTIRMA
========================================================= */

function runEmployeeAutomation(
    employee
) {
    if (
        !gameState.automationEnabled
    ) {
        return false;
    }

    if (
        employee.energy
        <
        12
    ) {
        employee.status =
            "resting";

        employee.currentTask =
            getLanguage() === "de"
                ? "Ruht sich aus"
                : getLanguage() === "en"
                    ? "Resting"
                    : "Dinleniyor";

        scheduleEmployeeNextAction(
            employee
        );

        return false;
    }

    employee.status =
        "working";

    const managerStrength =
        getManagerStrength();

    const successProbability =
        clamp(
            0.64
            +
            employee.quality
            *
            0.0032
            +
            (employee.morale ?? 70)
            *
            0.0011
            -
            (employee.fatigue ?? 0)
            *
            0.0010
            +
            managerStrength
            *
            0.35,
            0.68,
            0.98
        );

    let taskSucceeded = false;

    if (
        chance(
            successProbability
        )
    ) {
        if (
            employee.role
            ===
            "sales"
        ) {
            taskSucceeded =
                runSalesAutomation(
                    employee
                );
        }

        if (
            employee.role
            ===
            "technician"
        ) {
            taskSucceeded =
                runTechnicianAutomation(
                    employee
                );
        }

        if (
            employee.role
            ===
            "buyer"
        ) {
            taskSucceeded =
                runBuyerAutomation(
                    employee
                );
        }

        if (
            employee.role
            ===
            "accountant"
        ) {
            taskSucceeded =
                runAccountantAutomation(
                    employee
                );
        }

        if (
            employee.role
            ===
            "manager"
        ) {
            taskSucceeded =
                runManagerAutomation(
                    employee
                );
        }
    } else {
        employee.currentTask =
            getLanguage() === "de"
                ? "Aufgabe konnte nicht abgeschlossen werden"
                : getLanguage() === "en"
                    ? "Could not complete the task"
                    : "Görevi tamamlayamadı";
    }

    const energyCost =
        taskSucceeded
            ? randomInt(
                8,
                15
            )
            : randomInt(
                3,
                7
            );

    employee.energy =
        clamp(
            employee.energy
            -
            energyCost,
            0,
            100
        );

    employee.experience +=
        taskSucceeded
            ? randomInt(
                3,
                8
            )
            : 1;

    employee.status =
        taskSucceeded
            ? "completed"
            : "idle";

    if (taskSucceeded) {
        gameState.daily.staffTasks += 1;

        addActivity(
            getLanguage() === "de"
                ? (
                    `${employee.name}: `
                    +
                    `${employee.currentTask}.`
                )
                : getLanguage() === "en"
                    ? (
                        `${employee.name}: `
                        +
                        `${employee.currentTask}.`
                    )
                    : (
                        `${employee.name}: `
                        +
                        `${employee.currentTask}.`
                    ),
            "staff"
        );
    }

    gameState.lastStaffActions[
        employee.id
    ] = {
        time:
            minutesToTime(
                gameState.calendar.minutes
            ),

        date:
            formatGameDate(),

        success:
            taskSucceeded,

        description:
            employee.currentTask
    };

    scheduleEmployeeNextAction(
        employee
    );

    saveGame(false);

    return taskSucceeded;
}


/* =========================================================
   OYUN ZAMANI
========================================================= */

function getAbsoluteGameMinutes() {
    const date =
        Date.UTC(
            gameState.calendar.year,
            gameState.calendar.month,
            gameState.calendar.day
        );

    return (
        Math.floor(
            date / 60000
        )
        +
        Math.floor(
            gameState.calendar.minutes
        )
    );
}


/* =========================================================
   PERSONEL ENERJİ YENİLEME
========================================================= */

function recoverEmployeeEnergy(
    passedGameMinutes
) {
    const managerStrength =
        getManagerStrength();

    const recoveryPerMinute =
        0.030
        +
        managerStrength
        *
        0.035;

    for (
        const employee
        of gameState.staff
    ) {
        if (
            employee.status
            ===
            "working"
        ) {
            continue;
        }

        employee.energy =
            clamp(
                employee.energy
                +
                passedGameMinutes
                *
                recoveryPerMinute,
                0,
                100
            );
    }
}


/* =========================================================
   PERSONEL OTOMASYONUNU KONTROL ETME
========================================================= */

function processStaffAutomation() {
    if (
        !gameState.automationEnabled
        ||
        gameState.paused
    ) {
        return;
    }

    const currentGameMinutes =
        getAbsoluteGameMinutes();

    for (
        const employee
        of gameState.staff
    ) {
        if (
            !employee.nextActionAt
        ) {
            scheduleEmployeeNextAction(
                employee
            );
        }

        if (
            currentGameMinutes
            >=
            employee.nextActionAt
        ) {
            runEmployeeAutomation(
                employee
            );
        }
    }
}


/* =========================================================
   OYUN HIZI
========================================================= */

function setGameSpeed(speed) {
    const acceptedSpeeds = [
        1,
        2,
        4
    ];

    if (
        !acceptedSpeeds.includes(
            speed
        )
    ) {
        return;
    }

    gameState.speed =
        speed;

    gameState.paused =
        false;

    saveGame(false);

    if (
        typeof updateTimeControlButtons
        ===
        "function"
    ) {
        updateTimeControlButtons();
    }
}


function toggleGamePause() {
    gameState.paused =
        !gameState.paused;

    saveGame(false);

    if (
        typeof updateTimeControlButtons
        ===
        "function"
    ) {
        updateTimeControlButtons();
    }
}


/* =========================================================
   ZAMAN SAYACI
========================================================= */

function gameClockTick(timestamp) {
    if (!runtime.lastTick) {
        runtime.lastTick =
            timestamp;

        return;
    }

    const passedRealSeconds =
        Math.min(
            1,
            (
                timestamp
                -
                runtime.lastTick
            )
            /
            1000
        );

    runtime.lastTick =
        timestamp;

    if (
        gameState.paused
        ||
        document.getElementById(
            "game-screen"
        )?.classList.contains(
            "hidden"
        )
    ) {
        return;
    }

    const passedGameMinutes =
        passedRealSeconds
        *
        GAME_MINUTES_PER_REAL_SECOND
        *
        gameState.speed;

    gameState.calendar.minutes +=
        passedGameMinutes;

    recoverEmployeeEnergy(
        passedGameMinutes
    );

    processStaffAutomation();

    processOperationalIncidents();

    runtime.renderLimiter +=
        passedRealSeconds;

    if (
        runtime.renderLimiter
        >=
        0.25
    ) {
        runtime.renderLimiter = 0;

        if (
            typeof updateTopBar
            ===
            "function"
        ) {
            updateTopBar();
        }

        if (
            typeof renderAutomationPanel
            ===
            "function"
        ) {
            renderAutomationPanel();
        }
    }

    if (
        gameState.calendar.minutes
        >=
        DAY_END_MINUTES
    ) {
        gameState.calendar.minutes =
            DAY_END_MINUTES;

        gameState.paused =
            true;

        if (
            !runtime.dayEndTriggered
        ) {
            runtime.dayEndTriggered =
                true;

            if (
                typeof finishWorkingDay
                ===
                "function"
            ) {
                finishWorkingDay(
                    true
                );
            }
        }
    }
}


/* =========================================================
   ZAMAN SAYACINI BAŞLATMA
========================================================= */

function startGameClock() {
    stopGameClock();

    runtime.lastTick =
        performance.now();

    runtime.timer =
        window.setInterval(
            () => {
                gameClockTick(
                    performance.now()
                );
            },
            200
        );
}


function stopGameClock() {
    if (runtime.timer) {
        window.clearInterval(
            runtime.timer
        );

        runtime.timer = null;
    }

    runtime.lastTick = 0;
}


/* =========================================================
   OTOMASYONU AÇIP KAPATMA
========================================================= */

function toggleStaffAutomation() {
    gameState.automationEnabled =
        !gameState.automationEnabled;

    for (
        const employee
        of gameState.staff
    ) {
        if (
            gameState.automationEnabled
            &&
            !employee.nextActionAt
        ) {
            scheduleEmployeeNextAction(
                employee
            );
        }
    }

    saveGame(false);

    if (
        typeof renderAutomationPanel
        ===
        "function"
    ) {
        renderAutomationPanel();
    }
}

/* =========================================================
   PC SHOP EMPIRE
   GAME.JS — BÖLÜM 3
   OYUN İÇİ PENCERELER, YENİ OYUN, KİRA, FİNANS
   GÜN SONU VE RASTGELE OLAYLAR
========================================================= */


/* =========================================================
   EK ÇEVİRİLER
========================================================= */

Object.assign(
    translations.tr,
    {
        information: "Bilgi",
        warning: "Uyarı",
        error: "Hata",
        success: "Başarılı",
        confirmation: "Onay",
        close: "Kapat",
        deleteSave: "Kaydı Sil",
        savedGame: "Kayıtlı Oyun",
        saveDate: "Kayıt Tarihi",
        noSavedProgress: "Henüz kayıtlı ilerleme bulunmuyor.",
        continueSavedGame: "Kayıtlı oyuna devam et",
        settingsDescription:
            "Dil ve oyun kayıt seçeneklerini buradan değiştirebilirsin.",
        currentLanguage: "Mevcut Dil",
        newGameStarted: "Yeni oyun başlatıldı.",
        returnedToMenu: "Ana menüye dönüldü.",
        purchaseCompleted: "Satın alma tamamlandı.",
        productOutOfStock: "Bu ürünün stoğu tükendi.",
        invalidAmount: "Geçersiz ürün miktarı.",
        quickSaleCompleted: "Hızlı satış tamamlandı.",
        moveQuestion:
            "Bu dükkâna taşınmak istediğine emin misin?",
        moveCompleted: "Yeni dükkâna taşınıldı.",
        reputationRequired: "Gerekli itibar",
        deposit: "Depozito",
        staffCapacity: "Personel Kapasitesi",
        storageCapacity: "Depo Kapasitesi",
        contractDiscount: "Sözleşme İndirimi",
        providerChanged: "Hizmet sağlayıcısı değiştirildi.",
        dailyReport: "Gün Sonu Raporu",
        totalIncome: "Toplam Gelir",
        totalExpense: "Toplam Gider",
        netProfit: "Net Kâr",
        soldComputers: "Satılan PC",
        builtComputersCount: "Toplanan PC",
        staffTasksCompleted: "Personel Görevi",
        newCustomers: "Yeni Müşteri",
        expiredCustomers: "Süresi Dolan Müşteri",
        salaries: "Personel Maaşları",
        administration: "İdari Giderler",
        accountingSaving: "Muhasebe Tasarrufu",
        propertyRent: "Dükkân Kirası",
        serviceCosts: "Hizmet Giderleri",
        eventLoss: "Olay Zararı",
        eventIncome: "Olay Geliri",
        bankruptcy: "İflas",
        bankruptcyMessage:
            "Borç sınırı aşıldı. Mağaza faaliyetlerini sürdüremiyor.",
        debtWarning: "Kasa eksi bakiyeye düştü.",
        customerExpired:
            "Bir müşterinin sipariş süresi doldu.",
        powerOutage: "Elektrik Kesintisi",
        powerOutageText:
            "Elektrik kesintisi nedeniyle atölye çalışmaları aksadı.",
        theft: "Hırsızlık",
        theftText:
            "Depoda hırsızlık veya ürün kaybı yaşandı.",
        warrantyReturn: "Garanti İadesi",
        warrantyReturnText:
            "Eski bir müşteri garanti kapsamında para iadesi aldı.",
        socialMedia: "Sosyal Medya Paylaşımı",
        socialMediaText:
            "Memnun bir müşteri mağazanı sosyal medyada paylaştı.",
        corporateDeal: "Kurumsal Anlaşma",
        corporateDealText:
            "Yerel bir işletmeyle küçük bir satış anlaşması yapıldı.",
        maintenanceFailure: "Atölye Arızası",
        maintenanceFailureText:
            "Atölyedeki bir ekipmanın acil bakıma ihtiyacı oldu.",
        supplierRefund: "Tedarikçi İadesi",
        supplierRefundText:
            "Bir tedarikçi fiyat farkı nedeniyle geri ödeme yaptı.",
        taxDay: "Vergi Günü",
        automaticDayEnd:
            "Çalışma saatleri sona erdi. Gün sonu raporu hazırlanıyor.",
        manualDayEnd:
            "İş günü erken kapatılacak ve tüm günlük giderler hesaplanacak.",
        nextDayReady: "Yeni gün hazır.",
        automationOn: "Personel otomasyonu açıldı.",
        automationOff: "Personel otomasyonu kapatıldı."
    }
);


Object.assign(
    translations.en,
    {
        information: "Information",
        warning: "Warning",
        error: "Error",
        success: "Success",
        confirmation: "Confirmation",
        close: "Close",
        deleteSave: "Delete Save",
        savedGame: "Saved Game",
        saveDate: "Save Date",
        noSavedProgress: "There is no saved progress yet.",
        continueSavedGame: "Continue saved game",
        settingsDescription:
            "Change language and save options here.",
        currentLanguage: "Current Language",
        newGameStarted: "A new game has started.",
        returnedToMenu: "Returned to the main menu.",
        purchaseCompleted: "Purchase completed.",
        productOutOfStock: "This product is out of stock.",
        invalidAmount: "Invalid product quantity.",
        quickSaleCompleted: "Quick sale completed.",
        moveQuestion:
            "Are you sure you want to move to this store?",
        moveCompleted: "Moved to the new store.",
        reputationRequired: "Required reputation",
        deposit: "Deposit",
        staffCapacity: "Staff Capacity",
        storageCapacity: "Storage Capacity",
        contractDiscount: "Contract Discount",
        providerChanged: "Service provider changed.",
        dailyReport: "End of Day Report",
        totalIncome: "Total Income",
        totalExpense: "Total Expense",
        netProfit: "Net Profit",
        soldComputers: "Computers Sold",
        builtComputersCount: "Computers Built",
        staffTasksCompleted: "Staff Tasks",
        newCustomers: "New Customers",
        expiredCustomers: "Expired Customers",
        salaries: "Staff Salaries",
        administration: "Administration",
        accountingSaving: "Accounting Savings",
        propertyRent: "Store Rent",
        serviceCosts: "Service Costs",
        eventLoss: "Event Loss",
        eventIncome: "Event Income",
        bankruptcy: "Bankruptcy",
        bankruptcyMessage:
            "The debt limit has been exceeded. The store can no longer operate.",
        debtWarning: "The store balance has fallen below zero.",
        customerExpired:
            "A customer's order deadline expired.",
        powerOutage: "Power Outage",
        powerOutageText:
            "Workshop operations were interrupted by a power outage.",
        theft: "Theft",
        theftText:
            "The store suffered a theft or inventory loss.",
        warrantyReturn: "Warranty Return",
        warrantyReturnText:
            "A former customer received a warranty refund.",
        socialMedia: "Social Media Post",
        socialMediaText:
            "A satisfied customer promoted the store on social media.",
        corporateDeal: "Corporate Deal",
        corporateDealText:
            "A small sales agreement was made with a local business.",
        maintenanceFailure: "Workshop Failure",
        maintenanceFailureText:
            "Workshop equipment required emergency maintenance.",
        supplierRefund: "Supplier Refund",
        supplierRefundText:
            "A supplier refunded a price difference.",
        taxDay: "Tax Day",
        automaticDayEnd:
            "Working hours are over. Preparing the daily report.",
        manualDayEnd:
            "The store will close early and all daily expenses will be charged.",
        nextDayReady: "The next day is ready.",
        automationOn: "Staff automation enabled.",
        automationOff: "Staff automation disabled."
    }
);


Object.assign(
    translations.de,
    {
        information: "Information",
        warning: "Warnung",
        error: "Fehler",
        success: "Erfolgreich",
        confirmation: "Bestätigung",
        close: "Schließen",
        deleteSave: "Spielstand löschen",
        savedGame: "Gespeichertes Spiel",
        saveDate: "Speicherdatum",
        noSavedProgress: "Es ist noch kein Spielstand vorhanden.",
        continueSavedGame: "Gespeichertes Spiel fortsetzen",
        settingsDescription:
            "Hier kannst du Sprache und Speicheroptionen ändern.",
        currentLanguage: "Aktuelle Sprache",
        newGameStarted: "Ein neues Spiel wurde gestartet.",
        returnedToMenu: "Zum Hauptmenü zurückgekehrt.",
        purchaseCompleted: "Einkauf abgeschlossen.",
        productOutOfStock: "Dieses Produkt ist ausverkauft.",
        invalidAmount: "Ungültige Produktmenge.",
        quickSaleCompleted: "Schnellverkauf abgeschlossen.",
        moveQuestion:
            "Möchtest du wirklich in dieses Geschäft umziehen?",
        moveCompleted: "In das neue Geschäft umgezogen.",
        reputationRequired: "Benötigter Ruf",
        deposit: "Kaution",
        staffCapacity: "Personalkapazität",
        storageCapacity: "Lagerkapazität",
        contractDiscount: "Vertragsrabatt",
        providerChanged: "Dienstanbieter geändert.",
        dailyReport: "Tagesbericht",
        totalIncome: "Gesamteinnahmen",
        totalExpense: "Gesamtausgaben",
        netProfit: "Nettogewinn",
        soldComputers: "Verkaufte Computer",
        builtComputersCount: "Montierte Computer",
        staffTasksCompleted: "Personalaufgaben",
        newCustomers: "Neue Kunden",
        expiredCustomers: "Abgelaufene Kunden",
        salaries: "Personalgehälter",
        administration: "Verwaltung",
        accountingSaving: "Buchhaltungsersparnis",
        propertyRent: "Geschäftsmiete",
        serviceCosts: "Dienstleistungskosten",
        eventLoss: "Ereignisverlust",
        eventIncome: "Ereigniseinnahmen",
        bankruptcy: "Insolvenz",
        bankruptcyMessage:
            "Die Schuldengrenze wurde überschritten. Das Geschäft kann nicht weitergeführt werden.",
        debtWarning: "Der Kontostand ist negativ.",
        customerExpired:
            "Die Frist eines Kundenauftrags ist abgelaufen.",
        powerOutage: "Stromausfall",
        powerOutageText:
            "Die Werkstattarbeit wurde durch einen Stromausfall unterbrochen.",
        theft: "Diebstahl",
        theftText:
            "Im Geschäft kam es zu Diebstahl oder Lagerverlust.",
        warrantyReturn: "Garantierückgabe",
        warrantyReturnText:
            "Ein früherer Kunde erhielt eine Rückzahlung.",
        socialMedia: "Social-Media-Beitrag",
        socialMediaText:
            "Ein zufriedener Kunde hat das Geschäft online empfohlen.",
        corporateDeal: "Firmenvertrag",
        corporateDealText:
            "Mit einem lokalen Unternehmen wurde ein kleiner Vertrag abgeschlossen.",
        maintenanceFailure: "Werkstattstörung",
        maintenanceFailureText:
            "Ein Werkstattgerät musste dringend repariert werden.",
        supplierRefund: "Lieferantenrückzahlung",
        supplierRefundText:
            "Ein Lieferant zahlte eine Preisdifferenz zurück.",
        taxDay: "Steuertag",
        automaticDayEnd:
            "Die Arbeitszeit ist beendet. Der Tagesbericht wird vorbereitet.",
        manualDayEnd:
            "Das Geschäft wird früher geschlossen und alle Tageskosten werden berechnet.",
        nextDayReady: "Der neue Tag ist bereit.",
        automationOn: "Personalautomatisierung aktiviert.",
        automationOff: "Personalautomatisierung deaktiviert."
    }
);


/* =========================================================
   GÜVENLİ SAYFA YENİLEME
========================================================= */

function safeRender() {
    if (
        typeof renderCurrentPage
        ===
        "function"
    ) {
        renderCurrentPage();
    }

    if (
        typeof updateTopBar
        ===
        "function"
    ) {
        updateTopBar();
    }

    if (
        typeof renderAutomationPanel
        ===
        "function"
    ) {
        renderAutomationPanel();
    }
}


/* =========================================================
   OYUN İÇİ BİLDİRİM
========================================================= */

function showToast(
    title,
    message,
    type = "info",
    duration = 3600
) {
    const container =
        document.getElementById(
            "toast-container"
        );

    if (!container) {
        return;
    }

    const icons = {
        success: "✓",
        warning: "!",
        error: "×",
        info: "i"
    };

    const toast =
        document.createElement(
            "div"
        );

    toast.className =
        `toast ${type}`;

    toast.innerHTML = `
        <div class="toast-icon">
            ${icons[type] || "i"}
        </div>

        <div class="toast-content">
            <strong>${title}</strong>
            <p>${message}</p>
        </div>
    `;

    container.appendChild(
        toast
    );

    window.setTimeout(
        () => {
            toast.style.opacity = "0";
            toast.style.transform =
                "translateX(22px)";

            window.setTimeout(
                () => {
                    toast.remove();
                },
                230
            );
        },
        duration
    );
}


/* =========================================================
   OYUN İÇİ ÖZEL MODAL PENCERE
========================================================= */

function showGameModal(options = {}) {
    const {
        title = t("confirmation"),
        message = "",
        icon = "?",
        type = "info",
        confirmText = t("yes"),
        cancelText = t("no"),
        showCancel = true,
        extraHtml = ""
    } = options;

    const overlay =
        document.getElementById(
            "game-modal"
        );

    const titleElement =
        document.getElementById(
            "modal-title"
        );

    const messageElement =
        document.getElementById(
            "modal-message"
        );

    const iconElement =
        document.getElementById(
            "modal-icon"
        );

    const extraElement =
        document.getElementById(
            "modal-extra-content"
        );

    const confirmButton =
        document.getElementById(
            "modal-confirm-button"
        );

    const cancelButton =
        document.getElementById(
            "modal-cancel-button"
        );

    if (
        !overlay
        ||
        !titleElement
        ||
        !messageElement
        ||
        !iconElement
        ||
        !confirmButton
        ||
        !cancelButton
    ) {
        return Promise.resolve(
            false
        );
    }

    titleElement.textContent =
        title;

    messageElement.textContent =
        message;

    iconElement.textContent =
        icon;

    iconElement.className =
        `modal-icon ${type}`;

    extraElement.innerHTML =
        extraHtml;

    confirmButton.textContent =
        confirmText;

    cancelButton.textContent =
        cancelText;

    cancelButton.classList.toggle(
        "hidden",
        !showCancel
    );

    overlay.classList.remove(
        "hidden"
    );

    return new Promise(
        resolve => {
            runtime.modalResolve =
                resolve;

            const finish = result => {
                overlay.classList.add(
                    "hidden"
                );

                confirmButton.onclick =
                    null;

                cancelButton.onclick =
                    null;

                runtime.modalResolve =
                    null;

                resolve(result);
            };

            confirmButton.onclick =
                () => finish(true);

            cancelButton.onclick =
                () => finish(false);
        }
    );
}


/* =========================================================
   SADECE BİLGİ GÖSTEREN OYUN İÇİ PENCERE
========================================================= */

async function showInformationModal(
    title,
    message,
    type = "info",
    icon = "i"
) {
    return showGameModal({
        title,
        message,
        type,
        icon,
        confirmText:
            t("close"),
        showCancel:
            false
    });
}


/* =========================================================
   DİLİ DEĞİŞTİRME
========================================================= */

function changeLanguage(language) {
    if (
        !["tr", "en", "de"].includes(
            language
        )
    ) {
        return;
    }

    gameState.language =
        language;

    document.documentElement.lang =
        language;

    document
        .querySelectorAll(
            "[data-i18n]"
        )
        .forEach(
            element => {
                const key =
                    element.dataset.i18n;

                element.textContent =
                    t(key);
            }
        );

    document
        .querySelectorAll(
            ".language-button"
        )
        .forEach(
            button => {
                button.classList.toggle(
                    "active",
                    button.dataset.language
                    ===
                    language
                );
            }
        );

    const description =
        document.getElementById(
            "start-description"
        );

    if (description) {
        const descriptions = {
            tr:
                "Bilgisayar mağazanı kur, sistemler topla, personel yönet ve bir teknoloji imparatorluğu oluştur.",

            en:
                "Build your computer store, assemble systems, manage employees and create a technology empire.",

            de:
                "Baue dein Computergeschäft auf, montiere Systeme, verwalte Mitarbeiter und erschaffe ein Technologieimperium."
        };

        description.textContent =
            descriptions[language];
    }

    saveGame(false);

    safeRender();
}


/* =========================================================
   BAŞLANGIÇ EKRANINI GÖSTERME
========================================================= */

function showStartScreen() {
    stopGameClock();

    const startScreen =
        document.getElementById(
            "start-screen"
        );

    const gameScreen =
        document.getElementById(
            "game-screen"
        );

    startScreen?.classList.remove(
        "hidden"
    );

    gameScreen?.classList.add(
        "hidden"
    );

    const continueButton =
        document.getElementById(
            "continue-button"
        );

    if (continueButton) {
        continueButton.disabled =
            !hasSaveGame();
    }

    changeLanguage(
        gameState.language
    );
}


/* =========================================================
   OYUN EKRANINA GİRME
========================================================= */

function enterGameScreen() {
    const startScreen =
        document.getElementById(
            "start-screen"
        );

    const gameScreen =
        document.getElementById(
            "game-screen"
        );

    startScreen?.classList.add(
        "hidden"
    );

    gameScreen?.classList.remove(
        "hidden"
    );

    normalizeGameState();

    runtime.dayEndTriggered =
        false;

    gameState.paused =
        false;

    changeLanguage(
        gameState.language
    );

    safeRender();

    startGameClock();
}


/* =========================================================
   YENİ OYUN İSTEĞİ
========================================================= */

async function requestNewGame() {
    let accepted = true;

    if (hasSaveGame()) {
        accepted =
            await showGameModal({
                title:
                    t("newGame"),

                message:
                    t("newGameQuestion"),

                icon:
                    "＋",

                type:
                    "warning",

                confirmText:
                    t("yes"),

                cancelText:
                    t("no")
            });
    }

    if (!accepted) {
        return;
    }

    const selectedLanguage =
        gameState.language
        ||
        loadSavedLanguage();

    deleteSaveGame();

    prepareNewGame(
        selectedLanguage
    );

    normalizeGameState();

    enterGameScreen();

    showToast(
        t("success"),
        t("newGameStarted"),
        "success"
    );
}


/* =========================================================
   KAYITLI OYUNA DEVAM ETME
========================================================= */

async function continueGame() {
    if (!loadGame()) {
        await showInformationModal(
            t("warning"),
            t("noSave"),
            "warning",
            "!"
        );

        return;
    }

    normalizeGameState();

    enterGameScreen();
}


/* =========================================================
   ANA MENÜYE DÖNME
========================================================= */

async function requestReturnToMenu() {
    const accepted =
        await showGameModal({
            title:
                t("confirmation"),

            message:
                t("returnMenuQuestion"),

            icon:
                "☰",

            type:
                "info",

            confirmText:
                t("yes"),

            cancelText:
                t("no")
        });

    if (!accepted) {
        return;
    }

    saveGame(false);

    showStartScreen();

    showToast(
        t("information"),
        t("returnedToMenu"),
        "info"
    );
}


/* =========================================================
   KAYIT YUVASI PENCERESİ
========================================================= */

async function showSaveSlotWindow() {
    if (!hasSaveGame()) {
        await showInformationModal(
            t("savedGame"),
            t("noSavedProgress"),
            "warning",
            "▣"
        );

        return;
    }

    let savedState = null;

    try {
        savedState =
            JSON.parse(
                localStorage.getItem(
                    SAVE_KEY
                )
            );
    } catch (error) {
        console.error(error);
    }

    const savedMoney =
        savedState?.money || 0;

    const savedReputation =
        savedState?.reputation || 0;

    const savedCalendar =
        savedState?.calendar;

    let savedDate = "-";

    if (savedCalendar) {
        const date =
            new Date(
                savedCalendar.year,
                savedCalendar.month,
                savedCalendar.day
            );

        savedDate =
            date.toLocaleDateString(
                {
                    tr: "tr-TR",
                    en: "en-GB",
                    de: "de-DE"
                }[
                    gameState.language
                ]
                ||
                "tr-TR"
            );
    }

    const extraHtml = `
        <div class="game-panel panel-padding">
            <div class="detail-row">
                <span>${t("money")}</span>
                <strong>${formatMoney(savedMoney)}</strong>
            </div>

            <div class="detail-row">
                <span>${t("reputation")}</span>
                <strong>${savedReputation}</strong>
            </div>

            <div class="detail-row">
                <span>${t("saveDate")}</span>
                <strong>${savedDate}</strong>
            </div>
        </div>
    `;

    const accepted =
        await showGameModal({
            title:
                t("savedGame"),

            message:
                t("continueSavedGame"),

            icon:
                "▣",

            type:
                "info",

            confirmText:
                t("continueGame"),

            cancelText:
                t("deleteSave"),

            extraHtml
        });

    if (accepted) {
        continueGame();
        return;
    }

    const deleteAccepted =
        await showGameModal({
            title:
                t("deleteSave"),

            message:
                t("deleteSaveQuestion"),

            icon:
                "×",

            type:
                "danger",

            confirmText:
                t("yes"),

            cancelText:
                t("no")
        });

    if (!deleteAccepted) {
        return;
    }

    deleteSaveGame();

    showToast(
        t("success"),
        t("saveDeleted"),
        "success"
    );

    showStartScreen();
}


/* =========================================================
   AYARLAR PENCERESİ
========================================================= */

async function showSettingsWindow() {
    const languageNames = {
        tr: "Türkçe",
        en: "English",
        de: "Deutsch"
    };

    const extraHtml = `
        <div class="game-panel panel-padding">
            <div class="detail-row">
                <span>${t("currentLanguage")}</span>
                <strong>
                    ${languageNames[gameState.language]}
                </strong>
            </div>

            <div class="language-buttons"
                 style="justify-content:center; margin-top:14px;">
                <button
                    type="button"
                    class="language-button settings-language"
                    data-settings-language="tr"
                >
                    TR
                </button>

                <button
                    type="button"
                    class="language-button settings-language"
                    data-settings-language="en"
                >
                    EN
                </button>

                <button
                    type="button"
                    class="language-button settings-language"
                    data-settings-language="de"
                >
                    DE
                </button>
            </div>
        </div>
    `;

    const modalPromise =
        showGameModal({
            title:
                t("settings"),

            message:
                t("settingsDescription"),

            icon:
                "⚙",

            type:
                "info",

            confirmText:
                t("close"),

            showCancel:
                false,

            extraHtml
        });

    window.setTimeout(
        () => {
            document
                .querySelectorAll(
                    ".settings-language"
                )
                .forEach(
                    button => {
                        button.classList.toggle(
                            "active",
                            button.dataset.settingsLanguage
                            ===
                            gameState.language
                        );

                        button.addEventListener(
                            "click",
                            () => {
                                changeLanguage(
                                    button.dataset.settingsLanguage
                                );

                                document
                                    .querySelectorAll(
                                        ".settings-language"
                                    )
                                    .forEach(
                                        item => {
                                            item.classList.toggle(
                                                "active",
                                                item
                                                    .dataset
                                                    .settingsLanguage
                                                ===
                                                gameState.language
                                            );
                                        }
                                    );
                            }
                        );
                    }
                );
        },
        30
    );

    await modalPromise;
}


/* =========================================================
   PAZARDAN ÜRÜN SATIN ALMA
========================================================= */

function purchaseMarketOffer(
    offerId,
    amount = 1
) {
    const offer =
        getOfferById(
            offerId
        );

    const normalizedAmount =
        Math.floor(
            Number(amount)
        );

    if (
        !offer
        ||
        normalizedAmount <= 0
    ) {
        return {
            success: false,
            message:
                t("invalidAmount")
        };
    }

    if (
        offer.stock
        <
        normalizedAmount
    ) {
        return {
            success: false,
            message:
                t("productOutOfStock")
        };
    }

    if (
        getInventoryCount()
        +
        normalizedAmount
        >
        getStorageCapacity()
    ) {
        return {
            success: false,
            message:
                t("insufficientStorage")
        };
    }

    const totalPrice =
        offer.price
        *
        normalizedAmount;

    if (
        gameState.money
        <
        totalPrice
    ) {
        return {
            success: false,
            message:
                t("insufficientMoney")
        };
    }

    registerExpense(
        totalPrice
    );

    addInventory(
        offer.partId,
        normalizedAmount,
        offer.price
    );

    offer.stock -=
        normalizedAmount;

    gameState.daily.partsPurchased +=
        normalizedAmount;

    gameState.lifetime.partsPurchased +=
        normalizedAmount;

    const part =
        getPartById(
            offer.partId
        );

    addActivity(
        getLanguage() === "de"
            ? (
                `${normalizedAmount} × ${part.name} `
                +
                `für ${formatMoney(totalPrice)} gekauft.`
            )
            : getLanguage() === "en"
                ? (
                    `Purchased ${normalizedAmount} × ${part.name} `
                    +
                    `for ${formatMoney(totalPrice)}.`
                )
                : (
                    `${normalizedAmount} adet ${part.name}, `
                    +
                    `${formatMoney(totalPrice)} karşılığında alındı.`
                ),
        "purchase"
    );

    saveGame(false);

    safeRender();

    return {
        success: true,
        totalPrice,
        part
    };
}


/* =========================================================
   HAZIR BİLGİSAYARI HIZLI SATMA
========================================================= */

function quickSellComputer(
    computerId
) {
    const computer =
        getBuiltPcById(
            computerId
        );

    if (!computer) {
        return {
            success: false,
            message:
                t("selectPc")
        };
    }

    const salePrice =
        Math.round(
            computer.value
            *
            randomFloat(
                0.72,
                0.86
            )
        );

    registerRevenue(
        salePrice
    );

    gameState.builtComputers =
        gameState.builtComputers.filter(
            item =>
                item.id
                !==
                computerId
        );

    gameState.reputation =
        Math.max(
            0,
            gameState.reputation - 1
        );

    gameState.daily.computersSold += 1;
    gameState.lifetime.computersSold += 1;

    addActivity(
        getLanguage() === "de"
            ? (
                `${computer.id} wurde schnell für `
                +
                `${formatMoney(salePrice)} verkauft. Ruf -1.`
            )
            : getLanguage() === "en"
                ? (
                    `${computer.id} was quick-sold for `
                    +
                    `${formatMoney(salePrice)}. Reputation -1.`
                )
                : (
                    `${computer.id}, ${formatMoney(salePrice)} `
                    +
                    `karşılığında hızlı satıldı. İtibar -1.`
                ),
        "sale"
    );

    saveGame(false);

    safeRender();

    return {
        success: true,
        salePrice
    };
}


/* =========================================================
   KİRA SÖZLEŞMESİ İNDİRİMLERİ
========================================================= */

function getContractDiscount(
    contractType =
        gameState.contractType
) {
    const discounts = {
        monthly: 0,
        sixMonths: 0.07,
        yearly: 0.15
    };

    return (
        discounts[contractType]
        ||
        0
    );
}


/* =========================================================
   GÜNLÜK KİRA HESAPLAMA
========================================================= */

function calculateDailyRent() {
    const property =
        getPropertyById(
            gameState.propertyId
        );

    const contractDiscount =
        getContractDiscount();

    return Math.round(
        property.rent
        /
        30
        *
        (
            1
            -
            contractDiscount
        )
    );
}


/* =========================================================
   DÜKKÂN DEĞİŞTİRME
========================================================= */

async function requestPropertyMove(
    propertyId,
    contractType = "monthly"
) {
    const property =
        getPropertyById(
            propertyId
        );

    if (!property) {
        return;
    }

    if (
        gameState.reputation
        <
        property.requiredReputation
    ) {
        await showInformationModal(
            t("warning"),
            (
                `${t("reputationRequired")}: `
                +
                `${property.requiredReputation}`
            ),
            "warning",
            "!"
        );

        return;
    }

    if (
        gameState.staff.length
        >
        property.staffCapacity
    ) {
        await showInformationModal(
            t("warning"),
            getLanguage() === "de"
                ? "Dieses Geschäft hat nicht genügend Platz für deine Mitarbeiter."
                : getLanguage() === "en"
                    ? "This store does not have enough capacity for your employees."
                    : "Bu dükkân mevcut personelin için yeterli kapasiteye sahip değil.",
            "warning",
            "!"
        );

        return;
    }

    const movingCost =
        property.deposit;

    const accepted =
        await showGameModal({
            title:
                t("property"),

            message:
                (
                    `${t(property.nameKey)}\n\n`
                    +
                    `${t("deposit")}: `
                    +
                    `${formatMoney(movingCost)}\n`
                    +
                    `${t("rent")}: `
                    +
                    `${formatMoney(property.rent)} / 30 gün\n`
                    +
                    `${t("contractDiscount")}: `
                    +
                    `%${Math.round(
                        getContractDiscount(
                            contractType
                        )
                        *
                        100
                    )}\n\n`
                    +
                    `${t("moveQuestion")}`
                ),

            icon:
                "▰",

            type:
                "info",

            confirmText:
                t("yes"),

            cancelText:
                t("no")
        });

    if (!accepted) {
        return;
    }

    if (
        gameState.money
        <
        movingCost
    ) {
        await showInformationModal(
            t("error"),
            t("insufficientMoney"),
            "danger",
            "×"
        );

        return;
    }

    registerExpense(
        movingCost
    );

    gameState.propertyId =
        propertyId;

    gameState.contractType =
        contractType;

    addActivity(
        getLanguage() === "de"
            ? (
                `Umzug in ${t(property.nameKey)}. `
                +
                `Kaution ${formatMoney(movingCost)}.`
            )
            : getLanguage() === "en"
                ? (
                    `Moved to ${t(property.nameKey)}. `
                    +
                    `Deposit ${formatMoney(movingCost)}.`
                )
                : (
                    `${t(property.nameKey)} dükkânına taşınıldı. `
                    +
                    `Depozito ${formatMoney(movingCost)}.`
                ),
        "store"
    );

    saveGame(false);

    safeRender();

    showToast(
        t("success"),
        t("moveCompleted"),
        "success"
    );
}


/* =========================================================
   HİZMET SAĞLAYICISI DEĞİŞTİRME
========================================================= */

function changeProvider(
    providerType,
    providerId
) {
    const provider =
        getProviderById(
            providerType,
            providerId
        );

    if (!provider) {
        return false;
    }

    gameState.providers[
        providerType
    ] = providerId;

    addActivity(
        getLanguage() === "de"
            ? (
                `${providerType}: Anbieter `
                +
                `${provider.name} ausgewählt.`
            )
            : getLanguage() === "en"
                ? (
                    `${providerType}: ${provider.name} selected.`
                )
                : (
                    `${providerType}: ${provider.name} seçildi.`
                ),
        "finance"
    );

    saveGame(false);

    safeRender();

    showToast(
        t("success"),
        t("providerChanged"),
        "success"
    );

    return true;
}


/* =========================================================
   GÜNLÜK MAAŞLAR
========================================================= */

function calculateDailySalaries() {
    return gameState.staff.reduce(
        (
            total,
            employee
        ) =>
            total
            +
            Number(
                employee.salary || 0
            ),
        0
    );
}


/* =========================================================
   GÜNLÜK HİZMET GİDERLERİ
========================================================= */

function calculateDailyServiceCosts() {
    return calculateDailyServiceBreakdown().total;
}


function calculateDailyServiceBreakdown() {
    const electricity =
        getProviderById(
            "electricity",
            gameState.providers.electricity
        );

    const internet =
        getProviderById(
            "internet",
            gameState.providers.internet
        );

    const insurance =
        getProviderById(
            "insurance",
            gameState.providers.insurance
        );

    const electricityCost =
        electricity.dailyCost
        +
        electricity.buildCost
        *
        gameState.daily.computersBuilt;

    const property = getPropertyById(gameState.propertyId);
    const waterAndCleaning = Math.round(
        18 + property.size * 0.08 + gameState.customers.length * 1.5
    );

    return {
        electricity: Math.round(electricityCost),
        internet: Math.round(internet.dailyCost),
        insurance: Math.round(insurance.dailyCost),
        waterAndCleaning,
        total: Math.round(
            electricityCost
            + internet.dailyCost
            + insurance.dailyCost
            + waterAndCleaning
        )
    };
}


/* =========================================================
   GÜNLÜK BAKIM GİDERİ
========================================================= */

function calculateDailyMaintenance() {
    return calculateDailyMaintenanceBreakdown().total;
}


function calculateDailyMaintenanceBreakdown() {
    const property =
        getPropertyById(
            gameState.propertyId
        );

    const facility = Math.round(20 + property.size * 0.18);
    const workshopWear = Math.round(gameState.daily.computersBuilt * 13);
    const equipmentDepreciation = Math.round(
        12 + gameState.upgrades.workshop * 20 + gameState.staff.length * 3
    );

    return {
        facility,
        workshopWear,
        equipmentDepreciation,
        total: facility + workshopWear + equipmentDepreciation
    };
}


/* =========================================================
   İDARİ GİDERLER
========================================================= */

function calculateAdministrativeCost() {
    return calculateAdministrativeBreakdown().total;
}


function calculateAdministrativeBreakdown() {
    const property =
        getPropertyById(
            gameState.propertyId
        );

    const permitsAndSoftware = Math.round(25 + gameState.level * 4);
    const paymentFees = Math.round(gameState.daily.revenue * 0.012);
    const officeSupplies = Math.round(12 + property.size * 0.09);

    return {
        permitsAndSoftware,
        paymentFees,
        officeSupplies,
        total: permitsAndSoftware + paymentFees + officeSupplies
    };
}


/* =========================================================
   VERGİ HESAPLAMA
========================================================= */

function calculateTax() {
    if (
        gameState.lifetime.daysCompleted
        %
        7
        !==
        6
    ) {
        return 0;
    }

    const profit =
        Math.max(
            0,
            gameState.daily.revenue
            -
            gameState.daily.expenses
        );

    const accountantDiscount =
        getAccountantDiscount();

    const taxRate =
        Math.max(
            0.08,
            0.19
            -
            accountantDiscount
            *
            0.20
        );

    return Math.round(
        profit
        *
        taxRate
    );
}


/* =========================================================
   GÜNLÜK GİDER RAPORU
========================================================= */

function calculateDailyOperatingExpenses() {
    const rent =
        calculateDailyRent();

    const salaries =
        calculateDailySalaries();

    const services =
        calculateDailyServiceCosts();

    const maintenance =
        calculateDailyMaintenance();

    const administration =
        calculateAdministrativeCost();

    const tax =
        calculateTax();

    const loanPayment =
        calculateEmergencyLoanPayment();

    const rawTotal =
        rent
        +
        salaries
        +
        services
        +
        maintenance
        +
        administration
        +
        tax
        +
        loanPayment;

    const accountantDiscount =
        getAccountantDiscount();

    const staffSaving =
        gameState.daily.accountingSavings
        ||
        0;

    const percentageSaving =
        Math.round(
            rawTotal
            *
            accountantDiscount
        );

    const totalSaving =
        Math.min(
            rawTotal,
            percentageSaving
            +
            staffSaving
        );

    const finalTotal =
        Math.max(
            0,
            rawTotal
            -
            totalSaving
        );

    return {
        rent,
        salaries,
        services,
        maintenance,
        administration,
        tax,
        loanPayment,
        breakdown: {
            services: calculateDailyServiceBreakdown(),
            maintenance: calculateDailyMaintenanceBreakdown(),
            administration: calculateAdministrativeBreakdown()
        },
        rawTotal,
        saving:
            totalSaving,
        total:
            finalTotal
    };
}


/* =========================================================
   SÜRESİ DOLAN MÜŞTERİLER
========================================================= */

function processCustomerDeadlines() {
    let expiredCount = 0;

    for (
        const customer
        of gameState.customers
    ) {
        customer.deadlineDays -= 1;
    }

    const remainingCustomers = [];

    for (
        const customer
        of gameState.customers
    ) {
        if (
            customer.deadlineDays
            <=
            0
        ) {
            expiredCount += 1;

            gameState.reputation =
                Math.max(
                    0,
                    gameState.reputation - 2
                );

            addActivity(
                getLanguage() === "de"
                    ? (
                        `Auftrag von ${customer.name} ist abgelaufen. Ruf -2.`
                    )
                    : getLanguage() === "en"
                        ? (
                            `${customer.name}'s order expired. Reputation -2.`
                        )
                        : (
                            `${customer.name} müşterisinin süresi doldu. İtibar -2.`
                        ),
                "customer"
            );
        } else {
            remainingCustomers.push(
                customer
            );
        }
    }

    gameState.customers =
        remainingCustomers;

    return expiredCount;
}


/* =========================================================
   RASTGELE GÜN SONU OLAYI
========================================================= */

function processRandomDailyEvent() {
    if (
        !chance(0.42)
    ) {
        return null;
    }

    const electricity =
        getProviderById(
            "electricity",
            gameState.providers.electricity
        );

    const insurance =
        getProviderById(
            "insurance",
            gameState.providers.insurance
        );

    const securityProtection =
        clamp(
            gameState.upgrades.security
            *
            0.09
            +
            insurance.theftProtection,
            0,
            0.90
        );

    const warrantyProtection =
        clamp(
            insurance.warrantyProtection
            +
            gameState.reputation
            /
            1000,
            0,
            0.85
        );

    const events = [
        {
            id: "power",
            weight:
                electricity.outageRisk
                *
                12,

            titleKey:
                "powerOutage",

            textKey:
                "powerOutageText",

            money:
                -randomInt(
                    90,
                    430
                ),

            reputation:
                0
        },
        {
            id: "theft",
            weight:
                Math.max(
                    0.3,
                    3.2
                    *
                    (
                        1
                        -
                        securityProtection
                    )
                ),

            titleKey:
                "theft",

            textKey:
                "theftText",

            money:
                -randomInt(
                    180,
                    950
                ),

            reputation:
                -1
        },
        {
            id: "warranty",
            weight:
                Math.max(
                    0.4,
                    2.5
                    *
                    (
                        1
                        -
                        warrantyProtection
                    )
                ),

            titleKey:
                "warrantyReturn",

            textKey:
                "warrantyReturnText",

            money:
                -randomInt(
                    120,
                    620
                ),

            reputation:
                -1
        },
        {
            id: "social",
            weight: 2.8,

            titleKey:
                "socialMedia",

            textKey:
                "socialMediaText",

            money:
                randomInt(
                    60,
                    240
                ),

            reputation:
                randomInt(
                    1,
                    3
                )
        },
        {
            id: "corporate",
            weight: 1.8,

            titleKey:
                "corporateDeal",

            textKey:
                "corporateDealText",

            money:
                randomInt(
                    280,
                    1100
                ),

            reputation:
                2
        },
        {
            id: "maintenance",
            weight: 2.4,

            titleKey:
                "maintenanceFailure",

            textKey:
                "maintenanceFailureText",

            money:
                -randomInt(
                    100,
                    520
                ),

            reputation:
                0
        },
        {
            id: "refund",
            weight: 1.7,

            titleKey:
                "supplierRefund",

            textKey:
                "supplierRefundText",

            money:
                randomInt(
                    90,
                    470
                ),

            reputation:
                0
        }
    ];

    const totalWeight =
        events.reduce(
            (
                total,
                event
            ) =>
                total
                +
                event.weight,
            0
        );

    let roll =
        Math.random()
        *
        totalWeight;

    let selectedEvent =
        events[0];

    for (
        const event
        of events
    ) {
        roll -=
            event.weight;

        if (
            roll
            <=
            0
        ) {
            selectedEvent =
                event;

            break;
        }
    }

    let finalMoney =
        selectedEvent.money;

    if (
        finalMoney < 0
        &&
        selectedEvent.id === "theft"
    ) {
        finalMoney =
            Math.round(
                finalMoney
                *
                (
                    1
                    -
                    securityProtection
                )
            );
    }

    if (
        finalMoney < 0
        &&
        selectedEvent.id === "warranty"
    ) {
        finalMoney =
            Math.round(
                finalMoney
                *
                (
                    1
                    -
                    warrantyProtection
                )
            );
    }

    if (
        finalMoney > 0
    ) {
        registerRevenue(
            finalMoney
        );
    } else if (
        finalMoney < 0
    ) {
        registerExpense(
            Math.abs(
                finalMoney
            )
        );
    }

    gameState.reputation =
        Math.max(
            0,
            gameState.reputation
            +
            selectedEvent.reputation
        );

    addActivity(
        (
            `${t(selectedEvent.titleKey)}: `
            +
            `${t(selectedEvent.textKey)} `
            +
            `${formatMoney(finalMoney)}`
        ),
        "event"
    );

    return {
        title:
            t(
                selectedEvent.titleKey
            ),

        text:
            t(
                selectedEvent.textKey
            ),

        money:
            finalMoney,

        reputation:
            selectedEvent.reputation
    };
}


/* =========================================================
   PERSONEL ENERJİSİNİ YENİ GÜNE HAZIRLAMA
========================================================= */

function prepareStaffForNextDay() {
    for (
        const employee
        of gameState.staff
    ) {
        const managerBonus =
            Math.round(
                getManagerStrength()
                *
                35
            );

        employee.energy =
            clamp(
                72
                +
                randomInt(
                    12,
                    25
                )
                +
                managerBonus,
                0,
                100
            );

        employee.status =
            "idle";

        employee.currentTask =
            getLanguage() === "de"
                ? "Bereit für den Arbeitstag"
                : getLanguage() === "en"
                    ? "Ready for the working day"
                    : "İş gününe hazır";

        scheduleEmployeeNextAction(
            employee
        );
    }
}


/* =========================================================
   GÜN SONU RAPORUNU GÖSTERME
========================================================= */

function displayDayReport(
    report
) {
    const overlay =
        document.getElementById(
            "day-report-overlay"
        );

    const dateElement =
        document.getElementById(
            "report-date"
        );

    const statisticsElement =
        document.getElementById(
            "report-statistics"
        );

    const eventsElement =
        document.getElementById(
            "report-events"
        );

    if (
        !overlay
        ||
        !statisticsElement
        ||
        !eventsElement
    ) {
        return;
    }

    dateElement.textContent =
        report.date;

    statisticsElement.innerHTML = `
        <div class="report-stat">
            <span>${t("totalIncome")}</span>
            <strong class="text-success">
                ${formatMoney(report.revenue)}
            </strong>
        </div>

        <div class="report-stat">
            <span>${t("totalExpense")}</span>
            <strong class="text-danger">
                ${formatMoney(report.expenses)}
            </strong>
        </div>

        <div class="report-stat">
            <span>${t("netProfit")}</span>
            <strong class="${
                report.net >= 0
                    ? "text-success"
                    : "text-danger"
            }">
                ${formatMoney(report.net)}
            </strong>
        </div>

        <div class="report-stat">
            <span>${t("staffTasksCompleted")}</span>
            <strong>
                ${report.staffTasks}
            </strong>
        </div>
    `;

    const eventRows = [
        `
            <div class="report-event">
                ${t("propertyRent")}:
                <strong>${formatMoney(report.operating.rent)}</strong>
            </div>
        `,
        `
            <div class="report-event">
                ${t("salaries")}:
                <strong>${formatMoney(report.operating.salaries)}</strong>
            </div>
        `,
        `
            <div class="report-event">
                ${t("serviceCosts")}:
                <strong>${formatMoney(report.operating.services)}</strong>
            </div>
        `,
        `
            <div class="report-event">
                ${t("maintenance")}:
                <strong>${formatMoney(report.operating.maintenance)}</strong>
            </div>
        `,
        `
            <div class="report-event">
                ${t("accountingSaving")}:
                <strong class="text-success">
                    ${formatMoney(report.operating.saving)}
                </strong>
            </div>
        `,
        `
            <div class="report-event">
                ${t("expiredCustomers")}:
                <strong>${report.expiredCustomers}</strong>
            </div>
        `
    ];

    if (
        report.operating.tax
        >
        0
    ) {
        eventRows.push(
            `
                <div class="report-event">
                    ${t("taxDay")}:
                    <strong>${formatMoney(report.operating.tax)}</strong>
                </div>
            `
        );
    }

    if (report.event) {
        eventRows.push(
            `
                <div class="report-event">
                    <strong>${report.event.title}</strong><br>
                    ${report.event.text}<br>
                    ${
                        report.event.money >= 0
                            ? "+"
                            : ""
                    }${formatMoney(report.event.money)}
                </div>
            `
        );
    }

    eventsElement.innerHTML =
        eventRows.join("");

    overlay.classList.remove(
        "hidden"
    );
}


/* =========================================================
   İŞ GÜNÜNÜ BİTİRME
========================================================= */

function finishWorkingDay(
    automatic = false
) {
    if (
        runtime.dayReportOpen
    ) {
        return;
    }

    runtime.dayReportOpen =
        true;

    gameState.paused =
        true;

    const revenueBefore =
        gameState.daily.revenue;

    const expensesBefore =
        gameState.daily.expenses;

    const operating =
        calculateDailyOperatingExpenses();

    registerExpense(
        operating.total
    );

    applyEmergencyLoanPayment(
        operating.loanPayment
    );

    const expiredCustomers =
        processCustomerDeadlines();

    const event =
        processRandomDailyEvent();

    const finalRevenue =
        gameState.daily.revenue;

    const finalExpenses =
        gameState.daily.expenses;

    const report = {
        date:
            formatGameDate(),

        revenue:
            finalRevenue,

        expenses:
            finalExpenses,

        net:
            finalRevenue
            -
            finalExpenses,

        staffTasks:
            gameState.daily.staffTasks,

        computersBuilt:
            gameState.daily.computersBuilt,

        computersSold:
            gameState.daily.computersSold,

        expiredCustomers,

        operating,

        event,

        automatic,

        revenueBefore,
        expensesBefore
    };

    runtime.lastDayReport =
        report;

    gameState.lifetime.daysCompleted +=
        1;

    addActivity(
        getLanguage() === "de"
            ? (
                `Arbeitstag beendet. Ergebnis: `
                +
                `${formatMoney(report.net)}.`
            )
            : getLanguage() === "en"
                ? (
                    `Working day completed. Result: `
                    +
                    `${formatMoney(report.net)}.`
                )
                : (
                    `İş günü tamamlandı. Sonuç: `
                    +
                    `${formatMoney(report.net)}.`
                ),
        "day"
    );

    saveGame(false);

    displayDayReport(
        report
    );
}


/* =========================================================
   GÜNÜ BİTİRME ONAYI
========================================================= */

async function requestEndDay() {
    if (
        runtime.dayReportOpen
    ) {
        return;
    }

    const accepted =
        await showGameModal({
            title:
                t("endDay"),

            message:
                (
                    `${t("endDayQuestion")}\n\n`
                    +
                    `${t("manualDayEnd")}`
                ),

            icon:
                "☾",

            type:
                "warning",

            confirmText:
                t("yes"),

            cancelText:
                t("no")
        });

    if (!accepted) {
        return;
    }

    finishWorkingDay(
        false
    );
}


/* =========================================================
   YENİ GÜNE BAŞLAMA
========================================================= */

function startNextDay() {
    const overlay =
        document.getElementById(
            "day-report-overlay"
        );

    overlay?.classList.add(
        "hidden"
    );

    addCalendarDay();

    gameState.calendar.minutes =
        DAY_START_MINUTES;

    gameState.daily = {
        revenue: 0,
        expenses: 0,
        computersBuilt: 0,
        computersSold: 0,
        partsPurchased: 0,
        staffTasks: 0,
        accountingSavings: 0
    };

    prepareStaffForNextDay();

    refreshMarket();

    const property =
        getPropertyById(
            gameState.propertyId
        );

    const salesEmployees =
        gameState.staff.filter(
            employee =>
                employee.role
                ===
                "sales"
        );

    const averageSalesQuality =
        salesEmployees.length
        >
        0
            ? (
                salesEmployees.reduce(
                    (
                        total,
                        employee
                    ) =>
                        total
                        +
                        employee.quality,
                    0
                )
                /
                salesEmployees.length
            )
            : 0;

    const newCustomerCount =
        2
        +
        property.customerBonus
        +
        gameState.upgrades.marketing
        +
        Math.floor(
            averageSalesQuality
            /
            35
        );

    generateCustomers(
        newCustomerCount
    );

    runtime.dayReportOpen =
        false;

    runtime.dayEndTriggered =
        false;

    runtime.lastDayReport =
        null;

    gameState.paused =
        false;

    addActivity(
        t("newDayStarted"),
        "day",
        "09:00"
    );

    saveGame(false);

    safeRender();

    showToast(
        t("success"),
        t("nextDayReady"),
        "success"
    );
}


/* =========================================================
   İFLAS VE BORÇ KONTROLÜ
========================================================= */

async function checkFinancialFailure() {
    if (
        gameState.money
        <
        -7500
    ) {
        await showInformationModal(
            t("bankruptcy"),
            t("bankruptcyMessage"),
            "danger",
            "×"
        );

        deleteSaveGame();

        showStartScreen();

        return true;
    }

    if (
        gameState.money
        <
        0
    ) {
        showToast(
            t("warning"),
            t("debtWarning"),
            "warning"
        );
    }

    return false;
}

/* =========================================================
   PC SHOP EMPIRE
   GAME.JS — BÖLÜM 4
   ANA ARAYÜZ, PAZAR, ENVANTER, MONTAJ VE MÜŞTERİLER
========================================================= */


/* =========================================================
   EK ÇEVİRİLER
========================================================= */

Object.assign(
    translations.tr,
    {
        dashboardDescription:
            "Mağazanın finansal durumunu, personel faaliyetlerini ve günlük ilerlemeyi takip et.",
        marketDescription:
            "Farklı marka, model, fiyat ve ürün durumlarından oluşan dinamik pazarı incele.",
        inventoryDescription:
            "Satın alınan parçaları ve satışa hazır bilgisayarları yönet.",
        workshopDescription:
            "Uyumlu parçaları seçerek yeni bilgisayar sistemleri oluştur.",
        customersDescription:
            "Müşterilerin ihtiyaçlarını karşılayan bilgisayarları teslim et.",
        selectProduct: "Bir ürün seç",
        selectedProduct: "Seçili Ürün",
        noProductSelected:
            "Özelliklerini görmek için pazardan bir ürün seç.",
        marketSearchPlaceholder:
            "Marka veya model ara...",
        owned: "Sahip olunan",
        buy: "Satın Al",
        parts: "Parçalar",
        computers: "Bilgisayarlar",
        emptyBuiltComputers:
            "Henüz monte edilmiş bir bilgisayar bulunmuyor.",
        computerId: "Bilgisayar",
        powerUsage: "Güç Tüketimi",
        estimatedValue: "Tahmini Değer",
        buildCost: "Montaj Maliyeti",
        buildTime: "Montaj Zamanı",
        selectedComponents: "Seçilen Bileşenler",
        checkCompatibility: "Uyumluluğu Kontrol Et",
        compatibilitySuccess:
            "Seçilen bütün parçalar birbiriyle uyumlu.",
        compatibilityErrors:
            "Uyumluluk hataları bulundu.",
        estimatedPerformance: "Tahmini Performans",
        estimatedSaleValue: "Tahmini Satış Değeri",
        componentMissing: "Bileşen seçilmedi",
        deliveryComputer: "Teslim Edilecek Bilgisayar",
        customerRequirementsMet:
            "Müşterinin bütün zorunlu gereksinimleri karşılanıyor.",
        customerRequirementsNotMet:
            "Bilgisayar müşterinin bazı zorunlu gereksinimlerini karşılamıyor.",
        minimumAccepted: "En düşük kabul edilen",
        orderResult: "Sipariş Sonucu",
        profit: "Kâr",
        loss: "Zarar",
        currentTask: "Mevcut Görev",
        resting: "Dinleniyor",
        working: "Çalışıyor",
        completed: "Görev tamamlandı",
        idle: "Bekliyor",
        storageUsage: "Depo Kullanımı",
        customerTraffic: "Müşteri Yoğunluğu",
        workshopEfficiency: "Atölye Verimliliği",
        automationPerformance: "Otomasyon Performansı",
        openOrders: "Açık Siparişler",
        todaySummary: "Bugünün Özeti",
        noActivity: "Henüz faaliyet kaydı bulunmuyor.",
        manualSale: "Manuel Satış",
        automaticSale: "Otomatik Satış",
        manualBuild: "Manuel Montaj",
        automaticBuild: "Otomatik Montaj",
        selectCustomer: "Bir müşteri seç",
        noSuitableComputer:
            "Bu müşteri için uygun hazır bilgisayar bulunmuyor.",
        customerPaymentRange:
            "Ödeme, bilgisayarın müşteriye uygunluğuna göre değişebilir.",
        allBrands: "Tüm Markalar",
        allConditions: "Tüm Durumlar",
        sortBy: "Sıralama",
        cheapest: "En Ucuz",
        mostExpensive: "En Pahalı",
        highestScore: "En Yüksek Puan",
        bestDiscount: "En İyi İndirim",
        marketResult: "ürün bulundu",
        viewDetails: "Ayrıntıları Gör",
        selectedPcDetails: "Seçili Bilgisayar",
        partsUsed: "Kullanılan Parçalar",
        quickSaleWarning:
            "Hızlı satış normal müşteri satışından daha düşük gelir sağlar ve 1 itibar kaybettirir.",
        quickSellQuestion:
            "Seçili bilgisayarı hızlı satışla satmak istediğine emin misin?",
        buildQuestion:
            "Seçilen parçalar kullanılarak bilgisayar monte edilsin mi?",
        deliverQuestion:
            "Seçilen bilgisayar bu müşteriye teslim edilsin mi?",
        unitPrice: "Birim Fiyat",
        totalCostLabel: "Toplam Maliyet",
        available: "Mevcut",
        unavailable: "Mevcut değil",
        staffWorkingNow: "Şu anda çalışan personel",
        automationDescription:
            "Personel, oyun saati ilerledikçe yeteneklerine ve görevlerine göre otomatik çalışır."
    }
);


Object.assign(
    translations.en,
    {
        dashboardDescription:
            "Track your store's finances, employee activity and daily progress.",
        marketDescription:
            "Browse a dynamic market containing different brands, models, prices and conditions.",
        inventoryDescription:
            "Manage purchased components and computers ready for sale.",
        workshopDescription:
            "Build new computer systems by selecting compatible components.",
        customersDescription:
            "Deliver computers that meet each customer's requirements.",
        selectProduct: "Select a product",
        selectedProduct: "Selected Product",
        noProductSelected:
            "Select a product from the market to view its specifications.",
        marketSearchPlaceholder:
            "Search brand or model...",
        owned: "Owned",
        buy: "Purchase",
        parts: "Components",
        computers: "Computers",
        emptyBuiltComputers:
            "There are no assembled computers yet.",
        computerId: "Computer",
        powerUsage: "Power Usage",
        estimatedValue: "Estimated Value",
        buildCost: "Build Cost",
        buildTime: "Build Time",
        selectedComponents: "Selected Components",
        checkCompatibility: "Check Compatibility",
        compatibilitySuccess:
            "All selected components are compatible.",
        compatibilityErrors:
            "Compatibility errors were found.",
        estimatedPerformance: "Estimated Performance",
        estimatedSaleValue: "Estimated Sale Value",
        componentMissing: "No component selected",
        deliveryComputer: "Computer to Deliver",
        customerRequirementsMet:
            "All mandatory customer requirements are met.",
        customerRequirementsNotMet:
            "The computer does not meet some mandatory customer requirements.",
        minimumAccepted: "Minimum accepted",
        orderResult: "Order Result",
        profit: "Profit",
        loss: "Loss",
        currentTask: "Current Task",
        resting: "Resting",
        working: "Working",
        completed: "Task completed",
        idle: "Waiting",
        storageUsage: "Storage Usage",
        customerTraffic: "Customer Traffic",
        workshopEfficiency: "Workshop Efficiency",
        automationPerformance: "Automation Performance",
        openOrders: "Open Orders",
        todaySummary: "Today's Summary",
        noActivity: "There is no activity yet.",
        manualSale: "Manual Sale",
        automaticSale: "Automatic Sale",
        manualBuild: "Manual Assembly",
        automaticBuild: "Automatic Assembly",
        selectCustomer: "Select a customer",
        noSuitableComputer:
            "There is no suitable computer for this customer.",
        customerPaymentRange:
            "Payment may change depending on how well the computer matches the customer.",
        allBrands: "All Brands",
        allConditions: "All Conditions",
        sortBy: "Sort",
        cheapest: "Cheapest",
        mostExpensive: "Most Expensive",
        highestScore: "Highest Score",
        bestDiscount: "Best Discount",
        marketResult: "products found",
        viewDetails: "View Details",
        selectedPcDetails: "Selected Computer",
        partsUsed: "Components Used",
        quickSaleWarning:
            "Quick selling provides less revenue than a customer sale and costs 1 reputation.",
        quickSellQuestion:
            "Are you sure you want to quick-sell the selected computer?",
        buildQuestion:
            "Assemble a computer using the selected components?",
        deliverQuestion:
            "Deliver the selected computer to this customer?",
        unitPrice: "Unit Price",
        totalCostLabel: "Total Cost",
        available: "Available",
        unavailable: "Unavailable",
        staffWorkingNow: "Employees currently working",
        automationDescription:
            "Employees work automatically as game time advances, according to their abilities and roles."
    }
);


Object.assign(
    translations.de,
    {
        dashboardDescription:
            "Verfolge Finanzen, Mitarbeiteraktivitäten und den täglichen Fortschritt.",
        marketDescription:
            "Durchsuche einen dynamischen Markt mit verschiedenen Marken, Modellen, Preisen und Zuständen.",
        inventoryDescription:
            "Verwalte gekaufte Komponenten und verkaufsbereite Computer.",
        workshopDescription:
            "Baue neue Computersysteme aus kompatiblen Komponenten.",
        customersDescription:
            "Liefere Computer, die den Kundenanforderungen entsprechen.",
        selectProduct: "Produkt auswählen",
        selectedProduct: "Ausgewähltes Produkt",
        noProductSelected:
            "Wähle ein Produkt aus, um die Eigenschaften anzuzeigen.",
        marketSearchPlaceholder:
            "Marke oder Modell suchen...",
        owned: "Im Besitz",
        buy: "Kaufen",
        parts: "Komponenten",
        computers: "Computer",
        emptyBuiltComputers:
            "Es gibt noch keine montierten Computer.",
        computerId: "Computer",
        powerUsage: "Stromverbrauch",
        estimatedValue: "Geschätzter Wert",
        buildCost: "Montagekosten",
        buildTime: "Montagezeit",
        selectedComponents: "Ausgewählte Komponenten",
        checkCompatibility: "Kompatibilität prüfen",
        compatibilitySuccess:
            "Alle ausgewählten Komponenten sind kompatibel.",
        compatibilityErrors:
            "Es wurden Kompatibilitätsfehler gefunden.",
        estimatedPerformance: "Geschätzte Leistung",
        estimatedSaleValue: "Geschätzter Verkaufswert",
        componentMissing: "Keine Komponente ausgewählt",
        deliveryComputer: "Zu liefernder Computer",
        customerRequirementsMet:
            "Alle verpflichtenden Kundenanforderungen sind erfüllt.",
        customerRequirementsNotMet:
            "Einige verpflichtende Kundenanforderungen werden nicht erfüllt.",
        minimumAccepted: "Minimal akzeptiert",
        orderResult: "Auftragsergebnis",
        profit: "Gewinn",
        loss: "Verlust",
        currentTask: "Aktuelle Aufgabe",
        resting: "Ruht sich aus",
        working: "Arbeitet",
        completed: "Aufgabe abgeschlossen",
        idle: "Wartet",
        storageUsage: "Lagernutzung",
        customerTraffic: "Kundenaufkommen",
        workshopEfficiency: "Werkstatteffizienz",
        automationPerformance: "Automatisierungsleistung",
        openOrders: "Offene Aufträge",
        todaySummary: "Heutige Zusammenfassung",
        noActivity: "Noch keine Aktivitäten vorhanden.",
        manualSale: "Manueller Verkauf",
        automaticSale: "Automatischer Verkauf",
        manualBuild: "Manuelle Montage",
        automaticBuild: "Automatische Montage",
        selectCustomer: "Kunden auswählen",
        noSuitableComputer:
            "Für diesen Kunden ist kein geeigneter Computer vorhanden.",
        customerPaymentRange:
            "Die Zahlung kann sich abhängig von der Eignung des Computers ändern.",
        allBrands: "Alle Marken",
        allConditions: "Alle Zustände",
        sortBy: "Sortierung",
        cheapest: "Günstigste",
        mostExpensive: "Teuerste",
        highestScore: "Höchste Punktzahl",
        bestDiscount: "Bester Rabatt",
        marketResult: "Produkte gefunden",
        viewDetails: "Details anzeigen",
        selectedPcDetails: "Ausgewählter Computer",
        partsUsed: "Verwendete Komponenten",
        quickSaleWarning:
            "Ein Schnellverkauf bringt weniger Einnahmen und kostet 1 Rufpunkt.",
        quickSellQuestion:
            "Möchtest du den ausgewählten Computer wirklich schnell verkaufen?",
        buildQuestion:
            "Computer aus den ausgewählten Komponenten montieren?",
        deliverQuestion:
            "Den ausgewählten Computer an diesen Kunden liefern?",
        unitPrice: "Stückpreis",
        totalCostLabel: "Gesamtkosten",
        available: "Verfügbar",
        unavailable: "Nicht verfügbar",
        staffWorkingNow: "Aktuell arbeitende Mitarbeiter",
        automationDescription:
            "Mitarbeiter arbeiten automatisch entsprechend ihrer Fähigkeiten und Rollen, während die Spielzeit läuft."
    }
);


/* =========================================================
   GÜVENLİ HTML METNİ
========================================================= */

function escapeHtml(value) {
    return String(value)
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll('"', "&quot;")
        .replaceAll("'", "&#039;");
}


/* =========================================================
   SAYFA BAŞLIĞI
========================================================= */

function createPageHeader(
    title,
    description,
    actionsHtml = ""
) {
    return `
        <div class="page-header">
            <div class="page-title-group">
                <h1>${escapeHtml(title)}</h1>
                <p>${escapeHtml(description)}</p>
            </div>

            <div class="page-actions">
                ${actionsHtml}
            </div>
        </div>
    `;
}


/* =========================================================
   METRİK KARTI
========================================================= */

function createMetricCard(
    label,
    value,
    description,
    changeText = "",
    changeType = ""
) {
    return `
        <div class="metric-card">
            <span class="metric-label">
                ${escapeHtml(label)}
            </span>

            <strong class="metric-value">
                ${escapeHtml(value)}
            </strong>

            <span class="metric-description">
                ${escapeHtml(description)}
            </span>

            ${
                changeText
                    ? `
                        <span class="metric-change ${changeType}">
                            ${escapeHtml(changeText)}
                        </span>
                    `
                    : ""
            }
        </div>
    `;
}


/* =========================================================
   PARÇA TÜRÜ İKONU
========================================================= */

function getComponentIconInformation(type) {
    const icons = {
        CPU: {
            text: "CPU",
            className: ""
        },

        Motherboard: {
            text: "MB",
            className: ""
        },

        GPU: {
            text: "GPU",
            className: "wide"
        },

        RAM: {
            text: "RAM",
            className: "wide"
        },

        Storage: {
            text: "SSD",
            className: ""
        },

        PSU: {
            text: "PSU",
            className: "round"
        },

        Case: {
            text: "CASE",
            className: ""
        },

        Cooler: {
            text: "FAN",
            className: "round"
        }
    };

    return (
        icons[type]
        ||
        {
            text: "?",
            className: ""
        }
    );
}


function createComponentIcon(type) {
    const icon =
        getComponentIconInformation(
            type
        );

    return `
        <div class="component-icon ${icon.className}">
            ${icon.text}
        </div>
    `;
}


/* =========================================================
   ÜRÜN DURUMU METNİ
========================================================= */

function getConditionText(condition) {
    const conditionKeys = {
        new: "newCondition",
        outlet: "outletCondition",
        refurbished:
            "refurbishedCondition"
    };

    return t(
        conditionKeys[condition]
        ||
        condition
    );
}


/* =========================================================
   PARÇA ÖZELLİK ETİKETLERİ
========================================================= */

function getPartSpecificationTags(part) {
    if (!part) {
        return [];
    }

    if (part.type === "CPU") {
        return [
            part.socket,
            part.ramType,
            `${part.cores} Core`,
            `${part.wattage}W`
        ];
    }

    if (part.type === "Motherboard") {
        return [
            part.socket,
            part.ramType,
            part.formFactor
        ];
    }

    if (part.type === "GPU") {
        return [
            `${part.vram}GB`,
            `${part.length}mm`,
            `${part.wattage}W`
        ];
    }

    if (part.type === "RAM") {
        return [
            part.ramType,
            `${part.capacityGb}GB`,
            `${part.speed}MHz`
        ];
    }

    if (part.type === "Storage") {
        return [
            part.storageInterface,
            part.capacityGb >= 1000
                ? `${part.capacityGb / 1000}TB`
                : `${part.capacityGb}GB`
        ];
    }

    if (part.type === "PSU") {
        return [
            `${part.wattage}W`,
            `80+ ${part.rating}`
        ];
    }

    if (part.type === "Case") {
        return [
            part.supportedForms.join("/"),
            `${part.maximumGpuLength}mm GPU`
        ];
    }

    if (part.type === "Cooler") {
        return [
            `${part.tdp}W TDP`,
            ...part.supportedSockets.slice(
                0,
                2
            )
        ];
    }

    return [];
}


/* =========================================================
   ÜST BİLGİ ÇUBUĞUNU GÜNCELLEME
========================================================= */

function updateTopBar() {
    const moneyElement =
        document.getElementById(
            "top-money"
        );

    const reputationElement =
        document.getElementById(
            "top-reputation"
        );

    const levelElement =
        document.getElementById(
            "top-level"
        );

    const stageElement =
        document.getElementById(
            "store-stage-label"
        );

    const dateElement =
        document.getElementById(
            "calendar-date"
        );

    const timeElement =
        document.getElementById(
            "calendar-time"
        );

    const dayNumberElement =
        document.getElementById(
            "calendar-day-number"
        );

    const progressBar =
        document.getElementById(
            "day-progress-bar"
        );

    const progressText =
        document.getElementById(
            "day-progress-text"
        );

    const marketBadge =
        document.getElementById(
            "market-badge"
        );

    const customerBadge =
        document.getElementById(
            "customer-badge"
        );

    if (moneyElement) {
        moneyElement.textContent =
            formatMoney(
                gameState.money
            );
    }

    if (reputationElement) {
        reputationElement.textContent =
            formatNumber(
                gameState.reputation
            );
    }

    if (levelElement) {
        levelElement.textContent =
            formatNumber(
                gameState.level
            );
    }

    if (stageElement) {
        stageElement.textContent =
            getStoreStage().name;
    }

    if (dateElement) {
        dateElement.textContent =
            formatGameDate();
    }

    if (timeElement) {
        timeElement.textContent =
            minutesToTime(
                gameState.calendar.minutes
            );
    }

    if (dayNumberElement) {
        dayNumberElement.textContent =
            gameState.calendar.day;
    }

    const progress =
        Math.round(
            getDayProgress()
            *
            100
        );

    if (progressBar) {
        progressBar.style.width =
            `${progress}%`;
    }

    if (progressText) {
        progressText.textContent =
            `%${progress}`;
    }

    if (marketBadge) {
        marketBadge.textContent =
            gameState.marketOffers.filter(
                offer =>
                    offer.stock > 0
            ).length;
    }

    if (customerBadge) {
        customerBadge.textContent =
            gameState.customers.length;
    }

    updateTimeControlButtons();
}


/* =========================================================
   ZAMAN BUTONLARI
========================================================= */

function updateTimeControlButtons() {
    const pauseButton =
        document.getElementById(
            "pause-button"
        );

    if (pauseButton) {
        pauseButton.classList.toggle(
            "active",
            gameState.paused
        );

        pauseButton.textContent =
            gameState.paused
                ? "▶"
                : "‖";
    }

    document
        .querySelectorAll(
            ".speed-button"
        )
        .forEach(
            button => {
                button.classList.toggle(
                    "active",
                    !gameState.paused
                    &&
                    Number(
                        button.dataset.speed
                    )
                    ===
                    gameState.speed
                );
            }
        );
}


/* =========================================================
   GEÇERLİ SAYFAYI GÖSTERME
========================================================= */

function renderCurrentPage() {
    const content =
        document.getElementById(
            "page-content"
        );

    if (!content) {
        return;
    }

    document
        .querySelectorAll(
            ".nav-button"
        )
        .forEach(
            button => {
                button.classList.toggle(
                    "active",
                    button.dataset.page
                    ===
                    runtime.currentPage
                );
            }
        );

    const renderFunctions = {
        dashboard:
            renderDashboardPage,

        market:
            renderMarketPage,

        inventory:
            renderInventoryPage,

        workshop:
            renderWorkshopPage,

        customers:
            renderCustomersPage,

        staff:
            typeof renderStaffPage
            ===
            "function"
                ? renderStaffPage
                : renderDashboardPage,

        properties:
            typeof renderPropertiesPage
            ===
            "function"
                ? renderPropertiesPage
                : renderDashboardPage,

        finance:
            typeof renderFinancePage
            ===
            "function"
                ? renderFinancePage
                : renderDashboardPage,

        upgrades:
            typeof renderUpgradesPage
            ===
            "function"
                ? renderUpgradesPage
                : renderDashboardPage,

        activity:
            typeof renderActivityPage
            ===
            "function"
                ? renderActivityPage
                : renderDashboardPage
    };

    const renderFunction =
        renderFunctions[
            runtime.currentPage
        ]
        ||
        renderDashboardPage;

    renderFunction();

    renderOperationalAlert();

    updateTopBar();
}


/* =========================================================
   SAYFA DEĞİŞTİRME
========================================================= */

function navigateToPage(page) {
    runtime.currentPage =
        page;

    renderCurrentPage();
}


/* =========================================================
   GENEL BAKIŞ SAYFASI
========================================================= */

function renderDashboardPage() {
    const content =
        document.getElementById(
            "page-content"
        );

    if (!content) {
        return;
    }

    const property =
        getPropertyById(
            gameState.propertyId
        );

    const storageCount =
        getInventoryCount();

    const storageCapacity =
        getStorageCapacity();

    const storagePercentage =
        Math.round(
            storageCount
            /
            Math.max(
                1,
                storageCapacity
            )
            *
            100
        );

    const workingStaff =
        gameState.staff.filter(
            employee =>
                employee.status
                ===
                "working"
        ).length;

    const totalStaff =
        gameState.staff.length;

    const averageStaffQuality =
        totalStaff > 0
            ? Math.round(
                gameState.staff.reduce(
                    (
                        total,
                        employee
                    ) =>
                        total
                        +
                        employee.quality,
                    0
                )
                /
                totalStaff
            )
            : 0;

    const recentActivities =
        gameState.activity.slice(
            0,
            8
        );

    content.innerHTML =
        createPageHeader(
            t("dashboard"),
            t("dashboardDescription")
        )
        +
        `
            <div class="content-grid four-columns">
                ${createMetricCard(
                    t("money"),
                    formatMoney(
                        gameState.money
                    ),
                    t("todaySummary"),
                    (
                        gameState.daily.revenue
                        -
                        gameState.daily.expenses
                        >=
                        0
                    )
                        ? `+${formatMoney(
                            gameState.daily.revenue
                            -
                            gameState.daily.expenses
                        )}`
                        : formatMoney(
                            gameState.daily.revenue
                            -
                            gameState.daily.expenses
                        ),
                    (
                        gameState.daily.revenue
                        -
                        gameState.daily.expenses
                        >=
                        0
                    )
                        ? "positive"
                        : "negative"
                )}

                ${createMetricCard(
                    t("reputation"),
                    formatNumber(
                        gameState.reputation
                    ),
                    getStoreStage().name
                )}

                ${createMetricCard(
                    t("readyPcs"),
                    formatNumber(
                        gameState.builtComputers.length
                    ),
                    t("builtComputers")
                )}

                ${createMetricCard(
                    t("openOrders"),
                    formatNumber(
                        gameState.customers.length
                    ),
                    t("activeCustomers")
                )}
            </div>

            <div
                class="content-grid two-columns"
                style="margin-top:13px;"
            >
                <div class="shop-scene">
                    <div class="shop-ceiling"></div>
                    <div class="shop-wall"></div>
                    <div class="shop-floor"></div>

                    <div class="shop-sign">
                        ${escapeHtml(
                            getStoreStage().name
                        )}
                    </div>

                    <div class="shop-zone sales-zone">
                        <span class="zone-label">
                            ${t("salesArea")}
                        </span>

                        <div class="display-shelf"></div>
                        <div class="display-shelf"></div>
                        <div class="display-shelf"></div>
                    </div>

                    <div class="shop-zone workshop-zone">
                        <span class="zone-label">
                            ${t("assemblyArea")}
                        </span>

                        <div class="workbench left"></div>
                        <div class="workbench right"></div>
                    </div>

                    <div class="shop-stage-chip">
                        <span>◆</span>
                        <span>
                            ${escapeHtml(
                                property.size
                            )} m²
                            ·
                            ${escapeHtml(
                                property.storageCapacity
                            )}
                            ${t("capacity")}
                        </span>
                    </div>

                    <div class="shop-customer-count">
                        ${t("activeCustomers")}:
                        ${gameState.customers.length}
                    </div>
                </div>

                <div class="game-panel">
                    <div class="panel-header">
                        <div class="panel-title">
                            <div class="panel-title-icon">
                                ≡
                            </div>

                            <div>
                                <h2>${t("recentActivity")}</h2>
                                <span class="panel-subtitle">
                                    ${formatGameDate()}
                                </span>
                            </div>
                        </div>
                    </div>

                    <div class="panel-padding activity-list">
                        ${
                            recentActivities.length > 0
                                ? recentActivities
                                    .map(
                                        activity => `
                                            <div class="activity-item">
                                                <span class="activity-time">
                                                    ${escapeHtml(
                                                        activity.time
                                                    )}
                                                </span>

                                                <span class="activity-icon">
                                                    ${
                                                        activity.type
                                                        ===
                                                        "sale"
                                                            ? "€"
                                                            : activity.type
                                                            ===
                                                            "build"
                                                                ? "⚒"
                                                                : activity.type
                                                                ===
                                                                "purchase"
                                                                    ? "◆"
                                                                    : activity.type
                                                                    ===
                                                                    "staff"
                                                                        ? "♟"
                                                                        : "i"
                                                    }
                                                </span>

                                                <span class="activity-text">
                                                    ${escapeHtml(
                                                        activity.message
                                                    )}
                                                </span>
                                            </div>
                                        `
                                    )
                                    .join("")
                                : `
                                    <div class="empty-state">
                                        <div class="empty-state-icon">
                                            ≡
                                        </div>

                                        <h3>${t("noActivity")}</h3>
                                    </div>
                                `
                        }
                    </div>
                </div>
            </div>

            <div
                class="content-grid four-columns"
                style="margin-top:13px;"
            >
                ${createMetricCard(
                    t("storageUsage"),
                    `${storageCount}/${storageCapacity}`,
                    `%${storagePercentage}`
                )}

                ${createMetricCard(
                    t("staff"),
                    `${workingStaff}/${totalStaff}`,
                    t("staffWorkingNow")
                )}

                ${createMetricCard(
                    t("automationPerformance"),
                    `%${averageStaffQuality}`,
                    gameState.automationEnabled
                        ? t("active")
                        : t("inactive")
                )}

                ${createMetricCard(
                    t("todayRevenue"),
                    formatMoney(
                        gameState.daily.revenue
                    ),
                    `${t("todayExpenses")}: ${
                        formatMoney(
                            gameState.daily.expenses
                        )
                    }`
                )}
            </div>
        `;
}


/* =========================================================
   PAZAR FİLTRE DURUMU
========================================================= */

const marketViewState = {
    category: "All",
    brand: "All",
    condition: "All",
    search: "",
    sort: "cheapest"
};


/* =========================================================
   PAZAR SAYFASI
========================================================= */

function renderMarketPage() {
    const content =
        document.getElementById(
            "page-content"
        );

    if (!content) {
        return;
    }

    const categories = [
        "All",
        ...REQUIRED_COMPONENT_TYPES
    ];

    const brands = [
        "All",
        ...new Set(
            gameState.marketOffers
                .map(
                    offer =>
                        getPartById(
                            offer.partId
                        )?.brand
                )
                .filter(Boolean)
        )
    ].sort();

    let filteredOffers =
        gameState.marketOffers.filter(
            offer =>
                offer.stock > 0
        );

    if (
        marketViewState.category
        !==
        "All"
    ) {
        filteredOffers =
            filteredOffers.filter(
                offer =>
                    getPartById(
                        offer.partId
                    )?.type
                    ===
                    marketViewState.category
            );
    }

    if (
        marketViewState.brand
        !==
        "All"
    ) {
        filteredOffers =
            filteredOffers.filter(
                offer =>
                    getPartById(
                        offer.partId
                    )?.brand
                    ===
                    marketViewState.brand
            );
    }

    if (
        marketViewState.condition
        !==
        "All"
    ) {
        filteredOffers =
            filteredOffers.filter(
                offer =>
                    offer.condition
                    ===
                    marketViewState.condition
            );
    }

    const searchText =
        marketViewState.search
            .trim()
            .toLocaleLowerCase();

    if (searchText) {
        filteredOffers =
            filteredOffers.filter(
                offer => {
                    const part =
                        getPartById(
                            offer.partId
                        );

                    const searchableText =
                        (
                            `${part?.brand || ""} `
                            +
                            `${part?.model || ""} `
                            +
                            `${part?.name || ""}`
                        )
                        .toLocaleLowerCase();

                    return searchableText.includes(
                        searchText
                    );
                }
            );
    }

    filteredOffers.sort(
        (
            first,
            second
        ) => {
            const firstPart =
                getPartById(
                    first.partId
                );

            const secondPart =
                getPartById(
                    second.partId
                );

            if (
                marketViewState.sort
                ===
                "mostExpensive"
            ) {
                return (
                    second.price
                    -
                    first.price
                );
            }

            if (
                marketViewState.sort
                ===
                "highestScore"
            ) {
                return (
                    secondPart.score
                    -
                    firstPart.score
                );
            }

            if (
                marketViewState.sort
                ===
                "bestDiscount"
            ) {
                const firstRatio =
                    first.price
                    /
                    Math.max(
                        1,
                        firstPart.basePrice
                    );

                const secondRatio =
                    second.price
                    /
                    Math.max(
                        1,
                        secondPart.basePrice
                    );

                return (
                    firstRatio
                    -
                    secondRatio
                );
            }

            return (
                first.price
                -
                second.price
            );
        }
    );

    const selectedOffer =
        getOfferById(
            runtime.selectedOfferId
        );

    const selectedPart =
        selectedOffer
            ? getPartById(
                selectedOffer.partId
            )
            : null;

    content.innerHTML =
        createPageHeader(
            t("market"),
            t("marketDescription")
        )
        +
        `
            <div class="filter-bar">
                <div class="filter-group">
                    <span class="filter-label">
                        ${t("category")}
                    </span>

                    <select
                        id="market-category-filter"
                        class="game-select"
                    >
                        ${categories
                            .map(
                                category => `
                                    <option
                                        value="${category}"
                                        ${
                                            category
                                            ===
                                            marketViewState.category
                                                ? "selected"
                                                : ""
                                        }
                                    >
                                        ${
                                            category
                                            ===
                                            "All"
                                                ? t("all")
                                                : getComponentTypeLabel(
                                                    category
                                                )
                                        }
                                    </option>
                                `
                            )
                            .join("")}
                    </select>
                </div>

                <div class="filter-group">
                    <span class="filter-label">
                        ${t("allBrands")}
                    </span>

                    <select
                        id="market-brand-filter"
                        class="game-select"
                    >
                        ${brands
                            .map(
                                brand => `
                                    <option
                                        value="${escapeHtml(brand)}"
                                        ${
                                            brand
                                            ===
                                            marketViewState.brand
                                                ? "selected"
                                                : ""
                                        }
                                    >
                                        ${
                                            brand
                                            ===
                                            "All"
                                                ? t("allBrands")
                                                : escapeHtml(
                                                    brand
                                                )
                                        }
                                    </option>
                                `
                            )
                            .join("")}
                    </select>
                </div>

                <div class="filter-group">
                    <span class="filter-label">
                        ${t("condition")}
                    </span>

                    <select
                        id="market-condition-filter"
                        class="game-select"
                    >
                        <option
                            value="All"
                            ${
                                marketViewState.condition
                                ===
                                "All"
                                    ? "selected"
                                    : ""
                            }
                        >
                            ${t("allConditions")}
                        </option>

                        <option
                            value="new"
                            ${
                                marketViewState.condition
                                ===
                                "new"
                                    ? "selected"
                                    : ""
                            }
                        >
                            ${t("newCondition")}
                        </option>

                        <option
                            value="outlet"
                            ${
                                marketViewState.condition
                                ===
                                "outlet"
                                    ? "selected"
                                    : ""
                            }
                        >
                            ${t("outletCondition")}
                        </option>

                        <option
                            value="refurbished"
                            ${
                                marketViewState.condition
                                ===
                                "refurbished"
                                    ? "selected"
                                    : ""
                            }
                        >
                            ${t("refurbishedCondition")}
                        </option>
                    </select>
                </div>

                <div class="filter-group">
                    <span class="filter-label">
                        ${t("sortBy")}
                    </span>

                    <select
                        id="market-sort-filter"
                        class="game-select"
                    >
                        <option value="cheapest">
                            ${t("cheapest")}
                        </option>

                        <option value="mostExpensive">
                            ${t("mostExpensive")}
                        </option>

                        <option value="highestScore">
                            ${t("highestScore")}
                        </option>

                        <option value="bestDiscount">
                            ${t("bestDiscount")}
                        </option>
                    </select>
                </div>

                <input
                    id="market-search-input"
                    class="game-input"
                    type="search"
                    value="${escapeHtml(
                        marketViewState.search
                    )}"
                    placeholder="${escapeHtml(
                        t(
                            "marketSearchPlaceholder"
                        )
                    )}"
                    style="min-width:210px; flex:1;"
                >

                <span class="status-badge info">
                    ${filteredOffers.length}
                    ${t("marketResult")}
                </span>
            </div>

            <div class="market-layout">
                <div class="product-grid">
                    ${
                        filteredOffers
                            .map(
                                offer => {
                                    const part =
                                        getPartById(
                                            offer.partId
                                        );

                                    const tags =
                                        getPartSpecificationTags(
                                            part
                                        );

                                    const discount =
                                        Math.round(
                                            (
                                                1
                                                -
                                                offer.price
                                                /
                                                Math.max(
                                                    1,
                                                    part.basePrice
                                                )
                                            )
                                            *
                                            100
                                        );

                                    return `
                                        <article
                                            class="product-card ${
                                                runtime.selectedOfferId
                                                ===
                                                offer.id
                                                    ? "selected"
                                                    : ""
                                            }"
                                            data-offer-id="${
                                                offer.id
                                            }"
                                        >
                                            <div class="product-visual">
                                                ${createComponentIcon(
                                                    part.type
                                                )}

                                                <span class="product-condition">
                                                    ${escapeHtml(
                                                        getConditionText(
                                                            offer.condition
                                                        )
                                                    )}
                                                </span>

                                                <span class="product-bundle">
                                                    ${t("compatibleSet")} ${offer.bundleId || ""}
                                                </span>
                                            </div>

                                            <div class="product-card-body">
                                                <div class="product-brand">
                                                    ${escapeHtml(
                                                        part.brand
                                                    )}
                                                </div>

                                                <div class="product-name">
                                                    ${escapeHtml(
                                                        part.model
                                                    )}
                                                </div>

                                                <div class="product-specs">
                                                    ${tags
                                                        .slice(
                                                            0,
                                                            3
                                                        )
                                                        .map(
                                                            tag => `
                                                                <span class="small-tag">
                                                                    ${escapeHtml(
                                                                        tag
                                                                    )}
                                                                </span>
                                                            `
                                                        )
                                                        .join("")}
                                                </div>

                                                <div class="product-card-footer">
                                                    <div>
                                                        <div class="product-price">
                                                            ${formatMoney(
                                                                offer.price
                                                            )}
                                                        </div>

                                                        <div class="product-stock">
                                                            ${t("stock")}:
                                                            ${offer.stock}
                                                            ·
                                                            ${t("owned")}:
                                                            ${getInventoryQuantity(
                                                                part.id
                                                            )}
                                                        </div>
                                                    </div>

                                                    ${
                                                        discount > 0
                                                            ? `
                                                                <span class="status-badge success">
                                                                    -%${discount}
                                                                </span>
                                                            `
                                                            : ""
                                                    }
                                                </div>
                                            </div>
                                        </article>
                                    `;
                                }
                            )
                            .join("")
                    }
                </div>

                <aside class="game-panel product-detail-panel">
                    ${
                        selectedOffer
                        &&
                        selectedPart
                            ? `
                                <div class="large-product-visual">
                                    ${createComponentIcon(
                                        selectedPart.type
                                    )}
                                </div>

                                <div class="detail-content">
                                    <span class="status-badge info">
                                        ${escapeHtml(
                                            getComponentTypeLabel(
                                                selectedPart.type
                                            )
                                        )}
                                    </span>

                                    <h2 class="detail-title">
                                        ${escapeHtml(
                                            selectedPart.name
                                        )}
                                    </h2>

                                    <p class="detail-description">
                                        ${escapeHtml(
                                            selectedOffer.seller
                                        )}
                                        ·
                                        ${escapeHtml(
                                            getConditionText(
                                                selectedOffer.condition
                                            )
                                        )}
                                    </p>

                                    <div class="detail-row">
                                        <span>${t("score")}</span>
                                        <strong>
                                            ${selectedPart.score}
                                        </strong>
                                    </div>

                                    <div class="detail-row">
                                        <span>${t("unitPrice")}</span>
                                        <strong class="text-success">
                                            ${formatMoney(
                                                selectedOffer.price
                                            )}
                                        </strong>
                                    </div>

                                    <div class="detail-row">
                                        <span>${t("stock")}</span>
                                        <strong>
                                            ${selectedOffer.stock}
                                        </strong>
                                    </div>

                                    <div class="detail-row">
                                        <span>${t("owned")}</span>
                                        <strong>
                                            ${getInventoryQuantity(
                                                selectedPart.id
                                            )}
                                        </strong>
                                    </div>

                                    <div class="detail-row">
                                        <span>${t("compatibility")}</span>
                                        <strong class="text-success">
                                            ${t("compatibleSet")} ${selectedOffer.bundleId || ""}
                                        </strong>
                                    </div>

                                    <div class="tag-list"
                                         style="margin:13px 0;">
                                        ${getPartSpecificationTags(
                                            selectedPart
                                        )
                                            .map(
                                                tag => `
                                                    <span class="small-tag">
                                                        ${escapeHtml(
                                                            tag
                                                        )}
                                                    </span>
                                                `
                                            )
                                            .join("")}
                                    </div>

                                    <button
                                        id="buy-selected-one"
                                        class="game-button primary full-width"
                                        type="button"
                                    >
                                        ${t("buyOne")}
                                    </button>

                                    <button
                                        id="buy-selected-five"
                                        class="game-button secondary full-width"
                                        type="button"
                                        style="margin-top:7px;"
                                    >
                                        ${t("buyFive")}
                                    </button>
                                </div>
                            `
                            : `
                                <div class="empty-state">
                                    <div class="empty-state-icon">
                                        ◆
                                    </div>

                                    <h3>${t("selectProduct")}</h3>

                                    <p>
                                        ${t("noProductSelected")}
                                    </p>
                                </div>
                            `
                    }
                </aside>
            </div>
        `;

    const sortElement =
        document.getElementById(
            "market-sort-filter"
        );

    if (sortElement) {
        sortElement.value =
            marketViewState.sort;
    }

    document
        .querySelectorAll(
            ".product-card"
        )
        .forEach(
            card => {
                card.addEventListener(
                    "click",
                    () => {
                        runtime.selectedOfferId =
                            card.dataset.offerId;

                        renderMarketPage();
                    }
                );
            }
        );

    document
        .getElementById(
            "market-category-filter"
        )
        ?.addEventListener(
            "change",
            event => {
                marketViewState.category =
                    event.target.value;

                runtime.selectedOfferId =
                    null;

                renderMarketPage();
            }
        );

    document
        .getElementById(
            "market-brand-filter"
        )
        ?.addEventListener(
            "change",
            event => {
                marketViewState.brand =
                    event.target.value;

                runtime.selectedOfferId =
                    null;

                renderMarketPage();
            }
        );

    document
        .getElementById(
            "market-condition-filter"
        )
        ?.addEventListener(
            "change",
            event => {
                marketViewState.condition =
                    event.target.value;

                runtime.selectedOfferId =
                    null;

                renderMarketPage();
            }
        );

    sortElement?.addEventListener(
        "change",
        event => {
            marketViewState.sort =
                event.target.value;

            renderMarketPage();
        }
    );

    document
        .getElementById(
            "market-search-input"
        )
        ?.addEventListener(
            "input",
            event => {
                marketViewState.search =
                    event.target.value;

                renderMarketPage();

                const newInput =
                    document.getElementById(
                        "market-search-input"
                    );

                newInput?.focus();

                newInput?.setSelectionRange(
                    marketViewState.search.length,
                    marketViewState.search.length
                );
            }
        );

    document
        .getElementById(
            "buy-selected-one"
        )
        ?.addEventListener(
            "click",
            () => {
                handleMarketPurchase(
                    runtime.selectedOfferId,
                    1
                );
            }
        );

    document
        .getElementById(
            "buy-selected-five"
        )
        ?.addEventListener(
            "click",
            () => {
                handleMarketPurchase(
                    runtime.selectedOfferId,
                    5
                );
            }
        );
}


/* =========================================================
   PAZAR SATIN ALMA ARAYÜZÜ
========================================================= */

function handleMarketPurchase(
    offerId,
    amount
) {
    const result =
        purchaseMarketOffer(
            offerId,
            amount
        );

    if (!result.success) {
        showToast(
            t("error"),
            result.message,
            "error"
        );

        return;
    }

    showToast(
        t("purchaseCompleted"),
        (
            `${amount} × ${result.part.name} · `
            +
            `${formatMoney(result.totalPrice)}`
        ),
        "success"
    );

    renderMarketPage();
}


/* =========================================================
   ENVANTER SAYFASI
========================================================= */

function renderInventoryPage() {
    const content =
        document.getElementById(
            "page-content"
        );

    if (!content) {
        return;
    }

    const inventoryItems =
        Object.entries(
            gameState.inventory
        )
        .filter(
            (
                [
                    _partId,
                    inventoryItem
                ]
            ) =>
                inventoryItem.quantity
                >
                0
        )
        .map(
            (
                [
                    partId,
                    inventoryItem
                ]
            ) => ({
                part:
                    getPartById(
                        partId
                    ),

                quantity:
                    inventoryItem.quantity,

                averageCost:
                    inventoryItem.averageCost
            })
        )
        .filter(
            item =>
                item.part
        )
        .sort(
            (
                first,
                second
            ) =>
                first.part.type.localeCompare(
                    second.part.type
                )
                ||
                first.part.name.localeCompare(
                    second.part.name
                )
        );

    const selectedComputer =
        getBuiltPcById(
            runtime.selectedBuiltPcId
        );

    content.innerHTML =
        createPageHeader(
            t("inventory"),
            t("inventoryDescription")
        )
        +
        `
            <div class="content-grid three-columns">
                ${createMetricCard(
                    t("storageUsage"),
                    `${getInventoryCount()}/${getStorageCapacity()}`,
                    t("storageCapacity")
                )}

                ${createMetricCard(
                    t("inventoryValue"),
                    formatMoney(
                        getInventoryValue()
                    ),
                    `${inventoryItems.length} ${t("parts")}`
                )}

                ${createMetricCard(
                    t("readyPcs"),
                    formatNumber(
                        gameState.builtComputers.length
                    ),
                    t("builtComputers")
                )}
            </div>

            <div
                class="game-panel"
                style="margin-top:13px;"
            >
                <div class="panel-header">
                    <div class="panel-title">
                        <div class="panel-title-icon">
                            ▦
                        </div>

                        <div>
                            <h2>${t("computerParts")}</h2>
                            <span class="panel-subtitle">
                                ${inventoryItems.length}
                                ${t("parts")}
                            </span>
                        </div>
                    </div>
                </div>

                ${
                    inventoryItems.length > 0
                        ? `
                            <div class="table-wrapper">
                                <table class="game-table">
                                    <thead>
                                        <tr>
                                            <th>${t("category")}</th>
                                            <th>${t("selectedProduct")}</th>
                                            <th>${t("quantity")}</th>
                                            <th>${t("averageCost")}</th>
                                            <th>${t("totalValue")}</th>
                                            <th>${t("score")}</th>
                                            <th>${t("actions")}</th>
                                        </tr>
                                    </thead>

                                    <tbody>
                                        ${inventoryItems
                                            .map(
                                                item => `
                                                    <tr>
                                                        <td>
                                                            ${escapeHtml(
                                                                getComponentTypeLabel(
                                                                    item.part.type
                                                                )
                                                            )}
                                                        </td>

                                                        <td>
                                                            <strong>
                                                                ${escapeHtml(
                                                                    item.part.name
                                                                )}
                                                            </strong>
                                                        </td>

                                                        <td>
                                                            ${item.quantity}
                                                        </td>

                                                        <td>
                                                            ${formatMoney(
                                                                item.averageCost
                                                            )}
                                                        </td>

                                                        <td>
                                                            ${formatMoney(
                                                                item.averageCost
                                                                *
                                                                item.quantity
                                                            )}
                                                        </td>

                                                        <td>
                                                            ${item.part.score}
                                                        </td>

                                                        <td>
                                                            <div class="inventory-sale-actions">
                                                                <button
                                                                    class="mini-action-button"
                                                                    type="button"
                                                                    data-sell-part="${item.part.id}"
                                                                    data-sell-amount="1"
                                                                >
                                                                    ${t("sellOne")}
                                                                </button>

                                                                <button
                                                                    class="mini-action-button danger"
                                                                    type="button"
                                                                    data-sell-part="${item.part.id}"
                                                                    data-sell-amount="${item.quantity}"
                                                                >
                                                                    ${t("sellAll")}
                                                                </button>
                                                            </div>

                                                            <small class="resale-hint">
                                                                ${formatMoney(
                                                                    getInventoryPartResalePrice(
                                                                        item.part.id
                                                                    )
                                                                )} / ${t("unit")}
                                                            </small>
                                                        </td>
                                                    </tr>
                                                `
                                            )
                                            .join("")}
                                    </tbody>
                                </table>
                            </div>
                        `
                        : `
                            <div class="empty-state">
                                <div class="empty-state-icon">
                                    ▦
                                </div>

                                <h3>${t("inventoryEmpty")}</h3>
                            </div>
                        `
                }
            </div>

            <div
                class="market-layout"
                style="margin-top:13px;"
            >
                <div class="game-panel">
                    <div class="panel-header">
                        <div class="panel-title">
                            <div class="panel-title-icon">
                                PC
                            </div>

                            <div>
                                <h2>${t("builtComputers")}</h2>
                                <span class="panel-subtitle">
                                    ${gameState.builtComputers.length}
                                    ${t("computers")}
                                </span>
                            </div>
                        </div>
                    </div>

                    ${
                        gameState.builtComputers.length > 0
                            ? `
                                <div class="table-wrapper">
                                    <table class="game-table">
                                        <thead>
                                            <tr>
                                                <th>${t("computerId")}</th>
                                                <th>${t("score")}</th>
                                                <th>${t("powerUsage")}</th>
                                                <th>${t("buildCost")}</th>
                                                <th>${t("estimatedValue")}</th>
                                                <th>${t("buildTime")}</th>
                                            </tr>
                                        </thead>

                                        <tbody>
                                            ${gameState.builtComputers
                                                .map(
                                                    computer => `
                                                        <tr
                                                            class="${
                                                                runtime.selectedBuiltPcId
                                                                ===
                                                                computer.id
                                                                    ? "selected"
                                                                    : ""
                                                            }"
                                                            data-computer-id="${
                                                                computer.id
                                                            }"
                                                        >
                                                            <td>
                                                                <strong>
                                                                    ${escapeHtml(
                                                                        computer.id
                                                                    )}
                                                                </strong>
                                                            </td>

                                                            <td>
                                                                ${computer.score}
                                                            </td>

                                                            <td>
                                                                ${computer.powerDraw}W
                                                            </td>

                                                            <td>
                                                                ${formatMoney(
                                                                    computer.cost
                                                                )}
                                                            </td>

                                                            <td class="text-success">
                                                                ${formatMoney(
                                                                    computer.value
                                                                )}
                                                            </td>

                                                            <td>
                                                                ${escapeHtml(
                                                                    computer.builtTime
                                                                )}
                                                            </td>
                                                        </tr>
                                                    `
                                                )
                                                .join("")}
                                        </tbody>
                                    </table>
                                </div>
                            `
                            : `
                                <div class="empty-state">
                                    <div class="empty-state-icon">
                                        PC
                                    </div>

                                    <h3>
                                        ${t("emptyBuiltComputers")}
                                    </h3>
                                </div>
                            `
                    }
                </div>

                <aside class="game-panel">
                    ${
                        selectedComputer
                            ? createComputerDetailPanel(
                                selectedComputer,
                                true
                            )
                            : `
                                <div class="empty-state">
                                    <div class="empty-state-icon">
                                        PC
                                    </div>

                                    <h3>${t("selectPc")}</h3>
                                </div>
                            `
                    }
                </aside>
            </div>
        `;

    document
        .querySelectorAll(
            "[data-computer-id]"
        )
        .forEach(
            row => {
                row.addEventListener(
                    "click",
                    () => {
                        runtime.selectedBuiltPcId =
                            row.dataset.computerId;

                        renderInventoryPage();
                    }
                );
            }
        );

    document
        .getElementById(
            "quick-sell-selected-pc"
        )
        ?.addEventListener(
            "click",
            handleQuickSellSelectedComputer
        );

    document
        .querySelectorAll("[data-sell-part]")
        .forEach(button => {
            button.addEventListener("click", () => {
                handleInventoryPartSale(
                    button.dataset.sellPart,
                    Number(button.dataset.sellAmount)
                );
            });
        });
}


/* =========================================================
   BİLGİSAYAR DETAY PANELİ
========================================================= */

function createComputerDetailPanel(
    computer,
    includeQuickSellButton = false
) {
    const partRows =
        computer.partIds
            .map(
                partId =>
                    getPartById(
                        partId
                    )
            )
            .filter(Boolean)
            .map(
                part => `
                    <div class="detail-row">
                        <span>
                            ${escapeHtml(
                                getComponentTypeLabel(
                                    part.type
                                )
                            )}
                        </span>

                        <strong>
                            ${escapeHtml(
                                part.name
                            )}
                        </strong>
                    </div>
                `
            )
            .join("");

    const expectedProfit =
        computer.value
        -
        computer.cost;

    return `
        <div class="panel-header">
            <div class="panel-title">
                <div class="panel-title-icon">
                    PC
                </div>

                <div>
                    <h2>${escapeHtml(computer.id)}</h2>
                    <span class="panel-subtitle">
                        ${t("selectedPcDetails")}
                    </span>
                </div>
            </div>
        </div>

        <div class="detail-content">
            <div class="detail-row">
                <span>${t("score")}</span>
                <strong>
                    ${computer.score}
                </strong>
            </div>

            <div class="detail-row">
                <span>${t("powerUsage")}</span>
                <strong>
                    ${computer.powerDraw}W
                </strong>
            </div>

            <div class="detail-row">
                <span>${t("buildCost")}</span>
                <strong>
                    ${formatMoney(
                        computer.cost
                    )}
                </strong>
            </div>

            <div class="detail-row">
                <span>${t("estimatedValue")}</span>
                <strong class="text-success">
                    ${formatMoney(
                        computer.value
                    )}
                </strong>
            </div>

            <div class="detail-row">
                <span>${t("profit")}</span>
                <strong class="${
                    expectedProfit >= 0
                        ? "text-success"
                        : "text-danger"
                }">
                    ${formatMoney(
                        expectedProfit
                    )}
                </strong>
            </div>

            <h3 style="margin-top:17px;">
                ${t("partsUsed")}
            </h3>

            ${partRows}

            ${
                includeQuickSellButton
                    ? `
                        <p
                            class="text-warning"
                            style="font-size:10px; line-height:1.5;"
                        >
                            ${t("quickSaleWarning")}
                        </p>

                        <button
                            id="quick-sell-selected-pc"
                            class="game-button danger full-width"
                            type="button"
                        >
                            ${t("quickSell")}
                        </button>
                    `
                    : ""
            }
        </div>
    `;
}


/* =========================================================
   HIZLI SATIŞ ARAYÜZÜ
========================================================= */

async function handleQuickSellSelectedComputer() {
    const computer =
        getBuiltPcById(
            runtime.selectedBuiltPcId
        );

    if (!computer) {
        return;
    }

    const accepted =
        await showGameModal({
            title:
                t("quickSell"),

            message:
                (
                    `${t("quickSellQuestion")}\n\n`
                    +
                    `${computer.id} · `
                    +
                    `${formatMoney(computer.value)}`
                ),

            icon:
                "€",

            type:
                "warning",

            confirmText:
                t("yes"),

            cancelText:
                t("no")
        });

    if (!accepted) {
        return;
    }

    const result =
        quickSellComputer(
            computer.id
        );

    if (!result.success) {
        showToast(
            t("error"),
            result.message,
            "error"
        );

        return;
    }

    runtime.selectedBuiltPcId =
        null;

    showToast(
        t("quickSaleCompleted"),
        formatMoney(
            result.salePrice
        ),
        "success"
    );

    renderInventoryPage();
}


/* =========================================================
   MONTAJ SEÇİMLERİ
========================================================= */

runtime.selectedBuildParts =
    runtime.selectedBuildParts || {};


/* =========================================================
   MONTAJ SAYFASI
========================================================= */

function renderWorkshopPage() {
    const content =
        document.getElementById(
            "page-content"
        );

    if (!content) {
        return;
    }

    const selectedPartIds =
        REQUIRED_COMPONENT_TYPES
            .map(
                type =>
                    runtime.selectedBuildParts[
                        type
                    ]
            )
            .filter(Boolean);

    const allSelected =
        selectedPartIds.length
        ===
        REQUIRED_COMPONENT_TYPES.length;

    const errors =
        allSelected
            ? getCompatibilityErrors(
                selectedPartIds
            )
            : [];

    const specifications =
        allSelected
        &&
        errors.length === 0
            ? calculateComputerSpecifications(
                selectedPartIds
            )
            : null;

    content.innerHTML =
        createPageHeader(
            t("workshop"),
            t("workshopDescription")
        )
        +
        `
            <div class="workshop-layout">
                <div class="game-panel">
                    <div class="panel-header">
                        <div class="panel-title">
                            <div class="panel-title-icon">
                                ⚒
                            </div>

                            <div>
                                <h2>${t("selectedComponents")}</h2>
                                <span class="panel-subtitle">
                                    ${selectedPartIds.length}/${
                                        REQUIRED_COMPONENT_TYPES.length
                                    }
                                </span>
                            </div>
                        </div>
                    </div>

                    <div class="component-slots">
                        ${REQUIRED_COMPONENT_TYPES
                            .map(
                                type => {
                                    const availableParts =
                                        getInventoryPartsByType(
                                            type
                                        );

                                    const selectedId =
                                        runtime.selectedBuildParts[
                                            type
                                        ]
                                        ||
                                        "";

                                    const selectedPart =
                                        selectedId
                                            ? getPartById(
                                                selectedId
                                            )
                                            : null;

                                    return `
                                        <div class="component-slot ${
                                            selectedPart
                                                ? "filled"
                                                : ""
                                        }">
                                            <div class="slot-header">
                                                <span class="slot-title">
                                                    ${escapeHtml(
                                                        getComponentTypeLabel(
                                                            type
                                                        )
                                                    )}
                                                </span>

                                                <span class="status-badge ${
                                                    selectedPart
                                                        ? "success"
                                                        : "warning"
                                                }">
                                                    ${
                                                        selectedPart
                                                            ? t("available")
                                                            : t("componentMissing")
                                                    }
                                                </span>
                                            </div>

                                            <select
                                                class="game-select slot-selection"
                                                data-build-type="${type}"
                                            >
                                                <option value="">
                                                    ${t("componentMissing")}
                                                </option>

                                                ${availableParts
                                                    .map(
                                                        part => `
                                                            <option
                                                                value="${
                                                                    part.id
                                                                }"
                                                                ${
                                                                    selectedId
                                                                    ===
                                                                    part.id
                                                                        ? "selected"
                                                                        : ""
                                                                }
                                                            >
                                                                ${escapeHtml(
                                                                    part.name
                                                                )}
                                                                ·
                                                                ${part.score}
                                                                ·
                                                                x${getInventoryQuantity(
                                                                    part.id
                                                                )}
                                                            </option>
                                                        `
                                                    )
                                                    .join("")}
                                            </select>

                                            ${
                                                selectedPart
                                                    ? `
                                                        <div class="tag-list"
                                                             style="margin-top:8px;">
                                                            ${getPartSpecificationTags(
                                                                selectedPart
                                                            )
                                                                .slice(
                                                                    0,
                                                                    3
                                                                )
                                                                .map(
                                                                    tag => `
                                                                        <span class="small-tag">
                                                                            ${escapeHtml(
                                                                                tag
                                                                            )}
                                                                        </span>
                                                                    `
                                                                )
                                                                .join("")}
                                                        </div>
                                                    `
                                                    : ""
                                            }
                                        </div>
                                    `;
                                }
                            )
                            .join("")}
                    </div>
                </div>

                <aside class="game-panel">
                    <div class="panel-header">
                        <div class="panel-title">
                            <div class="panel-title-icon">
                                PC
                            </div>

                            <div>
                                <h2>${t("compatibility")}</h2>
                                <span class="panel-subtitle">
                                    ${t("estimatedPerformance")}
                                </span>
                            </div>
                        </div>
                    </div>

                    <div class="panel-padding">
                        <div class="pc-case-visual ${
                            !allSelected
                                ? ""
                                : errors.length === 0
                                    ? "compatible"
                                    : "incompatible"
                        }">
                            <div class="case-fan"></div>
                            <div class="case-board"></div>
                            <div class="case-gpu"></div>
                        </div>

                        ${
                            !allSelected
                                ? `
                                    <div class="compatibility-item error">
                                        <span>!</span>
                                        <span>${t("missingPart")}</span>
                                    </div>
                                `
                                : errors.length > 0
                                    ? `
                                        <div class="compatibility-list">
                                            ${errors
                                                .map(
                                                    error => `
                                                        <div class="compatibility-item error">
                                                            <span>×</span>
                                                            <span>
                                                                ${escapeHtml(
                                                                    error
                                                                )}
                                                            </span>
                                                        </div>
                                                    `
                                                )
                                                .join("")}
                                        </div>
                                    `
                                    : `
                                        <div class="compatibility-item success">
                                            <span>✓</span>
                                            <span>
                                                ${t("compatibilitySuccess")}
                                            </span>
                                        </div>

                                        <div class="detail-row">
                                            <span>
                                                ${t("estimatedPerformance")}
                                            </span>

                                            <strong>
                                                ${specifications.score}
                                            </strong>
                                        </div>

                                        <div class="detail-row">
                                            <span>
                                                ${t("powerUsage")}
                                            </span>

                                            <strong>
                                                ${specifications.powerDraw}W
                                            </strong>
                                        </div>

                                        <div class="detail-row">
                                            <span>
                                                ${t("buildCost")}
                                            </span>

                                            <strong>
                                                ${formatMoney(
                                                    specifications.totalCost
                                                )}
                                            </strong>
                                        </div>

                                        <div class="detail-row">
                                            <span>
                                                ${t("estimatedSaleValue")}
                                            </span>

                                            <strong class="text-success">
                                                ${formatMoney(
                                                    specifications.estimatedValue
                                                )}
                                            </strong>
                                        </div>
                                    `
                        }

                        <button
                            id="assemble-selected-computer"
                            class="game-button primary full-width"
                            type="button"
                            style="margin-top:13px;"
                            ${
                                !allSelected
                                ||
                                errors.length > 0
                                    ? "disabled"
                                    : ""
                            }
                        >
                            ${t("assemble")}
                        </button>
                    </div>
                </aside>
            </div>
        `;

    document
        .querySelectorAll(
            "[data-build-type]"
        )
        .forEach(
            select => {
                select.addEventListener(
                    "change",
                    () => {
                        const type =
                            select.dataset.buildType;

                        if (select.value) {
                            runtime.selectedBuildParts[
                                type
                            ] =
                                select.value;
                        } else {
                            delete runtime.selectedBuildParts[
                                type
                            ];
                        }

                        renderWorkshopPage();
                    }
                );
            }
        );

    document
        .getElementById(
            "assemble-selected-computer"
        )
        ?.addEventListener(
            "click",
            handleManualComputerBuild
        );
}


/* =========================================================
   MANUEL MONTAJ
========================================================= */

async function handleManualComputerBuild() {
    const partIds =
        REQUIRED_COMPONENT_TYPES.map(
            type =>
                runtime.selectedBuildParts[
                    type
                ]
        );

    const errors =
        getCompatibilityErrors(
            partIds
        );

    if (
        errors.length
        >
        0
    ) {
        await showInformationModal(
            t("compatibilityErrors"),
            errors.join("\n"),
            "danger",
            "×"
        );

        return;
    }

    const specifications =
        calculateComputerSpecifications(
            partIds
        );

    const accepted =
        await showGameModal({
            title:
                t("assemble"),

            message:
                (
                    `${t("buildQuestion")}\n\n`
                    +
                    `${t("score")}: `
                    +
                    `${specifications.score}\n`
                    +
                    `${t("buildCost")}: `
                    +
                    `${formatMoney(
                        specifications.totalCost
                    )}\n`
                    +
                    `${t("estimatedValue")}: `
                    +
                    `${formatMoney(
                        specifications.estimatedValue
                    )}`
                ),

            icon:
                "⚒",

            type:
                "info",

            confirmText:
                t("yes"),

            cancelText:
                t("no")
        });

    if (!accepted) {
        return;
    }

    const result =
        buildComputerFromParts(
            partIds,
            {
                automated: false
            }
        );

    if (!result.success) {
        await showInformationModal(
            t("error"),
            result.errors.join("\n"),
            "danger",
            "×"
        );

        return;
    }

    runtime.selectedBuildParts = {};

    showToast(
        t("pcBuilt"),
        (
            `${result.computer.id} · `
            +
            `${result.computer.score} `
            +
            `${t("score")}`
        ),
        "success"
    );

    renderWorkshopPage();
}


/* =========================================================
   MÜŞTERİ TÜRÜ
========================================================= */

function getCustomerType(customer) {
    return CUSTOMER_TYPES.find(
        type =>
            type.id
            ===
            customer.typeId
    );
}


function getCustomerTypeName(customer) {
    const type =
        getCustomerType(
            customer
        );

    return (
        type?.names?.[
            getLanguage()
        ]
        ||
        customer.typeId
    );
}


/* =========================================================
   MÜŞTERİ SAYFASI
========================================================= */

function renderCustomersPage() {
    const content =
        document.getElementById(
            "page-content"
        );

    if (!content) {
        return;
    }

    const selectedCustomer =
        getCustomerById(
            runtime.selectedCustomerId
        );

    const selectedComputer =
        getBuiltPcById(
            runtime.selectedBuiltPcId
        );

    let requirementFailures = [];

    if (
        selectedCustomer
        &&
        selectedComputer
    ) {
        requirementFailures =
            getCustomerRequirementFailures(
                selectedCustomer,
                selectedComputer
            );
    }

    content.innerHTML =
        createPageHeader(
            t("customers"),
            t("customersDescription")
        )
        +
        `
            <div class="market-layout">
                <div class="customer-grid">
                    ${
                        gameState.customers.length > 0
                            ? gameState.customers
                                .map(
                                    customer => {
                                        const deadlinePercentage =
                                            clamp(
                                                customer.deadlineDays
                                                /
                                                Math.max(
                                                    1,
                                                    customer.originalDeadline
                                                )
                                                *
                                                100,
                                                0,
                                                100
                                            );

                                        const deadlineClass =
                                            deadlinePercentage
                                            <=
                                            30
                                                ? "danger"
                                                : deadlinePercentage
                                                <=
                                                55
                                                    ? "warning"
                                                    : "";

                                        const initials =
                                            customer.name
                                                .split(" ")
                                                .map(
                                                    part =>
                                                        part[0]
                                                )
                                                .join("")
                                                .slice(
                                                    0,
                                                    2
                                                );

                                        return `
                                            <article
                                                class="customer-card ${
                                                    runtime.selectedCustomerId
                                                    ===
                                                    customer.id
                                                        ? "selected"
                                                        : ""
                                                }"
                                                data-customer-id="${
                                                    customer.id
                                                }"
                                            >
                                                <div class="customer-header">
                                                    <div class="customer-avatar">
                                                        ${escapeHtml(
                                                            initials
                                                        )}
                                                    </div>

                                                    <div class="customer-name">
                                                        <strong>
                                                            ${escapeHtml(
                                                                customer.name
                                                            )}
                                                        </strong>

                                                        <span>
                                                            ${escapeHtml(
                                                                getCustomerTypeName(
                                                                    customer
                                                                )
                                                            )}
                                                        </span>
                                                    </div>

                                                    <span class="status-badge info">
                                                        ${formatMoney(
                                                            customer.payment
                                                        )}
                                                    </span>
                                                </div>

                                                <div class="customer-score-range">
                                                    <div>
                                                        <span>
                                                            ${t("minimumAccepted")}
                                                        </span>

                                                        <strong>
                                                            ${Math.floor(
                                                                customer.minimumScore
                                                                *
                                                                customer.tolerance
                                                            )}
                                                        </strong>
                                                    </div>

                                                    <div>
                                                        <span>
                                                            ${t("requestedScore")}
                                                        </span>

                                                        <strong>
                                                            ${customer.minimumScore}
                                                            –
                                                            ${customer.maximumScore}
                                                        </strong>
                                                    </div>
                                                </div>

                                                <div class="tag-list"
                                                     style="margin-top:10px;">
                                                    ${customer.requirements
                                                        .map(
                                                            requirement => `
                                                                <span class="small-tag">
                                                                    ${escapeHtml(
                                                                        getRequirementText(
                                                                            requirement
                                                                        )
                                                                    )}
                                                                </span>
                                                            `
                                                        )
                                                        .join("")}
                                                </div>

                                                <div class="deadline-bar">
                                                    <div class="deadline-header">
                                                        <span>
                                                            ${t("deadline")}
                                                        </span>

                                                        <span>
                                                            ${customer.deadlineDays}
                                                            ${t("day")}
                                                        </span>
                                                    </div>

                                                    <div class="deadline-track">
                                                        <div
                                                            class="deadline-fill ${deadlineClass}"
                                                            style="width:${
                                                                deadlinePercentage
                                                            }%;"
                                                        ></div>
                                                    </div>
                                                </div>
                                            </article>
                                        `;
                                    }
                                )
                                .join("")
                            : `
                                <div class="empty-state">
                                    <div class="empty-state-icon">
                                        ♙
                                    </div>

                                    <h3>${t("noSuitableComputer")}</h3>
                                </div>
                            `
                    }
                </div>

                <aside class="game-panel">
                    ${
                        selectedCustomer
                            ? `
                                <div class="panel-header">
                                    <div class="panel-title">
                                        <div class="panel-title-icon">
                                            ♙
                                        </div>

                                        <div>
                                            <h2>
                                                ${escapeHtml(
                                                    selectedCustomer.name
                                                )}
                                            </h2>

                                            <span class="panel-subtitle">
                                                ${escapeHtml(
                                                    getCustomerTypeName(
                                                        selectedCustomer
                                                    )
                                                )}
                                            </span>
                                        </div>
                                    </div>
                                </div>

                                <div class="detail-content">
                                    <div class="detail-row">
                                        <span>${t("requestedScore")}</span>
                                        <strong>
                                            ${selectedCustomer.minimumScore}
                                            –
                                            ${selectedCustomer.maximumScore}
                                        </strong>
                                    </div>

                                    <div class="detail-row">
                                        <span>${t("minimumAccepted")}</span>
                                        <strong>
                                            ${Math.floor(
                                                selectedCustomer.minimumScore
                                                *
                                                selectedCustomer.tolerance
                                            )}
                                        </strong>
                                    </div>

                                    <div class="detail-row">
                                        <span>${t("payment")}</span>
                                        <strong class="text-success">
                                            ${formatMoney(
                                                selectedCustomer.payment
                                            )}
                                        </strong>
                                    </div>

                                    <div class="detail-row">
                                        <span>${t("deadline")}</span>
                                        <strong>
                                            ${selectedCustomer.deadlineDays}
                                            ${t("day")}
                                        </strong>
                                    </div>

                                    <h3 style="margin-top:16px;">
                                        ${t("requirements")}
                                    </h3>

                                    <div class="tag-list">
                                        ${selectedCustomer.requirements
                                            .map(
                                                requirement => `
                                                    <span class="small-tag">
                                                        ${escapeHtml(
                                                            getRequirementText(
                                                                requirement
                                                            )
                                                        )}
                                                    </span>
                                                `
                                            )
                                            .join("")}
                                    </div>

                                    <h3 style="margin-top:18px;">
                                        ${t("deliveryComputer")}
                                    </h3>

                                    <select
                                        id="customer-computer-selection"
                                        class="game-select"
                                        style="width:100%;"
                                    >
                                        <option value="">
                                            ${t("selectPc")}
                                        </option>

                                        ${gameState.builtComputers
                                            .map(
                                                computer => `
                                                    <option
                                                        value="${
                                                            computer.id
                                                        }"
                                                        ${
                                                            runtime.selectedBuiltPcId
                                                            ===
                                                            computer.id
                                                                ? "selected"
                                                                : ""
                                                        }
                                                    >
                                                        ${escapeHtml(
                                                            computer.id
                                                        )}
                                                        ·
                                                        ${computer.score}
                                                        ${t("score")}
                                                        ·
                                                        ${formatMoney(
                                                            computer.value
                                                        )}
                                                    </option>
                                                `
                                            )
                                            .join("")}
                                    </select>

                                    ${
                                        selectedComputer
                                            ? `
                                                <div
                                                    class="compatibility-list"
                                                    style="margin-top:12px;"
                                                >
                                                    ${
                                                        requirementFailures.length
                                                        ===
                                                        0
                                                            ? `
                                                                <div class="compatibility-item success">
                                                                    <span>✓</span>

                                                                    <span>
                                                                        ${t(
                                                                            "customerRequirementsMet"
                                                                        )}
                                                                    </span>
                                                                </div>
                                                            `
                                                            : requirementFailures
                                                                .map(
                                                                    failure => `
                                                                        <div class="compatibility-item error">
                                                                            <span>×</span>

                                                                            <span>
                                                                                ${escapeHtml(
                                                                                    failure
                                                                                )}
                                                                            </span>
                                                                        </div>
                                                                    `
                                                                )
                                                                .join("")
                                                    }

                                                    <div class="detail-row">
                                                        <span>${t("score")}</span>
                                                        <strong>
                                                            ${selectedComputer.score}
                                                        </strong>
                                                    </div>

                                                    <div class="detail-row">
                                                        <span>${t("buildCost")}</span>
                                                        <strong>
                                                            ${formatMoney(
                                                                selectedComputer.cost
                                                            )}
                                                        </strong>
                                                    </div>

                                                    <div class="detail-row">
                                                        <span>${t("profit")}</span>
                                                        <strong class="${
                                                            selectedCustomer.payment
                                                            -
                                                            selectedComputer.cost
                                                            >=
                                                            0
                                                                ? "text-success"
                                                                : "text-danger"
                                                        }">
                                                            ${formatMoney(
                                                                selectedCustomer.payment
                                                                -
                                                                selectedComputer.cost
                                                            )}
                                                        </strong>
                                                    </div>
                                                </div>
                                            `
                                            : ""
                                    }

                                    <p
                                        class="text-muted"
                                        style="font-size:10px; line-height:1.5;"
                                    >
                                        ${t("customerPaymentRange")}
                                    </p>

                                    <button
                                        id="deliver-selected-computer"
                                        class="game-button primary full-width"
                                        type="button"
                                        ${
                                            !selectedComputer
                                                ? "disabled"
                                                : ""
                                        }
                                    >
                                        ${t("deliverPc")}
                                    </button>
                                </div>
                            `
                            : `
                                <div class="empty-state">
                                    <div class="empty-state-icon">
                                        ♙
                                    </div>

                                    <h3>${t("selectCustomer")}</h3>
                                </div>
                            `
                    }
                </aside>
            </div>
        `;

    document
        .querySelectorAll(
            "[data-customer-id]"
        )
        .forEach(
            card => {
                card.addEventListener(
                    "click",
                    () => {
                        runtime.selectedCustomerId =
                            card.dataset.customerId;

                        runtime.selectedBuiltPcId =
                            null;

                        renderCustomersPage();
                    }
                );
            }
        );

    document
        .getElementById(
            "customer-computer-selection"
        )
        ?.addEventListener(
            "change",
            event => {
                runtime.selectedBuiltPcId =
                    event.target.value
                    ||
                    null;

                renderCustomersPage();
            }
        );

    document
        .getElementById(
            "deliver-selected-computer"
        )
        ?.addEventListener(
            "click",
            handleCustomerDelivery
        );
}


/* =========================================================
   MANUEL MÜŞTERİ TESLİMATI
========================================================= */

async function handleCustomerDelivery() {
    const customer =
        getCustomerById(
            runtime.selectedCustomerId
        );

    const computer =
        getBuiltPcById(
            runtime.selectedBuiltPcId
        );

    if (
        !customer
        ||
        !computer
    ) {
        return;
    }

    const failures =
        getCustomerRequirementFailures(
            customer,
            computer
        );

    if (
        failures.length
        >
        0
    ) {
        await showInformationModal(
            t("customerRequirementsNotMet"),
            failures.join("\n"),
            "danger",
            "×"
        );

        return;
    }

    const minimumAccepted =
        Math.floor(
            customer.minimumScore
            *
            customer.tolerance
        );

    if (
        computer.score
        <
        minimumAccepted
    ) {
        await showInformationModal(
            t("rejected"),
            (
                `${t("minimumAccepted")}: `
                +
                `${minimumAccepted}`
            ),
            "danger",
            "×"
        );

        return;
    }

    const accepted =
        await showGameModal({
            title:
                t("deliverPc"),

            message:
                (
                    `${t("deliverQuestion")}\n\n`
                    +
                    `${customer.name}\n`
                    +
                    `${computer.id} · `
                    +
                    `${computer.score} `
                    +
                    `${t("score")}\n`
                    +
                    `${t("payment")}: `
                    +
                    `${formatMoney(
                        customer.payment
                    )}`
                ),

            icon:
                "€",

            type:
                "info",

            confirmText:
                t("yes"),

            cancelText:
                t("no")
        });

    if (!accepted) {
        return;
    }

    const result =
        deliverComputerToCustomer(
            customer.id,
            computer.id,
            {
                automated: false
            }
        );

    if (!result.success) {
        showToast(
            t("error"),
            result.message,
            "error"
        );

        return;
    }

    runtime.selectedCustomerId =
        null;

    runtime.selectedBuiltPcId =
        null;

    showToast(
        t("delivered"),
        (
            `${formatMoney(result.payout)} · `
            +
            `${t("reputation")} `
            +
            `${result.reputationChange >= 0 ? "+" : ""}`
            +
            `${result.reputationChange}`
        ),
        "success"
    );

    renderCustomersPage();
}


/* =========================================================
   PERSONEL OTOMASYON PANELİ
========================================================= */

function renderAutomationPanel() {
    const list =
        document.getElementById(
            "automation-list"
        );

    const status =
        document.getElementById(
            "automation-status"
        );

    const toggle =
        document.getElementById(
            "automation-toggle"
        );

    if (
        !list
        ||
        !status
        ||
        !toggle
    ) {
        return;
    }

    status.textContent =
        gameState.automationEnabled
            ? t("active")
            : t("inactive");

    toggle.classList.toggle(
        "active",
        gameState.automationEnabled
    );

    if (
        gameState.staff.length
        ===
        0
    ) {
        list.innerHTML = `
            <div class="empty-automation">
                <span>⚙</span>

                <p>${t("noStaff")}</p>
            </div>
        `;

        return;
    }

    const currentAbsoluteMinutes =
        getAbsoluteGameMinutes();

    list.innerHTML =
        gameState.staff
            .map(
                employee => {
                    const nextActionIn =
                        Math.max(
                            0,
                            employee.nextActionAt
                            -
                            currentAbsoluteMinutes
                        );

                    const interval =
                        getAutomationInterval(
                            employee
                        );

                    const progress =
                        clamp(
                            (
                                1
                                -
                                nextActionIn
                                /
                                Math.max(
                                    1,
                                    interval
                                )
                            )
                            *
                            100,
                            0,
                            100
                        );

                    const statusText =
                        employee.status
                        ===
                        "working"
                            ? t("working")
                            : employee.status
                            ===
                            "resting"
                                ? t("resting")
                                : employee.status
                                ===
                                "completed"
                                    ? t("completed")
                                    : t("idle");

                    return `
                        <div class="automation-worker">
                            <div class="worker-header">
                                <div class="worker-avatar">
                                    ${STAFF_ROLES[
                                        employee.role
                                    ]?.icon || "?"}
                                </div>

                                <div class="worker-info">
                                    <strong>
                                        ${escapeHtml(
                                            employee.name
                                        )}
                                    </strong>

                                    <span>
                                        ${escapeHtml(
                                            getStaffRoleName(
                                                employee.role
                                            )
                                        )}
                                    </span>
                                </div>

                                <span class="worker-status">
                                    ${escapeHtml(
                                        statusText
                                    )}
                                </span>
                            </div>

                            <div class="worker-progress">
                                <div class="worker-progress-info">
                                    <span>${t("energy")}</span>
                                    <span>
                                        ${Math.round(
                                            employee.energy
                                        )}%
                                    </span>
                                </div>

                                <div class="progress-track">
                                    <div
                                        class="progress-fill"
                                        style="width:${
                                            Math.round(
                                                employee.energy
                                            )
                                        }%;"
                                    ></div>
                                </div>
                            </div>

                            <div class="worker-progress">
                                <div class="worker-progress-info">
                                    <span>${t("automationTask")}</span>
                                    <span>
                                        ${Math.round(
                                            nextActionIn
                                        )}
                                        min
                                    </span>
                                </div>

                                <div class="progress-track">
                                    <div
                                        class="progress-fill"
                                        style="width:${
                                            progress
                                        }%;"
                                    ></div>
                                </div>
                            </div>

                            <div class="worker-task">
                                ${
                                    escapeHtml(
                                        employee.currentTask
                                        ||
                                        t(
                                            STAFF_ROLES[
                                                employee.role
                                            ]?.taskKey
                                        )
                                    )
                                }
                            </div>
                        </div>
                    `;
                }
            )
            .join("");
}

/* =========================================================
   PC SHOP EMPIRE
   GAME.JS — BÖLÜM 5
   PERSONEL, DÜKKÂNLAR, FİNANS, GELİŞTİRMELER
   BUTONLAR VE OYUNUN BAŞLATILMASI
========================================================= */


/* =========================================================
   SON ÇEVİRİLER
========================================================= */

Object.assign(
    translations.tr,
    {
        staffDescription:
            "Çalışanların kalitesi, deneyimi, enerjisi ve otomatik görevlerini yönet.",
        hiredEmployees: "Çalışan Personeller",
        availableRoles: "İşe Alınabilecek Görevler",
        hireCost: "İşe Alım Ücreti",
        trainingCost: "Eğitim Ücreti",
        automationInterval: "Görev Aralığı",
        minutes: "dakika",
        employeeCapacityFull:
            "Dükkânın personel kapasitesi dolu.",
        hireQuestion:
            "Bu görev için yeni bir çalışan işe alınsın mı?",
        trainingQuestion:
            "Seçili personele eğitim verilsin mi?",
        fireQuestion:
            "Seçili personel işten çıkarılsın mı?",
        employeeHired: "Yeni personel işe alındı.",
        employeeTrained: "Personel eğitimi tamamlandı.",
        employeeFired: "Personel işten çıkarıldı.",
        roleBenefit: "Görev Etkisi",
        nextTask: "Sonraki Görev",
        teamBonus: "Takım Bonusu",
        noEmployees:
            "Henüz çalışan personel bulunmuyor.",
        propertiesDescription:
            "Farklı büyüklüklerde mağazalar, kiralar, depo alanları ve sözleşmeler arasından seçim yap.",
        squareMeters: "Metrekare",
        requiredLevel: "Gerekli İtibar",
        moveCost: "Taşınma Maliyeti",
        currentContract: "Mevcut Sözleşme",
        propertyLocked: "Bu dükkân henüz kilitli.",
        sameProperty:
            "Zaten bu dükkânda bulunuyorsun.",
        selectContract:
            "Bir kira sözleşmesi seç.",
        financeDescription:
            "Günlük giderleri ve elektrik, internet, sigorta sağlayıcılarını yönet.",
        projectedExpenses: "Tahmini Gün Sonu Gideri",
        rawExpenses: "İndirim Öncesi Gider",
        providerOptions: "Hizmet Sağlayıcıları",
        outageRisk: "Kesinti Riski",
        marketBonus: "Pazar Teklifi Bonusu",
        automationBonus: "Otomasyon Bonusu",
        theftProtection: "Hırsızlık Koruması",
        warrantyProtection: "Garanti Koruması",
        perBuild: "Montaj Başına",
        selectProvider: "Sağlayıcıyı Seç",
        selectedProvider: "Seçili Sağlayıcı",
        upgradesDescription:
            "Kalıcı geliştirmeler satın alarak mağazanın kapasitesini, hızını ve verimliliğini artır.",
        upgradeLevel: "Geliştirme Seviyesi",
        nextLevelCost: "Sonraki Seviye Maliyeti",
        maximumLevel: "Maksimum seviye",
        purchaseUpgrade: "Geliştirmeyi Satın Al",
        upgradeQuestion:
            "Bu geliştirme satın alınsın mı?",
        upgradePurchased:
            "Geliştirme başarıyla satın alındı.",
        storageUpgrade: "Akıllı Depo Sistemi",
        storageUpgradeEffect:
            "Her seviyede depo kapasitesini 45 parça artırır.",
        workshopUpgrade: "Profesyonel Atölye",
        workshopUpgradeEffect:
            "Montaj puanını %4,5 ve bilgisayar değerini %2,5 artırır.",
        marketingUpgrade: "Dijital Pazarlama",
        marketingUpgradeEffect:
            "Pazar tekliflerini ve günlük müşteri sayısını artırır.",
        securityUpgrade: "Güvenlik Altyapısı",
        securityUpgradeEffect:
            "Hırsızlık ve depo kaybı zararlarını azaltır.",
        accountingUpgrade: "Muhasebe Yazılımı",
        accountingUpgradeEffect:
            "Günlük işletme giderlerine uygulanan indirimi artırır.",
        automationUpgrade: "Otomasyon Merkezi",
        automationUpgradeEffect:
            "Personel kapasitesini ve otomatik çalışma hızını artırır.",
        activityDescription:
            "Satın almaları, satışları, montajları, personel görevlerini ve günlük olayları incele.",
        allActivities: "Tüm Faaliyetler",
        salesActivities: "Satışlar",
        purchaseActivities: "Satın Almalar",
        buildActivities: "Montajlar",
        staffActivities: "Personel",
        financeActivities: "Finans",
        storeActivities: "Mağaza",
        dayActivities: "Günler",
        eventActivities: "Olaylar",
        clearActivity: "Faaliyet Kaydını Temizle",
        clearActivityQuestion:
            "Faaliyet kaydı tamamen temizlensin mi?",
        activityCleared: "Faaliyet kaydı temizlendi.",
        gameReady: "Oyun hazır.",
        saveSystemFixed:
            "Kayıt sistemi hazır. İlerleme uygulama kapatıldığında silinmez."
    }
);


Object.assign(
    translations.en,
    {
        staffDescription:
            "Manage employee quality, experience, energy and automated duties.",
        hiredEmployees: "Hired Employees",
        availableRoles: "Available Roles",
        hireCost: "Hiring Cost",
        trainingCost: "Training Cost",
        automationInterval: "Task Interval",
        minutes: "minutes",
        employeeCapacityFull:
            "The store's employee capacity is full.",
        hireQuestion:
            "Hire a new employee for this role?",
        trainingQuestion:
            "Train the selected employee?",
        fireQuestion:
            "Dismiss the selected employee?",
        employeeHired: "A new employee was hired.",
        employeeTrained: "Employee training completed.",
        employeeFired: "Employee dismissed.",
        roleBenefit: "Role Effect",
        nextTask: "Next Task",
        teamBonus: "Team Bonus",
        noEmployees:
            "There are no hired employees yet.",
        propertiesDescription:
            "Choose between stores with different sizes, rents, capacities and contracts.",
        squareMeters: "Square Metres",
        requiredLevel: "Required Reputation",
        moveCost: "Moving Cost",
        currentContract: "Current Contract",
        propertyLocked: "This store is still locked.",
        sameProperty:
            "You are already operating in this store.",
        selectContract:
            "Select a rental contract.",
        financeDescription:
            "Manage daily expenses and electricity, internet and insurance providers.",
        projectedExpenses: "Projected End-of-Day Expenses",
        rawExpenses: "Expenses Before Discounts",
        providerOptions: "Service Providers",
        outageRisk: "Outage Risk",
        marketBonus: "Market Offer Bonus",
        automationBonus: "Automation Bonus",
        theftProtection: "Theft Protection",
        warrantyProtection: "Warranty Protection",
        perBuild: "Per Assembly",
        selectProvider: "Select Provider",
        selectedProvider: "Selected Provider",
        upgradesDescription:
            "Purchase permanent upgrades to improve capacity, speed and efficiency.",
        upgradeLevel: "Upgrade Level",
        nextLevelCost: "Next Level Cost",
        maximumLevel: "Maximum level",
        purchaseUpgrade: "Purchase Upgrade",
        upgradeQuestion:
            "Purchase this upgrade?",
        upgradePurchased:
            "Upgrade purchased successfully.",
        storageUpgrade: "Smart Storage System",
        storageUpgradeEffect:
            "Increases storage capacity by 45 components per level.",
        workshopUpgrade: "Professional Workshop",
        workshopUpgradeEffect:
            "Increases assembly score by 4.5% and computer value by 2.5%.",
        marketingUpgrade: "Digital Marketing",
        marketingUpgradeEffect:
            "Increases market offers and daily customer arrivals.",
        securityUpgrade: "Security Infrastructure",
        securityUpgradeEffect:
            "Reduces theft and inventory-loss damage.",
        accountingUpgrade: "Accounting Software",
        accountingUpgradeEffect:
            "Increases the discount applied to daily operating expenses.",
        automationUpgrade: "Automation Center",
        automationUpgradeEffect:
            "Increases staff capacity and automation speed.",
        activityDescription:
            "Review purchases, sales, assemblies, employee tasks and daily events.",
        allActivities: "All Activities",
        salesActivities: "Sales",
        purchaseActivities: "Purchases",
        buildActivities: "Assemblies",
        staffActivities: "Staff",
        financeActivities: "Finance",
        storeActivities: "Store",
        dayActivities: "Days",
        eventActivities: "Events",
        clearActivity: "Clear Activity Log",
        clearActivityQuestion:
            "Permanently clear the activity log?",
        activityCleared: "Activity log cleared.",
        gameReady: "The game is ready.",
        saveSystemFixed:
            "The save system is ready. Progress is preserved when the application closes."
    }
);


Object.assign(
    translations.de,
    {
        staffDescription:
            "Verwalte Qualität, Erfahrung, Energie und automatische Aufgaben der Mitarbeiter.",
        hiredEmployees: "Eingestellte Mitarbeiter",
        availableRoles: "Verfügbare Aufgaben",
        hireCost: "Einstellungskosten",
        trainingCost: "Schulungskosten",
        automationInterval: "Aufgabenintervall",
        minutes: "Minuten",
        employeeCapacityFull:
            "Die Personalkapazität des Geschäfts ist voll.",
        hireQuestion:
            "Einen neuen Mitarbeiter für diese Aufgabe einstellen?",
        trainingQuestion:
            "Den ausgewählten Mitarbeiter schulen?",
        fireQuestion:
            "Den ausgewählten Mitarbeiter entlassen?",
        employeeHired: "Ein neuer Mitarbeiter wurde eingestellt.",
        employeeTrained: "Mitarbeiterschulung abgeschlossen.",
        employeeFired: "Mitarbeiter entlassen.",
        roleBenefit: "Aufgabeneffekt",
        nextTask: "Nächste Aufgabe",
        teamBonus: "Teambonus",
        noEmployees:
            "Es wurden noch keine Mitarbeiter eingestellt.",
        propertiesDescription:
            "Wähle zwischen Geschäften mit unterschiedlichen Größen, Mieten, Kapazitäten und Verträgen.",
        squareMeters: "Quadratmeter",
        requiredLevel: "Benötigter Ruf",
        moveCost: "Umzugskosten",
        currentContract: "Aktueller Vertrag",
        propertyLocked: "Dieses Geschäft ist noch gesperrt.",
        sameProperty:
            "Du betreibst bereits dieses Geschäft.",
        selectContract:
            "Wähle einen Mietvertrag.",
        financeDescription:
            "Verwalte tägliche Ausgaben sowie Strom-, Internet- und Versicherungsanbieter.",
        projectedExpenses: "Voraussichtliche Tagesausgaben",
        rawExpenses: "Ausgaben vor Rabatten",
        providerOptions: "Dienstanbieter",
        outageRisk: "Ausfallrisiko",
        marketBonus: "Marktangebotsbonus",
        automationBonus: "Automatisierungsbonus",
        theftProtection: "Diebstahlschutz",
        warrantyProtection: "Garantieschutz",
        perBuild: "Pro Montage",
        selectProvider: "Anbieter auswählen",
        selectedProvider: "Ausgewählter Anbieter",
        upgradesDescription:
            "Kaufe dauerhafte Verbesserungen für Kapazität, Geschwindigkeit und Effizienz.",
        upgradeLevel: "Verbesserungsstufe",
        nextLevelCost: "Kosten der nächsten Stufe",
        maximumLevel: "Maximale Stufe",
        purchaseUpgrade: "Verbesserung kaufen",
        upgradeQuestion:
            "Diese Verbesserung kaufen?",
        upgradePurchased:
            "Verbesserung erfolgreich gekauft.",
        storageUpgrade: "Intelligentes Lagersystem",
        storageUpgradeEffect:
            "Erhöht die Lagerkapazität pro Stufe um 45 Teile.",
        workshopUpgrade: "Professionelle Werkstatt",
        workshopUpgradeEffect:
            "Erhöht Montagepunkte um 4,5 % und Computerwert um 2,5 %.",
        marketingUpgrade: "Digitales Marketing",
        marketingUpgradeEffect:
            "Erhöht Marktangebote und tägliche Kundenzahl.",
        securityUpgrade: "Sicherheitsinfrastruktur",
        securityUpgradeEffect:
            "Reduziert Schäden durch Diebstahl und Lagerverlust.",
        accountingUpgrade: "Buchhaltungssoftware",
        accountingUpgradeEffect:
            "Erhöht den Rabatt auf tägliche Betriebskosten.",
        automationUpgrade: "Automatisierungszentrum",
        automationUpgradeEffect:
            "Erhöht Personalkapazität und Automatisierungsgeschwindigkeit.",
        activityDescription:
            "Prüfe Einkäufe, Verkäufe, Montagen, Personalaufgaben und tägliche Ereignisse.",
        allActivities: "Alle Aktivitäten",
        salesActivities: "Verkäufe",
        purchaseActivities: "Einkäufe",
        buildActivities: "Montagen",
        staffActivities: "Personal",
        financeActivities: "Finanzen",
        storeActivities: "Geschäft",
        dayActivities: "Tage",
        eventActivities: "Ereignisse",
        clearActivity: "Aktivitätsprotokoll leeren",
        clearActivityQuestion:
            "Das Aktivitätsprotokoll vollständig leeren?",
        activityCleared: "Aktivitätsprotokoll geleert.",
        gameReady: "Das Spiel ist bereit.",
        saveSystemFixed:
            "Das Speichersystem ist bereit. Der Fortschritt bleibt beim Schließen erhalten."
    }
);


/* =========================================================
   KAYIT SİSTEMİ DÜZELTMESİ
========================================================= */

runtime.gameStarted =
    runtime.gameStarted || false;


function saveGame(
    showNotification = false
) {
    if (
        !runtime.gameStarted
    ) {
        return false;
    }

    try {
        const saveState =
            document.getElementById(
                "auto-save-state"
            );

        if (saveState) {
            saveState.innerHTML = `
                <span class="save-dot"></span>
                <span>${t("saving")}</span>
            `;
        }

        localStorage.setItem(
            SAVE_KEY,
            JSON.stringify(
                gameState
            )
        );

        localStorage.setItem(
            SETTINGS_KEY,
            JSON.stringify({
                language:
                    gameState.language
            })
        );

        window.setTimeout(
            () => {
                if (saveState) {
                    saveState.innerHTML = `
                        <span class="save-dot"></span>
                        <span>${t("saved")}</span>
                    `;
                }
            },
            250
        );

        if (
            showNotification
        ) {
            showToast(
                t("gameSaved"),
                t("autoSaveDescription"),
                "success"
            );
        }

        return true;

    } catch (error) {
        console.error(
            "Save error:",
            error
        );

        return false;
    }
}


function loadGame() {
    const saveText =
        localStorage.getItem(
            SAVE_KEY
        );

    if (!saveText) {
        return false;
    }

    try {
        const loadedState =
            JSON.parse(
                saveText
            );

        if (
            !loadedState
            ||
            !isCompatibleSaveState(loadedState)
        ) {
            return false;
        }

        gameState =
            loadedState;

        runtime.gameStarted =
            true;

        gameState.calendar.minutes =
            clamp(
                gameState.calendar.minutes,
                DAY_START_MINUTES,
                DAY_END_MINUTES
            );

        gameState.paused =
            false;

        normalizeGameState();

        return true;

    } catch (error) {
        console.error(
            "Load error:",
            error
        );

        return false;
    }
}


function deleteSaveGame() {
    localStorage.removeItem(
        SAVE_KEY
    );

    runtime.gameStarted =
        false;

    runtime.selectedOfferId =
        null;

    runtime.selectedCustomerId =
        null;

    runtime.selectedBuiltPcId =
        null;

    runtime.selectedBuildParts = {};
}


function prepareNewGame(
    language = "en"
) {
    runtime.gameStarted =
        true;

    gameState =
        createInitialState(
            language
        );

    normalizeGameState();

    refreshMarket();

    generateCustomers(
        6
    );

    addActivity(
        t("storeOpened"),
        "store",
        "09:00"
    );

    runtime.currentPage =
        "dashboard";

    runtime.selectedOfferId =
        null;

    runtime.selectedCustomerId =
        null;

    runtime.selectedBuiltPcId =
        null;

    runtime.selectedBuildParts = {};

    saveGame(false);
}


/* =========================================================
   DİL DEĞİŞTİRME DÜZELTMESİ
========================================================= */

function changeLanguage(
    language
) {
    if (
        ![
            "tr",
            "en",
            "de"
        ].includes(
            language
        )
    ) {
        return;
    }

    gameState.language =
        language;

    document.documentElement.lang =
        language;

    localStorage.setItem(
        SETTINGS_KEY,
        JSON.stringify({
            ...readSettings(),
            language,
            activeSaveSlot:
                runtime.activeSaveSlot || 1
        })
    );

    document
        .querySelectorAll(
            "[data-i18n]"
        )
        .forEach(
            element => {
                const key =
                    element.dataset.i18n;

                element.textContent =
                    t(key);
            }
        );

    document
        .querySelectorAll(
            ".language-button"
        )
        .forEach(
            button => {
                button.classList.toggle(
                    "active",
                    button.dataset.language
                    ===
                    language
                );
            }
        );

    const descriptions = {
        tr:
            "Bilgisayar mağazanı kur, sistemler topla, personel yönet ve bir teknoloji imparatorluğu oluştur.",

        en:
            "Build your computer store, assemble systems, manage employees and create a technology empire.",

        de:
            "Baue dein Computergeschäft auf, montiere Systeme, verwalte Mitarbeiter und erschaffe ein Technologieimperium."
    };

    const description =
        document.getElementById(
            "start-description"
        );

    if (description) {
        description.textContent =
            descriptions[language];
    }

    if (
        runtime.gameStarted
    ) {
        saveGame(false);
    }

    if (
        !document
            .getElementById(
                "game-screen"
            )
            ?.classList.contains(
                "hidden"
            )
    ) {
        safeRender();
    }
}


/* =========================================================
   PERSONEL GÖREV AÇIKLAMASI
========================================================= */

function getRoleDescription(
    role
) {
    return (
        STAFF_ROLES[
            role
        ]?.description?.[
            getLanguage()
        ]
        ||
        ""
    );
}


function getEmployeeEffectText(
    employee
) {
    const quality =
        employee.quality;

    if (
        employee.role
        ===
        "sales"
    ) {
        const bonus =
            Math.min(
                4,
                quality * 0.035
            );

        return (
            `+%${bonus.toFixed(1)} `
            +
            `${t("payment")}`
        );
    }

    if (
        employee.role
        ===
        "technician"
    ) {
        const scoreBonus =
            quality * 0.22;

        return (
            `+%${scoreBonus.toFixed(1)} `
            +
            `${t("score")}`
        );
    }

    if (
        employee.role
        ===
        "buyer"
    ) {
        const discount =
            clamp(
                quality * 0.13,
                2,
                14
            );

        return (
            `-%${discount.toFixed(1)} `
            +
            `${t("price")}`
        );
    }

    if (
        employee.role
        ===
        "accountant"
    ) {
        const discount =
            quality * 0.08;

        return (
            `-%${discount.toFixed(1)} `
            +
            `${t("expenses") || t("todayExpenses")}`
        );
    }

    if (
        employee.role
        ===
        "manager"
    ) {
        const bonus =
            quality / 5;

        return (
            `+%${bonus.toFixed(1)} `
            +
            `${t("teamBonus")}`
        );
    }

    return "";
}


/* =========================================================
   PERSONEL SAYFASI
========================================================= */

function renderStaffPage() {
    const content =
        document.getElementById(
            "page-content"
        );

    if (!content) {
        return;
    }

    const roleEntries =
        Object.entries(
            STAFF_ROLES
        );

    content.innerHTML =
        createPageHeader(
            t("staff"),
            t("staffDescription")
        )
        +
        `
            <div class="content-grid three-columns">
                ${createMetricCard(
                    t("staff"),
                    `${gameState.staff.length}/${getStaffCapacity()}`,
                    t("staffCapacity")
                )}

                ${createMetricCard(
                    t("salaries"),
                    formatMoney(
                        calculateDailySalaries()
                    ),
                    t("dailyCost")
                )}

                ${createMetricCard(
                    t("automationPerformance"),
                    gameState.automationEnabled
                        ? t("active")
                        : t("inactive"),
                    t("automationDescription")
                )}
            </div>

            <div
                class="game-panel"
                style="margin-top:13px;"
            >
                <div class="panel-header">
                    <div class="panel-title">
                        <div class="panel-title-icon">
                            ＋
                        </div>

                        <div>
                            <h2>
                                ${t("availableRoles")}
                            </h2>

                            <span class="panel-subtitle">
                                ${gameState.staff.length}
                                /
                                ${getStaffCapacity()}
                            </span>
                        </div>
                    </div>
                </div>

                <div class="staff-grid panel-padding">
                    ${roleEntries
                        .map(
                            (
                                [
                                    role,
                                    information
                                ]
                            ) => `
                                <article class="staff-card">
                                    <div class="staff-top">
                                        <div class="staff-avatar">
                                            ${information.icon}
                                        </div>

                                        <div class="staff-main-info">
                                            <strong>
                                                ${escapeHtml(
                                                    getStaffRoleName(
                                                        role
                                                    )
                                                )}
                                            </strong>

                                            <span>
                                                ${t(
                                                    information.taskKey
                                                )}
                                            </span>
                                        </div>
                                    </div>

                                    <div class="staff-benefit">
                                        ${escapeHtml(
                                            getRoleDescription(
                                                role
                                            )
                                        )}
                                    </div>

                                    <div class="staff-stat-list">
                                        <div class="detail-row">
                                            <span>
                                                ${t("hireCost")}
                                            </span>

                                            <strong>
                                                ${formatMoney(
                                                    information.hiringCost
                                                )}
                                            </strong>
                                        </div>

                                        <div class="detail-row">
                                            <span>
                                                ${t("salary")}
                                            </span>

                                            <strong>
                                                ${formatMoney(
                                                    information.salaryBase
                                                )}
                                            </strong>
                                        </div>

                                        <div class="detail-row">
                                            <span>
                                                ${t("automationInterval")}
                                            </span>

                                            <strong>
                                                ${information.baseIntervalMinutes}
                                                ${t("minutes")}
                                            </strong>
                                        </div>
                                    </div>

                                    <div class="staff-actions">
                                        <button
                                            class="game-button primary"
                                            type="button"
                                            data-hire-role="${role}"
                                            ${
                                                gameState.staff.length
                                                >=
                                                getStaffCapacity()
                                                    ? "disabled"
                                                    : ""
                                            }
                                        >
                                            ${t("hire")}
                                        </button>
                                    </div>
                                </article>
                            `
                        )
                        .join("")}
                </div>
            </div>

            <div
                class="game-panel"
                style="margin-top:13px;"
            >
                <div class="panel-header">
                    <div class="panel-title">
                        <div class="panel-title-icon">
                            ♟
                        </div>

                        <div>
                            <h2>
                                ${t("hiredEmployees")}
                            </h2>

                            <span class="panel-subtitle">
                                ${gameState.staff.length}
                            </span>
                        </div>
                    </div>
                </div>

                ${
                    gameState.staff.length
                    >
                    0
                        ? `
                            <div class="staff-grid panel-padding">
                                ${gameState.staff
                                    .map(
                                        employee => {
                                            const interval =
                                                getAutomationInterval(
                                                    employee
                                                );

                                            const trainingPrice =
                                                Math.round(
                                                    STAFF_ROLES[
                                                        employee.role
                                                    ].trainingCost
                                                    *
                                                    (
                                                        1
                                                        +
                                                        employee.quality
                                                        /
                                                        120
                                                    )
                                                );

                                            return `
                                                <article class="staff-card">
                                                    <div class="staff-top">
                                                        <div class="staff-avatar">
                                                            ${STAFF_ROLES[
                                                                employee.role
                                                            ].icon}
                                                        </div>

                                                        <div class="staff-main-info">
                                                            <strong>
                                                                ${escapeHtml(
                                                                    employee.name
                                                                )}
                                                            </strong>

                                                            <span>
                                                                ${escapeHtml(
                                                                    getStaffRoleName(
                                                                        employee.role
                                                                    )
                                                                )}
                                                            </span>
                                                        </div>

                                                        <span class="status-badge success">
                                                            ${Math.round(
                                                                employee.energy
                                                            )}%
                                                        </span>
                                                    </div>

                                                    <div class="staff-stat-list">
                                                        ${createStaffStatRow(
                                                            t("quality"),
                                                            employee.quality
                                                        )}

                                                        ${createStaffStatRow(
                                                            t("experience"),
                                                            Math.min(
                                                                100,
                                                                employee.experience
                                                                /
                                                                5
                                                            )
                                                        )}

                                                        ${createStaffStatRow(
                                                            t("energy"),
                                                            employee.energy
                                                        )}

                                                        <div class="detail-row">
                                                            <span>
                                                                ${t("salary")}
                                                            </span>

                                                            <strong>
                                                                ${formatMoney(
                                                                    employee.salary
                                                                )}
                                                            </strong>
                                                        </div>

                                                        <div class="detail-row">
                                                            <span>
                                                                ${t("automationInterval")}
                                                            </span>

                                                            <strong>
                                                                ${interval}
                                                                ${t("minutes")}
                                                            </strong>
                                                        </div>

                                                        <div class="detail-row">
                                                            <span>
                                                                ${t("roleBenefit")}
                                                            </span>

                                                            <strong class="text-success">
                                                                ${escapeHtml(
                                                                    getEmployeeEffectText(
                                                                        employee
                                                                    )
                                                                )}
                                                            </strong>
                                                        </div>
                                                    </div>

                                                    <div class="staff-benefit">
                                                        <strong>
                                                            ${t("currentTask")}:
                                                        </strong>

                                                        <br>

                                                        ${escapeHtml(
                                                            employee.currentTask
                                                            ||
                                                            t("idle")
                                                        )}
                                                    </div>

                                                    <div class="staff-actions">
                                                        <button
                                                            class="game-button secondary"
                                                            type="button"
                                                            data-train-employee="${
                                                                employee.id
                                                            }"
                                                        >
                                                            ${t("train")}
                                                            ·
                                                            ${formatMoney(
                                                                trainingPrice
                                                            )}
                                                        </button>

                                                        <button
                                                            class="game-button danger"
                                                            type="button"
                                                            data-fire-employee="${
                                                                employee.id
                                                            }"
                                                        >
                                                            ${t("fire")}
                                                        </button>
                                                    </div>
                                                </article>
                                            `;
                                        }
                                    )
                                    .join("")}
                            </div>
                        `
                        : `
                            <div class="empty-state">
                                <div class="empty-state-icon">
                                    ♟
                                </div>

                                <h3>
                                    ${t("noEmployees")}
                                </h3>
                            </div>
                        `
                }
            </div>
        `;

    document
        .querySelectorAll(
            "[data-hire-role]"
        )
        .forEach(
            button => {
                button.addEventListener(
                    "click",
                    () => {
                        handleHireEmployee(
                            button.dataset.hireRole
                        );
                    }
                );
            }
        );

    document
        .querySelectorAll(
            "[data-train-employee]"
        )
        .forEach(
            button => {
                button.addEventListener(
                    "click",
                    () => {
                        handleTrainEmployee(
                            button.dataset.trainEmployee
                        );
                    }
                );
            }
        );

    document
        .querySelectorAll(
            "[data-fire-employee]"
        )
        .forEach(
            button => {
                button.addEventListener(
                    "click",
                    () => {
                        handleFireEmployee(
                            button.dataset.fireEmployee
                        );
                    }
                );
            }
        );
}


function createStaffStatRow(
    title,
    value
) {
    const normalizedValue =
        clamp(
            Math.round(
                value
            ),
            0,
            100
        );

    return `
        <div class="staff-stat-row">
            <span>
                ${escapeHtml(title)}
            </span>

            <div class="staff-stat-track">
                <div
                    class="staff-stat-fill"
                    style="width:${normalizedValue}%;"
                ></div>
            </div>

            <strong>
                ${normalizedValue}
            </strong>
        </div>
    `;
}


/* =========================================================
   PERSONEL BUTON İŞLEMLERİ
========================================================= */

async function handleHireEmployee(
    role
) {
    const information =
        STAFF_ROLES[
            role
        ];

    if (!information) {
        return;
    }

    const accepted =
        await showGameModal({
            title:
                getStaffRoleName(
                    role
                ),

            message:
                (
                    `${t("hireQuestion")}\n\n`
                    +
                    `${t("hireCost")}: `
                    +
                    `${formatMoney(
                        information.hiringCost
                    )}\n`
                    +
                    `${t("salary")}: `
                    +
                    `${formatMoney(
                        information.salaryBase
                    )}`
                ),

            icon:
                information.icon,

            type:
                "info",

            confirmText:
                t("yes"),

            cancelText:
                t("no")
        });

    if (!accepted) {
        return;
    }

    const result =
        hireEmployee(
            role
        );

    if (!result.success) {
        showToast(
            t("error"),
            result.message
            ||
            t("employeeCapacityFull"),
            "error"
        );

        return;
    }

    showToast(
        t("success"),
        (
            `${result.employee.name} · `
            +
            `${t("employeeHired")}`
        ),
        "success"
    );

    renderStaffPage();

    renderAutomationPanel();
}


async function handleTrainEmployee(
    employeeId
) {
    const employee =
        gameState.staff.find(
            item =>
                item.id
                ===
                employeeId
        );

    if (!employee) {
        return;
    }

    const information =
        STAFF_ROLES[
            employee.role
        ];

    const price =
        Math.round(
            information.trainingCost
            *
            (
                1
                +
                employee.quality
                /
                120
            )
        );

    const accepted =
        await showGameModal({
            title:
                t("train"),

            message:
                (
                    `${employee.name}\n\n`
                    +
                    `${t("trainingQuestion")}\n`
                    +
                    `${t("trainingCost")}: `
                    +
                    `${formatMoney(price)}`
                ),

            icon:
                "▲",

            type:
                "info",

            confirmText:
                t("yes"),

            cancelText:
                t("no")
        });

    if (!accepted) {
        return;
    }

    const result =
        trainEmployee(
            employeeId
        );

    if (!result.success) {
        showToast(
            t("error"),
            result.message
            ||
            t("insufficientMoney"),
            "error"
        );

        return;
    }

    showToast(
        t("success"),
        (
            `${t("employeeTrained")} `
            +
            `+${result.qualityIncrease} `
            +
            `${t("quality")}`
        ),
        "success"
    );

    renderStaffPage();
}


async function handleFireEmployee(
    employeeId
) {
    const employee =
        gameState.staff.find(
            item =>
                item.id
                ===
                employeeId
        );

    if (!employee) {
        return;
    }

    const accepted =
        await showGameModal({
            title:
                t("fire"),

            message:
                (
                    `${employee.name}\n\n`
                    +
                    `${t("fireQuestion")}\n`
                    +
                    `${t("reputation")}: -1`
                ),

            icon:
                "×",

            type:
                "danger",

            confirmText:
                t("yes"),

            cancelText:
                t("no")
        });

    if (!accepted) {
        return;
    }

    fireEmployee(
        employeeId
    );

    showToast(
        t("warning"),
        t("employeeFired"),
        "warning"
    );

    renderStaffPage();

    renderAutomationPanel();
}


/* =========================================================
   DÜKKÂN SÖZLEŞME SEÇİMLERİ
========================================================= */

runtime.propertyContracts =
    runtime.propertyContracts
    ||
    {};


/* =========================================================
   DÜKKÂN VE KİRA SAYFASI
========================================================= */

function renderPropertiesPage() {
    const content =
        document.getElementById(
            "page-content"
        );

    if (!content) {
        return;
    }

    const currentProperty =
        getPropertyById(
            gameState.propertyId
        );

    content.innerHTML =
        createPageHeader(
            t("properties"),
            t("propertiesDescription")
        )
        +
        `
            <div class="content-grid three-columns">
                ${createMetricCard(
                    t("currentProperty"),
                    t(
                        currentProperty.nameKey
                    ),
                    `${currentProperty.size} m²`
                )}

                ${createMetricCard(
                    t("rent"),
                    formatMoney(
                        calculateDailyRent()
                    ),
                    t("dailyCost")
                )}

                ${createMetricCard(
                    t("storageCapacity"),
                    formatNumber(
                        getStorageCapacity()
                    ),
                    t("capacity")
                )}
            </div>

            <div
                class="property-grid"
                style="margin-top:13px;"
            >
                ${PROPERTY_OPTIONS
                    .map(
                        property => {
                            const active =
                                property.id
                                ===
                                gameState.propertyId;

                            const unlocked =
                                gameState.reputation
                                >=
                                property.requiredReputation;

                            const selectedContract =
                                runtime
                                    .propertyContracts[
                                        property.id
                                    ]
                                ||
                                (
                                    active
                                        ? gameState.contractType
                                        : "monthly"
                                );

                            return `
                                <article
                                    class="property-card ${
                                        active
                                            ? "active"
                                            : ""
                                    }"
                                >
                                    <div class="property-visual">
                                        <div class="property-building">
                                        </div>
                                    </div>

                                    <div class="property-content">
                                        <div class="property-name">
                                            <h3>
                                                ${escapeHtml(
                                                    t(
                                                        property.nameKey
                                                    )
                                                )}
                                            </h3>

                                            <span class="property-price">
                                                ${formatMoney(
                                                    property.rent
                                                )}
                                            </span>
                                        </div>

                                        ${
                                            active
                                                ? `
                                                    <span class="status-badge success">
                                                        ${t("currentProperty")}
                                                    </span>
                                                `
                                                : unlocked
                                                    ? `
                                                        <span class="status-badge info">
                                                            ${t("available")}
                                                        </span>
                                                    `
                                                    : `
                                                        <span class="status-badge danger">
                                                            ${t("propertyLocked")}
                                                        </span>
                                                    `
                                        }

                                        <div class="property-features">
                                            <div class="property-feature">
                                                <span>
                                                    ${t("size")}
                                                </span>

                                                <strong>
                                                    ${property.size}
                                                    m²
                                                </strong>
                                            </div>

                                            <div class="property-feature">
                                                <span>
                                                    ${t("storageCapacity")}
                                                </span>

                                                <strong>
                                                    ${property.storageCapacity}
                                                </strong>
                                            </div>

                                            <div class="property-feature">
                                                <span>
                                                    ${t("staffCapacity")}
                                                </span>

                                                <strong>
                                                    ${property.staffCapacity}
                                                </strong>
                                            </div>

                                            <div class="property-feature">
                                                <span>
                                                    ${t("customerBonus")}
                                                </span>

                                                <strong>
                                                    +${property.customerBonus}
                                                </strong>
                                            </div>

                                            <div class="property-feature">
                                                <span>
                                                    ${t("workshopBonus")}
                                                </span>

                                                <strong>
                                                    +%${Math.round(
                                                        property.workshopBonus
                                                        *
                                                        100
                                                    )}
                                                </strong>
                                            </div>

                                            <div class="property-feature">
                                                <span>
                                                    ${t("requiredLevel")}
                                                </span>

                                                <strong>
                                                    ${property.requiredReputation}
                                                </strong>
                                            </div>

                                            <div class="property-feature">
                                                <span>
                                                    ${t("deposit")}
                                                </span>

                                                <strong>
                                                    ${formatMoney(
                                                        property.deposit
                                                    )}
                                                </strong>
                                            </div>
                                        </div>

                                        <div class="contract-options">
                                            ${[
                                                [
                                                    "monthly",
                                                    "monthly"
                                                ],
                                                [
                                                    "sixMonths",
                                                    "sixMonths"
                                                ],
                                                [
                                                    "yearly",
                                                    "yearly"
                                                ]
                                            ]
                                                .map(
                                                    (
                                                        [
                                                            contract,
                                                            translationKey
                                                        ]
                                                    ) => `
                                                        <button
                                                            class="contract-option ${
                                                                selectedContract
                                                                ===
                                                                contract
                                                                    ? "selected"
                                                                    : ""
                                                            }"
                                                            type="button"
                                                            data-property-contract="${
                                                                property.id
                                                            }"
                                                            data-contract-type="${
                                                                contract
                                                            }"
                                                        >
                                                            ${t(
                                                                translationKey
                                                            )}

                                                            <br>

                                                            -%${Math.round(
                                                                getContractDiscount(
                                                                    contract
                                                                )
                                                                *
                                                                100
                                                            )}
                                                        </button>
                                                    `
                                                )
                                                .join("")}
                                        </div>

                                        <button
                                            class="game-button ${
                                                active
                                                    ? "secondary"
                                                    : "primary"
                                            } full-width"
                                            type="button"
                                            data-move-property="${
                                                property.id
                                            }"
                                            style="margin-top:12px;"
                                            ${
                                                !unlocked
                                                ||
                                                active
                                                    ? "disabled"
                                                    : ""
                                            }
                                        >
                                            ${
                                                active
                                                    ? t("currentProperty")
                                                    : t("moveToProperty")
                                            }
                                        </button>
                                    </div>
                                </article>
                            `;
                        }
                    )
                    .join("")}
            </div>
        `;

    document
        .querySelectorAll(
            "[data-property-contract]"
        )
        .forEach(
            button => {
                button.addEventListener(
                    "click",
                    () => {
                        runtime
                            .propertyContracts[
                                button
                                    .dataset
                                    .propertyContract
                            ] =
                                button
                                    .dataset
                                    .contractType;

                        renderPropertiesPage();
                    }
                );
            }
        );

    document
        .querySelectorAll(
            "[data-move-property]"
        )
        .forEach(
            button => {
                button.addEventListener(
                    "click",
                    () => {
                        const propertyId =
                            button
                                .dataset
                                .moveProperty;

                        const contract =
                            runtime
                                .propertyContracts[
                                    propertyId
                                ]
                            ||
                            "monthly";

                        requestPropertyMove(
                            propertyId,
                            contract
                        );
                    }
                );
            }
        );
}


/* =========================================================
   SAĞLAYICI AÇIKLAMASI
========================================================= */

function createProviderFeatureText(
    providerType,
    provider
) {
    if (
        providerType
        ===
        "electricity"
    ) {
        return (
            `${t("outageRisk")}: `
            +
            `%${(
                provider.outageRisk
                *
                100
            ).toFixed(1)} · `
            +
            `${t("perBuild")}: `
            +
            `${formatMoney(
                provider.buildCost
            )}`
        );
    }

    if (
        providerType
        ===
        "internet"
    ) {
        return (
            `${t("marketBonus")}: `
            +
            `+${provider.marketBonus} · `
            +
            `${t("automationBonus")}: `
            +
            `+%${Math.round(
                provider.automationBonus
                *
                100
            )}`
        );
    }

    if (
        providerType
        ===
        "insurance"
    ) {
        return (
            `${t("theftProtection")}: `
            +
            `%${Math.round(
                provider.theftProtection
                *
                100
            )} · `
            +
            `${t("warrantyProtection")}: `
            +
            `%${Math.round(
                provider.warrantyProtection
                *
                100
            )}`
        );
    }

    return "";
}


/* =========================================================
   FİNANS SAYFASI
========================================================= */

function renderFinancePage() {
    const content =
        document.getElementById(
            "page-content"
        );

    if (!content) {
        return;
    }

    const operating =
        calculateDailyOperatingExpenses();

    content.innerHTML =
        createPageHeader(
            t("finance"),
            t("financeDescription")
        )
        +
        `
            <div class="content-grid four-columns">
                ${createMetricCard(
                    t("todayRevenue"),
                    formatMoney(
                        gameState.daily.revenue
                    ),
                    t("totalIncome")
                )}

                ${createMetricCard(
                    t("todayExpenses"),
                    formatMoney(
                        gameState.daily.expenses
                    ),
                    t("totalExpense")
                )}

                ${createMetricCard(
                    t("projectedExpenses"),
                    formatMoney(
                        operating.total
                    ),
                    t("dailyCost")
                )}

                ${createMetricCard(
                    t("accountingSaving"),
                    formatMoney(
                        operating.saving
                    ),
                    `-%${Math.round(
                        getAccountantDiscount()
                        *
                        100
                    )}`
                )}
            </div>

            <div
                class="finance-layout"
                style="margin-top:13px;"
            >
                <div class="game-panel">
                    <div class="panel-header">
                        <div class="panel-title">
                            <div class="panel-title-icon">
                                €
                            </div>

                            <div>
                                <h2>
                                    ${t("projectedExpenses")}
                                </h2>

                                <span class="panel-subtitle">
                                    ${formatGameDate()}
                                </span>
                            </div>
                        </div>
                    </div>

                    ${createFinanceRow(
                        "▰",
                        t("propertyRent"),
                        t("rent"),
                        operating.rent
                    )}

                    ${createFinanceRow(
                        "♟",
                        t("salaries"),
                        `${gameState.staff.length} ${t("employee")}`,
                        operating.salaries
                    )}

                    ${createFinanceRow(
                        "⚡",
                        t("serviceCosts"),
                        t("providerOptions"),
                        operating.services
                    )}

                    ${createFinanceRow(
                        "⚒",
                        t("maintenance"),
                        t("workshop"),
                        operating.maintenance
                    )}

                    ${createFinanceRow(
                        "≡",
                        t("administration"),
                        t("finance"),
                        operating.administration
                    )}

                    ${
                        operating.loanPayment > 0
                            ? createFinanceRow(
                                "↻",
                                t("emergencyLoan"),
                                `${gameState.finance.loanDaysRemaining} ${t("daysRemaining")}`,
                                operating.loanPayment
                            )
                            : ""
                    }

                    ${
                        operating.tax > 0
                            ? createFinanceRow(
                                "%",
                                t("tax"),
                                t("taxDay"),
                                operating.tax
                            )
                            : ""
                    }

                    <div class="finance-row">
                        <div class="finance-row-info">
                            <div class="finance-icon">
                                ↓
                            </div>

                            <div>
                                <strong>
                                    ${t("accountingSaving")}
                                </strong>

                                <span>
                                    ${t("accountant")}
                                </span>
                            </div>
                        </div>

                        <strong class="text-success">
                            -${formatMoney(
                                operating.saving
                            )}
                        </strong>
                    </div>

                    <div class="finance-row">
                        <div class="finance-row-info">
                            <div class="finance-icon">
                                =
                            </div>

                            <div>
                                <strong>
                                    ${t("projectedExpenses")}
                                </strong>

                                <span>
                                    ${t("rawExpenses")}:
                                    ${formatMoney(
                                        operating.rawTotal
                                    )}
                                </span>
                            </div>
                        </div>

                        <strong class="finance-value">
                            ${formatMoney(
                                operating.total
                            )}
                        </strong>
                    </div>
                </div>

                <aside class="game-panel">
                    <div class="panel-header">
                        <div class="panel-title">
                            <div class="panel-title-icon">
                                ⚙
                            </div>

                            <div>
                                <h2>
                                    ${t("providerOptions")}
                                </h2>

                                <span class="panel-subtitle">
                                    ${t("dailyCost")}
                                </span>
                            </div>
                        </div>
                    </div>

                    <div class="panel-padding">
                        ${Object.entries(
                            PROVIDERS
                        )
                            .map(
                                (
                                    [
                                        providerType,
                                        providers
                                    ]
                                ) => `
                                    <h3>
                                        ${t(
                                            providerType
                                        )}
                                    </h3>

                                    <div class="provider-grid">
                                        ${providers
                                            .map(
                                                provider => {
                                                    const selected =
                                                        gameState
                                                            .providers[
                                                                providerType
                                                            ]
                                                        ===
                                                        provider.id;

                                                    return `
                                                        <button
                                                            type="button"
                                                            class="provider-option ${
                                                                selected
                                                                    ? "selected"
                                                                    : ""
                                                            }"
                                                            data-provider-type="${
                                                                providerType
                                                            }"
                                                            data-provider-id="${
                                                                provider.id
                                                            }"
                                                        >
                                                            <div>
                                                                <strong>
                                                                    ${escapeHtml(
                                                                        provider.name
                                                                    )}
                                                                </strong>

                                                                <span>
                                                                    ${escapeHtml(
                                                                        createProviderFeatureText(
                                                                            providerType,
                                                                            provider
                                                                        )
                                                                    )}
                                                                </span>
                                                            </div>

                                                            <div class="provider-price">
                                                                ${formatMoney(
                                                                    provider.dailyCost
                                                                )}
                                                            </div>
                                                        </button>
                                                    `;
                                                }
                                            )
                                            .join("")}
                                    </div>
                                `
                            )
                            .join("")}
                    </div>
                </aside>
            </div>
        `;

    renderFinanceEnhancements(content, operating);

    document
        .querySelectorAll(
            "[data-provider-type]"
        )
        .forEach(
            button => {
                button.addEventListener(
                    "click",
                    () => {
                        changeProvider(
                            button.dataset.providerType,
                            button.dataset.providerId
                        );

                        renderFinancePage();
                    }
                );
            }
        );
}


function createFinanceRow(
    icon,
    title,
    subtitle,
    value
) {
    return `
        <div class="finance-row">
            <div class="finance-row-info">
                <div class="finance-icon">
                    ${icon}
                </div>

                <div>
                    <strong>
                        ${escapeHtml(title)}
                    </strong>

                    <span>
                        ${escapeHtml(subtitle)}
                    </span>
                </div>
            </div>

            <strong class="finance-value">
                ${formatMoney(value)}
            </strong>
        </div>
    `;
}


/* =========================================================
   GELİŞTİRME TANIMLARI
========================================================= */

const UPGRADE_DEFINITIONS = {
    storage: {
        nameKey:
            "storageUpgrade",

        effectKey:
            "storageUpgradeEffect",

        icon:
            "▦",

        baseCost:
            950,

        maximumLevel:
            10
    },

    workshop: {
        nameKey:
            "workshopUpgrade",

        effectKey:
            "workshopUpgradeEffect",

        icon:
            "⚒",

        baseCost:
            1450,

        maximumLevel:
            10
    },

    marketing: {
        nameKey:
            "marketingUpgrade",

        effectKey:
            "marketingUpgradeEffect",

        icon:
            "◆",

        baseCost:
            1250,

        maximumLevel:
            10
    },

    security: {
        nameKey:
            "securityUpgrade",

        effectKey:
            "securityUpgradeEffect",

        icon:
            "▣",

        baseCost:
            1100,

        maximumLevel:
            10
    },

    accounting: {
        nameKey:
            "accountingUpgrade",

        effectKey:
            "accountingUpgradeEffect",

        icon:
            "€",

        baseCost:
            1350,

        maximumLevel:
            10
    },

    automation: {
        nameKey:
            "automationUpgrade",

        effectKey:
            "automationUpgradeEffect",

        icon:
            "⚙",

        baseCost:
            1750,

        maximumLevel:
            10
    }
};


function getUpgradeCost(
    upgradeKey
) {
    const definition =
        UPGRADE_DEFINITIONS[
            upgradeKey
        ];

    const level =
        gameState.upgrades[
            upgradeKey
        ];

    return Math.round(
        definition.baseCost
        *
        Math.pow(
            1.72,
            level
        )
    );
}


/* =========================================================
   GELİŞTİRME SAYFASI
========================================================= */

function renderUpgradesPage() {
    const content =
        document.getElementById(
            "page-content"
        );

    if (!content) {
        return;
    }

    content.innerHTML =
        createPageHeader(
            t("upgrades"),
            t("upgradesDescription")
        )
        +
        `
            <div class="staff-grid">
                ${Object.entries(
                    UPGRADE_DEFINITIONS
                )
                    .map(
                        (
                            [
                                key,
                                definition
                            ]
                        ) => {
                            const level =
                                gameState.upgrades[
                                    key
                                ];

                            const maximumReached =
                                level
                                >=
                                definition.maximumLevel;

                            const price =
                                getUpgradeCost(
                                    key
                                );

                            return `
                                <article class="staff-card">
                                    <div class="staff-top">
                                        <div class="staff-avatar">
                                            ${definition.icon}
                                        </div>

                                        <div class="staff-main-info">
                                            <strong>
                                                ${t(
                                                    definition.nameKey
                                                )}
                                            </strong>

                                            <span>
                                                ${t("upgradeLevel")}:
                                                ${level}
                                                /
                                                ${definition.maximumLevel}
                                            </span>
                                        </div>
                                    </div>

                                    <div class="staff-benefit">
                                        ${t(
                                            definition.effectKey
                                        )}
                                    </div>

                                    <div class="staff-stat-list">
                                        <div class="staff-stat-row">
                                            <span>
                                                ${t("upgradeLevel")}
                                            </span>

                                            <div class="staff-stat-track">
                                                <div
                                                    class="staff-stat-fill"
                                                    style="width:${
                                                        level
                                                        /
                                                        definition.maximumLevel
                                                        *
                                                        100
                                                    }%;"
                                                ></div>
                                            </div>

                                            <strong>
                                                ${level}
                                            </strong>
                                        </div>

                                        <div class="detail-row">
                                            <span>
                                                ${
                                                    maximumReached
                                                        ? t("maximumLevel")
                                                        : t("nextLevelCost")
                                                }
                                            </span>

                                            <strong>
                                                ${
                                                    maximumReached
                                                        ? "MAX"
                                                        : formatMoney(
                                                            price
                                                        )
                                                }
                                            </strong>
                                        </div>
                                    </div>

                                    <div class="staff-actions">
                                        <button
                                            class="game-button primary"
                                            type="button"
                                            data-buy-upgrade="${key}"
                                            ${
                                                maximumReached
                                                    ? "disabled"
                                                    : ""
                                            }
                                        >
                                            ${t("purchaseUpgrade")}
                                        </button>
                                    </div>
                                </article>
                            `;
                        }
                    )
                    .join("")}
            </div>
        `;

    document
        .querySelectorAll(
            "[data-buy-upgrade]"
        )
        .forEach(
            button => {
                button.addEventListener(
                    "click",
                    () => {
                        handleBuyUpgrade(
                            button.dataset.buyUpgrade
                        );
                    }
                );
            }
        );
}


/* =========================================================
   GELİŞTİRME SATIN ALMA
========================================================= */

async function handleBuyUpgrade(
    upgradeKey
) {
    const definition =
        UPGRADE_DEFINITIONS[
            upgradeKey
        ];

    if (!definition) {
        return;
    }

    const currentLevel =
        gameState.upgrades[
            upgradeKey
        ];

    if (
        currentLevel
        >=
        definition.maximumLevel
    ) {
        return;
    }

    const price =
        getUpgradeCost(
            upgradeKey
        );

    const accepted =
        await showGameModal({
            title:
                t(
                    definition.nameKey
                ),

            message:
                (
                    `${t("upgradeQuestion")}\n\n`
                    +
                    `${t("upgradeLevel")}: `
                    +
                    `${currentLevel} → `
                    +
                    `${currentLevel + 1}\n`
                    +
                    `${t("price")}: `
                    +
                    `${formatMoney(price)}`
                ),

            icon:
                definition.icon,

            type:
                "info",

            confirmText:
                t("yes"),

            cancelText:
                t("no")
        });

    if (!accepted) {
        return;
    }

    if (
        gameState.money
        <
        price
    ) {
        showToast(
            t("error"),
            t("insufficientMoney"),
            "error"
        );

        return;
    }

    registerExpense(
        price
    );

    gameState.upgrades[
        upgradeKey
    ] += 1;

    addActivity(
        getLanguage() === "de"
            ? (
                `${t(definition.nameKey)} `
                +
                `Stufe ${
                    gameState.upgrades[
                        upgradeKey
                    ]
                } erreicht.`
            )
            : getLanguage() === "en"
                ? (
                    `${t(definition.nameKey)} reached `
                    +
                    `level ${
                        gameState.upgrades[
                            upgradeKey
                        ]
                    }.`
                )
                : (
                    `${t(definition.nameKey)} `
                    +
                    `${gameState.upgrades[
                        upgradeKey
                    ]}. seviyeye yükseltildi.`
                ),
        "store"
    );

    saveGame(false);

    showToast(
        t("success"),
        t("upgradePurchased"),
        "success"
    );

    renderUpgradesPage();

    renderAutomationPanel();
}


/* =========================================================
   FAALİYET FİLTRESİ
========================================================= */

runtime.activityFilter =
    runtime.activityFilter
    ||
    "all";


/* =========================================================
   FAALİYET SAYFASI
========================================================= */

function renderActivityPage() {
    const content =
        document.getElementById(
            "page-content"
        );

    if (!content) {
        return;
    }

    const filters = [
        [
            "all",
            "allActivities"
        ],
        [
            "sale",
            "salesActivities"
        ],
        [
            "purchase",
            "purchaseActivities"
        ],
        [
            "build",
            "buildActivities"
        ],
        [
            "staff",
            "staffActivities"
        ],
        [
            "finance",
            "financeActivities"
        ],
        [
            "store",
            "storeActivities"
        ],
        [
            "day",
            "dayActivities"
        ],
        [
            "event",
            "eventActivities"
        ]
    ];

    const activities =
        runtime.activityFilter
        ===
        "all"
            ? gameState.activity
            : gameState.activity.filter(
                activity =>
                    activity.type
                    ===
                    runtime.activityFilter
            );

    content.innerHTML =
        createPageHeader(
            t("activity"),
            t("activityDescription"),
            `
                <button
                    id="clear-activity-button"
                    class="game-button danger"
                    type="button"
                >
                    ${t("clearActivity")}
                </button>
            `
        )
        +
        `
            <div class="filter-bar">
                ${filters
                    .map(
                        (
                            [
                                filter,
                                translationKey
                            ]
                        ) => `
                            <button
                                type="button"
                                class="game-button ${
                                    runtime.activityFilter
                                    ===
                                    filter
                                        ? "primary"
                                        : "secondary"
                                }"
                                data-activity-filter="${
                                    filter
                                }"
                            >
                                ${t(
                                    translationKey
                                )}
                            </button>
                        `
                    )
                    .join("")}
            </div>

            <div class="game-panel panel-padding">
                <div class="activity-list">
                    ${
                        activities.length
                        >
                        0
                            ? activities
                                .map(
                                    activity => `
                                        <div class="activity-item">
                                            <span class="activity-time">
                                                ${escapeHtml(
                                                    activity.time
                                                )}
                                            </span>

                                            <span class="activity-icon">
                                                ${getActivityIcon(
                                                    activity.type
                                                )}
                                            </span>

                                            <span class="activity-text">
                                                <strong>
                                                    ${escapeHtml(
                                                        activity.date
                                                    )}
                                                </strong>

                                                <br>

                                                ${escapeHtml(
                                                    activity.message
                                                )}
                                            </span>
                                        </div>
                                    `
                                )
                                .join("")
                            : `
                                <div class="empty-state">
                                    <div class="empty-state-icon">
                                        ≡
                                    </div>

                                    <h3>
                                        ${t("noActivity")}
                                    </h3>
                                </div>
                            `
                    }
                </div>
            </div>
        `;

    document
        .querySelectorAll(
            "[data-activity-filter]"
        )
        .forEach(
            button => {
                button.addEventListener(
                    "click",
                    () => {
                        runtime.activityFilter =
                            button
                                .dataset
                                .activityFilter;

                        renderActivityPage();
                    }
                );
            }
        );

    document
        .getElementById(
            "clear-activity-button"
        )
        ?.addEventListener(
            "click",
            clearActivityLog
        );
}


function getActivityIcon(
    type
) {
    const icons = {
        sale: "€",
        purchase: "◆",
        build: "⚒",
        staff: "♟",
        finance: "%",
        store: "▰",
        day: "☾",
        event: "!",
        customer: "♙",
        info: "i"
    };

    return (
        icons[type]
        ||
        "i"
    );
}


async function clearActivityLog() {
    const accepted =
        await showGameModal({
            title:
                t("clearActivity"),

            message:
                t("clearActivityQuestion"),

            icon:
                "×",

            type:
                "danger",

            confirmText:
                t("yes"),

            cancelText:
                t("no")
        });

    if (!accepted) {
        return;
    }

    gameState.activity = [];

    saveGame(false);

    showToast(
        t("success"),
        t("activityCleared"),
        "success"
    );

    renderActivityPage();
}


/* =========================================================
   OTOMASYON BUTONU
========================================================= */

function handleAutomationToggle() {
    toggleStaffAutomation();

    showToast(
        t("staffAutomation"),
        gameState.automationEnabled
            ? t("automationOn")
            : t("automationOff"),
        gameState.automationEnabled
            ? "success"
            : "warning"
    );

    renderAutomationPanel();
}


/* =========================================================
   BÜTÜN SABİT BUTONLARI BAĞLAMA
========================================================= */

function bindPermanentInterfaceEvents() {
    document
        .getElementById(
            "new-game-button"
        )
        ?.addEventListener(
            "click",
            requestNewGame
        );

    document
        .getElementById(
            "continue-button"
        )
        ?.addEventListener(
            "click",
            () => continueGame()
        );

    document
        .getElementById(
            "load-slot-button"
        )
        ?.addEventListener(
            "click",
            () => showSaveSlotWindow("manage")
        );

    document
        .getElementById(
            "start-settings-button"
        )
        ?.addEventListener(
            "click",
            showSettingsWindow
        );

    document
        .getElementById("exit-game-button")
        ?.addEventListener("click", requestExitGame);

    document
        .getElementById(
            "top-settings-button"
        )
        ?.addEventListener(
            "click",
            showSettingsWindow
        );

    document
        .getElementById(
            "return-menu-button"
        )
        ?.addEventListener(
            "click",
            requestReturnToMenu
        );

    document
        .getElementById(
            "end-day-button"
        )
        ?.addEventListener(
            "click",
            requestEndDay
        );

    document
        .getElementById(
            "next-day-button"
        )
        ?.addEventListener(
            "click",
            async () => {
                startNextDay();

                await checkFinancialFailure();
            }
        );

    document
        .getElementById(
            "pause-button"
        )
        ?.addEventListener(
            "click",
            toggleGamePause
        );

    document
        .querySelectorAll(
            ".speed-button"
        )
        .forEach(
            button => {
                button.addEventListener(
                    "click",
                    () => {
                        setGameSpeed(
                            Number(
                                button.dataset.speed
                            )
                        );
                    }
                );
            }
        );

    document
        .querySelectorAll(
            ".nav-button"
        )
        .forEach(
            button => {
                button.addEventListener(
                    "click",
                    () => {
                        navigateToPage(
                            button.dataset.page
                        );
                    }
                );
            }
        );

    document
        .querySelectorAll(
            ".language-button[data-language]"
        )
        .forEach(
            button => {
                button.addEventListener(
                    "click",
                    () => {
                        changeLanguage(
                            button.dataset.language
                        );
                    }
                );
            }
        );

    document
        .getElementById(
            "automation-toggle"
        )
        ?.addEventListener(
            "click",
            handleAutomationToggle
        );

    document.addEventListener(
        "keydown",
        event => {
            if (event.key === "F11") {
                event.preventDefault();
                window.pcShopDesktop?.toggleFullscreen();
                return;
            }

            if (
                event.key
                ===
                "Escape"
            ) {
                const modal =
                    document.getElementById(
                        "game-modal"
                    );

                const cancelButton =
                    document.getElementById(
                        "modal-cancel-button"
                    );

                if (
                    modal
                    &&
                    !modal.classList.contains(
                        "hidden"
                    )
                    &&
                    cancelButton
                    &&
                    !cancelButton.classList.contains(
                        "hidden"
                    )
                ) {
                    cancelButton.click();
                }
            }
        }
    );
}


/* =========================================================
   UYGULAMA KAPANIRKEN KAYDET
========================================================= */

window.addEventListener(
    "beforeunload",
    () => {
        if (
            runtime.gameStarted
        ) {
            saveGame(false);
        }
    }
);


/* =========================================================
   OYUNU BAŞLATMA
========================================================= */

document.addEventListener(
    "DOMContentLoaded",
    () => {
        const savedLanguage =
            loadSavedLanguage();

        runtime.activeSaveSlot = clamp(
            Number(readSettings().activeSaveSlot) || 1,
            1,
            SAVE_SLOT_COUNT
        );

        gameState =
            createInitialState(
                savedLanguage
            );

        runtime.gameStarted =
            false;

        normalizeGameState();

        bindPermanentInterfaceEvents();

        changeLanguage(
            savedLanguage
        );

        showStartScreen();

        updateTopBar();

        renderAutomationPanel();

        const continueButton =
            document.getElementById(
                "continue-button"
            );

        if (continueButton) {
            continueButton.disabled =
                !hasSaveGame();
        }

        console.log(
            `PC Shop Empire ${APP_VERSION} ready.`
        );
    }
);


/* =========================================================
   PC SHOP EMPIRE 1.1.6 OPERASYON KATMANI
   Çoklu kayıt, kurtarma ekonomisi ve operasyon olayları
========================================================= */

Object.assign(translations.tr, {
    exitGame: "Oyundan Çık",
    exitGameQuestion: "Oyunu kapatmak istediğine emin misin? Aktif kayıt önce kaydedilecek.",
    saveSlot: "Kayıt Yuvası",
    emptySlot: "Boş yuva",
    loadSlot: "Yükle",
    createSlot: "Yeni Oyun",
    deleteSlot: "Sil",
    lastSaved: "Son kayıt",
    slotManagerDescription: "Üç bağımsız kariyer kaydından birini seç.",
    overwriteSlotQuestion: "Bu yuvadaki mevcut ilerleme silinip yeni oyun başlatılsın mı?",
    actions: "İşlemler",
    sellOne: "1 Sat",
    sellAll: "Tümünü Sat",
    unit: "adet",
    partResold: "Parça geri satıldı",
    compatibleSet: "Uyumlu Set",
    emergencyLoan: "Kurtarma Kredisi",
    emergencyLoanDescription: "Kasa sıkıştığında 7 günlük vadeyle €3.500 işletme sermayesi sağlar.",
    takeLoan: "€3.500 Kredi Kullan",
    loanTaken: "Kurtarma kredisi kasaya aktarıldı.",
    daysRemaining: "gün kaldı",
    detailedExpenses: "Ayrıntılı İşletme Giderleri",
    electricityUsage: "Elektrik ve montaj tüketimi",
    internetService: "İnternet ve bulut hizmeti",
    insuranceService: "Sigorta primi",
    waterCleaning: "Su, temizlik ve atık",
    facilityMaintenance: "Bina ve tesis bakımı",
    workshopWear: "Atölye sarf ve aşınma",
    equipmentDepreciation: "Ekipman amortismanı",
    permitsSoftware: "Ruhsat ve yazılım",
    paymentFees: "Ödeme komisyonları",
    officeSupplies: "Ofis sarf malzemeleri",
    operationsCenter: "Canlı Operasyon Akışı",
    operationsCalm: "Mağaza şu anda normal çalışıyor.",
    nextRiskWindow: "Sonraki risk kontrolü",
    operationalIncident: "Operasyon Olayı",
    chooseResponse: "Bir müdahale seç",
    incidentResolved: "Olay çözüldü",
    powerIncidentTitle: "Şebeke Kesintisi",
    powerIncidentText: "Elektrik kesildi; atölye ve satış terminalleri durdu.",
    theftIncidentTitle: "Şüpheli Depo Hareketi",
    theftIncidentText: "Güvenlik sistemi depoda olası hırsızlık tespit etti.",
    workshopIncidentTitle: "Atölye Ekipmanı Arızası",
    workshopIncidentText: "Montaj tezgâhındaki ekipman acil onarım istiyor.",
    repairIncidentTitle: "Acil Tamir Talebi",
    repairIncidentText: "Bir müşteri aynı gün teslim bilgisayar tamiri istiyor.",
    rushIncidentTitle: "Geciken Tedarik",
    rushIncidentText: "Uyumlu set teslimatı gecikti; hızlı kurye seçeneği mevcut.",
    useGenerator: "Jeneratörü Çalıştır",
    waitOutage: "Kesintiyi Bekle",
    secureWarehouse: "Güvenliği Çağır",
    claimInsurance: "Hasarı Sigortaya Bildir",
    professionalRepair: "Servis Çağır",
    technicianRepair: "Teknisyen Onarsın",
    acceptRepair: "Tamiri Kabul Et",
    declineRepair: "İşi Reddet",
    expressCourier: "Hızlı Kurye",
    delayDelivery: "Teslimatı Ertele",
    debtRecoveryHint: "Parçaları envanterden geri satabilir veya Finans sayfasından kurtarma kredisi kullanabilirsin.",
    restructuring: "Acil Yapılandırma",
    restructuringQuestion: "Borç limiti aşıldı. Mağazayı kapatmak yerine mali yapılandırma uygulansın mı?"
});

Object.assign(translations.en, {
    exitGame: "Exit Game",
    exitGameQuestion: "Exit the game? The active slot will be saved first.",
    saveSlot: "Save Slot",
    emptySlot: "Empty slot",
    loadSlot: "Load",
    createSlot: "New Game",
    deleteSlot: "Delete",
    lastSaved: "Last saved",
    slotManagerDescription: "Choose one of three independent careers.",
    overwriteSlotQuestion: "Replace this slot with a new game?",
    actions: "Actions",
    sellOne: "Sell 1",
    sellAll: "Sell All",
    unit: "unit",
    partResold: "Component resold",
    compatibleSet: "Compatible Set",
    emergencyLoan: "Recovery Loan",
    emergencyLoanDescription: "Provides €3,500 working capital over seven days.",
    takeLoan: "Borrow €3,500",
    loanTaken: "Recovery funds were added to the cash balance.",
    daysRemaining: "days remaining",
    detailedExpenses: "Detailed Operating Expenses",
    electricityUsage: "Electricity and assembly usage",
    internetService: "Internet and cloud service",
    insuranceService: "Insurance premium",
    waterCleaning: "Water, cleaning and waste",
    facilityMaintenance: "Facility maintenance",
    workshopWear: "Workshop supplies and wear",
    equipmentDepreciation: "Equipment depreciation",
    permitsSoftware: "Permits and software",
    paymentFees: "Payment processing fees",
    officeSupplies: "Office supplies",
    operationsCenter: "Live Operations Flow",
    operationsCalm: "The store is operating normally.",
    nextRiskWindow: "Next risk check",
    operationalIncident: "Operational Incident",
    chooseResponse: "Choose a response",
    incidentResolved: "Incident resolved",
    powerIncidentTitle: "Grid Outage",
    powerIncidentText: "Power failed; the workshop and sales terminals stopped.",
    theftIncidentTitle: "Suspicious Warehouse Activity",
    theftIncidentText: "Security detected a possible warehouse theft.",
    workshopIncidentTitle: "Workshop Equipment Failure",
    workshopIncidentText: "Assembly equipment requires an urgent repair.",
    repairIncidentTitle: "Urgent Repair Request",
    repairIncidentText: "A customer needs a same-day computer repair.",
    rushIncidentTitle: "Delayed Supply",
    rushIncidentText: "A compatible-set delivery is late; express courier is available.",
    useGenerator: "Start Generator",
    waitOutage: "Wait for Power",
    secureWarehouse: "Call Security",
    claimInsurance: "File Insurance Claim",
    professionalRepair: "Call Service",
    technicianRepair: "Technician Repairs",
    acceptRepair: "Accept Repair",
    declineRepair: "Decline Job",
    expressCourier: "Express Courier",
    delayDelivery: "Delay Delivery",
    debtRecoveryHint: "Resell parts from inventory or use a recovery loan on the Finance page.",
    restructuring: "Emergency Restructuring",
    restructuringQuestion: "The debt limit was exceeded. Restructure instead of closing the store?"
});

Object.assign(translations.de, {
    exitGame: "Spiel beenden",
    exitGameQuestion: "Spiel beenden? Der aktive Spielstand wird vorher gespeichert.",
    saveSlot: "Spielstand",
    emptySlot: "Leerer Platz",
    loadSlot: "Laden",
    createSlot: "Neues Spiel",
    deleteSlot: "Löschen",
    lastSaved: "Zuletzt gespeichert",
    slotManagerDescription: "Wähle eine von drei unabhängigen Karrieren.",
    overwriteSlotQuestion: "Diesen Spielstand mit einem neuen Spiel ersetzen?",
    actions: "Aktionen",
    sellOne: "1 verkaufen",
    sellAll: "Alle verkaufen",
    unit: "Stück",
    partResold: "Komponente weiterverkauft",
    compatibleSet: "Kompatibles Set",
    emergencyLoan: "Rettungskredit",
    emergencyLoanDescription: "Stellt €3.500 Betriebskapital für sieben Tage bereit.",
    takeLoan: "€3.500 leihen",
    loanTaken: "Der Rettungskredit wurde der Kasse gutgeschrieben.",
    daysRemaining: "Tage verbleibend",
    detailedExpenses: "Detaillierte Betriebskosten",
    electricityUsage: "Strom und Montageverbrauch",
    internetService: "Internet und Cloud",
    insuranceService: "Versicherungsprämie",
    waterCleaning: "Wasser, Reinigung und Abfall",
    facilityMaintenance: "Gebäudewartung",
    workshopWear: "Werkstattverschleiß",
    equipmentDepreciation: "Geräteabschreibung",
    permitsSoftware: "Lizenzen und Software",
    paymentFees: "Zahlungsgebühren",
    officeSupplies: "Büromaterial",
    operationsCenter: "Live-Betriebsablauf",
    operationsCalm: "Das Geschäft arbeitet derzeit normal.",
    nextRiskWindow: "Nächste Risikoprüfung",
    operationalIncident: "Betriebsereignis",
    chooseResponse: "Reaktion auswählen",
    incidentResolved: "Ereignis gelöst",
    powerIncidentTitle: "Stromausfall",
    powerIncidentText: "Werkstatt und Verkaufsterminals sind ausgefallen.",
    theftIncidentTitle: "Verdächtige Lageraktivität",
    theftIncidentText: "Das Sicherheitssystem meldet möglichen Diebstahl.",
    workshopIncidentTitle: "Werkstattdefekt",
    workshopIncidentText: "Ein Montagegerät muss dringend repariert werden.",
    repairIncidentTitle: "Dringender Reparaturauftrag",
    repairIncidentText: "Ein Kunde benötigt eine Reparatur am selben Tag.",
    rushIncidentTitle: "Verspätete Lieferung",
    rushIncidentText: "Eine Set-Lieferung verspätet sich; Express ist verfügbar.",
    useGenerator: "Generator starten",
    waitOutage: "Ausfall abwarten",
    secureWarehouse: "Sicherheitsdienst rufen",
    claimInsurance: "Versicherung melden",
    professionalRepair: "Service rufen",
    technicianRepair: "Techniker repariert",
    acceptRepair: "Reparatur annehmen",
    declineRepair: "Auftrag ablehnen",
    expressCourier: "Expresskurier",
    delayDelivery: "Lieferung verschieben",
    debtRecoveryHint: "Verkaufe Lagerteile oder nutze den Rettungskredit auf der Finanzseite.",
    restructuring: "Notfall-Umschuldung",
    restructuringQuestion: "Schuldenlimit überschritten. Umschulden statt schließen?"
});


function readSettings() {
    try {
        return JSON.parse(localStorage.getItem(SETTINGS_KEY) || "{}") || {};
    } catch (_error) {
        return {};
    }
}


function writeSettings(changes = {}) {
    localStorage.setItem(
        SETTINGS_KEY,
        JSON.stringify({ ...readSettings(), ...changes })
    );
}


function getSaveKey(slot = 1) {
    return Number(slot) === 1 ? SAVE_KEY : `${SAVE_KEY}_${slot}`;
}


function getSaveSlotSnapshot(slot) {
    try {
        const value = localStorage.getItem(getSaveKey(slot));
        const state = value ? JSON.parse(value) : null;
        return isCompatibleSaveState(state) ? state : null;
    } catch (_error) {
        return null;
    }
}


function getMostRecentSaveSlot() {
    const settingsSlot = Number(readSettings().activeSaveSlot);

    if (settingsSlot >= 1 && settingsSlot <= SAVE_SLOT_COUNT
        && getSaveSlotSnapshot(settingsSlot)) {
        return settingsSlot;
    }

    const saves = Array.from({ length: SAVE_SLOT_COUNT }, (_, index) => ({
        slot: index + 1,
        state: getSaveSlotSnapshot(index + 1)
    })).filter(item => item.state);

    saves.sort((first, second) =>
        String(second.state.savedAt || "").localeCompare(
            String(first.state.savedAt || "")
        )
    );

    return saves[0]?.slot || 1;
}


function saveGame(showNotification = false) {
    if (!runtime.gameStarted) {
        return false;
    }

    try {
        const saveState = document.getElementById("auto-save-state");
        const slot = clamp(Number(runtime.activeSaveSlot) || 1, 1, SAVE_SLOT_COUNT);
        const preferredLanguage = loadSavedLanguage();
        gameState.language = preferredLanguage;
        gameState.savedAt = new Date().toISOString();
        gameState.saveSlot = slot;

        if (saveState) {
            saveState.innerHTML = `<span class="save-dot"></span><span>${t("saving")}</span>`;
        }

        localStorage.setItem(getSaveKey(slot), JSON.stringify(gameState));
        writeSettings({ activeSaveSlot: slot });

        window.setTimeout(() => {
            if (saveState) {
                saveState.innerHTML = `<span class="save-dot"></span><span>${t("saved")} · ${t("saveSlot")} ${slot}</span>`;
            }
        }, 250);

        if (showNotification) {
            showToast(t("gameSaved"), `${t("saveSlot")} ${slot}`, "success");
        }

        return true;
    } catch (error) {
        console.error("Save error:", error);
        return false;
    }
}


function hasSaveGame(slot = null) {
    if (slot !== null) {
        return Boolean(getSaveSlotSnapshot(slot));
    }

    return Array.from(
        { length: SAVE_SLOT_COUNT },
        (_, index) => Boolean(getSaveSlotSnapshot(index + 1))
    ).some(Boolean);
}


function loadGame(slot = getMostRecentSaveSlot()) {
    const preferredLanguage = loadSavedLanguage();
    const loadedState = getSaveSlotSnapshot(slot);

    if (!loadedState) {
        return false;
    }

    gameState = loadedState;
    gameState.language = preferredLanguage;
    runtime.activeSaveSlot = Number(slot);
    runtime.gameStarted = true;
    gameState.calendar.minutes = clamp(
        gameState.calendar.minutes,
        DAY_START_MINUTES,
        DAY_END_MINUTES
    );
    gameState.paused = false;
    normalizeGameState();
    writeSettings({ activeSaveSlot: runtime.activeSaveSlot });
    return true;
}


function deleteSaveGame(slot = runtime.activeSaveSlot || 1) {
    localStorage.removeItem(getSaveKey(slot));

    if (Number(slot) === Number(runtime.activeSaveSlot)) {
        runtime.gameStarted = false;
        runtime.selectedOfferId = null;
        runtime.selectedCustomerId = null;
        runtime.selectedBuiltPcId = null;
        runtime.selectedBuildParts = {};
    }
}


function prepareNewGame(language = loadSavedLanguage(), slot = runtime.activeSaveSlot || 1) {
    const preferredLanguage = ["tr", "en", "de"].includes(language)
        ? language
        : loadSavedLanguage();
    runtime.activeSaveSlot = clamp(Number(slot) || 1, 1, SAVE_SLOT_COUNT);
    runtime.gameStarted = true;
    gameState = createInitialState(preferredLanguage);
    writeSettings({
        language: preferredLanguage,
        activeSaveSlot: runtime.activeSaveSlot
    });
    normalizeGameState();
    refreshMarket();
    generateCustomers(6);
    addActivity(t("storeOpened"), "store", "09:00");
    runtime.currentPage = "dashboard";
    runtime.selectedOfferId = null;
    runtime.selectedCustomerId = null;
    runtime.selectedBuiltPcId = null;
    runtime.selectedBuildParts = {};
    saveGame(false);
}


async function requestNewGame() {
    await showSaveSlotWindow("new");
}


async function continueGame(slot = getMostRecentSaveSlot()) {
    const requestedSlot = Number(slot);

    // DOM click handlers pass a PointerEvent as their first argument. Treat
    // non-numeric values as a request for the most recently used save slot.
    if (!Number.isInteger(requestedSlot)
        || requestedSlot < 1
        || requestedSlot > SAVE_SLOT_COUNT) {
        slot = getMostRecentSaveSlot();
    } else {
        slot = requestedSlot;
    }

    if (!loadGame(slot)) {
        await showInformationModal(t("warning"), t("noSave"), "warning", "!");
        return;
    }

    enterGameScreen();
}


function createSaveSlotCards(mode) {
    return Array.from({ length: SAVE_SLOT_COUNT }, (_, index) => {
        const slot = index + 1;
        const saved = getSaveSlotSnapshot(slot);
        const date = saved?.savedAt
            ? new Date(saved.savedAt).toLocaleString()
            : "—";

        return `
            <article class="save-slot-card ${saved ? "occupied" : "empty"}">
                <div class="save-slot-number">${slot}</div>
                <div class="save-slot-info">
                    <strong>${t("saveSlot")} ${slot}</strong>
                    ${saved ? `
                        <span>${formatMoney(saved.money)} · ${t("reputation")} ${saved.reputation}</span>
                        <span>${t("day")} ${saved.lifetime?.daysCompleted + 1 || 1} · ${t("lastSaved")}: ${escapeHtml(date)}</span>
                    ` : `<span>${t("emptySlot")}</span>`}
                </div>
                <div class="save-slot-actions">
                    ${saved && mode !== "new" ? `
                        <button class="game-button primary" data-slot-action="load" data-slot="${slot}" type="button">${t("loadSlot")}</button>
                    ` : ""}
                    <button class="game-button secondary" data-slot-action="new" data-slot="${slot}" type="button">${t("createSlot")}</button>
                    ${saved ? `
                        <button class="game-button danger" data-slot-action="delete" data-slot="${slot}" type="button">${t("deleteSlot")}</button>
                    ` : ""}
                </div>
            </article>
        `;
    }).join("");
}


async function showSaveSlotWindow(mode = "manage") {
    const modalPromise = showGameModal({
        title: t("saveSlots"),
        message: t("slotManagerDescription"),
        icon: "▣",
        type: "info",
        confirmText: t("close"),
        showCancel: false,
        extraHtml: `<div class="save-slot-grid">${createSaveSlotCards(mode)}</div>`
    });

    window.setTimeout(() => {
        document.querySelectorAll("[data-slot-action]").forEach(button => {
            button.addEventListener("click", () => {
                const action = button.dataset.slotAction;
                const slot = Number(button.dataset.slot);
                document.getElementById("modal-confirm-button")?.click();

                window.setTimeout(async () => {
                    if (action === "load") {
                        await continueGame(slot);
                    } else if (action === "new") {
                        await startNewGameInSlot(slot);
                    } else if (action === "delete") {
                        await requestDeleteSaveSlot(slot);
                    }
                }, 0);
            });
        });
    }, 30);

    await modalPromise;
}


async function startNewGameInSlot(slot) {
    if (hasSaveGame(slot)) {
        const accepted = await showGameModal({
            title: `${t("saveSlot")} ${slot}`,
            message: t("overwriteSlotQuestion"),
            icon: "!",
            type: "warning",
            confirmText: t("yes"),
            cancelText: t("no")
        });

        if (!accepted) {
            return;
        }
    }

    deleteSaveGame(slot);
    prepareNewGame(loadSavedLanguage(), slot);
    enterGameScreen();
    showToast(t("success"), `${t("newGameStarted")} · ${t("saveSlot")} ${slot}`, "success");
}


async function requestDeleteSaveSlot(slot) {
    const accepted = await showGameModal({
        title: `${t("deleteSave")} · ${t("saveSlot")} ${slot}`,
        message: t("deleteSaveQuestion"),
        icon: "×",
        type: "danger",
        confirmText: t("yes"),
        cancelText: t("no")
    });

    if (accepted) {
        deleteSaveGame(slot);
        showToast(t("success"), t("saveDeleted"), "success");
        showStartScreen();
    }
}


async function requestExitGame() {
    const accepted = await showGameModal({
        title: t("exitGame"),
        message: t("exitGameQuestion"),
        icon: "⏻",
        type: "danger",
        confirmText: t("yes"),
        cancelText: t("no")
    });

    if (!accepted) {
        return;
    }

    saveGame(false);
    if (window.pcShopDesktop?.quit) {
        window.pcShopDesktop.quit();
    } else {
        window.close();
    }
}


function getInventoryPartResalePrice(partId) {
    const buyerQuality = gameState.staff
        .filter(employee => employee.role === "buyer")
        .reduce((total, employee) => total + employee.quality, 0);
    const resaleRate = clamp(0.70 + buyerQuality * 0.00045, 0.70, 0.80);
    return Math.max(1, Math.round(getInventoryAverageCost(partId) * resaleRate));
}


function resellInventoryPart(partId, requestedAmount = 1) {
    const available = getInventoryQuantity(partId);
    const amount = clamp(Math.floor(Number(requestedAmount) || 0), 0, available);
    const part = getPartById(partId);

    if (!part || amount <= 0) {
        return { success: false, total: 0 };
    }

    const unitPrice = getInventoryPartResalePrice(partId);
    const total = unitPrice * amount;
    removeInventory(partId, amount);
    registerRevenue(total);
    addActivity(`${amount} × ${part.name} · ${t("partResold")}: ${formatMoney(total)}.`, "sale");
    saveGame(false);
    return { success: true, total };
}


function handleInventoryPartSale(partId, amount) {
    const result = resellInventoryPart(partId, amount);

    if (result.success) {
        showToast(t("partResold"), formatMoney(result.total), "success");
        renderInventoryPage();
        updateTopBar();
    }
}


function calculateEmergencyLoanPayment() {
    const finance = gameState.finance || {};

    if (finance.emergencyLoanBalance <= 0 || finance.loanDaysRemaining <= 0) {
        return 0;
    }

    return Math.ceil(finance.emergencyLoanBalance / finance.loanDaysRemaining);
}


function applyEmergencyLoanPayment(payment) {
    if (!payment || !gameState.finance) {
        return;
    }

    gameState.finance.emergencyLoanBalance = Math.max(
        0,
        gameState.finance.emergencyLoanBalance - payment
    );
    gameState.finance.loanDaysRemaining = Math.max(
        0,
        gameState.finance.loanDaysRemaining - 1
    );
}


function takeEmergencyLoan() {
    const finance = gameState.finance;

    if (finance.emergencyLoanBalance > 0) {
        return false;
    }

    gameState.money += 3500;
    finance.emergencyLoanBalance = 4200;
    finance.loanDaysRemaining = 7;
    finance.lastLoanDay = gameState.lifetime.daysCompleted;
    addActivity(`${t("emergencyLoan")}: +${formatMoney(3500)} / ${formatMoney(4200)}.`, "finance");
    saveGame(false);
    showToast(t("emergencyLoan"), t("loanTaken"), "success");
    safeRender();
    return true;
}


function renderFinanceEnhancements(content, operating) {
    const services = operating.breakdown.services;
    const maintenance = operating.breakdown.maintenance;
    const administration = operating.breakdown.administration;

    content.insertAdjacentHTML("beforeend", `
        <section class="game-panel expense-breakdown-panel">
            <div class="panel-header">
                <div class="panel-title"><div class="panel-title-icon">≋</div><div>
                    <h2>${t("detailedExpenses")}</h2>
                    <span class="panel-subtitle">${formatGameDate()}</span>
                </div></div>
            </div>
            <div class="expense-breakdown-grid panel-padding">
                <div class="expense-group"><h3>⚡ ${t("serviceCosts")}</h3>
                    ${createExpenseDetail(t("electricityUsage"), services.electricity)}
                    ${createExpenseDetail(t("internetService"), services.internet)}
                    ${createExpenseDetail(t("insuranceService"), services.insurance)}
                    ${createExpenseDetail(t("waterCleaning"), services.waterAndCleaning)}
                </div>
                <div class="expense-group"><h3>⚒ ${t("maintenance")}</h3>
                    ${createExpenseDetail(t("facilityMaintenance"), maintenance.facility)}
                    ${createExpenseDetail(t("workshopWear"), maintenance.workshopWear)}
                    ${createExpenseDetail(t("equipmentDepreciation"), maintenance.equipmentDepreciation)}
                </div>
                <div class="expense-group"><h3>≡ ${t("administration")}</h3>
                    ${createExpenseDetail(t("permitsSoftware"), administration.permitsAndSoftware)}
                    ${createExpenseDetail(t("paymentFees"), administration.paymentFees)}
                    ${createExpenseDetail(t("officeSupplies"), administration.officeSupplies)}
                </div>
            </div>
            <div class="recovery-loan-strip">
                <div><strong>${t("emergencyLoan")}</strong><span>${t("emergencyLoanDescription")}</span></div>
                ${gameState.finance.emergencyLoanBalance > 0
                    ? `<strong>${formatMoney(gameState.finance.emergencyLoanBalance)} · ${gameState.finance.loanDaysRemaining} ${t("daysRemaining")}</strong>`
                    : `<button id="take-emergency-loan" class="game-button warning" type="button" ${gameState.money >= 2500 ? "disabled" : ""}>${t("takeLoan")}</button>`}
            </div>
        </section>
    `);

    document.getElementById("take-emergency-loan")
        ?.addEventListener("click", takeEmergencyLoan);
}


function createExpenseDetail(label, value) {
    return `<div class="expense-detail"><span>${escapeHtml(label)}</span><strong>${formatMoney(value)}</strong></div>`;
}


function processOperationalIncidents() {
    if (!gameState.operations || gameState.operations.activeIncident
        || runtime.dayReportOpen) {
        return;
    }

    const now = getAbsoluteGameMinutes();

    if (now < gameState.operations.nextIncidentAt) {
        return;
    }

    if (!chance(0.72)) {
        gameState.operations.nextIncidentAt = now + randomInt(70, 150);
        return;
    }

    const electricity = getProviderById("electricity", gameState.providers.electricity);
    const insurance = getProviderById("insurance", gameState.providers.insurance);
    const security = clamp(
        insurance.theftProtection + gameState.upgrades.security * 0.08,
        0,
        0.92
    );
    const candidates = [
        { kind: "power", weight: Math.max(0.35, electricity.outageRisk * 65) },
        { kind: "theft", weight: Math.max(0.35, 3 * (1 - security)) },
        { kind: "workshop", weight: 2.2 },
        { kind: "repair", weight: 2.6 },
        { kind: "rush", weight: 1.8 }
    ];
    const totalWeight = candidates.reduce((sum, item) => sum + item.weight, 0);
    let roll = Math.random() * totalWeight;
    let chosen = candidates[0];

    for (const candidate of candidates) {
        roll -= candidate.weight;
        if (roll <= 0) {
            chosen = candidate;
            break;
        }
    }

    gameState.operations.activeIncident = {
        id: `incident_${Date.now()}`,
        kind: chosen.kind,
        createdAt: now,
        severity: ["theft", "power", "workshop"].includes(chosen.kind)
            ? "danger"
            : "warning"
    };
    saveGame(false);
    showToast(t("operationalIncident"), getIncidentText(chosen.kind).title, "warning", 5200);
    safeRender();
}


function getIncidentText(kind) {
    const mapping = {
        power: ["powerIncidentTitle", "powerIncidentText"],
        theft: ["theftIncidentTitle", "theftIncidentText"],
        workshop: ["workshopIncidentTitle", "workshopIncidentText"],
        repair: ["repairIncidentTitle", "repairIncidentText"],
        rush: ["rushIncidentTitle", "rushIncidentText"]
    };
    const keys = mapping[kind] || mapping.workshop;
    return { title: t(keys[0]), text: t(keys[1]) };
}


function getIncidentChoices(kind) {
    return {
        power: [["generator", "useGenerator", "primary"], ["wait", "waitOutage", "secondary"]],
        theft: [["security", "secureWarehouse", "primary"], ["insurance", "claimInsurance", "secondary"]],
        workshop: [["service", "professionalRepair", "primary"], ["technician", "technicianRepair", "secondary"]],
        repair: [["accept", "acceptRepair", "primary"], ["decline", "declineRepair", "secondary"]],
        rush: [["express", "expressCourier", "primary"], ["delay", "delayDelivery", "secondary"]]
    }[kind] || [];
}


function renderOperationalAlert() {
    const content = document.getElementById("page-content");

    if (!content || !gameState.operations) {
        return;
    }

    const incident = gameState.operations.activeIncident;

    if (incident) {
        const text = getIncidentText(incident.kind);
        content.insertAdjacentHTML("afterbegin", `
            <section class="operations-alert ${incident.severity}">
                <div class="operations-alert-icon">!</div>
                <div class="operations-alert-copy"><span>${t("operationalIncident")}</span><strong>${escapeHtml(text.title)}</strong><p>${escapeHtml(text.text)}</p></div>
                <div class="operations-alert-actions">
                    ${getIncidentChoices(incident.kind).map(choice => `
                        <button type="button" class="game-button ${choice[2]}" data-incident-choice="${choice[0]}">${t(choice[1])}</button>
                    `).join("")}
                </div>
            </section>
        `);

        content.querySelectorAll("[data-incident-choice]").forEach(button => {
            button.addEventListener("click", () =>
                resolveOperationalIncident(button.dataset.incidentChoice)
            );
        });
        return;
    }

    if (runtime.currentPage === "dashboard") {
        const minutes = Math.max(
            0,
            Math.round(gameState.operations.nextIncidentAt - getAbsoluteGameMinutes())
        );
        const logs = gameState.operations.incidentLog.slice(0, 4);
        content.insertAdjacentHTML("afterbegin", `
            <section class="operations-flow-bar">
                <div><span class="live-dot"></span><strong>${t("operationsCenter")}</strong><span>${t("operationsCalm")}</span></div>
                <div class="operations-mini-log">
                    ${logs.map(log => `<span>${escapeHtml(log.time)} · ${escapeHtml(getIncidentText(log.kind).title)}</span>`).join("")}
                </div>
                <strong>${t("nextRiskWindow")}: ~${minutes} ${t("minutes")}</strong>
            </section>
        `);
    }
}


function removeRandomInventory(amount) {
    let value = 0;
    let removed = 0;

    while (removed < amount && getInventoryCount() > 0) {
        const ids = Object.keys(gameState.inventory).filter(id => getInventoryQuantity(id) > 0);
        const partId = randomItem(ids);
        value += getInventoryAverageCost(partId);
        removeInventory(partId, 1);
        removed += 1;
    }

    return Math.round(value);
}


function resolveOperationalIncident(choice) {
    const incident = gameState.operations.activeIncident;

    if (!incident) {
        return;
    }

    let moneyChange = 0;
    let reputationChange = 0;
    const technician = gameState.staff
        .filter(employee => employee.role === "technician")
        .sort((first, second) => second.quality - first.quality)[0];

    if (incident.kind === "power") {
        const cost = choice === "generator" ? randomInt(130, 220) : randomInt(220, 480);
        registerExpense(cost);
        moneyChange = -cost;
        if (choice === "wait") {
            gameState.staff.forEach(employee => employee.energy = Math.max(0, employee.energy - 8));
        }
    } else if (incident.kind === "theft") {
        if (choice === "security") {
            const cost = 145;
            registerExpense(cost);
            moneyChange = -cost;
        } else {
            const lostValue = removeRandomInventory(randomInt(1, 3));
            const insurance = getProviderById("insurance", gameState.providers.insurance);
            const netLoss = Math.round(lostValue * (1 - insurance.theftProtection));
            registerExpense(netLoss);
            moneyChange = -netLoss;
        }
    } else if (incident.kind === "workshop") {
        const cost = choice === "technician" && technician ? 70 : 310;
        registerExpense(cost);
        moneyChange = -cost;
        if (choice === "technician" && technician) {
            technician.energy = Math.max(0, technician.energy - 18);
            technician.experience += 12;
        }
    } else if (incident.kind === "repair") {
        if (choice === "accept") {
            const payout = randomInt(260, 520) + Math.round((technician?.quality || 25) * 2.8);
            registerRevenue(payout);
            moneyChange = payout;
            reputationChange = technician ? 2 : 1;
            if (technician) {
                technician.energy = Math.max(0, technician.energy - 12);
                technician.experience += 14;
            }
        }
    } else if (incident.kind === "rush") {
        if (choice === "express") {
            const cost = 190;
            registerExpense(cost);
            moneyChange = -cost;
        } else {
            reputationChange = -1;
        }
    }

    gameState.reputation = Math.max(0, gameState.reputation + reputationChange);
    const text = getIncidentText(incident.kind);
    gameState.operations.incidentLog.unshift({
        kind: incident.kind,
        choice,
        money: moneyChange,
        reputation: reputationChange,
        time: minutesToTime(gameState.calendar.minutes)
    });
    gameState.operations.incidentLog = gameState.operations.incidentLog.slice(0, 20);
    gameState.operations.activeIncident = null;
    gameState.operations.nextIncidentAt = getAbsoluteGameMinutes() + randomInt(150, 330);
    addActivity(`${text.title}: ${t("incidentResolved")} · ${formatMoney(moneyChange)}.`, "event");
    saveGame(false);
    showToast(text.title, t("incidentResolved"), "success");
    safeRender();
}


async function checkFinancialFailure() {
    const debtLimit = -12000 - gameState.reputation * 80;

    if (gameState.money < debtLimit) {
        const accepted = await showGameModal({
            title: t("restructuring"),
            message: t("restructuringQuestion"),
            icon: "€",
            type: "danger",
            confirmText: t("yes"),
            cancelText: t("no")
        });

        if (accepted) {
            gameState.money = 1000;
            gameState.finance.emergencyLoanBalance += 9000;
            gameState.finance.loanDaysRemaining = Math.max(
                gameState.finance.loanDaysRemaining,
                10
            );
            gameState.reputation = Math.max(0, gameState.reputation - 8);
            addActivity(`${t("restructuring")}: ${formatMoney(9000)}.`, "finance");
            saveGame(false);
            safeRender();
            return false;
        }

        gameState.paused = true;
        saveGame(false);
        return true;
    }

    if (gameState.money < 0) {
        showToast(t("debtWarning"), t("debtRecoveryHint"), "warning", 6200);
    }

    return false;
}
