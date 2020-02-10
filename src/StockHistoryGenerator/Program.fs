open System
open StockHistoryGenerator

[<EntryPoint>]
let main argv =
    

    let date = DateTime(2015, 3, 24)
    
    let res = StockMarket.searchStock "FB" date
    printf "Res %A" res
    
    Console.ReadLine() |> ignore
    
    let stockObservable = StockMarket.observableStream (500.) StockMarket.Stock.parse
    
    stockObservable.Subscribe(fun s -> printfn "%s - %A" s.Symbol s.LastChange) |> ignore
    
    Console.ReadLine() |> ignore
    0
