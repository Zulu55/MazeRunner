using System.Text;
using System.Text.Json;
using MazeRunner.Models.Responses;
using Microsoft.Extensions.Configuration;

namespace MazeRunner.Services
{
    public class ApiService : IApiService
    {
        private readonly string _urlBase;
        private readonly string _tokenValue;

        public ApiService(IConfiguration configuration)
        {
            _urlBase = configuration["MazeAPI:urlBase"]!;
            _tokenValue = configuration["MazeAPI:tokenValue"]!;
        }

        private JsonSerializerOptions _jsonDefaultOptions => new()
        {
            PropertyNameCaseInsensitive = true,
        };

        public async Task<ActionResponse<T>> GetAsync<T>(string servicePrefix, string controller)
        {
            try
            {
                var client = new HttpClient()
                {
                    BaseAddress = new Uri(_urlBase),
                };

                var url = $"{servicePrefix}{controller}{_tokenValue}";
                var responseHttp = await client.GetAsync(url);
                var response = await responseHttp.Content.ReadAsStringAsync();
                if (!responseHttp.IsSuccessStatusCode)
                {
                    return new ActionResponse<T>
                    {
                        WasSuccess = false,
                        Message = response
                    };
                }

                return new ActionResponse<T>
                {
                    WasSuccess = true,
                    Result = JsonSerializer.Deserialize<T>(response, _jsonDefaultOptions)!
                };
            }
            catch (Exception ex)
            {
                return new ActionResponse<T>
                {
                    WasSuccess = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ActionResponse<TResponse>> PostAsync<T, TResponse>(string servicePrefix, string controller, T model)
        {
            try
            {
                var client = new HttpClient()
                {
                    BaseAddress = new Uri(_urlBase),
                };

                var url = $"{servicePrefix}{controller}{_tokenValue}";
                var messageJSON = JsonSerializer.Serialize(model);
                var messageContent = new StringContent(messageJSON, Encoding.UTF8, "application/json");
                var responseHttp = await client.PostAsync(url, messageContent);
                var response = await responseHttp.Content.ReadAsStringAsync();
                if (!responseHttp.IsSuccessStatusCode)
                {
                    return new ActionResponse<TResponse>
                    {
                        WasSuccess = false,
                        Message = response
                    };
                }

                return new ActionResponse<TResponse>
                {
                    WasSuccess = true,
                    Result = JsonSerializer.Deserialize<TResponse>(response, _jsonDefaultOptions)!
                };
            }
            catch (Exception ex)
            {
                return new ActionResponse<TResponse>
                {
                    WasSuccess = false,
                    Message = ex.Message
                };
            }
        }
    }
}