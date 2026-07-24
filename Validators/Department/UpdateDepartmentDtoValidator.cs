using FluentValidation;
using Employee_Task_and_Attendance_Management_System.DTOs.Department;

namespace Employee_Task_and_Attendance_Management_System.Validators.Department
{
    public class UpdateDepartmentDtoValidator : AbstractValidator<UpdateDepartmentDto>
    {
        public UpdateDepartmentDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");
        }
    }
}
