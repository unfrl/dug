using System.Collections.Generic;
using System.Linq;
using DnsClient;
using DnsClient.Protocol;

namespace dug.Data
{
    public class DnsResponse
    {
        public DnsResponse(IDnsQueryResponse queryResponse, long responseTime, QueryType desiredRecordTypes){
            QueryResponse = queryResponse;
            ResponseTime = responseTime;
            DesiredRecordTypes = desiredRecordTypes;
        }

        public DnsResponse(DnsResponseException error, long responseTime, QueryType desiredRecordTypes){
            Error = error;
            ResponseTime = responseTime;
            DesiredRecordTypes = desiredRecordTypes;
        }

        public IDnsQueryResponse QueryResponse { get; private set; }

        public long ResponseTime { get; private set; }

        public QueryType DesiredRecordTypes { get; private set; }

        public bool HasError { 
            get
            {
                return Error != null;
            }
        }

        public DnsResponseException Error { get; set; }
    }
}