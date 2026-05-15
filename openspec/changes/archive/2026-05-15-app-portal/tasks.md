## 1. Project Scaffold & Configuration

- [x] 1.1 Create ASP.NET Core Razor Pages project via `dotnet new razor`
- [x] 1.2 Add LiteDB NuGet package dependency
- [x] 1.3 Initialize npm project and install tailwindcss + @tailwindcss/cli
- [x] 1.4 Create Tailwind CSS source file (`Styles/app.css`) with `@import "tailwindcss"` and `@source` directives
- [x] 1.5 Add MSBuild target to run Tailwind CLI on build
- [x] 1.6 Add LiteDB path configuration to `appsettings.json` with `AppPortal.db` fallback
- [x] 1.7 Add `package.json` scripts for dev watch and production build

## 2. Data Model & Storage Layer

- [x] 2.1 Create `AppEntry` and `AppUrl` model classes in `Models/` folder
- [x] 2.2 Create `IAppService` interface with all CRUD methods (async)
- [x] 2.3 Implement `AppService` with LiteDB operations (GetAll, GetEnabled, GetById, Create, Update, Delete, ToggleEnabled)
- [x] 2.4 Register `LiteDatabase` (singleton) and `IAppService` (scoped) in `Program.cs`

## 3. Shared Layout & Base UI

- [x] 3.1 Create `_Layout.cshtml` with Tailwind CSS link, responsive nav bar (门户标题 + 管理后台入口)
- [x] 3.2 Create `_ViewImports.cshtml` with common `@using` and `@inject` directives
- [x] 3.3 Create `_ViewStart.cshtml` with default layout
- [x] 3.4 Add base styles in Styles/app.css (theme colors, custom utilities via `@theme`)

## 4. Home Page (App Listing)

- [x] 4.1 Build `Index.cshtml` with responsive card grid (2-3-4 columns based on viewport)
- [x] 4.2 Create `Index.cshtml.cs` PageModel that fetches enabled apps via IAppService
- [x] 4.3 Render app card with name, description, icon (if set), and all URL links (target="_blank")
- [x] 4.4 Add empty state when no apps are enabled
- [x] 4.5 Add "管理后台" navigation link in page body

## 5. Admin List Page

- [x] 5.1 Build `Admin/Index.cshtml` with responsive table and "新增应用" button
- [x] 5.2 Create `Admin/Index.cshtml.cs` PageModel that fetches all apps
- [x] 5.3 Show app status badges (启用/禁用), action buttons (编辑, 删除, 切换)
- [x] 5.4 Implement ToggleEnabled POST handler (via form POST per row)

## 6. Admin Create Page

- [x] 6.1 Build `Admin/Create.cshtml` form with fields: Name, Description, IconUrl, SortOrder, dynamic URL rows
- [x] 6.2 Create `Admin/Create.cshtml.cs` with [BindProperty] and POST handler
- [x] 6.3 Add server-side validation (Name required, at least one URL)
- [x] 6.4 Add JavaScript for dynamic URL row add/remove with index management
- [x] 6.5 On success redirect to Admin/Index; on error redisplay with validation messages

## 7. Admin Edit Page

- [x] 7.1 Build `Admin/Edit.cshtml` with pre-filled form, reusing create form structure
- [x] 7.2 Create `Admin/Edit.cshtml.cs` with {id} route parameter, GET fetches existing data, POST updates
- [x] 7.3 Pre-populate dynamic URL rows from existing data
- [x] 7.4 Same validation and JS dynamic row logic as Create
- [x] 7.5 On success redirect to Admin/Index

## 8. Admin Delete Page

- [x] 8.1 Build `Admin/Delete.cshtml` with read-only app info summary and confirm/cancel buttons
- [x] 8.2 Create `Admin/Delete.cshtml.cs` with GET (show confirmation) and POST (confirm delete) handlers
- [x] 8.3 POST handler calls DeleteAsync and redirects to Admin/Index

## 9. Verification & Polish

- [x] 9.1 Run `dotnet build` and fix any compilation errors
- [x] 9.2 Verify all Razor Pages routes work correctly
- [x] 9.3 Verify Tailwind CSS builds and responsive layout renders properly
- [x] 9.4 Final LSP diagnostics check on all code files
