# Employee Task and Attendance Management System - SQL Driven MVP Blueprint

This document is generated from the current SQL schema only.

## 1. Core Functionalities

### 1.1 Authentication and Identity
- User registration
- User login
- Session identity lookup
- Role-based authorization using user role values

### 1.2 Department Management
- Create department
- View departments
- Update department
- Delete department
- Keep department names unique

### 1.3 User and Employee Management
- Create user
- View users
- Update user details
- Delete user
- Assign or change user department
- Assign or change user role
- Keep user email unique

### 1.4 Attendance Management
- Employee check-in
- Employee check-out
- Manual attendance create and correction
- Attendance history listing and filtering
- Attendance status management
- Enforce one attendance record per employee per date

### 1.5 Task Management
- Create task
- View tasks
- Update task details
- Assign or reassign task
- Update task status
- Delete task
- Support unassigned tasks at creation time

### 1.6 Leave Management
- Apply leave
- View leaves
- Update pending leave details
- Approve leave
- Reject leave
- Delete leave request under allowed conditions

### 1.7 Reporting
- Monthly attendance summary
- Task summary by status, assignee, deadline
- Leave summary by status and employee

## 2. Role System

### 2.1 Allowed Role Values
- Admin
- Manager
- Employee

### 2.2 Role Meaning
- Admin: Full system control
- Manager: Team operations and approvals
- Employee: Self-service operations

## 3. SQL Constraints That Define Business Rules

### 3.1 Enum Constraints
- User role: Admin, Manager, Employee
- Attendance status: Present, Absent, Late, HalfDay, OnLeave
- Task priority: Low, Medium, High
- Task status: Pending, InProgress, Completed, Blocked
- Leave status: Pending, Approved, Rejected

### 3.2 Foreign Key Rules
- Users.DepartmentId references Departments.Id and becomes null if department is deleted
- Attendance.EmployeeId references Users.Id
- Tasks.AssignedTo references Users.Id and can be null
- Tasks.AssignedBy references Users.Id and is required
- Leaves.EmployeeId references Users.Id

### 3.3 Uniqueness Rules
- Department name is unique
- User email is unique
- Attendance is unique on EmployeeId + Date

### 3.4 Performance Indexes
- Users: DepartmentId, Role
- Attendance: Date + Status
- Tasks: AssignedTo + Status, Deadline, AssignedBy
- Leaves: EmployeeId + StartDate, Status

## 4. Complete Endpoints and Allowed Roles

| Endpoint | Method | Purpose | Allowed Roles |
|---|---|---|---|
| /api/auth/register | POST | Create user account | Admin |
| /api/auth/login | POST | Authenticate user | Admin, Manager, Employee |
| /api/auth/me | GET | Get current profile | Admin, Manager, Employee |
| /api/departments | GET | List departments | Admin, Manager, Employee |
| /api/departments/{id} | GET | Get department by id | Admin, Manager, Employee |
| /api/departments | POST | Create department | Admin |
| /api/departments/{id} | PUT | Update department | Admin |
| /api/departments/{id} | DELETE | Delete department | Admin |
| /api/users | GET | List users | Admin, Manager |
| /api/users/{id} | GET | Get user by id | Admin, Manager, Employee self |
| /api/users | POST | Create user | Admin |
| /api/users/{id} | PUT | Update user profile | Admin, Employee self |
| /api/users/{id}/role | PATCH | Change role | Admin |
| /api/users/{id}/department | PATCH | Change department | Admin |
| /api/users/{id} | DELETE | Delete user | Admin |
| /api/attendance | GET | List attendance records | Admin, Manager, Employee self |
| /api/attendance/{id} | GET | Get attendance by id | Admin, Manager, Employee self |
| /api/attendance/checkin | POST | Check-in | Employee self, Admin |
| /api/attendance/checkout | POST | Check-out | Employee self, Admin |
| /api/attendance | POST | Manual attendance create | Admin, Manager |
| /api/attendance/{id} | PUT | Update attendance | Admin, Manager |
| /api/attendance/{id} | DELETE | Delete attendance | Admin |
| /api/tasks | GET | List tasks | Admin, Manager, Employee assigned or created by self |
| /api/tasks/{id} | GET | Get task by id | Admin, Manager, Employee assigned or created by self |
| /api/tasks | POST | Create task | Admin, Manager |
| /api/tasks/{id} | PUT | Update task details | Admin, Manager |
| /api/tasks/{id}/assign | PATCH | Assign or reassign task | Admin, Manager |
| /api/tasks/{id}/status | PATCH | Update task status | Admin, Manager, Employee assigned |
| /api/tasks/{id} | DELETE | Delete task | Admin, Manager |
| /api/leaves | GET | List leave requests | Admin, Manager, Employee self |
| /api/leaves/{id} | GET | Get leave request by id | Admin, Manager, Employee self |
| /api/leaves | POST | Apply leave | Employee self, Admin |
| /api/leaves/{id} | PUT | Update leave details | Employee self while Pending, Admin |
| /api/leaves/{id}/approve | PATCH | Approve leave | Admin, Manager |
| /api/leaves/{id}/reject | PATCH | Reject leave | Admin, Manager |
| /api/leaves/{id} | DELETE | Delete leave request | Admin, Employee self while Pending |
| /api/reports/attendance/monthly | GET | Monthly attendance report | Admin, Manager |
| /api/reports/tasks/summary | GET | Task summary report | Admin, Manager |
| /api/reports/leaves/summary | GET | Leave summary report | Admin, Manager |

## 5. All Possible Workflows

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

## 6. Endpoint Security Matrix by Module

### 6.1 Admin
- Full access to all endpoints.

### 6.2 Manager
- Read access to departments.
- Read access to users.
- Full operational access to attendance except hard delete policy where restricted.
- Full operational access to tasks.
- Approve and reject leaves.
- Read access to leave lists and leave details.
- Access to reports.

### 6.3 Employee
- Login and profile access.
- Self check-in and check-out.
- Self attendance read access.
- Read tasks assigned to self.
- Update status for assigned tasks.
- Create and manage own leave while pending.
- No department administration.
- No user administration.
- No approval actions.

## 7. MVP Summary

The SQL schema supports a strong MVP with:
- 5 core entities
- role-driven operations
- strict integrity rules
- clear lifecycle states
- scalable query performance through indexes

This contract is implementation-ready for backend APIs, authorization policies, and workflow automation.
