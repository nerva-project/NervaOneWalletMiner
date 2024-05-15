namespace NervaWalletMiner.Rpc.Daemon.Downloads
{
    public class DownloadXNV : IDownload
    {
        public string CliUrlWindows64 => "https://github.com/nerva-project/nerva/releases/download/v0.1.8.0/nerva-v0.1.8.0_windows_minimal.zip";

        public string CliUrlWindows32 => "https://github.com/nerva-project/nerva/releases/download/v0.1.8.0/nerva-v0.1.8.0_windows_minimal.zip";

        public string CliUrlLinux64 => "https://github.com/nerva-project/nerva/releases/download/v0.1.8.0/nerva-v0.1.8.0_linux_minimal.zip";

        public string CliUrlLinux32 => "https://github.com/nerva-project/nerva/releases/download/v0.1.8.0/nerva-v0.1.8.0_linux_minimal.zip";
       
        public string CliUrlLinuxArm => "https://github.com/nerva-project/nerva/releases/download/v0.1.8.0/nerva-v0.1.8.0_linux_minimal.zip";       

        public string CliUrlMacIntel => "https://github.com/nerva-project/nerva/releases/download/v0.1.8.0/nerva-v0.1.8.0_osx_minimal.zip";

        public string CliUrlMacArm => "https://github.com/nerva-project/nerva/releases/download/v0.1.8.0/nerva-v0.1.8.0_osx_minimal.zip";
       

        public string QuickSyncUrl => "https://nerva.one/quicksync/quicksync.raw";
    }
}
