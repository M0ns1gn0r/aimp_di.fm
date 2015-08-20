namespace DI.FM.WPF.ViewModels

open DI.FM.Client
open Chessie.ErrorHandling.Trial
open FSharp.ViewModule.Validation
open DI.FM.State

type LoginViewModel() as x =
    inherit FSharp.ViewModule.ViewModelBase()

    let hasValue str = not(System.String.IsNullOrWhiteSpace(str))

    let login = x.Factory.Backing(<@ x.Login @>, "Your login", notNullOrWhitespace)
    let password = x.Factory.Backing(<@ x.Password @>, "", notNullOrWhitespace)

    let doLogin () =
        ()
        raiseEvent Events.LoggedIn
        
        (*config <-
            match loginToDiFm x.Login x.Password with
            | Pass config | Warn (config, _) ->
                // Navigate to another view.
                raiseEvent Events.LoggedIn
                Some config
            | Fail errors -> 
                let errorToString = function
                | InvalidCredentials -> "Invalid credentials."
                | NoCookiesFound -> "No cookies found."
                | SessionCookieNotFound -> "Session cookie not found."
                | LoginFailed ex -> sprintf "Login failed: %O" ex
                | LoadConfigFailed ex -> sprintf "Config load failed: %O" ex
                | TrackHistoryLookupFailed ex -> sprintf "Failed to retrieve track history: %O" ex
                | VoteFailed ex -> sprintf "Failed to cast a vote: %O" ex

                errors
                |> Seq.map errorToString
                |> String.concat ", "
                |> System.Diagnostics.Debug.WriteLine
                |> ignore
                None
        ()*)

    member x.Login 
        with get() = login.Value and set(value: string) = login.Value <- value
    member x.Password
        with get() = password.Value and set(value: string) = password.Value <- value
    member x.LoginCommand = 
        // TODO: use an asynchronous version.
        x.Factory.CommandSyncChecked(
            doLogin,
            (fun _ -> hasValue x.Login && hasValue x.Password),
            [ <@ x.Login @> ; <@ x.Password @> ])

