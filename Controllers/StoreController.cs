using WebApplication2.Models;

namespace WebApplication2.Controllers;

using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

[Route("api/clients")]
public class StoreController : ControllerBase
{
    private readonly IStoreRepo _storeRepo;

    public StoreController(IStoreRepo storeRepo)
    {
        _storeRepo = storeRepo;
    }

    [HttpGet("{clientId}/orders")]
    public ActionResult<List<OrderDetails>> GetOrdersByClientId(int clientId)
    {
        try
        {
            var orders = _storeRepo.GetOrdersByClientId(clientId);

            if (orders == null || orders.Count == 0)
            {
                return NotFound($"No orders found for client with ID {clientId}.");
            }

            return orders;
        }
        catch (Exception ex)
        {
            // Tutaj można dodać logowanie błędów
            return StatusCode(500, "An error occurred while processing the request.");
        }
    }
    
    [HttpPost("{clientId}/orders")]
    public ActionResult<int> CreateOrder(int clientId, List<OrderItemInput> orderItems)
    {
        try
        {
            int? orderId = _storeRepo.CreateOrder(clientId, orderItems);

            if (orderId == null)
            {
                return NotFound($"Client with ID {clientId} not found.");
            }

            return CreatedAtAction(nameof(GetOrdersByClientId), new { clientId = clientId }, orderId.Value);
        }
        catch (Exception ex)
        {
            // Tutaj można dodać logowanie błędów
            return StatusCode(500, "An error occurred while processing the request.");
        }
    }
}