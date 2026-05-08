using System;
using System.Collections.Generic;

namespace Employee_Task_and_Attendance_Management_System.Models;

public partial class Department
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
