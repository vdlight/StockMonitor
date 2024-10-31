using Microsoft.Identity.Client;
using StocksMonitor.Migrations;
using StocksMonitor.src;
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

    public enum TMarket
    {
        All,
        AllExceptFirstNorth,
        LargeCap,
        MidCap,
        SmallCap,
        FirstNorth,
        IndexFirstNorthAll,
        IndexOMXSmallCap, 
        IndexOMXMidCap,
        IndexOMXLargeCap,
        IndexOMXSGI,
    }

    public enum TRule
    {
        BelowMa,
        AboveMa,
        DividentAbove,
        PeAbove,
        Index,
        None,
        Never
    }
    public class Rule
    {
        public readonly TRule rule;
        public readonly decimal RuleValue;

        public Rule( TRule rule, decimal value = 0) {  this.rule = rule; this.RuleValue = value; }  

    }
}
