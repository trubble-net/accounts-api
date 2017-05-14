using System.Collections.Generic;
using System.Linq;
using Picassi.Core.Accounts.DAL.Entities;
using Picassi.Core.Accounts.Models.Budgets;
using Picassi.Core.Accounts.Services;

namespace Picassi.Core.Accounts.DAL.Services
{
    public interface IBudgetsDataService : IGenericDataService<BudgetModel>
    {
        IEnumerable<BudgetModel> Query(BudgetsQueryModel query);
    }

    public class BudgetsDataService : GenericDataService<BudgetModel, Budget>, IBudgetsDataService
    {
        public BudgetsDataService(IModelMapper<BudgetModel, Budget> modelMapper, IAccountsDataContext dbContext) 
            : base(modelMapper, dbContext)
        {
        }

        public IEnumerable<BudgetModel> Query(BudgetsQueryModel query)
        {
            var queryResults = DbContext.Budgets.AsQueryable();

            return queryResults.Select(ModelMapper.Map);
        }

    }
}