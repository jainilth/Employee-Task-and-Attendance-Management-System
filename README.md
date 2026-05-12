# Employee Task and Attendance Management System

A comprehensive employee management system built with ASP.NET Core that handles task management, attendance tracking, and leave management with role-based access control.

## Table of Contents
- [Core Functionalities](#core-functionalities)
- [Role System](#role-system)
- [API Endpoints](#api-endpoints)
- [SQL Constraints](#sql-constraints)
- [Workflows](#workflows)
- [Implementation Status](#implementation-status)

---

## Core Functionalities

### 1.1 Authentication and Identity
- User login
- Session identity lookup
- Role-based authorization using user role values
- **Status**: ✅ Done

### 1.2 Department Management
- Create department
- View departments
- Update department
- Delete department
- Keep department names unique
- **Status**: ✅ Done

### 1.3 User and Employee Management
- Create user
- View users
- Update user details
- Delete user
- Assign or change user department
- Assign or change user role
- Keep user email unique
- **Status**: ✅ Done

### 1.4 Attendance Management
- Employee check-in
- Employee check-out
- Manual attendance create and correction
- Attendance history listing and filtering
- Attendance status management
- Enforce one attendance record per employee per date
- **Status**: ✅ Done

### 1.5 Task Management
- Create task
- View tasks
- Update task details
- Assign or reassign task
- Update task status
- Delete task
- Support unassigned tasks at creation time
- **Status**: 🟡 Partially Done (status-only update missing)

### 1.6 Leave Management
- Apply leave
- View leaves
- Update pending leave details
- Approve leave
- Reject leave
- Delete leave request under allowed conditions
- **Status**: 🟡 Partially Done (approval/rejection endpoints missing)

### 1.7 Reporting
- Monthly attendance summary
- Task summary by status, assignee, deadline
- Leave summary by status and employee
- **Status**: ❌ Not started

---

## Role System

### Allowed Role Values
- **Admin**: Full system control
- **Manager**: Team operations and approvals
- **Employee**: Self-service operations

### Role Meaning
- **Admin**: Full access to all endpoints and operations
- **Manager**: Team operations, approvals, and reporting
- **Employee**: Self-service operations and limited access

---

## API Endpoints

### Complete Endpoint Mapping (40 Total)

| Endpoint | Method | Purpose | Allowed Roles | Status |
|---|---|---|---|---|
| **Authentication** |
| /api/auth/login | POST | Authenticate user | Admin, Manager, Employee | ✅ |
| /api/auth/me | GET | Get current profile | Admin, Manager, Employee | ✅ |
| **Departments** |
| /api/departments | GET | List departments | Admin, Manager, Employee | ✅ |
| /api/departments/{id} | GET | Get department by id | Admin, Manager, Employee | ✅ |
| /api/departments | POST | Create department | Admin | ✅ |
| /api/departments/{id} | PUT | Update department | Admin | ✅ |
| /api/departments/{id} | DELETE | Delete department | Admin | ✅ |
| **Users** |
| /api/users | GET | List users | Admin, Manager | ✅ |
| /api/users/{id} | GET | Get user by id | Admin, Manager, Employee self | ✅ |
| /api/users | POST | Create user | Admin | ✅ |
| /api/users/{id} | PUT | Update user profile | Admin, Employee self | ✅ |
| /api/users/{id}/role | PATCH | Change role | Admin | ✅ |
| /api/users/{id}/department | PATCH | Change department | Admin | ✅ |
| /api/users/{id} | DELETE | Delete user | Admin | ✅ |
| **Attendance** |
| /api/attendance | GET | List attendance records | Admin, Manager, Employee self | ✅ |
| /api/attendance/{id} | GET | Get attendance by id | Admin, Manager, Employee self | ✅ |
| /api/attendance/checkin | POST | Check-in | Employee self, Admin | ✅ |
| /api/attendance/checkout | POST | Check-out | Employee self, Admin | ✅ |
| /api/attendance | POST | Manual attendance create | Admin, Manager | ✅ |
| /api/attendance/{id} | PUT | Update attendance | Admin, Manager | ✅ |
| /api/attendance/{id} | DELETE | Delete attendance | Admin | ✅ |
| **Tasks** |
| /api/tasks | GET | List tasks | Admin, Manager, Employee assigned or created by self | ✅ |
| /api/tasks/{id} | GET | Get task by id | Admin, Manager, Employee assigned or created by self | ✅ |
| /api/tasks | POST | Create task | Admin, Manager | ✅ |
| /api/tasks/{id} | PUT | Update task details | Admin, Manager | ✅ |
| /api/tasks/{id}/assign | PATCH | Assign or reassign task | Admin, Manager | ✅ |
| /api/tasks/{id}/status | PATCH | Update task status | Admin, Manager, Employee assigned | ❌ |
| /api/tasks/{id} | DELETE | Delete task | Admin, Manager | ✅ |
| **Leaves** |
| /api/leaves | GET | List leave requests | Admin, Manager, Employee self | ✅ |
| /api/leaves/{id} | GET | Get leave request by id | Admin, Manager, Employee self | ✅ |
| /api/leaves | POST | Apply leave | Employee self, Admin | ✅ |
| /api/leaves/{id} | PUT | Update leave details | Employee self while Pending, Admin | ✅ |
| /api/leaves/{id}/approve | PATCH | Approve leave | Admin, Manager | ❌ |
| /api/leaves/{id}/reject | PATCH | Reject leave | Admin, Manager | ❌ |
| /api/leaves/{id} | DELETE | Delete leave request | Admin, Employee self while Pending | ✅ |
| **Reporting** |
| /api/reports/attendance/monthly | GET | Monthly attendance report | Admin, Manager | ❌ |
| /api/reports/tasks/summary | GET | Task summary report | Admin, Manager | ❌ |
| /api/reports/leaves/summary | GET | Leave summary report | Admin, Manager | ❌ |

---

## SQL Constraints

### Enum Constraints
- **User role**: Admin, Manager, Employee
- **Attendance status**: Present, Absent, Late, HalfDay, OnLeave
- **Task priority**: Low, Medium, High
- **Task status**: Pending, InProgress, Completed, Blocked
- **Leave status**: Pending, Approved, Rejected

### Foreign Key Rules
- Users.DepartmentId references Departments.Id and becomes null if department is deleted
- Attendance.EmployeeId references Users.Id
- Tasks.AssignedTo references Users.Id and can be null
- Tasks.AssignedBy references Users.Id and is required
- Leaves.EmployeeId references Users.Id

### Uniqueness Rules
- Department name is unique
- User email is unique
- Attendance is unique on EmployeeId + Date

### Performance Indexes
- Users: DepartmentId, Role
- Attendance: Date + Status
- Tasks: AssignedTo + Status, Deadline, AssignedBy
- Leaves: EmployeeId + StartDate, Status

---

## Workflows

### 5.1 Department Setup Workflow
1. Admin creates departments.
2. Admin updates department names if needed.
3. Admin deletes inactive departments.
4. Linked users remain valid with DepartmentId set to null.

### 5.2 User Onboarding Workflow
1. Admin creates user.
2. System enforces unique email.
3. Admin assigns role.
4. Admin optionally assigns department.
5. User logs in and starts role-based actions.

### 5.3 Employee Daily Attendance Workflow
1. Employee logs in.
2. Employee performs check-in.
3. Employee performs check-out.
4. System stores working hours and status.
5. System blocks duplicate attendance for same employee and date.

### 5.4 Attendance Correction Workflow
1. Manager or Admin opens attendance record.
2. Record is corrected for check-in, check-out, working hours, or status.
3. Updated record remains under unique employee-date rule.

### 5.5 Task Creation and Assignment Workflow
1. Admin or Manager creates task.
2. Task can be created with or without assignee.
3. Admin or Manager assigns or reassigns task.
4. Employee receives task and updates status.
5. Manager tracks progress through status and deadline.

### 5.6 Task Escalation Workflow
1. Task remains Pending or Blocked near deadline.
2. Manager reassigns task or updates timeline.
3. Employee continues progress updates.
4. Task reaches Completed state.

### 5.7 Leave Application Workflow
1. Employee creates leave request with Pending status.
2. Manager or Admin reviews request.
3. Reviewer approves or rejects leave.
4. Leave status is finalized and visible in history.

### 5.8 Leave Edit and Cancel Workflow
1. Employee edits or deletes own leave while status is Pending.
2. Admin can update or delete when policy allows.
3. Approved or Rejected requests are treated as finalized records.

### 5.9 User Offboarding Workflow
1. Admin prepares to remove user.
2. Admin validates dependency records in attendance, tasks, and leaves.
3. Admin reassigns ownership where needed.
4. Admin deletes user safely without foreign key conflicts.

### 5.10 Reporting Workflow
1. Manager or Admin filters attendance by date and status.
2. Manager or Admin filters tasks by assignee, status, and deadline.
3. Manager or Admin filters leaves by employee and status.
4. Reports are used for review and planning.

---

## Implementation Status

### Summary
- **Total MVP Endpoints**: 39
- **Implemented**: 32 ✅
- **Not Started**: 7 ❌

### Completed Areas
| Area | Status | Endpoints |
| --- | --- | --- |
| Department Management | ✅ Done | 5/5 |
| User and Employee Management | ✅ Done | 7/7 |
| Attendance Management | ✅ Done | 7/7 |
| Task Management | 🟡 Partial | 6/7 |
| Leave Management | 🟡 Partial | 5/7 |

### Missing Implementations
- ❌ Task status-only update (1 endpoint)
- ❌ Leave approval/rejection (2 endpoints)
- ❌ Various role-based access control implementations
- ❌ Reporting (3 endpoints)

### Next Steps
1. Add approval/rejection workflows for leaves
2. Implement reporting endpoints
