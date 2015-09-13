namespace DI.FM.WPF.ViewModels

type NoRadioIsPlayingViewModel() =
    inherit FSharp.ViewModule.ViewModelBase()

    member x.Title = "No DI.FM tracks are playing at the moment."