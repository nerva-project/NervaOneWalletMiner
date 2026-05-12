using System.Collections.Generic;

namespace NervaOneWalletMiner.Rpc.Common
{
    public static class SendPriority
    {
        // All coins
        public const string Low = "Low";

        // XMR-based priorities (maps to monero priority 0-3)
        public const string Default = "Default";        
        public const string Medium = "Medium";
        public const string High = "High";

        // BTC-based priorities (maps to conf_target blocks)
        public const string Economy = "Economy";
        public const string Normal = "Normal";
        public const string Fast = "Fast";
        public const string Urgent = "Urgent";

        public static List<string> GetXmrPriorityList() => [Default, Low, Medium, High];
        public static List<string> GetBtcPriorityList() => [Economy, Low, Normal, Fast, Urgent];

        public static string GetBtcConfirmationTarget(string priority)
        {
            int confTarget = priority switch
            {
                Low => 36,
                Normal => 6,
                Fast => 3,
                Urgent => 1,
                _ => 144 // Economy
            };

            return "~" + confTarget + (confTarget == 1 ? " block" : " blocks");
        }
    }
}
