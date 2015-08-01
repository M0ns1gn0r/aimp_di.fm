module Interop
    [<System.Runtime.InteropServices.DllImport(
        "msvcrt.dll",
        CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)>]
    extern int _controlfp(int newControl, int mask)

    /// <summary>Resets the floating point broken by the Delphi.</summary>
    /// <remarks>See https://social.msdn.microsoft.com/Forums/vstudio/en-US/a31f9c7a-0e15-4a09-a544-bec07f0f152c/systemarithmeticexception-overflow-or-underflow-in-the-arithmetic-operation?forum=wpf</remarks>
    let resetFPU () =
        _controlfp(0x9001F, 0xfffff) |> ignore