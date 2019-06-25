﻿namespace LearningSystem.Web.Areas.Blog.Controllers
{
    using System.Threading.Tasks;
    using LearningSystem.Data.Models;
    using LearningSystem.Services.Blog;
    using LearningSystem.Web.Areas.Blog.Models.Articles;
    using LearningSystem.Web.Infrastructure.Extensions;
    using LearningSystem.Web.Infrastructure.Filters;
    using LearningSystem.Web.Models;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;

    [Area(WebConstants.BlogArea)]
    [Authorize]
    public class ArticlesController : Controller
    {
        private readonly UserManager<User> userManager;
        private readonly IArticleService articleService;

        public ArticlesController(
            UserManager<User> userManager,
            IArticleService articleService)
        {
            this.userManager = userManager;
            this.articleService = articleService;
        }

        public async Task<IActionResult> Index(string search = null, int currentPage = 1)
        {
            var pagination = new PaginationModel
            {
                SearchTerm = search,
                Action = nameof(Index),
                RequestedPage = currentPage,
                TotalItems = await this.articleService.TotalAsync(search)
            };

            var articles = await this.articleService.AllAsync(search, pagination.CurrentPage, WebConstants.PageSize);

            var model = new ArticlePageListingViewModel
            {
                Articles = articles,
                Pagination = pagination,
                Search = new SearchModel { SearchTerm = search, Placeholder = WebConstants.SearchByArticleTitleOrContent }
            };

            return this.View(model);
        }

        [Authorize(Roles = WebConstants.BlogAuthorRole)]
        public IActionResult Create() => this.View();

        [Authorize(Roles = WebConstants.BlogAuthorRole)]
        [HttpPost]
        [ValidateModelState] // attribute simple model validation
        public async Task<IActionResult> Create(ArticleFormModel model)
        {
            //if (!this.ModelState.IsValid)
            //{
            //    return this.View(model);
            //}

            var userId = this.userManager.GetUserId(this.User);
            if (userId == null)
            {
                this.TempData.AddErrorMessage(WebConstants.InvalidUserMsg);
                return this.View(model);
            }

            // Raw Html Content sanitized in service
            await this.articleService.CreateAsync(model.Title, model.Content, userId);

            this.TempData.AddSuccessMessage(WebConstants.ArticlePublishedMsg);

            return this.RedirectToAction(nameof(Index));
        }

        // SEO friendly URL
        [Route(WebConstants.BlogArea + "/" + WebConstants.ArticlesController + "/{id}/{title?}")]
        public async Task<IActionResult> Details(int id)
        {
            var model = await this.articleService.GetByIdAsync(id);
            if (model == null)
            {
                this.TempData.AddErrorMessage(WebConstants.ArticleNotFoundMsg);
                return this.RedirectToAction(nameof(Index));
            }

            return this.View(model);
        }
    }
}