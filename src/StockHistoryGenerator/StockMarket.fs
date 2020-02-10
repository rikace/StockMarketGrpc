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
    
    let tickersDirectory = DirectoryInfo("../../../Tickers")
    
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
              Date     = DateTime.Now // TODO Fix me
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
                    match Double.TryParse(s) with
                    | true, x -> decimal x
                    | _, _ -> -1M
                    
                match DateTime.TryParse(cells.[0]) with
                | true, dt ->
                    
                    let open' = parseDouble cells.[1]
                    let high = parseDouble cells.[2]
                    let low = parseDouble cells.[3]
                    let close = parseDouble cells.[4]
                    {
                        Symbol = symbol
                        Date = dt
                        LastChange = close
                        Price = open'
                        DayOpen = open'
                        DayLow = low
                        DayHigh = high
                    } |> Some
                | _, _ -> None

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
         let tickerHistory =
             seq {
                 use stream = tickerFile.OpenRead()
                 use reader = new StreamReader(stream)
                 while reader.EndOfStream |> not do
                     let line = reader.ReadLine()
                     let stock = f line
                     yield stock
             } |> Seq.choose id
            
         seq {
             while true do
                 for tickerRecord in tickerHistory do
                     yield tickerRecord
         } |> Observable.toObservable TaskPoolScheduler.Default

    let observableStream (delay : float) (f: string -> string -> Stock option) =
        let filePaths = tickersDirectory.GetFiles("*.csv")
        
        let startDate = DateTime(2001,1,1)

        let streams =
            filePaths
            |> Seq.map(fun ticker -> ticker, (Path.GetFileNameWithoutExtension(ticker.Name).ToUpper()))
            |> Seq.map(fun (tickerFile, ticker) -> getLinesObservable tickerFile (fun row -> f ticker row))
            |> Seq.map(fun tickerObs ->
                Observable.interval (TimeSpan.FromMilliseconds(delay))
                |> Observable.zip (fun stock tick ->
                    { stock with Date = startDate + TimeSpan.FromDays(float tick)} ) tickerObs)
            |> Seq.reduce(fun a b -> Observable.merge a b)
        streams
                
                
    [<CompiledName("RetrieveStockHistory")>]
    let retrieveStockHistory (ticker: string) = Async.StartAsTask <| async {
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

    [<CompiledName("SearchStock")>]
    let searchStock (ticker: string) (date: DateTime) = Async.StartAsTask <| async {
         let! stockHistory = 
            retrieveStockHistory ticker |> Async.AwaitTask
     
         match stockHistory with
         | [||] -> return None
         | arr ->
            return arr |> Array.tryFind (fun t -> t.Date.Date = date.Date) 
    }
 
    let stockTickers = lazy (
        tickersDirectory.GetFiles("*.csv")
        |> Array.map(fun ticker -> Path.GetFileNameWithoutExtension(ticker.Name).ToUpper(),
                                   File.ReadLines(ticker.FullName) |> Seq.skip 1 |> Seq.head)
        |> Array.map(fun (ticker, row) ->  Stock.parse ticker row)
        |> Array.choose id
    )
                        