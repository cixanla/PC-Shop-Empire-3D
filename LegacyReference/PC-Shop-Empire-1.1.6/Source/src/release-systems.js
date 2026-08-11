"use strict";

(function initializeReleaseSystems(global) {
    const release = global.PCShopRelease;

    if (!release) {
        throw new Error("Release data must be loaded before release systems.");
    }

    const localize = value => value?.[gameState.language] || value?.tr || "";
    const average = values => values.length
        ? values.reduce((sum, value) => sum + value, 0) / values.length
        : 0;

    release.normalizeState = function normalizeReleaseState() {
        gameState.version = release.version;
        gameState.saveSchema = release.saveSchema;

        gameState.daily = {
            serviceJobs: 0,
            serviceRevenue: 0,
            tenderRevenue: 0,
            campaignSpend: 0,
            ...gameState.daily
        };

        gameState.lifetime = {
            serviceJobs: 0,
            tendersCompleted: 0,
            campaignsRun: 0,
            ...gameState.lifetime
        };

        gameState.nextIds = {
            service: 1,
            tender: 1,
            transaction: 1,
            ...gameState.nextIds
        };

        gameState.serviceCenter = {
            jobs: [],
            history: [],
            tenders: [],
            completed: 0,
            failed: 0,
            quality: 55,
            satisfaction: 60,
            ...gameState.serviceCenter
        };

        gameState.marketing = {
            awareness: 8,
            loyalty: 4,
            activeCampaign: null,
            campaignsRun: 0,
            reviews: [],
            ...gameState.marketing
        };

        gameState.marketDynamics = {
            cycleId: "calm",
            daysRemaining: 3,
            competitorPressure: 24,
            supplierRelations: {},
            supplierDeal: null,
            totalSupplierOrders: 0,
            ...gameState.marketDynamics
        };

        gameState.analytics = {
            history: [],
            ledger: [],
            ...gameState.analytics
        };

        gameState.career = {
            objectives: [],
            achievements: {},
            totalObjectiveRewards: 0,
            ...gameState.career
        };

        gameState.simulation = {
            difficulty: release.getSettings?.().difficulty || "normal",
            ...gameState.simulation
        };

        gameState.release115 = {
            initialized: false,
            ...gameState.release115
        };

        for (const employee of gameState.staff) {
            employee.morale = clamp(employee.morale ?? 74, 0, 100);
            employee.fatigue = clamp(employee.fatigue ?? 10, 0, 100);
            employee.specialty = employee.specialty || {
                sales: "relationship",
                technician: "diagnostics",
                buyer: "negotiation",
                accountant: "forecasting",
                manager: "leadership"
            }[employee.role] || "operations";
        }

        if (!gameState.career.objectives.length) {
            release.rollDailyObjectives();
        }

        if (!gameState.release115.initialized) {
            if (!gameState.serviceCenter.jobs.length) {
                release.generateServiceJobs(3);
            }

            if (!gameState.serviceCenter.tenders.length && gameState.reputation >= 10) {
                release.generateTenders(1);
            }

            gameState.release115.initialized = true;
        }
    };

    release.getDifficultyProfile = function getDifficultyProfile() {
        const difficulty = gameState.simulation?.difficulty
            || release.getSettings?.().difficulty
            || "normal";
        return {
            relaxed: { income: 1.14, expenses: 0.88, market: 0.94, service: 0.10 },
            normal: { income: 1, expenses: 1, market: 1, service: 0 },
            expert: { income: 0.91, expenses: 1.14, market: 1.08, service: -0.08 }
        }[difficulty] || { income: 1, expenses: 1, market: 1, service: 0 };
    };

    release.getCycle = function getCycle() {
        return release.marketCycles[gameState.marketDynamics?.cycleId]
            || release.marketCycles.calm;
    };

    release.getCampaign = function getCampaign() {
        const active = gameState.marketing?.activeCampaign;
        return active
            ? release.campaigns.find(campaign => campaign.id === active.id) || null
            : null;
    };

    release.getTrafficAdjustment = function getTrafficAdjustment() {
        const cycle = release.getCycle();
        const campaign = release.getCampaign();
        const awareness = gameState.marketing?.awareness || 0;
        const loyalty = gameState.marketing?.loyalty || 0;
        const pressure = gameState.marketDynamics?.competitorPressure || 0;
        return clamp(
            cycle.traffic
                + (campaign?.traffic || 0)
                + Math.floor(awareness / 24)
                + Math.floor(loyalty / 30)
                - Math.floor(pressure / 48),
            -2,
            9
        );
    };

    release.adjustCustomer = function adjustCustomer(customer) {
        if (!customer) {
            return customer;
        }

        const profile = release.getDifficultyProfile();
        const brandPremium = 1
            + (gameState.marketing?.awareness || 0) * 0.0015
            + (gameState.marketing?.loyalty || 0) * 0.001;
        const competition = 1 - (gameState.marketDynamics?.competitorPressure || 0) * 0.0008;
        customer.payment = Math.max(
            250,
            Math.round(customer.payment * profile.income * brandPremium * competition)
        );

        if (gameState.simulation?.difficulty === "relaxed") {
            customer.deadlineDays += 1;
            customer.originalDeadline += 1;
        } else if (gameState.simulation?.difficulty === "expert") {
            customer.deadlineDays = Math.max(1, customer.deadlineDays - 1);
        }

        customer.leadSource = release.getCampaign()?.id || "organic";
        customer.loyaltyChance = clamp((gameState.marketing?.loyalty || 0) / 150, 0, 0.55);
        return customer;
    };

    release.adjustMarketOffers = function adjustMarketOffers() {
        const cycle = release.getCycle();
        const profile = release.getDifficultyProfile();
        const deal = gameState.marketDynamics?.supplierDeal;

        for (const offer of gameState.marketOffers) {
            const relation = gameState.marketDynamics.supplierRelations[offer.seller]?.trust || 0;
            const relationDiscount = Math.min(0.045, relation * 0.00045);
            const dealDiscount = deal?.seller === offer.seller ? deal.discount : 0;
            offer.price = Math.max(
                12,
                Math.round(offer.price * cycle.price * profile.market * (1 - relationDiscount - dealDiscount))
            );
            const part = getPartById(offer.partId);
            const minimum = part?.type === "CPU" ? 12 : 4;
            offer.stock = Math.max(minimum, Math.round(offer.stock * cycle.stock));
            offer.marketCycle = gameState.marketDynamics.cycleId;
        }
    };

    release.generateServiceJobs = function generateServiceJobs(amount = 2) {
        release.normalizeStateShallow?.();
        const center = gameState.serviceCenter;
        const maximum = 7 + Math.floor(gameState.reputation / 60);

        for (let index = 0; index < amount && center.jobs.length < maximum; index += 1) {
            const available = release.serviceTemplates.filter(template =>
                template.difficulty <= 0.45 + gameState.reputation / 250
            );
            const template = randomItem(available.length ? available : release.serviceTemplates.slice(0, 2));
            const urgency = randomFloat(0.92, 1.18);
            center.jobs.push({
                id: createId("service"),
                templateId: template.id,
                customerName: `${randomItem(UNIVERSAL_FIRST_NAMES)} ${randomItem(UNIVERSAL_LAST_NAMES)}`,
                cost: Math.round(randomInt(template.cost[0], template.cost[1]) * urgency),
                payment: Math.round(randomInt(template.payment[0], template.payment[1]) * urgency),
                difficulty: clamp(template.difficulty + randomFloat(-0.06, 0.08), 0.1, 0.86),
                deadlineDays: randomInt(1, 4),
                createdDay: gameState.lifetime.daysCompleted
            });
        }
    };

    release.generateTenders = function generateTenders(amount = 1) {
        const center = gameState.serviceCenter;
        const maximum = 3;

        for (let index = 0; index < amount && center.tenders.length < maximum; index += 1) {
            const minimumScore = Math.round(clamp(
                150 + gameState.level * 18 + randomInt(-20, 85),
                140,
                620
            ));
            const quantity = randomInt(2, Math.min(5, 2 + Math.floor(gameState.level / 3)));
            const reward = Math.round(quantity * (minimumScore * randomFloat(4.2, 5.1) + 520));
            center.tenders.push({
                id: createId("tender"),
                company: randomItem([
                    "Northbridge Studio", "Atlas Architecture", "Verde Logistics",
                    "Nova Learning", "Lighthouse Media", "Meridian Labs", "Urban Grid"
                ]),
                minimumScore,
                quantity,
                reward,
                deadlineDays: randomInt(2, 5),
                reputationReward: randomInt(2, 4)
            });
        }
    };

    release.getBestTechnician = function getBestTechnician() {
        return [...gameState.staff]
            .filter(employee => employee.role === "technician")
            .sort((first, second) =>
                (second.quality + second.energy * 0.35 + second.morale * 0.2)
                - (first.quality + first.energy * 0.35 + first.morale * 0.2)
            )[0] || null;
    };

    release.getServiceSuccessChance = function getServiceSuccessChance(job, approach = "standard") {
        const technician = release.getBestTechnician();
        const quality = technician?.quality || 44;
        const energy = technician?.energy ?? 70;
        const morale = technician?.morale ?? 65;
        const premiumBonus = approach === "premium" ? 0.16 : 0;
        const workshopBonus = (gameState.upgrades.workshop || 0) * 0.018;
        const centerBonus = ((gameState.serviceCenter?.quality || 50) - 50) * 0.002;
        return clamp(
            0.55
                + quality * 0.0031
                + energy * 0.001
                + morale * 0.0007
                + workshopBonus
                + centerBonus
                + premiumBonus
                - job.difficulty * 0.36
                + release.getDifficultyProfile().service,
            0.28,
            0.97
        );
    };

    release.completeServiceJob = function completeServiceJob(jobId, approach = "standard") {
        const center = gameState.serviceCenter;
        const job = center.jobs.find(item => item.id === jobId);

        if (!job) {
            return false;
        }

        const technician = release.getBestTechnician();
        const premium = approach === "premium";
        const expense = Math.round(job.cost * (premium ? 1.65 : 1));

        if (!premium && technician && technician.energy < 12) {
            showToast(release.text("serviceCenter"), release.text("technicianRequired"), "warning");
            return false;
        }

        if (gameState.money < expense && gameState.money < -2500) {
            showToast(t("insufficientMoney"), formatMoney(expense), "warning");
            return false;
        }

        runtime.transactionContext = localize(
            release.serviceTemplates.find(template => template.id === job.templateId)?.names
        ) || release.text("serviceCenter");
        registerExpense(expense);
        runtime.transactionContext = null;

        const successProbability = release.getServiceSuccessChance(job, approach);
        const succeeded = chance(successProbability);
        let payout = 0;
        let reputationChange = 0;

        if (technician) {
            technician.energy = Math.max(0, technician.energy - (premium ? 9 : 17));
            technician.fatigue = clamp((technician.fatigue || 0) + (premium ? 5 : 10), 0, 100);
            technician.experience += succeeded ? 16 : 8;
            technician.morale = clamp((technician.morale || 70) + (succeeded ? 2 : -4), 0, 100);
        }

        if (succeeded) {
            payout = Math.round(job.payment * (premium ? 1.08 : 1));
            runtime.transactionContext = release.text("serviceCenter");
            registerRevenue(payout);
            runtime.transactionContext = null;
            reputationChange = job.difficulty > 0.6 ? 3 : 1;
            center.completed += 1;
            center.quality = clamp(center.quality + (premium ? 1.6 : 0.8), 0, 100);
            center.satisfaction = clamp(center.satisfaction + 1.2, 0, 100);
            gameState.marketing.loyalty = clamp(gameState.marketing.loyalty + randomFloat(0.4, 1.2), 0, 100);
            gameState.daily.serviceJobs += 1;
            gameState.daily.serviceRevenue += payout;
            gameState.lifetime.serviceJobs += 1;
            showToast(release.text("serviceCompleted"), `+${formatMoney(payout)}`, "success");
        } else {
            reputationChange = premium ? -1 : -2;
            center.failed += 1;
            center.quality = clamp(center.quality - (premium ? 0.8 : 1.8), 0, 100);
            center.satisfaction = clamp(center.satisfaction - 2.5, 0, 100);
            showToast(release.text("serviceFailed"), formatMoney(-expense), "error");
        }

        gameState.reputation = Math.max(0, gameState.reputation + reputationChange);
        center.jobs = center.jobs.filter(item => item.id !== jobId);
        center.history.unshift({
            ...job,
            approach,
            succeeded,
            payout,
            expense,
            completedAt: formatGameDate(),
            technicianId: technician?.id || null
        });
        center.history = center.history.slice(0, 40);
        addActivity(
            `${job.customerName} · ${release.text(succeeded ? "serviceCompleted" : "serviceFailed")} ${formatMoney(payout - expense)}`,
            "service"
        );
        release.checkAchievements();
        saveGame(false);
        safeRender();
        return succeeded;
    };

    release.getEligibleTenderComputers = function getEligibleTenderComputers(tender) {
        return gameState.builtComputers
            .filter(computer => computer.score >= tender.minimumScore)
            .sort((first, second) => first.score - second.score);
    };

    release.deliverTender = function deliverTender(tenderId) {
        const center = gameState.serviceCenter;
        const tender = center.tenders.find(item => item.id === tenderId);

        if (!tender) {
            return false;
        }

        const eligible = release.getEligibleTenderComputers(tender);

        if (eligible.length < tender.quantity) {
            showToast(
                release.text("corporateTenders"),
                `${release.text("eligibleComputers")}: ${eligible.length}/${tender.quantity}`,
                "warning"
            );
            return false;
        }

        const selectedIds = new Set(eligible.slice(0, tender.quantity).map(computer => computer.id));
        gameState.builtComputers = gameState.builtComputers.filter(computer => !selectedIds.has(computer.id));
        runtime.transactionContext = release.text("corporateTenders");
        registerRevenue(tender.reward);
        runtime.transactionContext = null;
        gameState.reputation += tender.reputationReward;
        gameState.daily.computersSold += tender.quantity;
        gameState.daily.tenderRevenue += tender.reward;
        gameState.lifetime.computersSold += tender.quantity;
        gameState.lifetime.tendersCompleted += 1;
        center.tenders = center.tenders.filter(item => item.id !== tenderId);
        addActivity(
            `${tender.company} · ${release.text("tenderDelivered")}: ${formatMoney(tender.reward)}.`,
            "sale"
        );
        showToast(release.text("tenderDelivered"), `+${formatMoney(tender.reward)}`, "success");
        release.checkAchievements();
        saveGame(false);
        safeRender();
        return true;
    };

    release.startCampaign = function startCampaign(campaignId) {
        const campaign = release.campaigns.find(item => item.id === campaignId);

        if (!campaign || gameState.marketing.activeCampaign) {
            return false;
        }

        if (gameState.money < campaign.cost) {
            showToast(t("insufficientMoney"), formatMoney(campaign.cost), "warning");
            return false;
        }

        runtime.transactionContext = release.text("campaigns");
        registerExpense(campaign.cost);
        runtime.transactionContext = null;
        gameState.daily.campaignSpend += campaign.cost;
        gameState.marketing.activeCampaign = {
            id: campaign.id,
            daysRemaining: campaign.duration,
            startedDay: gameState.lifetime.daysCompleted
        };
        gameState.marketing.campaignsRun += 1;
        gameState.lifetime.campaignsRun += 1;
        gameState.marketing.awareness = clamp(gameState.marketing.awareness + campaign.awareness * 0.35, 0, 100);
        addActivity(`${localize(campaign.names)} · ${release.text("campaignStarted")}`, "marketing");
        showToast(release.text("campaignStarted"), localize(campaign.names), "success");
        saveGame(false);
        safeRender();
        return true;
    };

    release.recordSupplierPurchase = function recordSupplierPurchase(seller, total) {
        if (!seller || !gameState.marketDynamics) {
            return;
        }

        const current = gameState.marketDynamics.supplierRelations[seller] || {
            trust: 0,
            orders: 0,
            spend: 0
        };
        current.orders += 1;
        current.spend += total;
        current.trust = clamp(current.trust + Math.min(2.5, 0.4 + total / 4500), 0, 100);
        gameState.marketDynamics.supplierRelations[seller] = current;
        gameState.marketDynamics.totalSupplierOrders += 1;
    };

    release.signSupplierDeal = function signSupplierDeal(seller) {
        const cost = 650;

        if (gameState.money < cost || !seller) {
            showToast(t("insufficientMoney"), formatMoney(cost), "warning");
            return false;
        }

        runtime.transactionContext = seller;
        registerExpense(cost);
        runtime.transactionContext = null;
        gameState.marketDynamics.supplierDeal = {
            seller,
            daysRemaining: 5,
            discount: 0.08
        };
        addActivity(`${seller} · ${release.text("supplierDealActivity")}`, "purchase");
        refreshMarket();
        saveGame(false);
        safeRender();
        return true;
    };

    release.rollDailyObjectives = function rollDailyObjectives() {
        const definitions = [...release.objectiveDefinitions]
            .sort(() => Math.random() - 0.5)
            .slice(0, 3);
        gameState.career.objectives = definitions.map(definition => ({
            id: definition.id,
            claimed: false,
            day: gameState.lifetime.daysCompleted
        }));
    };

    release.getObjectiveProgress = function getObjectiveProgress(objective) {
        const definition = release.objectiveDefinitions.find(item => item.id === objective.id);
        const progress = definition ? Math.max(0, definition.source(gameState)) : 0;
        return { definition, progress, complete: Boolean(definition && progress >= definition.target) };
    };

    release.claimObjective = function claimObjective(objectiveId, silent = false) {
        const objective = gameState.career.objectives.find(item => item.id === objectiveId);
        const detail = objective ? release.getObjectiveProgress(objective) : null;

        if (!objective || objective.claimed || !detail?.complete) {
            return false;
        }

        objective.claimed = true;
        runtime.transactionContext = release.text("dailyObjectives");
        registerRevenue(detail.definition.reward);
        runtime.transactionContext = null;
        gameState.career.totalObjectiveRewards += detail.definition.reward;

        if (!silent) {
            showToast(release.text("objectiveCompleted"), `+${formatMoney(detail.definition.reward)}`, "success");
        }

        saveGame(false);
        return true;
    };

    release.checkAchievements = function checkAchievements() {
        if (!gameState.career) {
            return [];
        }

        const unlocked = [];

        for (const definition of release.achievementDefinitions) {
            if (gameState.career.achievements[definition.id]) {
                continue;
            }

            const progress = definition.source(gameState);

            if (progress < definition.target) {
                continue;
            }

            gameState.career.achievements[definition.id] = {
                unlockedAt: formatGameDate(),
                reward: definition.reward
            };
            runtime.transactionContext = release.text("achievements");
            registerRevenue(definition.reward);
            runtime.transactionContext = null;
            unlocked.push(definition.id);
            showToast(
                `${release.text("achievementUnlocked")}: ${localize(definition.names)}`,
                `+${formatMoney(definition.reward)}`,
                "success",
                5600
            );
            addActivity(`${release.text("achievementUnlocked")}: ${localize(definition.names)}.`, "achievement");
        }

        return unlocked;
    };

    release.recordTransaction = function recordTransaction(type, amount) {
        if (!gameState.analytics || !amount) {
            return;
        }

        gameState.analytics.ledger.unshift({
            id: createId("transaction"),
            type,
            amount: Math.round(amount),
            category: runtime.transactionContext || (type === "revenue" ? release.text("revenue") : release.text("expense")),
            day: gameState.lifetime.daysCompleted + 1,
            date: formatGameDate(),
            time: minutesToTime(gameState.calendar.minutes),
            balance: gameState.money
        });
        gameState.analytics.ledger = gameState.analytics.ledger.slice(0, 500);
    };

    release.processDayClosing = function processDayClosing() {
        const center = gameState.serviceCenter;
        const expiredJobs = [];
        const expiredTenders = [];

        for (const objective of gameState.career.objectives) {
            const detail = release.getObjectiveProgress(objective);
            if (detail.complete && !objective.claimed) {
                release.claimObjective(objective.id, true);
            }
        }

        center.jobs.forEach(job => job.deadlineDays -= 1);
        center.jobs = center.jobs.filter(job => {
            if (job.deadlineDays <= 0) {
                expiredJobs.push(job);
                return false;
            }
            return true;
        });

        center.tenders.forEach(tender => tender.deadlineDays -= 1);
        center.tenders = center.tenders.filter(tender => {
            if (tender.deadlineDays <= 0) {
                expiredTenders.push(tender);
                return false;
            }
            return true;
        });

        if (expiredJobs.length) {
            gameState.reputation = Math.max(0, gameState.reputation - expiredJobs.length);
            center.satisfaction = clamp(center.satisfaction - expiredJobs.length * 1.8, 0, 100);
        }

        if (expiredTenders.length) {
            gameState.reputation = Math.max(0, gameState.reputation - expiredTenders.length * 2);
        }

        const campaign = release.getCampaign();
        if (campaign && gameState.marketing.activeCampaign) {
            gameState.marketing.activeCampaign.daysRemaining -= 1;
            gameState.marketing.awareness = clamp(gameState.marketing.awareness + campaign.awareness / campaign.duration, 0, 100);
            gameState.marketing.loyalty = clamp(gameState.marketing.loyalty + campaign.loyalty / campaign.duration, 0, 100);

            if (gameState.marketing.activeCampaign.daysRemaining <= 0) {
                addActivity(`${localize(campaign.names)} · ${release.text("campaignFinished")}`, "marketing");
                gameState.marketing.activeCampaign = null;
            }
        } else {
            gameState.marketing.awareness = clamp(gameState.marketing.awareness - 0.25, 0, 100);
        }

        const deal = gameState.marketDynamics.supplierDeal;
        if (deal) {
            deal.daysRemaining -= 1;
            if (deal.daysRemaining <= 0) {
                gameState.marketDynamics.supplierDeal = null;
            }
        }

        gameState.marketDynamics.daysRemaining -= 1;
        let cycleChanged = false;

        if (gameState.marketDynamics.daysRemaining <= 0) {
            const ids = Object.keys(release.marketCycles)
                .filter(id => id !== gameState.marketDynamics.cycleId);
            gameState.marketDynamics.cycleId = randomItem(ids);
            gameState.marketDynamics.daysRemaining = randomInt(2, 4);
            cycleChanged = true;
        }

        gameState.marketDynamics.competitorPressure = clamp(
            gameState.marketDynamics.competitorPressure + randomInt(-6, 7)
                - Math.floor((gameState.marketing.awareness || 0) / 35),
            8,
            92
        );

        for (const employee of gameState.staff) {
            const managerEffect = getManagerStrength() * 12;
            employee.fatigue = clamp((employee.fatigue || 0) + (100 - employee.energy) * 0.08 - 6, 0, 100);
            employee.morale = clamp(
                (employee.morale || 70)
                    + managerEffect
                    + (employee.energy > 45 ? 1.5 : -3)
                    + (gameState.daily.revenue > gameState.daily.expenses ? 1 : -1),
                0,
                100
            );
        }

        const summary = {
            expiredJobs: expiredJobs.length,
            expiredTenders: expiredTenders.length,
            cycleChanged,
            objectiveRewards: gameState.career.objectives
                .filter(objective => objective.claimed)
                .reduce((sum, objective) => {
                    const definition = release.objectiveDefinitions.find(item => item.id === objective.id);
                    return sum + (definition?.reward || 0);
                }, 0)
        };
        runtime.releaseClosingSummary = summary;
        return summary;
    };

    release.recordDayHistory = function recordDayHistory(report) {
        if (!report || !gameState.analytics) {
            return;
        }

        gameState.analytics.history.push({
            day: gameState.lifetime.daysCompleted,
            date: report.date,
            revenue: report.revenue,
            expenses: report.expenses,
            net: report.net,
            cash: gameState.money,
            reputation: gameState.reputation,
            computersBuilt: report.computersBuilt || 0,
            computersSold: report.computersSold || 0,
            serviceJobs: gameState.daily.serviceJobs || 0,
            serviceRevenue: gameState.daily.serviceRevenue || 0,
            tenderRevenue: gameState.daily.tenderRevenue || 0,
            marketCycle: gameState.marketDynamics.cycleId
        });
        gameState.analytics.history = gameState.analytics.history.slice(-90);
    };

    release.prepareNewDay = function prepareNewDay() {
        release.generateServiceJobs(randomInt(2, 4));
        const campaign = release.getCampaign();
        const tenderChance = 0.28
            + release.getCycle().tender
            + (campaign?.tenderBonus || 0)
            + gameState.reputation / 500;

        if (chance(clamp(tenderChance, 0.15, 0.82))) {
            release.generateTenders(1);
        }

        release.rollDailyObjectives();
        release.checkAchievements();
    };

    release.getBusinessMetrics = function getBusinessMetrics() {
        const history = gameState.analytics?.history || [];
        const recent = history.slice(-7);
        const averageNet = average(recent.map(day => day.net));
        const averageRevenue = average(recent.map(day => day.revenue));
        const averageExpenses = average(recent.map(day => day.expenses));
        const margin = averageRevenue > 0
            ? ((averageRevenue - averageExpenses) / averageRevenue) * 100
            : 0;
        const staffEnergy = average(gameState.staff.map(employee => employee.energy));
        const inventoryUnits = getInventoryCount();
        const salesPerDay = average(recent.map(day => day.computersSold));
        const turnover = inventoryUnits > 0 ? salesPerDay / inventoryUnits * 100 : 0;
        let risk = 18;
        risk += gameState.money < 0 ? 35 : gameState.money < 2500 ? 18 : 0;
        risk += inventoryUnits < REQUIRED_COMPONENT_TYPES.length ? 12 : 0;
        risk += staffEnergy < 35 && gameState.staff.length ? 14 : 0;
        risk += (gameState.marketDynamics?.competitorPressure || 0) * 0.18;
        risk += gameState.finance?.emergencyLoanBalance > 0 ? 12 : 0;
        risk -= Math.min(15, gameState.reputation / 8);
        risk = clamp(Math.round(risk), 3, 98);
        const health = clamp(Math.round(100 - risk + Math.max(-12, Math.min(16, margin))), 0, 100);
        return {
            recent,
            averageNet,
            averageRevenue,
            averageExpenses,
            margin,
            staffEnergy,
            turnover,
            risk,
            health,
            forecast: Math.round(gameState.money + averageNet * 7)
        };
    };

    release.getHealthLabel = function getHealthLabel(value) {
        if (value >= 82) return release.text("excellent");
        if (value >= 62) return release.text("stable");
        if (value >= 38) return release.text("fragile");
        return release.text("critical");
    };

    release.renderServicePage = function renderServicePage() {
        const content = document.getElementById("page-content");
        if (!content) return;

        const center = gameState.serviceCenter;
        const technician = release.getBestTechnician();
        const jobs = center.jobs.map(job => {
            const template = release.serviceTemplates.find(item => item.id === job.templateId);
            const standardChance = release.getServiceSuccessChance(job, "standard");
            const premiumChance = release.getServiceSuccessChance(job, "premium");
            return `
                <article class="release-card service-job-card">
                    <div class="release-card-heading">
                        <span class="release-card-icon">${template?.icon || "⌁"}</span>
                        <div><strong>${escapeHtml(localize(template?.names))}</strong><span>${escapeHtml(job.customerName)}</span></div>
                        <span class="deadline-chip ${job.deadlineDays <= 1 ? "danger" : ""}">${job.deadlineDays} ${release.text("days")}</span>
                    </div>
                    <div class="release-stat-pair"><span>${release.text("partsCost")}</span><strong>${formatMoney(job.cost)}</strong></div>
                    <div class="release-stat-pair"><span>${release.text("expectedPayment")}</span><strong class="text-success">${formatMoney(job.payment)}</strong></div>
                    <div class="probability-row"><span>${release.text("successChance")}</span><div><i style="width:${Math.round(standardChance * 100)}%"></i></div><strong>${Math.round(standardChance * 100)}%</strong></div>
                    <div class="release-card-actions">
                        <button class="game-button secondary" data-service-job="${job.id}" data-service-approach="standard" type="button">${release.text("standardRepair")}</button>
                        <button class="game-button primary" data-service-job="${job.id}" data-service-approach="premium" type="button">${release.text("premiumRepair")} · ${Math.round(premiumChance * 100)}%</button>
                    </div>
                </article>`;
        }).join("");

        const tenders = center.tenders.map(tender => {
            const eligible = release.getEligibleTenderComputers(tender).length;
            return `
                <article class="release-card tender-card">
                    <div class="release-card-heading"><span class="release-card-icon">▰</span><div><strong>${escapeHtml(tender.company)}</strong><span>${release.text("corporateTenders")}</span></div><span class="deadline-chip">${tender.deadlineDays} ${release.text("days")}</span></div>
                    <div class="tender-requirements">
                        <div><span>${release.text("requiredScore")}</span><strong>${tender.minimumScore}+</strong></div>
                        <div><span>${release.text("quantity")}</span><strong>${tender.quantity}</strong></div>
                        <div><span>${release.text("eligibleComputers")}</span><strong class="${eligible >= tender.quantity ? "text-success" : "text-warning"}">${eligible}/${tender.quantity}</strong></div>
                    </div>
                    <div class="tender-payout"><span>${release.text("reward")}</span><strong>${formatMoney(tender.reward)}</strong></div>
                    <button class="game-button primary full-width" data-tender-deliver="${tender.id}" type="button" ${eligible < tender.quantity ? "disabled" : ""}>${release.text("deliverTender")}</button>
                </article>`;
        }).join("");

        const history = center.history.slice(0, 8).map(entry => {
            const template = release.serviceTemplates.find(item => item.id === entry.templateId);
            return `<div class="release-history-row"><span class="status-dot ${entry.succeeded ? "success" : "danger"}"></span><div><strong>${escapeHtml(localize(template?.names))}</strong><span>${escapeHtml(entry.customerName)} · ${escapeHtml(entry.completedAt)}</span></div><strong class="${entry.payout - entry.expense >= 0 ? "text-success" : "text-danger"}">${formatMoney(entry.payout - entry.expense)}</strong></div>`;
        }).join("");

        content.innerHTML = createPageHeader(release.text("serviceCenter"), release.text("serviceCenterDescription")) + `
            <div class="release-metric-grid four">
                ${createMetricCard(release.text("openJobs"), String(center.jobs.length), `${release.text("deadline")}: ${center.jobs.length ? Math.min(...center.jobs.map(job => job.deadlineDays)) : "—"}`)}
                ${createMetricCard(release.text("serviceQuality"), `${Math.round(center.quality)}%`, release.getHealthLabel(center.quality))}
                ${createMetricCard(release.text("customerLoyalty"), `${Math.round(gameState.marketing.loyalty)}%`, `${center.completed} ${release.text("completed").toLocaleLowerCase()}`)}
                ${createMetricCard(release.text("technician"), technician ? `${Math.round(technician.energy)}%` : "—", technician ? technician.name : release.text("ownerManaged"))}
            </div>
            <section class="release-section">
                <div class="release-section-header"><div><h2>${release.text("openJobs")}</h2><p>${center.jobs.length} ${release.text("activeJobs")}</p></div></div>
                <div class="release-card-grid">${jobs || `<div class="release-empty">${release.text("noServiceJobs")}</div>`}</div>
            </section>
            <section class="release-section">
                <div class="release-section-header"><div><h2>${release.text("corporateTenders")}</h2><p>${release.text("bulkSystemDeliveries")}</p></div></div>
                <div class="release-card-grid tenders">${tenders || `<div class="release-empty">${release.text("noTenders")}</div>`}</div>
            </section>
            <section class="release-section compact"><div class="release-section-header"><div><h2>${release.text("serviceHistory")}</h2></div></div><div class="release-history">${history || `<div class="release-empty small">—</div>`}</div></section>`;

        content.querySelectorAll("[data-service-job]").forEach(button => button.addEventListener("click", () =>
            release.completeServiceJob(button.dataset.serviceJob, button.dataset.serviceApproach)
        ));
        content.querySelectorAll("[data-tender-deliver]").forEach(button => button.addEventListener("click", () =>
            release.deliverTender(button.dataset.tenderDeliver)
        ));
    };

    release.renderBrandPage = function renderBrandPage() {
        const content = document.getElementById("page-content");
        if (!content) return;
        const cycle = release.getCycle();
        const campaign = release.getCampaign();
        const active = gameState.marketing.activeCampaign;
        const relationEntries = Object.entries(gameState.marketDynamics.supplierRelations)
            .sort((first, second) => second[1].trust - first[1].trust)
            .slice(0, 6);
        const suppliers = [...new Set([...relationEntries.map(entry => entry[0]), ...MARKET_SELLERS])]
            .slice(0, 6)
            .map(seller => {
                const relation = gameState.marketDynamics.supplierRelations[seller] || { trust: 0, orders: 0, spend: 0 };
                const selected = gameState.marketDynamics.supplierDeal?.seller === seller;
                return `<article class="supplier-card ${selected ? "selected" : ""}"><div><strong>${escapeHtml(seller)}</strong><span>${relation.orders} ${release.text("orders")} · ${formatMoney(relation.spend)}</span></div><div class="supplier-trust"><i style="width:${relation.trust}%"></i></div><button class="mini-action-button" data-supplier-deal="${escapeHtml(seller)}" type="button" ${selected ? "disabled" : ""}>${selected ? `%8 · ${gameState.marketDynamics.supplierDeal.daysRemaining} ${release.text("days")}` : `${release.text("supplierDeal")} · ${formatMoney(650)}`}</button></article>`;
            }).join("");

        content.innerHTML = createPageHeader(release.text("brandMarket"), release.text("brandMarketDescription")) + `
            <section class="market-cycle-hero cycle-${gameState.marketDynamics.cycleId}">
                <div><span>${release.text("economyPulse")}</span><h2>${release.text(cycle.name)}</h2><p>${release.text(cycle.description)}</p></div>
                <div class="cycle-indices">
                    <div><span>${release.text("priceIndex")}</span><strong>${Math.round(cycle.price * 100)}</strong></div>
                    <div><span>${release.text("stockIndex")}</span><strong>${Math.round(cycle.stock * 100)}</strong></div>
                    <div><span>${release.text("competitorPressure")}</span><strong>${Math.round(gameState.marketDynamics.competitorPressure)}</strong></div>
                    <div><span>${release.text("changesIn")}</span><strong>${gameState.marketDynamics.daysRemaining} ${release.text("days")}</strong></div>
                </div>
            </section>
            <div class="release-metric-grid three">
                ${createMetricCard(release.text("awareness"), `${Math.round(gameState.marketing.awareness)}%`, `${release.getTrafficAdjustment() >= 0 ? "+" : ""}${release.getTrafficAdjustment()} ${release.text("trafficBonus")}`)}
                ${createMetricCard(release.text("customerLoyalty"), `${Math.round(gameState.marketing.loyalty)}%`, `${gameState.serviceCenter.completed} ${release.text("serviceDeliveries")}`)}
                ${createMetricCard(release.text("activeCampaign"), campaign ? localize(campaign.names) : "—", active ? `${active.daysRemaining} ${release.text("days")}` : release.text("noActiveCampaign"))}
            </div>
            <section class="release-section">
                <div class="release-section-header"><div><h2>${release.text("campaigns")}</h2><p>${release.text("campaignsDescription")}</p></div></div>
                <div class="campaign-grid">
                    ${release.campaigns.map(item => `<article class="campaign-card ${active?.id === item.id ? "active" : ""}"><span class="campaign-icon">${item.icon}</span><div><h3>${escapeHtml(localize(item.names))}</h3><p>${escapeHtml(localize(item.descriptions))}</p></div><div class="campaign-stats"><span>+${item.traffic} ${release.text("trafficBonus")}</span><span>${item.duration} ${release.text("days")}</span><strong>${formatMoney(item.cost)}</strong></div><button class="game-button ${active?.id === item.id ? "secondary" : "primary"} full-width" data-start-campaign="${item.id}" type="button" ${active || gameState.money < item.cost ? "disabled" : ""}>${active?.id === item.id ? release.text("campaignRunning") : release.text("startCampaign")}</button></article>`).join("")}
                </div>
            </section>
            <section class="release-section compact">
                <div class="release-section-header"><div><h2>${release.text("supplierRelations")}</h2><p>${release.text("supplierRelationsDescription")}</p></div></div>
                <div class="supplier-grid">${suppliers}</div>
            </section>`;

        content.querySelectorAll("[data-start-campaign]").forEach(button => button.addEventListener("click", () => release.startCampaign(button.dataset.startCampaign)));
        content.querySelectorAll("[data-supplier-deal]").forEach(button => button.addEventListener("click", () => release.signSupplierDeal(button.dataset.supplierDeal)));
    };

    release.renderIntelligencePage = function renderIntelligencePage() {
        const content = document.getElementById("page-content");
        if (!content) return;
        const metrics = release.getBusinessMetrics();
        const history = metrics.recent;
        const maximum = Math.max(1, ...history.flatMap(day => [day.revenue, day.expenses]));
        const chart = history.map(day => `<div class="finance-chart-day"><div class="finance-bars"><i class="revenue" style="height:${Math.max(4, day.revenue / maximum * 100)}%"></i><i class="expense" style="height:${Math.max(4, day.expenses / maximum * 100)}%"></i></div><span>${day.day}</span><strong class="${day.net >= 0 ? "text-success" : "text-danger"}">${formatMoney(day.net)}</strong></div>`).join("");
        const riskLabel = metrics.risk < 35 ? release.text("lowRisk") : metrics.risk < 65 ? release.text("mediumRisk") : release.text("highRisk");
        const ledger = gameState.analytics.ledger.slice(0, 12).map(entry => `<div class="ledger-row"><span>${escapeHtml(entry.time)}</span><div><strong>${escapeHtml(release.text(entry.category))}</strong><small>${escapeHtml(entry.date)}</small></div><strong class="${entry.type === "revenue" ? "text-success" : "text-danger"}">${entry.type === "revenue" ? "+" : "−"}${formatMoney(entry.amount)}</strong><span>${formatMoney(entry.balance)}</span></div>`).join("");

        content.innerHTML = createPageHeader(release.text("intelligence"), release.text("intelligenceDescription")) + `
            <div class="release-metric-grid four">
                ${createMetricCard(release.text("businessHealth"), `${metrics.health}/100`, release.getHealthLabel(metrics.health), metrics.health >= 62 ? "▲" : "▼", metrics.health >= 62 ? "positive" : "negative")}
                ${createMetricCard(release.text("cashForecast"), formatMoney(metrics.forecast), `${formatMoney(metrics.averageNet)} · ${release.text("perDay")}`, metrics.forecast >= gameState.money ? "▲" : "▼", metrics.forecast >= gameState.money ? "positive" : "negative")}
                ${createMetricCard(release.text("profitMargin"), `${metrics.margin.toFixed(1)}%`, release.text("lastSevenDays"))}
                ${createMetricCard(release.text("riskScore"), `${metrics.risk}/100`, riskLabel, metrics.risk < 35 ? "●" : "!", metrics.risk < 35 ? "positive" : "negative")}
            </div>
            <div class="intelligence-layout">
                <section class="release-section chart-panel">
                    <div class="release-section-header"><div><h2>${release.text("financialHistory")}</h2><p>${release.text("lastSevenDays")}</p></div><div class="chart-legend"><span><i class="revenue"></i>${release.text("revenue")}</span><span><i class="expense"></i>${release.text("expense")}</span></div></div>
                    ${history.length ? `<div class="finance-chart">${chart}</div>` : `<div class="release-empty">${release.text("noHistory")}</div>`}
                </section>
                <section class="release-section health-panel">
                    <div class="release-section-header"><div><h2>${release.text("operationalEfficiency")}</h2></div></div>
                    <div class="health-ring" style="--health:${metrics.health * 3.6}deg"><div><strong>${metrics.health}</strong><span>${release.getHealthLabel(metrics.health)}</span></div></div>
                    <div class="efficiency-list">
                        <div><span>${release.text("inventoryTurnover")}</span><strong>${metrics.turnover.toFixed(1)}%</strong></div>
                        <div><span>${release.text("averageStaffEnergy")}</span><strong>${metrics.staffEnergy.toFixed(0)}%</strong></div>
                        <div><span>${release.text("serviceQuality")}</span><strong>${gameState.serviceCenter.quality.toFixed(0)}%</strong></div>
                        <div><span>${release.text("customerLoyalty")}</span><strong>${gameState.marketing.loyalty.toFixed(0)}%</strong></div>
                    </div>
                </section>
            </div>
            <section class="release-section compact"><div class="release-section-header"><div><h2>${release.text("transactionLedger")}</h2><p>${release.text("ledgerDescription")}</p></div></div><div class="ledger-table">${ledger || `<div class="release-empty small">—</div>`}</div></section>`;
    };

    release.renderCareerPage = function renderCareerPage() {
        const content = document.getElementById("page-content");
        if (!content) return;
        release.checkAchievements();
        const objectives = gameState.career.objectives.map(objective => {
            const detail = release.getObjectiveProgress(objective);
            const percent = Math.min(100, detail.progress / detail.definition.target * 100);
            return `<article class="objective-card ${detail.complete ? "complete" : ""}"><div><span class="objective-state">${objective.claimed ? "✓" : detail.complete ? "!" : "○"}</span><div><strong>${escapeHtml(localize(detail.definition.labels))}</strong><span>${Math.round(detail.progress)}/${detail.definition.target} · ${formatMoney(detail.definition.reward)}</span></div></div><div class="objective-progress"><i style="width:${percent}%"></i></div>${detail.complete && !objective.claimed ? `<button class="game-button primary" data-claim-objective="${objective.id}" type="button">${release.text("claimReward")}</button>` : `<span class="objective-status">${objective.claimed ? release.text("completed") : release.text("inProgress")}</span>`}</article>`;
        }).join("");
        const achievements = release.achievementDefinitions.map(definition => {
            const progress = definition.source(gameState);
            const unlocked = gameState.career.achievements[definition.id];
            const percent = Math.min(100, progress / definition.target * 100);
            return `<article class="achievement-card ${unlocked ? "unlocked" : "locked"}"><span class="achievement-icon">${definition.icon}</span><div><h3>${escapeHtml(localize(definition.names))}</h3><p>${escapeHtml(localize(definition.descriptions))}</p><div class="achievement-progress"><i style="width:${percent}%"></i></div><span>${Math.round(progress)}/${definition.target} · ${formatMoney(definition.reward)}</span></div><strong>${unlocked ? "✓" : Math.round(percent) + "%"}</strong></article>`;
        }).join("");
        const stage = getStoreStage();

        content.innerHTML = createPageHeader(release.text("career"), release.text("careerDescription")) + `
            <section class="career-hero"><div><span>${release.text("ownerSignature")}</span><h2>${release.owner}</h2><p>PC Shop Empire · ${release.version}</p></div><div class="career-level"><span>${release.text("levelLabel")}</span><strong>${gameState.level}</strong><small>${escapeHtml(stage.name)}</small></div><div class="career-stats"><div><span>${release.text("daysCompletedLabel")}</span><strong>${gameState.lifetime.daysCompleted}</strong></div><div><span>${release.text("lifetimeRevenueLabel")}</span><strong>${formatMoney(gameState.lifetime.revenue)}</strong></div><div><span>${release.text("reputationLabel")}</span><strong>${gameState.reputation}</strong></div></div></section>
            <section class="release-section compact"><div class="release-section-header"><div><h2>${release.text("dailyObjectives")}</h2><p>${release.text("objectivesDescription")}</p></div></div><div class="objective-list">${objectives}</div></section>
            <section class="release-section"><div class="release-section-header"><div><h2>${release.text("achievements")}</h2><p>${Object.keys(gameState.career.achievements).length}/${release.achievementDefinitions.length} ${release.text("completed").toLocaleLowerCase()}</p></div></div><div class="achievement-grid">${achievements}</div></section>`;

        content.querySelectorAll("[data-claim-objective]").forEach(button => button.addEventListener("click", () => {
            if (release.claimObjective(button.dataset.claimObjective)) {
                release.renderCareerPage();
            }
        }));
    };

    release.decorateMarketPage = function decorateMarketPage() {
        const content = document.getElementById("page-content");
        if (!content || runtime.currentPage !== "market") return;
        const cycle = release.getCycle();
        content.insertAdjacentHTML("afterbegin", `<div class="market-pulse-strip cycle-${gameState.marketDynamics.cycleId}"><div><span class="live-dot"></span><strong>${release.text(cycle.name)}</strong><span>${release.text(cycle.description)}</span></div><div><span>${release.text("priceIndex")}</span><strong>${Math.round(cycle.price * 100)}</strong><span>${release.text("stockIndex")}</span><strong>${Math.round(cycle.stock * 100)}</strong></div></div>`);
    };

    release.decorateDashboard = function decorateDashboard() {
        const content = document.getElementById("page-content");
        if (!content || runtime.currentPage !== "dashboard") return;
        const metrics = release.getBusinessMetrics();
        const campaign = release.getCampaign();
        content.insertAdjacentHTML("afterbegin", `<section class="executive-strip"><div class="executive-health"><span>${release.text("businessHealth")}</span><strong>${metrics.health}</strong><i><b style="width:${metrics.health}%"></b></i></div><div><span>${release.text("marketCycle")}</span><strong>${release.text(release.getCycle().name)}</strong></div><div><span>${release.text("activeCampaign")}</span><strong>${campaign ? escapeHtml(localize(campaign.names)) : "—"}</strong></div><div><span>${release.text("cashForecast")}</span><strong class="${metrics.forecast >= 0 ? "text-success" : "text-danger"}">${formatMoney(metrics.forecast)}</strong></div><button class="game-button secondary" data-page-jump="intelligence" type="button">${release.text("intelligence")} →</button></section>`);
        content.querySelector("[data-page-jump]")?.addEventListener("click", event => navigateToPage(event.currentTarget.dataset.pageJump));
    };
})(globalThis);
