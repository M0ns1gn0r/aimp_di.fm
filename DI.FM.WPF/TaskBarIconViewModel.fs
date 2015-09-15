namespace DI.FM.WPF.ViewModels

open FSharp.ViewModule
open FSharp.ViewModule.Validation
open DI.FM.State

type Icons = Enabled | Disabled | Error

type TaskBarIconViewModel() as x =
    inherit ViewModelBase()
    
    let resourcePrefix = "pack://application:,,,/DI.FM.WPF;component"
    let iconSource = x.Factory.Backing(<@ x.IconSource @>, Disabled)
    let currentView = x.Factory.Backing<ViewModelBase>(<@ x.CurrentView @>, new LoginViewModel())

    let goToTrackInfoPage config trackData =
        currentView.Value <- new RadioIsPlayingViewModel(trackData)

    let goToNoTrackPage config =
        currentView.Value <- new NoRadioIsPlayingViewModel()

    let stateTransitions currentState event =
        match currentState, event with
        | (LoggedOut, Playing trackData), HasLoggedIn config -> 
            goToTrackInfoPage config trackData
            LoggedIn config, Playing trackData
        | (LoggedOut, NotPlaying), HasLoggedIn config -> 
            goToNoTrackPage config
            LoggedIn config, NotPlaying
        | (LoggedOut, _), TrackStarted (DiFm trackData) ->
            // No special actions needed.
            LoggedOut, Playing trackData
        | (LoggedOut, _), TrackStarted Other
        | (LoggedOut, _), TrackStopped ->
            // No special actions needed.
            LoggedOut, NotPlaying

        | (LoggedIn _, _), HasLoggedIn _ -> 
            failwith "Unexpected 'Logged in' event: already logged in."
        | (LoggedIn config, _), TrackStarted (DiFm trackData) ->
            goToTrackInfoPage config trackData
            LoggedIn config, Playing trackData
        | (LoggedIn config, _), TrackStarted Other
        | (LoggedIn config, _), TrackStopped ->
            goToNoTrackPage config
            LoggedIn config, NotPlaying


    let mutable state = LoggedOut, Playing "Penetralia - Forest"
    let eventHandler e = state <- stateTransitions state e
    let eventsSubscription = Observable.subscribe eventHandler eventsStream
    
    member x.IconSource
        with get() = match iconSource.Value with
                     | Enabled -> resourcePrefix + "/Icons/enabled.ico"
                     | _ -> resourcePrefix + "/Icons/disabled.ico"

    member x.CurrentView
        with get() = currentView.Value
        and set(value: ViewModelBase) = currentView.Value <- value

    interface System.IDisposable with
        member x.Dispose() = eventsSubscription.Dispose()