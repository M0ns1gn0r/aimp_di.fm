namespace DI.FM

open AIMP.SDK
open AIMP.SDK.Services.MenuManager
open AIMP.SDK.UI.MenuItem

[<AimpPlugin("DI.FM", "Roman Nikitin", "1")>]
type Plugin() = 
    inherit AimpPlugin()

    let menuType = ParentMenuType.AIMP_MENUID_PLAYER_TRAY
    let menuItemPlus = new StandartMenuItem("DI.FM vote +");
    let menuItemMinus = new StandartMenuItem("DI.FM vote -");

    override this.HasSettingDialog = false
    override this.ShowSettingDialog(wnd) = ()

    override this.Initialize() =
        this.Player.MenuManager.Add(menuType, menuItemPlus)
        this.Player.MenuManager.Add(menuType, menuItemMinus)

    override this.Dispose() =
        this.Player.MenuManager.Delete(menuItemPlus)
        this.Player.MenuManager.Delete(menuItemMinus)
