using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.IO.Compression;

namespace RobloxDeployHistory
{
    internal class DeployDownloader
    {
        private static readonly Dictionary<string, string> Player = new Dictionary<string, string>()
        {
            ["RobloxApp.zip"] = "",
            ["shaders.zip"] = "shaders/",
            ["ssl.zip"] = "ssl/",

            ["WebView2.zip"] = "",
            ["WebView2RuntimeInstaller.zip"] = "WebView2RuntimeInstaller/",

            ["content-avatar.zip"] = "content/avatar/",
            ["content-configs.zip"] = "content/configs/",
            ["content-fonts.zip"] = "content/fonts/",
            ["content-sky.zip"] = "content/sky/",
            ["content-sounds.zip"] = "content/sounds/",
            ["content-textures2.zip"] = "content/textures/",
            ["content-models.zip"] = "content/models/",

            ["content-textures3.zip"] = "PlatformContent/pc/textures/",
            ["content-terrain.zip"] = "PlatformContent/pc/terrain/",
            ["content-platform-fonts.zip"] = "PlatformContent/pc/fonts/",

            ["extracontent-luapackages.zip"] = "ExtraContent/LuaPackages/",
            ["extracontent-translations.zip"] = "ExtraContent/translations/",
            ["extracontent-models.zip"] = "ExtraContent/models/",
            ["extracontent-textures.zip"] = "ExtraContent/textures/",
            ["extracontent-places.zip"] = "ExtraContent/places/",
        };

        private static readonly Dictionary<string, string> Studio = new Dictionary<string, string>()
        {
            ["RobloxStudio.zip"] = "",
            ["redist.zip"] = "",
            ["Libraries.zip"] = "",
            ["LibrariesQt5.zip"] = "",

            ["WebView2.zip"] = "",
            ["WebView2RuntimeInstaller.zip"] = "WebView2RuntimeInstaller/",

            ["shaders.zip"] = "shaders/",
            ["ssl.zip"] = "ssl/",

            ["Qml.zip"] = "Qml/",
            ["Plugins.zip"] = "Plugins/",
            ["StudioFonts.zip"] = "StudioFonts/",
            ["BuiltInPlugins.zip"] = "BuiltInPlugins/",
            ["ApplicationConfig.zip"] = "ApplicationConfig/",
            ["BuiltInStandalonePlugins.zip"] = "BuiltInStandalonePlugins/",

            ["content-qt_translations.zip"] = "content/qt_translations/",
            ["content-sky.zip"] = "content/sky/",
            ["content-fonts.zip"] = "content/fonts/",
            ["content-avatar.zip"] = "content/avatar/",
            ["content-models.zip"] = "content/models/",
            ["content-sounds.zip"] = "content/sounds/",
            ["content-configs.zip"] = "content/configs/",
            ["content-api-docs.zip"] = "content/api_docs/",
            ["content-textures2.zip"] = "content/textures/",
            ["content-studio_svg_textures.zip"] = "content/studio_svg_textures/",

            ["content-platform-fonts.zip"] = "PlatformContent/pc/fonts/",
            ["content-terrain.zip"] = "PlatformContent/pc/terrain/",
            ["content-textures3.zip"] = "PlatformContent/pc/textures/",

            ["extracontent-translations.zip"] = "ExtraContent/translations/",
            ["extracontent-luapackages.zip"] = "ExtraContent/LuaPackages/",
            ["extracontent-textures.zip"] = "ExtraContent/textures/",
            ["extracontent-scripts.zip"] = "ExtraContent/scripts/",
            ["extracontent-models.zip"] = "ExtraContent/models/",
        };

        public static async Task DownloadDeploy(Channel channel, string version, string path = "%USERPROFILE%\\Desktop", string name = "Roblox Deployment")
        {
            string channelPath = channel.Name.ToLowerInvariant() == "live"
                ? "https://setup.rbxcdn.com"
                : $"https://setup.rbxcdn.com/channel/{channel.Name.ToLowerInvariant()}";

            string basePATH = $"{channelPath}/{version}";

            Trace.WriteLine($"Fetching rbxPkgManifest.txt...");
            try
            {
                HttpClient client = new HttpClient();
                string pkg_manifest = await client.GetStringAsync($"{basePATH}-rbxPkgManifest.txt");

                Trace.WriteLine($"Fetched rbxPkgManifest!");

                string? deployType = pkg_manifest.Contains("RobloxApp.zip") ? "Player" : pkg_manifest.Contains("RobloxStudio.zip") ? "Studio" : null;

                if (deployType != null)
                {
                    Dictionary<string, string>? extractRoots = deployType == "Player" ? Player : deployType == "Studio" ? Studio : null;

                    string folderPath = $"{path}/{name}";

                    if (Directory.Exists(folderPath))
                    {
                        Directory.Delete(folderPath, true);
                    }

                    Directory.CreateDirectory(folderPath);

                    foreach (string dir in extractRoots.Values)
                    {
                        if (dir != "")
                        {
                            Directory.CreateDirectory($"{folderPath}/{dir}");
                        }
                    }

                    foreach (string file in pkg_manifest.Split(Environment.NewLine))
                    {
                        if (extractRoots.ContainsKey(file))
                        {
                            Trace.WriteLine($"Fetching {file}...");

                            using (var stream = await client.GetStreamAsync($"{basePATH}-{file}"))
                            {
                                string targetPath = $"{folderPath}/{extractRoots[file]}";
                                using (var archive = new ZipArchive(stream, ZipArchiveMode.Read))
                                {
                                    foreach (var entry in archive.Entries)
                                    {
                                        if (!string.IsNullOrEmpty(entry.Name) && !entry.FullName.Contains(".."))
                                        {
                                            string entryPath = Path.Combine(targetPath, entry.FullName);

                                            Directory.CreateDirectory(Path.GetDirectoryName(entryPath));

                                            entry.ExtractToFile(entryPath, overwrite: true);
                                        }
                                    }
                                }
                                Trace.WriteLine($"Fetched {file}!");
                            }

                        }
                    }

                    File.WriteAllText($"{folderPath}/AppSettings.xml", @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Settings>
    <ContentFolder>content</ContentFolder>
    <BaseUrl>http://www.roblox.com</BaseUrl>
</Settings>");
                }
                else Trace.WriteLine("Wrong Manifest, aborting...");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"[!] Error fetching manifest: {ex.Message}");
            }
        }
    }
}
