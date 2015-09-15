module DI.FM

open AIMP.SDK
open AIMP.SDK360
open AIMP.SDK.Services.MenuManager
open AIMP.SDK.UI.MenuItem
open DI.FM.WPF
open DI.FM.WPF.Logic

/// Makes the UI culture invariant so that exceptions are not getting localized.
let initUICulture () =
    System.Threading.Thread.CurrentThread.CurrentUICulture <-
        System.Globalization.CultureInfo.InvariantCulture

[<AimpPlugin("DI.FM", "Roman Nikitin", "1")>]
type Plugin() = 
    inherit AimpPlugin()
     
    let menuType = ParentMenuType.AIMP_MENUID_PLAYER_TRAY
    let menuItemPlus = new StandartMenuItem("DI.FM vote up");
    let menuItemMinus = new StandartMenuItem("DI.FM vote down");

    override this.HasSettingDialog = false
    override this.ShowSettingDialog(wnd) = ()

    override this.Initialize() =
        initUICulture()
        Interop.resetFPU ()
        UI.start()

        // TODO: register hotkeys.

        let trackChangedHandler _ =
            let fi = this.Player.CurrentFileInfo
            { 
                artist = fi.Artist;
                title = fi.Title;
                streamUrl = fi.FileName 
            }
            |> Logic.raiseTrackChanged 
        this.Player.TrackChanged.Add trackChangedHandler

    override this.Dispose() =
        UI.stop()

        this.Player.MenuManager.Delete(menuItemMinus)
        this.Player.MenuManager.Delete(menuItemPlus)
