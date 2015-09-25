namespace DI.FM.WPF.ViewModels

open DI.FM.WPF.Logic

type FullTrackData = {
    ChannelName: string
    Artist: string
    Title: string
    Likes: int
    Dislikes: int
}

type RadioIsPlayingViewModel(track: FullTrackData) =
    inherit FSharp.ViewModule.ViewModelBase()

    member x.ChannelName = track.ChannelName.ToUpper()
    member x.Artist = track.Artist
    member x.Title = track.Title
    member x.Likes = track.Likes
    member x.Dislikes = track.Dislikes