using System;
using CommandLine;

namespace dug.Options
{
    public class GlobalOptions
    {
        [Option('v', "verbose", Required = false, HelpText = "HT_Enable_Verbose_Output", ResourceType = typeof(i18n.dug))]
        public bool Verbose { get; set; }

        private int _queryParallelism;
        [Option('p', "parallel", Required = false, HelpText = "HT_Parallel", ResourceType = typeof(i18n.dug), Default = 200)]
        public int QueryParallelism { get {return _queryParallelism;}
            set {
                if(value < 1){
                    throw new Exception(i18n.dug.ER_Parallel_Out_Of_Range);
                }
                _queryParallelism = value;
            }
        }

        private int _queryRetries;
        [Option("retries", Required = false, HelpText = "HT_Retries", ResourceType = typeof(i18n.dug), Default = 0)]
        public int QueryRetries { get {return _queryRetries;}
            set{
                if(value < 0){
                    throw new Exception(i18n.dug.ER_Retries_Out_Of_Range);
                }
                _queryRetries = value;
            }
        }

        private int _timeout;
        [Option('t', "timeout", Required = false, HelpText = "HT_Timeout", ResourceType = typeof(i18n.dug), Default = 3000)]
        public int Timeout { get {return _timeout;}
            set{
                if(value < 0){
                    throw new Exception(i18n.dug.ER_Timeout_Out_Of_Range);
                }
                _timeout = value;
            }
        }
    }
}