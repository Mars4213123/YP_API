using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using YP_API.Interfaces;
using YP_API.Models;

namespace YP_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IIngredientRepository _ingredientRepository;
        private readonly IRepository<UserInventory> _inventoryRepository;
        private readonly ILogger<InventoryController> _logger;

        public InventoryController(
            IUserRepository userRepository,
            IIngredientRepository ingredientRepository,
            IRepository<UserInventory> inventoryRepository,
            ILogger<InventoryController> logger)
        {
            _userRepository = userRepository;
            _ingredientRepository = ingredientRepository;
            _inventoryRepository = inventoryRepository;
            _logger = logger;
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult> GetInventory(int userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        error = "Пользователь не найден",
                        message = $"Пользователь с ID {userId} не существует"
                    });
                }

                var inventory = await _inventoryRepository.GetAllAsync();
                var userInventory = inventory.OfType<UserInventory>()
                    .Where(ui => ui.UserId == userId)
                    .ToList();

                return Ok(new
                {
                    success = true,
                    message = "Инвентарь получен успешно",
                    data = userInventory.Select(ui => new
                    {
                        Id = ui.Id,
                        ProductName = ui.Ingredient?.Name,
                        IngredientId = ui.IngredientId,
                        Quantity = ui.Quantity,
                        Unit = ui.Unit,
                        ExpiryDate = ui.ExpiryDate,
                        AddedAt = ui.AddedAt
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetInventory: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    error = "Внутренняя ошибка сервера",
                    message = "Ошибка при получении инвентаря"
                });
            }
        }

        [HttpPost("add/{userId}")]
        public async Task<ActionResult> AddToInventory(
            int userId,
            [FromForm]
            [Required(ErrorMessage = "Название продукта обязательно")]
            [Display(Name = "Название продукта")]
            string productName,

            [FromForm]
            [Range(0.001, 1000, ErrorMessage = "Количество должно быть от 0.001 до 1000")]
            [Display(Name = "Количество")]
            decimal quantity = 1,

            [FromForm]
            [Display(Name = "Единица измерения")]
            string unit = "шт")
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        error = "Пользователь не найден",
                        message = "Не удалось найти указанного пользователя"
                    });
                }

                var ingredient = await _ingredientRepository.GetIngredientByNameAsync(productName);
                if (ingredient == null)
                {
                    ingredient = new Ingredient
                    {
                        Name = productName,
                        Category = "Другое",
                        StandardUnit = unit
                    };
                    await _ingredientRepository.AddAsync(ingredient);
                    await _ingredientRepository.SaveAllAsync();
                }

                var existingInventory = (await _inventoryRepository.GetAllAsync())
                    .OfType<UserInventory>()
                    .FirstOrDefault(ui => ui.UserId == userId && ui.IngredientId == ingredient.Id);

                if (existingInventory != null)
                {
                    existingInventory.Quantity += quantity;
                    _inventoryRepository.Update(existingInventory);
                }
                else
                {
                    // Создаем новую запись
                    var inventoryItem = new UserInventory
                    {
                        UserId = userId,
                        IngredientId = ingredient.Id,
                        Quantity = quantity,
                        Unit = unit,
                        AddedAt = DateTime.UtcNow
                    };
                    await _inventoryRepository.AddAsync(inventoryItem);
                }

                if (await _inventoryRepository.SaveAllAsync())
                {
                    return Ok(new
                    {
                        success = true,
                        message = "Продукт успешно добавлен в инвентарь",
                        data = new
                        {
                            ProductName = ingredient.Name,
                            Quantity = quantity,
                            Unit = unit
                        }
                    });
                }

                return BadRequest(new
                {
                    success = false,
                    error = "Ошибка сохранения",
                    message = "Не удалось сохранить продукт в инвентарь"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in AddToInventory: {ex.Message}");
                return BadRequest(new
                {
                    success = false,
                    error = "Ошибка добавления в инвентарь",
                    message = ex.Message
                });
            }
        }

        [HttpDelete("{userId}/items/{itemId}")]
        public async Task<ActionResult> RemoveFromInventory(int userId, int itemId)
        {
            try
            {
                var inventoryItem = await _inventoryRepository.GetByIdAsync(itemId) as UserInventory;
                if (inventoryItem == null || inventoryItem.UserId != userId)
                {
                    return NotFound(new
                    {
                        success = false,
                        error = "Элемент не найден",
                        message = "Элемент инвентаря не найден или не принадлежит пользователю"
                    });
                }

                _inventoryRepository.Delete(inventoryItem);

                if (await _inventoryRepository.SaveAllAsync())
                {
                    return Ok(new
                    {
                        success = true,
                        message = "Продукт успешно удален из инвентаря"
                    });
                }

                return BadRequest(new
                {
                    success = false,
                    error = "Ошибка удаления",
                    message = "Не удалось удалить продукт из инвентаря"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in RemoveFromInventory: {ex.Message}");
                return BadRequest(new
                {
                    success = false,
                    error = "Ошибка удаления из инвентаря",
                    message = ex.Message
                });
            }
        }

        [HttpPut("{userId}/items/{itemId}")]
        public async Task<ActionResult> UpdateInventoryItem(
            int userId,
            int itemId,
            [FromForm]
            [Range(0.001, 1000, ErrorMessage = "Количество должно быть от 0.001 до 1000")]
            [Display(Name = "Количество")]
            decimal quantity,

            [FromForm]
            [Display(Name = "Единица измерения")]
            string unit = "шт")
        {
            try
            {
                var inventoryItem = await _inventoryRepository.GetByIdAsync(itemId) as UserInventory;
                if (inventoryItem == null || inventoryItem.UserId != userId)
                {
                    return NotFound(new
                    {
                        success = false,
                        error = "Элемент не найден",
                        message = "Элемент инвентаря не найден или не принадлежит пользователю"
                    });
                }

                inventoryItem.Quantity = quantity;
                inventoryItem.Unit = unit;

                _inventoryRepository.Update(inventoryItem);

                if (await _inventoryRepository.SaveAllAsync())
                {
                    return Ok(new
                    {
                        success = true,
                        message = "Продукт успешно обновлен",
                        data = new
                        {
                            Id = inventoryItem.Id,
                            Quantity = inventoryItem.Quantity,
                            Unit = inventoryItem.Unit
                        }
                    });
                }

                return BadRequest(new
                {
                    success = false,
                    error = "Ошибка обновления",
                    message = "Не удалось обновить продукт в инвентаре"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in UpdateInventoryItem: {ex.Message}");
                return BadRequest(new
                {
                    success = false,
                    error = "Ошибка обновления инвентаря",
                    message = ex.Message
                });
            }
        }
    }
}