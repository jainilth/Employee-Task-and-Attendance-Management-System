# Employee Task and Attendance Management System

Backend API for managing employees, departments, attendance, tasks, and leave requests with JWT authentication and role-based authorization.

## Core Functionalities

### Authentication
- User login with email and password
- JWT token generation
- Current-user profile lookup through the token

### Department Management
- Create departments
- View all departments or one department by id
- Update department names
- Delete departments

### User Management
- Create users with hashed passwords
- View all users or a single user by id
- Update user details
- Assign or change role
- Assign or change department
- Delete users

### Attendance Management
- Employee self check-in and check-out
- Manual attendance creation for Admin and Manager
- View attendance by employee, date, or status
- Update attendance status for a specific employee and date
- Delete attendance records

### Task Management
- Create tasks with title, description, assignee, priority, and deadline
- View all tasks or tasks assigned to the current user
- Update task details
- Assign or reassign tasks
- Update task status
- Delete tasks

### Leave Management
- Admin or Manager can create leave requests for employees
- Employees can apply for their own leave
- View all leave requests or only current-user leaves
- Approve or reject leave requests
- Update leave records
- Delete leave requests

## Role System

The application uses the following role values in both the code and SQL schema:

- Admin
- Manager
- Employee

### Role Access Summary
- Admin: full control over users, departments, attendance, tasks, and leaves
- Manager: operational access to attendance, task assignment, and leave approval/rejection
- Employee: self-service access for profile, attendance, tasks assigned to them, and leave application

## API Endpoints

### Authentication
| Method | Endpoint | Access | Purpose |
|---|---|---|---|
| POST | /api/auth/login | Anonymous | Login and receive JWT token |
| GET | /api/auth/me | Authenticated | Return the current user profile |

### Departments
| Method | Endpoint | Access | Purpose |
|---|---|---|---|
| GET | /api/departments | Authenticated | List all departments |
| GET | /api/departments/{id} | Authenticated | Get a department by id |
| POST | /api/departments | Admin | Create a department |
| PATCH | /api/departments/{id} | Admin | Update a department |
| DELETE | /api/departments/{id} | Admin | Delete a department |

### Users
| Method | Endpoint | Access | Purpose |
|---|---|---|---|
| GET | /api/users | Admin, Manager | List all users |
| GET | /api/users/{id} | Admin, Manager, Employee | Get user by id |
| POST | /api/users | Admin | Create a user |
| PATCH | /api/users/{id} | Admin, Employee | Update a user |
| DELETE | /api/users/{id} | Admin | Delete a user |
| PATCH | /api/users/{id}/role | Admin | Change a user role |
| PATCH | /api/users/{id}/department | Admin | Change a user department |

### Attendance
| Method | Endpoint | Access | Purpose |
|---|---|---|---|
| GET | /api/attendance | Admin, Manager | List all attendance records |
| GET | /api/attendance/self | Authenticated | List attendance for the current user |
| GET | /api/attendance/employee/{id} | Admin, Manager | List attendance by employee |
| GET | /api/attendance/employee/{id}/{date} | Admin, Manager | Get attendance by employee and date |
| GET | /api/attendance/status/{status} | Admin, Manager | Filter attendance by status |
| POST | /api/attendance/checkin | Authenticated | Create today\'s check-in for the current user |
| PATCH | /api/attendance/checkout | Authenticated | Complete today\'s checkout for the current user |
| POST | /api/attendance | Admin, Manager | Manually create attendance |
| PATCH | /api/attendance/{id}/employee/{date}/{status} | Admin, Manager | Update attendance status |
| DELETE | /api/attendance/{id} | Admin, Manager | Delete an attendance record |

### Tasks
| Method | Endpoint | Access | Purpose |
|---|---|---|---|
| GET | /api/tasks | Admin, Manager | List all tasks |
| GET | /api/tasks/my | Admin, Manager | List tasks assigned to the current user |
| GET | /api/tasks/{id} | Admin, Manager | Get task by id |
| POST | /api/tasks | Admin, Manager | Create a task |
| PUT | /api/tasks/{id} | Admin | Update task details |
| PATCH | /api/tasks/{id}/assign | Admin, Manager | Assign or reassign a task |
| PATCH | /api/tasks/{id}/{status} | Admin, Manager | Update task status |
| PATCH | /api/tasks/my/{id}/{status} | Authenticated | Update status for a task assigned to the current user |
| DELETE | /api/tasks/{id} | Admin, Manager | Delete a task |

### Leaves
| Method | Endpoint | Access | Purpose |
|---|---|---|---|
| GET | /api/leaves | Admin, Manager | List all leave requests |
| GET | /api/leaves/self | Authenticated | List current-user leave requests |
| GET | /api/leaves/{id} | Admin, Manager | Get leave request by id |
| POST | /api/leaves | Admin, Manager | Create a leave request for an employee |
| POST | /api/leaves/apply | Authenticated | Apply for leave as the current user |
| PUT | /api/leaves/{id} | Admin | Update a leave request |
| PATCH | /api/leaves/{id}/approve | Admin, Manager | Approve a leave request |
| PATCH | /api/leaves/{id}/reject | Admin, Manager | Reject a leave request |
| DELETE | /api/leaves/{id} | Admin, Employee | Delete a leave request |

## SQL Constraints

### Unique Constraints
- `Departments.Name` is unique
- `Users.Email` is unique
- `Attendance.EmployeeId + Date` is unique, so one attendance row per employee per day

### Foreign Keys
- `Users.DepartmentId` references `Departments.Id` and is set to null when the department is deleted
- `Attendance.EmployeeId` references `Users.Id`
- `Tasks.AssignedTo` references `Users.Id`
- `Tasks.AssignedBy` references `Users.Id`
- `Leaves.EmployeeId` references `Users.Id`

### Check Constraints
- `Users.Role` must be one of `Admin`, `Manager`, or `Employee`
- `Attendance.Status` must be one of `Present`, `Absent`, `Late`, `HalfDay`, or `OnLeave`
- `Tasks.Priority` must be one of `Low`, `Medium`, or `High`
- `Tasks.Status` must be one of `Pending`, `InProgress`, `Completed`, or `Blocked`
- `Leaves.Status` must be one of `Pending`, `Approved`, or `Rejected`

### Defaults and Indexes
- `Tasks.CreatedAt` defaults to `SYSUTCDATETIME()`
- The schema includes indexes for department name, user email, user role, attendance date and status, task assignment and deadline lookups, and leave filters

## Workflows

### Login and Identity
1. User submits email and password to `/api/auth/login`.
2. The API verifies the password hash and returns a JWT token.
3. The token carries user id, email, and role claims.
4. The `/api/auth/me` endpoint resolves the current profile from the token.

### Department Administration
1. Admin creates or updates departments.
2. Users can be assigned to a department.
3. If a department is deleted, linked users keep their account and their `DepartmentId` becomes null.

### User Administration
1. Admin creates a user with a hashed password.
2. Admin assigns a role and optionally a department.
3. Admin can later update, reassign, or delete the user.

### Attendance Tracking
1. An authenticated user checks in for the current day.
2. The API creates a single attendance record for that employee and date.
3. The same user checks out later in the day.
4. Working hours are calculated from `CheckIn` and `CheckOut`.
5. Admin or Manager can review and correct attendance status manually.

### Task Handling
1. Admin or Manager creates a task.
2. The task can be created with or without an assignee.
3. Admin or Manager assigns or reassigns the task.
4. The assignee can update the task status through the self-status endpoint.
5. Admin or Manager can also update, reassign, or delete tasks.

### Leave Handling
1. An authenticated employee applies for leave through `/api/leaves/apply`.
2. Admin or Manager can also create leave requests directly.
3. Admin or Manager approves or rejects the request.
4. Admin can update a leave request, and employees can delete leave requests when allowed by the API policy.

## Notes

- The project currently focuses on authentication, departments, users, attendance, tasks, and leaves.
- There are no dedicated reporting, notification, or audit-log endpoints in the current controllers.
- Scalar is enabled in development on top of the generated OpenAPI document, and JWT bearer authentication is configured for the API.
