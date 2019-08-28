﻿namespace University.Tests.Web.Controllers
{
    using System.Linq;
    using System.Threading.Tasks;
    using University.Services.Models.Resources;
    using University.Tests.Mocks;
    using University.Web;
    using University.Web.Controllers;
    using University.Web.Models.Resources;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Xunit;

    public class ResourcesControllerTest
    {
        private const int TestCourseId = 1;
        private const int TestResourceId = 10;
        private const string TestUserId = "TestUserId";
        private const string TestContentType = "application/zip";
        private const string TestFileName = "TestFileName";

        [Fact]
        public void ResourcesController_ShouldBeForAuthorizedUsersOnly()
        {
            // Act
            var authorizeAttribute = typeof(ResourcesController)
                .GetCustomAttributes(true)
                .FirstOrDefault(a => a.GetType() == typeof(AuthorizeAttribute))
                as AuthorizeAttribute;

            // Assert
            Assert.NotNull(authorizeAttribute);
        }

        [Fact]
        public void Create_ShouldBeForAuthorizedTrainersOnly()
            => this.AssertAuthorizeAttributeForRoleTrainer(nameof(ResourcesController.Create));

        [Fact]
        public void Create_ShouldHaveHpptPostAttribute()
            => this.AssertHttpPostAttribute(nameof(ResourcesController.Create));

        [Fact]
        public async Task Create_ShouldRedirectToTrainersResources_GivenInvalidFormModel()
        {
            // Arrange
            var controller = new ResourcesController(
                userManager: null,
                courseService: null,
                resourceService: null,
                trainerService: null)
            {
                TempData = TempDataMock.GetMock
            };

            using (controller)
            {
                controller.ModelState.AddModelError(string.Empty, "Error");

                // Act
                var result = await controller.Create(TestCourseId, null);

                // Assert
                controller.TempData.AssertErrorMsg(WebConstants.FileInvalidMsg);

                this.AssertRedirectToTrainersResourcesWithRouteId(result);
            }
        }

        [Fact]
        public async Task Create_ShouldRedirectToTrainersIndex_GivenMismatchingCourseAndModel()
        {
            // Arrange
            var testModel = GetResourceCreateModel();

            var courseService = CourseServiceMock.GetMock;

            var controller = new ResourcesController(
                userManager: null,
                courseService.Object,
                resourceService: null,
                trainerService: null)
            {
                TempData = TempDataMock.GetMock
            };

            using (controller)
            {
                // Act
                var result = await controller.Create(TestCourseId + 1, testModel);

                // Assert
                controller.TempData.AssertErrorMsg(WebConstants.CourseInvalidMsg);

                this.AssertRedirectToTrainersIndex(result);

                courseService.Verify();
            }
        }

        [Fact]
        public async Task Create_ShouldRedirectToTrainersIndex_GivenInvalidCourse()
        {
            // Arrange
            var testModel = GetResourceCreateModel();

            var courseService = CourseServiceMock.GetMock;
            courseService.Exists(false);

            var controller = new ResourcesController(
                userManager: null,
                courseService.Object,
                resourceService: null,
                trainerService: null)
            {
                TempData = TempDataMock.GetMock
            };

            using (controller)
            {
                // Act
                var result = await controller.Create(TestCourseId, testModel);

                // Assert
                controller.TempData.AssertErrorMsg(WebConstants.CourseNotFoundMsg);

                this.AssertRedirectToTrainersIndex(result);

                courseService.Verify();
            }
        }

        [Fact]
        public async Task Create_ShouldRedirectToTrainersIndex_GivenInvalidUser()
        {
            // Arrange
            var testModel = GetResourceCreateModel();

            var courseService = CourseServiceMock.GetMock;
            courseService.Exists(true);

            var userManager = UserManagerMock.GetMock;
            userManager.GetUserId(null);

            var controller = new ResourcesController(
                userManager.Object,
                courseService.Object,
                resourceService: null,
                trainerService: null)
            {
                TempData = TempDataMock.GetMock
            };

            using (controller)
            {
                // Act
                var result = await controller.Create(TestCourseId, testModel);

                // Assert
                controller.TempData.AssertErrorMsg(WebConstants.InvalidUserMsg);

                this.AssertRedirectToTrainersIndex(result);

                courseService.Verify();
                userManager.Verify();
            }
        }

        [Fact]
        public async Task Create_ShouldRedirectToTrainersIndex_GivenInvalidTrainer()
        {
            // Arrange
            var testModel = GetResourceCreateModel();

            var courseService = CourseServiceMock.GetMock;
            courseService.Exists(true);

            var userManager = UserManagerMock.GetMock;
            userManager.GetUserId(TestUserId);

            var trainerService = TrainerServiceMock.GetMock;
            trainerService.IsTrainerForCourseAsync(false);

            var controller = new ResourcesController(
                userManager.Object,
                courseService.Object,
                resourceService: null,
                trainerService.Object)
            {
                TempData = TempDataMock.GetMock
            };

            using (controller)
            {
                // Act
                var result = await controller.Create(TestCourseId, testModel);

                // Assert
                controller.TempData.AssertErrorMsg(WebConstants.NotTrainerForCourseMsg);

                this.AssertRedirectToTrainersIndex(result);

                courseService.Verify();
                userManager.Verify();
                trainerService.Verify();
            }
        }

        [Fact]
        public async Task Create_ShouldRedirectToTrainersResourcesWithErrorMsg_GivenCreateError()
        {
            // Arrange
            var testModel = GetResourceCreateModel();

            var courseService = CourseServiceMock.GetMock;
            courseService.Exists(true);

            var userManager = UserManagerMock.GetMock;
            userManager.GetUserId(TestUserId);

            var trainerService = TrainerServiceMock.GetMock;
            trainerService.IsTrainerForCourseAsync(true);

            var resourceService = ResourceServiceMock.GetMock;
            resourceService.CreateAsync(false);

            var controller = new ResourcesController(
                userManager.Object,
                courseService.Object,
                resourceService.Object,
                trainerService.Object)
            {
                TempData = TempDataMock.GetMock
            };

            using (controller)
            {
                // Act
                var result = await controller.Create(TestCourseId, testModel);

                // Assert
                controller.TempData.AssertErrorMsg(WebConstants.ResourceFileUploadErrorMsg);

                this.AssertRedirectToTrainersResourcesWithRouteId(result);

                courseService.Verify();
                userManager.Verify();
                trainerService.Verify();
                resourceService.Verify();
            }
        }

        [Fact]
        public async Task Create_ShouldRedirectToTrainersResourcesWithSuccessMsg_GivenCreateSuccess()
        {
            // Arrange
            var testModel = GetResourceCreateModel();

            var courseService = CourseServiceMock.GetMock;
            courseService.Exists(true);

            var userManager = UserManagerMock.GetMock;
            userManager.GetUserId(TestUserId);

            var trainerService = TrainerServiceMock.GetMock;
            trainerService.IsTrainerForCourseAsync(true);

            var resourceService = ResourceServiceMock.GetMock;
            resourceService.CreateAsync(true);

            var controller = new ResourcesController(
                userManager.Object,
                courseService.Object,
                resourceService.Object,
                trainerService.Object)
            {
                TempData = TempDataMock.GetMock
            };

            using (controller)
            {
                // Act
                var result = await controller.Create(TestCourseId, testModel);

                // Assert
                controller.TempData.AssertSuccessMsg(WebConstants.ResourceCreatedMsg);

                this.AssertRedirectToTrainersResourcesWithRouteId(result);

                courseService.Verify();
                userManager.Verify();
                trainerService.Verify();
                resourceService.Verify();
            }
        }

        [Fact]
        public void Delete_ShouldBeForAuthorizedTrainersOnly()
            => this.AssertAuthorizeAttributeForRoleTrainer(nameof(ResourcesController.Delete));

        [Fact]
        public void Delete_ShouldHaveHpptPostAttribute()
            => this.AssertHttpPostAttribute(nameof(ResourcesController.Delete));

        [Fact]
        public async Task Delete_ShouldRedirectToTrainersIndex_GivenInvalidUser()
        {
            // Arrange
            var testModel = this.GetResource();

            var userManager = UserManagerMock.GetMock;
            userManager.GetUserId(null);

            var controller = new ResourcesController(
                userManager.Object,
                courseService: null,
                resourceService: null,
                trainerService: null)
            {
                TempData = TempDataMock.GetMock
            };

            using (controller)
            {
                // Act
                var result = await controller.Delete(TestResourceId, testModel);

                // Assert
                controller.TempData.AssertErrorMsg(WebConstants.InvalidUserMsg);

                this.AssertRedirectToTrainersIndex(result);

                userManager.Verify();
            }
        }

        [Fact]
        public async Task Delete_ShouldRedirectToTrainersIndex_GivenInvalidTrainer()
        {
            // Arrange
            var testModel = this.GetResource();

            var userManager = UserManagerMock.GetMock;
            userManager.GetUserId(TestUserId);

            var trainerService = TrainerServiceMock.GetMock;
            trainerService.IsTrainerForCourseAsync(false);

            var controller = new ResourcesController(
                userManager.Object,
                courseService: null,
                resourceService: null,
                trainerService.Object)
            {
                TempData = TempDataMock.GetMock
            };

            using (controller)
            {
                // Act
                var result = await controller.Delete(TestCourseId, testModel);

                // Assert
                controller.TempData.AssertErrorMsg(WebConstants.NotTrainerForCourseMsg);

                this.AssertRedirectToTrainersIndex(result);

                userManager.Verify();
                trainerService.Verify();
            }
        }

        [Fact]
        public async Task Delete_ShouldRedirectToTrainersResources_GivenInvalidResource()
        {
            // Arrange
            var testModel = this.GetResource();

            var userManager = UserManagerMock.GetMock;
            userManager.GetUserId(TestUserId);

            var trainerService = TrainerServiceMock.GetMock;
            trainerService.IsTrainerForCourseAsync(true);

            var resourceService = ResourceServiceMock.GetMock;
            resourceService.Exists(false);

            var controller = new ResourcesController(
                userManager.Object,
                courseService: null,
                resourceService.Object,
                trainerService.Object)
            {
                TempData = TempDataMock.GetMock
            };

            using (controller)
            {
                // Act
                var result = await controller.Delete(TestCourseId, testModel);

                // Assert
                controller.TempData.AssertErrorMsg(WebConstants.ResourceNotFoundMsg);

                this.AssertRedirectToTrainersResourcesWithRouteId(result);

                userManager.Verify();
                trainerService.Verify();
                resourceService.Verify();
            }
        }

        [Fact]
        public async Task Delete_ShouldRedirectToTrainersResourcesWithErrorMsg_GivenRemoveError()
        {
            // Arrange
            var testModel = this.GetResource();

            var userManager = UserManagerMock.GetMock;
            userManager.GetUserId(TestUserId);

            var trainerService = TrainerServiceMock.GetMock;
            trainerService.IsTrainerForCourseAsync(true);

            var resourceService = ResourceServiceMock.GetMock;
            resourceService
                .Exists(true)
                .RemoveAsync(false);

            var controller = new ResourcesController(
                userManager.Object,
                courseService: null,
                resourceService.Object,
                trainerService.Object)
            {
                TempData = TempDataMock.GetMock
            };

            using (controller)
            {
                // Act
                var result = await controller.Delete(TestCourseId, testModel);

                // Assert
                controller.TempData.AssertErrorMsg(WebConstants.ResourceNotDeletedMsg);

                this.AssertRedirectToTrainersResourcesWithRouteId(result);

                userManager.Verify();
                trainerService.Verify();
                resourceService.Verify();
            }
        }

        [Fact]
        public async Task Delete_ShouldRedirectToTrainersResourcesWithSuccessMsg_GivenRemoveSuccess()
        {
            // Arrange
            var testModel = this.GetResource();

            var userManager = UserManagerMock.GetMock;
            userManager.GetUserId(TestUserId);

            var trainerService = TrainerServiceMock.GetMock;
            trainerService.IsTrainerForCourseAsync(true);

            var resourceService = ResourceServiceMock.GetMock;
            resourceService
                .Exists(true)
                .RemoveAsync(true);

            var controller = new ResourcesController(
                userManager.Object,
                courseService: null,
                resourceService.Object,
                trainerService.Object)
            {
                TempData = TempDataMock.GetMock
            };

            using (controller)
            {
                // Act
                var result = await controller.Delete(TestCourseId, testModel);

                // Assert
                controller.TempData.AssertSuccessMsg(WebConstants.ResourceDeletedMsg);

                this.AssertRedirectToTrainersResourcesWithRouteId(result);

                userManager.Verify();
                trainerService.Verify();
                resourceService.Verify();
            }
        }

        [Fact]
        public async Task Download_ShouldRedirectToCourseIndex_GivenInvalidUser()
        {
            // Arrange
            var userManager = UserManagerMock.GetMock;
            userManager.GetUserId(null);

            var controller = new ResourcesController(
                userManager.Object,
                courseService: null,
                resourceService: null,
                trainerService: null)
            {
                TempData = TempDataMock.GetMock
            };

            using (controller)
            {
                // Act
                var result = await controller.Download(TestResourceId);

                // Assert
                controller.TempData.AssertErrorMsg(WebConstants.InvalidUserMsg);

                this.AssertRedirectToCoursesIndex(result);

                userManager.Verify();
            }
        }

        [Fact]
        public async Task Download_ShouldRedirectToCourseIndex_GivenInvalidResource()
        {
            // Arrange
            var userManager = UserManagerMock.GetMock;
            userManager.GetUserId(TestUserId);

            var resourceService = ResourceServiceMock.GetMock;
            resourceService.Exists(false);

            var controller = new ResourcesController(
                userManager.Object,
                courseService: null,
                resourceService.Object,
                trainerService: null)
            {
                TempData = TempDataMock.GetMock
            };

            using (controller)
            {
                // Act
                var result = await controller.Download(TestResourceId);

                // Assert
                controller.TempData.AssertErrorMsg(WebConstants.ResourceNotFoundMsg);

                this.AssertRedirectToCoursesIndex(result);

                userManager.Verify();
                resourceService.Verify();
            }
        }

        [Fact]
        public async Task Download_ShouldRedirectToCourseIndex_GivenInvalidTrainerOrStudent()
        {
            // Arrange
            var userManager = UserManagerMock.GetMock;
            userManager.GetUserId(TestUserId);

            var resourceService = ResourceServiceMock.GetMock;
            resourceService
                .Exists(true)
                .CanBeDownloadedByUser(false);

            var controller = new ResourcesController(
                userManager.Object,
                courseService: null,
                resourceService.Object,
                trainerService: null)
            {
                TempData = TempDataMock.GetMock
            };

            using (controller)
            {
                // Act
                var result = await controller.Download(TestResourceId);

                // Assert
                controller.TempData.AssertErrorMsg(WebConstants.ResourceDownloadUnauthorizedMsg);

                this.AssertRedirectToCoursesIndex(result);

                userManager.Verify();
                resourceService.Verify();
            }
        }

        [Fact]
        public async Task Download_ShouldRedirectToCourseIndexWithErrorMsg_GivenDownloadError()
        {
            // Arrange
            var userManager = UserManagerMock.GetMock;
            userManager.GetUserId(TestUserId);

            var resourceService = ResourceServiceMock.GetMock;
            resourceService
                .Exists(true)
                .CanBeDownloadedByUser(true)
                .DownloadAsync(null);

            var controller = new ResourcesController(
                userManager.Object,
                courseService: null,
                resourceService.Object,
                trainerService: null)
            {
                TempData = TempDataMock.GetMock
            };

            using (controller)
            {
                // Act
                var result = await controller.Download(TestResourceId);

                // Assert
                controller.TempData.AssertErrorMsg(WebConstants.ResourceDownloadErrorMsg);

                this.AssertRedirectToCoursesIndex(result);

                userManager.Verify();
                resourceService.Verify();
            }
        }

        [Fact]
        public async Task Download_ShouldRedirectToCourseDetailsWithSuccessMsg_GivenDownloadSuccess()
        {
            // Arrange
            var testResourceDload = this.GetResourceDownload();

            var userManager = UserManagerMock.GetMock;
            userManager.GetUserId(TestUserId);

            var resourceService = ResourceServiceMock.GetMock;
            resourceService
                .Exists(true)
                .CanBeDownloadedByUser(true)
                .DownloadAsync(testResourceDload);

            var controller = new ResourcesController(
                userManager.Object,
                courseService: null,
                resourceService.Object,
                trainerService: null);

            using (controller)
            {
                // Act
                var result = await controller.Download(TestResourceId);

                // Assert
                this.AssertResourceDownload(result);

                userManager.Verify();
                resourceService.Verify();
            }
        }

        private void AssertAuthorizeAttributeForRoleTrainer(string methodName)
        {
            // Act
            var method = typeof(ResourcesController).GetMethod(methodName);

            var authorizeAttribute = method
                .GetCustomAttributes(true)
                .FirstOrDefault(a => a.GetType() == typeof(AuthorizeAttribute))
                as AuthorizeAttribute;

            // Assert
            Assert.NotNull(authorizeAttribute);
            Assert.Equal(WebConstants.TrainerRole, authorizeAttribute.Roles);
        }

        private void AssertHttpPostAttribute(string methodName)
        {
            // Act
            var method = typeof(ResourcesController).GetMethod(methodName);

            var httpPostAttribute = method
                .GetCustomAttributes(true)
                .FirstOrDefault(a => a.GetType() == typeof(HttpPostAttribute))
                as HttpPostAttribute;

            // Assert
            Assert.NotNull(httpPostAttribute);
        }

        private RedirectToActionResult AssertRedirectToAction(IActionResult result)
            => Assert.IsType<RedirectToActionResult>(result);

        private void AssertRedirectToCoursesIndex(IActionResult result)
        {
            var redirectToActionResult = this.AssertRedirectToAction(result);
            Assert.Equal(nameof(CoursesController.Index), redirectToActionResult.ActionName);
            Assert.Equal(WebConstants.CoursesController, redirectToActionResult.ControllerName);
        }

        private void AssertRedirectToTrainersIndex(IActionResult result)
        {
            var redirectToActionResult = this.AssertRedirectToAction(result);
            Assert.Equal(nameof(TrainersController.Courses), redirectToActionResult.ActionName);
            Assert.Equal(WebConstants.TrainersController, redirectToActionResult.ControllerName);
        }

        private void AssertRedirectToTrainersResourcesWithRouteId(IActionResult result)
        {
            var redirectToActionResult = this.AssertRedirectToAction(result);

            Assert.Equal(nameof(TrainersController.Resources), redirectToActionResult.ActionName);
            Assert.Equal(WebConstants.TrainersController, redirectToActionResult.ControllerName);

            Assert.NotNull(redirectToActionResult.RouteValues);
            Assert.Contains(redirectToActionResult.RouteValues.Keys, k => k == WebConstants.Id);
            Assert.Equal(TestCourseId, redirectToActionResult.RouteValues[WebConstants.Id]);
        }

        private void AssertResourceDownload(IActionResult result)
        {
            var expected = this.GetResourceDownload();

            var fileContentResult = Assert.IsType<FileContentResult>(result);

            Assert.Equal(expected.FileBytes, fileContentResult.FileContents);
            Assert.Equal(expected.ContentType, fileContentResult.ContentType);
            Assert.Equal(expected.FileName, fileContentResult.FileDownloadName);
        }

        private byte[] GetFileBytes() => new byte[] { 158, 201, 3, 7 };

        private ResourceFormModel GetResource()
            => new ResourceFormModel { Id = TestResourceId, CourseId = TestCourseId };

        private static ResourceCreateFormModel GetResourceCreateModel()
            => new ResourceCreateFormModel
            {
                CourseId = TestCourseId,
                ResourceFile = IFormFileMock.GetMock.Object
            };

        private ResourceDownloadServiceModel GetResourceDownload()
            => new ResourceDownloadServiceModel
            {
                FileName = TestFileName,
                ContentType = TestContentType,
                FileBytes = this.GetFileBytes()
            };
    }
}
