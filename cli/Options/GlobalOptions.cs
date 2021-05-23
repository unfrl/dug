using CommandLine;

namespace dug.Options
{
    public class GlobalOptions
    {
        [Option('v', "verbose", Required = false, HelpText = "HT_Enable_Verbose_Output", ResourceType = typeof(i18n.dug))]
        public bool Verbose { get; set; }

        [Option('p', "parallel", Required = false, HelpText = "HT_Parallel", ResourceType = typeof(i18n.dug), Default = 200)]
        public int QueryParallelism { get; set; }

        [Option("retries", Required = false, HelpText = "HT_Retries", ResourceType = typeof(i18n.dug), Default = 0)]
        public int QueryRetries { get; set; }

        [Option('t', "timeout", Required = false, HelpText = "HT_Timeout", ResourceType = typeof(i18n.dug), Default = 3000)]
        public int Timeout { get; set; }
    }
}