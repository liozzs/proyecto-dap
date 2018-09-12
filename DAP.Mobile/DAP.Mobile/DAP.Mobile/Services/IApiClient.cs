using System.Threading.Tasks;

namespace DAP.Mobile.Services
{
    public interface IApiClient
    {
        Task<TResponse> InvokeDataServiceAsync<TResponse>(ApiClientOption option) where TResponse : class;
        Task<string> InvokeDataServiceStringAsync(ApiClientOption option);
        Task InvokeDataServiceAsync(ApiClientOption option);
    }
}