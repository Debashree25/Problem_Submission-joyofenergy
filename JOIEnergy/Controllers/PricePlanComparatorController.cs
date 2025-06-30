using System.Collections.Generic;
using System.Linq;
using JOIEnergy.Enums;
using JOIEnergy.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace JOIEnergy.Controllers
{
    [Route("price-plans")]
    public class PricePlanComparatorController : Controller
    {
        public const string PRICE_PLAN_ID_KEY = "pricePlanId";
        public const string PRICE_PLAN_COMPARISONS_KEY = "pricePlanComparisons";
        public const string PRICE_PLAN_USAGE_KEY = "pricePlanUsageCost";
        private readonly IPricePlanService _pricePlanService;
        private readonly IAccountService _accountService;

        public PricePlanComparatorController(IPricePlanService pricePlanService, IAccountService accountService)
        {
            this._pricePlanService = pricePlanService;
            this._accountService = accountService;
        }

        [HttpGet("compare-all/{smartMeterId}")]
        public ObjectResult CalculatedCostForEachPricePlan(string smartMeterId)
        {
            string pricePlanId = _accountService.GetPricePlanIdForSmartMeterId(smartMeterId);
            
            
            Dictionary<string, decimal> costPerPricePlan = _pricePlanService.GetConsumptionCostOfElectricityReadingsForEachPricePlan(smartMeterId);
            if (!costPerPricePlan.Any())
            {
                return new NotFoundObjectResult(string.Format("Smart Meter ID ({0}) not found", smartMeterId));
            }

            return new ObjectResult(new Dictionary<string, object>() {
                {PRICE_PLAN_ID_KEY, pricePlanId},
                {PRICE_PLAN_COMPARISONS_KEY, costPerPricePlan},
            });
        }

        [HttpGet("recommend/{smartMeterId}")]
        public ObjectResult RecommendCheapestPricePlans(string smartMeterId, int? limit = null) {
            var consumptionForPricePlans = _pricePlanService.GetConsumptionCostOfElectricityReadingsForEachPricePlan(smartMeterId);

            if (!consumptionForPricePlans.Any()) {
                return new NotFoundObjectResult(string.Format("Smart Meter ID ({0}) not found", smartMeterId));
            }

            var recommendations = consumptionForPricePlans.OrderBy(pricePlanComparison => pricePlanComparison.Value);

            if (limit.HasValue && limit.Value < recommendations.Count())
            {
                return new ObjectResult(recommendations.Take(limit.Value));
            }

            return new ObjectResult(recommendations);
        }

        [HttpGet("usage-cost/{smartMeterId}")]
        public ObjectResult ReturnUsageCostOfLastWeekSpending(string smartMeterId)
        {
            string pricePlanName = _accountService.GetPricePlanIdForSmartMeterId(smartMeterId);

            if(!pricePlanName.Any())
            {
                return new NotFoundObjectResult(string.Format("Smart Meter ID ({0}) has no Price Plan", smartMeterId));
            }

            Dictionary<string, decimal> usageCost = _pricePlanService.GetUsageCostOfLastWeekSpending(smartMeterId, pricePlanName);
            if (!usageCost.Any())
            {
                return new NotFoundObjectResult(string.Format("Smart Meter ID ({0}) has no usage data", smartMeterId));
            }

            return new ObjectResult(new Dictionary<string, object>() {
                    {PRICE_PLAN_ID_KEY, pricePlanName},
                    {PRICE_PLAN_USAGE_KEY, usageCost},
            });
         }
    }
}

