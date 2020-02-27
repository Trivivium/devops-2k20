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
    {

        //http://*:5000

        private string _url = "http://localhost:11501";

        public ApiControllerTests()
        {
           
        }


        [Fact]
        public async Task TestLatest()
        {
            var client = new HttpClient();

            var data = new { Email = "test@test",Password = "foo", Username = "test"};

            var jsonContent = JsonConvert.SerializeObject(data);

            var httpResponse = await client.PostAsync($"{_url}/api/register", new StringContent(jsonContent, UnicodeEncoding.UTF8, "application/json"));

            Console.WriteLine(httpResponse.StatusCode);

            Assert.True(httpResponse.StatusCode  == HttpStatusCode.BadRequest);


        }
    }
}
