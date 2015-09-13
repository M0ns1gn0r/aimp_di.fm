module DI.FM.State

open DI.FM.Client

let mutable config : Config option = None

type TrackData = string

type TrackType =
| Other
| DiFm of TrackData

type State =
| NotLoggedIn
| NoRadioIsPlaying of Config
| RadioIsPlaying of Config * TrackData

type Events =
| LoggedIn of Config
| TrackStarted of TrackType
| TrackStopped

let setupStateMachine loggedIn trackStarted trackStopped = 

    let stateTransitions currentState event =
        match currentState, event with
        | NotLoggedIn, LoggedIn config -> 
            // TODO: check if a track is already playing and decide which state to go.
            loggedIn config
        | NotLoggedIn, TrackStarted _
        | NotLoggedIn, TrackStopped ->
            NotLoggedIn // Ignoring.

        | NoRadioIsPlaying config, TrackStarted (DiFm trackData) ->
            // First DI.FM track is started.
            trackStarted config trackData
        | NoRadioIsPlaying config, TrackStarted _
        | NoRadioIsPlaying config, TrackStopped _ ->
            NoRadioIsPlaying config  // Ignoring.
        | NoRadioIsPlaying _, LoggedIn _ ->
            failwith "Unexpected LoggedIn event on NoRadioIsPlaying state."

        | RadioIsPlaying (config, _), TrackStarted (DiFm newTrackData) ->
            // Another DI.FM track is started.
            trackStarted config newTrackData
        | RadioIsPlaying (config, _), TrackStarted Other
        | RadioIsPlaying (config, _), TrackStopped ->
            trackStopped config
        | RadioIsPlaying _, LoggedIn _ ->
            failwith "Unexpected LoggedIn event on RadioIsPlaying state."
    stateTransitions


let raiseEvent, eventsStream = 
    let event = Event<Events>()
    event.Trigger, event.Publish