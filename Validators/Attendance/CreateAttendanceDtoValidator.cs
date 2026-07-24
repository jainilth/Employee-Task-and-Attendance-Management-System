using FluentValidation;
using Employee_Task_and_Attendance_Management_System.DTOs.Attendance;

namespace Employee_Task_and_Attendance_Management_System.Validators.Attendance
{
    public class CreateAttendanceDtoValidator : AbstractValidator<CreateAttendanceDto>
    {
        public CreateAttendanceDtoValidator()
        {
            RuleFor(x => x.EmployeeId)
                .GreaterThan(0).WithMessage("EmployeeId must be a positive integer.");

            RuleFor(x => x.CheckIn)
                .NotEmpty().WithMessage("CheckIn is required.");

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Status is required.")
                .MaximumLength(20).WithMessage("Status cannot exceed 20 characters.");

            RuleFor(x => x.Date)
                .NotEmpty().WithMessage("Date is required.");
        }
    }
}
