using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Cors;
using Picassi.Api.Accounts.Contract;
using Picassi.Api.Accounts.Contract.Calendar;
using Picassi.Core.Accounts.DAL.Services;
using Picassi.Core.Accounts.Models.ModelledTransactions;

namespace Picassi.Api.Accounts.Controllers
{
	[EnableCors(origins: "*", headers: "*", methods: "*")]
    [PicassiApiAuthorise]
    public class ModelledTransactionsController : ApiController
	{
        private readonly IModelledTransactionsDataService _transactionDataService;

	    public ModelledTransactionsController(IModelledTransactionsDataService transactionDataService)
	    {
	        _transactionDataService = transactionDataService;
	    }

	    [HttpGet]
        [Route("accounts/{accountId}/projected-transactions")]
        public ResultsViewModel<ModelledTransactionModel> GetTransactionsForAccount(int accountId, [FromUri]ModelledTransactionQueryModel query)
        {
            return _transactionDataService.Query(accounts: new [] { accountId}, pageNumber: query?.PageNumber, pageSize: query?.PageSize);
        }

	    [HttpGet]
	    [Route("accounts/{accountId}/projected-transactions/weekly")]
	    public IEnumerable<TransactionCategoriesGroupedByPeriodModel> GetWeeklyTransactionsForAccount(int accountId, [FromUri]ModelledTransactionQueryModel query)
	    {
	        return _transactionDataService.QueryGrouped(query.DateFrom, query.DateTo, ReportingPeriod.Week, accounts: new[] { accountId }, pageNumber: query?.PageNumber, pageSize: query?.PageSize);
	    }


        [HttpPost]
        [Route("accounts/{accountId}/projected-transactions")]
        public ModelledTransactionModel CreateTransaction(int accountId, [FromBody]ModelledTransactionModel transactionModel)
        {
            return _transactionDataService.Create(transactionModel);
        }

        [HttpGet]
        [Route("accounts/{accountId}/projected-transactions/{transactionId}")]
        public ModelledTransactionModel GetTransaction(int accountId, int transactionId)
        {
            return _transactionDataService.Get(transactionId);
        }

        [HttpPut]
        [Route("accounts/{accountId}/projected-transactions/{transactionId}")]
        public ModelledTransactionModel UpdateTransaction(int accountId, int transactionId, [FromBody]ModelledTransactionModel transactionModel)
        {
            return _transactionDataService.Update(transactionId, transactionModel);
        }

        [HttpDelete]
        [Route("accounts/{accountId}/projected-transactions/{transactionId}")]
        public bool DeleteAccount(int accountId, int transactionId)
        {
            return _transactionDataService.Delete(transactionId);
        }
    }
}