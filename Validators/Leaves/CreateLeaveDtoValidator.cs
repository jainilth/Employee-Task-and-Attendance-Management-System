using FluentValidation;
using Employee_Task_and_Attendance_Management_System.DTOs.Leaves;

namespace Employee_Task_and_Attendance_Management_System.Validators.Leaves
{
    public class CreateLeaveDtoValidator : AbstractValidator<CreateLeaveDto>
    {
        public CreateLeaveDtoValidator()
        {
            RuleFor(x => x.EmployeeId)
                .GreaterThan(0).WithMessage("EmployeeId must be a positive integer.");

            RuleFor(x => x.LeaveType)
                .NotEmpty().WithMessage("LeaveType is required.")
                .MaximumLength(30).WithMessage("LeaveType cannot exceed 30 characters.");

            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage("StartDate is required.");

            RuleFor(x => x.EndDate)
                .NotEmpty().WithMessage("EndDate is required.")
                .GreaterThanOrEqualTo(x => x.StartDate).WithMessage("EndDate must be on or after StartDate.");

            RuleFor(x => x.Reason)
                .MaximumLength(500).WithMessage("Reason cannot exceed 500 characters.");

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Status is required.")
                .MaximumLength(20).WithMessage("Status cannot exceed 20 characters.");
        }
    }
}
