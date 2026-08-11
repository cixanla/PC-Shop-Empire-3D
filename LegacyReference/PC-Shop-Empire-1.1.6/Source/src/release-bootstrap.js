"use strict";

(function integrateRelease(global) {
    const release = global.PCShopRelease;

    if (!release) {
        throw new Error("PC Shop Empire release modules are not available.");
    }

    const base = {
        normalizeGameState,
        prepareNewGame,
        loadGame,
        saveGame,
        refreshMarket,
        generateCustomer,
        generateCustomers,
        calculateDailyOperatingExpenses,
        registerRevenue,
        registerExpense,
        purchaseMarketOffer,
        finishWorkingDay,
        startNextDay,
        displayDayReport,
        renderCurrentPage,
        changeLanguage,
        showStartScreen,
        showToast,
        processOperationalIncidents,
        resolveOperationalIncident,
        renderStaffPage,
        getAutomationInterval,
        runEmployeeAutomation,
        prepareStaffForNextDay
    };

    getSaveSlotSnapshot = function getCompatibleSaveSlotSnapshot(slot) {
        try {
            const value = localStorage.getItem(getSaveKey(slot));
            const state = value ? JSON.parse(value) : null;
            const compatible = state && (
                state.saveSchema === release.saveSchema
                || state.version === release.saveSchema
                || state.version === release.version
            );
            return compatible ? state : null;
        } catch (_error) {
            return null;
        }
    };

    normalizeGameState = function normalizeGameState116() {
        base.normalizeGameState();
        release.normalizeState();
    };

    saveGame = function saveGame116(showNotification = false) {
        if (runtime.gameStarted) {
            gameState.version = release.version;
            gameState.saveSchema = release.saveSchema;
            release.normalizeState();
        }
        return base.saveGame(showNotification);
    };

    loadGame = function loadGame116(slot = getMostRecentSaveSlot()) {
        const preferredLanguage = release.getSettings().language;
        const loaded = base.loadGame(slot);
        if (loaded) {
            gameState.language = preferredLanguage;
            release.persistSettings({
                ...release.getSettings(),
                language: preferredLanguage,
                activeSaveSlot: runtime.activeSaveSlot
            });
            normalizeGameState();
            saveGame(false);
        }
        return loaded;
    };

    prepareNewGame = function prepareNewGame116(language = "en", slot = runtime.activeSaveSlot || 1) {
        base.prepareNewGame(language, slot);
        normalizeGameState();
        gameState.simulation.difficulty = release.getSettings().difficulty;
        saveGame(false);
    };

    refreshMarket = function refreshMarket116() {
        base.refreshMarket();
        if (gameState.marketDynamics) {
            release.adjustMarketOffers();
        }
    };

    generateCustomer = function generateCustomer116() {
        return release.adjustCustomer(base.generateCustomer());
    };

    generateCustomers = function generateCustomers116(amount) {
        const adjusted = gameState.marketing
            ? Math.max(0, Math.round(amount + release.getTrafficAdjustment()))
            : amount;
        return base.generateCustomers(adjusted);
    };

    calculateDailyOperatingExpenses = function calculateDailyOperatingExpenses116() {
        const operating = base.calculateDailyOperatingExpenses();
        const profile = release.getDifficultyProfile();
        const eligibleRaw = Math.max(0, operating.rawTotal - operating.loanPayment);
        const eligibleSaving = Math.min(eligibleRaw, operating.saving);
        const adjustedOperations = Math.max(0, Math.round((eligibleRaw - eligibleSaving) * profile.expenses));
        return {
            ...operating,
            saving: eligibleSaving,
            total: adjustedOperations + operating.loanPayment,
            difficultyAdjustment: adjustedOperations - (eligibleRaw - eligibleSaving)
        };
    };

    registerRevenue = function registerRevenue116(amount) {
        base.registerRevenue(amount);
        release.recordTransaction?.("revenue", Math.max(0, Math.round(amount)));
    };

    registerExpense = function registerExpense116(amount) {
        base.registerExpense(amount);
        release.recordTransaction?.("expense", Math.max(0, Math.round(amount)));
    };

    purchaseMarketOffer = function purchaseMarketOffer116(offerId, amount = 1) {
        const offer = getOfferById(offerId);
        const seller = offer?.seller;
        const estimatedTotal = offer ? offer.price * Math.floor(Number(amount) || 0) : 0;
        const result = base.purchaseMarketOffer(offerId, amount);
        if (result?.success) {
            release.recordSupplierPurchase(seller, result.totalPrice || estimatedTotal);
        }
        return result;
    };

    finishWorkingDay = function finishWorkingDay116(automatic = false) {
        if (runtime.dayReportOpen) return;
        release.processDayClosing();
        base.finishWorkingDay(automatic);
        release.recordDayHistory(runtime.lastDayReport);
        release.checkAchievements();
        saveGame(false);
    };

    startNextDay = function startNextDay116() {
        base.startNextDay();
        normalizeGameState();
        release.prepareNewDay();
        saveGame(false);
        safeRender();
    };

    displayDayReport = function displayDayReport116(report) {
        base.displayDayReport(report);
        const events = document.getElementById("report-events");
        const summary = runtime.releaseClosingSummary;
        if (!events || !summary) return;
        events.insertAdjacentHTML("beforeend", `
            <div class="report-event release-report-event"><strong>${release.text("serviceCenter")}</strong><br>${gameState.daily.serviceJobs || 0} ${release.text("completed").toLocaleLowerCase()} · ${formatMoney(gameState.daily.serviceRevenue || 0)}</div>
            <div class="report-event release-report-event"><strong>${release.text("marketCycle")}</strong><br>${release.text(release.getCycle().name)}${summary.cycleChanged ? " · NEW" : ""}</div>
            ${summary.expiredJobs || summary.expiredTenders ? `<div class="report-event text-warning">Expired: ${summary.expiredJobs} service jobs · ${summary.expiredTenders} tenders</div>` : ""}
        `);
    };

    renderCurrentPage = function renderCurrentPage116() {
        const newPages = {
            service: release.renderServicePage,
            brand: release.renderBrandPage,
            intelligence: release.renderIntelligencePage,
            career: release.renderCareerPage
        };
        const renderer = newPages[runtime.currentPage];

        if (renderer) {
            document.querySelectorAll(".nav-button").forEach(button => {
                const active = button.dataset.page === runtime.currentPage;
                button.classList.toggle("active", active);
                if (active) button.setAttribute("aria-current", "page");
                else button.removeAttribute("aria-current");
            });
            renderer();
            renderOperationalAlert();
            updateTopBar();
        } else {
            base.renderCurrentPage();
            release.decorateDashboard();
            release.decorateMarketPage();
        }

        document.querySelectorAll(".nav-button").forEach(button => {
            const active = button.dataset.page === runtime.currentPage;
            if (active) button.setAttribute("aria-current", "page");
            else button.removeAttribute("aria-current");
        });
        document.querySelector("#page-content h1")?.setAttribute("tabindex", "-1");
    };

    renderStaffPage = function renderStaffPage116() {
        base.renderStaffPage();
        const content = document.getElementById("page-content");
        if (!content || runtime.currentPage !== "staff") return;
        const morale = gameState.staff.length
            ? gameState.staff.reduce((sum, employee) => sum + (employee.morale || 0), 0) / gameState.staff.length
            : 0;
        const fatigue = gameState.staff.length
            ? gameState.staff.reduce((sum, employee) => sum + (employee.fatigue || 0), 0) / gameState.staff.length
            : 0;
        content.insertAdjacentHTML("afterbegin", `<div class="team-pulse-strip"><div><span>TEAM PULSE</span><strong>${Math.round(morale)}% morale</strong></div><div class="team-pulse-meter"><i style="width:${morale}%"></i></div><div><span>AVERAGE FATIGUE</span><strong>${Math.round(fatigue)}%</strong></div><p>High morale improves automation efficiency, while severe fatigue reduces task success.</p></div>`);
    };

    getAutomationInterval = function getAutomationInterval116(employee) {
        const interval = base.getAutomationInterval(employee);
        const morale = employee.morale ?? 70;
        const fatigue = employee.fatigue ?? 10;
        const multiplier = clamp(1 + fatigue * 0.0022 - (morale - 50) * 0.0024, 0.78, 1.28);
        return Math.max(38, Math.round(interval * multiplier));
    };

    runEmployeeAutomation = function runEmployeeAutomation116(employee) {
        const morale = employee.morale ?? 70;
        const fatigue = employee.fatigue ?? 10;
        const succeeded = base.runEmployeeAutomation(employee);
        employee.morale = clamp(morale + (succeeded ? 0.9 : -0.6), 0, 100);
        employee.fatigue = clamp(fatigue + (succeeded ? 1.8 : 0.6), 0, 100);
        saveGame(false);
        return succeeded;
    };

    prepareStaffForNextDay = function prepareStaffForNextDay116() {
        base.prepareStaffForNextDay();
        for (const employee of gameState.staff) {
            employee.fatigue = clamp((employee.fatigue || 0) - 22, 0, 100);
            employee.morale = clamp((employee.morale || 70) + 3 + getManagerStrength() * 9, 0, 100);
        }
    };

    processOperationalIncidents = function processOperationalIncidents116() {
        const frequency = release.getSettings().eventFrequency;
        const operations = gameState.operations;
        if (frequency === "low" && operations && !operations.activeIncident
            && getAbsoluteGameMinutes() >= operations.nextIncidentAt && chance(0.55)) {
            operations.nextIncidentAt += randomInt(90, 180);
            return;
        }
        base.processOperationalIncidents();
    };

    resolveOperationalIncident = function resolveOperationalIncident116(choice) {
        base.resolveOperationalIncident(choice);
        const frequency = release.getSettings().eventFrequency;
        if (gameState.operations && !gameState.operations.activeIncident) {
            const multiplier = frequency === "high" ? 0.62 : frequency === "low" ? 1.45 : 1;
            const now = getAbsoluteGameMinutes();
            gameState.operations.nextIncidentAt = now
                + Math.max(70, Math.round((gameState.operations.nextIncidentAt - now) * multiplier));
        }
    };

    changeLanguage = function changeLanguage116(language) {
        const previousLanguage = gameState.language;
        base.changeLanguage(language);
        release.updateStaticBranding?.();
        if (previousLanguage !== gameState.language) {
            document.getElementById("release-settings-overlay")?.remove();
        }
    };

    showStartScreen = function showStartScreen116() {
        base.showStartScreen();
        release.updateStaticBranding?.();
    };

    showSettingsWindow = release.openSettings;

    showToast = function showToast116(title, message, type = "info", duration = 3900) {
        const settings = release.getSettings();
        if (!settings.notifications && !["error", "danger"].includes(type)) return;
        base.showToast(title, message, type, duration);
    };

    release.updateStaticBranding = function updateStaticBranding() {
        document.querySelectorAll("[data-release-version]").forEach(element => element.textContent = release.version);
        document.querySelectorAll("[data-release-owner]").forEach(element => element.textContent = release.owner);
        document.querySelectorAll("[data-release-text]").forEach(element => element.textContent = release.text(element.dataset.releaseText));
        document.title = `PC Shop Empire ${release.version}`;
        const navLabels = {
            service: "serviceCenter",
            brand: "brandMarket",
            intelligence: "intelligence",
            career: "career"
        };
        Object.entries(navLabels).forEach(([page, key]) => {
            const label = document.querySelector(`.nav-button[data-page="${page}"] .nav-label`);
            if (label) label.textContent = release.text(key);
        });
        const serviceBadge = document.querySelector('.nav-button[data-page="service"] .release-badge');
        if (serviceBadge && typeof gameState !== "undefined") {
            const count = gameState.serviceCenter?.jobs?.length || 0;
            serviceBadge.textContent = String(count);
            serviceBadge.classList.toggle("hidden", count === 0);
        }
        const automationToggle = document.getElementById("automation-toggle");
        if (automationToggle && typeof gameState !== "undefined") {
            automationToggle.setAttribute("aria-checked", String(Boolean(gameState.automationEnabled)));
        }
    };

    release.bindReleaseEvents = function bindReleaseEvents() {
        document.addEventListener("pointerup", event => {
            const button = event.target.closest?.("button");
            if (button && !button.disabled) release.audio.play(button.classList.contains("primary") ? "confirm" : "click");
        });

        global.addEventListener("blur", () => {
            if (runtime.gameStarted && release.getSettings().pauseOnBlur) {
                runtime.releasePausedOnBlur = !gameState.paused;
                gameState.paused = true;
                updateTimeControlButtons();
            }
        });

        global.addEventListener("focus", () => {
            if (runtime.gameStarted && runtime.releasePausedOnBlur) {
                runtime.releasePausedOnBlur = false;
                gameState.paused = false;
                updateTimeControlButtons();
            }
        });

        document.addEventListener("keydown", event => {
            if (event.target.matches?.("input, select, textarea")) return;
            if (event.code === "Space" && runtime.gameStarted) {
                event.preventDefault();
                toggleGamePause();
            }
            if (["1", "2", "4"].includes(event.key) && !event.altKey && runtime.gameStarted) {
                setGameSpeed(Number(event.key));
            }
            if (event.altKey && /^[1-9]$/.test(event.key) && runtime.gameStarted) {
                const buttons = [...document.querySelectorAll(".nav-button")];
                buttons[Number(event.key) - 1]?.click();
            }
        });
    };

    document.addEventListener("DOMContentLoaded", () => {
        release.applySettings(release.getSettings());
        release.updateStaticBranding();
        release.bindReleaseEvents();
    });
})(globalThis);
