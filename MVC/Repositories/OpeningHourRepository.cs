using Microsoft.EntityFrameworkCore;
using mvc.Models;
using mvc.RepoInterfaces;
using mvc.Models;
using System.Linq.Expressions;

namespace mvc.Repositories
{
    public class OpeningHourRepository : GeniricRepository<int, OpeningHour>, IOpeningHourRepository, IGeniricRepository<int, OpeningHour>
    {
        private readonly ProjectContext _context;

        public OpeningHourRepository(ProjectContext context) : base(context)
        {
            _context = context;
        }

        public new IQueryable<OpeningHour> GetAll(Expression<Func<OpeningHour, bool>> predicate = null)
        {
            try
            {
                if (predicate != null)
                    return _context.OpeningHours.Where(predicate);
                
                return _context.OpeningHours;
            }
            catch (Exception ex)
            {
                // Log exception
                Console.WriteLine($"Error in GetAll: {ex.Message}");
                throw;
            }
        }

        public new async Task<OpeningHour> GetByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid ID provided", nameof(id));
                
            try
            {
                return await _context.OpeningHours.FindAsync(id);
            }
            catch (Exception ex)
            {
                // Log exception
                Console.WriteLine($"Error in GetByIdAsync: {ex.Message}");
                throw;
            }
        }

        public new async Task AddAsync(OpeningHour openingHour)
        {
            if (openingHour == null)
                throw new ArgumentNullException(nameof(openingHour), "Opening hour cannot be null");
                
            if (openingHour.BusinessId <= 0)
                throw new ArgumentException("Invalid BusinessId", nameof(openingHour));
                
            try
            {
                // Validate business exists
                var businessExists = await _context.Businesses.AnyAsync(b => b.Id == openingHour.BusinessId);
                if (!businessExists)
                    throw new InvalidOperationException($"Business with ID {openingHour.BusinessId} does not exist");
                
                if (openingHour.IsClosed)
                {
                    // If closed, clear the time values
                    openingHour.OpenTime = null;
                    openingHour.CloseTime = null;
                }
                else
                {
                    // Validate times
                    if (!ValidateOpeningHours(openingHour))
                        throw new ArgumentException("Invalid opening hours: open time must be before close time");
                }
                
                await _context.OpeningHours.AddAsync(openingHour);
            }
            catch (Exception ex)
            {
                // Log exception
                Console.WriteLine($"Error in AddAsync: {ex.Message}");
                throw;
            }
        }

        public new void Update(OpeningHour openingHour)
        {
            if (openingHour == null)
                throw new ArgumentNullException(nameof(openingHour), "Opening hour cannot be null");
                
            if (openingHour.Id <= 0)
                throw new ArgumentException("Invalid Id", nameof(openingHour));
                
            try
            {
                // If closed, clear the time values
                if (openingHour.IsClosed)
                {
                    openingHour.OpenTime = null;
                    openingHour.CloseTime = null;
                }
                else
                {
                    // Validate times
                    if (!ValidateOpeningHours(openingHour))
                        throw new ArgumentException("Invalid opening hours: open time must be before close time");
                }
                
                _context.OpeningHours.Update(openingHour);
            }
            catch (Exception ex)
            {
                // Log exception
                Console.WriteLine($"Error in Update: {ex.Message}");
                throw;
            }
        }

        public new async Task DeleteAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid ID provided", nameof(id));
                
            try
            {
                var openingHour = await GetByIdAsync(id);
                if (openingHour != null)
                {
                    _context.OpeningHours.Remove(openingHour);
                }
            }
            catch (Exception ex)
            {
                // Log exception
                Console.WriteLine($"Error in DeleteAsync: {ex.Message}");
                throw;
            }
        }

        public new async Task SaveAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // Handle concurrency issues
                Console.WriteLine($"Concurrency error: {ex.Message}");
                throw;
            }
            catch (DbUpdateException ex)
            {
                // Handle database update errors
                Console.WriteLine($"Database update error: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                // Log any other exceptions
                Console.WriteLine($"Error in SaveAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<List<OpeningHour>> GetByBusinessIdAsync(int businessId)
        {
            if (businessId <= 0)
                throw new ArgumentException("Invalid business ID provided", nameof(businessId));
                
            try
            {
                return await _context.OpeningHours
                    .Where(oh => oh.BusinessId == businessId)
                    .OrderBy(oh => oh.DayOfWeek) // Order by day of week
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                // Log exception
                Console.WriteLine($"Error in GetByBusinessIdAsync: {ex.Message}");
                throw;
            }
        }

        // More efficient method for deleting by business ID without loading data first
        public async Task DeleteByBusinessIdAsync(int businessId)
        {
            if (businessId <= 0)
                throw new ArgumentException("Invalid business ID provided", nameof(businessId));
                
            try
            {
                // Use SQL for more efficient deletion
                await _context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM OpeningHours WHERE BusinessId = {0}", businessId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DeleteByBusinessIdAsync: {ex.Message}");
                throw;
            }
        }

        // Validate that open time is before close time
        private bool ValidateOpeningHours(OpeningHour openingHour)
        {
            if (openingHour.IsClosed)
                return true;
                
            // Check if either time value is null
            if (openingHour.OpenTime == null || openingHour.CloseTime == null)
                return false;
                
            try 
            {
                // Compare the TimeSpan values
                return openingHour.OpenTime < openingHour.CloseTime;
            }
            catch 
            {
                return false; // Any exception means invalid format
            }
        }

        // Method for batch updating business hours with transaction support
        public async Task UpdateBusinessHoursAsync(int businessId, List<OpeningHour> openingHours)
        {
            if (businessId <= 0)
                throw new ArgumentException("Invalid business ID", nameof(businessId));
                
            if (openingHours == null)
                throw new ArgumentNullException(nameof(openingHours));
                
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                // Delete existing hours
                await DeleteByBusinessIdAsync(businessId);
                
                // Add new hours
                foreach (var hour in openingHours)
                {
                    hour.BusinessId = businessId;
                    
                    if (!ValidateOpeningHours(hour))
                        throw new ArgumentException($"Invalid opening hours for day {hour.DayOfWeek}");
                        
                    await AddAsync(hour);
                }
                
                await SaveAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Error updating business hours: {ex.Message}");
                throw;
            }
        }
    }
}
