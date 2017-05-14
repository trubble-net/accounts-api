using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Picassi.Core.Accounts.DAL.Entities;
using Picassi.Core.Accounts.Models.Categories;
using Picassi.Core.Accounts.Services;
using Picassi.Utils.Data.Extensions;

namespace Picassi.Core.Accounts.DAL.Services
{
    public interface ICategoriesDataService : IGenericDataService<CategoryModel>
    {
        IEnumerable<CategoryModel> Query(CategoriesQueryModel query);
    }

    public class CategoriesDataService : GenericDataService<CategoryModel, Category>, ICategoriesDataService
    {
        public CategoriesDataService(IModelMapper<CategoryModel, Category> modelMapper, IAccountsDataContext dbContext) 
            : base(modelMapper, dbContext)
        {
        }

        public IEnumerable<CategoryModel> Query(CategoriesQueryModel query)
        {
            var queryResults = DbContext.Categories.AsQueryable();

            if (query?.Name != null) queryResults = queryResults.Where(x => x.Name.Contains(query.Name));
            queryResults = query == null ? queryResults : OrderResults(queryResults, query.SortBy, query.SortAscending);
            return Mapper.Map<IEnumerable<CategoryModel>>(queryResults);
        }

        private static IQueryable<Category> OrderResults(IQueryable<Category> categories, string field, bool ascending)
        {
            if (field == null)
            {
                field = "Id";
                @ascending = true;
            }

            return field == "Id" ? categories.OrderBy(field, @ascending) : categories.OrderBy(field, @ascending).ThenBy("Id", @ascending);
        }

    }

    public class CategoryModelMapper : IModelMapper<CategoryModel, Category>
    {
        public Category CreateEntity(CategoryModel model)
        {
            return Mapper.Map<Category>(model);
        }

        public CategoryModel Map(Category model)
        {
            return Mapper.Map<CategoryModel>(model);
        }

        public void Patch(CategoryModel model, Category entity)
        {
            Mapper.Map(model, entity);
        }
    }
}