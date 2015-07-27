namespace DI.FM

open AIMP.SDK
open AIMP.SDK360
open AIMP.SDK.Services.MenuManager
open AIMP.SDK.UI.MenuItem
open DI.FM.WPF

[<AimpPlugin("DI.FM", "Roman Nikitin", "1")>]
type Plugin() = 
    inherit AimpPlugin()
     
    //let mutable taskbarIcon = null
    let mutable windowDisposeSubscription = fun () -> ()
    let menuType = ParentMenuType.AIMP_MENUID_PLAYER_TRAY
    let menuItemPlus = new StandartMenuItem("DI.FM vote +");
    let menuItemMinus = new StandartMenuItem("DI.FM vote -");

    override this.HasSettingDialog = false
    override this.ShowSettingDialog(wnd) = ()

    override this.Initialize() =
        //taskbarIcon <- Window.createTaskBarIcon ()
        //Window.createTaskBarIcon ()
        
        // TODO: register hotkeys.
                
        let menuItemPlusHandler _ =
            windowDisposeSubscription <- DI.FM.WPF.Window.show()
            
            // TODO: use AIMP_MSG_CMD_SHOW_NOTIFICATION to show the result of vote?
            this.AimpCore.SendMessage(AimpMessages.AimpCoreMessageType.AIMP_MSG_CMD_SHOW_NOTIFICATION, 0, "Hello world!")
        menuItemPlus.Click.Add menuItemPlusHandler

        let menuItemMinusHandler _ =
            windowDisposeSubscription()            
        menuItemMinus.Click.Add menuItemMinusHandler

        // Register menu items.
        this.Player.MenuManager.Add(menuType, menuItemMinus)
        this.Player.MenuManager.Add(menuType, menuItemPlus)

    override this.Dispose() =
        //taskbarIcon.Dispose()
        //windowDisposeSubscription()

        this.Player.MenuManager.Delete(menuItemMinus)
        this.Player.MenuManager.Delete(menuItemPlus)
