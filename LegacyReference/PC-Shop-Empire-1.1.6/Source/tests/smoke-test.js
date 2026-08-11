"use strict";

const assert = require("node:assert/strict");
const fs = require("node:fs");
const path = require("node:path");
const vm = require("node:vm");

const storage = new Map();
const documentListeners = new Map();

const documentMock = {
    documentElement: { lang: "tr" },
    visibilityState: "visible",
    hidden: false,
    addEventListener(type, handler) {
        documentListeners.set(type, handler);
    },
    getElementById() {
        return null;
    },
    querySelectorAll() {
        return [];
    },
    querySelector() {
        return null;
    }
};

const windowMock = {
    addEventListener() {},
    setTimeout(handler) {
        handler();
        return 1;
    },
    clearTimeout() {},
    setInterval() {
        return 1;
    },
    clearInterval() {},
    close() {}
};

const context = vm.createContext({
    console,
    Date,
    JSON,
    Math,
    Intl,
    Object,
    Array,
    Number,
    String,
    Boolean,
    Set,
    Map,
    Promise,
    performance: { now: () => 0 },
    document: documentMock,
    window: windowMock,
    localStorage: {
        getItem(key) {
            return storage.has(key) ? storage.get(key) : null;
        },
        setItem(key, value) {
            storage.set(key, String(value));
        },
        removeItem(key) {
            storage.delete(key);
        }
    }
});

const gamePath = path.join(__dirname, "..", "game.js");
vm.runInContext(fs.readFileSync(gamePath, "utf8"), context, {
    filename: gamePath
});

for (const relativePath of [
    "src/release-data.js",
    "src/release-systems.js",
    "src/release-settings.js",
    "src/release-bootstrap.js"
]) {
    const modulePath = path.join(__dirname, "..", relativePath);
    vm.runInContext(fs.readFileSync(modulePath, "utf8"), context, {
        filename: modulePath
    });
}

function evaluate(source) {
    return vm.runInContext(source, context);
}

const missingLocaleKeys = JSON.parse(evaluate(`JSON.stringify(
    [...new Set(Object.values(PCShopRelease.locales).flatMap(locale => Object.keys(locale)))]
        .filter(key => Object.values(PCShopRelease.locales).some(locale => !locale[key]))
)`));
assert.deepEqual(missingLocaleKeys, [], "1.1.6 interface text must be defined in all three languages");

evaluate("prepareNewGame('tr', 1)");

assert.equal(evaluate("gameState.version"), "1.1.6");
assert.equal(evaluate("gameState.saveSchema"), 3);
assert.equal(evaluate("gameState.serviceCenter.jobs.length >= 3"), true);
assert.equal(evaluate("gameState.career.objectives.length"), 3);

const offerCount = evaluate("gameState.marketOffers.length");
assert.ok(offerCount >= 56, "Pazar yeterli sayıda teklif üretmeli");

const invalidBundles = evaluate(`
    [...new Set(gameState.marketOffers.map(offer => offer.bundleId))]
        .filter(bundleId => {
            const ids = gameState.marketOffers
                .filter(offer => offer.bundleId === bundleId)
                .map(offer => offer.partId);
            return ids.length !== REQUIRED_COMPONENT_TYPES.length
                || getCompatibilityErrors(ids).length > 0;
        }).length
`);
assert.equal(invalidBundles, 0, "Her pazar seti tam ve uyumlu olmalı");

const cpuStockMinimum = evaluate(`
    Math.min(...gameState.marketOffers
        .filter(offer => getPartById(offer.partId).type === 'CPU')
        .map(offer => offer.stock))
`);
assert.ok(cpuStockMinimum >= 12, "CPU stoğu artırılmış olmalı");

evaluate(`
    const testOffer = gameState.marketOffers[0];
    addInventory(testOffer.partId, 3, testOffer.price);
    globalThis.__testPartId = testOffer.partId;
    globalThis.__moneyBeforeResale = gameState.money;
`);
const resaleResult = evaluate("resellInventoryPart(globalThis.__testPartId, 2)");
assert.equal(resaleResult.success, true, "Parça geri satışı başarılı olmalı");
assert.equal(evaluate("getInventoryQuantity(globalThis.__testPartId)"), 1);
assert.ok(
    evaluate("gameState.money") > evaluate("globalThis.__moneyBeforeResale"),
    "Geri satış kasayı artırmalı"
);

evaluate("gameState.money = 11111; saveGame(false); prepareNewGame('tr', 2); gameState.money = 22222; saveGame(false)");
assert.equal(evaluate("hasSaveGame(1)"), true);
assert.equal(evaluate("hasSaveGame(2)"), true);
assert.equal(evaluate("loadGame(1); gameState.money"), 11111);
assert.equal(evaluate("loadGame(2); gameState.money"), 22222);

evaluate("gameState.money = 33333; saveGame(false); gameState.money = 0; continueGame({ type: 'click' })");
assert.equal(
    evaluate("gameState.money"),
    33333,
    "Devam Et düğmesinden gelen tıklama olayı etkin kayıt yuvası olarak yorumlanmamalı"
);

evaluate(`
    PCShopRelease.persistSettings({
        ...PCShopRelease.getSettings(),
        language: 'en',
        visualQuality: 'low'
    });
    changeLanguage('en');
    const legacyTurkishSave = JSON.parse(localStorage.getItem(getSaveKey(1)));
    legacyTurkishSave.language = 'tr';
    localStorage.setItem(getSaveKey(1), JSON.stringify(legacyTurkishSave));
    loadGame(1);
`);
assert.equal(
    evaluate("gameState.language"),
    "en",
    "Loading a legacy Turkish save must preserve the global English language preference"
);
assert.equal(evaluate("readSettings().language"), "en");
assert.equal(evaluate("readSettings().visualQuality"), "low");
assert.equal(
    evaluate("JSON.parse(localStorage.getItem(getSaveKey(1))).language"),
    "en",
    "The loaded save should migrate to the selected global language"
);

evaluate("changeLanguage('de'); loadGame(2)");
assert.equal(
    evaluate("gameState.language"),
    "de",
    "The German language preference must also survive loading a Turkish save"
);
assert.equal(evaluate("readSettings().language"), "de");
assert.equal(
    evaluate("JSON.parse(localStorage.getItem(getSaveKey(2))).language"),
    "de"
);

evaluate(`
    const legacySave = JSON.parse(localStorage.getItem(getSaveKey(2)));
    legacySave.version = 3;
    delete legacySave.saveSchema;
    localStorage.setItem(getSaveKey(3), JSON.stringify(legacySave));
`);
assert.equal(evaluate("Boolean(getSaveSlotSnapshot(3))"), true, "Sürüm 3 kayıtları yüklenebilmeli");

const operating = evaluate("calculateDailyOperatingExpenses()");
assert.ok(operating.breakdown.services.waterAndCleaning > 0);
assert.ok(operating.breakdown.maintenance.equipmentDepreciation > 0);
assert.ok(operating.breakdown.administration.permitsAndSoftware > 0);

evaluate("gameState.money = -500; gameState.finance.emergencyLoanBalance = 0; takeEmergencyLoan()");
assert.equal(evaluate("gameState.money"), 3000);
assert.equal(evaluate("gameState.finance.loanDaysRemaining"), 7);

const loanOperating = evaluate("calculateDailyOperatingExpenses()");
assert.ok(
    loanOperating.total >= loanOperating.loanPayment,
    "Kredi taksiti muhasebe indirimiyle eksilmemeli"
);

evaluate(`
    gameState.operations.activeIncident = {
        id: 'test-incident',
        kind: 'repair',
        severity: 'warning',
        createdAt: getAbsoluteGameMinutes()
    };
    globalThis.__incidentMoney = gameState.money;
    resolveOperationalIncident('accept');
`);
assert.equal(evaluate("gameState.operations.activeIncident"), null);
assert.ok(evaluate("gameState.money") > evaluate("globalThis.__incidentMoney"));

evaluate(`
    gameState.money = 10000;
    globalThis.__campaignMoney = gameState.money;
    globalThis.__campaignStarted = PCShopRelease.startCampaign('local');
`);
assert.equal(evaluate("globalThis.__campaignStarted"), true);
assert.equal(evaluate("gameState.marketing.activeCampaign.id"), "local");
assert.ok(evaluate("gameState.money") < evaluate("globalThis.__campaignMoney"));

evaluate(`
    const serviceJob = gameState.serviceCenter.jobs[0];
    serviceJob.difficulty = 0.1;
    const originalRandom = Math.random;
    Math.random = () => 0.01;
    globalThis.__serviceCompleted = PCShopRelease.completeServiceJob(serviceJob.id, 'premium');
    Math.random = originalRandom;
`);
assert.equal(evaluate("globalThis.__serviceCompleted"), true);
assert.ok(evaluate("gameState.serviceCenter.completed") >= 1);
assert.ok(evaluate("gameState.analytics.ledger.length") > 0);

evaluate(`
    const tender = {
        id: 'tender_test', company: 'Test Company', minimumScore: 100,
        quantity: 2, reward: 5000, deadlineDays: 3, reputationReward: 2
    };
    gameState.serviceCenter.tenders = [tender];
    gameState.builtComputers.push(
        { id: 'pc_tender_1', score: 200, partIds: [], value: 1000, cost: 700 },
        { id: 'pc_tender_2', score: 210, partIds: [], value: 1000, cost: 700 }
    );
    globalThis.__tenderDelivered = PCShopRelease.deliverTender('tender_test');
`);
assert.equal(evaluate("globalThis.__tenderDelivered"), true);
assert.equal(evaluate("gameState.serviceCenter.tenders.length"), 0);
assert.ok(evaluate("gameState.lifetime.tendersCompleted") >= 1);

evaluate(`
    PCShopRelease.recordDayHistory({
        date: '1 Ocak 2026', revenue: 5000, expenses: 3000, net: 2000,
        computersBuilt: 2, computersSold: 1
    });
`);
assert.ok(evaluate("gameState.analytics.history.length") >= 1);
assert.equal(evaluate("PCShopRelease.getBusinessMetrics().recent.length >= 1"), true);

console.log(`Smoke tests passed: ${offerCount} compatible offers, save migration, resale, finance, incidents, campaign, service, tender and analytics.`);
