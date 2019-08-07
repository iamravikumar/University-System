﻿namespace LearningSystem.Tests.Mocks
{
    using System;
    using LearningSystem.Services.Admin;
    using LearningSystem.Services.Admin.Models;
    using Moq;

    public static class AdminCourseServiceMock
    {
        public static Mock<IAdminCourseService> GetMock
            => new Mock<IAdminCourseService>();

        public static Mock<IAdminCourseService> CreateAsync(this Mock<IAdminCourseService> mock, int courseId)
        {
            mock.Setup(a => a.CreateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<string>()))
                .ReturnsAsync(courseId)
                .Verifiable();

            return mock;
        }

        public static Mock<IAdminCourseService> GetByIdAsync(this Mock<IAdminCourseService> mock, AdminCourseServiceModel course)
        {
            mock.Setup(a => a.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(course)
                .Verifiable();

            return mock;
        }

        public static Mock<IAdminCourseService> UpdateAsync(this Mock<IAdminCourseService> mock, bool success)
        {
            mock.Setup(a => a.UpdateAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<string>()))
                .ReturnsAsync(success)
                .Verifiable();

            return mock;
        }
    }
}
