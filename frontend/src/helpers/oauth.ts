

export function getOidcUrl(): string {
    const my_url = "http://localhost:5173/";
    const search_params = new URLSearchParams({
      // TODO: read these from env variables or backend or something
      client_id: "9d5ed736-7954-4602-b311-116a30e2f262",
      response_type: "id_token",
      redirect_url: `${my_url}`,
      // I dont think redirecting with a GET to the frontend is the typical way to do this
      // but I think it is nicer if the backend doesn't have to know the url of the frontend
      response_mode: "fragment",
      scope: "openid",
      prompt: "select_account",
    
    })
    return `https://login.microsoftonline.com/common/oauth2/v2.0/authorize?${search_params}`
}