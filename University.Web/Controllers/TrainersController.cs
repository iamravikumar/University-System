﻿namespace University.Web.Controllers
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using University.Data.Models;
    using University.Services;
    using University.Web.Infrastructure.Extensions;
    using University.Web.Infrastructure.Helpers;
    using University.Web.Models;
    using University.Web.Models.Courses;
    using University.Web.Models.Trainers;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;

    [Authorize]
    public class TrainersController : Controller
    {
        private readonly UserManager<User> userManager;
        private readonly ICertificateService certificateService;
        private readonly ICourseService courseService;
        private readonly IExamService examService;
        private readonly ITrainerService trainerService;

        public TrainersController(
            UserManager<User> userManager,
            ICertificateService certificateService,
            ICourseService courseService,
            IExamService examService,
            ITrainerService trainerService)
        {
            this.userManager = userManager;
            this.certificateService = certificateService;
            this.courseService = courseService;
            this.examService = examService;
            this.trainerService = trainerService;
        }

        [Authorize(Roles = WebConstants.TrainerRole)]
        public async Task<IActionResult> Index(string search = null, int currentPage = 1)
        {
            var userId = this.userManager.GetUserId(this.User);
            if (userId == null)
            {
                this.TempData.AddErrorMessage(WebConstants.InvalidUserMsg);
                return this.RedirectToAction(nameof(CoursesController.Index), WebConstants.CoursesController);
            }

            var model = await this.GetTrainerCoursesWithSearchAndPagination(userId, search, currentPage, nameof(Index));

            return this.View(model);
        }

        /// <summary>
        /// Trainer's courses with search & pagination for authorized users
        /// </summary>
        /// <param name="id"> NB! Provide trainer Username</param>
        public async Task<IActionResult> Details(string id, string search = null, int currentPage = 1)
        {
            var trainerUsername = id;

            var trainerId = await this.GetTrainerId(trainerUsername);
            if (trainerId == null)
            {
                this.TempData.AddErrorMessage(WebConstants.TrainerNotFoundMsg);
                return this.RedirectToAction(nameof(CoursesController.Index), WebConstants.CoursesController);
            }

            var courses = await this.GetTrainerCoursesWithSearchAndPagination(trainerId, search, currentPage, nameof(Details));
            var profile = await this.trainerService.GetProfileAsync(trainerId);

            var model = new TrainerDetailsViewModel { Courses = courses, Trainer = profile };

            return this.View(model);
        }

        [Authorize(Roles = WebConstants.TrainerRole)]
        public async Task<IActionResult> Resources(int id)
        {
            var courseExists = this.courseService.Exists(id);
            if (!courseExists)
            {
                this.TempData.AddErrorMessage(WebConstants.CourseNotFoundMsg);
                return this.RedirectToAction(nameof(Index));
            }

            var userId = this.userManager.GetUserId(this.User);
            if (userId == null)
            {
                this.TempData.AddErrorMessage(WebConstants.InvalidUserMsg);
                return this.RedirectToAction(nameof(Index));
            }

            var isTrainer = await this.trainerService.IsTrainerForCourseAsync(userId, id);
            if (!isTrainer)
            {
                this.TempData.AddErrorMessage(WebConstants.NotTrainerForCourseMsg);
                return this.RedirectToAction(nameof(Index));
            }

            var model = await this.trainerService.CourseWithResourcesByIdAsync(userId, id);

            return this.View(model);
        }

        [Authorize(Roles = WebConstants.TrainerRole)]
        public async Task<IActionResult> Students(int id)
        {
            var courseExists = this.courseService.Exists(id);
            if (!courseExists)
            {
                this.TempData.AddErrorMessage(WebConstants.CourseNotFoundMsg);
                return this.RedirectToAction(nameof(Index));
            }

            var userId = this.userManager.GetUserId(this.User);
            if (userId == null)
            {
                this.TempData.AddErrorMessage(WebConstants.InvalidUserMsg);
                return this.RedirectToAction(nameof(Index));
            }

            var isTrainer = await this.trainerService.IsTrainerForCourseAsync(userId, id);
            if (!isTrainer)
            {
                this.TempData.AddErrorMessage(WebConstants.NotTrainerForCourseMsg);
                return this.RedirectToAction(nameof(Index));
            }

            var model = new StudentCourseGradeViewModel
            {
                Course = await this.trainerService.CourseByIdAsync(userId, id),
                Students = await this.trainerService.StudentsInCourseAsync(id)
            };

            return this.View(model);
        }

        [Authorize(Roles = WebConstants.TrainerRole)]
        [HttpPost]
        public async Task<IActionResult> EvaluateExam(int id, StudentCourseGradeFormModel model)
        {
            var courseExists = this.courseService.Exists(id);
            if (!courseExists)
            {
                this.TempData.AddErrorMessage(WebConstants.CourseNotFoundMsg);
                return this.RedirectToAction(nameof(Index));
            }

            if (!this.ModelState.IsValid)
            {
                this.TempData.AddErrorMessage(WebConstants.StudentAssessmentErrorMsg);
                return this.RedirectToTrainerStudentsForCourse(id);
            }

            var userId = this.userManager.GetUserId(this.User);
            if (userId == null)
            {
                this.TempData.AddErrorMessage(WebConstants.InvalidUserMsg);
                return this.RedirectToAction(nameof(Index));
            }

            var isTrainer = await this.trainerService.IsTrainerForCourseAsync(userId, id);
            if (!isTrainer)
            {
                this.TempData.AddErrorMessage(WebConstants.NotTrainerForCourseMsg);
                return this.RedirectToAction(nameof(Index));
            }

            var courseHasEnded = await this.trainerService.CourseHasEndedAsync(id);
            if (!courseHasEnded)
            {
                this.TempData.AddErrorMessage(WebConstants.CourseHasNotEndedMsg);
                return this.RedirectToTrainerStudentsForCourse(id);
            }

            var isStudentInCourse = await this.courseService.IsUserEnrolledInCourseAsync(model.CourseId, model.StudentId);
            if (!isStudentInCourse)
            {
                this.TempData.AddErrorMessage(WebConstants.StudentNotEnrolledInCourseMsg);
                return this.RedirectToTrainerStudentsForCourse(id);
            }

            var gradeValue = model.Grade.Value;
            var success = await this.examService.EvaluateAsync(userId, id, model.StudentId, gradeValue);
            if (!success)
            {
                this.TempData.AddErrorMessage(WebConstants.ExamAssessmentErrorMsg);
                return this.RedirectToTrainerStudentsForCourse(id);
            }

            this.TempData.AddSuccessMessage(WebConstants.ExamAssessedMsg);
            await this.CreateCertificate(id, userId, model);

            return this.RedirectToTrainerStudentsForCourse(id);
        }

        [Authorize(Roles = WebConstants.TrainerRole)]
        public async Task<IActionResult> DownloadExam(int id, string studentId)
        {
            var courseExists = this.courseService.Exists(id);
            if (!courseExists)
            {
                this.TempData.AddErrorMessage(WebConstants.CourseNotFoundMsg);
                return this.RedirectToAction(nameof(Index));
            }

            var userId = this.userManager.GetUserId(this.User);
            if (userId == null)
            {
                this.TempData.AddErrorMessage(WebConstants.InvalidUserMsg);
                return this.RedirectToAction(nameof(Index));
            }

            var isTrainer = await this.trainerService.IsTrainerForCourseAsync(userId, id);
            if (!isTrainer)
            {
                this.TempData.AddErrorMessage(WebConstants.NotTrainerForCourseMsg);
                return this.RedirectToAction(nameof(Index));
            }

            var courseHasEnded = await this.trainerService.CourseHasEndedAsync(id);
            if (!courseHasEnded)
            {
                this.TempData.AddErrorMessage(WebConstants.CourseHasNotEndedMsg);
                return this.RedirectToTrainerStudentsForCourse(id);
            }

            var exam = await this.examService.DownloadForTrainerAsync(userId, id, studentId);
            if (exam == null)
            {
                this.TempData.AddErrorMessage(WebConstants.StudentHasNotSubmittedExamMsg);
                return this.RedirectToTrainerStudentsForCourse(id);
            }

            var fileName = FileHelpers.ExamFileName(exam.CourseName, exam.StudentUserName, exam.SubmissionDate);

            return this.File(exam.FileSubmission, WebConstants.ApplicationZip, fileName);
        }

        private async Task CreateCertificate(int courseId, string trainerId, StudentCourseGradeFormModel model)
        {
            if (this.certificateService.IsGradeEligibleForCertificate(model.Grade))
            {
                var success = await this.certificateService.CreateAsync(trainerId, courseId, model.StudentId, model.Grade.Value);
                if (success)
                {
                    this.TempData.AddSuccessMessage(WebConstants.ExamAssessedMsg + Environment.NewLine + WebConstants.CertificateIssuedMsg);
                }
            }
        }

        private async Task<CoursePageListingViewModel> GetTrainerCoursesWithSearchAndPagination(string trainerId, string search, int currentPage, string action = nameof(Index))
        {
            var pagination = new PaginationViewModel
            {
                SearchTerm = search,
                Action = action,
                RequestedPage = currentPage,
                TotalItems = await this.trainerService.TotalCoursesAsync(trainerId, search)
            };

            var courses = await this.trainerService.CoursesAsync(trainerId, search, pagination.CurrentPage, WebConstants.PageSize);

            var model = new CoursePageListingViewModel
            {
                Courses = courses,
                Pagination = pagination,
                Search = new SearchViewModel { SearchTerm = search, Placeholder = WebConstants.SearchByCourseName }
            };

            return model;
        }

        private async Task<string> GetTrainerId(string trainerUsername)
            => (await this.userManager.GetUsersInRoleAsync(WebConstants.TrainerRole))
            .Where(u => u.UserName == trainerUsername)
            .Select(u => u.Id)
            .FirstOrDefault();

        private IActionResult RedirectToTrainerStudentsForCourse(int courseId)
            => this.RedirectToAction(nameof(Students), routeValues: new { id = courseId });
    }
}