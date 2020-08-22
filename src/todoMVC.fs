module App.todoMVC

open Browser.Types
open Feliz

module Utils =
    open Browser
    open Fable.Core
    open Fable.Core.JsInterop
    let load<'T> key: 'T option =
        window.localStorage.getItem(key)
        |> Option.ofObj
        |> Option.map (fun json -> !!JS.JSON.parse(json))

    let save key (data: 'T) =
        window.localStorage.setItem(key, JS.JSON.stringify data)

open System
type Todo =
    { id: Guid
      title: string
      completed: bool }

type TodoModel(key) = 
    member val key = key
    member val todos: Todo[] = defaultArg (Utils.load key) [||] with get, set
    member val onChanges: (unit -> unit)[] = [||] with get, set
    
    member this.subscribe(onChange) =
        this.onChanges <- [|onChange|]
        
    member this.inform() = 
        Utils.save this.key this.todos
        this.onChanges |> Seq.iter (fun cb -> cb())
        
    member this.addTodo(title) =
        this.todos <-
            [|{id = Guid.NewGuid(); title = title; completed = false }|]
            |> Array.append this.todos
        this.inform()
        
    member this.toggleAll(checked') =
        this.todos <- this.todos |> Array.map (fun todo ->
            { todo with completed = checked' })
        this.inform()
        
    member this.toggle(todoToToggle) =
        this.todos <- this.todos |> Array.map (fun todo ->
            if todo.id <> todoToToggle.id then todo
            else {todo with completed = (not todo.completed)})
        this.inform()
        
    member this.destroy(todoToDestroy) =
        this.todos <- this.todos |> Array.filter (fun todo ->
            todo.id <> todoToDestroy.id)
        this.inform()
    
    member this.save(todoToSave, text) =
        this.todos <- this.todos |> Array.map (fun todo ->
            if todo.id <> todoToSave.id then todo
            else {todo with title = text})
        this.inform()
        
let [<Literal>] ESCAPE_KEY = 27.
let [<Literal>] ENTER_KEY = 13.

let todoItem = React.functionComponent("todoItem", fun (props : {| key: Guid
                                                                   todo: Todo
                                                                   editing: bool
                                                                   onSave: string->unit
                                                                   onEdit: unit->unit
                                                                   onDestroy: unit->unit
                                                                   onCancel: unit->unit
                                                                   onToggle: unit->unit |}) ->
    
    let inputRef = React.useRef(None)
    let inputValue, setInputValue = React.useState(props.todo.title)
    let handleSubmit _ =
        match inputValue.Trim() with
        | value when value.Length > 0 -> props.onSave(value)
        | _ -> ()
        
    let handleKeydown (e: KeyboardEvent) =
        match e.which with
        | ESCAPE_KEY ->
            setInputValue props.todo.title
            props.onCancel()
        | ENTER_KEY ->
            handleSubmit()
        | _ -> ()
        
    Html.li [
        prop.className (if props.todo.completed then ["completed" ] else [])
        prop.className (if props.editing then ["editing" ] else [])
        prop.children [
            Html.div [
               prop.className "view"
               prop.children [
                   Html.input [
                       prop.className "toggle"
                       prop.type' "checkbox"
                       prop.isChecked props.todo.completed
                       prop.onCheckedChange (fun _ -> props.onToggle())
                   ]
                   Html.label [
                       prop.onDoubleClick (fun _ -> props.onEdit())
                       prop.text props.todo.title
                   ]
                   Html.button [
                       prop.className "destroy"
                       prop.onClick (fun _ -> props.onDestroy())
                       prop.text "del"
                   ]
                   
               ]
            ]
            
            Html.input [
                prop.className "edit"
                prop.ref inputRef
                prop.value inputValue
                prop.onChange setInputValue
                prop.onBlur  handleSubmit
                prop.onKeyDown handleKeydown
            ]
        ]
    ]
    )
    
let TodoApp = React.functionComponent("TodoApp", fun (props: {|model: TodoModel|}) ->
    
    let newTodo, setNewTodo = React.useState("")
    let editingId, setEditingId = React.useState(Option<Guid>.None)
    
    let handleNewTodoKeyDown (ev: KeyboardEvent) =
        if ev.keyCode = ENTER_KEY then
            ev.preventDefault()
            let v = newTodo.Trim()
            if v.Length > 0 then
                props.model.addTodo(v)
                setNewTodo ""
                
    let toggle (todoToToggle) =
        props.model.toggle(todoToToggle)
        
    let destroy (todo) =
        props.model.destroy(todo)
        
    let edit (todo: Todo) =
        setEditingId (Some todo.id)
        
    let save (todoToSave, text) =
        props.model.save(todoToSave, text)
        setEditingId None
        
    let cancel () =
        setEditingId None
        
    let (todos, setTodos) = React.useState(props.model.todos)
    props.model.subscribe(fun _ -> setTodos(props.model.todos))
    
    let todoItems =
        todos
        |> Seq.map (fun todo ->
            todoItem (
                    {| key = todo.id
                       todo = todo
                       onToggle = fun _ -> toggle(todo)
                       onDestroy = fun _ -> destroy(todo)
                       onEdit = fun _ -> edit(todo)
                       onSave = fun text -> save(todo, text)
                       onCancel = cancel
                       editing =
                             match editingId with
                             | Some editing -> editing = todo.id
                             | None -> false
                    |})
            )
        |> Seq.toList
        
    Html.div [
        Html.header [
            prop.className "header"
            prop.children [
               Html.h1 "todos"
               Html.input [
                   prop.className "new-todo"
                   prop.placeholder "What needs to be done?"
                   prop.value newTodo
                   prop.onChange setNewTodo
                   prop.autoFocus true
                   prop.onKeyDown handleNewTodoKeyDown
               ]
            ]
        ]
        
        Html.div [
            prop.className "todoapp"
            prop.children [
                 Html.ul [
                    prop.className "todo-list"
                    prop.children todoItems      
                ]
            ]
        ]
    ]
    )



let model = TodoModel("react-todos")

let app =  TodoApp {| model = model |}
