using System.Collections.Generic;
using System.Linq;
using DnsClient;
using DnsClient.Protocol;

namespace dug.Data
{
    public class DnsResponse
    {
        public DnsResponse(IDnsQueryResponse queryResponse, long responseTime, IEnumerable<QueryType> desiredRecordTypes){
            QueryResponse = queryResponse;
            ResponseTime = responseTime;
            DesiredRecordTypes = desiredRecordTypes;
        }

        public DnsResponse(DnsResponseException error, long responseTime){
            Error = error;
            ResponseTime = responseTime;
        }

        public IDnsQueryResponse QueryResponse { get; private set; }

        public long ResponseTime { get; private set; }

        public IEnumerable<QueryType> DesiredRecordTypes { get; private set; }

        public IEnumerable<DnsResourceRecord> FilteredAnswers { 
            get {
                if(DesiredRecordTypes.Contains(QueryType.ANY)){
                    return QueryResponse.Answers;
                }
                return QueryResponse.Answers.Where(record => DesiredRecordTypes.Contains((QueryType)record.RecordType));
            }
        }

        public bool HasError { 
            get
            {
                return Error != null;
            }
        }

        public DnsResponseException Error { get; set; }
    }
}