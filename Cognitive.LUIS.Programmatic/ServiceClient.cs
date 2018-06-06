﻿using Cognitive.LUIS.Programmatic.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Cognitive.LUIS.Programmatic
{
    public class ServiceClient
    {
        private readonly HttpClient _client;

        public ServiceClient(string subscriptionKey, Location location)
        {
            var baseUrl = $"https://{location.ToString().ToLower()}.api.cognitive.microsoft.com/luis/api/v2.0";
            _client = HttpClientFactory.Create(baseUrl, subscriptionKey);
        }

        protected async Task<HttpResponseMessage> Get(string path) =>
            await _client.GetAsync(path);

        protected async Task<string> Post(string path)
        {
            var response = await _client.PostAsync(path, null);
            var responseContent = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
                return responseContent;
            else
            {
                var exception = JsonConvert.DeserializeObject<ServiceException>(responseContent);
                throw new Exception($"{exception.Error?.Message ?? exception.Message}");
            }
        }

        protected async Task<string> Post<TRequest>(string path, TRequest requestBody)
        {
            using (var content = new ByteArrayContent(GetByteData(requestBody)))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var response = await _client.PostAsync(path, content);
                var responseContent = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                    return responseContent;
                else
                {
                    var exception = JsonConvert.DeserializeObject<ServiceException>(responseContent);
                    throw new Exception($"{exception.Error?.Message ?? exception.Message}");
                }
            }
        }

        protected async Task Put<TRequest>(string path, TRequest requestBody)
        {
            using (var content = new ByteArrayContent(GetByteData(requestBody)))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var response = await _client.PutAsync(path, content);
                if (!response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var exception = JsonConvert.DeserializeObject<ServiceException>(responseContent);
                    throw new Exception($"{exception.Error?.Message ?? exception.Message}");
                }
            }
        }

        protected async Task Delete(string path)
        {
            var response = await _client.DeleteAsync(path);
            if (!response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var exception = JsonConvert.DeserializeObject<ServiceException>(responseContent);
                throw new Exception($"{exception.Error?.Message ?? exception.Message}");
            }
        }

        private byte[] GetByteData<TRequest>(TRequest requestBody)
        {
            var settings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
            var body = JsonConvert.SerializeObject(requestBody, settings);
            return Encoding.UTF8.GetBytes(body);
        }
    }
}
