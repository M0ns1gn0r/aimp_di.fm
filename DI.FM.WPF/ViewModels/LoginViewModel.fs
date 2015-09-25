namespace DI.FM.WPF.ViewModels

open DI.FM.Client
open DI.FM.WPF.Logic
open Chessie.ErrorHandling.Trial
open FSharp.ViewModule.Validation

type LoginViewModel() as x =
    inherit FSharp.ViewModule.ViewModelBase()
