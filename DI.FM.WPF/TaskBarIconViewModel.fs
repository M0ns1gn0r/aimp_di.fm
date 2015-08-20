namespace DI.FM.WPF.ViewModels

open FSharp.ViewModule
open FSharp.ViewModule.Validation

type Icons = Enabled | Disabled | Error

type TaskBarIconViewModel() as x =
    inherit ViewModelBase()
    
    let resourcePrefix = "pack://application:,,,/DI.FM.WPF;component"
    let iconSource = x.Factory.Backing(<@ x.IconSource @>, Disabled)
    let currentView = x.Factory.Backing<ViewModelBase>(<@ x.CurrentView @>, new LoginViewModel())

    let eventsSubscription =
        DI.FM.State.eventsStream
        |> Observable.filter (fun e -> e = DI.FM.State.Events.LoggedIn)
        |> Observable.subscribe (fun _ -> currentView.Value <- new NoTrackPlaysViewModel())

    member x.IconSource
        with get() = match iconSource.Value with
                     | Enabled -> resourcePrefix + "/Icons/enabled.ico"
                     | _ -> resourcePrefix + "/Icons/disabled.ico"

    member x.CurrentView
        with get() = currentView.Value
        and set(value: ViewModelBase) = currentView.Value <- value

    interface System.IDisposable with
        member x.Dispose() = eventsSubscription.Dispose()