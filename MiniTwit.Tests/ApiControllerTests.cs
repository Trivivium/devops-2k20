using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using WebApplication;
using WebApplication.Entities;
using WebApplication.Models.Authentication;
using Xunit;

namespace MiniTwit.Tests
{
    public class ApiControllerTests 
        //: IClassFixture<CustomWebApplicationFactory<Startup>>
    {

       // private readonly CustomWebApplicationFactory<Startup> _factory;

        public ApiControllerTests(CustomWebApplicationFactory<Startup> factory)
        {
           
        }


        [Fact]
        public async Task TestLatest()
        {
            //var _client = _factory.CreateClient();

            var data = new { Email = "test@test", Password = "foo", Username = "test", Latest = 1337 };

            var jsonContent = JsonConvert.SerializeObject(data);


           // var httpResponse = await _client.PostAsync("/api/register", new StringContent(jsonContent, UnicodeEncoding.UTF8, "application/json"));

           // Assert.True(httpResponse.StatusCode  == HttpStatusCode.NoContent);


        }
    }
}
