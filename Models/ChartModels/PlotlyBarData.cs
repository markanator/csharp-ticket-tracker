﻿using System.Collections.Generic;

namespace TheBugTracker.Models.ChartModels
{
    public class PlotlyBarData
    {
        public List<PlotlyBar> Data { get; set; }
    }


    public class PlotlyBar
    {
        public string[] X { get; set; }
        public int[] Y { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
    }
}
