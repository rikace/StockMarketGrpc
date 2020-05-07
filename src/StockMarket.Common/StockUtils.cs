using System;
using StockMarket.Grpc.Proto;

namespace StockMarket.Common
{
    public static class StockUtils
    {
        public static void PrintStockInfo(this Stock stock)
        {
            bool compare(string item1, string item2)
                =>
                    String.Compare(item1, item2, StringComparison.OrdinalIgnoreCase) == 0;

            var symbol = stock.Symbol;

            var color = Console.ForegroundColor;
            if (compare(symbol, "MSFT"))
                Console.ForegroundColor = ConsoleColor.Green;
            else if (compare(symbol, "FB"))
                Console.ForegroundColor = ConsoleColor.Blue;
            else if (compare(symbol, "AAPL"))
                Console.ForegroundColor = ConsoleColor.Red;
            else if (compare(symbol, "GOOG"))
                Console.ForegroundColor = ConsoleColor.Magenta;
            else if (compare(symbol, "AMZN"))
                Console.ForegroundColor = ConsoleColor.Yellow;

            Console.WriteLine(
                $"{stock.Symbol} |\t\t Date {stock.Date.ToString("MM/dd/yyyy")} | Higher price {stock.High} | Lower price {stock.Low}");
            Console.ForegroundColor = color;
        }
    }
}