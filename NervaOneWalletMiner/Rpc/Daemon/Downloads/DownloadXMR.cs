using System;

namespace NervaOneWalletMiner.Rpc.Daemon.Downloads
{
    internal class DownloadXMR : IDownload
    {
        public string CliUrlWindows64 => "https://downloads.getmonero.org/cli/monero-win-x64-v0.18.3.3.zip";

        public string CliUrlWindows32 => "https://downloads.getmonero.org/cli/monero-win-x86-v0.18.3.3.zip";

        public string CliUrlLinux64 => "https://downloads.getmonero.org/cli/monero-linux-x64-v0.18.3.3.tar.bz2";

        public string CliUrlLinux32 => "https://downloads.getmonero.org/cli/monero-linux-x86-v0.18.3.3.tar.bz2";

        public string CliUrlLinuxArm => "https://downloads.getmonero.org/cli/monero-linux-armv7-v0.18.3.3.tar.bz2";

        public string CliUrlMacIntel => "https://downloads.getmonero.org/cli/monero-mac-x64-v0.18.3.3.tar.bz2";

        public string CliUrlMacArm => "https://downloads.getmonero.org/cli/monero-mac-armv8-v0.18.3.3.tar.bz2";

        public string QuickSyncUrl => throw new NotImplementedException();
    }
}