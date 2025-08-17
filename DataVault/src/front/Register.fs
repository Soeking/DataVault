module DataVault.front.Register

open System
open System.IO
open System.Text
open DataVault.front.LayoutPage
open Giraffe
open Giraffe.ViewEngine
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Identity

let registerPage =
    [ form
          [ _action "/register"; _method "POST" ]
          [ div [] [ label [] [ str "Email:" ]; input [ _name "Email"; _type "text" ] ]
            div [] [ label [] [ str "User name:" ]; input [ _name "UserName"; _type "text" ] ]
            div [] [ label [] [ str "Password:" ]; input [ _name "Password"; _type "password" ] ]
            input [ _type "submit" ] ] ]
    |> masterPage "Register"

[<CLIMutable>]
type RegisterModel =
    { UserName: string
      Email: string
      Password: string }

let showErrors errorMessage = errorMessage |> text

let getAllowedUser =
    let filePath = Environment.GetEnvironmentVariable("ASP_AUTH_USER_FILE")
    let fileText = File.ReadAllText(filePath)
    fileText.Split("\n")

let registerHandler: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! model = ctx.BindFormAsync<RegisterModel>()

            if Array.contains model.UserName getAllowedUser then
                let user = IdentityUser(UserName = model.UserName, Email = model.Email)
                let userManager = ctx.GetService<UserManager<IdentityUser>>()
                let! result = userManager.CreateAsync(user, model.Password)

                match result.Succeeded with
                | false ->
                    return!
                        result.Errors
                        |> Seq.fold
                            (fun acc err ->
                                sprintf "Code: %s, Description: %s" err.Code err.Description |> acc.AppendLine
                                : StringBuilder)
                            (StringBuilder(""))
                        |> _.ToString()
                        |> fun msg -> showErrors msg next ctx
                | true ->
                    let signInManager = ctx.GetService<SignInManager<IdentityUser>>()
                    do! signInManager.SignInAsync(user, true)
                    return! redirectTo false "/user" next ctx
            else
                return! showErrors "invalid user" next ctx
        }
