using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebApplication;
using WebApplication.Entities;
using WebApplication.Models.Authentication;
using Xunit;
using Xunit.Abstractions;

namespace MiniTwit.Tests
{
    public class ApiControllerTests : IClassFixture<CustomWebApplicationFactory<Startup>>
    {

        //http://*:5000

        //private string _url = "http://localhost:11501";

        private readonly CustomWebApplicationFactory<Startup> _factory;

        private readonly ITestOutputHelper _output;

        private  HttpClient _client;

        public ApiControllerTests(CustomWebApplicationFactory<Startup> factory, ITestOutputHelper output)
        {
            _factory = factory;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
            _output = output;


        }


        [Theory]
        [InlineData("/")]
        [InlineData("/login")]
        public async Task Get_Endpoints(string url)
        {
            _client = _factory.CreateClient();

            var response = await _client.GetAsync(url);

            response.EnsureSuccessStatusCode();
            Assert.Equal("text/html; charset=utf-8",
            response.Content.Headers.ContentType.ToString());
        }

        
       // [Fact]
        public async Task TestLatest()
        {
            

            var data = new { Email = "test@test",Password = "foo", Username = "test"};

            var jsonContent = JsonConvert.SerializeObject(data);

            var httpResponse = await _client.PostAsync("/api/register", new StringContent(jsonContent, UnicodeEncoding.UTF8, "application/json"));


            Assert.True(httpResponse.StatusCode  == HttpStatusCode.NoContent);

            //get latest here


        }


        [Fact]
        public async Task Test_Register()
        {
            var data = new { Email = "a@a.a", Password = "a", Username = "a", Latest = 1};

            var jsonContent = JsonConvert.SerializeObject(data);

            var httpResponse = await _client.PostAsync("/api/register", new StringContent(jsonContent, UnicodeEncoding.UTF8, "application/json"));

            var httpResponseLatest = await _client.GetAsync("/api/latest");


            _output.WriteLine(httpResponseLatest.StatusCode.ToString());

            //get latest here


            Assert.True(httpResponse.StatusCode == HttpStatusCode.NoContent);

        }


    }
}
