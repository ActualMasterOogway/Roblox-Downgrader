using System.Text;
using System.Windows;
using System.IO;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;
using RobloxDeployHistory;

namespace RobloxDowngrader
{
    /// <summary>
    /// Interaction logic for Launcher.xaml
    /// </summary>
    public partial class Launcher : Window
    {
        private string bloxstrapDirectory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Bloxstrap");
        private DeployLog? targetedVersion;
        private string? versionsDirectory;
        private Channel Channel = new Channel("LIVE");

        private void Lock(bool doLock = true)
        {
            doLock = !doLock;
            this.targetVersion.IsEnabled = doLock;
            this.logo.IsEnabled = doLock;
            this.title.IsEnabled = doLock;
            this.downgrade.IsEnabled = doLock;
            this.opendir.IsEnabled = doLock;
            this.targetVersionLabel.IsEnabled = doLock;
        }

        public void Initialize()
        {
            this.versionsDirectory = this.bloxstrapDirectory + "\\Versions";

            if (Directory.Exists(this.versionsDirectory) == false)
            {
                MessageBox.Show("The Bloxstrap Versions could not be found. Please check the installation and make sure you have Bloxstrap installed.", "Folder Not Found", MessageBoxButton.OK, MessageBoxImage.Error);
                Process.Start("explorer.exe", "https://github.com/pizzaboxer/bloxstrap/releases/latest");
                this.Close();
            }
            else this.InitializeComponent();
        }

        private void opendir_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", this.bloxstrapDirectory);
        }

        private async void downgrade_Click(object sender, RoutedEventArgs e)
        {
            this.Lock(true);
            this.downgrade.Content = "Downgrading...";

            this.targetedVersion = (DeployLog)this.targetVersion.SelectedValue;

            string versionDir = $"{this.versionsDirectory}/{(await PlayerDeployLogs.GetVersionData(this.Channel)).VersionGuid}";

            if (Directory.Exists(versionDir))
            {
                Directory.Delete(versionDir, true);
            }

            await DeployDownloader.DownloadDeploy(this.Channel, this.targetedVersion.VersionGuid, versionDir.Split("/")[0], versionDir.Split("/")[1]);

            this.downgrade.Content = "Downgrade Roblox";
            this.Lock(false);
        }

        private async void targetVersion_Initialized(object sender, EventArgs e)
        {
            this.Initialize();
            PlayerDeployLogs deployLogs = await PlayerDeployLogs.Get(this.Channel).ConfigureAwait(true);

            this.targetVersion.Items.Clear();

            if (deployLogs.HasHistory)
            {
                deployLogs.CurrentLogs_x64
                .OrderByDescending(log => log.TimeStamp)
                .Cast<DeployLog>()
                .ToList()
                .ForEach(
                    item => this.targetVersion.Items.Add(item)
                );
            }
            else
            {
                MessageBox.Show("Failed to initialize Roblox Downgrader, please restart the Application.", "Initializaton Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }

            this.targetVersion.SelectedIndex = 0;

            this.Lock(false);
        }
    }
}