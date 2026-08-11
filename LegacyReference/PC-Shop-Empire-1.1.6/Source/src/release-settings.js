"use strict";

(function initializeReleaseSettings(global) {
    const release = global.PCShopRelease;

    if (!release) {
        throw new Error("Release data must be loaded before settings.");
    }

    release.defaultSettings = Object.freeze({
        language: "en",
        activeSaveSlot: 1,
        autoSave: true,
        autoSaveInterval: 30,
        pauseOnBlur: true,
        notifications: true,
        fullscreen: true,
        uiScale: 100,
        visualQuality: "high",
        masterVolume: 55,
        interfaceVolume: 65,
        muteInBackground: true,
        difficulty: "normal",
        eventFrequency: "normal",
        tutorialHints: true,
        reduceMotion: false,
        highContrast: false,
        largeText: false,
        colorProfile: "default",
        largerTargets: false
    });

    release.sanitizeSettings = function sanitizeSettings(input = {}) {
        const settings = { ...release.defaultSettings, ...input };
        settings.language = ["tr", "en", "de"].includes(settings.language) ? settings.language : "en";
        settings.activeSaveSlot = clamp(Number(settings.activeSaveSlot) || 1, 1, SAVE_SLOT_COUNT);
        settings.autoSaveInterval = [30, 60, 120].includes(Number(settings.autoSaveInterval))
            ? Number(settings.autoSaveInterval)
            : 30;
        settings.uiScale = clamp(Number(settings.uiScale) || 100, 80, 125);
        settings.masterVolume = clamp(Number(settings.masterVolume) || 0, 0, 100);
        settings.interfaceVolume = clamp(Number(settings.interfaceVolume) || 0, 0, 100);
        settings.visualQuality = ["low", "high"].includes(settings.visualQuality) ? settings.visualQuality : "high";
        settings.difficulty = ["relaxed", "normal", "expert"].includes(settings.difficulty) ? settings.difficulty : "normal";
        settings.eventFrequency = ["low", "normal", "high"].includes(settings.eventFrequency) ? settings.eventFrequency : "normal";
        settings.colorProfile = ["default", "deuteranopia", "tritanopia"].includes(settings.colorProfile) ? settings.colorProfile : "default";
        [
            "autoSave", "pauseOnBlur", "notifications", "fullscreen", "muteInBackground",
            "tutorialHints", "reduceMotion", "highContrast", "largeText", "largerTargets"
        ].forEach(key => settings[key] = settings[key] !== false);
        return settings;
    };

    release.getSettings = function getSettings() {
        let stored = {};
        try {
            stored = JSON.parse(localStorage.getItem(SETTINGS_KEY) || "{}") || {};
        } catch (_error) {
            stored = {};
        }
        return release.sanitizeSettings(stored);
    };

    release.persistSettings = function persistSettings(settings) {
        const sanitized = release.sanitizeSettings(settings);
        localStorage.setItem(SETTINGS_KEY, JSON.stringify(sanitized));
        return sanitized;
    };

    release.applySettings = function applySettings(settings = release.getSettings(), options = {}) {
        const sanitized = release.sanitizeSettings(settings);
        const root = document.documentElement;
        const body = document.body;

        root.style.setProperty("--ui-scale", String(sanitized.uiScale / 100));
        root.style.setProperty("--ui-font-scale", String((sanitized.largeText ? 1.14 : 1) * sanitized.uiScale / 100));
        body.classList.toggle("quality-performance", sanitized.visualQuality === "low");
        body.classList.toggle("reduce-motion", sanitized.reduceMotion);
        body.classList.toggle("high-contrast", sanitized.highContrast);
        body.classList.toggle("large-text", sanitized.largeText);
        body.classList.toggle("large-targets", sanitized.largerTargets);
        body.dataset.colorProfile = sanitized.colorProfile;
        release.audio?.setSettings(sanitized);

        if (runtime.gameStarted && gameState.simulation) {
            gameState.simulation.difficulty = sanitized.difficulty;
        }

        if (!options.skipFullscreen) {
            global.pcShopDesktop?.setFullscreen?.(sanitized.fullscreen);
        }

        return sanitized;
    };

    release.audio = {
        context: null,
        settings: release.defaultSettings,
        setSettings(settings) {
            this.settings = settings;
        },
        play(kind = "click") {
            const settings = this.settings || release.getSettings();
            if (!settings.masterVolume || !settings.interfaceVolume) return;
            if (settings.muteInBackground && document.hidden) return;
            const AudioContextClass = global.AudioContext || global.webkitAudioContext;
            if (!AudioContextClass) return;

            try {
                this.context = this.context || new AudioContextClass();
                const oscillator = this.context.createOscillator();
                const gain = this.context.createGain();
                const now = this.context.currentTime;
                const profile = {
                    click: [410, 0.035, "sine"],
                    confirm: [620, 0.07, "sine"],
                    warning: [220, 0.10, "triangle"],
                    navigate: [330, 0.045, "sine"]
                }[kind] || [410, 0.035, "sine"];
                const volume = (settings.masterVolume / 100) * (settings.interfaceVolume / 100) * 0.055;
                oscillator.type = profile[2];
                oscillator.frequency.setValueAtTime(profile[0], now);
                gain.gain.setValueAtTime(volume, now);
                gain.gain.exponentialRampToValueAtTime(0.0001, now + profile[1]);
                oscillator.connect(gain);
                gain.connect(this.context.destination);
                oscillator.start(now);
                oscillator.stop(now + profile[1]);
            } catch (_error) {
                this.context = null;
            }
        }
    };

    function toggleField(key, label, description = "") {
        return `<label class="setting-toggle"><span><strong>${release.text(label)}</strong>${description ? `<small>${release.text(description)}</small>` : ""}</span><input type="checkbox" data-setting="${key}"><i aria-hidden="true"></i></label>`;
    }

    function selectField(key, label, options) {
        return `<label class="setting-select"><span>${release.text(label)}</span><select data-setting="${key}">${options.map(option => `<option value="${option[0]}">${release.text(option[1])}</option>`).join("")}</select></label>`;
    }

    function rangeField(key, label, minimum, maximum, suffix = "%") {
        return `<label class="setting-range"><span><strong>${release.text(label)}</strong><output data-setting-output="${key}"></output></span><input type="range" min="${minimum}" max="${maximum}" step="5" data-setting="${key}" data-suffix="${suffix}"></label>`;
    }

    release.createSettingsOverlay = function createSettingsOverlay() {
        let overlay = document.getElementById("release-settings-overlay");
        const language = release.getSettings().language;
        if (overlay && overlay.dataset.language !== language) {
            overlay.remove();
            overlay = null;
        }
        if (overlay) return overlay;

        overlay = document.createElement("div");
        overlay.id = "release-settings-overlay";
        overlay.className = "settings-overlay hidden";
        overlay.dataset.language = language;
        overlay.setAttribute("role", "dialog");
        overlay.setAttribute("aria-modal", "true");
        overlay.setAttribute("aria-labelledby", "release-settings-title");
        overlay.innerHTML = `
            <div class="settings-shell">
                <header class="settings-header">
                    <div class="settings-emblem">⚙</div>
                    <div><span>PC SHOP EMPIRE · ${release.version}</span><h1 id="release-settings-title">${release.text("settingsTitle")}</h1><p>${release.text("settingsSubtitle")}</p></div>
                    <button class="settings-close" data-settings-close type="button" aria-label="${release.text("close")}">×</button>
                </header>
                <div class="settings-body">
                    <nav class="settings-tabs" aria-label="${release.text("settingsTitle")}">
                        ${[
                            ["general", "●", "general"], ["display", "▣", "display"], ["audio", "♪", "audio"],
                            ["gameplay", "◆", "gameplay"], ["accessibility", "◉", "accessibility"],
                            ["controls", "⌨", "controls"], ["data", "▤", "data"], ["about", "i", "about"]
                        ].map((tab, index) => `<button class="settings-tab ${index === 0 ? "active" : ""}" data-settings-tab="${tab[0]}" type="button" role="tab" aria-selected="${index === 0}"><span>${tab[1]}</span>${release.text(tab[2])}</button>`).join("")}
                    </nav>
                    <div class="settings-panels">
                        <section class="settings-panel active" data-settings-panel="general" role="tabpanel">
                            <h2>${release.text("general")}</h2>
                            <p>${release.text("privacyLocal")}</p>
                            ${selectField("language", "language", [["en", "English"], ["de", "German"], ["tr", "Turkish"]])}
                            ${toggleField("autoSave", "autoSave", "autoSaveDescription")}
                            <label class="setting-select"><span>${release.text("saveInterval")}</span><select data-setting="autoSaveInterval"><option value="30">30 ${release.text("secondsShort")}</option><option value="60">60 ${release.text("secondsShort")}</option><option value="120">120 ${release.text("secondsShort")}</option></select></label>
                            ${toggleField("pauseOnBlur", "pauseOnBlur", "pauseOnBlurDescription")}
                            ${toggleField("notifications", "notifications")}
                        </section>
                        <section class="settings-panel" data-settings-panel="display" role="tabpanel">
                            <h2>${release.text("display")}</h2><p>${release.text("displayDescription")}</p>
                            ${toggleField("fullscreen", "fullscreen", "fullscreenDescription")}
                            ${rangeField("uiScale", "uiScale", 80, 125)}
                            ${selectField("visualQuality", "visualQuality", [["high", "qualityHigh"], ["low", "qualityLow"]])}
                        </section>
                        <section class="settings-panel" data-settings-panel="audio" role="tabpanel">
                            <h2>${release.text("audio")}</h2><p>${release.text("audioDescription")}</p>
                            ${rangeField("masterVolume", "masterVolume", 0, 100)}
                            ${rangeField("interfaceVolume", "interfaceVolume", 0, 100)}
                            ${toggleField("muteInBackground", "muteInBackground")}
                            <button class="game-button secondary" data-test-sound type="button">♪ ${release.text("testSound")}</button>
                        </section>
                        <section class="settings-panel" data-settings-panel="gameplay" role="tabpanel">
                            <h2>${release.text("gameplay")}</h2><p>${release.text("gameplayDescription")}</p>
                            ${selectField("difficulty", "difficulty", [["relaxed", "difficultyRelaxed"], ["normal", "difficultyNormal"], ["expert", "difficultyExpert"]])}
                            ${selectField("eventFrequency", "eventFrequency", [["low", "frequencyLow"], ["normal", "frequencyNormal"], ["high", "frequencyHigh"]])}
                            ${toggleField("tutorialHints", "tutorialHints")}
                        </section>
                        <section class="settings-panel" data-settings-panel="accessibility" role="tabpanel">
                            <h2>${release.text("accessibility")}</h2><p>${release.text("accessibilityDescription")}</p>
                            ${toggleField("reduceMotion", "reduceMotion")}
                            ${toggleField("highContrast", "highContrast")}
                            ${toggleField("largeText", "largeText")}
                            ${toggleField("largerTargets", "largerTargets")}
                            ${selectField("colorProfile", "colorProfile", [["default", "colorDefault"], ["deuteranopia", "colorDeuteranopia"], ["tritanopia", "colorTritanopia"]])}
                        </section>
                        <section class="settings-panel" data-settings-panel="controls" role="tabpanel">
                            <h2>${release.text("keyboardShortcuts")}</h2>
                            <div class="shortcut-list"><div><kbd>Space</kbd><span>${release.text("pauseKey").replace("Space: ", "")}</span></div><div><kbd>1 · 2 · 4</kbd><span>${release.text("speedKeys").split(": ")[1]}</span></div><div><kbd>Alt + 1…9</kbd><span>${release.text("navigationKeys").split(": ")[1]}</span></div><div><kbd>F11</kbd><span>${release.text("fullscreenKey").split(": ")[1]}</span></div><div><kbd>Esc</kbd><span>${release.text("escapeKey").split(": ")[1]}</span></div></div>
                        </section>
                        <section class="settings-panel" data-settings-panel="data" role="tabpanel">
                            <h2>${release.text("data")}</h2><p>${release.text("privacyLocal")}</p>
                            <div class="data-action-grid"><button class="game-button primary" data-save-now type="button">${release.text("saveNow")}</button><button class="game-button secondary" data-export-save type="button">${release.text("exportSave")}</button><button class="game-button secondary" data-import-save type="button">${release.text("importSave")}</button><button class="game-button danger" data-reset-settings type="button">${release.text("resetSettings")}</button></div>
                            <input class="hidden" type="file" accept="application/json,.json" data-import-file>
                            <div class="settings-data-summary"><span>${release.text("activeSave")}</span><strong>${runtime.activeSaveSlot || 1}</strong><span>${release.text("autoSave")}</span><strong>${release.getSettings().autoSave ? t("active") : t("inactive")}</strong></div>
                        </section>
                        <section class="settings-panel" data-settings-panel="about" role="tabpanel">
                            <div class="about-brand"><div class="about-logo">PC</div><div><span>${release.text("releaseBuild")}</span><h2>PC Shop Empire</h2><strong>v${release.version}</strong></div></div>
                            <div class="about-owner"><span>${release.text("ownerSignature")}</span><strong>${release.owner}</strong><p>© 2026 ${release.owner}. ${release.text("allRightsReserved")}</p></div>
                            <div class="about-notices"><p>${release.text("privacyLocal")}</p><p>${release.text("thirdPartyNotice")}</p></div>
                        </section>
                    </div>
                </div>
                <footer class="settings-footer"><button class="game-button ghost" data-reset-draft type="button">${release.text("reset")}</button><div><button class="game-button secondary" data-settings-close type="button">${release.text("close")}</button><button class="game-button primary" data-settings-apply type="button">${release.text("apply")}</button></div></footer>
            </div>`;
        document.body.appendChild(overlay);
        return overlay;
    };

    release.openSettings = async function openSettings() {
        let draft = { ...release.getSettings() };
        if (draft.language !== gameState.language) {
            changeLanguage(draft.language);
        }
        const overlay = release.createSettingsOverlay();
        const previousFocus = document.activeElement;
        const wasPaused = Boolean(gameState.paused);
        if (gameState.simulation?.difficulty) draft.difficulty = gameState.simulation.difficulty;

        const syncFields = () => {
            overlay.querySelectorAll("[data-setting]").forEach(field => {
                const key = field.dataset.setting;
                if (field.type === "checkbox") field.checked = Boolean(draft[key]);
                else field.value = String(draft[key]);
            });
            overlay.querySelectorAll("[data-setting-output]").forEach(output => {
                const key = output.dataset.settingOutput;
                const input = overlay.querySelector(`[data-setting="${key}"]`);
                output.textContent = `${draft[key]}${input?.dataset.suffix || ""}`;
            });
        };

        const close = applied => {
            overlay.classList.add("hidden");
            document.getElementById("app")?.removeAttribute("inert");
            if (runtime.gameStarted) gameState.paused = wasPaused;
            previousFocus?.focus?.();
            if (applied) release.audio.play("confirm");
        };

        if (runtime.gameStarted) gameState.paused = true;
        document.getElementById("app")?.setAttribute("inert", "");
        overlay.classList.remove("hidden");
        syncFields();
        overlay.querySelector(".settings-tab")?.focus();

        overlay.querySelectorAll("[data-settings-tab]").forEach(button => {
            button.onclick = () => {
                overlay.querySelectorAll("[data-settings-tab]").forEach(tab => {
                    const active = tab === button;
                    tab.classList.toggle("active", active);
                    tab.setAttribute("aria-selected", String(active));
                });
                overlay.querySelectorAll("[data-settings-panel]").forEach(panel => panel.classList.toggle("active", panel.dataset.settingsPanel === button.dataset.settingsTab));
                release.audio.play("navigate");
            };
        });

        overlay.querySelectorAll("[data-setting]").forEach(field => {
            field.oninput = () => {
                draft[field.dataset.setting] = field.type === "checkbox"
                    ? field.checked
                    : field.type === "range"
                        ? Number(field.value)
                        : field.value;
                const output = overlay.querySelector(`[data-setting-output="${field.dataset.setting}"]`);
                if (output) output.textContent = `${draft[field.dataset.setting]}${field.dataset.suffix || ""}`;
            };
        });

        overlay.querySelectorAll("[data-settings-close]").forEach(button => button.onclick = () => close(false));
        overlay.querySelector("[data-settings-apply]").onclick = () => {
            draft = release.persistSettings(draft);
            changeLanguage(draft.language);
            release.applySettings(draft);
            if (runtime.gameStarted) {
                gameState.simulation.difficulty = draft.difficulty;
                saveGame(false);
                safeRender();
            }
            showToast(release.text("settingsTitle"), release.text("settingsApplied"), "success");
            close(true);
        };
        overlay.querySelector("[data-reset-draft]").onclick = () => {
            draft = { ...release.defaultSettings, language: gameState.language, activeSaveSlot: runtime.activeSaveSlot || 1 };
            syncFields();
        };
        overlay.querySelector("[data-test-sound]").onclick = () => {
            release.audio.setSettings(release.sanitizeSettings(draft));
            release.audio.play("confirm");
        };
        overlay.querySelector("[data-save-now]").onclick = () => saveGame(true);
        overlay.querySelector("[data-export-save]").onclick = release.exportActiveSave;
        overlay.querySelector("[data-import-save]").onclick = () => overlay.querySelector("[data-import-file]").click();
        overlay.querySelector("[data-import-file]").onchange = event => release.importSaveFile(event.target.files?.[0]);
        overlay.querySelector("[data-reset-settings]").onclick = () => {
            draft = { ...release.defaultSettings, language: gameState.language, activeSaveSlot: runtime.activeSaveSlot || 1 };
            release.persistSettings(draft);
            release.applySettings(draft);
            syncFields();
            showToast(release.text("settingsTitle"), release.text("settingsReset"), "success");
        };

        overlay.onkeydown = event => {
            if (event.key === "Escape") {
                event.preventDefault();
                close(false);
                return;
            }
            if (event.key !== "Tab") return;
            const focusable = [...overlay.querySelectorAll("button:not([disabled]), input:not([disabled]), select:not([disabled])")]
                .filter(element => element.offsetParent !== null);
            if (!focusable.length) return;
            const first = focusable[0];
            const last = focusable[focusable.length - 1];
            if (event.shiftKey && document.activeElement === first) {
                event.preventDefault();
                last.focus();
            } else if (!event.shiftKey && document.activeElement === last) {
                event.preventDefault();
                first.focus();
            }
        };
    };

    release.exportActiveSave = function exportActiveSave() {
        if (!runtime.gameStarted) return false;
        saveGame(false);
        const payload = {
            application: "PC Shop Empire",
            version: release.version,
            saveSchema: release.saveSchema,
            owner: release.owner,
            exportedAt: new Date().toISOString(),
            slot: runtime.activeSaveSlot || 1,
            state: gameState
        };
        const blob = new Blob([JSON.stringify(payload, null, 2)], { type: "application/json" });
        const url = URL.createObjectURL(blob);
        const anchor = document.createElement("a");
        anchor.href = url;
        anchor.download = `PC-Shop-Empire-${release.version}-slot-${runtime.activeSaveSlot || 1}.json`;
        anchor.click();
        URL.revokeObjectURL(url);
        showToast(release.text("data"), release.text("saveExported"), "success");
        return true;
    };

    release.importSaveFile = function importSaveFile(file) {
        if (!file) return;
        const reader = new FileReader();
        reader.onload = () => {
            try {
                const payload = JSON.parse(String(reader.result));
                const state = payload?.state;
                const compatible = state && (
                    state.saveSchema === release.saveSchema
                    || state.version === release.saveSchema
                    || state.version === release.version
                );
                if (!compatible) throw new Error("Invalid save schema");
                const preferredLanguage = release.getSettings().language;
                gameState = state;
                gameState.language = preferredLanguage;
                runtime.activeSaveSlot = clamp(Number(payload.slot) || runtime.activeSaveSlot || 1, 1, SAVE_SLOT_COUNT);
                runtime.gameStarted = true;
                release.persistSettings({
                    ...release.getSettings(),
                    language: preferredLanguage,
                    activeSaveSlot: runtime.activeSaveSlot
                });
                normalizeGameState();
                saveGame(false);
                enterGameScreen();
                showToast(release.text("data"), release.text("saveImported"), "success");
                document.querySelector("[data-settings-close]")?.click();
            } catch (_error) {
                showToast(release.text("data"), release.text("invalidSaveFile"), "error");
            }
        };
        reader.readAsText(file);
    };
})(globalThis);
