using Marten.Linq;
using Moq;

namespace Review.Test
{
    public static class MartenQueryableExtensions
    {
        public static IMartenQueryable<T> BuildMockQueryable<T>(this IEnumerable<T> source)
        {
            var mock = new Mock<IMartenQueryable<T>>();
            mock.Setup(x => x.GetEnumerator()).Returns(source.GetEnumerator());
            return mock.Object;
        }
    }
}
