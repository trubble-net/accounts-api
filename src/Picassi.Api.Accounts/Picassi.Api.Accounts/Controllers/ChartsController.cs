using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;
using Picassi.Api.Accounts.Contract;
using Picassi.Api.Accounts.Contract.Enums;
using Picassi.Core.Accounts.Models;
using Picassi.Core.Accounts.Services.Charts;
using Picassi.Core.Accounts.Time.Periods;

namespace Picassi.Api.Accounts.Controllers
{
	[EnableCors(origins: "*", headers: "*", methods: "*")]
    [PicassiApiAuthorise]
    public class ChartsController : ApiController
	{
	    private readonly IChartCompiler _chartCompiler;

	    public ChartsController(IChartCompiler chartCompiler)
	    {
	        _chartCompiler = chartCompiler;
	    }

	    [HttpPost]
        [Route("charts/account-balance")]
        public LineChartModel GetAccounts([FromBody]TransactionChartRequest query)
        {
            var dataPoints = _chartCompiler.GetTransactionSeriesData(query.DateFrom, query.DateTo, PeriodType.Day, GroupingType.Accounts).ToList();
            var grouped = dataPoints.SelectMany(x => x.Data)
                .Select(x => new { Start = x.PeriodStart, Transaction = x})
                .GroupBy(x => x.Start)
                .ToList();
            return new LineChartModel
            {
                Labels = grouped.Select(x => x.Key.ToString("dd MMM")).ToList(),
                Total = grouped.Select(grp => CompiledTransactions(grp.Key, grp.Select(x => x.Transaction).ToList())),
                Series = dataPoints,
                Type = "spline"
            };
        }

	    [HttpPost]
	    [Route("charts/category-spending")]
	    public LineChartModel GetCategories([FromBody]TransactionChartRequest query)
	    {
	        var dateFrom = query?.DateFrom ?? DateTime.Today.AddMonths(-1);
	        var dateTo = query?.DateTo ?? DateTime.Today;
	        var groupBy = query?.GroupBy ?? PeriodType.Week;
            var dataPoints = _chartCompiler.GetTransactionSeriesData(dateFrom, dateTo, groupBy, 
                GroupingType.Categories, query?.Accounts, query?.Categories, query?.IncludeSubcategories == true).ToList();
	        return new LineChartModel
	        {
	            Labels = dataPoints[0].Data.Select(x => x.PeriodStart.ToString("dd/MM/yyyy")).ToList(),
	            Series = dataPoints,
	            Type = "spline"
	        };
	    }

        [HttpPost]
	    [Route("charts/spending-by-category")]
	    public PieChartModel GetSpendingByCategory([FromBody]TransactionChartRequest query)
	    {
	        var data = _chartCompiler.GetSpendingByCategoryDataSeries(query.DateFrom, query.DateTo);
	        return new PieChartModel
	        {
	            Series = data
	        };
	    }

	    [HttpPost]
	    [Route("charts/spending-versus-budget")]
	    public LineChartModel GetSpendingVersusBudget([FromBody]TransactionChartRequest query)
	    {
            throw new NotImplementedException();
	    }

	    private static TransactionPeriodSummary CompiledTransactions(DateTime start, IList<TransactionPeriodSummary> transactions)
	    {
	        return new TransactionPeriodSummary
	        {
	            EndBalance = transactions.Sum(t => t.EndBalance),
	            PeriodStart = start,
	            TotalIncome = transactions.Sum(t => t.TotalIncome),
	            TotalSpending = transactions.Sum(t => t.TotalSpending)
	        };
	    }
    }
}