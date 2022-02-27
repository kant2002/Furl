#r "System.Net.Http"

open System
open System.Net.Http

let private добавитьЗаголовок (заголовки : Headers.HttpHeaders) (название, значение : string) =
    заголовки.Add (название, значение)

let private добавитьТело (запр : HttpRequestMessage) заголовки тело =
    запр.Content <- new StringContent (тело)
    let заголовокТипКонтента =
        заголовки |> List.tryFind (fun (n, _) -> n = "Content-Type")
    заголовокТипКонтента
    |> Option.iter (fun (_, v) -> запр.Content.Headers.ContentType.MediaType <- v)

let результат (з : System.Threading.Tasks.Task<_>) = з.Result

let скомпоноватьСообщение мет (урл : Uri) заголовки тело =
    let запр = new HttpRequestMessage (мет, урл)
    Option.iter (добавитьТело запр заголовки) тело

    заголовки
    |> List.partition (fun (n, _) -> n = "Content-Type")
    |> snd
    |> List.iter (добавитьЗаголовок запр.Headers)
    запр

let получить урл заголовки =
    use клиент = new HttpClient ()
    // HttpMethod is qualified to avoid collision with FSharp.Data.HttpMethod,
    // if FSharp.Data is imported in a script as well as Furl.
    скомпоноватьСообщение Net.Http.HttpMethod.Get (Uri урл) заголовки None
    |> клиент.SendAsync
    |> результат

let отправить урл заголовки тело =
    use клиент = new HttpClient ()
    // HttpMethod is qualified to avoid collision with FSharp.Data.HttpMethod,
    // if FSharp.Data is imported in a script as well as Furl.
    скомпоноватьСообщение Net.Http.HttpMethod.Post (Uri урл) заголовки (Some тело)
    |> клиент.SendAsync
    |> результат

let текстТела (отв : HttpResponseMessage) =
    отв.Content.ReadAsStringAsync().Result
