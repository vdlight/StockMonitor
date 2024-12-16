using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StocksMonitor.Data.StockNS;
using StocksMonitor.Simulation.ConfigurationNS;
using StocksMonitor.Simulation.SimulationNS;

namespace StocksMonitor.Simulation.DefinitionsNS
{
#if SIMULATIONS
    public static class SimulationDefinitions
    {
        public static List<SimulationConfiguration> generateSimulationForInduvidualStocks()
        {
            List<SimulationConfiguration> returnSims = AddIndexes();

            var names = new List<String> 
            {
                "Atlas Copco A",
                "Cloetta"
            };

            //TODO INV value osv, kopiera till senaste. 
            // möjlighet att köra custom interval
            foreach (var name in names)
            {
                for (int balance = 0; balance <= 1; balance++)
                {
                    for (int profitRequired = 0; profitRequired <= 1; profitRequired++)
                    {
                        for (int divident = 0; divident <= 1; divident++)
                        {
                            // buy within ma limits. never adjust, sell
                            returnSims.Add(new SimulationConfiguration()
                            {
                                configuration = {
                                    dividentRequired = divident == 1,
                                    profitRequired = profitRequired == 1,
                                    balanceInvestment = balance == 1,
                                    buyRules =
                                    {
                                        new Rule(TRule.AboveMa, 0),
                                        new Rule(TRule.BelowMa, 15),
                                    },
                                    adjustBuyRules =
                                    {
                                        new Rule(TRule.Never)
                                    },
                                    sellRules =
                                    {
                                        new Rule(TRule.BelowMa, -5)
                                    }
                                },
                                individualStock = name
                            });
                            // buy within ma limits. never adjust, never sell
                            returnSims.Add(new SimulationConfiguration()
                            {
                                configuration = {
                                    dividentRequired = divident == 1,
                                    profitRequired = profitRequired == 1,
                                    balanceInvestment = balance == 1,
                                    buyRules =
                                    {
                                        new Rule(TRule.AboveMa, 0),
                                        new Rule(TRule.BelowMa, 15),
                                    },
                                    adjustBuyRules =
                                    {
                                        new Rule(TRule.Never)
                                    },
                                    sellRules =
                                    {
                                        new Rule(TRule.Never)
                                    }
                                },
                                individualStock = name
                            });
                            // buy and keep, never adjust
                            returnSims.Add(new SimulationConfiguration()
                            {
                                configuration = {
                                    dividentRequired = divident == 1,
                                    profitRequired = profitRequired == 1,
                                    balanceInvestment = balance == 1,
                                    buyRules =
                                    {
                                        new Rule(TRule.Never),
                                    },
                                    adjustBuyRules =
                                    {
                                        new Rule(TRule.Never)
                                    },
                                    sellRules =
                                    {
                                        new Rule(TRule.Never)
                                    }
                                },
                                individualStock = name
                            });
                            // buy within limits, and rebuy, sell below -5
                            returnSims.Add(new SimulationConfiguration()
                            {
                                configuration = {
                                    dividentRequired = divident == 1,
                                    profitRequired = profitRequired == 1,
                                    balanceInvestment = balance == 1,
                                    buyRules =
                                    {
                                        new Rule(TRule.Never),
                                    },
                                    adjustBuyRules =
                                    {
                                        new Rule(TRule.AboveMa, 0),
                                        new Rule(TRule.BelowMa, 15)
                                    },
                                    sellRules =
                                    {
                                        new Rule(TRule.BelowMa, -5)
                                    }
                                },
                                individualStock = name
                            });
                            // buy within limits, and rebuy, never sell
                            returnSims.Add(new SimulationConfiguration()
                            {
                                configuration = {
                                    dividentRequired = divident == 1,
                                    profitRequired = profitRequired == 1,
                                    balanceInvestment = balance == 1,
                                    buyRules =
                                    {
                                        new Rule(TRule.Never),
                                    },
                                    adjustBuyRules =
                                    {
                                        new Rule(TRule.AboveMa, 0),
                                        new Rule(TRule.BelowMa, 15)
                                    },
                                    sellRules =
                                    {
                                        new Rule(TRule.Never)
                                    }
                                },
                                individualStock = name
                            });
                        }
                    }
                }
            }
            return returnSims;
        }

        public static List<SimulationConfiguration> generateSimulations()
        {
            List<SimulationConfiguration> returnSims = AddIndexes();
            // TODO, Möjlighet att i simuleringar, välja vilka markander som ska köras. Så kör jag alla varianter för vald marknad sedan
            TMarket[] selectedMarkets = {
                TMarket.All, TMarket.AllExceptFirstNorth
            };

            foreach (var market in selectedMarkets)
            {
                for (int balance = 0; balance <= 1; balance++)
                {
                    for (int profitRequired = 0; profitRequired <= 1; profitRequired++)
                    {
                        for (int divident = 0; divident <= 1; divident++)
                        {
                            // buy over ma, 2 * limit. adj if below ma, never sell
                            returnSims.Add(new SimulationConfiguration()
                            {
                                configuration = {
                                    dividentRequired = divident == 1,
                                    profitRequired = profitRequired == 1,
                                    balanceInvestment = balance == 1,
                                    doubleStake = true,
                                    buyRules =
                                    {
                                        new Rule(TRule.AboveMa, 0),
                                        new Rule(TRule.BelowMa, 15),
                                    },
                                    adjustBuyRules =
                                    {
                                        new Rule(TRule.AboveMa, 1)
                                    },
                                    adjustSellRules =
                                    {
                                        new Rule(TRule.BelowMa, -2)
                                    },
                                    sellRules =
                                    {
                                        new Rule(TRule.Never)
                                    }
                                },
                                stockMarket = market,
                            }); 

                            // buy within ma limits. never adjust, sell
                            returnSims.Add(new SimulationConfiguration()
                            {
                                configuration = {
                                    dividentRequired = divident == 1,
                                    profitRequired = profitRequired == 1,
                                    balanceInvestment = balance == 1,
                                    buyRules =
                                    {
                                        new Rule(TRule.AboveMa, 0),
                                        new Rule(TRule.BelowMa, 15),
                                    },
                                    adjustBuyRules =
                                    {
                                        new Rule(TRule.Never)
                                    },
                                    adjustSellRules =
                                    {
                                        new Rule(TRule.Never)
                                    },
                                    sellRules =
                                    {
                                        new Rule(TRule.BelowMa, -5)
                                    }
                                },
                                stockMarket = market,
                            });
                            // buy within ma limits. never adjust, never sell
                            returnSims.Add(new SimulationConfiguration()
                            {
                                configuration = {
                                    dividentRequired = divident == 1,
                                    profitRequired = profitRequired == 1,
                                    balanceInvestment = balance == 1,
                                    buyRules =
                                    {
                                        new Rule(TRule.AboveMa, 0),
                                        new Rule(TRule.BelowMa, 15),
                                    },
                                    adjustBuyRules =
                                    {
                                        new Rule(TRule.Never)
                                    },
                                    adjustSellRules =
                                    {
                                        new Rule(TRule.Never)
                                    },
                                    sellRules =
                                    {
                                        new Rule(TRule.Never)
                                    }
                                },
                                stockMarket = market,
                            });
                            // buy and keep, never adjust
                            returnSims.Add(new SimulationConfiguration()
                            {
                                configuration = {
                                    dividentRequired = divident == 1,
                                    profitRequired = profitRequired == 1,
                                    balanceInvestment = balance == 1,
                                    buyRules =
                                    {
                                        new Rule(TRule.Never),
                                    },
                                    adjustBuyRules =
                                    {
                                        new Rule(TRule.Never)
                                    },
                                    adjustSellRules =
                                    {
                                        new Rule(TRule.Never)
                                    },
                                    sellRules =
                                    {
                                        new Rule(TRule.Never)
                                    }
                                },
                                stockMarket = market,
                            });
                            // buy within limits, and rebuy, sell below -5
                            returnSims.Add(new SimulationConfiguration()
                            {
                                configuration = {
                                    dividentRequired = divident == 1,
                                    profitRequired = profitRequired == 1,
                                    balanceInvestment = balance == 1,
                                    buyRules =
                                    {
                                        new Rule(TRule.Never),
                                    },
                                    adjustSellRules =
                                    {
                                        new Rule(TRule.Never)
                                    },
                                    adjustBuyRules =
                                    {
                                        new Rule(TRule.AboveMa, 0),
                                        new Rule(TRule.BelowMa, 15)
                                    },
                                    sellRules =
                                    {
                                        new Rule(TRule.BelowMa, -5)
                                    }
                                },
                                stockMarket = market,
                            });
                            // buy within limits, and rebuy, never sell
                            returnSims.Add(new SimulationConfiguration()
                            {
                                configuration = {
                                    dividentRequired = divident == 1,
                                    profitRequired = profitRequired == 1,
                                    balanceInvestment = balance == 1,
                                    buyRules =
                                    {
                                        new Rule(TRule.Never),
                                    },
                                    adjustBuyRules =
                                    {
                                        new Rule(TRule.AboveMa, 0),
                                        new Rule(TRule.BelowMa, 15)
                                    },
                                    adjustSellRules =
                                    {
                                        new Rule(TRule.Never)
                                    },
                                    sellRules =
                                    {
                                        new Rule(TRule.Never)
                                    }
                                },
                                stockMarket = market,
                            });
                        }
                    }
                }
            }
            return returnSims;
        }

        private static List<SimulationConfiguration> AddIndexes()
        {
            return new List<SimulationConfiguration> {

                new SimulationConfiguration()
                {
                    stockMarket = TMarket.IndexOMXSGI,
                    configuration =
                    {
                        indexCalculation = true,
                        dividentRequired = false,
                        profitRequired = false,
                        buyRules =
                        {
                            new Rule (TRule.Never)
                        },
                        sellRules =
                        {
                            new Rule(TRule.Never)
                        }
                    }
                }
            };
        }
    }
#endif
}
