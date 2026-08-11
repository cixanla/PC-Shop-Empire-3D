const { app, BrowserWindow, ipcMain, Menu } = require("electron");
const path = require("node:path");

if (require("electron-squirrel-startup")) {
    app.quit();
}

app.setAppUserModelId(
    "com.cixanla.pcshopempire"
);

app.setName("PC Shop Empire");

let mainWindow = null;

function createMainWindow() {
    mainWindow = new BrowserWindow({
        width: 1600,
        height: 960,

    icon: path.join(
        __dirname,
        "assets",
        "icon.ico"
    ),

        minWidth: 1180,
        minHeight: 720,

        backgroundColor: "#06101d",
        fullscreen: true,
        fullscreenable: true,
        show: false,

        autoHideMenuBar: true,

        title: `PC Shop Empire ${app.getVersion()}`,

        webPreferences: {
            preload: path.join(__dirname, "preload.js"),
            contextIsolation: true,
            nodeIntegration: false,
            sandbox: true
        }
    });

    mainWindow.loadFile(
        path.join(__dirname, "index.html")
    );

    mainWindow.webContents.setWindowOpenHandler(() => ({ action: "deny" }));
    mainWindow.webContents.on("will-navigate", event => event.preventDefault());

    mainWindow.once("ready-to-show", () => {
        mainWindow?.setFullScreen(true);
        mainWindow?.show();
    });

    mainWindow.on("closed", () => {
        mainWindow = null;
    });
}

ipcMain.on("pc-shop:quit", () => {
    app.quit();
});

ipcMain.on("pc-shop:toggle-fullscreen", event => {
    const window = BrowserWindow.fromWebContents(event.sender);

    if (window) {
        window.setFullScreen(!window.isFullScreen());
    }
});

ipcMain.on("pc-shop:set-fullscreen", (event, enabled) => {
    const window = BrowserWindow.fromWebContents(event.sender);
    window?.setFullScreen(Boolean(enabled));
});

ipcMain.handle("pc-shop:get-fullscreen", event => {
    const window = BrowserWindow.fromWebContents(event.sender);
    return Boolean(window?.isFullScreen());
});

ipcMain.handle("pc-shop:get-app-info", () => ({
    name: app.getName(),
    version: app.getVersion(),
    owner: "cixanla",
    platform: process.platform,
    copyright: "Copyright © 2026 cixanla. All rights reserved."
}));

function configureApplicationMenu() {
    if (process.platform !== "darwin") {
        Menu.setApplicationMenu(null);
        return;
    }

    Menu.setApplicationMenu(Menu.buildFromTemplate([
        {
            label: app.getName(),
            submenu: [
                { role: "about" },
                { type: "separator" },
                { role: "hide" },
                { role: "hideOthers" },
                { role: "unhide" },
                { type: "separator" },
                { role: "quit" }
            ]
        },
        {
            label: "View",
            submenu: [
                { role: "reload" },
                { type: "separator" },
                { role: "togglefullscreen", accelerator: "Ctrl+Command+F" }
            ]
        },
        { role: "windowMenu" }
    ]));
}

app.whenReady().then(() => {
    configureApplicationMenu();
    createMainWindow();

    app.on("activate", () => {
        if (
            BrowserWindow.getAllWindows().length === 0
        ) {
            createMainWindow();
        }
    });
});

app.on("window-all-closed", () => {
    if (process.platform !== "darwin") {
        app.quit();
    }
});
