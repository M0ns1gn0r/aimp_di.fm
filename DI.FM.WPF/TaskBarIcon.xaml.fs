namespace DI.FM.WPF.ViewModels

type Icons = Enabled | Disabled | Error

type TaskBarIconViewModel() as x =
    inherit FSharp.ViewModule.ViewModelBase()

    let resourcePrefix = "pack://application:,,,/DI.FM.WPF;component"
    let iconSource = x.Factory.Backing(<@ x.IconSource @>, Disabled)


    member x.IconSource
        with get() = match iconSource.Value with
                     | Enabled -> resourcePrefix + "/Icons/enabled.ico"
                     | _ -> resourcePrefix + "/Icons/disabled.ico"


type LoginViewModel() as x =
    inherit FSharp.ViewModule.ViewModelBase()

    member x.Title = "This is a login page."


type NoTrackPlaysViewModel() as x =
    inherit FSharp.ViewModule.ViewModelBase()

    member x.Title = "No DI.FM tracks are playing at the moment."