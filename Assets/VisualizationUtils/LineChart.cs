using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Linq;

namespace VisualizationUtils
{
    public class LineChartData
    {
        // Dictionary to store columns dynamically
        public List<float> pointCoordsX = new List<float>();
        public List<float> pointCoordsY = new List<float>();
        public List<float> graphCoordsX = new List<float>();
        public List<float> graphCoordsY = new List<float>();

        public int numPoints;

        public string xLabel;
        public string yLabel;
        public string header;

    }
}
