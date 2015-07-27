namespace DI.FM.WPF

open System
open System.Windows
open FSharp.ViewModule
open FSharp.ViewModule.Validation
open FsXaml

type MainWindow = XAML<"MainWindow.xaml", true>

type MainViewModel() = 
    inherit ViewModelBase()
    member this.Title = "This is the MainWindow"