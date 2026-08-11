const { contextBridge, ipcRenderer } = require("electron");

contextBridge.exposeInMainWorld("pcShopDesktop", {
    quit: () => ipcRenderer.send("pc-shop:quit"),
    toggleFullscreen: () =>
        ipcRenderer.send("pc-shop:toggle-fullscreen"),
    setFullscreen: enabled =>
        ipcRenderer.send("pc-shop:set-fullscreen", Boolean(enabled)),
    getFullscreen: () => ipcRenderer.invoke("pc-shop:get-fullscreen"),
    getAppInfo: () => ipcRenderer.invoke("pc-shop:get-app-info")
});
