using FluentValidation;
using Employee_Task_and_Attendance_Management_System.DTOs.Tasks;

namespace Employee_Task_and_Attendance_Management_System.Validators.Tasks
{
    public class UpdateTaskDtoValidator : AbstractValidator<UpdateTaskDto>
    {
        public UpdateTaskDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.");

            RuleFor(x => x.AssignedBy)
                .GreaterThan(0).WithMessage("AssignedBy must be a positive integer.");

            RuleFor(x => x.Priority)
                .NotEmpty().WithMessage("Priority is required.")
                .MaximumLength(20).WithMessage("Priority cannot exceed 20 characters.");

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Status is required.")
                .MaximumLength(20).WithMessage("Status cannot exceed 20 characters.");
        }
    }
}
