namespace NervaOneWalletMiner.Rpc.Daemon.Downloads
{
    public interface IDownload
    {
        string CliUrlWindows64 { get; }
        string CliUrlWindows32 { get; }
        string CliUrlLinux64 { get; }
        string CliUrlLinux32 { get; }
        string CliUrlLinuxArm { get; }
        string CliUrlMacIntel { get; }
        string CliUrlMacArm { get; }

        string QuickSyncUrl { get; }

        // TODO: Add ability to update to latest version from GitHub
        /*
            //https://github.com/octokit/octokit.net
            // dotnet add package Octokit

            GitHubClient client = new GitHubClient(new ProductHeaderValue("NervaOne"));
            IReadOnlyList<Release> releases = await client.Repository.Release.GetAll("nerva-project", "nerva");
            //IReadOnlyList<Release> releases = await client.Repository.Release.GetAll("monero-project", "monero");
            //IReadOnlyList<Release> releases = await client.Repository.Release.GetAll("wownero-project", "wownero");

            releases[0].TagName
            releases[0].Assets[0].Name
            releases[0].Assets[0].BrowserDownloadUrl

            // If Assets are not there (like Monero), use body and extract download links:
            releases[0].Body
         */
    }
}