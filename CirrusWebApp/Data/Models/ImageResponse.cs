using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace CirrusWebApp.Data.Models
{
	public partial class ImageResponse
	{
		[JsonProperty("Results")]
		public WebServiceResponse Response { get; set; }
	}

	public partial class WebServiceResponse
	{
		[JsonProperty("WebServiceOutput0")]
		public List<Dictionary<string, string>> Scores = new List<Dictionary<string, string>>();
		private Dictionary<string, string> InternalScores = new Dictionary<string, string>();
		public Dictionary<string, double> ScoreValues = new Dictionary<string, double>();

		public void SetScoreValues()
        {
			InternalScores = Scores[0];
			InternalScores.Remove("category");
			InternalScores.Remove("id");
			InternalScores.Remove("Scored Labels");
			foreach (var entry in InternalScores)
            {
				try
				{
					if (entry.Value.Contains("e"))
					{
						ScoreValues.Add(entry.Key.Replace("Scored Probabilities_", ""), Double.Parse(entry.Value, NumberStyles.Float, CultureInfo.InvariantCulture));
					}
					else
					{
						ScoreValues.Add(entry.Key.Replace("Scored Probabilities_", ""), Double.Parse(entry.Value));
					}
				}
				catch (FormatException e)
                {
                    Console.WriteLine(entry.Value + " was not in correct format");
					continue;
                }
            }
        }
	}
}
