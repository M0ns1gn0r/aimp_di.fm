module DI.FM.WPF.Logic

open DI.FM.Client

let mutable config : Config option = None

type TrackData = {
    channelKey: string
    artist: string
    title: string
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

type RawTrackData = { artist: string; title: string; streamUrl: string }
let raiseTrackChanged rawTrackData =
    let event = 
        match rawTrackData.streamUrl with
         | DiFmUrl (channelKey, listeningKey) ->             
            let trackData = { 
                channelKey = channelKey
                artist = rawTrackData.artist
                title = rawTrackData.title
            }
            TrackStarted (DiFm trackData)
         | _ -> TrackStarted Other
    raiseEvent event