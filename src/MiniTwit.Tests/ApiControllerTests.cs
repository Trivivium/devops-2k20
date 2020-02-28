using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebApplication;
using WebApplication.Entities;
using WebApplication.Models.Authentication;
using WebApplication.Models.Timeline;
using Xunit;
using Xunit.Abstractions;

namespace MiniTwit.Tests
{
    public class ApiControllerTests : IClassFixture<CustomWebApplicationFactory<Startup>>
    {


        private readonly CustomWebApplicationFactory<Startup> _factory;

        private  HttpClient _client;

        public ApiControllerTests(CustomWebApplicationFactory<Startup> factory, ITestOutputHelper output)
        {
            _factory = factory;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }


        //[Theory]
        //[InlineData("/")]
        //[InlineData("/login")]
        public async Task Get_Endpoints(string url)
        {
            _client = _factory.CreateClient();

            var response = await _client.GetAsync(url);

            response.EnsureSuccessStatusCode();
            Assert.Equal("text/html; charset=utf-8",
            response.Content.Headers.ContentType.ToString());
        }


        [Fact]
        public async Task Test_Register()
        {
            var data = new { Email = "a@a.a", Password = "a", Username = "a", Latest = 1};

            var jsonContent = JsonConvert.SerializeObject(data);

            var httpResponse = await _client.PostAsync("/api/register", new StringContent(jsonContent, Encoding.UTF8, "application/json"));

            //get latest here

            Assert.True(httpResponse.IsSuccessStatusCode);

            //TODO: verify that latest was updated


        }
        [Fact]
        public async Task TestLatest()
        {
            var httpResponse = await _client.GetAsync("/api/latest");
            
            Assert.True(httpResponse.IsSuccessStatusCode);

            //get latest here
            //TODO: verify that latest was updated

        }

        [Fact]
        public async Task Test_Create_Msg()
        {
            var username = "a";

            var myObject = (dynamic)new JObject();
            myObject.Content = "Blub!";
            myObject.Username = username;
            myObject.Latest = 2;


            var content = new StringContent(myObject.ToString(), Encoding.UTF8, "application/json");

         
        }


    }
}
