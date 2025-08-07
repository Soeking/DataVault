module DataVault.front.LayoutPage

open Giraffe.ViewEngine

let masterPage (pageTitle: string) (content: XmlNode list) =
    html
        []
        [ head
              []
              [ title [] [ str pageTitle ]
                style [] [ rawText "label { display: inline-block; width: 80px; }" ]
                link [ _rel "stylesheet"; _href "/css/style.css" ]
                link [ _rel "shortcut icon"; _type "image/x-icon"; _href "/img/soeki_logo.ico" ] ]
          body
              []
              [ div [ _class "header-bar" ] [ a [ _href "/" ] [ img [ _src "/img/soeki_logo.png" ] ] ]
                h1 [] [ str pageTitle ]
                main [ _class ".main" ] content ] ]
