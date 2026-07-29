# 1. Project Overview

SchoolEye is a School ERP application.
The system is designed to manage school operations and contains modules such as:

- Student Admission
- Student Registration
- Student Management
- Teacher Management
- Staff Management
- Fee Management
- Fee Collection
- Transport Management
- Attendance Management
- Examination Management
- Result Management
- Reports
- Dashboard
- User Management
- Authentication
- Authorization

The existing project architecture must be understood before implementing new functionality.
Always inspect existing code and find similar modules before creating new code.
Do not introduce a new architecture or coding pattern when an existing project pattern can be reused.

---

# 2. Technology Stackgfdsds


## Backend

- C#
- ASP.NET Core
- REST API
- Dapper
- SQL Server
- Stored Procedures

## Frontend


- HTML
- CSS
- Bootstrap

## Database

- Microsoft SQL Server
- Tables
- Stored Procedures
- Views
- Functions
- Indexes

## Testing

- xUnit
- Moq
- Integration Testing where appropriate

---

# 3. Backend Architecture

Follow the existing backend architecture.

The preferred request flow is:

Controller
    ↓
Service
    ↓
Repository
    ↓
Dapper
    ↓
Stored Procedure
    ↓
SQL Server

Do not bypass existing application layers without a valid reason.

Before creating a new class:

1. Search for similar classes.
2. Understand the existing pattern.
3. Reuse existing interfaces where appropriate.
4. Follow existing naming conventions.
5. Follow existing dependency injection patterns.

---

# 4. Frontend Architecture

The preferred frontend request flow is:

Angular Component
    ↓
Angular Service
    ↓
HttpClient
    ↓
ASP.NET Core API
    ↓
Service
    ↓
Repository
    ↓
SQL Server

Follow the existing Angular project structure.

Before creating a new component:

1. Search for similar components.
2. Search for existing reusable components.
3. Search for existing Angular services.
4. Search for existing API integration patterns.
5. Reuse existing UI patterns.

Do not introduce a new UI library without approval.

---

# 5. General Development Rules

1. Analyze the existing project before making changes.

2. Find similar existing functionality before creating new functionality.

3. Reuse existing patterns.

4. Do not create duplicate functionality.

5. Do not modify unrelated files.

6. Preserve existing functionality unless the requirement explicitly changes it.

7. Use meaningful and consistent names.

8. Use async/await for asynchronous operations.

9. Follow existing API response patterns.

10. Follow existing exception handling patterns.

11. Validate user input.

12. Do not hardcode secrets.

13. Do not expose passwords, API keys, tokens, or sensitive information.

14. Build the project after major changes.

15. Run appropriate tests after implementation.

16. Keep changes focused on the requested feature.

17. Do not delete existing functionality without explicit approval.

---

# 6. Database Rules

Database changes must follow these principles:

1. Preserve existing data.

2. Do not perform destructive database changes without explicit approval.

3. Do not drop tables without explicit approval.

4. Do not drop columns without explicit approval.

5. Provide migration scripts for schema changes when appropriate.

6. Provide rollback guidance for important database changes.

7. Use transactions for multi-step operations that must succeed or fail together.

8. Use parameterized queries.

9. Avoid SELECT *.

10. Add indexes only when justified.

11. Review existing indexes before adding new indexes.

12. Follow existing database naming conventions.

13. Do not change existing stored procedure behavior without checking its callers.

14. Check dependencies before modifying database objects.

---

# 7. API Rules

All APIs should:

1. Follow REST conventions.

2. Use appropriate HTTP status codes.

3. Validate input.

4. Use consistent response models.

5. Use async methods where appropriate.

6. Handle exceptions consistently.

7. Do not expose internal database errors to API clients.

8. Follow the existing ApiResponse pattern.

9. Follow existing pagination patterns.

10. Follow existing filtering and sorting patterns.

11. Use Swagger/OpenAPI conventions already established in the project.

12. Protect sensitive endpoints with appropriate authentication and authorization.

---

# 8. Security Rules

Always check for:

- SQL Injection
- Broken Authentication
- Broken Authorization
- Hardcoded Secrets
- Sensitive Data Exposure
- Missing Input Validation
- Insecure API Endpoints
- Improper Error Handling
- Excessive Data Exposure

Never commit:

- Passwords
- Database credentials
- API keys
- JWT secrets
- Private tokens
- Connection strings containing credentials

Use configuration and secure secret management where appropriate.

---

# 9. Feature Development Workflow

For every new feature:

## Step 1 — Understand

Understand the user's requirement.

## Step 2 — Analyze

Analyze the existing project.

Find:

- Similar modules
- Existing database tables
- Existing stored procedures
- Existing APIs
- Existing frontend components
- Existing services
- Existing tests

## Step 3 — Plan

Create an implementation plan.

The plan should identify:

- Database changes
- Backend changes
- Frontend changes
- Testing changes
- Security considerations

## Step 4 — Approval

For large or multi-layer changes, present the plan before implementation.

Wait for user approval.

## Step 5 — Database

Implement required database changes.

## Step 6 — Backend

Implement:

- Models
- DTOs
- Interfaces
- Repositories
- Services
- Controllers
- APIs

Follow the existing architecture.

## Step 7 — Frontend

Implement:

- Components
- Services
- Models
- Forms
- Validation
- API integration

## Step 8 — Testing

Create or update:

- Unit tests
- Integration tests
- API tests
- Validation tests

## Step 9 — Build

Build the affected projects.

## Step 10 — Test

Run the relevant test suites.

## Step 11 — Review

Perform a code review for:

- Architecture
- Security
- Performance
- Maintainability
- Code quality

## Step 12 — Summary

Provide a final summary containing:

- Files created
- Files modified
- Database changes
- API endpoints
- Frontend changes
- Tests created
- Build result
- Test result
- Potential risks

---

# 10. Agent Responsibilities

## Database Agent

Use the Database Agent for:

- SQL Server
- Tables
- Columns
- Relationships
- Stored Procedures
- Views
- Functions
- Indexes
- Query optimization
- Database performance

---

## Backend Agent

Use the Backend Agent for:

- C#
- ASP.NET Core
- Models
- DTOs
- Dapper
- Repositories
- Services
- Controllers
- REST APIs
- Dependency Injection
- Exception Handling

---

## Frontend Agent

Use the Frontend Agent for:

- Angular
- TypeScript
- HTML
- CSS
- Bootstrap
- Components
- Angular Services
- Forms
- Validation
- API Integration

---

## Testing Agent

Use the Testing Agent for:

- Unit Tests
- Integration Tests
- API Tests
- Validation Tests
- Regression Tests

---

## Code Review Agent

Use the Code Review Agent for:

- Architecture Review
- Security Review
- Performance Review
- SQL Review
- API Review
- Code Quality
- SOLID Principles
- Maintainability

---

# 11. Main Agent Behavior

The Main Agent is responsible for understanding the complete SchoolEye project.

The Main Agent should coordinate database, backend, frontend, testing, and code review work.

When a task requires specialized knowledge, use the appropriate specialized agent.

For example:

Student Admission Feature
    ↓
Database Agent
    ↓
Backend Agent
    ↓
Frontend Agent
    ↓
Testing Agent
    ↓
Code Review Agent

The Main Agent should maintain the overall architecture and ensure that all changes work together.

---

# 12. Important Rules for Large Features

Before implementing a large feature:

1. Analyze the existing project.
2. Identify affected modules.
3. Identify affected database objects.
4. Identify affected APIs.
5. Identify affected frontend components.
6. Identify required tests.
7. Create an implementation plan.
8. Present the plan to the user.
9. Wait for approval.
10. Implement in small, verifiable steps.

Do not make large unrelated changes.

Do not rewrite the entire project unless explicitly requested.

---

# 13. Definition of Done

A feature is considered complete only when:

- Database changes are complete.
- Backend changes are complete.
- Frontend changes are complete.
- Validation is implemented.
- Security has been considered.
- Appropriate tests are created.
- The project builds successfully.
- Relevant tests pass.
- Code review is complete.
- No unrelated files are modified.
- A final change summary is provided.



