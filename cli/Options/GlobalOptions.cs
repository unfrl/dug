using CommandLine;

namespace dug.Options
{
    public class GlobalOptions
    {
        [Option('v', "verbose", Required = false, HelpText = "Enable Verbose Output")]
        public bool Verbose { get; set; }

        [Option('p', "parallel", Required = false, HelpText = "The number of servers to perform queries against in parralel", Default = 200)]
        public int QueryParallelism { get; set; }

        [Option("retries", Required = false, HelpText = "The number of times to retry queries one servers that error (or timeout)", Default = 0)]
        public int QueryRetries { get; set; }

        [Option('t', "timeout", Required = false, HelpText = "The timeout (in ms) to be used when querying the DNS Server(s). If there are multiple it will apply to each server", Default = 3000)]
        public int Timeout { get; set; }
    }
}