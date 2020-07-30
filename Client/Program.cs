using Grpc.Core;
using Grpc.Net.Client;
using Orders;
using Products;
using System;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var ordersClient = new OrderPlacement.OrderPlacementClient(channel);
            Console.WriteLine("Welcome to the gRPC client");
            var reply = await ordersClient.CreateOrderAsync(new CreateOrderRequest
            {
                ProductId = "ABC1234",
                Quantity = 1,
                Address = "Mock Address"
            });

            channel = GrpcChannel.ForAddress("http://localhost:5003");
            var productsClient = new ProductsInventory.ProductsInventoryClient(channel);
            var replyProduct = await productsClient.DetailsAsync(new ProductDetailsRequest { ProductId = "Product6996" });
            Console.WriteLine($"Product details received. Name={replyProduct.Name}, Category={replyProduct.Category}");

            Console.WriteLine($"Created order: {reply.OrderId}");
            using (var statusReplies = ordersClient.GetOrderStatus(new GetOrderStatusRequest { OrderId = reply.OrderId }))
            {
                while (await statusReplies.ResponseStream.MoveNext())
                {
                    var statusReply = statusReplies.ResponseStream.Current.Status;
                    Console.WriteLine($"Order status: {statusReply}");
                }
            }

            Console.ReadKey();
        }
    }
}
