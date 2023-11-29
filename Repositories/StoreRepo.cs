using WebApplication2.Models;

namespace WebApplication2;

using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

public class StoreRepo : IStoreRepo
{
    private readonly IConfiguration _configuration;

    public StoreRepo(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public List<OrderDetails> GetOrdersByClientId(int clientId)
    {
        var orders = new List<OrderDetails>();

        using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        {
            connection.Open();

            using (var command = new SqlCommand(
                "SELECT o.ID, o.CREATEDAT, s.NAME as STATUS, po.PRODUCT_ID, po.AMOUNT, p.NAME as PRODUCT_NAME, p.PRICE " +
                "FROM [Order] o " +
                "JOIN STATUS s ON o.STATUS_ID = s.ID " +
                "JOIN PRODUCT_ORDER po ON o.ID = po.ORDER_ID " +
                "JOIN PRODUCT p ON po.PRODUCT_ID = p.ID " +
                "WHERE o.CLIENT_ID = @clientId " +
                "ORDER BY o.ID, p.NAME", connection))
            {
                command.Parameters.Add("@clientId", SqlDbType.Int).Value = clientId;

                using (var reader = command.ExecuteReader())
                {
                    OrderDetails currentOrder = null;

                    while (reader.Read())
                    {
                        int orderId = reader.GetInt32(0);

                        if (currentOrder == null || currentOrder.Id != orderId)
                        {
                            currentOrder = new OrderDetails
                            {
                                Id = orderId,
                                CreatedAt = reader.GetDateTime(1),
                                Status = reader.GetString(2),
                                Products = new List<Product>()
                            };

                            orders.Add(currentOrder);
                        }

                        currentOrder.Products.Add(new Product
                        {
                            Id = reader.GetInt32(3),
                            Amount = reader.GetInt32(4),
                            Name = reader.GetString(5),
                            Price = reader.GetDecimal(6)
                        });
                    }
                }
            }
        }

        return orders;
    }
    
    public int? CreateOrder(int clientId, List<OrderItemInput> orderItems)
    {
        using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        {
            connection.Open();
            SqlTransaction transaction = connection.BeginTransaction();

            try
            {
                // Weryfikacja istnienia klienta
                SqlCommand checkClientCmd = new SqlCommand("SELECT COUNT(*) FROM CLIENT WHERE ID = @ClientId", connection, transaction);
                checkClientCmd.Parameters.AddWithValue("@ClientId", clientId);
                if ((int)checkClientCmd.ExecuteScalar() == 0)
                {
                    return null;
                }

                // Utworzenie zamówienia
                SqlCommand createOrderCmd = new SqlCommand("INSERT INTO [Order] (CREATEDAT, CLIENT_ID, STATUS_ID) OUTPUT INSERTED.ID VALUES (GETDATE(), @ClientId, 1)", connection, transaction);
                createOrderCmd.Parameters.AddWithValue("@ClientId", clientId);
                int orderId = (int)createOrderCmd.ExecuteScalar();

                // Dodanie produktów do zamówienia
                foreach (var item in orderItems)
                {
                    SqlCommand addProductCmd = new SqlCommand("INSERT INTO PRODUCT_ORDER (PRODUCT_ID, ORDER_ID, AMOUNT) VALUES (@ProductId, @OrderId, @Amount)", connection, transaction);
                    addProductCmd.Parameters.AddWithValue("@ProductId", item.ProductId);
                    addProductCmd.Parameters.AddWithValue("@OrderId", orderId);
                    addProductCmd.Parameters.AddWithValue("@Amount", item.Amount);
                    addProductCmd.ExecuteNonQuery();
                }

                transaction.Commit();
                return orderId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
    
}