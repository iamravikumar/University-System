﻿namespace LearningSystem.Tests.Web.Controllers
{
    using System.Threading.Tasks;
    using LearningSystem.Tests.Mocks;
    using LearningSystem.Web.Controllers;
    using LearningSystem.Web.Models.Courses;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Xunit;

    public class HomeControllerTest
    {
        private const string TestSearchTerm = "TestSearchTerm";
        private const int TestTotalItems = 120;

        [Fact]
        public void HomeController_ShouldBeForAllUsers()
        {
            // Act
            var attributes = typeof(HomeController).GetCustomAttributes(true);

            // Assert
            Assert.DoesNotContain(attributes, a => a.GetType() == typeof(AuthorizeAttribute));
        }

        [Theory]
        [InlineData(int.MinValue)] // invalid
        [InlineData(1)] // first
        [InlineData(5)]
        [InlineData(10)] // last
        [InlineData(int.MaxValue)] // invalid
        public async Task Index_ShouldReturnViewResultWithCorrectModel(int requestedPage)
        {
            // Arrange
            var testPagination = Tests.GetPaginationViewModel(requestedPage, TestTotalItems, TestSearchTerm);

            var courseService = CourseServiceMock.GetMock;
            courseService
                .TotalActiveAsync(TestTotalItems)
                .AllActiveWithTrainersAsync(Tests.GetCourseServiceModelCollection());

            var controller = new HomeController(courseService.Object);

            // Act
            var result = await controller.Index(TestSearchTerm, requestedPage);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CoursePageListingViewModel>(viewResult.Model);

            Assert.NotNull(model);
            Tests.AssertCourseServiceModelCollection(model.Courses);
            Tests.AssertSearchViewModel(TestSearchTerm, model.Search);
            Tests.AssertPagination(testPagination, model.Pagination);

            courseService.Verify();
        }
    }
}
