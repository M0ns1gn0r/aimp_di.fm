module DI.FM.Client

open System.Text.RegularExpressions
open Chessie.ErrorHandling.Trial
open FSharp.Data

type Vote = Up | Down
type Channel = { Id: int; Name: string }
type Config = {
    LoggedInAs: string
    ApiKey: string
    /// A mapping from channelKey to channel data.
    Channels: Map<string, Channel>
}
type Error = 
| InvalidCredentials
| NoCookiesFound
| SessionCookieNotFound
| LoginFailed of System.Exception
| LoadConfigFailed of System.Exception
| TrackHistoryLookupFailed of System.Exception
| TrackNotFoundInHistory
| VoteFailed of System.Exception


let userAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/44.0.2403.125 Safari/537.36"
let diFmCookiesDomain = new System.Uri "http://www.di.fm/"
let diFmSessionCookieName = "audio_addict_session"
let diFmLoginUrl = "http://www.di.fm/login"
let diFmConfigUrl = "http://www.di.fm/webplayer3/config"
let diFmTrackHistoryUrl channelId = 
    sprintf "http://www.di.fm/_papi/v1/di/track_history/channel/%d?per_page=10" channelId
let diFmVoteUrl channelId trackId direction = 
    let direction = match direction with | Up -> "up" | Down -> "down"
    sprintf "http://www.di.fm/_papi/v1/di/tracks/%d/vote/%d/%s" trackId channelId direction

type DiFmLoginResponse = JsonProvider<""" { "auth": true } """>
type DiFmConfig = JsonProvider<"""
{
    "API":{
        "Config":{
            "channels":[
                {
                    "id": 400,
                    "key": "chillntropicalhouse",
                    "name": "Chill & Tropical House"
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
        "artist": "Tears Of Technology",
        "title": "My Curse (Original Mix)",
        "track": "Tears of Technology - My Curse (Original Mix)",
        "track_id":913147,
        "type":"track",
        "votes":{
            "up":2,
            "down":1
        }
    }
]
""">
// This part was removed from the request above to reduce the number of "option" types.
//,
//    {
//        "artist":null,
//        "title":null,
//        "track_id":null,
//        "type":"advertisement"
//    }

type DiFmVoteResponse = JsonProvider<""" {"up": 1, "down": 0} """, RootName="VoteResults">

let (|DiFmUrl|_|) (url: string) =
    let pattern = "^https?:\/\/[a-z\d\.]*di.fm.*\/di_([a-z]+)_.*\?([a-z\d]+)$"
    let m = Regex.Match(url, pattern)
    if m.Success
    then
        let channelKey = m.Groups.[1].Value
        let listeningKey = m.Groups.[2].Value
        Some (channelKey, listeningKey)
    else None

/// Attempts to login into DI.FM and retrieve the session cookie.
let getSessionId login password =
    let cc = System.Net.CookieContainer()

    let validate (response: DiFmLoginResponse.Root) =
        if (not response.Auth) then fail InvalidCredentials
        else
            let cookies = cc.GetCookies diFmCookiesDomain 
            if cookies = null then fail NoCookiesFound
            else
                let cookie = cookies.[diFmSessionCookieName]
                if cookie = null then fail SessionCookieNotFound
                else ok cookie.Value
    try
        Http.RequestString(
            diFmLoginUrl,
            httpMethod = "POST",
            headers = [ 
                HttpRequestHeaders.UserAgent userAgent
                "X-Requested-With", "XMLHttpRequest"
            ],
            cookieContainer = cc,
            body = FormValues [
                "member_session[username]", login
                "member_session[password]", password
                "member_session[remember_me]", "1"
            ])
        |> DiFmLoginResponse.Parse
        |> validate
    with ex -> LoginFailed ex |> fail

/// Retrieves the apiKey and builds "channel name" to "channel id" map.
let getDiFmConfig login sessionId =
    try
        let response = 
            Http.RequestString(
                diFmConfigUrl,
                headers = [ HttpRequestHeaders.UserAgent userAgent ],
                cookies = [ diFmSessionCookieName, sessionId ])
            |> DiFmConfig.Parse
        let config = response.Api.Config
        ok
            {
                LoggedInAs = login
                ApiKey = config.Member.ApiKey;
                Channels = config.Channels
                            |> Seq.map (fun c -> (c.Key, { Id = c.Id; Name = c.Name }))
                            |> Map.ofSeq
            }
    with ex -> LoadConfigFailed ex |> fail

/// Attempts to login into DI.FM and load the stations configuration.
let loginToDiFm login password = getSessionId login password >>= (getDiFmConfig login)

/// Retrieves track history of the specified channel and tries to find the requested track there.
let findTrackInHistory channelId track =
    try
        let history = 
            diFmTrackHistoryUrl channelId 
            |> DiFmTrackHistory.Load
        let result = 
            history
            |> Seq.tryFind (fun t -> t.Type = "track" && t.Track = track)
        match result with 
        | Some data -> ok data 
        | None -> fail TrackNotFoundInHistory
    with ex -> TrackHistoryLookupFailed ex |> fail

/// Votes for or against the specified track.
let vote apiKey channelId trackId direction =
    let url = diFmVoteUrl channelId trackId direction
    try
        Http.RequestString(
            url,
            httpMethod = "POST",
            headers = [ 
                HttpRequestHeaders.Accept HttpContentTypes.Json
                HttpRequestHeaders.UserAgent userAgent
                "X-Api-Key", apiKey
            ])
        |> DiFmVoteResponse.Parse
        |> ok
    with ex -> VoteFailed ex |> fail