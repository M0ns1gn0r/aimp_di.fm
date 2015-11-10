namespace DI.FM.WPF.ViewModels

open DI.FM
open Chessie.ErrorHandling.Trial

type FullTrackData = {
    ChannelId: int
    ChannelName: string
    TrackId: int
    Artist: string
    Title: string
    Likes: int
    Dislikes: int
}

type RadioIsPlayingViewModel (apiKey: string, track: FullTrackData) as x =
    inherit FSharp.ViewModule.ViewModelBase()

    let hasVotedUp = x.Factory.Backing(<@ x.HasVotedUp @>, false)
    let hasVotedDown = x.Factory.Backing(<@ x.HasVotedDown @>, false)

    let castVote direction () = 
        match Client.vote apiKey track.ChannelId track.TrackId direction with
        | Pass _ | Warn _ -> 
            match direction with
                | Client.Vote.Up -> x.HasVotedUp <- true
                | Client.Vote.Down -> x.HasVotedDown <- true
        | Fail _ -> 
            // TODO: go to error view instead?
            failwith "vote failed"

    member x.ChannelName = track.ChannelName.ToUpper()
    member x.Artist = track.Artist
    member x.Title = track.Title
    member x.Likes = track.Likes
    member x.Dislikes = track.Dislikes

    member x.HasVotedUp 
        with get() = hasVotedUp.Value and set(value: bool) = hasVotedUp.Value <- value
    member x.HasVotedDown 
        with get() = hasVotedDown.Value and set(value: bool) = hasVotedDown.Value <- value

    member x.VoteUpCommand = 
        // TODO: use an asynchronous version.
        x.Factory.CommandSyncParamChecked(
            castVote Client.Vote.Up,
            (fun _ -> not x.HasVotedUp ),
            [ <@ x.HasVotedUp @> ])

    member x.VoteDownCommand = 
        // TODO: use an asynchronous version.
        x.Factory.CommandSyncParamChecked(
            castVote Client.Vote.Down,
            (fun _ -> not x.HasVotedDown),
            [ <@ x.HasVotedDown @> ])