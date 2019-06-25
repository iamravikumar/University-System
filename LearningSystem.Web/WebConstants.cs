﻿namespace LearningSystem.Web
{
    using System.Collections.Generic;
    using LearningSystem.Web.Models;

    public class WebConstants
    {
        // Roles
        public const string AdministratorRole = "Administrator";
        public const string BlogAuthorRole = "BlogAuthor";
        public const string TrainerRole = "Trainer";
        // Student Role => default role for registered users

        // Admin
        public const string AdminEmail = "admin@admin.com";
        public const string AdminUsername = "admin";

        // Secret Manager Keys
        public const string AdminPassword = "AdminPassword";
        public const string AuthFacebookAppId = "Authentication:Facebook:AppId";
        public const string AuthFacebookAppSecret = "Authentication:Facebook:AppSecret";
        public const string AuthGoogleClientId = "Authentication:Google:ClientId";
        public const string AuthGoogleClientSecret = "Authentication:Google:ClientSecret";

        // TempData Keys
        public const string TempDataErrorMessageKey = "ErrorMessage";
        public const string TempDataInfoMessageKey = "InfoMessage";
        public const string TempDataSuccessMessageKey = "SuccessMessage";

        // Areas
        public const string AdminArea = "Admin";
        public const string BlogArea = "Blog";

        // Controllers
        public const string ArticlesController = "Articles";
        public const string CoursesController = "Courses";
        public const string HomeController = "Home";
        public const string TrainersController = "Trainers";
        public const string UsersController = "Users";

        // Actions
        public const string Index = "Index";

        // Pagination
        public const int PageSize = 12;

        // Bootstrap Styles
        public const string CreateStyle = "success";
        public const string DeleteStyle = "danger";
        public const string EditStyle = "warning";
        public const string ErrorStyle = "danger";
        public const string InfoStyle = "info";
        public const string PrimaryStyle = "primary";
        public const string SuccessStyle = "success";

        // Notifications
        public const string ArticleNotFoundMsg = "Article not found.";
        public const string ArticlePublishedMsg = "Article published successfully.";
        public const string FormErrorsMsg = "Review the form for errors.";
        public const string CourseCreatedMsg = "Course created successfully.";
        public const string CourseDeletedMsg = "Course deleted successfully.";
        public const string CourseEnrollmentClosedMsg = "Course is closed for enrollment after start date.";
        public const string CourseEnrollmentOpenMsg = "Course is open for enrollment for {0}."; // days/hours
        public const string CourseHasNotEndedMsg = "Course has not ended.";
        public const string CourseNotFoundMsg = "Course not found.";
        public const string CourseUpdatedMsg = "Course updated successfully.";
        public const string ExternalLoginInfoErrorMsg = "Unable to load external login info.";
        public const string Disable2faErrorMsg = "Cannot disable 2FA for user.";
        public const string InvalidIdentityOrRoleMsg = "Invalid identity or role.";
        public const string InvalidUserMsg = "Invalid user.";
        public const string NotCourseTrainerMsg = "Not authorized as trainer for this course.";
        public const string StudentAssessmentErrorMsg = "Student assessment error.";
        public const string StudentNotEnrolledInCourseMsg = "Student not enrolled in this course.";
        public const string TrainersAssessPerformanceAfterCourseEndsMsg = "Trainers can assess students' course performance only when the course is over.";
        public const string UserAvddedToRoleMsg = "User {0} added to role {1}.";
        public const string UserAlreadyEnrolledInCourseMsg = "User already enrolled in this course.";
        public const string UserCancelledEnrollmentInCourseMsg = "Course enrollment cancelled successfully.";
        public const string UserCannotBeDeletedMsg = "User cannot be deleted. Delete user's blog articles & remove user as trainer from all courses first.";
        public const string UserEnrolledInCourseMsg = "Enrolled in course successfully.";
        public const string UserNotEnrolledInCourseMsg = "User not enrolled in this course.";
        public const string UserAddedToRoleMsg = "User {0} added to role {1}.";
        public const string UserRemovedFromRoleMsg = "User {0} removed from role {1}.";

        public static Dictionary<string, string> Styles;

        static WebConstants()
        {
            Styles = new Dictionary<string, string>
            {
                [TempDataErrorMessageKey] = ErrorStyle,
                [TempDataInfoMessageKey] = InfoStyle,
                [TempDataSuccessMessageKey] = SuccessStyle,
                [FormActionEnum.Add.ToString()] = CreateStyle,
                [FormActionEnum.Cancel.ToString()] = DeleteStyle,
                [FormActionEnum.Create.ToString()] = CreateStyle,
                [FormActionEnum.Default.ToString()] = PrimaryStyle,
                [FormActionEnum.Delete.ToString()] = DeleteStyle,
                [FormActionEnum.Edit.ToString()] = EditStyle,
                [FormActionEnum.Enroll.ToString()] = CreateStyle,
                [FormActionEnum.Grade.ToString()] = SuccessStyle,
                [FormActionEnum.Remove.ToString()] = DeleteStyle,
            };
        }
    }
}
