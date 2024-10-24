using StocksMonitor.Migrations;
using StocksMonitor.src.databaseWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace StocksMonitor.src
{
    public enum StratAction
    {
        SELL, 
        ADJ_DOWN, 
        BUY,
        NONE
    }

    public class ActionData
    {
        public StratAction action;
        public int adjustmentCount;
        public decimal value;

        public ActionData(StratAction action) 
        {
            this.action = action;
        }
    }
    public abstract class Strategy
    {
        readonly protected decimal investmentTarget;
        readonly decimal adjPercent;

        public Strategy(decimal investmentTarget, decimal adjPercent)
        {
            this.investmentTarget = investmentTarget;
            this.adjPercent = adjPercent;
        }

        public abstract ActionData DetermineAction(History dataPoint, decimal wallet);
        protected (int, decimal) CalculateCost(History dataPoint, decimal wallet)
        {
            int buyCount = (int)((investmentTarget - (dataPoint.OwnedCnt * dataPoint.Price)) / dataPoint.Price);
            decimal cost = (dataPoint.Price * buyCount);
            
            var walletAllows = cost > 0 && wallet >= cost;

            return walletAllows? (buyCount, cost) : (0, 0);
        }
    }

    public class Strat_BuyAndHold_NoMA : Strategy
    {
        public const string Name = "Buy and hold no MA";

        public Strat_BuyAndHold_NoMA(decimal investmentTarget, decimal adjPercentage)
            : base(investmentTarget, adjPercentage)
        {
     
        }

        public override ActionData DetermineAction(History dataPoint, decimal wallet)
        {
            if (dataPoint.OwnedCnt == 0)
            {
                var (count, price) = CalculateCost(dataPoint, wallet);

                if (count > 0)
                {
                    return new ActionData(StratAction.BUY)
                    {
                        adjustmentCount = count,
                        value = price
                    };

                }
            }
            return new ActionData(StratAction.NONE);
        }    
    }
    public class Strat_BuyWithinMA0And15_AndHold : Strategy
    {
        public const string Name = "Buy within MA200 0-15 and hold";
        private const decimal MA200_min = 0;
        private const decimal MA200_max = 15;
        public Strat_BuyWithinMA0And15_AndHold(decimal investmentTarget, decimal adjPercentage)
            : base(investmentTarget, adjPercentage)
        {

        }
        public override ActionData DetermineAction(History dataPoint, decimal wallet)
        {
            if (dataPoint.OwnedCnt == 0)
            {
                if (dataPoint.MA200 >= MA200_min && dataPoint.MA200 <= MA200_max)
                {
                    var (count, price) = CalculateCost(dataPoint, wallet);

                    if (count > 0)
                    {
                        return new ActionData(StratAction.BUY)
                        {
                            adjustmentCount = count,
                            value = price
                        };
                    }
                }
            }
            return new ActionData(StratAction.NONE);
        }
    }
    public class Strat_BuyWithinMA0And15_AdjustDownToTarget : Strategy
    {
        public const string Name = "Buy within MA200 0-15 and sell off to target";
        public Strat_BuyWithinMA0And15_AdjustDownToTarget(decimal investmentTarget, decimal adjPercentage)
          : base(investmentTarget, adjPercentage)
        {

        }
        public override ActionData DetermineAction(History dataPoint, decimal wallet)
        {
            if (dataPoint.OwnedCnt == 0)
            {
                var roomToBuy = investmentTarget >= dataPoint.Price;
                var walletAllows = wallet >= dataPoint.Price;
                var MA_Allows = dataPoint.MA200 >= 0 && dataPoint.MA200 <= 15;
                
                if (roomToBuy && walletAllows && MA_Allows)
                {
                    return StratAction.BUY;
                }

                var currentValue = dataPoint.Price * dataPoint.OwnedCnt;
                var adjustmentValue = adjPercentage / 100 * investmentTarget;
                var roomToAdjust = investmentTarget - currentValue >=  dataPoint.Price;


                if ( 
                    )
            }
            return StratAction.NONE;
        }

    }
