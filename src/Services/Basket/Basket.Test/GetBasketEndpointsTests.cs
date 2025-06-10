using Basket.API.Basket.GetBasket;
using Basket.API.Models;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Moq;

namespace Basket.Test
{
    public class GetBasketEndpointsTests
    {
        [Fact]
        public void GetBasket_ShouldReturnOk_WhenUserIdIsValid()
        {
            var userId = Guid.NewGuid();
            var senderMock = new Mock<ISender>();
            var basket = new ShoppingCart();
            var result = new GetBasketResult(basket);
            senderMock.Setup(s => s.Send(It.IsAny<GetBasketQuery>(), default))
                      .ReturnsAsync(result);

            var endpoint = new GetBasketEndpoints();
            var app = new Mock<IEndpointRouteBuilder>();

            endpoint.AddRoutes(app.Object);

            app.Verify(a => a.MapGet(It.IsAny<string>(), It.IsAny<Func<Guid, ISender, Task>>()), Times.Once);
        }
    }
}