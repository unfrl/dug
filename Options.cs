using System;
using System.Collections.Generic;
using CommandLine;
using DnsClient;

namespace dug.Options
{
    [Verb("run", isDefault: true, HelpText = "Get DNS propogation info for a URL")]
    public class RunOptions : GlobalOptions
    {
        [Value(0, Required = true, HelpText = "The URL you would like to see propogation for", MetaName = "URL")]
        public string Url { get; set; }

        [Option('t', "timeout", Required = false, HelpText = "The timeout (in ms) to be used when querying the DNS Server(s). If there are multiple it will apply to each server", Default = 3000)]
        public int Timeout { get; set; }

        [Option('q', "query-type", Required = false, HelpText = "TODO: Put Help Here", Separator = ',', Default = new [] { QueryType.A })]
        public IEnumerable<QueryType> QueryTypes { get; set; }

        [Option('f', "file", Required = false, HelpText = "Use the specified DNS server list for this run.")] //TODO: At some point we need a link here to a readme showing the format the file must be in.
        public string CustomServerFile { get; set; }
    }

    [Verb("update", HelpText = "Update DNS server list with any new unique servers. Uses remote server to get list by default.")]
    public class UpdateOptions : GlobalOptions
    {
        [Option('f', "file", Required = false, HelpText = "Update DNS server list using the specified file.")] //TODO: At some point we need a link here to a readme showing the format the file must be in.
        public string CustomServerFile { get; set; }

        [Option('o', "overwrite", Required = false, HelpText = "Overwrite the current server list instead of updating it.")]
        public bool Overwite { get; set; }
    }

    public class GlobalOptions
    {
        [Option('v', "verbose", Required = false, HelpText = "Enable Verbose Output")]
        public bool Verbose { get; set; }
    }
}