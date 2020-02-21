using System.ServiceModel;
using System.Threading.Tasks;

namespace StockMarket.Grpc.Shared
{
    [ServiceContract(Name = "StockMarketService.Grpc")]
    public interface IStockMarketService
    {
        ValueTask<StockResult> GetStockAsync(StockRequest request);
    }
}
