using MazeRunner.Models.Responses;

namespace MazeRunner.Services
{
    public interface IApiService
    {
        Task<ActionResponse<T>> GetAsync<T>(string servicePrefix, string controller);

        Task<ActionResponse<TResponse>> PostAsync<T, TResponse>(string servicePrefix, string controller, T model);
    }
}