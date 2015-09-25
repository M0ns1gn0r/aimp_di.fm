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

    let loginFormVisibility = x.Factory.Backing(<@ x.LoginFormVisibility @>, Visibility.Visible)
    let logoutFormVisibility = x.Factory.Backing(<@ x.LogoutFormVisibility @>, Visibility.Collapsed)

    let hasValue str = not(System.String.IsNullOrWhiteSpace(str))
    let login = x.Factory.Backing(<@ x.Login @>, "rfever@gmail.com", notNullOrWhitespace)

    let goToTrackInfoPage (config: Config) (trackData: TrackData) =
        let track = trackData.artist + " - " + trackData.title

        // TODO: add exception handling: the station might not exist.
        let channel = config.Channels.[trackData.channelKey]

        let trackResult = findTrackInHistory channel.Id track
        match trackResult with
        | Pass track | Warn (track, _) ->
            let fullTrackData = {
                ChannelName = channel.Name
                Artist = track.Artist
                Title = track.Title
                Likes = track.Votes.Up
                Dislikes = track.Votes.Down

            }
            currentView.Value <- new RadioIsPlayingViewModel(fullTrackData)
            iconSource.Value <- Enabled
        | Fail (TrackNotFoundInHistory::[]) ->
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
            loginFormVisibility.Value <- Visibility.Collapsed
            logoutFormVisibility.Value <- Visibility.Visible
            goToTrackInfoPage config trackData
            LoggedIn config, Playing trackData
        | (LoggedOut, NotPlaying), HasLoggedIn config ->        
            loginFormVisibility.Value <- Visibility.Collapsed
            logoutFormVisibility.Value <- Visibility.Visible
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
    
    let doLogin (passwordBox: System.Windows.Controls.PasswordBox) =
        match loginToDiFm x.Login passwordBox.Password with
        | Pass config | Warn (config, _) ->
            raiseEvent <| Events.HasLoggedIn config
        | Fail errors -> 
            let errorToString = function
            | InvalidCredentials -> "Invalid credentials."
            | NoCookiesFound -> "No cookies found."
            | SessionCookieNotFound -> "Session cookie not found."
            | LoginFailed ex -> sprintf "Login failed: %O" ex
            | LoadConfigFailed ex -> sprintf "Config load failed: %O" ex
            | TrackHistoryLookupFailed ex -> sprintf "Failed to retrieve track history: %O" ex
            | TrackNotFoundInHistory -> sprintf "Failed to find the track in the track history."
            | VoteFailed ex -> sprintf "Failed to cast a vote: %O" ex

            errors
            |> Seq.map errorToString
            |> String.concat ", "
            |> System.Diagnostics.Debug.WriteLine
            |> ignore

    member x.IconSource
        with get() = match iconSource.Value with
                     | Enabled -> resourcePrefix + "/Icons/enabled.ico"
                     | _ -> resourcePrefix + "/Icons/disabled.ico"
    member x.CurrentView
        with get() = currentView.Value
        and set(value: ViewModelBase) = currentView.Value <- value
    member x.LoginFormVisibility
        with get() = loginFormVisibility.Value
        and set(value: Visibility) = loginFormVisibility.Value <- value
    member x.LogoutFormVisibility
        with get() = logoutFormVisibility.Value
        and set(value: Visibility) = logoutFormVisibility.Value <- value
    member x.Login 
        with get() = login.Value and set(value: string) = login.Value <- value
    member x.LoginCommand = 
        // TODO: use an asynchronous version.
        // TODO: any way to check the password field changed too?
        x.Factory.CommandSyncParamChecked(
            doLogin,
            (fun _ -> hasValue x.Login ),
            [ <@ x.Login @> ])

    interface System.IDisposable with
        // TODO: looks like doesn't work - ensure and remove.
        member x.Dispose() = eventsSubscription.Dispose()