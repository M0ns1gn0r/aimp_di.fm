namespace DI.FM.WPF.ViewModels

open FSharp.ViewModule.Validation

type Icons = Enabled | Disabled | Error

type TaskBarIconViewModel() as x =
    inherit FSharp.ViewModule.ViewModelBase()

    let resourcePrefix = "pack://application:,,,/DI.FM.WPF;component"
    let iconSource = x.Factory.Backing(<@ x.IconSource @>, Disabled)

    member x.IconSource
        with get() = match iconSource.Value with
                     | Enabled -> resourcePrefix + "/Icons/enabled.ico"
                     | _ -> resourcePrefix + "/Icons/disabled.ico"