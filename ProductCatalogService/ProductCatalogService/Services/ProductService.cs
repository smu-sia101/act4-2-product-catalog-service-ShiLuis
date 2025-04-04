using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ProductCatalogService.Models;

namespace ProductCatalogService.Services
{
    public class ProductService : IProductService
    {
        private readonly IMongoCollection<Product> _productsCollection;
        private readonly ILogger<ProductService> _logger;

        public ProductService(
            IOptions<ProductDBSettings> dbSettings,
            ILogger<ProductService> logger)
        {
            _logger = logger;

            try
            {
                var mongoClient = new MongoClient(dbSettings.Value.ConnectionString);
                var mongoDatabase = mongoClient.GetDatabase(dbSettings.Value.DatabaseName);
                _productsCollection = mongoDatabase.GetCollection<Product>(dbSettings.Value.ProductsCollectionName);

                _logger.LogInformation("Successfully connected to MongoDB");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to MongoDB");
                throw;
            }
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            _logger.LogInformation("Retrieving all products");
            return await _productsCollection.Find(_ => true).ToListAsync();
        }

        public async Task<Product?> GetProductByIdAsync(string id)
        {
            _logger.LogInformation("Retrieving product with ID: {ProductId}", id);
            return await _productsCollection.Find(p => p.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Product> CreateProductAsync(Product product)
        {
            _logger.LogInformation("Creating new product");
            await _productsCollection.InsertOneAsync(product);
            _logger.LogInformation("Product created with ID: {ProductId}", product.Id);
            return product;
        }

        public async Task<bool> UpdateProductAsync(string id, Product product)
        {
            _logger.LogInformation("Updating product with ID: {ProductId}", id);
            var result = await _productsCollection.ReplaceOneAsync(p => p.Id == id, product);

            if (result.IsAcknowledged && result.ModifiedCount > 0)
            {
                _logger.LogInformation("Product updated successfully");
                return true;
            }

            _logger.LogWarning("Product update failed - product not found or no changes made");
            return false;
        }

        public async Task<bool> DeleteProductAsync(string id)
        {
            _logger.LogInformation("Deleting product with ID: {ProductId}", id);
            var result = await _productsCollection.DeleteOneAsync(p => p.Id == id);

            if (result.IsAcknowledged && result.DeletedCount > 0)
            {
                _logger.LogInformation("Product deleted successfully");
                return true;
            }

            _logger.LogWarning("Product deletion failed - product not found");
            return false;
        }
    }
}