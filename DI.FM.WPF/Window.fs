module DI.FM.WPF.Window

let show () = 
    // This line simply forces .NET to load the reference. Othewise the XAML parser won't
    // find the TaskBar, and won't even try to load the corresponding assembly.
    let unused = Hardcodet.Wpf.TaskbarNotification.BalloonIcon.None

    let window = (new MainWindow()).Root
    window.Show()

    // Return a window disposal delegate.
    let dispose = window.Close
    dispose