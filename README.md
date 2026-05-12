# Word Guess Game

Console word game (Wordle-style feedback) with **PostgreSQL** storage for users, words, and scores. Uses **ADO.NET** via [Npgsql](https://www.npgsql.org/).

## Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) (project targets **.NET 10**)
- [PostgreSQL](https://www.postgresql.org/download/) installed and running

## Database: create it first

The app connects to a database name from your connection string.

Pick a database name (the sample config uses `wordguess`). Create it using **either** method below.

### Option A: `createdb` (command line)

```bash
createdb -U postgres wordguess
```

### Option B: `psql` / pgAdmin

```sql
CREATE DATABASE wordguess;
```

## Configure the connection string

Edit `WordGuessGame/appsettings.json` so it matches your server:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=wordguess;Username=postgres;Password=YOUR_PASSWORD"
}
```

- `**Database=**` must be the database you created (e.g. `wordguess`).
- `**Username` / `Password**` must be a role that can create tables in that database (the owner of the database is fine).

You can override the connection string without editing the file by setting an environment variable:

```bash
export ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=wordguess;Username=postgres;Password=YOUR_PASSWORD"
```

On Windows (cmd):

```cmd
set ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=wordguess;Username=postgres;Password=YOUR_PASSWORD
```

## Run the app

From the `WordGuessGame` folder:

```bash
cd WordGuessGame
dotnet run
```

On **first successful connection**, the app will:

1. Create tables (`users`, `words`, `scores`) if they do not exist.
2. If `words` is **empty**, load valid 5-letter words from `words.txt` (copied next to the executable).

Then you get the menu: **Register**, **Login**, or **Exit**. You must **log in** before playing. Each completed game is saved to `scores`.

## Project layout (high level)

- `Program.cs`   configuration, DB init, auth loop, game loop, score persistence.
- `Data/`   ADO.NET repositories and schema initialization.
- `Services/`   validation, feedback, authentication (BCrypt passwords).
- `AuthConsoleFlow.cs`   register / login console UI.

