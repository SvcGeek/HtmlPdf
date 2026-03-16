using System;
using System.Collections.Generic;

namespace Pdf.Abstractions.DTO
{
    /// <summary>
    /// Data Transfer Object for the sample endpoint PDF generation.
    /// Represents an order/invoice document with client information, line items, and totals.
    /// </summary>
    /// <remarks>
    /// This DTO is populated from the RenderPdfRequestBase.Data dictionary via JSON serialization.
    /// The Language and Direction properties are injected from the request's top-level fields.
    /// </remarks>
    public class SampleEndpointRenderDTO
    {
        /// <summary>Brand or company name to display on the document.</summary>
        public string? Brand { get; set; }

        /// <summary>Document language (injected from request).</summary>
        public string? Language { get; set; }

        /// <summary>Text direction: "ltr" or "rtl" (injected from request).</summary>
        public string? Direction { get; set; }

        /// <summary>Unique order identifier.</summary>
        public string? OrderId { get; set; }

        /// <summary>Date the order was placed.</summary>
        public DateTime OrderDate { get; set; }

        /// <summary>Customer/client information.</summary>
        public ClientModel? Client { get; set; }

        /// <summary>Name of the sales advisor who processed the order.</summary>
        public string? Advisor { get; set; }

        /// <summary>Unique identifier for the sales advisor.</summary>
        public string? AdvisorId { get; set; }

        /// <summary>Store name where the order was placed.</summary>
        public string? Store { get; set; }

        /// <summary>List of items/products in the order.</summary>
        public List<OrderItemModel> Items { get; set; } = new List<OrderItemModel>();

        /// <summary>Total number of items (sum of all quantities).</summary>
        public int TotalItems { get; set; }

        /// <summary>Total discount amount applied to the order.</summary>
        public decimal Discount { get; set; }

        /// <summary>Total order amount (after discounts).</summary>
        public decimal TotalAmount { get; set; }

        /// <summary>Amount paid upfront by the customer.</summary>
        public decimal AdvancePayment { get; set; }

        /// <summary>Remaining balance to be paid.</summary>
        public decimal Balance { get; set; }

        /// <summary>Physical address of the store.</summary>
        public string? StoreAddress { get; set; }

        /// <summary>City where the store is located.</summary>
        public string? StoreCity { get; set; }

        /// <summary>Store's contact phone number.</summary>
        public string? StorePhone { get; set; }

        /// <summary>Base64-encoded image or signature text from the client.</summary>
        public string? ClientSignature { get; set; }
    }

    /// <summary>
    /// Represents a single line item in an order.
    /// Includes product details, pricing, and quantity information.
    /// </summary>
    public class OrderItemModel
    {
        /// <summary>Style or look identifier (e.g., "casual", "formal").</summary>
        public string? Look { get; set; }

        /// <summary>Human-readable product name.</summary>
        public string? ProductName { get; set; }

        /// <summary>SKU or internal product code.</summary>
        public string? ProductCode { get; set; }

        /// <summary>URL to the product image for display in the PDF.</summary>
        public string? ImageUrl { get; set; }

        /// <summary>Material type (e.g., "cotton", "wool", "polyester").</summary>
        public string? Material { get; set; }

        /// <summary>Product color.</summary>
        public string? Color { get; set; }

        /// <summary>Size designation (e.g., "S", "M", "L", "XL", or numeric sizes).</summary>
        public string? Size { get; set; }

        /// <summary>Number of units ordered.</summary>
        public int Quantity { get; set; }

        /// <summary>Current price per unit (after discount if applicable).</summary>
        public decimal UnitPrice { get; set; }

        /// <summary>Original price per unit before discount (for showing savings).</summary>
        public decimal OldUnitPrice { get; set; }

        /// <summary>Total price for this line item (UnitPrice × Quantity).</summary>
        public decimal Total { get; set; }

        /// <summary>Original total before discount (OldUnitPrice × Quantity).</summary>
        public decimal OldTotal { get; set; }
    }

    /// <summary>
    /// Customer/client information for the order.
    /// Contains contact details and address for delivery or invoicing.
    /// </summary>
    public class ClientModel
    {
        /// <summary>Full name of the customer.</summary>
        public string? Name { get; set; }

        /// <summary>Customer identifier (account number, customer ID, or tax ID).</summary>
        public string? Id { get; set; }

        /// <summary>Customer's mobile phone number.</summary>
        public string? Mobile { get; set; }

        /// <summary>Customer's email address.</summary>
        public string? Email { get; set; }

        /// <summary>Street address for delivery or billing.</summary>
        public string? Address { get; set; }

        /// <summary>City for delivery or billing.</summary>
        public string? City { get; set; }

        /// <summary>Country name or code.</summary>
        public string? Country { get; set; }
    }
}
