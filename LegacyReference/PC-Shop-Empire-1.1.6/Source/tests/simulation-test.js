"use strict";

const assert = require("node:assert/strict");
const fs = require("node:fs");
const path = require("node:path");
const vm = require("node:vm");

const storage = new Map();
const documentMock = {
    documentElement: { lang: "tr" },
    visibilityState: "visible",
    hidden: false,
    addEventListener() {},
    getElementById() { return null; },
    querySelector() { return null; },
    querySelectorAll() { return []; }
};
const windowMock = {
    addEventListener() {},
    setTimeout(handler) { handler(); return 1; },
    clearTimeout() {},
    setInterval() { return 1; },
    clearInterval() {},
    close() {}
};
const context = vm.createContext({
    console, Date, JSON, Math, Intl, Object, Array, Number, String, Boolean,
    Set, Map, Promise, performance: { now: () => 0 },
    document: documentMock,
    window: windowMock,
    localStorage: {
        getItem(key) { return storage.has(key) ? storage.get(key) : null; },
        setItem(key, value) { storage.set(key, String(value)); },
        removeItem(key) { storage.delete(key); }
    }
});

for (const relativePath of [
    "game.js",
    "src/release-data.js",
    "src/release-systems.js",
    "src/release-settings.js",
    "src/release-bootstrap.js"
]) {
    const filePath = path.join(__dirname, "..", relativePath);
    vm.runInContext(fs.readFileSync(filePath, "utf8"), context, { filename: filePath });
}

const evaluate = source => vm.runInContext(source, context);
evaluate("prepareNewGame('tr', 1)");

for (let day = 0; day < 45; day += 1) {
    evaluate(`
        gameState.daily.revenue += 1800 + (${day} * 20);
        gameState.money += 1800 + (${day} * 20);
        gameState.lifetime.revenue += 1800 + (${day} * 20);
        finishWorkingDay(false);
        startNextDay();
    `);

    assert.equal(evaluate("Number.isFinite(gameState.money)"), true);
    assert.equal(evaluate("Number.isFinite(gameState.reputation)"), true);
    assert.equal(evaluate("gameState.marketOffers.length >= 56"), true);
    assert.equal(evaluate("Object.hasOwn(PCShopRelease.marketCycles, gameState.marketDynamics.cycleId)"), true);
    assert.equal(evaluate("gameState.career.objectives.length"), 3);
    assert.ok(evaluate("gameState.serviceCenter.jobs.length") <= 9);
    assert.ok(evaluate("gameState.serviceCenter.tenders.length") <= 3);
}

assert.equal(evaluate("gameState.lifetime.daysCompleted"), 45);
assert.equal(evaluate("gameState.analytics.history.length"), 45);
assert.ok(evaluate("gameState.analytics.ledger.length") > 0);
assert.ok(evaluate("gameState.analytics.ledger.length") <= 500);
assert.equal(evaluate("gameState.version"), "1.1.6");
assert.equal(evaluate("gameState.saveSchema"), 3);

console.log("Simulation test passed: 45 working days, market cycles, objectives, service queues and finance history remained valid.");
