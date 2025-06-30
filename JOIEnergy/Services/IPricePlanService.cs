using System.Collections.Generic;

namespace JOIEnergy.Services
{
    public interface IPricePlanService
    {
        Dictionary<string, decimal> GetConsumptionCostOfElectricityReadingsForEachPricePlan(string smartMeterId);
        Dictionary<string, decimal> GetUsageCostOfLastWeekSpending(string smartMeterId,string planName);
    }
}