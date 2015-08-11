module DI.FM.WPF.UI
open Hardcodet.Wpf.TaskbarNotification

type TaskBarIconResource = FsXaml.XAML<"TaskBarIcon.xaml">

let mutable _taskbarIcon = Option<TaskbarIcon>.None
let start () =
    let resourceDictionary = new TaskBarIconResource()
    let icon = resourceDictionary.Root.Item "TheNotifyIcon"
    _taskbarIcon <- match icon :?> TaskbarIcon with 
                    | null -> None
                    | i -> Some(i)
let stop () =
    _taskbarIcon |> Option.iter (fun i -> i.Dispose())