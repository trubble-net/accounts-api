﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;
using Picassi.Core.Accounts.DAL.Services;
using Picassi.Core.Accounts.Models;
using Picassi.Core.Accounts.Models.Categories;
using Picassi.Core.Accounts.Services.Reports;
using Picassi.Utils.Api.Attributes;

namespace Picassi.Api.Accounts.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [PicassiApiAuthorise]
    public class CategoriesController : ApiController
    {
        private readonly ICategoriesDataService _dataService;
        private readonly ICategorySummariser _categorySummariser;

        public CategoriesController(ICategoriesDataService dataService, ICategorySummariser categorySummariser)
        {
            _dataService = dataService;
            _categorySummariser = categorySummariser;
        }

        [HttpGet]
        [Route("categories")]
        public IEnumerable<CategoryModel> GetCategories([FromUri]CategoriesQueryModel query)
        {
            return _dataService.Query(query);
        }

        [HttpGet]
        [Route("categories/summary")]
        public IEnumerable<CategorySummaryViewModel> GetCategoriesSumary([FromUri]CategoriesQueryModel query)
        {
            return _categorySummariser.GetCategorySummaries(query?.DateFrom, query?.DateTo);
        }

        [HttpPost]
        [Route("categories")]
        public CategoryModel CreateCategory([FromBody]CategoryModel model)
        {
            if (string.IsNullOrEmpty(model?.Name))
            {
                throw new InvalidOperationException("Cannot create category with empty name");
            }
            var existingCategory = _dataService.Query(new CategoriesQueryModel { Name = model.Name }).FirstOrDefault();
            return existingCategory ?? _dataService.Create(model);
        }

        [HttpGet]
        [Route("categories/{id}")]
        public CategoryModel GetCategory(int id)
        {
            return _dataService.Get(id);
        }

        [HttpPut]
        [Route("categories/{id}")]
        public CategoryModel UpdateCategory(int id, [FromBody]CategoryModel categoryModel)
        {
            return _dataService.Update(id, categoryModel);
        }

        [HttpDelete]
        [Route("categories/{id}")]
        public bool DeleteAccount(int id)
        {
            return _dataService.Delete(id);
        }
    }
}