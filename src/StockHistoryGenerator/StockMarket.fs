namespace StockHistoryGenerator


[<AutoOpenAttribute>]
module internal ThreadSafeRandom =
    open System.Threading
    open System
    
    let getThreadSafeRandom = new ThreadLocal<Random>(fun () -> new Random(int DateTime.Now.Ticks))

module StockMarket =

    open System
    open System.IO
    open FSharp.Control
    open System.Reactive.Concurrency
    open System.Reactive.Linq
    
    let tickersDirectory = DirectoryInfo("../StockHistoryGenerator/Tickers")

    let observerOn obs = Observable.ObserveOn(obs, TaskPoolScheduler.Default)
    let subscribeOn obs = Observable.SubscribeOn(obs, TaskPoolScheduler.Default)
    
    [<CLIMutableAttribute>]
    type Stock =
        { Symbol: string
          DayOpen: decimal
          DayLow: decimal
          DayHigh: decimal
          LastChange: decimal
          Price: decimal
          Date : DateTime }
        
        static member Create (symbol: string) price =
            { Symbol   = symbol
              Date     = DateTime.UtcNow // TODO Fix me
              LastChange = 0M
              Price   = price
              DayOpen = 0M
              DayLow  = 0M
              DayHigh = 0M
            }
            
            
        static member parse (symbol : string) (row : string) =
            if row |> String.IsNullOrEmpty then None
            else
                let cells = row.Split(',', StringSplitOptions.RemoveEmptyEntries)
                let parseDouble (s : string) =
                    if String.IsNullOrWhiteSpace s then None 
                    else
                        match Double.TryParse(s) with
                        | true, x -> decimal x |> Some
                        | _, _ -> None
                    
                match DateTime.TryParse(cells.[0]) with
                | true, dt ->
                    
                    let open' = parseDouble cells.[1]                   
                    let high = parseDouble cells.[2]
                    let low = parseDouble cells.[3]
                    let close = parseDouble cells.[4]
                    let isValidStock = 
                        [open'; high; low; close] |> Seq.forall(fun p -> p.IsSome)
                    if isValidStock then
                        {
                            Symbol = symbol
                            Date = dt.ToUniversalTime()
                            LastChange = close.Value
                            Price = open'.Value
                            DayOpen = open'.Value
                            DayLow = low.Value
                            DayHigh = high.Value
                        } |> Some

                    else None
                | _, _ -> None

    [<CLIMutableAttribute>]
    type StockPriceRange =
        { Symbol     : string
          BestPrice  : decimal
          WorstPrice : decimal        
          BestDate   : DateTime 
          WorstDate  : DateTime  }


    let private changePrice (stock : Stock) (price : decimal) =
        if price = stock.Price then stock
        else
            let lastChange = price - stock.Price
            let dayOpen =
                if stock.DayOpen = 0M then price
                else stock.DayOpen
            let dayLow =
                if price < stock.DayLow || stock.DayLow = 0M then price
                else stock.DayLow
            let dayHigh =
                if price > stock.DayHigh then price
                else stock.DayHigh
            { stock with Price = price
                         LastChange = lastChange
                         DayOpen = dayOpen
                         DayLow = dayLow
                         DayHigh = dayHigh }


    module Observable =
        let interval (timeSpan : TimeSpan) =
            Observable.Interval(timeSpan)
        let zip (f : 'a -> 'b -> 'c) (sourceOne : IObservable<'a>) (sourceTwo : IObservable<'b>) =
            Observable.Zip(sourceOne, sourceTwo, (Func<'a,'b,'c>(f)))
            
        let merge (sourceOne : IObservable<'a>) (sourceTwo : IObservable<'a>) =
            sourceOne.Merge(sourceTwo)
            
        let toObservable scheduler (items : seq<'a>) =
            Observable.ToObservable(items, scheduler)
    
    let private getLinesObservable (tickerFile : FileInfo) (f: string -> 'a option) =
        asyncSeq {
            use stream = tickerFile.OpenRead()
            use reader = new StreamReader(stream)
            while reader.EndOfStream |> not do
                let! line = reader.ReadLineAsync() |> Async.AwaitTask
                let stock = f line
                yield stock
        } 
        |> AsyncSeq.choose id
        |> AsyncSeq.toObservable
        |> subscribeOn

    let observableStream(f: string -> string -> Stock option) =
        tickersDirectory.GetFiles("*.csv")        
        |> Seq.map(fun ticker -> ticker, (Path.GetFileNameWithoutExtension(ticker.Name).ToUpper()))
        |> Seq.map(fun (tickerFile, ticker) -> getLinesObservable tickerFile (fun row -> f ticker row))
        |> Seq.reduce(fun a b -> Observable.merge a b)
        |> observerOn 

    [<CompiledName("StockStream")>]
    let stockStream () =
        observableStream Stock.parse 


    [<CompiledName("RetrieveStockHistory")>]
    let retrieveStockHistory (ticker: string) = Async.StartAsTask <| async {
     
         let files = tickersDirectory.GetFiles("*.csv")

         let tickerHistory =
             tickersDirectory.GetFiles("*.csv")
             |> Seq.tryFind(fun f -> String.Compare(ticker, Path.GetFileNameWithoutExtension(f.Name), true) = 0)
         
         match tickerHistory with
         | None -> return Array.empty
         | Some tickerFile ->
             let symbol = Path.GetFileNameWithoutExtension(tickerFile.Name).ToUpper()
             let! stocks = 
                 asyncSeq {
                     use stream = tickerFile.OpenRead()
                     use reader = new StreamReader(stream)
                     while reader.EndOfStream |> not do
                         let! line = reader.ReadLineAsync() |> Async.AwaitTask
                         let stock = Stock.parse symbol line
                         // stock |> Option.iter(printfn "%A")
                         yield stock
                 }
                 |> AsyncSeq.choose id
                 |> AsyncSeq.toArrayAsync                 
             return stocks
    }

    [<CompiledName("StockPriceRangeHistory")>]
    let stockHistoryStream (ticker: string) = Async.StartAsTask <| async {
        let! data = retrieveStockHistory ticker |> Async.AwaitTask
        let bestStock = data |> Array.maxBy(fun s -> s.DayHigh)
        let worstStock = data |> Array.minBy(fun s -> s.DayLow)
        let stockPriceRangeHistory = {
            StockPriceRange.Symbol = ticker
            StockPriceRange.BestPrice = bestStock.DayHigh
            StockPriceRange.BestDate = bestStock.Date
            StockPriceRange.WorstPrice = worstStock.DayLow
            StockPriceRange.WorstDate = worstStock.Date
        }
        return stockPriceRangeHistory;
    }
                  

    [<CompiledName("SearchStock")>]
    let searchStock (ticker: string) (date: DateTime) = Async.StartAsTask <| async {
         let! stockHistory = 
            retrieveStockHistory ticker |> Async.AwaitTask
     
         return stockHistory |> Array.find (fun t -> t.Date.Date = date.Date) 
    }
 
    let stockTickers = lazy (
        tickersDirectory.GetFiles("*.csv")
        |> Array.map(fun ticker -> Path.GetFileNameWithoutExtension(ticker.Name).ToUpper(),
                                   File.ReadLines(ticker.FullName) |> Seq.skip 1 |> Seq.head)
        |> Array.map(fun (ticker, row) ->  Stock.parse ticker row)
        |> Array.choose id
    )
                        