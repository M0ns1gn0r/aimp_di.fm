module DI.FM.Client

open System.Text.RegularExpressions
open FSharp.Data

type Vote = Up | Down
type Config = {
    apiKey: string;
    stations: Map<string, int>;
}

let diFmConfigUrl = "http://www.di.fm/webplayer3/config"
let diFmTrackHistoryUrl channelId = 
    sprintf "http://www.di.fm/_papi/v1/di/track_history/channel/%d?per_page=10" channelId
let diFmVoteUrl channelId trackId direction = 
    let direction = match direction with | Up -> "up" | Down -> "down"
    sprintf "http://www.di.fm/_papi/v1/di/tracks/%d/vote/%d/%s" trackId channelId direction

type DiFmConfig = JsonProvider<"""
{
    "API":{
        "Config":{
            "channels":[
                {
                    "id": 42,
                    "key": "channelname"
                }
            ],
            "member":{
                "api_key": "some_key"
            }
        }
    }
}""">
type DiFmTrackHistory = JsonProvider<"""
[
    {
        "art_url":"\/\/static.audioaddict.com\/4\/a\/e\/e\/3\/8\/4aee38295c4cc174b016c4982a226298.jpg",
        "artist":"Digital Department",
        "title":"My Favourite Time (Platunoff Breaks Remix)",
        "track_id":913147,
        "type":"track",
        "votes":{
            "up":2,
            "down":1
        }
    },
    {
        "artist":null,
        "title":null,
        "track_id":null,
        "type":"advertisement"
    }
]
""">
type DiFmVoteResponse = FSharp.Data.JsonProvider<""" {"up": 1, "down": 0} """, RootName="VoteResults">

let (|DiFmUrl|_|) (url: string) =
    let pattern = "^https?:\/\/[a-z\d\.]*di.fm.*\/di_([a-z]+)_.*\?([a-z\d]+)$"
    let m = Regex.Match(url, pattern)
    if m.Success
    then
        let channelName = m.Groups.[1].Value
        let listeningKey = m.Groups.[2].Value
        Some (channelName, listeningKey)
    else None

let processDiFmUrl = function
    | DiFmUrl (channelName, listeningKey) -> 
        sprintf "DI.FM url detected: %A" (channelName, listeningKey)
        |>  System.Console.WriteLine
    | _ -> System.Console.WriteLine "Not a DI.FM url."

/// Retrieves the apiKey and builds "channel name" to "channel id" map.
let getDiFmConfig cookie =
    let response = 
        Http.RequestString(
            diFmConfigUrl,
            headers = [ 
                HttpRequestHeaders.Accept HttpContentTypes.Json;
                "Cookie", cookie
            ])
        |> DiFmConfig.Parse
    let config = response.Api.Config
    {
        apiKey = config.Member.ApiKey;
        stations = config.Channels
                    |> Seq.map (fun c -> (c.Key, c.Id))
                    |> Map.ofSeq
    }

/// Retrieves track history of the specified channel and tries to find the requested track there.
let findTrackInHistory channelId artist title =
    diFmTrackHistoryUrl channelId 
    |> DiFmTrackHistory.Load
    |> Seq.tryFind (fun t -> 
        t.Type = "track" 
        && t.Artist = Some(artist)
        && t.Title = Some(title))

/// Votes for or against the specified track.
let vote apiKey channelId trackId direction =
    let url = diFmVoteUrl channelId trackId direction
    Http.RequestString(
        url,
        httpMethod = "POST",
        headers = [ 
            HttpRequestHeaders.Accept HttpContentTypes.Json;
            "X-Api-Key", apiKey
        ])
    |> DiFmVoteResponse.Parse