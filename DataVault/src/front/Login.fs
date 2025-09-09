module DataVault.front.Login

open DataVault.front.LayoutPage
open Giraffe
open Giraffe.ViewEngine
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Identity

let loginPage (loginFailed: bool)  =
    [ if loginFailed then
          yield p [ _style "color: Red;" ] [ str "Login failed." ]

      yield
          form
              [ _action "/login"; _method "POST" ]
              [ div [] [ label [] [ str "User name:" ]; input [ _name "UserName"; _type "text" ] ]
                div [] [ label [] [ str "Password:" ]; input [ _name "Password"; _type "password" ] ]
                input [ _type "submit" ] ]
      yield
          p
              []
              [ str "Don't have an account yet?"
                a [ _href "/register" ] [ str "Go to registration" ] ] ]
    |> masterPage "Login"

[<CLIMutable>]
type LoginModel = { UserName: string; Password: string }

let loginHandler: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! model = ctx.BindFormAsync<LoginModel>()
            let signInManager = ctx.GetService<SignInManager<IdentityUser>>()
            let! result = signInManager.PasswordSignInAsync(model.UserName, model.Password, true, false)

            match result.Succeeded with
            | true -> return! redirectTo false "/" next ctx
            | false -> return! htmlView (loginPage true) next ctx
        }

let logoutHandler: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let signInManager = ctx.GetService<SignInManager<IdentityUser>>()
            do! signInManager.SignOutAsync()
            return! (redirectTo false "/login") next ctx
        }