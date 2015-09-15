namespace DI.FM.WPF.ViewModels

open DI.FM.WPF.Logic

type RadioIsPlayingViewModel(trackData) =
    inherit FSharp.ViewModule.ViewModelBase()

    member x.Title = "Currently playing track: " + trackData