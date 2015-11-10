module DI.FM

open AIMP.SDK
open AIMP.SDK360
open AIMP.SDK.Services.MenuManager
open AIMP.SDK.UI.MenuItem
open DI.FM.WPF
open DI.FM.WPF.Logic
open System.IO
open Newtonsoft.Json

/// Makes the UI culture invariant so that exceptions are not getting localized.
let initUICulture () =
    System.Threading.Thread.CurrentThread.CurrentUICulture <-
        System.Globalization.CultureInfo.InvariantCulture

let loadConfig profilePath =
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
type Plugin() = 
    inherit AimpPlugin()
     
    let menuType = ParentMenuType.AIMP_MENUID_PLAYER_TRAY
    let menuItemPlus = new StandartMenuItem("DI.FM vote up");
    let menuItemMinus = new StandartMenuItem("DI.FM vote down");

    let mutable configChangeSubscription: System.IDisposable = null;

    override this.HasSettingDialog = false
    override this.ShowSettingDialog(wnd) = ()

    override this.Initialize() =
        initUICulture()
        Interop.resetFPU ()
        UI.start()

        // TODO: register hotkeys.

        // Form config file path and request its loading.
        let profilePath =
            this.Player.Core.GetPath(AimpMessages.AimpCorePathType.AIMP_CORE_PATH_PROFILE)
        configChangeSubscription <- loadConfig profilePath

        // Transform AIMP's track changed events into internal events.
        let trackChangedHandler _ =
            let fi = this.Player.CurrentFileInfo
            { 
                Artist = fi.Artist;
                Title = fi.Title;
                StreamUrl = fi.FileName 
            }
            |> Logic.raiseTrackChanged 
        this.Player.TrackChanged.Add trackChangedHandler

    override this.Dispose() =
        UI.stop()

        if (configChangeSubscription <> null) then configChangeSubscription.Dispose()

        this.Player.MenuManager.Delete(menuItemMinus)
        this.Player.MenuManager.Delete(menuItemPlus)
