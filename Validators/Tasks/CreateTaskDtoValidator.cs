using FluentValidation;
using Employee_Task_and_Attendance_Management_System.DTOs.Tasks;

namespace Employee_Task_and_Attendance_Management_System.Validators.Tasks
{
    public class CreateTaskDtoValidator : AbstractValidator<CreateTaskDto>
    {
        public CreateTaskDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.");

            RuleFor(x => x.Priority)
                .NotEmpty().WithMessage("Priority is required.")
                .MaximumLength(20).WithMessage("Priority cannot exceed 20 characters.");
        }
    }
}
