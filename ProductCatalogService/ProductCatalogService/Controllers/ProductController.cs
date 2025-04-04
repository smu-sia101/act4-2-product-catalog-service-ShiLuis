using Microsoft.AspNetCore.Mvc;
using ProductCatalogService.Models;
using ProductCatalogService.Services;

namespace ProductCatalogService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IProductService productService, ILogger<ProductsController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<Product>>> GetAllProducts()
        {
            _logger.LogInformation("GET request received for all products");
            var products = await _productService.GetAllProductsAsync();
            return Ok(products);
        }

        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<Product>> GetProduct(string id)
        {
            _logger.LogInformation("GET request received for product with ID: {ProductId}", id);
            var product = await _productService.GetProductByIdAsync(id);

            if (product == null)
            {
                _logger.LogWarning("Product with ID: {ProductId} not found", id);
                return NotFound();
            }

            return Ok(product);
        }

        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct(Product product)
        {
            _logger.LogInformation("POST request received to create a new product");

            // Basic validation
            if (string.IsNullOrEmpty(product.Name))
            {
                _logger.LogWarning("Invalid product data: Name is required");
                return BadRequest("Product name is required");
            }

            var createdProduct = await _productService.CreateProductAsync(product);

            _logger.LogInformation("Product created with ID: {ProductId}", createdProduct.Id);
            return CreatedAtAction(nameof(GetProduct), new { id = createdProduct.Id }, createdProduct);
        }

        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> UpdateProduct(string id, Product product)
        {
            _logger.LogInformation("PUT request received to update product with ID: {ProductId}", id);

            if (id != product.Id)
            {
                _logger.LogWarning("ID mismatch in update request");
                return BadRequest("ID in URL must match ID in request body");
            }

            var updated = await _productService.UpdateProductAsync(id, product);

            if (!updated)
            {
                _logger.LogWarning("Product with ID: {ProductId} not found for update", id);
                return NotFound();
            }

            _logger.LogInformation("Product with ID: {ProductId} updated successfully", id);
            return NoContent();
        }

        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> DeleteProduct(string id)
        {
            _logger.LogInformation("DELETE request received for product with ID: {ProductId}", id);

            var deleted = await _productService.DeleteProductAsync(id);

            if (!deleted)
            {
                _logger.LogWarning("Product with ID: {ProductId} not found for deletion", id);
                return NotFound();
            }

            _logger.LogInformation("Product with ID: {ProductId} deleted successfully", id);
            return NoContent();
        }
    }
}