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

    let loggedIn config =
        currentView.Value <- new NoRadioIsPlayingViewModel()
        NotLoggedIn

    let trackStarted = id
    let trackStopped = id

    let mutable state = State.NotLoggedIn

    let eventHandler e = 
        let stateTransitions = setupStateMachine loggedIn trackStarted trackStopped
        state <- stateTransitions state e
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