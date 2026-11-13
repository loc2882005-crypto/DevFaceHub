# DevFaceHub

A mini social network built with **ASP.NET Core MVC (.NET 9)**, **Entity Framework Core (Code-First)** and **SQL Server**.
Users can register, post with images, like, comment, share a post link, and browse profiles.

<!-- TODO: add 3–5 screenshots to docs/screenshots/ and reference them here, e.g.
![Feed](docs/screenshots/feed.png)
-->

## Features

- **Authentication & authorization** with ASP.NET Core Identity (register, login, logout)
- Safe return-URL handling after login (`Url.IsLocalUrl`)
- **Posts** with optional image upload, edit and delete (only the author can edit or delete their own posts)
- Server-side cleanup of image files when a post is edited or deleted
- **AJAX likes** (`fetch` + JSON) that update the UI without a page reload
- **Comments** on posts
- **Share** a post link via the Web Share API, with clipboard copy as a fallback
- **Profile pages** showing a user's posts and total likes received
- Responsive Facebook-style UI with Bootstrap 5, modals and live image preview

## Tech stack

| Layer | Technology |
|---|---|
| Backend | C#, ASP.NET Core MVC (.NET 9), ASP.NET Core Identity |
| Data | Entity Framework Core 9 (Code-First, migrations), SQL Server |
| Frontend | Razor views, Bootstrap 5, JavaScript (`fetch`) |

## Data model

`ApplicationUser` (extends `IdentityUser`) · `Post` · `Comment` · `Like`

Deleting a post cascades to its comments and likes. The relationships from `ApplicationUser` to `Post`,
`Comment` and `Like` use `DeleteBehavior.NoAction` to avoid SQL Server's *multiple cascade paths* error.

## Getting started

**Prerequisites:** [.NET 9 SDK](https://dotnet.microsoft.com/download), SQL Server (Express or LocalDB), and the EF Core tools (`dotnet tool install --global dotnet-ef`).

```bash
git clone https://github.com/<your-username>/DevFaceHub.git
cd DevFaceHub
```

1. Open `appsettings.json` and change the `DefaultConnection` server name to match your machine, for example:
   `Server=(localdb)\\MSSQLLocalDB;Database=DevFaceHubDb;Trusted_Connection=True;TrustServerCertificate=True`
2. Create the database:
   ```bash
   dotnet ef database update
   ```
3. Run the app:
   ```bash
   dotnet run
   ```
   Then open the URL printed in the console (see `Properties/launchSettings.json`).

## Project structure

```
Controllers/   Account, Home (feed, posts, likes, comments), Profile
Data/          ApplicationDbContext (Identity + social tables)
Models/        ApplicationUser, Post, Comment, Like
ViewModels/    Login, Register, PostCreate, Profile
Views/         Razor views
Migrations/    EF Core migrations
wwwroot/       static files (uploaded images go to wwwroot/images/posts)
```

## Roadmap / known limitations

- Validate uploaded files on the server (allowed types, size limit)
- Global antiforgery validation, including the AJAX like request
- Unique index on `(PostId, UserId)` for likes
- Pagination for the feed and profile pages
- Move business logic out of controllers into services; add unit tests

## Author

Tống Trần Quang Lộc: IT student at Hanoi University of Industry.
