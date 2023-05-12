using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SboxAnalysis.Models
{
    public class LinearResultModel
    {
        public double MaxLinearProbability { get; set; }
        public double MinLinearProbability { get; set; }
        public double AvgLinearProbability { get; set; }
        public double MaxDifferentialProbability { get; set; }
        public double MinDifferentialProbability { get; set; }
        public double AvgDifferentialProbability { get; set; }
        public double MaxNonLinearProbability { get; set; }
        public double MinNonLinearProbability { get; set; }
        public double AvgNonLinearProbability { get; set; }
        public double NonLinearProbabilityMax { get; set; }
        public double NonLinearProbabilityMin { get; set; }
        public double NonLinearProbabilityAvg { get; set; }



        public double SacMax { get; set; }
        public double SacMin { get; set; }
        public double SacAvg { get; set; }

        public double BıcSacMax { get; set; }
        public double BıcSacMin { get; set; }
        public double BıcSacAvg { get; set; }
        public int BicNl { get; set; }



    }
}