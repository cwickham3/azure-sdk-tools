namespace Microsoft.Azure.Commands.BatchManager.Test
{
    using System;
    using Xunit;
    using Microsoft.Azure.Commands.Batch;

    public class BatchAccountContextTest
    {
        [Fact]
        public void TestBatchAccountContext()
        {
            var accountName = "charlwi";
            var goodSuffix = "batch.windows.core.net";
            //var fakeKey = "fakeAccountKey";

            var goodEndpoint = new UriBuilder(Uri.UriSchemeHttps, accountName + "." + goodSuffix).Uri;

            var acctContext = new BatchAccountContext(goodEndpoint.AbsoluteUri.ToString());
            //Assert.Equal<string>(goodEndpoint.DnsSafeHost, acctContext.AccountEndpoint.DnsSafeHost);
            Assert.Equal<string>(accountName, acctContext.AccountName);
            //Assert.Equal<string>(fakeKey, acctContext.AccountKey);
            Assert.Equal<string>(Uri.UriSchemeHttps + Uri.SchemeDelimiter + goodSuffix, acctContext.TaskTenantUrl);
        }
    }
}
