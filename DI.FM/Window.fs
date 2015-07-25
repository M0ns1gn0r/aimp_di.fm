module Window

open System
open System.Windows
open Hardcodet.Wpf.TaskbarNotification

let createTaskBarIcon () = 
    let tbi = new TaskbarIcon()        
    tbi.IconSource <- new Media.Imaging.BitmapImage(new Uri("pack://application:,,,/DI.FM;component/logo.ico"))
    tbi.ToolTipText <- "This is the DI.FM notification."
    tbi

(*let win = new Window()
win.Title <- "Say Hello"
win.ShowInTaskbar <- false
win.ShowActivated <- false
win.Visibility <- Visibility.Hidden
win.WindowStyle <- WindowStyle.None
win.WindowStartupLocation <- WindowStartupLocation.Manual
win.Top <- -1000.0
win.Left <- -1000.0
//win.Width <- 100.0
//win.Height <- 100.0
//win.Content <- tbi

let init () = 
    win.Show()

let deinit () = 
    win.Close()*)