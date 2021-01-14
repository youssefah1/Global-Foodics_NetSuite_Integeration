using NetSuiteIntegeration.com.netsuite.webservices;
using System;
using System.Net;

namespace NetSuiteIntegration
{
    public class DataCenterAwareNetSuiteService : NetSuiteService
    {

        private System.Uri OriginalUri;

        public DataCenterAwareNetSuiteService(string account, bool doNotSetUrl)
            : base()
        {
            OriginalUri = new System.Uri(this.Url);
            if (account == null || account.Length == 0)
                account = "empty";
            if (!doNotSetUrl)
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                DataCenterUrls urls = getDataCenterUrls(account).dataCenterUrls;
                Uri dataCenterUri = new Uri(urls.webservicesDomain + OriginalUri.PathAndQuery);
                this.Url = dataCenterUri.ToString();
            }
        }

        public void SetAccount(string account)
        {
            if (account == null || account.Length == 0)
                account = "empty";

            this.Url = OriginalUri.AbsoluteUri;
            DataCenterUrls urls = getDataCenterUrls(account).dataCenterUrls;
            Uri dataCenterUri = new Uri(urls.webservicesDomain + OriginalUri.PathAndQuery);
            this.Url = dataCenterUri.ToString();
        }
    }
}