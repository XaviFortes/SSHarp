using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace SSHarp
{
    public class GitHubReleaseChecker
    {
        private const string RepositoryOwner = "XaviFortes";
        private const string RepositoryName = "SSHarp";
        readonly string appDataFolder = Globals.AppDataFolder;

        public string GetCurrentVersion()
        {
            Assembly assembly = Assembly.GetEntryAssembly();
            Version version = assembly.GetName().Version;
            string currentVersion = version.ToString();

            return currentVersion;
        }

        public async Task CheckForUpdates(string currentVersion)
        {
            string latestVersion = await GetLatestReleaseVersion();

            if (!currentVersion.Equals(latestVersion))
            {
                MessageBoxResult result = MessageBox.Show("A new version is available. Do you want to download and install it?", "Update Available", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    await DownloadAndInstallLatestVersion();
                }
            }
        }

        public async Task<string> GetLatestReleaseVersion()
        {
            string apiUrl = $"https://api.github.com/repos/{RepositoryOwner}/{RepositoryName}/releases/latest";

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "SSHarp");

                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();

                    dynamic release = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                    string latestVersion = release.tag_name;

                    return latestVersion;
                }
                else
                {
                    // Handle the error case when the API request fails
                    Debug.WriteLine($"Failed to retrieve the latest release version. Status code: {response.StatusCode}");
                    throw new HttpRequestException($"Failed to retrieve the latest release version. Status code: {response.StatusCode}");
                }
            }
        }

        private async Task DownloadAndInstallLatestVersion()
        {
            string latestVersion = await GetLatestReleaseVersion();
            string downloadUrl = $"https://github.com/{RepositoryOwner}/{RepositoryName}/releases/latest/download/Setup.msi";

            // Perform the download and installation process using appropriate methods
            // For example, you can use WebClient.DownloadFileAsync to download the installer
            // and Process.Start to launch the installer.

            // Here's an example of using WebClient to download the installer file:
            string installerPath = Path.Combine(appDataFolder, $"Setup-{latestVersion}.msi");
            using (WebClient client = new WebClient())
            {
                await client.DownloadFileTaskAsync(downloadUrl, installerPath);
            }

            // After the download, you can launch the installer
            Process.Start(installerPath);

            // Close the current application
            Application.Current.Shutdown();
        }
    }
}
