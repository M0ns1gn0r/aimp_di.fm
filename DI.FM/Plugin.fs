namespace DI.FM

open AIMP.SDK

[<AimpPlugin("DI.FM", "Roman Nikitin", "1")>]
type Plugin() = 
    inherit AimpPlugin()
    override this.HasSettingDialog = false
    override this.ShowSettingDialog(wnd) = ()
    override this.Initialize() = ()
    override this.Dispose() = ()