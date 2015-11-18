namespace DI.FM.WPF.ViewModels

open FSharp.ViewModule
open FSharp.ViewModule.Validation
open Chessie.ErrorHandling.Trial
open DI.FM.Client
open DI.FM.WPF.Logic
open System.Windows

type Icons = Enabled | Disabled | Error

type TaskBarIconViewModel() as x =
    inherit ViewModelBase()
    
    let resourcePrefix = "pack://application:,,,/DI.FM.WPF;component"
    let iconSource = x.Factory.Backing(<@ x.IconSource @>, Disabled)
    let currentView = x.Factory.Backing<ViewModelBase>(<@ x.CurrentView @>, new LoginViewModel())

    let loggedInAs = x.Factory.Backing(<@ x.LoggedInAs @>, null)

    let goToTrackInfoPage (config: Config) (trackData: TrackData) =
        let track = trackData.Artist + " - " + trackData.Title

        // TODO: add exception handling: the station might not exist.
        let channel = config.Channels.[trackData.ChannelKey]

        let trackResult = findTrackInHistory channel.Id track
        match trackResult with
        | Pass track | Warn (track, _) ->
            let fullTrackData = {
                ChannelId = channel.Id
                ChannelName = channel.Name
                AlbumArtUrl = 
                    match track.ArtUrl with
                    | null | "" -> None
                    | artUrl -> Some ("http:" + artUrl)
                TrackId = track.TrackId
                Artist = track.Artist
                Title = track.Title
                Likes = track.Votes.Up
                Dislikes = track.Votes.Down
            }
            currentView.Value <- new RadioIsPlayingViewModel(config.ApiKey, fullTrackData)
            iconSource.Value <- Enabled
        | Fail [TrackNotFoundInHistory] ->
            // TODO: what to do if TrackNotInHistory?
            System.Diagnostics.Debug.WriteLine ("Track not in history: " + track)
        | Fail _ ->
            // TODO: go to error view instead?
            failwith "findTrackInHistory failed"

    let goToNoTrackPage config =
        currentView.Value <- new NoRadioIsPlayingViewModel()
        iconSource.Value <- Disabled

    let stateTransitions currentState event =
        match currentState, event with
        | (LoggedOut, Playing trackData), HasLoggedIn config ->
            loggedInAs.Value <- config.LoggedInAs
            goToTrackInfoPage config trackData
            LoggedIn config, Playing trackData
        | (LoggedOut, NotPlaying), HasLoggedIn config ->
            loggedInAs.Value <- config.LoggedInAs
            goToNoTrackPage config
            LoggedIn config, NotPlaying
        | (LoggedOut, _), TrackStarted (DiFm trackData) ->
            iconSource.Value <- Disabled
            LoggedOut, Playing trackData
        | (LoggedOut, _), TrackStarted Other
        | (LoggedOut, _), TrackStopped ->
            iconSource.Value <- Disabled
            LoggedOut, NotPlaying

        | (LoggedIn _, _), HasLoggedIn _ ->
            // TODO: write to log?
            failwith "Unexpected 'Logged in' event: already logged in."
        | (LoggedIn config, _), TrackStarted (DiFm trackData) ->
            goToTrackInfoPage config trackData
            LoggedIn config, Playing trackData
        | (LoggedIn config, _), TrackStarted Other
        | (LoggedIn config, _), TrackStopped ->
            goToNoTrackPage config
            LoggedIn config, NotPlaying


    let mutable state = LoggedOut, NotPlaying
    let eventHandler e = state <- stateTransitions state e
    let eventsSubscription = Observable.subscribe eventHandler eventsStream
    
    member x.IconSource
        with get() = match iconSource.Value with
                     | Enabled -> resourcePrefix + "/Icons/enabled.ico"
                     | _ -> resourcePrefix + "/Icons/disabled.ico"
    member x.CurrentView
        with get() = currentView.Value
        and set(value: ViewModelBase) = currentView.Value <- value
    member x.IsLoggedIn
        with get() = loggedInAs.Value <> null
    member x.LoggedInAs
        with get() = loggedInAs.Value

    interface System.IDisposable with
        // TODO: looks like doesn't work - ensure and remove.
        member x.Dispose() = eventsSubscription.Dispose()