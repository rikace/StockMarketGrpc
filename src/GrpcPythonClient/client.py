import grpc
import google.protobuf

# import the generated classes
import stockmarket_pb2
import stockmarket_pb2_grpc

# open a gRPC channel
# channel = grpc.insecure_channel('localhost:5001')
channel = grpc.insecure_channel('localhost:5000')

# create a stub (client)
clientStub = stockmarket_pb2_grpc.StockMarketServiceStub(channel)

# create a valid request message
stockHistoryRequest = stockmarket_pb2.StockHistoryRequest(symbol='MSFT')

# make the call
response = clientStub.GetStockHistory(stockHistoryRequest)

#for stock in response.stockData:
#    price = stock.dayOpen.units + stock.dayOpen.nanos / 1000000000
#    print(stock.symbol + ' date:' + stock.date.ToJsonString() + ' Open Price:' + str(price))


for stock in clientStub.GetStockMarketStream(google.protobuf.empty_pb2.Empty()):
    price = stock.dayOpen.units + stock.dayOpen.nanos / 1000000000
    print(stock.symbol + ' date:' + stock.date.ToJsonString() + ' Open Price:' + str(price))