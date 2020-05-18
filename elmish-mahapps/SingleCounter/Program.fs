module SingleCounter.Program

open System
open Elmish
open Elmish.WPF
open MahApps.Metro.Controls.Dialogs

type IViewService =
    abstract member ShowMessage: title: string * message: string -> Async<unit>

let view, setView =
    let v = ref Unchecked.defaultof<IViewService>
    (fun () -> v.Value), (fun s -> v.Value <- s)

type Model = {
    Count: int
    StepSize: int
}

let init () =
    {
        Count = 0
        StepSize = 1
    }, Cmd.none

type Msg =
    | Increment
    | Decrement
    | SetStepSize of int
    | Reset
    | ShowMessage of title: string * message: string

let update msg m =
    match msg with
    | Increment -> { m with Count = m.Count + m.StepSize }, Cmd.none
    | Decrement -> { m with Count = m.Count - m.StepSize }, Cmd.none
    | SetStepSize x -> { m with StepSize = x }, Cmd.none
    | Reset -> init()
    | ShowMessage (title, message) ->
        view().ShowMessage(title, message) |> Async.StartImmediate
        m, Cmd.none

let bindings () : Binding<Model, Msg> list = [
    "CounterValue" |> Binding.oneWay (fun m -> m.Count)
    "Increment" |> Binding.cmd Increment
    "Decrement" |> Binding.cmd Decrement
    "StepSize" |> Binding.twoWay(
        (fun m -> float m.StepSize),
        int >> SetStepSize)
    "Reset" |> Binding.cmd Reset
    "RadioButtons" |> Binding.oneWay (fun _ -> ["option 1"; "option 2"; "option 3"])
    "ShowMessage" |> Binding.cmd (ShowMessage("Hello", "Hello world!"))
]

[<EntryPoint; STAThread>]
let main _ =
    let mainWindow = SingleCounter.Views.MainWindow()
    setView
        { new IViewService with
            member this.ShowMessage(title, message) =
                mainWindow.ShowMessageAsync(title, message) |> Async.AwaitTask |> Async.Ignore
        } |> ignore

    Program.mkProgramWpf init update bindings
    |> Program.withConsoleTrace
    |> Program.runWindowWithConfig
        { ElmConfig.Default with LogConsole = true; Measure = true }
        mainWindow
