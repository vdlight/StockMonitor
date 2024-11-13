using StocksMonitor.src.databaseWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace StocksMonitor.src.Simulation
{
#if SIMULATIONS
    public class SimulationNew
    {
        public bool indexCalculation;
        public decimal Investment { get; private set; } = 0;
        public decimal Value { get; private set; } = 0;
        public decimal Wallet { get; private set; } = 0;
        const decimal investmentTarget = 500;
        readonly decimal originalInvestment = 0;

        private DateTime fromDate;

        private readonly Configuration configuration;

        public decimal result { get; private set; }

        private List<Stock> simulationStocks;
        public SimulationNew(List<Stock> filteredStocks, Configuration configuration)
        {
            simulationStocks = filteredStocks.ToList(); // create copy
            this.configuration = configuration;
        }

        public void SimulateStocks()
        {
            List<Portfolio> portfolioHistory = []; // TODO, if date is more than data, return 0

            var ClearInvestmentFirstSimulationDay = true;

            // latest hisory is the same as current, dont duplicate
            var histories = simulationStocks.SelectMany(s => s.History).OrderBy(h => h.Date);
            var oldestStock = histories.FirstOrDefault();
            var newestStock = histories.LastOrDefault();
            // TODO, värden som beräknas sparas i respektive värde, dvs 6m 1y osv. Kan fixa så när man vill ställa custom tid, så beräknas bara dessa om
            // History när läst från db går från äldsta i 0 --> 27/9, till nyaste sist 14 --> 22/10
            var simulationDay = oldestStock.Date;
            var dividentMonth = simulationDay.AddMonths(4);


            if (!indexCalculation)
            {
                AddToWallet(originalInvestment);
            }


            while (simulationDay != newestStock.Date.AddDays(1).Date)
            {
                var valueOfInvestments = 0m;
                bool divident = false;

                if (simulationDay.Month == dividentMonth.Month)
                {
                    dividentMonth = dividentMonth.AddMonths(4);
                    divident = true;
                }

                foreach (var stock in simulationStocks)
                {
                    var h = stock.History.FirstOrDefault(h => h.Date == simulationDay);
                    // TODO, datagrid och graf, kan yllas anting med simulator data eller utevklingsdata. Utvecklingsdata kan komma från simulering eller inte.

                    if (h != null)
                    {
                        if (indexCalculation)
                        {
                            stock.OwnedCnt = 1; // Always own the index, no actions
                        }
                        else
                        {
                            HandleDatapoint(stock: stock, datapoint: h, wallet: Wallet, divident: divident);
                        }
                        valueOfInvestments += stock.OwnedCnt * h.Price;
                    }
                }
                var allStocksHistoryDay = simulationStocks.SelectMany(s => s.History).Where(h => h.Date == simulationDay);

                if (allStocksHistoryDay.Any())
                {
                    // First day, i need a reference point otherwice it will be value - investment --> 0, rendering total simulation useless
                    if (ClearInvestmentFirstSimulationDay)
                    {
                        ClearInvestmentFirstSimulationDay = false;
                        Investment = 0;
                    }

                    portfolioHistory.Add(
                        new Portfolio(
                            date: simulationDay,
                            wallet: Wallet,
                            value: valueOfInvestments,
                            investment: Investment
                        ));
                }
                simulationDay = simulationDay.AddDays(+1);
            }


            // Calculate result
            var currentPortfolio = portfolioHistory.Last();
            var oldPortfolio = portfolioHistory.First();

            var currentVal = currentPortfolio.value;
            var oldVal = oldPortfolio.value;


            currentVal += currentPortfolio.wallet;
            currentVal -= currentPortfolio.investment;

            oldVal += oldPortfolio.wallet;
            oldVal -= oldPortfolio.investment;

            var diff = currentVal - oldVal;
            try { 
                result =  diff / oldVal * 100;
            }
            catch (Exception ex) { StockMonitorLogger.WriteMsg("ERROR: Division by zero? "); }
        }
        private void HandleDatapoint(Stock stock, History datapoint, decimal wallet, bool divident)
        {
            // Buy
            if (stock.OwnedCnt == 0)
            {
                if (ComplyToRules(configuration.buyRules, datapoint))
                {
                    var (count, totalCost) = CalculateCost(dataPoint: datapoint, wallet: Wallet, ownedCnt: stock.OwnedCnt);
                    Wallet -= totalCost;
                    stock.OwnedCnt += count;
                }
            }
            else
            {   // SELL ALL
                if (ComplyToRules(configuration.sellRules, datapoint))
                {
                    Wallet += stock.OwnedCnt * datapoint.Price;
                    stock.OwnedCnt = 0;
                }
                // buy more if room
                else if (ComplyToRules(configuration.adjustBuyRules, datapoint))
                {
                    var (count, totalCost) = CalculateCost(dataPoint: datapoint, wallet: Wallet, ownedCnt: stock.OwnedCnt);
                    Wallet -= totalCost;
                    stock.OwnedCnt += count;
                }
                // sell off to balance
                else if (configuration.balanceInvestment)
                {
                    var (count, totalValue) = CalculateBalancing(dataPoint: datapoint, wallet: Wallet, ownedCnt: stock.OwnedCnt);
                    Wallet += totalValue;
                    stock.OwnedCnt -= count;
                }
            }

            if (divident && stock.Divident > 0)
            {
                wallet += 0.03m * stock.OwnedCnt * datapoint.Price / 4; // assume three precent per year / pay out quarterly
            }

        }
        protected (int, decimal) CalculateCost(History dataPoint, decimal wallet, int ownedCnt)
        {
            int buyCount = (int)((investmentTarget - ownedCnt * dataPoint.Price) / dataPoint.Price);
            decimal cost = dataPoint.Price * buyCount;

            if (cost == 0) // cant fit more stocks in investment target
            {
                return (0, 0);
            }

            // always make room for investment, to have "optimal" simulation

            if (cost > wallet)
            {
                AddToWallet(cost);
            }

            return (buyCount, cost);
        }

        private bool ComplyToRules(List<Rule> rules, History datapoint)
        {
            if (rules.Any(r => r.rule == TRule.Never))
            {
                return false;
            }


            // breaking any rules
            foreach (var rule in rules)
            {
                switch (rule.rule)
                {
                    // DO not trust MA200 values of 0, since it can be just def value --> not calculated, will filter a few points, but very few i assume
                    case TRule.AboveMa:
                        if (datapoint.MA200 == 0 || datapoint.MA200 < rule.RuleValue)
                        {
                            return false;
                        }
                        break;
                    case TRule.BelowMa:
                        if (datapoint.MA200 == 0 || datapoint.MA200 > rule.RuleValue)
                        {
                            return false;
                        }
                        break;
                    case TRule.SellProfit:
                        break; // TODO, sell with proits

                }
            }
            return true;

        }
        private (int, decimal) CalculateBalancing(History dataPoint, decimal wallet, int ownedCnt)
        {
            if (dataPoint.Price == 0)
            {
                return (0, 0);
            }

            const decimal minAdjustment = investmentTarget * 0.20m;
            var invested = dataPoint.Price * ownedCnt;
            var room = invested - investmentTarget;


            int sellCount = (int)(room / dataPoint.Price);

            if (sellCount > 0)
            {
                var adjustment = sellCount * dataPoint.Price;
                if (adjustment > minAdjustment)
                {
                    return (sellCount, adjustment);
                }
            }
            return (0, 0);
        }
        private void AddToWallet(decimal value)
        {
            Wallet += value;
            Investment += value;
        }
    }
#endif
}
