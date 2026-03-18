namespace SwineBreedingManager.Services
{
    public interface IBreedingService
    {
        DateTime CalculateExpectedBirthDate(DateTime breedingDate);
        DateTime CalculateWeaningDate(DateTime birthDate);
    }

    public class BreedingService : IBreedingService
    {
        public DateTime CalculateExpectedBirthDate(DateTime breedingDate)
        {
            // Standard gestation is 114 days (3 months + 3 weeks + 3 days)
            return breedingDate.AddDays(114);
        }

        public DateTime CalculateWeaningDate(DateTime birthDate)
        {
            // Standard weaning is 21-28 days.
            return birthDate.AddDays(24);
        }
    }
}
