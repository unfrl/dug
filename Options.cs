using System;
using CommandLine;

namespace dug.Options
{
    [Verb("run", isDefault: true, HelpText = "Get DNS propogation info for a URL")]
    public class RunOptions : GlobalOptions
    {
        [Value(0, Required = true, HelpText = "The URL you would like to see propogation for", MetaName = "URL")]
        public string Url {get; set;}
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