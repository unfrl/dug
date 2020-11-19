using CommandLine;

namespace dug.Options
{
    [Verb("run", isDefault: true)]
    public class RunOptions
    {
        [Value(0)]
        public string Url {get; set;}
    }

    [Verb("update", HelpText = "Update DNS server list. Uses remote server list by default.")]
    public class UpdateOptions
    {
        [Option('f', "file", Required = false, HelpText = "Update DNS server list using the specified file")] //TODO: At some point we need a link here to a readme showing the format the file must be in.
        public string CustomServerFile { get; set; }
    }
}