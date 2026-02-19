# TASK-001: Implement Multi-User Support with Email-Based Authentication

## Epic
User Management & Authentication

## Type
Feature / Enhancement

## Priority
High

## Story Points
13

---

## Description

Currently, the Yanitor application uses a single hardcoded development user ("Anders") created via `EfHouseConfigurationService.EnsureDefaultUserAsync()`. The system assumes one logical house per developer and all services operate under this single-user assumption.

This task will enable multiple users to access the system by entering their email address, with each user having their own isolated user profile, house configuration, selected items, and active tasks.

**Note:** The existing "Anders" default user behavior can be completely removed once email-based sign-in is implemented. There is no need to preserve or migrate this dev-only data.

---

## Acceptance Criteria

1. ✅ Users can sign in using only their email address (no password initially)
2. ✅ New users are automatically created on first sign-in
3. ✅ Each user has their own isolated:
   - User profile (User entity)
   - House configuration (House entity)
   - Selected item types (SelectedItemType entities)
   - Active tasks (ActiveTaskRow entities)
4. ✅ Users can manage multiple houses (future-proof architecture)
5. ✅ All existing services respect user isolation
6. ✅ No data leakage between users
7. ✅ Culture/localization preferences persist per user session
8. ✅ `EnsureDefaultUserAsync` pattern is completely removed from codebase

---

## Technical Context

### Current Architecture Analysis

**Current User Entity (`YanitorDbContext.cs`):**
```csharp
public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;  // Currently only "Anders"
    public Guid? HouseId { get; set; }
    public House? House { get; set; }
}
```
- **Missing:** Email property (needed for authentication)
- **Missing:** Timestamps (CreatedAt, LastLoginAt)
- **Issue:** `Name` has unique index but we need Email to be unique
- **Relationship:** One-to-one with House (needs to support one-to-many for multiple houses)

**Current House Entity:**
```csharp
public class House
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OwnerId { get; set; }
    public User? Owner { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<SelectedItemType> SelectedItemTypes { get; set; } = new List<SelectedItemType>();
}
```
- **Good:** Already has `OwnerId` foreign key
- **Issue:** Current one-to-one relationship with User limits one house per user

**Service Dependencies:**
- `EfHouseConfigurationService`: Calls `EnsureDefaultUserAsync()` in 4 methods
- `ActiveTaskService`: Calls `EnsureDefaultUserAsync()` in 6 methods
- All services assume single user context

**Current Route Structure:**
- Pages support optional `/{culture}` prefix
- Culture preference persists via cookie
- No authentication/authorization in place

---

## Subtasks

### 1. Design & Planning
**Estimate: 2 points**

- [ ] **Review data model changes needed**
  - User: Add Email (unique), CreatedAt, LastLoginAt, remove unique index on Name
  - User-House relationship: Change to one-to-many
  - Add DisplayName property (optional, defaults to email prefix)
  
- [ ] **Design authentication flow**
  - Decision: Simple email entry (no magic link for MVP)
  - User enters email → system creates/retrieves user → sets context → redirects to /my-house
  - Email validation: trim whitespace from start and end, basic format check, normalize to lowercase
  
- [ ] **Define user context strategy**
  - Use Blazor Server circuit-scoped service
  - Store current user ID and email in scoped service
  - Lazy-load user entity when needed
  
- [ ] **Security considerations**
  - Email validation and sanitization
  - SQL injection prevention (EF Core parameterization)
  - No PII logging (avoid logging full emails)
  - Future: Add email verification, rate limiting

**Deliverables:**
- This section serves as the ADR
- Updated entity model documented below in Subtask 2

---

### 2. Update Data Model & Migrations
**Estimate: 3 points**

#### 2.1 Update User Entity
- [ ] Add `Email` property (string, required, max 320 chars - RFC 5321)
- [ ] Add `DisplayName` property (string, optional, max 200 chars)
- [ ] Add `CreatedAt` property (DateTime, required)
- [ ] Add `LastLoginAt` property (DateTime, nullable)
- [ ] Add `EmailVerified` property (bool, default false - for future use)
- [ ] Remove unique index from `Name` property
- [ ] Add unique index on `Email` property
- [ ] Consider making `Name` nullable or remove it (Email can serve as identifier)

**Proposed User Entity:**
```csharp
public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    public bool EmailVerified { get; set; } = false;
    
    // Navigation
    public ICollection<House> Houses { get; set; } = new List<House>();
}
```

#### 2.2 Update User-House Relationship
- [ ] Change User-House relationship from one-to-one to one-to-many
- [ ] Update `House` entity to have `User? Owner` navigation (already has `OwnerId`)
- [ ] Update EF configuration to remove `HasOne(x => x.House)` from User
- [ ] Add `HasMany(x => x.Houses)` on User entity

**Updated DbContext Configuration:**
```csharp
modelBuilder.Entity<User>(b =>
{
    b.HasKey(x => x.Id);
    b.HasIndex(x => x.Email).IsUnique();
    b.Property(x => x.Email).HasMaxLength(320).IsRequired();
    b.Property(x => x.DisplayName).HasMaxLength(200);
    b.Property(x => x.CreatedAt).IsRequired();
    b.HasMany(x => x.Houses)
        .WithOne(x => x.Owner)
        .HasForeignKey(x => x.OwnerId)
        .OnDelete(DeleteBehavior.Cascade);
});
```

#### 2.3 Create Migration
- [ ] Remove `HouseId` property from User entity (no longer needed)
- [ ] Create migration: `dotnet ef migrations add AddEmailAuthToUser`
- [ ] **Data migration:** No need to preserve "Anders" data - can delete database or let migration create fresh schema
- [ ] Test migration on clean database
- [ ] Document breaking change: existing databases need to be recreated

**Files to modify:**
- `src/Yanitor.Web/Data/YanitorDbContext.cs`
- New migration file in `src/Yanitor.Web/Migrations/`

**Acceptance:**
- Migration runs successfully on clean database
- Email column has unique constraint and proper length
- User can have multiple houses (relationship is one-to-many)
- Old "Anders" user pattern is incompatible (which is fine - we're removing it)

---

### 3. Implement User Context Service
**Estimate: 3 points**

#### 3.1 Create IUserContext Interface
- [ ] Define interface with methods:
  - `Task<Guid?> GetCurrentUserIdAsync()` - Returns current user ID or null
  - `Task<string?> GetCurrentUserEmailAsync()` - Returns current email or null
  - `Task<User?> GetCurrentUserAsync()` - Loads full user entity
  - `Task SetCurrentUserAsync(Guid userId, string email)` - Sets authenticated user
  - `Task ClearCurrentUserAsync()` - Clears authentication (sign out)

#### 3.2 Implement UserContext Service
- [ ] Create scoped service that stores user ID and email in memory
- [ ] Use `YanitorDbContext` to load full user entity when needed
- [ ] Thread-safe implementation (though Blazor circuits are single-threaded)
- [ ] Consider using `AsyncLocal<T>` or simple fields (since scoped to circuit)

**Implementation Pattern:**
```csharp
public class UserContext : IUserContext
{
    private Guid? _userId;
    private string? _email;
    private readonly YanitorDbContext _db;
    
    public UserContext(YanitorDbContext db) => _db = db;
    
    public Task<Guid?> GetCurrentUserIdAsync() 
        => Task.FromResult(_userId);
    
    public Task<string?> GetCurrentUserEmailAsync() 
        => Task.FromResult(_email);
    
    public async Task<User?> GetCurrentUserAsync()
    {
        if (_userId == null) return null;
        return await _db.Users
            .Include(u => u.Houses)
            .FirstOrDefaultAsync(u => u.Id == _userId.Value);
    }
    
    public Task SetCurrentUserAsync(Guid userId, string email)
    {
        _userId = userId;
        _email = email;
        return Task.CompletedTask;
    }
    
    public Task ClearCurrentUserAsync()
    {
        _userId = null;
        _email = null;
        return Task.CompletedTask;
    }
}
```

#### 3.3 Register Service
- [ ] Register as `Scoped` in `Program.cs` (tied to Blazor circuit lifetime)
- [ ] Add before other domain services that depend on it

**Files to create:**
- `src/Yanitor.Web/Services/IUserContext.cs`
- `src/Yanitor.Web/Services/UserContext.cs`

**Files to modify:**
- `src/Yanitor.Web/Program.cs`

**Acceptance:**
- Service is scoped to circuit/session
- Context persists across component navigations within same circuit
- Context is cleared when circuit ends
- Multiple concurrent users have isolated contexts

---

### 4. Implement Email-Based Sign-In
**Estimate: 5 points**

#### 4.1 Create Sign-In Page Component
- [ ] Create `SignIn.razor` with routes: `@page "/"` and `@page "/{culture}"`
- [ ] Move existing home page to `@page "/home"` and `@page "/{culture}/home"`
- [ ] Email input with `<input type="email" required />`
- [ ] Client-side validation: email format, not empty, max 320 chars
- [ ] Loading state during sign-in (disable button, show spinner)
- [ ] Error handling: display user-friendly messages
- [ ] Auto-focus email input on load
- [ ] Enter key triggers sign-in

**UI Requirements:**
- Follow existing design system (gradient background, centered card)
- BEM class naming: `.sign-in`, `.sign-in__form`, `.sign-in__input`
- Responsive: mobile-first design
- Accessibility: labels, ARIA attributes, focus management

**Localization Keys:**
```
PageTitle: "Sign In"
EmailLabel: "Email Address"
EmailPlaceholder: "Enter your email"
SignInButton: "Sign In"
SigningIn: "Signing in..."
InvalidEmail: "Please enter a valid email address"
ErrorMessage: "An error occurred. Please try again."
```

**Files to create:**
- `src/Yanitor.Web/Components/Pages/SignIn.razor`
- `src/Yanitor.Web/Components/Pages/SignIn.razor.css`
- `src/Yanitor.Web/Resources/Components.Pages.SignIn.resx`
- `src/Yanitor.Web/Resources/Components.Pages.SignIn.no.resx`

**Files to modify:**
- `src/Yanitor.Web/Components/Pages/Home.razor` (update routes)

#### 4.2 Create Authentication Service
- [ ] Create `IAuthenticationService` interface
- [ ] Implement `AuthenticationService` with dependency injection:
  - `YanitorDbContext` - for user database operations
  - `IUserContext` - to set authenticated user
  - `ILogger<AuthenticationService>` - for logging (no PII)

**Methods:**
```csharp
Task<SignInResult> SignInWithEmailAsync(string email);
Task SignOutAsync();
Task<User?> GetCurrentUserAsync();
```

**SignInResult:**
```csharp
public record SignInResult(bool Success, string? ErrorMessage, User? User);
```

**Implementation Details:**
- [ ] Normalize email to lowercase before querying/saving
- [ ] Trim whitespace from email
- [ ] Validate email format (use `MailAddress` or regex)
- [ ] If user exists: retrieve, update `LastLoginAt`, save
- [ ] If user doesn't exist: create new user with Email, CreatedAt, LastLoginAt
- [ ] Set user context on successful sign-in
- [ ] Log sign-in events (user ID only, not email)
- [ ] Create default house for new user (or defer to first access)

**Error Handling:**
- Catch `DbUpdateException` for unique constraint violations
- Return user-friendly error messages
- Log detailed errors for debugging

**Files to create:**
- `src/Yanitor.Web/Services/IAuthenticationService.cs`
- `src/Yanitor.Web/Services/AuthenticationService.cs`

**Files to modify:**
- `src/Yanitor.Web/Program.cs` (register as scoped service)

#### 4.3 Wire Up Sign-In Flow
- [ ] Inject `IAuthenticationService` into `SignIn.razor`
- [ ] Call `SignInWithEmailAsync` on button click
- [ ] Handle success: redirect to `/my-house` (or `/{culture}/my-house`)
- [ ] Handle failure: display error message
- [ ] Preserve culture in redirect URL

**Acceptance:**
- New users can sign in with email and are automatically created
- Existing users can sign in and their `LastLoginAt` is updated
- Invalid email formats are rejected with clear error message
- User context is set after successful sign-in
- User is redirected to their house page
- UI follows existing design patterns (BEM, scoped CSS, localization)
- Handles slow database operations gracefully (loading states)

---

### 5. Implement Authorization Guards
**Estimate: 2 points**

#### 5.1 Create Auth Guard Component (Optional Approach)
- [ ] Create `RequireAuth.razor` component that wraps protected content
- [ ] Check `IUserContext.GetCurrentUserIdAsync()` in `OnInitializedAsync`
- [ ] If user is null, redirect to sign-in page with return URL
- [ ] Render child content if authenticated

**Alternative: Code-Behind Pattern**
- [ ] Add auth check in each protected page's `OnInitializedAsync`
- [ ] Inject `IUserContext` and `NavigationManager`
- [ ] Redirect to sign-in if not authenticated

**Recommended: Hybrid Approach**
- Use code-behind in each page for explicit control
- Create helper method to reduce duplication

#### 5.2 Update Protected Pages
- [ ] `MyHouse.razor`: Add auth check at start of `OnInitializedAsync`
- [ ] `HouseBuilder.razor`: Add auth check
- [ ] `ActiveTasks.razor`: Add auth check
- [ ] `Components.razor` (Items): Add auth check
- [ ] Preserve culture parameter when redirecting to sign-in

**Auth Check Pattern:**
```csharp
protected override async Task OnInitializedAsync()
{
    var userId = await UserContext.GetCurrentUserIdAsync();
    if (userId == null)
    {
        var signInUrl = string.IsNullOrEmpty(Culture) 
            ? "/" 
            : $"/{Culture}";
        Navigation.NavigateTo(signInUrl);
        return;
    }
    
    // ... rest of initialization
}
```

#### 5.3 Add Sign-Out Button
- [ ] Add sign-out button to `MainLayout.razor` header
- [ ] Inject `IAuthenticationService`
- [ ] Call `SignOutAsync()` on click
- [ ] Redirect to sign-in page after sign-out
- [ ] Show user email in header when signed in

**Files to modify:**
- `src/Yanitor.Web/Components/Pages/MyHouse.razor`
- `src/Yanitor.Web/Components/Pages/HouseBuilder.razor`
- `src/Yanitor.Web/Components/Pages/ActiveTasks.razor`
- `src/Yanitor.Web/Components/Pages/Components.razor`
- `src/Yanitor.Web/Components/Layout/MainLayout.razor`

**Files to create (optional):**
- `src/Yanitor.Web/Components/Shared/RequireAuth.razor`

**Acceptance:**
- All protected pages redirect to sign-in when user is not authenticated
- Sign-out button clears context and redirects to sign-in
- No protected content is accessible without authentication
- Return URLs preserve culture parameter
- User email displayed in header when authenticated

---

### 6. Update Domain Services for User Isolation
**Estimate: 5 points**

#### 6.1 Remove Default User Pattern
- [ ] **Delete** `EnsureDefaultUserAsync` method from `EfHouseConfigurationService`
- [ ] Find all call sites and refactor to use `IUserContext`

**Call sites to update:**
- `GetConfigurationAsync()`
- `SaveConfigurationAsync()`
- `HasConfigurationAsync()`
- `ActiveTaskService.GetActiveTasksAsync()`
- `ActiveTaskService.GetTaskCountAsync()`
- `ActiveTaskService.SyncActiveTasksAsync()`
- `ActiveTaskService.CompleteTaskAsync()` (if it calls the method)

#### 6.2 Refactor EfHouseConfigurationService
- [ ] Inject `IUserContext` in constructor
- [ ] Create helper method: `GetCurrentUserHouseAsync()` 
  - Gets user from context
  - Throws exception if no user
  - Gets or creates user's first house
  - Returns house with included SelectedItemTypes

**New Helper Pattern:**
```csharp
private async Task<House> GetCurrentUserHouseAsync()
{
    var userId = await _userContext.GetCurrentUserIdAsync();
    if (userId == null)
        throw new InvalidOperationException("No authenticated user");
    
    var house = await _db.Houses
        .Include(h => h.SelectedItemTypes)
        .FirstOrDefaultAsync(h => h.OwnerId == userId.Value);
    
    if (house == null)
    {
        house = new House { OwnerId = userId.Value };
        _db.Houses.Add(house);
        await _db.SaveChangesAsync();
        _db.Entry(house).Collection(h => h.SelectedItemTypes).Load();
    }
    
    return house;
}
```

- [ ] Update `GetConfigurationAsync()` to use helper
- [ ] Update `SaveConfigurationAsync()` to use helper
- [ ] Update `HasConfigurationAsync()` to use helper
- [ ] Remove `EnsureDefaultUserAsync` method entirely

**Files to modify:**
- `src/Yanitor.Web/Domain/Services/EfHouseConfigurationService.cs`

#### 6.3 Refactor ActiveTaskService
- [ ] Inject `IUserContext` in constructor
- [ ] Create similar helper: `GetCurrentUserHouseIdAsync()`
- [ ] Replace all `EnsureDefaultUserAsync()` calls with context-based lookups
- [ ] Ensure all queries filter by current user's house

**Helper Pattern:**
```csharp
private async Task<Guid> GetCurrentUserHouseIdAsync()
{
    var userId = await _userContext.GetCurrentUserIdAsync();
    if (userId == null)
        throw new InvalidOperationException("No authenticated user");
    
    var house = await _db.Houses
        .Where(h => h.OwnerId == userId.Value)
        .Select(h => new { h.Id })
        .FirstOrDefaultAsync();
    
    if (house == null)
        throw new InvalidOperationException("User has no house configured");
    
    return house.Id;
}
```

- [ ] Update all methods in `ActiveTaskService`
- [ ] Verify no cross-user data access is possible

**Files to modify:**
- `src/Yanitor.Web/Domain/Services/ActiveTaskService.cs`

#### 6.4 Review ItemProvider
- [ ] Check if `ItemProvider` needs user context (likely doesn't - it provides catalog data)
- [ ] Ensure `GetItemsForCurrentConfigurationAsync` delegates to `IHouseConfigurationService` which now uses context
- [ ] No changes needed if it only uses `IHouseConfigurationService`

**Files to modify (if needed):**
- `src/Yanitor.Web/Domain/Services/ItemProvider.cs`

**Acceptance:**
- `EnsureDefaultUserAsync` is completely removed from codebase
- All services retrieve user from `IUserContext`
- All database queries include user/house filters
- No service allows cross-user data access
- Proper exception handling for missing user context
- No hardcoded "Anders" references remain

---

### 7. Add User Profile Management (Optional - Future Enhancement)
**Estimate: 3 points**

- [ ] Create user profile page showing:
  - Email (read-only)
  - Display name (editable)
  - Account created date
  - Last login date
  - Number of houses
  - Number of active tasks
- [ ] Add ability to update display name
- [ ] Add navigation link in MainLayout
- [ ] Future: manage multiple houses, delete account

**Files to create:**
- `src/Yanitor.Web/Components/Pages/Profile.razor`
- `src/Yanitor.Web/Components/Pages/Profile.razor.css`
- Localization resources

**Note:** This subtask is optional and can be deferred to a future sprint.

**Acceptance:**
- User can view their profile information
- Display name can be updated
- UI is consistent with existing design system

---

### 8. Testing & Validation
**Estimate: 3 points**

#### 8.1 Unit Tests
- [ ] Create test project structure (if not exists)
- [ ] Test `AuthenticationService`:
  - New user creation
  - Existing user retrieval
  - Email normalization
  - Invalid email handling
- [ ] Test `UserContext`:
  - Set and retrieve user
  - Clear user
  - Load full user entity
- [ ] Test service isolation (mock `IUserContext` in service tests)

#### 8.2 Integration Tests
- [ ] Test complete sign-in flow:
  - User A signs in → creates house → adds tasks
  - User B signs in → should see empty house
  - User A signs back in → should see their data
- [ ] Test sign-out flow
- [ ] Test culture preservation across sign-in/out
- [ ] Test concurrent users (multiple browser tabs)

#### 8.3 Database Migration Testing
- [ ] Test on clean database (fresh start)
- [ ] Verify indexes are created correctly
- [ ] Test cascade delete behavior (user deletion removes houses and tasks)

#### 8.4 Manual Testing Checklist
- [ ] Sign in with new email
- [ ] Build house configuration
- [ ] Create tasks
- [ ] Sign out
- [ ] Sign in again - verify data persists
- [ ] Sign in with different email - verify data isolation
- [ ] Test on mobile browser
- [ ] Test culture switching
- [ ] Test slow network (loading states)

**Files to create/modify:**
- Test project (if not exists): `tests/Yanitor.Tests/Yanitor.Tests.csproj`
- `tests/Yanitor.Tests/Services/AuthenticationServiceTests.cs`
- `tests/Yanitor.Tests/Services/UserContextTests.cs`
- `tests/Yanitor.Tests/Integration/MultiUserFlowTests.cs`

**Acceptance:**
- All unit tests pass
- Integration tests confirm user isolation
- No data leakage between users
- No regression in existing functionality
- Manual testing passes on desktop and mobile

---

### 9. Documentation & Deployment
**Estimate: 2 points**

#### 9.1 Update README
- [ ] Document new sign-in flow
- [ ] Update screenshots (if any)
- [ ] Document database schema changes
- [ ] Add migration instructions

#### 9.2 Architecture Documentation
- [ ] Document authentication flow (diagram)
- [ ] Document user context pattern
- [ ] Document data isolation strategy
- [ ] Update entity relationship diagram

#### 9.3 Update Copilot Instructions
- [ ] Update Section 9 (Architecture) to reflect multi-user support
- [ ] Document IUserContext usage pattern
- [ ] Remove references to default user
- [ ] Add notes about user isolation requirements

#### 9.4 User Guide
- [ ] Create getting started guide
- [ ] Explain email-based sign-in
- [ ] Explain data privacy (each user has isolated data)

#### 9.5 Deployment Guide
- [ ] Database migration instructions
- [ ] Breaking change notice (existing databases must be recreated)
- [ ] Rollback procedure

**Files to create/modify:**
- `README.md`
- `docs/architecture/authentication.md` (new)
- `docs/architecture/user-context-pattern.md` (new)
- `docs/user-guide/getting-started.md` (new)
- `docs/deployment/migration-guide.md` (new)
- `.github/copilot-instructions.md`

**Acceptance:**
- Documentation is comprehensive and up-to-date
- New developers can understand the auth flow
- Deployment guide is clear and actionable
- Breaking changes are clearly communicated

---

## Dependencies

- None (this is a foundational feature)

## Blocked By

- None

## Blocks

- Future tasks: Role-based permissions, email verification, password authentication, OAuth integration, house sharing

---

## Technical Decisions

### Authentication Approach
**Decision:** Email-only sign-in (no password, no magic link for MVP)

**Rationale:**
- Simplest implementation for MVP
- No password management complexity
- No email sending infrastructure needed
- Fastest user onboarding
- Suitable for trusted/internal user base
- Foundation for future enhancements

**Future Migration Path:**
1. Add email verification (send confirmation code)
2. Add magic link authentication (passwordless email link)
3. Add optional password authentication
4. Integrate OAuth providers (Google, Microsoft)

### User Context Management
**Decision:** Use Blazor Server circuit-scoped service

**Rationale:**
- Blazor Server maintains long-lived circuit per user session
- Scoped services align perfectly with circuit lifetime
- No cookies/JWT needed for Blazor Server (circuit state is server-side)
- Simple implementation with in-memory storage
- Works seamlessly with Blazor's connection model

**Trade-offs:**
- Doesn't work across browser tabs (each tab = new circuit)
- Circuit state lost on server restart (user must sign in again)
- Not suitable for Blazor WebAssembly (would need different approach)

### Data Isolation Strategy
**Decision:** Filter all queries by `OwnerId`/`HouseId` at service layer

**Rationale:**
- Clear separation of concerns (services enforce authorization)
- Centralized security logic (easier to audit)
- EF Core makes filtering straightforward with LINQ
- Performance acceptable with proper indexes
- Prevents accidental data leakage from UI layer

**Alternative Rejected:** Global query filters in DbContext
- Less explicit (harder to see where filtering happens)
- Can be bypassed with `IgnoreQueryFilters()`
- Better suited for soft-delete scenarios

### User-House Relationship
**Decision:** One user can have multiple houses (one-to-many)

**Rationale:**
- Future-proof architecture
- Enables family/shared living scenarios
- Common pattern in similar applications
- Low implementation cost (just change relationship)
- For MVP, users still only interact with first house

### Remove Default User Pattern
**Decision:** Completely remove `EnsureDefaultUserAsync` and "Anders" user

**Rationale:**
- Simplifies codebase (no legacy code to maintain)
- Forces proper user context usage
- Email-based auth is superior for multi-user
- Dev workflow: just sign in with any email
- No migration burden (data can be recreated easily)

---

## Risks & Mitigations

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Data leakage between users | Medium | High | Comprehensive testing, code review, all queries filtered by user ID, integration tests with multiple users |
| Email validation bypass | Low | Medium | Server-side validation, use `MailAddress` class, sanitize input, add future email verification |
| Circuit/session management issues | Medium | Medium | Thorough testing, proper service lifetimes, handle circuit disconnects gracefully |
| Performance with user filtering | Low | Low | Add indexes on `OwnerId` and `HouseId` (already planned), monitor query performance |
| Breaking existing dev workflow | Low | Low | Simple fix: sign in with any email (e.g., "dev@test.com"), one-time setup |
| Multiple tabs/circuits per user | Medium | Low | Document limitation, consider future session sharing if needed |
| Database migration failures | Low | Medium | Test thoroughly on clean DB, provide rollback script, document breaking changes |

---

## Definition of Done

- [ ] All subtasks completed and checked off
- [ ] All acceptance criteria met and verified
- [ ] Unit tests written and passing
- [ ] Integration tests passing
- [ ] Manual testing completed across multiple browsers (Chrome, Firefox, Edge, Safari on iOS)
- [ ] Mobile responsive testing completed (iOS and Android)
- [ ] Code reviewed and approved by peer
- [ ] Documentation updated and reviewed
- [ ] Database migration tested on dev/staging environments
- [ ] No regressions in existing functionality
- [ ] Accessibility validated (keyboard navigation, screen reader, ARIA)
- [ ] Localization complete (en, no) for all new UI strings
- [ ] Performance validated (no significant degradation, queries optimized)
- [ ] Security reviewed (no data leakage, input validation, proper error handling)
- [ ] Deployed to staging environment and smoke tested
- [ ] Product owner acceptance obtained
- [ ] `EnsureDefaultUserAsync` pattern completely removed from codebase (verified via code search)

---

## Notes

### Implementation Notes
- No need to preserve "Anders" data - can recreate database from scratch
- Email stored in lowercase for consistency
- DisplayName optional (defaults to email prefix if not set)
- First house is created automatically when user first accesses /my-house
- Culture preference still uses cookie (not tied to user profile yet - future enhancement)
- Consider adding CreatedBy/UpdatedBy audit fields in future

### Security Notes
- Email validation: basic format check for MVP, add email verification in future
- No rate limiting yet - add in future to prevent abuse
- No CAPTCHA - consider for production deployment
- Logging: use user ID, never log full emails (PII concern)
- GDPR consideration: add ability to delete account and all data (future)

### Performance Notes
- Expected queries per request: 1-2 (user context + data query)
- Indexes needed: User.Email (unique), House.OwnerId, ActiveTaskRow.HouseId (already exist)
- Blazor circuit maintains single DbContext per scope - be mindful of change tracking overhead
- Consider query splitting if performance issues arise

### Future Enhancements
- Email verification with confirmation code
- Password authentication (optional)
- OAuth integration (Google, Microsoft, GitHub)
- Multiple house management UI
- House sharing/collaboration features
- User roles (admin, member, viewer)
- Audit logging (who did what when)
- Account deletion (GDPR compliance)
- Session management across browser tabs
- "Remember me" functionality
- Profile picture upload

---

## Related Documentation

- [Copilot Instructions](.github/copilot-instructions.md) - Section 9 (Yanitor Architecture)
- [EF Core Migration Workflow](.github/copilot-instructions.md) - Section 11
- Current DB Schema: `src/Yanitor.Web/Data/YanitorDbContext.cs`
- Blazor Server Authentication: https://learn.microsoft.com/en-us/aspnet/core/blazor/security/server/
- EF Core Relationships: https://learn.microsoft.com/en-us/ef/core/modeling/relationships

---

**Created:** 2025-01-XX  
**Updated:** 2025-01-XX  
**Assignee:** TBD  
**Epic Link:** User Management & Authentication  
**Sprint:** TBD  
**Labels:** authentication, multi-user, breaking-change, database-migration
