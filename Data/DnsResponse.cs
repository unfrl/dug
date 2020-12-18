using System.Collections.Generic;
using System.Linq;
using DnsClient;
using DnsClient.Protocol;

namespace dug.Data
{
    //TODO: I want to remove this class, but there is an issue where ThrowDnsErrors isnt respected, so i have to catch them and deal with it. I use this to wrap the relevant data so I can render it. https://github.com/MichaCo/DnsClient.NET/issues/99
    public class DnsResponse
    {
        public DnsResponse(IDnsQueryResponse queryResponse, long responseTime, QueryType recordType){
            QueryResponse = queryResponse;
            ResponseTime = responseTime;
            RecordType = recordType;
        }

        public DnsResponse(DnsResponseException error, long responseTime, QueryType recordType){
            Error = error;
            ResponseTime = responseTime;
            RecordType = recordType;
        }

        public IDnsQueryResponse QueryResponse { get; private set; }

        public long ResponseTime { get; private set; }

        public QueryType RecordType { get; private set; }

        public bool HasError { 
            get
            {
                return Error != null;
            }
        }

        public DnsResponseException Error { get; set; }
    }
}