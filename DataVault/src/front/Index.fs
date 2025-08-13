module DataVault.front.Index

open DataVault.front.LayoutPage
open Giraffe.ViewEngine

let indexPage =
    [ p [] [ a [ _href "/register" ] [ str "Register" ] ]
      p [] [ a [ _href "/login" ] [ str "Login" ] ] ]
    |> masterPage "Home"