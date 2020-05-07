using System;

namespace StockMarket.Common
{
    public class Stock : IEquatable<Stock>
    {
        public Stock(string symbol, DateTime date, decimal open, decimal high, decimal low, decimal close)
        {
            Symbol = symbol;
            Date = date;
            Open = open;
            High = high;
            Low = low;
            Close = close;
        }

        public string Symbol { get; }
        public DateTime Date { get; set; }
        public Decimal Open { get; }
        public Decimal High { get; }
        public Decimal Low { get; }
        public Decimal Close { get; }

        public static Stock Parse(string symbol, string row)
        {
            if (string.IsNullOrWhiteSpace(row))
                return null;

            var cells = row.Split(',');
            if (DateTime.TryParse(cells[0], out DateTime date))
            {
                //var date = DateTime.UtcNow;
                var open = ParseDecimal(cells[1]);
                var high = ParseDecimal(cells[2]);
                var low = ParseDecimal(cells[3]);
                var close = ParseDecimal(cells[4]);
                return new Stock(symbol, date.ToUniversalTime(), open, high, low, close);
                
            }
            return null;
        }

        private static decimal ParseDecimal(string s)
        {
            if (decimal.TryParse(s, out var x))
                return x;
            return -1;
        }

        public bool Equals(Stock other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Symbol, other.Symbol, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Stock) obj);
        }

        public override int GetHashCode()
        {
            return (Symbol != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(Symbol) : 0);
        }

        public static bool operator == (Stock left, Stock right)
        {
            return Equals(left, right);
        }

        public static bool operator != (Stock left, Stock right)
        {
            return !Equals(left, right);
        }
    }
}