﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;
using Picassi.Core.Accounts.DbAccess.Categories;
using Picassi.Core.Accounts.Reports;
using Picassi.Core.Accounts.ViewModels;
using Picassi.Core.Accounts.ViewModels.Categories;
using Picassi.Utils.Api.Attributes;

namespace Picassi.Api.Accounts.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [PicassiApiAuthorise]
    public class CategoriesController : ApiController
    {
        private readonly ICategoryCrudService _crudService;
        private readonly ICategoryQueryService _queryService;
        private readonly ICategorySummaryService _categorySummaryService;

        public CategoriesController(ICategoryCrudService crudService, ICategoryQueryService queryService, ICategorySummaryService categorySummaryService)
        {
            _crudService = crudService;
            _queryService = queryService;
            _categorySummaryService = categorySummaryService;
        }

        [HttpGet]
        [Route("categories")]
        public IEnumerable<CategoryViewModel> GetCategories([FromUri]CategoriesQueryModel query)
        {
            return _queryService.Query(query);
        }

        [HttpGet]
        [Route("categories/summary")]
        public ResultsViewModel<CategorySummaryViewModel> GetCategoriesSumary([FromUri]CategoriesQueryModel query)
        {
            return _categorySummaryService.GetCategorySummaries(query);
        }

        [HttpPost]
        [Route("categories")]
        public CategoryViewModel CreateCategory([FromBody]CategoryViewModel model)
        {
            if (string.IsNullOrEmpty(model.Name))
            {
                throw new InvalidOperationException("Cannot create category with empty name");
            }
            var existingCategory = _queryService.Query(new CategoriesQueryModel { Name = model.Name }).FirstOrDefault();
            return existingCategory ?? _crudService.CreateCategory(model);
        }

        [HttpGet]
        [Route("categories/{id}")]
        public CategoryViewModel GetCategory(int id)
        {
            return _crudService.GetCategory(id);
        }

        [HttpPut]
        [Route("categories/{id}")]
        public CategoryViewModel UpdateCategory(int id, [FromBody]CategoryViewModel CategoryViewModel)
        {
            return _crudService.UpdateCategory(id, CategoryViewModel);
        }

        [HttpDelete]
        [Route("categories/{id}")]
        public bool DeleteAccount(int id)
        {
            return _crudService.DeleteCategory(id);
        }
    }
}