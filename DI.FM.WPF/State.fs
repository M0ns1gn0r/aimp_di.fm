module DI.FM.State

open DI.FM.Client

let mutable config : Config option = None

type TrackType =
| Other
| DiFm

type Events =
| LoggedIn
| TrackStarted of TrackType
| TrackStopped

let raiseEvent, eventsStream = 
    let event = Event<Events>()
    event.Trigger, event.Publish