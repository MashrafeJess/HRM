# HRM API Documentation

This document describes every HTTP endpoint exposed by the HRM Web API, for use when scaffolding the Angular frontend. It covers request/response payload shapes, field types, and role restrictions as they currently exist in the backend.

## Conventions used in this doc

- **Base URL**: `https://<host>/api`
- **Auth**: The backend issues a JWT (see `POST /api/Auth/login`) containing claims `NameIdentifier` (employee id), `Email`, `Name` (first name), and `Role` (role name, free-text from the `Roles` table — there is no fixed enum of role names in this system; roles are created ad-hoc via `POST /api/Role/AddOrUpdateRole`).
- **⚠️ Current state**: no controller or action in the backend has an `[Authorize]` attribute yet (only `Login` has `[AllowAnonymous]`). Every endpoint below is reachable without a token today. The "Roles allowed" field in each section states what *should* eventually be enforced based on the nature of the action — treat it as a TODO for the backend, not as current behavior. Do not build frontend logic that assumes the backend already rejects unauthorized calls.
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
9. [Known backend quirks](#known-backend-quirks)

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

Roles allowed: Admin (create/edit company records is an administrative action — not currently enforced).

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

Roles allowed: any authenticated user (read-only).

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

Roles allowed: any authenticated user (read-only).

**Route param**: `companyId` (`long`, required).

**Response**: `200 OK`, `CompanyDto` (full shape, all fields populated). Throws if not found.

---

## Department

Base route: `api/Department`

### POST `/api/Department/EditDepartment`

Roles allowed: Admin, Manager.

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

Roles allowed: any authenticated user (read-only).

**Route param**: `companyId` (`long`, required).

**Query params** (`PageFilterDto`): same as Company's — `viewOrder` (nullable, default `"desc"`), `pageNumber` (nullable, default `1`), `pageSize` (nullable, default `50`, max `100`).

**Response**: `200 OK`, `PagedResult<DepartmentDto>`. ⚠️ Items only populate `departmentId`, `companyId`, `departmentName`, `description`, `isActive` — `employeeCount` and timestamps are `null`/default (use `GetDepartmentById` if you need `employeeCount`).

---

### GET `/api/Department/GetDepartmentById{departmentId}`

⚠️ Note the missing `/` before the route parameter — the real path has no separator, e.g. `GET /api/Department/GetDepartmentById5` for id `5`. This is almost certainly an unintentional bug in the backend route template; confirm behavior against a running instance before wiring this up, since fixing it later will change the URL your frontend must call.

Roles allowed: any authenticated user (read-only).

**Route param**: `departmentId` (`long`, required).

**Response**: `200 OK`, `DepartmentDto` (full shape, including `employeeCount`, `createdAt`, `updatedAt`). Throws `404`-equivalent if not found.

---

## Employee

Base route: `api/Employee`

### POST `/api/Employee/AddOrUpdateEmployee`

Roles allowed: Admin, Manager.

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

Roles allowed: any authenticated user (read-only).

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

Roles allowed: any authenticated user (read-only).

**Route param**: `employeeId` (`long`, required).

**Response**: `200 OK`, `EmployeeDto`. ⚠️ Does **not** populate `id`, `password`, or `roleId` on this response (`roleName` is populated instead, resolved from the employee's role).

---

## Role

Base route: `api/Role`

### POST `/api/Role/AddOrUpdateRole`

Roles allowed: Admin only.

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

Roles allowed: any authenticated user (read-only) — needed to populate role dropdowns e.g. on the employee form.

**Request**: no params.

**Response**: `200 OK`, `List<RoleDto>` (not paginated — role lists are expected to be small). Each item only has `roleId`, `roleName`, `isActive` populated.

---

### GET `/api/Role/GetRoleById/{roleId}`

Roles allowed: any authenticated user (read-only).

**Route param**: `roleId` (`long`, required).

**Response**: `200 OK`, `RoleDto` (`roleId`, `roleName`, `isActive` only).

---

## Attendance

Base route: `api/Attendance`

### POST `/api/Attendance/CheckIn&CheckOut`

⚠️ The literal route contains an unencoded `&`. When calling from Angular's `HttpClient`, do not URL-encode it yourself — pass the path as-is (`/api/Attendance/CheckIn&CheckOut`); most HTTP clients leave `&` untouched in a path segment. Verify against a live call before relying on this.

Roles allowed: Employee (self check-in/out), Admin/Manager (on behalf of others).

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

Roles allowed: Admin, Manager.

**Query params**: `companyId` (`long`, required), `date` (`DateOnly`, `"YYYY-MM-DD"`, required).

**Response**: `200 OK`, `List<AttendanceDto>` (not paginated).

---

### GET `/api/Attendance/GetAttendanceByEmployeeId`

Roles allowed: Employee (own records), Admin/Manager (any).

**Query params**: `employeeId` (`long`, required).

**Response**: `200 OK`, `List<AttendanceDto>` (not paginated).

---

### GET `/api/Attendance/GetAttendancesStatisticsByEmployeeId`

Roles allowed: Employee (own), Admin/Manager (any).

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

Roles allowed: Admin, Manager.

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
| `lateRate` | `decimal?` | nullable |
| `highestAbsenteeId` | `long?` | nullable |
| `highestAbsenteeName` | `string?` | nullable |

`PerfectAttendanceEmployeeSummaryDto`:

| Field | Type | Notes |
|---|---|---|
| `employeeId` | `long` | |
| `employeeName` | `string?` | nullable |
| `departmentName` | `string?` | nullable |
| `totalAbsent` | `long?` | nullable |

---

### GET `/api/Attendance/GetAttendanceSummaryForADay`

Roles allowed: Admin, Manager.

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

Roles allowed: Employee (create own request), Admin/Manager (approve/reject via `status`).

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
| `approvedBy` | `long?` | nullable — set server-side from the acting user when status becomes `Approved` (⚠️ see [Known backend quirks](#known-backend-quirks) — this currently depends on auth claims that aren't enforced yet) |
| `approvedByName` | `string?` | nullable, response-only |
| `approvedAt` | `DateTime?` | nullable, response-only |
| `airecommendation` | `string?` | nullable |
| `ainotes` | `string?` | nullable |

**Response**: `200 OK`, empty object `{}` (MediatR `Unit` — the backend does not return the saved record; re-fetch via one of the GET endpoints below if you need it).

---

### GET `/api/Leave/GetLeaveRequestByEmployeeId?employeeId={id}`

Roles allowed: Employee (own), Admin/Manager (any).

**Query params**: `employeeId` (`long`, required).

**Response**: `200 OK`, `List<LeaveRequestDto>`. ⚠️ `employeeId` in each returned item is currently always `0` (backend bug — do not rely on this field from this endpoint specifically; you already know the id you queried with). `approvedByName`, `airecommendation`, `ainotes` are also not populated here.

---

### GET `/api/Leave/GetLeaveRequestByStatus`

Roles allowed: Admin, Manager.

**Query params**: `id` (int or string name of `LeaveRequestStatusEnum`: `All=0`, `Pending=1`, `Approved=2`, `Rejected=3`, `Cancelled=4`), `companyId` (`long`, required).

**Response**: `200 OK`, `List<LeaveRequestDto>` (`airecommendation`/`ainotes` not populated).

---

### GET `/api/Leave/GetEmployeeLeaveRequestsByEmployeeId?employeeId={id}`

Roles allowed: Employee (own), Admin/Manager (any).

**Query params**: `employeeId` (`long`, required).

**Response**: `200 OK`, `List<LeaveRequestDto>` (all fields populated except `approvedByName`).

---

### POST `/api/Leave/UpdateLeaveRequestStatus`

Marks a date range as "On Leave" in the attendance records (used after approving a leave request).

Roles allowed: Admin, Manager.

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

Roles allowed: Employee (own), Admin/Manager (any).

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

Roles allowed: Admin, Manager.

**Query params**: `companyId` (`long`, required), `yearId` (`long`, required), `monthId` (`long`, required).

**Response**: `200 OK`, `List<PayrollDto>` (same shape as above, not paginated).

---

## Known backend quirks

Things discovered while surveying the code that will bite you if the Angular side assumes "normal" REST behavior. Flag these to the backend owner rather than working around them silently in the frontend, since some are outright bugs:

1. **No authorization is enforced anywhere yet.** Every `Roles allowed` note above is aspirational — the backend accepts unauthenticated calls on every endpoint today. Don't build frontend logic that depends on the backend rejecting an unauthorized role; it currently won't.
2. **`GET /api/Department/GetDepartmentById{departmentId}`** has no `/` before the id — the real URL is e.g. `.../GetDepartmentById5`, not `.../GetDepartmentById/5`. Confirm this against a live call.
3. Several "edit" endpoints echo back an **incomplete DTO** rather than the full saved record: `EditCompany` (missing id/timestamps), `EditDepartment` (missing id/employeeCount/timestamps), `AddOrUpdateRole` (missing timestamps). If you need the generated id or full record after a create, re-fetch it with the matching `GetById`/`GetAll` endpoint.
4. **`AddLeaveRequest`** returns an empty `{}` body, not the saved leave request — re-fetch if you need it.
5. **`GetLeaveRequestByEmployeeId`** returns `employeeId: 0` on every item (bug) — don't read `employeeId` off this endpoint's results.
6. **`CheckIn&CheckOut`** route contains a literal, unencoded `&` — verify Angular's `HttpClient` doesn't mangle it before wiring this up.
7. **Refresh/logout** rely on an HttpOnly `refreshToken` cookie, not a request body — those two calls must be made with `withCredentials: true`, and there is nothing to send in the JSON body.
8. **Role names are entirely data-driven** — there is no fixed enum/constant list of role names (e.g. no guaranteed `"Admin"`/`"Manager"`/`"Employee"` strings). Populate role pickers from `GET /api/Role/GetAllRoles` rather than hardcoding options.
