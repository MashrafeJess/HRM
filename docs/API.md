# HRM API Documentation

This document describes every HTTP endpoint exposed by the HRM Web API, for use when scaffolding the Angular frontend. It covers request/response payload shapes, field types, and role restrictions as they currently exist in the backend.

## Conventions used in this doc

- **Base URL**: `https://<host>/api`
- **Auth**: The backend issues a JWT (see `POST /api/Auth/login`) containing claims `NameIdentifier` (employee id), `Email`, `Name` (first name), and `Role` (role name, free-text from the `Roles` table — there is no fixed enum of role names in this system; roles are created ad-hoc via `POST /api/Role/AddOrUpdateRole`).
- **⚠️ Current state**: `[Authorize]` is now enforced on every controller. Every endpoint below has a "🔒 Enforced" note stating the exact role(s) required — these are literal `Roles.RoleName` values, matching case-sensitively (`Super Admin`, `Company Admin`, `Common` are the ones currently used in code; see [Known backend quirks](#known-backend-quirks) for gaps). Send `Authorization: Bearer <accessToken>` on every call and handle `401` (missing/expired/invalid token) and `403` (valid token, wrong role).
- **Nullable fields**: a field marked `nullable` is optional in requests and may be `null` in responses. A field with no `nullable` marker is required in requests and always present in responses.
- **Pagination wrapper**: any endpoint returning `PagedResult<T>` responds with:
  ```json
  {
    "items": [ /* array of T */ ],
    "pageNumber": 1,
    "pageSize": 50,
    "totalCount": 237,
    "totalPages": 5
  }
  ```
  Paginated GET endpoints accept these query params: `viewOrder` (`"asc"` | `"desc"`, nullable, default `"desc"` or `"asc"` depending on endpoint — see each section), `pageNumber` (nullable, default `1`), `pageSize` (nullable, default `50`, max `100`).

---

## Table of contents

1. [Auth](#auth)
2. [Company](#company)
3. [Department](#department)
4. [Employee](#employee)
5. [Role](#role)
6. [Attendance](#attendance)
7. [Leave](#leave)
8. [Payroll](#payroll)
9. [AI Assistant](#ai-assistant)
10. [Known backend quirks](#known-backend-quirks)

---

## Auth

Base route: `api/Auth`

### POST `/api/Auth/login`

Roles allowed: public (unauthenticated).

**Request body** (`LoginDto`):

| Field | Type | Notes |
|---|---|---|
| `email` | `string` | required |
| `password` | `string` | required |

```json
{
  "email": "jane@company.com",
  "password": "hunter2"
}
```

**Response**: `200 OK`

| Field | Type | Notes |
|---|---|---|
| `accessToken` | `string` | JWT, short-lived (15 min) |

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs..."
}
```

A `refreshToken` is set as an **HttpOnly cookie** (not in the JSON body) — `Secure`, `SameSite=Strict`, 7-day expiry. The Angular app never reads this cookie directly; the browser sends it automatically on `/api/Auth/refresh` and `/api/Auth/logout` calls. Requests to those two endpoints must be made `withCredentials: true`.

---

### POST `/api/Auth/refresh`

Roles allowed: anyone holding a valid `refreshToken` cookie.

**Request**: no body. Requires the `refreshToken` cookie (send with `withCredentials: true`).

**Response**: `200 OK`, same shape as login (`{ "accessToken": "..." }`), and a rotated `refreshToken` cookie is set.

---

### POST `/api/Auth/logout`

Roles allowed: anyone holding a valid `refreshToken` cookie.

**Request**: no body. Requires the `refreshToken` cookie.

**Response**: `200 OK`, plain string body: `"Logged out."`. The `refreshToken` cookie is deleted.

---

## Company

Base route: `api/Company`

### POST `/api/Company/EditCompany`

🔒 **Enforced**: requires `Authorization: Bearer <accessToken>`. Roles allowed: `Super Admin`, `Company Admin`.

**Request body** (`CompanyDto`) — send `companyId: null` to create, or an existing id to update:

| Field | Type | Notes |
|---|---|---|
| `companyId` | `long?` | nullable — omit/null to create |
| `companyName` | `string` | required |
| `companyEmail` | `string` | required |
| `companyPhone` | `string` | required |
| `companyAddress` | `string?` | nullable |
| `logoUrl` | `string?` | nullable |
| `subscriptionPlan` | `string?` | nullable |
| `isActive` | `bool?` | nullable |
| `createdAt` | `DateTime?` | nullable — ignored by backend, do not send |
| `updatedAt` | `DateTime?` | nullable — ignored by backend, do not send |

```json
{
  "companyId": null,
  "companyName": "Khadlaj",
  "companyEmail": "k@k.k",
  "companyPhone": "1234",
  "companyAddress": "23456",
  "logoUrl": "https://example.com/logo.png",
  "subscriptionPlan": "Normal",
  "isActive": true,
  "createdAt": null,
  "updatedAt": null
}
```

**Response**: `200 OK`, `CompanyDto`. ⚠️ `companyId`, `createdAt`, `updatedAt` come back `null` even on successful create/update (backend doesn't populate them on this response — re-fetch via `GetCompanyById` or `GetAllCompany` if you need the generated id).

---

### GET `/api/Company/GetAllCompany`

🔒 **Enforced**: requires `Authorization: Bearer <accessToken>`. Roles allowed: `Super Admin` only.

**Query params** (`PageFilterDto`):

| Field | Type | Notes |
|---|---|---|
| `viewOrder` | `string?` | `"asc"` \| `"desc"`, nullable, default `"asc"` |
| `pageNumber` | `int?` | nullable, default `1` |
| `pageSize` | `int?` | nullable, default `50`, max `100` |

Example: `GET /api/Company/GetAllCompany?pageNumber=1&pageSize=100&viewOrder=desc`

**Response**: `200 OK`, `PagedResult<CompanyDto>` — each item has the full `CompanyDto` shape (all fields populated, unlike `EditCompany`'s response).

```json
{
  "items": [
    {
      "companyId": 1,
      "companyName": "Khadlaj",
      "companyEmail": "k@k.k",
      "companyPhone": "1234",
      "companyAddress": "23456",
      "logoUrl": "https://example.com/logo.png",
      "subscriptionPlan": "Normal",
      "isActive": true,
      "createdAt": "2026-01-10T09:00:00",
      "updatedAt": null
    }
  ],
  "pageNumber": 1,
  "pageSize": 50,
  "totalCount": 1,
  "totalPages": 1
}
```

---

### GET `/api/Company/GetCompanyById/{companyId}`

🔒 **Enforced**: requires `Authorization: Bearer <accessToken>`. Roles allowed: `Super Admin` only.

**Route param**: `companyId` (`long`, required).

**Response**: `200 OK`, `CompanyDto` (full shape, all fields populated). Throws if not found.

---

## Department

Base route: `api/Department`

### POST `/api/Department/EditDepartment`

🔒 **Enforced**: requires `Authorization: Bearer <accessToken>`. Roles allowed: `Company Admin` only.

**Request body** (`DepartmentDto`):

| Field | Type | Notes |
|---|---|---|
| `departmentId` | `long?` | nullable — omit/null to create |
| `companyId` | `long` | required |
| `departmentName` | `string` | required |
| `description` | `string?` | nullable |
| `isActive` | `bool` | required |
| `employeeCount` | `long?` | nullable — ignored by backend, do not send |
| `createdAt` | `DateTime?` | nullable — ignored by backend, do not send |
| `updatedAt` | `DateTime?` | nullable — ignored by backend, do not send |

**Response**: `200 OK`, `DepartmentDto`. ⚠️ Only `companyId`, `departmentName`, `description`, `isActive` come back populated — `departmentId`, `employeeCount`, timestamps are `null`/default on this response.

---

### GET `/api/Department/AllDepartmentsByCompanyId/{companyId}`

🔒 **Enforced**: requires `Authorization: Bearer <accessToken>`. Roles allowed: `Company Admin` only.

**Route param**: `companyId` (`long`, required).

**Query params** (`PageFilterDto`): same as Company's — `viewOrder` (nullable, default `"desc"`), `pageNumber` (nullable, default `1`), `pageSize` (nullable, default `50`, max `100`).

**Response**: `200 OK`, `PagedResult<DepartmentDto>`. Each item includes `employeeCount` (computed per department). ⚠️ Timestamps (`createdAt`/`updatedAt`) are still `null`/default on this endpoint — use `GetDepartmentById` if you need those.

---

### GET `/api/Department/GetDepartmentById{departmentId}`

⚠️ Note the missing `/` before the route parameter — the real path has no separator, e.g. `GET /api/Department/GetDepartmentById5` for id `5`. This is almost certainly an unintentional bug in the backend route template; confirm behavior against a running instance before wiring this up, since fixing it later will change the URL your frontend must call.

🔒 **Enforced**: requires `Authorization: Bearer <accessToken>`. Roles allowed: `Company Admin` only.

**Route param**: `departmentId` (`long`, required).

**Response**: `200 OK`, `DepartmentDto` (full shape, including `employeeCount`, `createdAt`, `updatedAt`). Throws `404`-equivalent if not found.

---

## Employee

Base route: `api/Employee`

### POST `/api/Employee/AddOrUpdateEmployee`

🔒 **Enforced**: requires `Authorization: Bearer <accessToken>`. Roles allowed: `Company Admin` only.

**Request body** (`EmployeeDto`):

| Field | Type | Notes |
|---|---|---|
| `id` | `long?` | nullable, default `0` — omit/`0`/null to create |
| `companyId` | `long` | required |
| `departmentId` | `long` | required |
| `employeeCode` | `string?` | nullable |
| `firstName` | `string` | required |
| `lastName` | `string` | required |
| `email` | `string` | required |
| `password` | `string?` | **required in practice** — backend throws if null on create/update, despite being typed nullable |
| `phone` | `string` | required |
| `roleId` | `long?` | nullable |
| `roleName` | `string?` | nullable, response-only in practice (ignored on write) |
| `gender` | `string?` | nullable |
| `dateOfBirth` | `DateTime?` | nullable |
| `joinDate` | `DateTime` | required |
| `salary` | `decimal` | required |
| `status` | `string` | required (free-text, e.g. `"Active"`) |
| `isActive` | `bool?` | nullable, default `false` |

**Response**: `200 OK`, `EmployeeDto` (same shape as request, echoing what was saved).

---

### GET `/api/Employee/GetAllEmployeesByCompanyId/{companyId}`

🔒 **Enforced**: requires `Authorization: Bearer <accessToken>`. Roles allowed: `Company Admin` only.

**Route param**: `companyId` (`long`, required).

**Query params**:

| Field | Type | Notes |
|---|---|---|
| `departmentId` | `long?` | nullable — filter to a single department |
| `viewOrder` | `string?` | nullable, default `"desc"` |
| `pageNumber` | `int?` | nullable, default `1` |
| `pageSize` | `int?` | nullable, default `50`, max `100` |

**Response**: `200 OK`, `PagedResult<EmployeeDto>`.

---

### GET `/api/Employee/GetEmployeeById/{employeeId}`

🔒 **Enforced**: requires `Authorization: Bearer <accessToken>`. Roles allowed: `Company Admin` only.

**Route param**: `employeeId` (`long`, required).

**Response**: `200 OK`, `EmployeeDto`. ⚠️ Does **not** populate `id`, `password`, or `roleId` on this response (`roleName` is populated instead, resolved from the employee's role).

---

## Role

Base route: `api/Role`

### POST `/api/Role/AddOrUpdateRole`

🔒 **Enforced**: requires `Authorization: Bearer <accessToken>`. Roles allowed: `Super Admin` only.

**Request body** (`RoleDto`):

| Field | Type | Notes |
|---|---|---|
| `roleId` | `long?` | nullable — omit/null to create |
| `roleName` | `string` | required (free text, e.g. `"Manager"`, `"HR"`, `"Employee"` — no fixed set) |
| `createdAt` | `DateTime?` | nullable — ignored by backend |
| `updatedAt` | `DateTime?` | nullable — ignored by backend |
| `isActive` | `bool?` | nullable |

**Response**: `200 OK`, `RoleDto`. ⚠️ Only `roleId`, `roleName`, `isActive` populated — timestamps are `null`.

---

### GET `/api/Role/GetAllRoles`

🔒 **Enforced**: requires `Authorization: Bearer <accessToken>`. Roles allowed: `Super Admin` only. ⚠️ This is needed to populate role dropdowns (e.g. on the employee form), but `Company Admin` — the role that actually calls `AddOrUpdateEmployee` — is locked out of it. Flag this to the backend owner; as written, a Company Admin can't populate the role picker they need.

**Request**: no params.

**Response**: `200 OK`, `List<RoleDto>` (not paginated — role lists are expected to be small). Each item only has `roleId`, `roleName`, `isActive` populated.

---

### GET `/api/Role/GetRoleById/{roleId}`

🔒 **Enforced**: requires `Authorization: Bearer <accessToken>`. Roles allowed: `Super Admin` only.

**Route param**: `roleId` (`long`, required).

**Response**: `200 OK`, `RoleDto` (`roleId`, `roleName`, `isActive` only).

---

## Attendance

Base route: `api/Attendance`

### POST `/api/Attendance/CheckIn&CheckOut`

⚠️ The literal route contains an unencoded `&`. When calling from Angular's `HttpClient`, do not URL-encode it yourself — pass the path as-is (`/api/Attendance/CheckIn&CheckOut`); most HTTP clients leave `&` untouched in a path segment. Verify against a live call before relying on this.

🔒 **Enforced**: requires `Authorization: Bearer <accessToken>`. Roles allowed: `Company Admin`, `Common`.

**Request body** (wraps `AttendanceDto` under a `dto` key, since the command is `CheckInCommand(AttendanceDto Dto)`):

| Field (under `dto`) | Type | Notes |
|---|---|---|
| `attendanceId` | `long?` | nullable — omit/null to create |
| `companyId` | `long` | required |
| `employeeId` | `long` | required |
| `attendanceDate` | `DateOnly` (`"YYYY-MM-DD"`) | required |
| `checkIn` | `TimeOnly?` (`"HH:mm:ss"`) | nullable |
| `checkOut` | `TimeOnly?` (`"HH:mm:ss"`) | nullable |
| `workingHours` | `decimal?` | nullable — computed server-side, don't send |
| `lateMinutes` | `int?` | nullable — computed server-side, don't send |
| `earlyLeaveMinutes` | `int?` | nullable — computed server-side, don't send |
| `status` | `string?` | nullable |
| `remarks` | `string?` | nullable |
| `createdAt` | `DateTime?` | nullable — ignored by backend |

```json
{
  "dto": {
    "attendanceId": null,
    "companyId": 1,
    "employeeId": 42,
    "attendanceDate": "2026-08-31",
    "checkIn": "09:05:00",
    "checkOut": null,
    "status": null,
    "remarks": null
  }
}
```

**Response**: `200 OK`, `AttendanceDto` (same shape, with server-computed fields filled in).

---

### GET `/api/Attendance/GetAttendanceByDate`

🔒 **Enforced**: requires `Authorization: Bearer <accessToken>`. Roles allowed: `Company Admin`, `Common`.

**Query params**: `companyId` (`long`, required), `date` (`DateOnly`, `"YYYY-MM-DD"`, required).

**Response**: `200 OK`, `List<AttendanceDto>` (not paginated).

---

### GET `/api/Attendance/GetAttendanceByEmployeeId`

🔒 **Enforced**: requires `Authorization: Bearer <accessToken>`. Roles allowed: `Company Admin`, `Common`.

**Query params**: `employeeId` (`long`, required).

**Response**: `200 OK`, `List<AttendanceDto>` (not paginated).

---

### GET `/api/Attendance/GetAttendancesStatisticsByEmployeeId`

🔒 **Enforced**: requires `Authorization: Bearer <accessToken>`. Roles allowed: `Company Admin`, `Common`.

**Query params**: `employeeId` (`long`, required), `monthId` (`int`, required), `yearId` (`int`, required).

**Response**: `200 OK`, `AttendanceStatisticsDto`:

| Field | Type |
|---|---|
| `presentDays` | `long` |
| `lateDays` | `long` |
| `leaveDays` | `long` |
| `attendanceRatio` | `long` |

---

### GET `/api/Attendance/GetAttendanceSummaryForMonth`

🔒 **Enforced**: requires `Authorization: Bearer <accessToken>`. Roles allowed: `Company Admin` only.

**Query params**: `companyId` (`long`, required), `monthId` (`int`, required), `yearId` (`long`, required).

**Response**: `200 OK`, `AttendanceSummaryDto`:

| Field | Type | Notes |
|---|---|---|
| `averageAttendanceRate` | `long?` | nullable |
| `totalLateArrivals` | `long?` | nullable |
| `numOfPerfectAttendance` | `long?` | nullable |
| `employeeList` | `PerfectAttendanceEmployeeSummaryDto[]?` | nullable, see below |
| `mostPunctualDepartmentId` | `long?` | nullable |
| `mostPunctualDepartmentName` | `string?` | nullable |
| `lateRate` | `decimal?` | nullable — this is the **most punctual department's** late rate, display it on that card |
| `highestAbsenteeId` | `long?` | nullable — id of the first highest absentee (kept for compatibility; prefer `highestAbsentees`) |
| `highestAbsenteeName` | `string?` | nullable — full name(s); on a tie all tied names joined with `", "`, e.g. `"Din Islam, Mash T, rakin a"` |
| `highestAbsentees` | `PerfectAttendanceEmployeeSummaryDto[]` | every employee sharing the highest non-zero absent count (empty when nobody was absent) — use this to render the tie properly |

Notes: `numOfPerfectAttendance` counts employees with zero absences in the month. `employeeList` contains **all** active employees with their absent counts (despite the DTO name), sorted by `totalAbsent` descending then name.

`PerfectAttendanceEmployeeSummaryDto`:

| Field | Type | Notes |
|---|---|---|
| `employeeId` | `long` | |
| `employeeName` | `string?` | nullable |
| `departmentName` | `string?` | nullable |
| `totalAbsent` | `long?` | nullable |

---

### GET `/api/Attendance/GetAttendanceSummaryForADay`

🔒 **Enforced**: requires `Authorization: Bearer <accessToken>`. Roles allowed: `Company Admin` only.

**Query params**: `companyId` (`long`, required), `date` (`DateOnly`, `"YYYY-MM-DD"`, required).

**Response**: `200 OK`, `AttendanceSummaryForADayDto`:

| Field | Type |
|---|---|
| `totalEmployees` | `long` |
| `totalPresent` | `long` |
| `totalLate` | `long` |
| `totalLeave` | `long` |
| `totalAbsent` | `long` |
| `totalAbsentArrival` | `long` |

---

## Leave

Base route: `api/Leave`

### POST `/api/Leave/AddLeaveRequest`

🔒 **Enforced**: requires `Authorization: Bearer <accessToken>`. Roles allowed: `Common` only. ⚠️ Note this is the *only* role that can call this endpoint — including for the "approve/reject via `status`" path, since there's no separate approval endpoint here (that's `UpdateLeaveRequestStatus`, which is `Company Admin`-only).

**Request body** (wraps `LeaveRequestDto` under `dto`):

| Field (under `dto`) | Type | Notes |
|---|---|---|
| `leaveRequestId` | `long?` | nullable — omit/null to create |
| `companyId` | `long` | required |
| `employeeId` | `long` | required |
| `leaveTypeId` | `long` | required |
| `fromDate` | `DateTime` | required |
| `toDate` | `DateTime` | required (must be ≥ `fromDate` — backend throws otherwise) |
| `totalDays` | `int` | ignored — recomputed server-side, don't rely on the value you send |
| `reason` | `string?` | nullable |
| `status` | `string?` | nullable — defaults to `"Pending"` on create; set to `"Approved"`/`"Rejected"`/`"Cancelled"` to action a request (see `LeaveRequestStatusEnum` below) |
| `approvedBy` | `long?` | nullable — set server-side from the acting user's `NameIdentifier` claim when status becomes `Approved` (⚠️ since this endpoint is `Common`-role-only, "approver" here means whichever authenticated `Common` user made the call — there's no separate Admin/Manager approval path) |
| `approvedByName` | `string?` | nullable, response-only |
| `approvedAt` | `DateTime?` | nullable, response-only |
| `airecommendation` | `string?` | nullable |
| `ainotes` | `string?` | nullable |

**Response**: `200 OK`, empty object `{}` (MediatR `Unit` — the backend does not return the saved record; re-fetch via one of the GET endpoints below if you need it).

---

### GET `/api/Leave/GetLeaveRequestByEmployeeId?employeeId={id}`

🔒 **Enforced**: requires `Authorization: Bearer <accessToken>`. Roles allowed: `Company Admin`, `Common`.

**Query params**: `employeeId` (`long`, required).

**Response**: `200 OK`, `List<LeaveRequestDto>`. ⚠️ `employeeId` in each returned item is currently always `0` (backend bug — do not rely on this field from this endpoint specifically; you already know the id you queried with). `approvedByName`, `airecommendation`, `ainotes` are also not populated here.

---

### GET `/api/Leave/GetLeaveRequestByStatus`

🔒 **Enforced**: requires `Authorization: Bearer <accessToken>`. Roles allowed: `Company Admin`, `Common`.

**Query params**: `id` (int or string name of `LeaveRequestStatusEnum`: `All=0`, `Pending=1`, `Approved=2`, `Rejected=3`, `Cancelled=4`), `companyId` (`long`, required).

**Response**: `200 OK`, `List<LeaveRequestDto>` (`airecommendation`/`ainotes` not populated).

---

### GET `/api/Leave/GetEmployeeLeaveRequestsByEmployeeId?employeeId={id}`

🔒 **Enforced**: requires `Authorization: Bearer <accessToken>`. Roles allowed: `Company Admin` only.

**Query params**: `employeeId` (`long`, required).

**Response**: `200 OK`, `List<LeaveRequestDto>` (all fields populated except `approvedByName`).

---

### POST `/api/Leave/UpdateLeaveRequestStatus`

Marks a date range as "On Leave" in the attendance records (used after approving a leave request).

🔒 **Enforced**: requires `Authorization: Bearer <accessToken>`. Roles allowed: `Company Admin` only.

**Request body**:

| Field | Type | Notes |
|---|---|---|
| `dto` | `AttendanceDto` | see [Attendance](#attendance) shape — only `employeeId`, `companyId`, `remarks` are meaningfully used |
| `fromDate` | `DateOnly` (`"YYYY-MM-DD"`) | required |
| `toDate` | `DateOnly` (`"YYYY-MM-DD"`) | required |

**Response**: `200 OK`, `true` (plain boolean body) on success. Weekends/holidays in the range are skipped automatically.

---

## Payroll

Base route: `api/Payroll`

### GET `/api/Payroll/GetPayRollForEmployee`

🔒 **Enforced**: requires `Authorization: Bearer <accessToken>`. Roles allowed: `Company Admin` only.

**Query params**: `employeeId` (`long`, required), `yearId` (`long`, required), `monthId` (`long`, required).

**Response**: `200 OK`, `PayrollDto`:

| Field | Type | Notes |
|---|---|---|
| `payrollId` | `long` | |
| `companyId` | `long` | |
| `employeeId` | `long` | |
| `month` | `long` | |
| `year` | `long` | |
| `basicSalary` | `decimal` | |
| `absentDeduction` | `decimal?` | nullable |
| `lateDeduction` | `decimal?` | nullable |
| `netSalary` | `decimal?` | nullable |
| `generatedAt` | `DateTime?` | nullable — not populated by this endpoint currently |

---

### GET `/api/Payroll/GetPayRollForCompany`

🔒 **Enforced**: requires `Authorization: Bearer <accessToken>`. Roles allowed: `Company Admin` only.

**Query params**: `companyId` (`long`, required), `yearId` (`long`, required), `monthId` (`long`, required).

**Response**: `200 OK`, `List<PayrollDto>` (same shape as above, not paginated).

---

### GET `/api/Payroll/GetPayrollStatusForEmployee`

🔒 **Enforced**: requires `Authorization: Bearer <accessToken>`. Roles allowed: `Company Admin` only.

Live, **non-persisted** payroll projection for the current month: computed on every call from this month's attendance so far using the exact same formula as real payroll generation. Nothing is written to the database.

**Query params**: `employeeId` (`long`, required). The month/year are always the current month (server time) — they cannot be chosen.

**Response**: `200 OK`, `PayrollDto`. `payrollId` is always `0` and `generatedAt` is always `null` — that is how the frontend can tell this is an estimate, not a generated payroll record. `employeeName` is populated. The value changes day to day as attendance accrues.

---

## AI Assistant

Base route: `api/Ai`

### GET `/api/Ai/Ask?question={text}`

🔒 **Enforced**: requires `Authorization: Bearer <accessToken>`. Roles allowed: `Company Admin` only.

Natural-language Q&A over the caller's own company data. The question is answered by an LLM that can only call a fixed set of read-only backend tools (attendance and leave) — it never writes SQL and never sees other companies' data (the company is taken from the token's `CompanyId` claim, not from the request). Stateless: each question must be self-contained (no conversation memory). Answers are given in the language the question was asked in.

**Query params**:

| Field | Type | Notes |
|---|---|---|
| `question` | `string` | required, max 1000 characters |

Example: `GET /api/Ai/Ask?question=Who%20was%20late%20the%20most%20in%20September%3F`

**Response**: `200 OK`, `AiAnswerDto`:

| Field | Type | Notes |
|---|---|---|
| `question` | `string` | the question as received (trimmed) |
| `answer` | `string` | plain-text answer (may contain line breaks / simple bullet lists) |
| `toolsUsed` | `string[]` | names of the backend data tools the AI called to produce the answer — show this to the user as "based on: …" for trust, or use it for debugging. Empty if the AI answered without fetching data (e.g. asked for clarification). |

```json
{
  "question": "Who was late the most in September?",
  "answer": "In September 2026 the highest number of late arrivals was Sahadat Sanbid (4 late days).",
  "toolsUsed": ["get_attendance_summary_for_month"]
}
```

What it can answer at launch (tools available to the model): company attendance summary for a day or a month, per-employee attendance counts for a month (who was late/absent the most, rankings), all attendance records for a day, one employee's attendance statistics for a month, one employee's attendance records over a date range, leave requests by status (Pending/Approved/Rejected/Cancelled/All), one employee's leave history, and employee lookup by name/code (id, name, code, department id, status — no salary/contact data). Payroll and salary questions are **not** answerable yet.

Errors: `400` if `question` is missing/too long; `403` if the token has no `CompanyId` claim; `500`/`502`-style failures if the LLM provider is unreachable, rate-limited (Groq free tier), or the `AiSettings:ApiKey` is not configured. Typical latency is 2–10 s — show a loading state.

---

## Known backend quirks

Things discovered while surveying the code that will bite you if the Angular side assumes "normal" REST behavior. Flag these to the backend owner rather than working around them silently in the frontend, since some are outright bugs:

1. **Authorization is now enforced everywhere.** Every controller action requires a bearer token and a matching role — see each endpoint's "🔒 Enforced" note for the exact role(s). Calling one without a token, with an expired token, or with the wrong role now gets `401`/`403`. Watch for these gaps discovered while surveying the code:
   - `GET /api/Role/GetAllRoles` is `Super Admin`-only, but `Company Admin` is the role that actually needs it to populate a role picker on the employee form (`POST /api/Employee/AddOrUpdateEmployee` is `Company Admin`-only). A `Company Admin` user cannot currently fetch the role list to build that dropdown — flag this to the backend owner rather than working around it (e.g. don't hardcode a role list on the frontend).
   - `POST /api/Leave/AddLeaveRequest` is `Common`-only, including the "approve/reject via `status`" path — there is no separate `Company Admin`/manager approval endpoint for leave requests. If an approval workflow restricted to admins/managers is expected, this is a backend gap, not a frontend one.
2. **`GET /api/Department/GetDepartmentById{departmentId}`** has no `/` before the id — the real URL is e.g. `.../GetDepartmentById5`, not `.../GetDepartmentById/5`. Confirm this against a live call.
3. Several "edit" endpoints echo back an **incomplete DTO** rather than the full saved record: `EditCompany` (missing id/timestamps), `EditDepartment` (missing id/employeeCount/timestamps), `AddOrUpdateRole` (missing timestamps). If you need the generated id or full record after a create, re-fetch it with the matching `GetById`/`GetAll` endpoint.
4. **`AddLeaveRequest`** returns an empty `{}` body, not the saved leave request — re-fetch if you need it.
5. **`GetLeaveRequestByEmployeeId`** returns `employeeId: 0` on every item (bug) — don't read `employeeId` off this endpoint's results.
6. **`CheckIn&CheckOut`** route contains a literal, unencoded `&` — verify Angular's `HttpClient` doesn't mangle it before wiring this up.
7. **Refresh/logout** rely on an HttpOnly `refreshToken` cookie, not a request body — those two calls must be made with `withCredentials: true`, and there is nothing to send in the JSON body.
8. **Role names are entirely data-driven** — there is no fixed enum/constant list of role names (e.g. no guaranteed `"Admin"`/`"Manager"`/`"Employee"` strings). Populate role pickers from `GET /api/Role/GetAllRoles` rather than hardcoding options.
