module DataVault.front.Index

open DataVault.front.LayoutPage
open Giraffe.ViewEngine

let indexPage =
    [ p [] [ a [ _href "/file" ] [ str "file download" ] ]
      p [] [ a [ _href "/logout" ] [ str "Logout" ] ] ]
    |> masterPage "Home"