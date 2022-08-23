using test.Application.Common.Interfaces;

namespace test.Infrastructure.Services;

public class DateTimeService : IDateTime
{
    public DateTime Now => DateTime.Now;
}
