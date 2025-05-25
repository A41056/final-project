using Marten;

namespace Ordering.Application.Orders.Queries.GetOrderStats
{
    public record GetOrderStatsQuery(string Range) : IQuery<OrderStatsDto>;

    public record OrderStatsDto(
        int TotalOrders,
        int PreviousOrders,
        double OrderChangePercent,

        decimal TotalSales,
        decimal PreviousSales,
        double SalesChangePercent,

        List<ChartPoint> Chart
    );

    public record ChartPoint(string Date, int OrderCount, decimal Sales);
    public class GetOrderStatsQueryHandler(IApplicationDbContext dbContext)
    : IQueryHandler<GetOrderStatsQuery, OrderStatsDto>
    {
        public async Task<OrderStatsDto> Handle(GetOrderStatsQuery query, CancellationToken ct)
        {
            var (currentFrom, currentTo, previousFrom, previousTo) = GetDateRanges(query.Range);

            var allOrders = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
                .ToListAsync(
                    dbContext.Orders
                        .Include(o => o.OrderItems)
                        .AsNoTracking()
                        .Where(o => o.PayDate >= previousFrom && o.PayDate < currentTo),
                    ct
                );

            var currentOrders = allOrders.Where(o => o.PayDate >= currentFrom && o.PayDate <= currentTo).ToList();
            var previousOrders = allOrders.Where(o => o.PayDate >= previousFrom && o.PayDate < currentFrom).ToList();

            int totalOrders = currentOrders.Count;
            int previousOrderCount = previousOrders.Count;

            decimal totalSales = currentOrders.Sum(o => o.TotalPrice);
            decimal previousSales = previousOrders.Sum(o => o.TotalPrice);

            double orderChange = CalcPercentChange(previousOrderCount, totalOrders);
            double salesChange = CalcPercentChange(previousSales, totalSales);

            var chart = currentOrders
                .GroupBy(o => o.PayDate.Date)
                .OrderBy(g => g.Key)
                .Select(g => new ChartPoint(
                    g.Key.ToString("yyyy-MM-dd"),
                    g.Count(),
                    g.Sum(o => o.TotalPrice)
                ))
                .ToList();

            return new OrderStatsDto(
                totalOrders, previousOrderCount, orderChange,
                totalSales, previousSales, salesChange,
                chart
            );
        }

        private static double CalcPercentChange(decimal oldVal, decimal newVal)
        {
            if (oldVal == 0) return newVal > 0 ? 100 : 0;
            return (double)((newVal - oldVal) / oldVal * 100);
        }

        private static double CalcPercentChange(int oldVal, int newVal)
            => CalcPercentChange((decimal)oldVal, (decimal)newVal);

        private static (DateTime currentFrom, DateTime currentTo, DateTime previousFrom, DateTime previousTo)
    GetDateRanges(string range)
        {
            var now = DateTime.UtcNow;
            var today = now.Date;
            var endOfToday = today.AddDays(1).AddTicks(-1);

            return range switch
            {
                "1d" => (today, endOfToday, today.AddDays(-1), today.AddTicks(-1)),
                "28d" => (today.AddDays(-27), endOfToday, today.AddDays(-55), today.AddDays(-28).AddTicks(-1)),
                _ => (today.AddDays(-6), endOfToday, today.AddDays(-13), today.AddDays(-7).AddTicks(-1)),
            };
        }
    }
}
