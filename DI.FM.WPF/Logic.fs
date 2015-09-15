module DI.FM.WPF.Logic

open DI.FM.Client

let mutable config : Config option = None

type TrackData = string

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
         | DiFmUrl (channelName, listeningKey) -> 
            let trackData = rawTrackData.artist 
                            + " - "
                            + rawTrackData.title
                            + ". Channel "
                            + channelName
            TrackStarted (DiFm trackData)
         | _ -> TrackStarted Other
    raiseEvent event