module DI.FM.WPF.Logic

open DI.FM.Client

let mutable config : Config option = None

type TrackData = {
    ChannelKey: string
    Artist: string
    Title: string
}

type TrackType =
| Other
| DiFm of TrackData

type LoginState =
| LoggedIn of Config
| LoggedOut

type RadioState = 
| Playing of TrackData
| NotPlaying

type State = LoginState * RadioState

type Events =
| HasLoggedIn of Config
| TrackStarted of TrackType
| TrackStopped

let raiseEvent, eventsStream = 
    let event = Event<Events>()
    event.Trigger, event.Publish

type RawTrackData = { Artist: string; Title: string; StreamUrl: string }
let raiseTrackChanged rawTrackData =
    let event = 
        match rawTrackData.StreamUrl with
         | DiFmUrl (channelKey, listeningKey) ->
            let trackData = { 
                ChannelKey = channelKey
                Artist = rawTrackData.Artist
                Title = rawTrackData.Title
            }
            TrackStarted (DiFm trackData)
         | _ -> TrackStarted Other
    raiseEvent event