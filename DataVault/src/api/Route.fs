module DataVault.api.Route

open DataVault.api.PostFunc
open DataVault.front.FileDownload
open DataVault.front.Index
open DataVault.front.Login
open DataVault.front.Register
open Giraffe
open Microsoft.AspNetCore.Http

let mustBeLoggedIn: HttpHandler = requiresAuthentication (redirectTo false "/login")

let webRouting: HttpFunc -> HttpContext -> HttpFuncResult =
    choose
        [ GET
          >=> choose
                  [ route "/" >=> mustBeLoggedIn >=> htmlView indexPage
                    route "/register" >=> htmlView registerPage
                    route "/login" >=> htmlView (loginPage false)
                    route "/logout" >=> mustBeLoggedIn >=> logoutHandler
                    route "/file" >=> mustBeLoggedIn >=> htmlView filePage
                    route "/file/sample" >=> mustBeLoggedIn >=> dynamicDownloadHandler ]
          POST
          >=> choose
                  [ route "/register" >=> registerHandler
                    route "/login" >=> loginHandler
                    route "/stock/insert" >=> insertStockHandler ]
          setStatusCode 404 >=> text "Not Found" ]
