﻿namespace LearningSystem.Data.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Microsoft.AspNetCore.Identity;

    public class User : IdentityUser, IValidatableObject
    {
        [Required]
        [StringLength(DataConstants.UserNameMaxLength,
            ErrorMessage = DataConstants.StringMinMaxLength,
            MinimumLength = DataConstants.UserNameMinLength)]
        public string Name { get; set; }

        [DataType(DataType.Date)]
        public DateTime Birthdate { get; set; }

        public ICollection<StudentCourse> Courses { get; set; } = new List<StudentCourse>();

        public ICollection<Course> Trainings { get; set; } = new List<Course>();

        public ICollection<Article> Articles { get; set; } = new List<Article>();

        public ICollection<ExamSubmission> ExamSubmissions { get; set; } = new List<ExamSubmission>();

        public ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();

        public ICollection<Order> Orders { get; set; } = new List<Order>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var isFutureDate = DateTime.Now.Date < this.Birthdate.ToLocalTime().Date;
            if (isFutureDate)
            {
                yield return new ValidationResult(DataConstants.UserBirthdate,
                    new[] { nameof(this.Birthdate) });
            }
        }
    }
}
