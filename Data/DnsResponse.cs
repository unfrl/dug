using DnsClient;

namespace dug.Data
{
    public class DnsResponse
    {
        public DnsResponse(IDnsQueryResponse queryResponse, long responseTime){
            QueryResponse = queryResponse;
            ResponseTime = responseTime;
        }

        public DnsResponse(DnsResponseException error, long responseTime){
            Error = error;
            ResponseTime = responseTime;
        }

        public IDnsQueryResponse QueryResponse { get; set; }

        public long ResponseTime { get; set; }

        public bool HasError { 
            get
            {
                return Error != null;
            }
        }

        public DnsResponseException Error { get; set; }
    }
}