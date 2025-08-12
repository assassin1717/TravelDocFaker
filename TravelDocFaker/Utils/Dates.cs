namespace TravelDocFaker.Utils
{
    public class Dates
    {
        /// <summary>
        /// Generates a random <see cref="DateOnly"/> between the specified minimum and maximum dates (inclusive of <paramref name="min"/>, exclusive of <paramref name="max"/>).
        /// </summary>
        /// <param name="min">
        /// The earliest possible date to return.
        /// </param>
        /// <param name="max">
        /// The latest possible date to return.
        /// </param>
        /// <returns>
        /// A <see cref="DateOnly"/> randomly selected between <paramref name="min"/> and <paramref name="max"/>.
        /// </returns>
        /// <remarks>
        /// The generated date is calculated by determining the day range between the two dates and selecting a random offset.  
        /// The day difference is clamped to a minimum of 1 to avoid invalid ranges.
        /// </remarks>
        public static DateOnly RandomDate(DateOnly min, DateOnly max)
        {
            var days = max.DayNumber - min.DayNumber;
            return DateOnly.FromDayNumber(min.DayNumber + Random.Shared.Next(Math.Max(1, days)));
        }
    }
}
