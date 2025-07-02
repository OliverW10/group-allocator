# GroupAllocator

## Production Setup

### Prerequisites
- Cloudflare Pages account
  - Any static hosting provider should work (including just putting the `dist` folder in front of a web server like nginx), but we use Cloudflare Pages and it works well.
- Docker / Container environment
  - We use Docker Compose, however anything that can run a container should work.
- PostgreSQL database
  - We also use Docker for PostgreSQL, however any existing PostgreSQL database should also work.

### Steps
1. Setup the frontend
   - Create a new Cloudflare Pages project [here](https://dash.cloudflare.com/?account=pages)
   - Connect using GitHub (use the Git url `https://github.com/OliverW10/group-allocator.git`)
   - Set the root directory to `frontend`
   - Set the output directory to `dist`
   - Set the environment variable `VITE_BACKEND_URL` to the URL of your backend API (e.g. `https://api.example.com`)
   - Magic
2. Deploy the database (keep the connection string handy)
   - It is up to you how you want to deploy this. If you don't want to do this separately and would rather use Docker, simply uncomment the `db` service in the `docker-compose.yaml` file.
3. Create your appsettings.json file
   - Place the file somewhere that can be mounted by the container
   - Ensure you swap out the connection string with your one
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "MainDb": "Server=db;Port=5432;Database=GroupAllocator;User Id=user;Password=password1234"
  },
  "AdminEmails": [
    "email@uts.edu.au",
    "anotheremail@uts.edu.au"
  ]
}
```
4. Deploy the `docker-compose.yaml` file
    - If you're not using Docker, just generally follow the structure of the file for your own deployment.
    - Ensure you mount the correct appsettings.json file to the container, and uncomment the `db` lines if required.
  
## Fullstack Setup (Testing)

1. Install Docker Desktop ([https://www.docker.com/products/docker-desktop/](https://www.docker.com/products/docker-desktop/))
2. Run `docker compose up --build --force-recreate` (Ctrl+C to stop)

## Frontend Setup (Development)

1. Install Node.js ([https://volta.sh/](https://volta.sh/))
2. Install PNPM (`npm i -g pnpm`)
3. Install dependencies (`pnpm i`)
4. Run `pnpm dev`

## Backend Setup (Development)

### Database

1. Install docker & docker-compose
1. Run `docker-compose -f database.yml up -d`

```**mermaid**
erDiagram
    Client {
        int Id PK
        string Name
        int MinProjects
        int MaxProjects
    }

    File {
        int Id PK
        int UserId FK
        byte[] Blob
        string Name
    }

    Preference {
        int Id PK
        int StudentId FK
        int ProjectId FK
        double Strength
    }
    
    Project {
        int Id PK
        int ClientId FK
        string Name
        bool RequiresNda
        int MinStudents
        int MaxStudents
    }

    SolveRun {
        int Id PK
        DateTime Timestamp
        double Evaluation
    }

    StudentAssignment {
        int Id PK
        int StudentId FK
        int ProjectId FK
        int SolveRunId FK
    }

    Student {
        int Id PK
        int UserId FK
        bool WillSignContract
    }

    User {
        int Id PK
        bool IsAdmin
        string Name
        string Email
        bool IsVerified
    }

    Project }o--|| Client : has_many
    Preference }o--|| Student : has_many
    Preference }o--|| Project : has_many
    StudentAssignment }o--|| Student : has_many
    StudentAssignment }o--|| Project : has_many
    StudentAssignment }|--|| SolveRun : references
    Student |o--|| User : has_one
    User ||--o{ File : has_many
```

### Application - Windows

1. Install Visual Studio with ASP.NET workload and .NET 9.0 component
2. In the Group-allocator/backend/group-allocator directory, run the following commands:
   1. `dotnet tool restore` 
   2. `dotnet run`
1. Run `dotnet user-secrets init` then `dotnet user-secrets set "Stripe:SecretKey" "{key}"`

### Application - Mac/Linux

1. Install [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
1. In `backend/GroupAllocator.Backend` run `dotnet run`
1. Run `dotnet user-secrets init` then `dotnet user-secrets set "Stripe:SecretKey" "{key}"`
****