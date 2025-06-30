using System;
using System.Collections.Generic;
using System.Linq;
using JOIEnergy.Domain;

namespace JOIEnergy.Services
{
    public class PricePlanService : IPricePlanService
    {
        public interface Debug { void Log(string s); };

        private readonly List<PricePlan> _pricePlans;
        private IMeterReadingService _meterReadingService;
      

        public PricePlanService(List<PricePlan> pricePlan, IMeterReadingService meterReadingService)
        {
            _pricePlans = pricePlan;
            _meterReadingService = meterReadingService;
        }

        private decimal calculateAverageReading(List<ElectricityReading> electricityReadings)
        {
            var newSummedReadings = electricityReadings.Select(readings => readings.Reading).Aggregate((reading, accumulator) => reading + accumulator);

            return newSummedReadings / electricityReadings.Count();
        }

        private decimal calculateTimeElapsed(List<ElectricityReading> electricityReadings)
        {
            var first = electricityReadings.Min(reading => reading.Time);
            var last = electricityReadings.Max(reading => reading.Time);

            return (decimal)(last - first).TotalHours;
        }
        private decimal calculateCost(List<ElectricityReading> electricityReadings, PricePlan pricePlan)
        {
            var average = calculateAverageReading(electricityReadings);
            var timeElapsed = calculateTimeElapsed(electricityReadings);
            var averagedCost = average/timeElapsed;
            return Math.Round(averagedCost * pricePlan.UnitRate, 3);
        }

        public Dictionary<string, decimal> GetConsumptionCostOfElectricityReadingsForEachPricePlan(string smartMeterId)
        {
            List<ElectricityReading> electricityReadings = _meterReadingService.GetReadings(smartMeterId);

            if (!electricityReadings.Any())
            {
                return new Dictionary<string, decimal>();
            }
            return _pricePlans.ToDictionary(plan => plan.PlanName, plan => calculateCost(electricityReadings, plan));
        }

        public Dictionary<string, decimal> GetUsageCostOfLastWeekSpending(string smartMeterId, string pricePlanName)
        {
            if (string.IsNullOrEmpty(smartMeterId) || !_meterReadingService.SmartMeterExists(smartMeterId))
            {
                return new Dictionary<string, decimal>();
            }
            var lastWeekReadings = _meterReadingService.GetReadings(smartMeterId)
                .Where(reading => reading.Time >= DateTime.Now.AddDays(-7))
                .ToList();
            if (!lastWeekReadings.Any())
            {
                return new Dictionary<string, decimal>();
            }

           
            var pricePlanDetails = _pricePlans.FirstOrDefault(plan => plan.PlanName == pricePlanName);

            var energyConsumed = calculateCost(lastWeekReadings, pricePlanDetails);

            return new Dictionary<string, decimal>()
           {
               { smartMeterId, energyConsumed }
           };
        }
    }
}
