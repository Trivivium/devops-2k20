using System;
using System.Net.Http;
using System.Threading.Tasks;
using WebApplication;
using Xunit;

namespace MiniTwit.Tests
{
    public class ApiControllerTests : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly HttpClient _client;


        public ApiControllerTests(CustomWebApplicationFactory<Startup> factory)
        {
            _client = factory.CreateClient();
        }


        [Fact]
        public async Task TestLatest()
        {


        }
    }
}
