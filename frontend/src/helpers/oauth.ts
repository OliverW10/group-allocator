

export function getOidcUrl(): string {
    const my_url = "http://localhost:5173";
    // https://developers.google.com/identity/openid-connect/openid-connect#authenticationuriparameters
    const search_params = new URLSearchParams({
      // TODO: read these from env variables or backend or something
      client_id: "516503324384-h73aa00v5gectf0oqr0c9fber709gm0s.apps.googleusercontent.com",
      response_type: "token id_token",
      redirect_uri: `${my_url}`,
      // I dont think redirecting with a GET to the frontend is the typical way to do this
      // but I think it is nicer if the backend doesn't have to know the url of the frontend
      response_mode: "fragment",
      scope: "openid profile email",
      prompt: "select_account",
      nonce: crypto.randomUUID(),
      hd: "uts.edu.au"
    })
    return `https://accounts.google.com/o/oauth2/v2/auth?${search_params}`
}