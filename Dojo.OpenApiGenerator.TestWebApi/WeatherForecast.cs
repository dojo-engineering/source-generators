using System;
using System.ComponentModel.DataAnnotations;

namespace Dojo.OpenApiGenerator.TestWebApi
{
    public class WeatherForecast
    {
        [Required]
        public DateTime Date { get; set; }

        public int TemperatureC { get; set; }

        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        [Required]
        [EmailAddress]
        public string Summary { get; set; }
    }
}
