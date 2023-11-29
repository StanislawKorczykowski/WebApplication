using WebApplication2.Models;

namespace WebApplication2;

using System.Collections.Generic;

public interface IStoreRepo
{
    List<OrderDetails> GetOrdersByClientId(int clientId);
    int? CreateOrder(int clientId, List<OrderItemInput> orderItems);
}