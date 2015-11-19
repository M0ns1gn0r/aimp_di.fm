module DI.FM

open AIMP.SDK
open AIMP.SDK.Services.MenuManager
open AIMP.SDK.Services.Player
open AIMP.SDK.UI.MenuItem
open DI.FM.WPF
open DI.FM.WPF.Logic
open System.IO
open Newtonsoft.Json

/// Makes the UI culture invariant so that exceptions are not getting localized.
let private initUICulture () =
    System.Threading.Thread.CurrentThread.CurrentUICulture <-
        System.Globalization.CultureInfo.InvariantCulture

let private loadConfig profilePath =
    let configPath = Path.Combine(profilePath, "DI.FM", "config.json")
    
    // Read the config and raise HasLoggedIn event.
    if File.Exists configPath 
    then
        // TODO: the file operation could fail. How to handle the error?
        let json = File.ReadAllText configPath
        let config = JsonConvert.DeserializeObject<DI.FM.Client.Config> json
        HasLoggedIn config |> Logic.raiseEvent

    // Subscribe to config change.
    let writeConfigToFileHandler event =
        match event with
        | HasLoggedIn config ->
            let json = JsonConvert.SerializeObject config
            // TODO: the file operation could fail. How to handle the error?
            let folder = Path.GetDirectoryName configPath
            Directory.CreateDirectory folder |> ignore
            File.WriteAllText(configPath, json)
        | _ -> ()
    Observable.subscribe writeConfigToFileHandler Logic.eventsStream

[<AimpPlugin("DI.FM", "Roman Nikitin", "1")>]
type Plugin() as this = 
    inherit AimpPlugin()
     
    let menuType = ParentMenuType.AIMP_MENUID_PLAYER_TRAY
    let menuItemPlus = new StandartMenuItem("DI.FM vote up");
    let menuItemMinus = new StandartMenuItem("DI.FM vote down");

    let mutable configChangeSubscription: System.IDisposable = null;
    let mutable trackChangedSubscription: System.IDisposable = null;
    let mutable stateChangedHandler: AimpStateChanged = null;

    let subscribeToStateChanged (player: IAimpPlayer) =
        stateChangedHandler <- new AimpStateChanged(
            function 
            | AimpPlayerState.Stopped -> Logic.raiseEvent TrackStopped
            | _ -> ())
        player.add_StateChanged stateChangedHandler

    let subscribeToTrackChanged (player: IAimpPlayer) =
        let trackChangedHandler _ =
            let fi = this.Player.CurrentFileInfo
            { 
                Artist = fi.Artist;
                Title = fi.Title;
                StreamUrl = fi.FileName 
            }
            |> Logic.raiseTrackChanged 
        trackChangedSubscription <- player.TrackChanged.Subscribe trackChangedHandler

    override this.HasSettingDialog = false
    override this.ShowSettingDialog(wnd) = ()

    override this.Initialize() =
        let player = this.Player

        initUICulture()
        Interop.resetFPU ()
        UI.start()

        // TODO: register hotkeys.

        // Form config file path and request its loading.
        let profilePath = player.Core.GetPath(AimpMessages.AimpCorePathType.AIMP_CORE_PATH_PROFILE)
        configChangeSubscription <- loadConfig profilePath

        // Transform AIMP's events into internal events.
        subscribeToStateChanged player
        subscribeToTrackChanged player

    override this.Dispose() =
        UI.stop()

        let isNotNull = function null -> false | _ -> true
        if isNotNull configChangeSubscription then configChangeSubscription.Dispose()
        if isNotNull trackChangedSubscription then trackChangedSubscription.Dispose()
        if isNotNull stateChangedHandler then this.Player.remove_StateChanged stateChangedHandler

        this.Player.MenuManager.Delete(menuItemMinus)
        this.Player.MenuManager.Delete(menuItemPlus)