const path = require("node:path");
const { FusesPlugin } = require('@electron-forge/plugin-fuses');
const { FuseV1Options, FuseVersion } = require('@electron/fuses');

const packagedWindowsRoot = path.join(
  __dirname,
  "out",
  "PC Shop Empire-win32-x64"
);

module.exports = {
  packagerConfig: {
    asar: true,
    appBundleId: "com.cixanla.pcshopempire",
    appCategoryType: "public.app-category.games",
    appCopyright: "Copyright © 2026 cixanla. All rights reserved.",
    executableName: "PC Shop Empire",
    icon: path.join(
        __dirname,
        "assets",
        "icon"
    ),
    win32metadata: {
      CompanyName: "cixanla",
      FileDescription: "PC Shop Empire - Computer Store Management Simulation",
      InternalName: "PCShopEmpire",
      OriginalFilename: "PC Shop Empire.exe",
      ProductName: "PC Shop Empire"
    },
    ignore: [
      /^\/(?:out|release|tests|docs|scripts|legacy)(?:\/|$)/,
      /^\/.*\.(?:zip|nupkg)$/i,
      /^\/(?:\.git|\.agents)(?:\/|$)/,
      /^\/forge\.config\.js$/
    ]
},
  rebuildConfig: {},
  makers: [
{
    name: "@electron-forge/maker-squirrel",

    config: {
        name: "PCShopEmpireCixanla",

        authors: "cixanla",
        owners: "cixanla",
        copyright: "Copyright © 2026 cixanla. All rights reserved.",

        description:
            "Professional computer store and technical service management simulation",

        additionalFiles: [
            {
              src: path.join(packagedWindowsRoot, "LICENSES.chromium.html"),
              target: "lib\\net45\\LICENSES.chromium.html"
            },
            {
              src: path.join(packagedWindowsRoot, "version"),
              target: "lib\\net45\\version"
            }
        ],

        setupIcon: path.join(
            __dirname,
            "assets",
            "icon.ico"
        )
    }
},
    {
      name: '@electron-forge/maker-zip',
      platforms: ['darwin'],
    },
    {
      name: '@electron-forge/maker-deb',
      config: {},
    },
    {
      name: '@electron-forge/maker-rpm',
      config: {},
    },
  ],
  plugins: [
    {
      name: '@electron-forge/plugin-auto-unpack-natives',
      config: {},
    },
    // Fuses are used to enable/disable various Electron functionality
    // at package time, before code signing the application
    new FusesPlugin({
      version: FuseVersion.V1,
      [FuseV1Options.RunAsNode]: false,
      [FuseV1Options.EnableCookieEncryption]: true,
      [FuseV1Options.EnableNodeOptionsEnvironmentVariable]: false,
      [FuseV1Options.EnableNodeCliInspectArguments]: false,
      [FuseV1Options.EnableEmbeddedAsarIntegrityValidation]: true,
      [FuseV1Options.OnlyLoadAppFromAsar]: true,
    }),
  ],
};
