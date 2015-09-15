module DI.FM.State

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