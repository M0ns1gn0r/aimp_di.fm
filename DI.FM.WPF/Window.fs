module DI.FM.WPF.Window
open Hardcodet.Wpf.TaskbarNotification

type TaskBarIconResource = FsXaml.XAML<"TaskBarIcon.xaml">

let mutable _icon = Option<TaskbarIcon>.None
let show () =
    let resourceDictionary = new TaskBarIconResource()
    let icon = resourceDictionary.Root.Item "TheNotifyIcon"
    _icon <- match icon :?> TaskbarIcon with
                | null -> None
                | i -> Some(i)
let hide () =
    _icon |> Option.iter (fun i -> i.Dispose())