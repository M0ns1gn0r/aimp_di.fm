module DI.FM.WPF.UI
open Hardcodet.Wpf.TaskbarNotification

type TaskBarIconResource = FsXaml.XAML<"Icon.xaml">

let mutable _icon = Option<TaskbarIcon>.None
let start () =
    let resourceDictionary = new TaskBarIconResource()
    let icon = resourceDictionary.Root.Item "TheNotifyIcon"
    _icon <- match icon :?> TaskbarIcon with
                | null -> None
                | i -> Some(i)
let stop () =
    _icon |> Option.iter (fun i -> i.Dispose())